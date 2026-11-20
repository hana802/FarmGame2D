using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.TimeSystem;
using FarmGame2D.World;
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

        private PrototypePlayerController player;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centeredStyle;
        private GUIStyle titleStyle;
        private GUIStyle inventoryTitleStyle;
        private GUIStyle speakerStyle;
        private GUIStyle dialogueBodyStyle;
        private GUIStyle panelStyle;
        private GUIStyle interactionPromptStyle;
        private GUIStyle interactionKeyStyle;
        private GUIStyle fishingMarkerStyle;
        private Font koreanFont;
        private FarmGuiSkin farmSkin;
        private readonly HudMessageVisibility messageVisibility = new();
        private readonly ShopPurchaseConfirmation shopPurchaseConfirmation = new();
        private Vector2 shopScrollPosition;
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
                if (!session.PhoneOpen)
                {
                    DrawHotbar(session);
                    DrawDogCommand(session);
                    DrawInteractionPrompt();
                }

                var modalOpen = session.PhoneOpen
                    || session.ShopOpen
                    || session.InventoryOpen
                    || session.QuestBoardOpen
                    || NpcInteractionOverlay.IsOpen;
                if (!modalOpen)
                {
                    DrawFishingMarker(viewport);
                }

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

                if (session.InventoryOpen)
                {
                    DrawInventory(session);
                }

                if (session.QuestBoardOpen)
                {
                    DrawQuestBoard(session);
                }
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawFishingMarker(HudViewport viewport)
        {
            if (player == null || !player.IsFishing || Camera.main is not Camera worldCamera) return;

            var screen = worldCamera.WorldToScreenPoint(player.transform.position + Vector3.up * 1.25f);
            if (screen.z <= 0f) return;

            const float width = 88f;
            const float height = 28f;
            var x = (screen.x - viewport.OffsetX) / viewport.Scale;
            var y = (Screen.height - screen.y - viewport.OffsetY) / viewport.Scale;
            if (fishingMarkerStyle == null)
            {
                fishingMarkerStyle = NpcSpeechBubblePresentation.CreateStyle(GUI.skin.box, koreanFont);
                fishingMarkerStyle.fontSize = 14;
            }

            NpcSpeechBubblePresentation.Draw(new Rect(x - width * 0.5f, y - height - 6f, width, height),
                "낚시 중", fishingMarkerStyle);
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
                $"돌 {session.Data.inventory.Count(GameItem.Stone)} · 구리 {session.Data.inventory.Count(GameItem.CopperOre)} · 철 {session.Data.inventory.Count(GameItem.IronOre)} · 금 {session.Data.inventory.Count(GameItem.GoldOre)}\n물고기 {fishCount} · 잡초 {session.Data.inventory.Count(GameItem.Weed)} · 돈 {session.Data.money}G",
                smallStyle);

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
                GUI.Label(toastLayout.Message, session.Message, centeredStyle);
                return;
            }

            var layout = HudLayout.CalculateDialogueLayout();
            farmSkin.DrawDialogue(layout.Panel);
            var portrait = GameArtCatalog.GetNpcPortrait(session.ActivePortraitRow);
            farmSkin.DrawSprite(layout.Portrait, portrait);
            GUI.Label(layout.Name, session.ActiveSpeaker, speakerStyle);
            GUI.Label(layout.Body, session.Message, dialogueBodyStyle);
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
            farmSkin.DrawPixelPanel(layout.Panel, Pine, DarkEdge);
            var phoneIcon = new Rect(layout.Panel.x + 23f, layout.Panel.y + 19f, 21f, 28f);
            farmSkin.DrawPixelPanel(phoneIcon, MidPine, Amber, false);
            FarmGuiSkin.DrawSolid(new Rect(phoneIcon.x + 7f, phoneIcon.yMax - 6f, 7f, 2f), Amber);
            GUI.Label(layout.Title, "농장 휴대폰", phoneTitleStyle);

            if (farmSkin.DrawPanelButton(layout.CloseButton, "닫기  P", DarkEdge, MidPine))
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
            if (DrawPhoneTab(layout.FarmTabButton, "농장", PhonePage.Farm))
            {
                phonePage = PhonePage.Farm;
                exitNotice = string.Empty;
            }

            if (DrawPhoneTab(layout.SettingsTabButton, "설정", PhonePage.Settings))
            {
                phonePage = PhonePage.Settings;
                exitNotice = string.Empty;
            }
        }

        private bool DrawPhoneTab(Rect rect, string title, PhonePage page)
        {
            var selected = phonePage == page;
            return farmSkin.DrawPanelButton(rect, title, selected ? Paper : MidPine,
                selected ? Amber : DarkEdge, !selected);
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
            farmSkin.DrawPixelPanel(card, Paper, PaperEdge, false);
            GUI.Label(HudLayout.InsetRow(card, 22f, 16f, 29f), "의뢰 수첩", phoneSectionStyle);
            GUI.Label(HudLayout.InsetRow(card, 22f, 48f, 27f),
                $"완료한 의뢰  {completed} / {quests.Length}", phoneSectionStyle);
            GUI.Label(layout.QuestBody,
                session.Data.activeQuestId < 0
                    ? "광장 게시판에서 다음 의뢰를 선택하세요."
                    : $"{active.Title} · {active.Current}/{active.Target}", phoneBodyStyle);

            var progress = HudLayout.InsetRow(card, 22f, 139f, 12f);
            FarmGuiSkin.DrawSolid(progress, new Color32(209, 187, 147, 255));
            if (completed > 0)
            {
                progress.width *= (float)completed / quests.Length;
                FarmGuiSkin.DrawSolid(progress, MidPine);
            }

            FarmGuiSkin.DrawSolid(HudLayout.InsetRow(card, 22f, 179f, 2f), PaperEdge);
            GUI.Label(HudLayout.InsetRow(card, 22f, 195f, 29f), "이 기기의 저장", phoneSectionStyle);
            GUI.Label(HudLayout.InsetRow(card, 22f, 226f, 36f), phoneSaveNotice, phoneBodyStyle);
            if (farmSkin.DrawPanelButton(layout.SaveButton, "저장", MidPine, DarkEdge))
            {
                session.Save(false);
                phoneSaveNotice = session.Message;
            }

            if (farmSkin.DrawPanelButton(layout.LoadButton, "불러오기", Cream, PaperEdge, false))
            {
                session.Load();
                phoneSaveNotice = session.Message;
            }
        }

        private void DrawPhoneSettingsTab(GameSession session, PhoneHudLayout layout)
        {
            farmSkin.DrawPixelPanel(layout.Content, Paper, PaperEdge, false);
            GUI.Label(HudLayout.InsetRow(layout.Content, 22f, 17f, 30f), "소리", phoneSectionStyle);
            GUI.Label(layout.VolumeLabel, $"전체 음량  {Mathf.RoundToInt(masterVolume * 100f)}%", phoneBodyStyle);
            var nextVolume = GUI.HorizontalSlider(layout.VolumeSlider, masterVolume, 0f, 1f);
            if (!Mathf.Approximately(nextVolume, masterVolume))
            {
                ApplyMasterVolume(nextVolume);
                exitNotice = "음량 설정을 저장했습니다.";
            }

            var muteLabel = masterVolume <= 0.001f ? "소리 켜기" : "음소거";
            if (farmSkin.DrawPanelButton(layout.MuteButton, muteLabel, Cream, PaperEdge, false))
            {
                var toggledVolume = masterVolume <= 0.001f
                    ? Mathf.Max(0.1f, volumeBeforeMute)
                    : 0f;
                ApplyMasterVolume(toggledVolume);
                exitNotice = toggledVolume <= 0.001f ? "소리를 껐습니다." : "소리를 켰습니다.";
            }

            if (farmSkin.DrawPanelButton(layout.SaveButton, "지금 저장", MidPine, DarkEdge))
            {
                session.Save(false);
                exitNotice = session.Message;
            }

            if (farmSkin.DrawPanelButton(layout.QuitButton, "저장하고 나가기", Clay, PaperEdge))
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
            farmSkin.DrawPixelPanel(layout.ExitConfirmationPanel, Paper, PaperEdge, false);
            GUI.Label(HudLayout.InsetRow(layout.ExitConfirmationPanel, 22f, 18f, 32f), "게임 종료", phoneSectionStyle);
            GUI.Label(layout.ExitConfirmationMessage,
                "현재 진행 상황을 저장하고\n게임에서 나갈까요?",
                phoneBodyStyle);

            if (farmSkin.DrawPanelButton(layout.CancelExitButton, "취소", Cream, PaperEdge, false))
            {
                exitConfirmationPending = false;
            }

            if (!farmSkin.DrawPanelButton(layout.ConfirmExitButton, "저장 후 나가기", Clay, PaperEdge))
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
            farmSkin.DrawPixelPanel(panel, DarkEdge, MidPine, false);
            GUI.Label(HudLayout.InsetRow(panel, 20f, 15f, 31f), "브라우저 동기화", warmLightStyle);

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
                GUI.Label(HudLayout.InsetRow(panel, 20f, 282f, 49f), session.CloudSyncNotice, phoneLightStyle);
            }
        }

        private void DrawCloudConnectionForm(GameSession session, Rect panel)
        {
            GUI.Label(HudLayout.InsetRow(panel, 20f, 49f, 43f),
                "다른 브라우저의 코드와 PIN을 입력하세요.", phoneLightStyle);
            cloudCodeInput = DrawCloudField(HudLayout.InsetRow(panel, 20f, 99f, 36f),
                "코드", "cloud-code", cloudCodeInput.ToUpperInvariant(), 10, false);
            cloudPinInput = DrawCloudField(HudLayout.InsetRow(panel, 20f, 143f, 36f),
                "PIN", "cloud-pin", cloudPinInput, 6, true);
            if (farmSkin.DrawPanelButton(HudLayout.InsetRow(panel, 20f, 194f, 36f), "기존 저장 연결", Amber, PaperEdge, false))
                session.ConnectCloudSave(cloudCodeInput, cloudPinInput);

            if (farmSkin.DrawPanelButton(HudLayout.InsetRow(panel, 20f, 238f, 36f), "새 동기화 코드 만들기", MidPine, DarkEdge))
                session.CreateCloudSave();
        }

        private void DrawLinkedCloudSync(GameSession session, Rect panel)
        {
            var connectionText = session.CloudLoadRequired
                ? "다른 브라우저의 저장이 있습니다.\n최신 저장 받기를 먼저 눌러 주세요."
                : string.IsNullOrEmpty(session.NewlyCreatedCloudPin)
                ? $"연결 코드  {session.CloudSyncCode}\n이 기기에서 저장하면 클라우드에도 저장됩니다."
                : $"연결 코드  {session.CloudSyncCode}\nPIN  {session.NewlyCreatedCloudPin}  · 지금 메모해 두세요";
            GUI.Label(HudLayout.InsetRow(panel, 20f, 59f, 93f), connectionText, phoneLightStyle);

            if (farmSkin.DrawPanelButton(new Rect(panel.x + 20f, panel.y + 166f, 165f, 41f),
                "최신 저장 받기", Amber, PaperEdge, false))
                session.LoadCloudSave();
            if (farmSkin.DrawPanelButton(new Rect(panel.xMax - 185f, panel.y + 166f, 165f, 41f),
                "연결 해제", MidPine, DarkEdge))
                session.DisconnectCloudSave();

            GUI.Label(HudLayout.InsetRow(panel, 20f, 224f, 47f),
                "다른 브라우저에서는 같은 코드와 PIN을 입력하세요.", phoneLightStyle);
        }

        private void DrawShop(GameSession session)
        {
            var layout = HudLayout.CalculateShopLayout();
            farmSkin.DrawWindow(layout.Panel);
            GUI.Label(layout.Title, ShopTitle, titleStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && !shopPurchaseConfirmation.Pending.HasValue;

            if (session.ActiveShop == ShopKind.Mining)
            {
                DrawMiningShop(session, layout);
            }
            else
            {
                DrawFarmShop(session, layout);
            }

            if (farmSkin.DrawButton(layout.CloseButton, "닫기"))
            {
                shopPurchaseConfirmation.Cancel();
                session.OpenShop(session.ActiveShop);
            }

            GUI.enabled = previousEnabled;
            DrawPurchaseConfirmation(session, layout);
        }

        private void DrawFarmShop(GameSession session, ShopHudLayout layout)
        {
            var crops = CropCatalog.GetForSeason(session.CurrentSeason);
            var productRows = crops.Count + 4;
            BeginShopScroll(layout, productRows, 72f);
            var row = 0;
            for (var index = 0; index < crops.Count; index++)
            {
                var crop = crops[index];
                var displayName = $"{crop.Label} 씨앗 5개";
                if (farmSkin.DrawButton(layout.GetScrollableProductButton(row), $"{displayName} — {crop.PackPrice}G"))
                {
                    shopPurchaseConfirmation.Request(new ShopPurchaseRequest(
                        ShopPurchaseKind.Seed,
                        displayName,
                        crop.Seed));
                }

                row++;
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(row++), "동물 사료 3개 — 15G", true))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Feed, "동물 사료 3개"));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(row++), "닭 1마리 — 150G"))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Chicken, "닭 1마리"));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(row++), "닭 1마리 판매 — +100G", true))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.ChickenSale, "닭 1마리", price: 100));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(row++), "소 1마리 — 350G"))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Cow, "소 1마리"));
            }

            GUI.Label(layout.GetScrollableInfoLabel(row, 64f), $"현재 보유: 닭 {session.Data.ownedChickens}마리 · 소 {session.Data.ownedCows}마리\n먹이를 주면 다음 날 달걀과 우유를 얻습니다.", centeredStyle);
            GUI.EndScrollView();
        }

        private void DrawMiningShop(GameSession session, ShopHudLayout layout)
        {
            const int productRows = 7;
            BeginShopScroll(layout, productRows, 92f);
            if (farmSkin.DrawButton(layout.GetScrollableProductButton(0), session.Data.hasPickaxe ? "곡괭이 — 구입 완료" : "곡괭이 — 120G"))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Pickaxe, "곡괭이"));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(1), "돌 5개 — 30G", true))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Ore, "돌 5개", GameItem.Stone, 5, 30));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(2), "구리 광석 3개 — 75G"))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Ore, "구리 광석 3개", GameItem.CopperOre, 3, 75));
            }

            if (farmSkin.DrawButton(layout.GetScrollableProductButton(3), "철 광석 2개 — 110G", true))
            {
                shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Ore, "철 광석 2개", GameItem.IronOre, 2, 110));
            }

            var decorations = TownDecorationCatalog.GetAll();
            for (var index = 0; index < decorations.Count; index++)
            {
                var decoration = decorations[index];
                var label = ItemCatalog.GetLabel(decoration.Item);
                if (farmSkin.DrawButton(layout.GetScrollableProductButton(4 + index), $"{label} — {decoration.Price}G", index % 2 == 0))
                    shopPurchaseConfirmation.Request(new ShopPurchaseRequest(ShopPurchaseKind.Decoration, label, decoration.Item));
            }

            GUI.Label(layout.GetScrollableInfoLabel(productRows, 84f), "장식품은 가방의 마을 장식 탭에서 골라 설치하세요.\n마을 잔디에 Space로 놓고 E로 다시 주울 수 있습니다.", centeredStyle);
            GUI.EndScrollView();
        }

        private void BeginShopScroll(ShopHudLayout layout, int productRows, float infoHeight)
        {
            var content = new Rect(
                0f,
                0f,
                layout.ProductsViewport.width - 18f,
                layout.GetContentHeight(productRows, infoHeight));
            shopScrollPosition = GUI.BeginScrollView(layout.ProductsViewport, shopScrollPosition, content, false, true);
        }

        private void DrawPurchaseConfirmation(GameSession session, ShopHudLayout layout)
        {
            if (!shopPurchaseConfirmation.Pending.HasValue)
            {
                return;
            }

            var request = shopPurchaseConfirmation.Pending.Value;
            farmSkin.DrawBanner(layout.ConfirmationPanel);
            GUI.Label(layout.ConfirmationMessage, ShopPurchaseConfirmation.GetPrompt(request), centeredStyle);
            if (farmSkin.DrawButton(layout.CancelButton, "취소", true))
            {
                shopPurchaseConfirmation.Cancel();
            }

            if (farmSkin.DrawButton(layout.PurchaseButton, request.IsSale ? "판매" : "구매"))
            {
                shopPurchaseConfirmation.Confirm(confirmed => ExecutePurchase(session, confirmed));
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
                case ShopPurchaseKind.Pickaxe:
                    session.PurchasePickaxe();
                    break;
                case ShopPurchaseKind.Ore:
                    session.PurchaseOre(request.Item, request.Quantity, request.Price);
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
            speakerStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            dialogueBodyStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 19,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                clipping = TextClipping.Clip,
                padding = new RectOffset(8, 8, 4, 4)
            };
            panelStyle ??= new GUIStyle(smallStyle)
            {
                fontSize = 14,
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
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
