using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.TimeSystem;
using UnityEngine;

namespace FarmGame2D.UI
{
    internal enum PhonePage
    {
        Farm,
        Settings
    }

    public sealed partial class PrototypeHud : MonoBehaviour
    {
        public const string ShopTitle = "상점";
        public const string InventoryTitle = "가방";
        public const int InventoryTitleFontSize = 30;

        public const string CharacterChoiceHeader = "선택해주세요.";
        public const string CharacterChoicePrompt = "당신의 성별을 골라주세요";
        public const int ToastMessageFontSize = 14;

        private PrototypePlayerController player;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centeredStyle;
        private GUIStyle titleStyle;
        private GUIStyle inventoryTitleStyle;
        private GUIStyle panelStyle;
        private GUIStyle toastStyle;
        private GUIStyle interactionPromptStyle;
        private GUIStyle interactionKeyStyle;
        private Font koreanFont;
        private FarmGuiSkin farmSkin;
        private readonly HudMessageVisibility messageVisibility = new();
        private Vector2 sellScrollPosition;
        private bool inventoryDecorationsPage;
        private string cloudCodeInput = string.Empty;
        private string cloudPinInput = string.Empty;
        private PhonePage phonePage;
        private bool exitConfirmationPending;
        private float masterVolume;
        private float volumeBeforeMute = GameAudioSettings.DefaultMasterVolume;
        private string exitNotice = string.Empty;

        public bool HasArtSkin => farmSkin?.IsReady == true;

        public void Configure(PrototypePlayerController controlledPlayer)
        {
            player = controlledPlayer;
            koreanFont = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
            farmSkin = new FarmGuiSkin(koreanFont);
            masterVolume = GameAudioSettings.LoadAndApply();
            if (masterVolume > 0.001f)
            {
                volumeBeforeMute = masterVolume;
            }
        }

        private void OnGUI()
        {
            var session = GameSession.Instance;
            if (session == null)
            {
                return;
            }

            EnsureStyles();
            var previousMatrix = GUI.matrix;
            var viewport = HudLayout.CalculateViewport(Screen.width, Screen.height);
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(viewport.OffsetX, viewport.OffsetY, 0f),
                Quaternion.identity,
                new Vector3(viewport.Scale, viewport.Scale, 1f));

