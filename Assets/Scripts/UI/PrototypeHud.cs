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
        Settings,
        Achievements
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
            EnsureSurfaceStyles();
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
                    DrawSellBox(session);
                else
                    sellBoxWasOpen = false;

                var input = Event.current;
                if (!modalOpen && input.type == EventType.MouseDown && input.button == 0
                    && player != null && (TryOpenClickedSellBox(Input.mousePosition)
                        || player.TryInteractWithClickedNpc(Input.mousePosition)))
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
            var rect = new Rect(HudLayout.ReferenceWidth - 230f, 16f, 212f, 76f);
            farmSkin.DrawPixelPanel(rect, Pine, DarkEdge);
            var seasonColor = session.CurrentSeason switch
            {
                Season.Spring => new Color32(217, 154, 167, 255),
                Season.Summer => new Color32(167, 190, 108, 255),
                Season.Autumn => new Color32(217, 153, 90, 255),
                _ => new Color32(173, 209, 220, 255)
            };
            var season = new Rect(rect.x + 10f, rect.y + 10f, 54f, 54f);
            farmSkin.DrawPixelPanel(season, seasonColor, DarkEdge, false);
            GUI.Label(season, GetSeasonLabel(session.CurrentSeason), seasonStampStyle);
            GUI.Label(new Rect(rect.x + 76f, rect.y + 10f, 126f, 25f),
                $"{SeasonCalendar.GetDayOfSeason(session.Data.day)}일  {timeText}", phoneLightStyle);
            GUI.Label(new Rect(rect.x + 76f, rect.y + 38f, 126f, 24f), $"{session.Data.money:N0} G", warmLightStyle);
        }

        private void DrawQuest(GameSession session)
        {
            var current = StarterQuestProgress.GetCurrent(session.Data);
            var active = session.Data.activeQuestId >= 0;
            var rect = new Rect(18f, 16f, 292f, 76f);
            farmSkin.DrawPixelPanel(rect, Paper, PaperEdge);
            FarmGuiSkin.DrawSolid(new Rect(rect.x + 10f, rect.y + 12f, 4f, 51f), Clay);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 8f, 248f, 25f),
                active ? $"진행 중인 의뢰  {current.Current}/{current.Target}" : "마을 의뢰", phoneSectionStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 36f, 248f, 34f),
                active ? current.Title : "광장 게시판에서 의뢰를 선택하세요", phoneBodyStyle);
        }

        private void DrawHotbar(GameSession session)
        {
            const float bottom = HudLayout.ReferenceHeight - 53f;
            const float gap = 8f;
            var bagWidth = GetKeyHintWidth("I", "가방");
            var actionKey = "E";
            var action = $"{GetToolLabel(session.SelectedTool)} 장착";
            var extraAction = string.Empty;
            if (session.SelectedDecoration.HasValue)
            {
                actionKey = "Space";
                action = $"{ItemCatalog.GetLabel(session.SelectedDecoration.Value)} 설치";
                extraAction = "줍기";
            }
            else if (session.SelectedTool == FarmTool.Hoe)
            {
                actionKey = "Space";
                action = "땅 갈기";
                extraAction = "상호작용";
            }
            else if (session.SelectedTool == FarmTool.Shovel)
            {
                action = "빈 밭 없애기";
            }
            else
            {
                actionKey = string.Empty;
            }

            var actionWidth = GetKeyHintWidth(actionKey, action);
            var extraWidth = string.IsNullOrEmpty(extraAction) ? 0f : GetKeyHintWidth("E", extraAction) + gap;
            var x = (HudLayout.ReferenceWidth - bagWidth - gap - actionWidth - extraWidth) * 0.5f;
            DrawKeyHint(new Rect(x, bottom, bagWidth, 37f), "I", "가방");
            x += bagWidth + gap;
            DrawKeyHint(new Rect(x, bottom, actionWidth, 37f), actionKey, action);
            if (extraWidth > 0f)
                DrawKeyHint(new Rect(x + actionWidth + gap, bottom, extraWidth - gap, 37f), "E", extraAction);
        }

        private void DrawDogCommand(GameSession session)
        {
            var label = session.Data.dogSitting ? "강아지 따라와" : "강아지 앉아";
            DrawKeyHint(new Rect(18f, HudLayout.ReferenceHeight - 53f, GetKeyHintWidth("G", label), 37f), "G", label);
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
            var settings = phonePage == PhonePage.Settings;
            var layout = HudLayout.CalculatePhoneLayout(settings);
            FarmGuiSkin.DrawSolid(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight), CanvasScrim);
            phoneSkin.DrawNotebook(layout.Panel, layout.Screen);
            phoneSkin.DrawHeader(layout.Header, layout.Title);
            GUI.Label(layout.Title, phonePage == PhonePage.Achievements ? "도전과제" : settings ? "설정" : "농장 수첩", phoneTitleStyle);

            if (phoneSkin.DrawClose(layout.CloseButton))
            {
                exitConfirmationPending = false;
                session.TogglePhone();
                return;
            }

            if (exitConfirmationPending)
            {
                DrawExitConfirmation(session, layout);
                return;
            }

            DrawPhoneTabs(layout);
            if (phonePage == PhonePage.Achievements)
            {
                DrawAchievements(session, Rect.MinMaxRect(layout.Content.x, layout.Content.y, layout.CloudPanel.xMax, layout.Content.yMax));
                return;
            }

            phoneSkin.DrawPageSeam(layout.Content, layout.CloudPanel);
            if (settings)
            {
                DrawPhoneSettingsTab(session, layout);
            }
            else
            {
                DrawPhoneFarmTab(session, layout);
                DrawCloudSync(session, layout.CloudPanel);
            }
        }

        private void DrawPhoneTabs(PhoneHudLayout layout)
        {
            if (DrawPhoneTab(GetPhoneTab(layout, 0), "농장", PhonePage.Farm))
            {
                phonePage = PhonePage.Farm;
                exitNotice = string.Empty;
            }

            if (DrawPhoneTab(GetPhoneTab(layout, 1), "설정", PhonePage.Settings))
            {
                phonePage = PhonePage.Settings;
                exitNotice = string.Empty;
            }

            if (DrawPhoneTab(GetPhoneTab(layout, 2), "도전과제", PhonePage.Achievements))
            {
                if (phonePage != PhonePage.Achievements) achievementScroll = Vector2.zero;
                phonePage = PhonePage.Achievements;
                exitNotice = string.Empty;
            }
        }

        private bool DrawPhoneTab(Rect rect, string title, PhonePage page)
        {
            var selected = phonePage == page;
            return phoneSkin.DrawButton(rect, title, selected ? PhoneButtonTone.Gold : PhoneButtonTone.Paper);
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
            var card = layout.Content;
            phoneSkin.DrawCard(card);
            GUI.Label(HudLayout.InsetRow(card, 22f, 17f, 33f), "의뢰 수첩", phoneHeadingStyle);
            GUI.Label(HudLayout.InsetRow(card, 22f, 50f, 28f),
                $"완료한 의뢰  {completed} / {quests.Length}", phoneSectionStyle);
            GUI.Label(layout.QuestBody,
                session.Data.activeQuestId < 0
                    ? "광장 게시판에서 다음 의뢰를 선택하세요."
                    : $"{active.Title} · {active.Current}/{active.Target}", phoneBodyStyle);

            phoneSkin.DrawProgress(HudLayout.InsetRow(card, 22f, 139f, 14f),
                quests.Length > 0 ? (float)completed / quests.Length : 0f);

            FarmGuiSkin.DrawSolid(HudLayout.InsetRow(card, 22f, 179f, 2f), PaperEdge);
            GUI.Label(HudLayout.InsetRow(card, 22f, 192f, 33f), "이 기기의 저장", phoneHeadingStyle);
            GUI.Label(HudLayout.InsetRow(card, 22f, 226f, 36f), phoneSaveNotice, phoneBodyStyle);
            if (phoneSkin.DrawButton(layout.SaveButton, "저장", PhoneButtonTone.Teal))
            {
                session.Save(false);
                phoneSaveNotice = session.Message;
            }

            if (phoneSkin.DrawButton(layout.LoadButton, "불러오기", PhoneButtonTone.Paper))
            {
                session.Load();
                phoneSaveNotice = session.Message;
            }
        }

        private void DrawPhoneSettingsTab(GameSession session, PhoneHudLayout layout)
        {
            phoneSkin.DrawCard(layout.Content);
            phoneSkin.DrawCard(layout.CloudPanel, warm: true);
            GUI.Label(HudLayout.InsetRow(layout.Content, 22f, 21f, 36f), "소리", phoneHeadingStyle);
            GUI.Label(layout.VolumeLabel, $"전체 음량  {Mathf.RoundToInt(masterVolume * 100f)}%", phoneSectionStyle);
            GUI.Label(HudLayout.InsetRow(layout.CloudPanel, 22f, 21f, 36f), "저장과 종료", phoneHeadingStyle);
            GUI.Label(HudLayout.InsetRow(layout.CloudPanel, 22f, 59f, 39f),
                "현재 진행 상황을 이 기기에 저장합니다.", phoneBodyStyle);
            var nextVolume = phoneSkin.DrawVolumeSlider(layout.VolumeSlider, masterVolume);
            if (!Mathf.Approximately(nextVolume, masterVolume))
            {
                ApplyMasterVolume(nextVolume);
                exitNotice = "음량 설정을 저장했습니다.";
            }

            var muteLabel = masterVolume <= 0.001f ? "소리 켜기" : "음소거";
            if (phoneSkin.DrawButton(layout.MuteButton, muteLabel, PhoneButtonTone.Paper))
            {
                var toggledVolume = masterVolume <= 0.001f
                    ? Mathf.Max(0.1f, volumeBeforeMute)
                    : 0f;
                ApplyMasterVolume(toggledVolume);
                exitNotice = toggledVolume <= 0.001f ? "소리를 껐습니다." : "소리를 켰습니다.";
            }

            if (phoneSkin.DrawButton(layout.SaveButton, "지금 저장", PhoneButtonTone.Teal))
            {
                session.Save(false);
                exitNotice = session.Message;
            }

            if (phoneSkin.DrawButton(layout.QuitButton, "저장하고 나가기", PhoneButtonTone.Coral))
            {
                exitConfirmationPending = true;
            }

            if (!string.IsNullOrEmpty(exitNotice))
            {
                GUI.Label(layout.Notice, exitNotice, phoneBodyStyle);
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
            phoneSkin.DrawCard(layout.ExitConfirmationPanel, warm: true);
            GUI.Label(HudLayout.InsetRow(layout.ExitConfirmationPanel, 25f, 21f, 36f), "게임 종료", phoneHeadingStyle);
            GUI.Label(layout.ExitConfirmationMessage,
                "현재 진행 상황을 저장하고\n게임에서 나갈까요?",
                phoneBodyStyle);

            if (phoneSkin.DrawButton(layout.CancelExitButton, "취소", PhoneButtonTone.Paper))
            {
                exitConfirmationPending = false;
            }

            if (!phoneSkin.DrawButton(layout.ConfirmExitButton, "저장 후 나가기", PhoneButtonTone.Coral))
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

        private void DrawCloudSync(GameSession session, Rect panel)
        {
            phoneSkin.DrawCard(panel, warm: true);
            GUI.Label(HudLayout.InsetRow(panel, 22f, 17f, 33f), "브라우저 동기화", phoneHeadingStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !session.CloudSyncBusy;
            try
            {
                if (session.CloudSyncLinked)
                    DrawLinkedCloudSync(session, panel);
                else
                    DrawCloudConnectionForm(session, panel);
            }
            finally
            {
                GUI.enabled = previousEnabled;
            }

            if (!string.IsNullOrEmpty(session.CloudSyncNotice))
            {
                GUI.Label(HudLayout.InsetRow(panel, 20f, 284f, 50f), session.CloudSyncNotice, phoneBodyStyle);
            }
        }

        private void DrawCloudConnectionForm(GameSession session, Rect panel)
        {
            GUI.Label(HudLayout.InsetRow(panel, 20f, 49f, 43f),
                "다른 브라우저의 코드와 PIN을 입력하세요.", phoneBodyStyle);
            cloudCodeInput = DrawCloudField(HudLayout.InsetRow(panel, 20f, 99f, 36f),
                "코드", "cloud-code", cloudCodeInput.ToUpperInvariant(), 10, false);
            cloudPinInput = DrawCloudField(HudLayout.InsetRow(panel, 20f, 143f, 36f),
                "PIN", "cloud-pin", cloudPinInput, 6, true);
            if (phoneSkin.DrawButton(HudLayout.InsetRow(panel, 20f, 194f, 37f), "기존 저장 연결", PhoneButtonTone.Teal))
                session.ConnectCloudSave(cloudCodeInput, cloudPinInput);

            if (phoneSkin.DrawButton(HudLayout.InsetRow(panel, 20f, 239f, 37f), "새 동기화 코드 만들기", PhoneButtonTone.Gold))
                session.CreateCloudSave();
        }

        private void DrawLinkedCloudSync(GameSession session, Rect panel)
        {
            var connectionText = session.CloudLoadRequired
                ? "다른 브라우저의 저장이 있습니다.\n최신 저장 받기를 먼저 눌러 주세요."
                : string.IsNullOrEmpty(session.NewlyCreatedCloudPin)
                ? $"연결 코드  {session.CloudSyncCode}\n이 기기에서 저장하면 클라우드에도 저장됩니다."
                : $"연결 코드  {session.CloudSyncCode}\nPIN  {session.NewlyCreatedCloudPin}  · 지금 메모해 두세요";
            GUI.Label(HudLayout.InsetRow(panel, 20f, 59f, 93f), connectionText, phoneBodyStyle);

            if (phoneSkin.DrawButton(new Rect(panel.x + 20f, panel.y + 166f, 172f, 41f),
                "최신 저장 받기", PhoneButtonTone.Teal))
                session.LoadCloudSave();
            if (phoneSkin.DrawButton(new Rect(panel.xMax - 192f, panel.y + 166f, 172f, 41f),
                "연결 해제", PhoneButtonTone.Paper))
                session.DisconnectCloudSave();

            GUI.Label(HudLayout.InsetRow(panel, 20f, 224f, 47f),
                "다른 브라우저에서는 같은 코드와 PIN을 입력하세요.", phoneBodyStyle);
        }

        private void ExecutePurchase(GameSession session, ShopPurchaseRequest request)
        {
            var count = ShopBulkOrder.ClampCount(
                request, shopQuantity, session.Data.money, session.Data.ownedChickens, session.Data.ownedCows);
            if (count < 1)
            {
                return;
            }

            switch (request.Kind)
            {
                case ShopPurchaseKind.Seed:
                    session.PurchaseSeed(request.Item, count);
                    break;
                case ShopPurchaseKind.Feed:
                    session.PurchaseFeed(count);
                    break;
                case ShopPurchaseKind.Chicken:
                    session.PurchaseChicken(count);
                    break;
                case ShopPurchaseKind.ChickenSale:
                    session.SellChicken(count);
                    break;
                case ShopPurchaseKind.Cow:
                    session.PurchaseCow(count);
                    break;
                case ShopPurchaseKind.CowSale:
                    session.SellCow(count);
                    break;
                case ShopPurchaseKind.Pickaxe:
                    session.PurchasePickaxe();
                    break;
                case ShopPurchaseKind.Ore:
                    session.PurchaseOre(request.Item, request.Quantity * count, request.Price * count);
                    break;
                case ShopPurchaseKind.Sprinkler:
                    session.PurchaseSprinkler(count);
                    break;
                case ShopPurchaseKind.Decoration:
                    session.PurchaseDecoration(request.Item, count);
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