            try
            {
                if (session.Data.characterChoice == CharacterChoice.None)
                {
                    DrawCharacterChoice(session);
                    return;
                }

                DrawStatus(session);
                DrawQuest(session);
                if (!NpcInteractionOverlay.IsOpen)
                {
                    DrawHotbar(session);
                    DrawDogCommand(session);
                    DrawInteractionPrompt();
                }

                var modalOpen = session.PhoneOpen
                    || session.ShopOpen
                    || session.InventoryOpen
                    || session.QuestBoardOpen
                    || session.SellBoxOpen
                    || NpcInteractionOverlay.IsOpen;
                if (messageVisibility.ShouldShow(
                        session.MessageRevision,
                        Time.unscaledTime,
                        session.MessageDisplaySeconds,
                        modalOpen))
                {
                    DrawMessage(session);
                }

                if (session.PhoneOpen)
                {
                    DrawPhone(session);
                }

                if (session.ShopOpen)
                {
                    DrawShop(session);
                }
                else
                {
                    shopWasOpen = false;
                }

                if (session.InventoryOpen)
                {
                    DrawInventory(session);
                }

                if (session.QuestBoardOpen)
                {
                    DrawQuestBoard(session);
                }

                if (session.SellBoxOpen)
                {
                    DrawSellBox(session);
                }

                var input = Event.current;
                if (!modalOpen && input.type == EventType.MouseDown && input.button == 0
                    && player != null && player.TryInteractWithClickedNpc(Input.mousePosition))
                {
                    input.Use();
                }
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawStatus(GameSession session)
        {
            var time = GameClockController.Instance?.CurrentTime;
            var timeText = time.HasValue ? $"{time.Value.Hour:00}:{time.Value.Minute:00}" : "--:--";
            var rect = new Rect(HudLayout.ReferenceWidth - 292f, 14f, 274f, 104f);
            farmSkin.DrawBanner(rect);
            GUI.Label(new Rect(rect.x + 90f, rect.y + 22f, rect.width - 116f, 62f),
                $"{GetSeasonLabel(session.CurrentSeason)} {SeasonCalendar.GetDayOfSeason(session.Data.day)}일  {timeText}\n보유 금액 {session.Data.money}G",
                panelStyle);
        }

        private void DrawQuest(GameSession session)
        {
            var current = StarterQuestProgress.GetCurrent(session.Data);
            var quest = session.Data.activeQuestId < 0
                ? "광장 게시판에서 의뢰를 선택하세요"
                : $"퀘스트: {current.Title} ({current.Current}/{current.Target})";
            var rect = new Rect(14f, 14f, 340f, 104f);
            farmSkin.DrawBanner(rect);
            GUI.Label(new Rect(rect.x + 90f, rect.y + 22f, rect.width - 120f, 62f), quest, panelStyle);
        }

        private void DrawHotbar(GameSession session)
        {
            var label = session.SelectedDecoration.HasValue
                ? $"[I] 가방   [Space] {ItemCatalog.GetLabel(session.SelectedDecoration.Value)} 설치   [E] 줍기"
                : session.SelectedTool switch
            {
                FarmTool.Hoe => "[I] 가방   [Space] 앞의 땅 갈기   [E] 상호작용",
                FarmTool.Shovel => "[I] 가방   [E] 빈 밭 없애기",
                _ => $"[I] 가방 열기   들고 있음: {GetToolLabel(session.SelectedTool)}"
            };
            var rect = new Rect((HudLayout.ReferenceWidth - 520f) / 2f, HudLayout.ReferenceHeight - 70f, 520f, 54f);
            farmSkin.DrawButtonBackground(rect, true);
            GUI.Label(rect, label, centeredStyle);
        }

        private void DrawDogCommand(GameSession session)
        {
            var rect = new Rect(16f, HudLayout.ReferenceHeight - 70f, 190f, 54f);
            farmSkin.DrawButtonBackground(rect, false);
            GUI.Label(rect, session.Data.dogSitting ? "[G] 강아지 따라와" : "[G] 강아지 앉아", centeredStyle);
        }

        private void DrawInventory(GameSession session)
        {
            var layout = HudLayout.CalculateInventoryLayout();
            farmSkin.DrawWindow(layout.Panel);
            farmSkin.DrawSprite(layout.Portrait, GameArtCatalog.GetPlayerPortrait(session.Data.characterChoice));
            GUI.Label(layout.Title, InventoryTitle, inventoryTitleStyle);

            var tabY = layout.Panel.y + 132f;
            if (farmSkin.DrawButton(new Rect(layout.Panel.x + 205f, tabY, 180f, 36f), "도구와 씨앗", !inventoryDecorationsPage))
                inventoryDecorationsPage = false;
            if (farmSkin.DrawButton(new Rect(layout.Panel.x + 405f, tabY, 180f, 36f), "마을 장식", inventoryDecorationsPage))
                inventoryDecorationsPage = true;

            if (inventoryDecorationsPage)
            {
                DrawInventoryDecorations(session, layout);
                return;
            }

            if (farmSkin.DrawButton(layout.HoeButton, "호미"))
            {
                session.SelectTool(FarmTool.Hoe);
            }

            if (farmSkin.DrawButton(layout.ShovelButton, "삽"))
            {
                session.SelectTool(FarmTool.Shovel);
            }

            if (farmSkin.DrawButton(layout.SeedsButton, ItemCatalog.GetLabel(session.SelectedSeed), true))
            {
                session.SelectSeed(session.SelectedSeed);
            }

            if (farmSkin.DrawButton(layout.WateringCanButton, "물뿌리개"))
            {
                session.SelectTool(FarmTool.WateringCan);
            }

            if (farmSkin.DrawButton(layout.PickaxeButton, session.Data.hasPickaxe ? "곡괭이" : "곡괭이 (미보유)"))
            {
                session.SelectTool(FarmTool.Pickaxe);
            }

            if (farmSkin.DrawButton(layout.FishingRodButton, "낚싯대"))
            {
                session.SelectTool(FarmTool.FishingRod);
            }

            var seedIndex = 0;
            foreach (var crop in CropCatalog.GetAll())
            {
                var count = session.Data.inventory.Count(crop.Seed);
                var label = $"{crop.Label} 씨앗 ×{count} · {GetSeasonLabel(crop.Season)}";
                if (farmSkin.DrawButton(layout.GetSeedButton(seedIndex), label, crop.Seed == session.SelectedSeed))
                {
                    session.SelectSeed(crop.Seed);
                }

                seedIndex++;
            }

            var juvenileCount = session.Data.chickenJuvenileDays.Count + session.Data.cowJuvenileDays.Count;
            var fishCount = session.Data.inventory.Count(GameItem.RiverFish)
                + session.Data.inventory.Count(GameItem.SeaBass)
                + session.Data.inventory.Count(GameItem.GoldenCarp);
            GUI.Label(layout.GetStatsLabel(0, 0),
                $"사료 {session.Data.inventory.Count(GameItem.Feed)} · 달걀 {session.Data.inventory.Count(GameItem.Egg)} · 우유 {session.Data.inventory.Count(GameItem.Milk)}\n닭 {session.Data.ownedChickens} · 소 {session.Data.ownedCows} · 새끼 {juvenileCount}",
                smallStyle);
            GUI.Label(layout.GetStatsLabel(1, 0),
                $"돌 {session.Data.inventory.Count(GameItem.Stone)} · 구리 {session.Data.inventory.Count(GameItem.CopperOre)} · 철 {session.Data.inventory.Count(GameItem.IronOre)} · 금 {session.Data.inventory.Count(GameItem.GoldOre)}\n물고기 {fishCount} · 잡초 {session.Data.inventory.Count(GameItem.Weed)} · 목재 {session.Data.inventory.Count(GameItem.Wood)}",
                smallStyle);

            var axeButton = new Rect(layout.Panel.x + 48f, layout.CloseButton.y, 138f, layout.CloseButton.height);
            var sprinklerButton = new Rect(layout.Panel.xMax - 204f, layout.CloseButton.y, 156f, layout.CloseButton.height);
            if (farmSkin.DrawButton(axeButton, "도끼")) session.SelectTool(FarmTool.Axe);
            if (farmSkin.DrawButton(sprinklerButton, $"스프링클러 ×{session.Data.inventory.Count(GameItem.Sprinkler)}"))
                session.SelectDecoration(GameItem.Sprinkler);

            if (farmSkin.DrawButton(layout.CloseButton, "닫기"))
            {
                session.ToggleInventory();
            }
        }

        private void DrawInventoryDecorations(GameSession session, InventoryHudLayout layout)
        {
            var decorations = TownDecorationCatalog.GetAll();
            for (var index = 0; index < decorations.Count; index++)
            {
                var item = decorations[index].Item;
                var label = $"{ItemCatalog.GetLabel(item)}  ×{session.Data.inventory.Count(item)}";
                var button = new Rect(layout.Panel.x + 120f, layout.Panel.y + 188f + index * 66f, 580f, 54f);
                if (farmSkin.DrawButton(button, label, session.SelectedDecoration == item))
                    session.SelectDecoration(item);
            }

            GUI.Label(new Rect(layout.Panel.x + 98f, layout.Panel.y + 395f, 630f, 42f),
                "잡화점에서 구입 · 마을 잔디 앞에서 Space로 설치 · E로 줍기", centeredStyle);
            if (farmSkin.DrawButton(layout.CloseButton, "닫기")) session.ToggleInventory();
        }

        private void DrawMessage(GameSession session)
        {
            if (session.ActivePortraitRow < 0)
            {
                var toastLayout = HudLayout.CalculateToastLayout();
                farmSkin.DrawBanner(toastLayout.Panel);
                GUI.Label(toastLayout.Message, session.Message, toastStyle);
                return;
            }

            farmSkin.DrawDialogue(HudLayout.CalculateDialogueLayout(), session.ActiveSpeaker, session.Message, session.ActivePortraitRow);
        }

        private void DrawInteractionPrompt()
        {
            var prompt = player?.VisibleNearbyPrompt;
            if (string.IsNullOrEmpty(prompt))
            {
                return;
            }

            var layout = HudLayout.CalculateInteractionPromptLayout();
            var text = prompt.StartsWith("E: ") ? prompt.Substring(3) : prompt;
            farmSkin.DrawMutedPrompt(layout.Panel, layout.Keycap);
            GUI.Label(layout.Keycap, "E", interactionKeyStyle);
            GUI.Label(layout.Text, text, interactionPromptStyle);
        }

        private void DrawCharacterChoice(GameSession session)
        {
            var rect = new Rect((HudLayout.ReferenceWidth - 420f) / 2f, (HudLayout.ReferenceHeight - 210f) / 2f, 420f, 210f);
            farmSkin.DrawWindow(rect);
            GUI.Label(new Rect(rect.x + 105f, rect.y + 20f, 280f, 42f), CharacterChoiceHeader, titleStyle);
            GUI.Label(new Rect(rect.x + 38f, rect.y + 70f, 344f, 38f), CharacterChoicePrompt, centeredStyle);
            if (farmSkin.DrawButton(new Rect(rect.x + 35f, rect.y + 125f, 160f, 52f), "남성형 (M)"))
            {
                session.ChooseCharacter(CharacterChoice.Male);
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 225f, rect.y + 125f, 160f, 52f), "여성형 (F)", true))
            {
                session.ChooseCharacter(CharacterChoice.Female);
            }
        }

        private void DrawPhone(GameSession session)
        {
            var layout = HudLayout.CalculatePhoneLayout();
            farmSkin.DrawWindow(layout.Panel);
            GUI.Label(layout.Title, "농장 휴대폰", titleStyle);

            if (exitConfirmationPending)
            {
                DrawExitConfirmation(session, layout);
                return;
            }

            DrawPhoneTabs(layout);
            if (phonePage == PhonePage.Farm)
            {
                DrawPhoneFarmTab(session, layout);
                DrawCloudSync(session, layout.Panel);
            }
            else
            {
                DrawPhoneSettingsTab(session, layout);
            }

            if (farmSkin.DrawButton(layout.CloseButton, "닫기 (P)"))
            {
                exitConfirmationPending = false;
                session.TogglePhone();
            }
        }

        private void DrawPhoneTabs(PhoneHudLayout layout)
        {
            if (farmSkin.DrawButton(layout.FarmTabButton, "농장", phonePage == PhonePage.Farm))
            {
                phonePage = PhonePage.Farm;
                exitNotice = string.Empty;
            }

            if (farmSkin.DrawButton(layout.SettingsTabButton, "설정", phonePage == PhonePage.Settings))
            {
                phonePage = PhonePage.Settings;
                exitNotice = string.Empty;
            }
        }

        private void DrawPhoneFarmTab(GameSession session, PhoneHudLayout layout)
        {
            var quests = StarterQuestProgress.GetAll(session.Data);
            var completed = 0;
            foreach (var quest in quests)
            {
                if (quest.Completed)
                {
                    completed++;
                }
            }
            var active = StarterQuestProgress.GetCurrent(session.Data);
            GUI.Label(layout.QuestBody,
                session.Data.activeQuestId < 0
                    ? $"완료한 의뢰 {completed}/{quests.Length}\n광장 게시판에서 다음 의뢰를 선택하세요."
                    : $"진행 중: {active.Title}\n{active.Current}/{active.Target} · 완료 {completed}/{quests.Length}", bodyStyle);

            if (farmSkin.DrawButton(layout.SaveButton, "저장"))
            {
                session.Save(false);
            }

            if (farmSkin.DrawButton(layout.LoadButton, "불러오기", true))
            {
                session.Load();
            }
        }

        private void DrawPhoneSettingsTab(GameSession session, PhoneHudLayout layout)
        {
            GUI.Label(layout.VolumeLabel, $"전체 음량  {Mathf.RoundToInt(masterVolume * 100f)}%", centeredStyle);
            var nextVolume = GUI.HorizontalSlider(layout.VolumeSlider, masterVolume, 0f, 1f);
            if (!Mathf.Approximately(nextVolume, masterVolume))
            {
                ApplyMasterVolume(nextVolume);
                exitNotice = "음량 설정을 저장했습니다.";
            }

            var muteLabel = masterVolume <= 0.001f ? "소리 켜기" : "음소거";
            if (farmSkin.DrawButton(layout.MuteButton, muteLabel))
            {
                var toggledVolume = masterVolume <= 0.001f
                    ? Mathf.Max(0.1f, volumeBeforeMute)
                    : 0f;
                ApplyMasterVolume(toggledVolume);
                exitNotice = toggledVolume <= 0.001f ? "소리를 껐습니다." : "소리를 켰습니다.";
            }

            if (farmSkin.DrawButton(layout.SaveButton, "지금 저장", true))
            {
                session.Save(false);
                exitNotice = "현재 진행 상황을 저장했습니다.";
            }

            if (farmSkin.DrawButton(layout.QuitButton, "저장하고 나가기"))
            {
                exitConfirmationPending = true;
            }

            if (!string.IsNullOrEmpty(exitNotice))
            {
                GUI.Label(layout.Notice, exitNotice, centeredStyle);
            }
        }

        private void ApplyMasterVolume(float volume)
        {
            if (volume > 0.001f)
            {
                volumeBeforeMute = volume;
            }

            masterVolume = GameAudioSettings.SetMasterVolume(volume);
        }

        private void DrawExitConfirmation(GameSession session, PhoneHudLayout layout)
        {
            farmSkin.DrawWindow(layout.ExitConfirmationPanel);
            GUI.Label(layout.ExitConfirmationMessage,
                "현재 진행 상황을 저장하고\n게임에서 나갈까요?",
                centeredStyle);

            if (farmSkin.DrawButton(layout.CancelExitButton, "취소"))
            {
                exitConfirmationPending = false;
            }

            if (!farmSkin.DrawButton(layout.ConfirmExitButton, "저장 후 나가기", true))
            {
                return;
            }

            session.Save(false);
            exitConfirmationPending = false;
#if UNITY_EDITOR
            exitNotice = "저장했습니다. 빌드에서는 게임이 종료됩니다.";
#elif UNITY_WEBGL
            exitNotice = "저장했습니다. 브라우저 탭을 닫아 주세요.";
#else
            Application.Quit();
#endif
        }

        private void DrawQuestBoard(GameSession session)
        {
            var layout = HudLayout.CalculateQuestBoardLayout();
            farmSkin.DrawWindow(layout.Panel);
            GUI.Label(layout.Title, "마을 광장 의뢰 게시판", titleStyle);
            GUI.Label(layout.Status,
                session.Data.activeQuestId >= 0 ? "진행 중인 의뢰를 먼저 끝내 주세요." : "하고 싶은 의뢰를 하나 골라 보세요.", centeredStyle);
            var refreshDays = StarterQuestProgress.GetDaysUntilBoardRefresh(session.Data.day);
            GUI.Label(layout.Refresh, $"의뢰는 3일마다 갱신됩니다 · 다음 갱신까지 {refreshDays}일", smallStyle);

            var quests = StarterQuestProgress.GetBoardOffers(session.Data);
            for (var index = 0; index < quests.Length; index++)
            {
                var quest = quests[index];
                var label = quest.Completed
                    ? $"완료 · {quest.Title}"
                    : $"{quest.Title}  ({quest.Current}/{quest.Target})";
                var previousEnabled = GUI.enabled;
                GUI.enabled = !quest.Completed && session.Data.activeQuestId < 0;
                if (farmSkin.DrawButton(layout.GetQuestButton(index), label,
                        session.Data.activeQuestId == (int)quest.Id))
                {
                    session.AcceptQuest(quest.Id);
                }
                GUI.enabled = previousEnabled;
            }

            if (farmSkin.DrawButton(layout.CloseButton, "게시판 닫기"))
            {
                session.CloseQuestBoard();
            }
        }

        private void DrawCloudSync(GameSession session, Rect phonePanel)
        {
            var panel = new Rect(phonePanel.x + phonePanel.width + 12f, phonePanel.y, 400f, 350f);
            farmSkin.DrawWindow(panel);
            GUI.Label(new Rect(panel.x + 90f, panel.y + 34f, 270f, 40f), "브라우저 동기화", titleStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = !session.CloudSyncBusy;
            if (session.CloudSyncLinked)
            {
                var connectionText = session.CloudLoadRequired
                    ? "다른 브라우저의 저장이 있습니다.\n최신 저장 받기를 먼저 눌러 주세요."
                    : string.IsNullOrEmpty(session.NewlyCreatedCloudPin)
                    ? $"연결된 코드  {session.CloudSyncCode}\n클라우드에도 함께 저장됩니다."
                    : $"연결 코드  {session.CloudSyncCode}\nPIN  {session.NewlyCreatedCloudPin}\n지금 메모해 두세요";
                GUI.Label(new Rect(panel.x + 28f, panel.y + 96f, 344f, 78f),
                    connectionText, centeredStyle);

                if (farmSkin.DrawButton(new Rect(panel.x + 40f, panel.y + 184f, 150f, 46f), "최신 저장 받기", true))
                {
                    session.LoadCloudSave();
                }

                if (farmSkin.DrawButton(new Rect(panel.x + 210f, panel.y + 184f, 150f, 46f), "연결 해제"))
                {
                    session.DisconnectCloudSave();
                }

                GUI.Label(new Rect(panel.x + 28f, panel.y + 240f, 344f, 52f),
                    "다른 브라우저에서는\n같은 코드와 PIN을 입력하세요.", centeredStyle);
            }
            else
            {
                GUI.Label(new Rect(panel.x + 40f, panel.y + 92f, 320f, 38f),
                    "계정 없이 코드와 PIN으로 이어서 할 수 있어요.", centeredStyle);
                if (farmSkin.DrawButton(new Rect(panel.x + 95f, panel.y + 132f, 210f, 42f), "새 동기화 코드 만들기", true))
                {
                    session.CreateCloudSave();
                }

                GUI.Label(new Rect(panel.x + 42f, panel.y + 188f, 88f, 34f), "코드", bodyStyle);
                cloudCodeInput = GUI.TextField(new Rect(panel.x + 126f, panel.y + 188f, 232f, 34f),
                    cloudCodeInput.ToUpperInvariant(), 10);
                GUI.Label(new Rect(panel.x + 42f, panel.y + 232f, 88f, 34f), "PIN", bodyStyle);
                cloudPinInput = GUI.PasswordField(new Rect(panel.x + 126f, panel.y + 232f, 232f, 34f),
                    cloudPinInput, '●', 6);
                if (farmSkin.DrawButton(new Rect(panel.x + 120f, panel.y + 278f, 160f, 42f), "기존 저장 연결"))
                {
                    session.ConnectCloudSave(cloudCodeInput, cloudPinInput);
                }
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawSellBox(GameSession session)
        {
            var layout = new SellBoxHudLayout();
            farmSkin.DrawWindow(layout.Panel);
            GUI.Label(layout.Title, "판매 상자", titleStyle);
            var offers = ProduceSale.List(session.Data.inventory);
            if (offers.Length == 0)
            {
                GUI.Label(layout.ProductsViewport, "판매할 농산물이나 광물이 없습니다.", centeredStyle);
            }
            else
            {
                var content = new Rect(0f, 0f, layout.ProductsViewport.width - 18f, layout.GetContentHeight(offers.Length + 1));
                sellScrollPosition = GUI.BeginScrollView(layout.ProductsViewport, sellScrollPosition, content, false, true);
                for (var index = 0; index < offers.Length; index++)
                {
                    DrawSellOffer(session, layout, offers[index], index);
                }

                if (farmSkin.DrawButton(layout.GetScrollableProductButton(offers.Length), "가지고 있는 것 모두 판매", true))
                {
                    session.SellAllProduce();
                }

                GUI.EndScrollView();
            }

            if (farmSkin.DrawButton(layout.CloseButton, "닫기"))
            {
                session.CloseSellBox();
            }
        }

        private void DrawSellOffer(GameSession session, SellBoxHudLayout layout, ProduceSale.Offer offer, int index)
        {
            var row = layout.GetScrollableProductButton(index);
            var textWidth = row.width * 0.5f;
            GUI.Label(new Rect(row.x, row.y, textWidth, row.height),
                $"{ItemCatalog.GetLabel(offer.Item)}  ×{offer.Quantity}\n개당 {offer.UnitPrice}G",
                panelStyle);
            var buttons = new Rect(row.x + textWidth, row.y, row.width - textWidth, row.height);
            if (offer.Quantity <= 1)
            {
                if (farmSkin.DrawButton(buttons, $"{offer.Total}G 판매"))
                {
                    session.SellProduce(offer.Item, offer.Quantity);
                }

                return;
            }

            var one = new Rect(buttons.x, buttons.y, buttons.width * 0.48f, buttons.height);
            var all = new Rect(buttons.x + buttons.width * 0.52f, buttons.y, buttons.width * 0.48f, buttons.height);
            if (farmSkin.DrawButton(one, "1개"))
            {
                session.SellProduce(offer.Item, 1);
            }

            if (farmSkin.DrawButton(all, "전부", true))
            {
                session.SellProduce(offer.Item, offer.Quantity);
            }
        }

        private static void ExecutePurchase(GameSession session, ShopPurchaseRequest request)
        {
            switch (request.Kind)
            {
                case ShopPurchaseKind.Seed:
                    session.PurchaseSeed(request.Item);
                    break;
                case ShopPurchaseKind.Feed:
                    session.PurchaseFeed();
                    break;
                case ShopPurchaseKind.Chicken:
                    session.PurchaseChicken();
                    break;
                case ShopPurchaseKind.ChickenSale:
                    session.SellChicken();
                    break;
                case ShopPurchaseKind.Cow:
                    session.PurchaseCow();
                    break;
                case ShopPurchaseKind.CowSale:
                    session.SellCow();
                    break;
                case ShopPurchaseKind.Pickaxe:
                    session.PurchasePickaxe();
                    break;
                case ShopPurchaseKind.Ore:
                    session.PurchaseOre(request.Item, request.Quantity, request.Price);
                    break;
                case ShopPurchaseKind.Sprinkler:
                    session.PurchaseSprinkler();
                    break;
                case ShopPurchaseKind.Decoration:
                    session.PurchaseDecoration(request.Item);
                    break;
            }
        }

        private void EnsureStyles()
        {
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                font = koreanFont,
                fontSize = 18,
                wordWrap = true,
                normal = { textColor = new Color32(78, 48, 31, 255) }
            };
            smallStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 14
            };
            centeredStyle ??= new GUIStyle(bodyStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            inventoryTitleStyle ??= new GUIStyle(titleStyle)
            {
                fontSize = InventoryTitleFontSize
            };
            panelStyle ??= new GUIStyle(smallStyle)
            {
                fontSize = 14,
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };
            toastStyle ??= new GUIStyle(panelStyle)
            {
                fontSize = ToastMessageFontSize,
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleCenter
            };
            interactionPromptStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                normal = { textColor = new Color32(244, 244, 240, 255) }
            };
            interactionKeyStyle ??= new GUIStyle(interactionPromptStyle)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color32(255, 222, 143, 255) }
            };
            GUI.skin.font = koreanFont;
        }

        private static string GetSeasonLabel(Season season)
        {
            return season switch
            {
                Season.Spring => "봄",
                Season.Summer => "여름",
                Season.Autumn => "가을",
                Season.Winter => "겨울",
                _ => season.ToString()
            };
        }

        private static string GetToolLabel(FarmTool tool)
        {
            return tool switch
            {
                FarmTool.Hoe => "호미",
                FarmTool.Seeds => "씨앗",
                FarmTool.WateringCan => "물뿌리개",
                FarmTool.Pickaxe => "곡괭이",
                FarmTool.FishingRod => "낚싯대",
                FarmTool.Shovel => "삽",
                FarmTool.Axe => "도끼",
                _ => tool.ToString()
            };
        }

        private static string FormatQuestLine(StarterQuestStatus quest)
        {
            var marker = quest.Completed ? "완료" : "진행";
            return $"[{marker}] {quest.Title} {quest.Current}/{quest.Target}";
        }
    }
}
