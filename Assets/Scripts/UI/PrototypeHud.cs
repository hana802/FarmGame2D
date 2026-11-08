using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.TimeSystem;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        public const string CharacterChoiceHeader = "선택해주세요.";
        public const string CharacterChoicePrompt = "당신의 성별을 골라주세요";

        private PrototypePlayerController player;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centeredStyle;
        private GUIStyle titleStyle;
        private GUIStyle speakerStyle;
        private GUIStyle dialogueBodyStyle;
        private GUIStyle panelStyle;
        private Font koreanFont;
        private FarmGuiSkin farmSkin;
        private readonly HudMessageVisibility messageVisibility = new();
        private string cloudCodeInput = string.Empty;
        private string cloudPinInput = string.Empty;

        public bool HasArtSkin => farmSkin?.IsReady == true;

        public void Configure(PrototypePlayerController controlledPlayer)
        {
            player = controlledPlayer;
            koreanFont = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
            farmSkin = new FarmGuiSkin(koreanFont);
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
                DrawHotbar(session);
                DrawInteractionPrompt();

                if (messageVisibility.ShouldShow(session.MessageRevision, Time.unscaledTime, session.MessageDisplaySeconds))
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
            var label = session.SelectedTool == FarmTool.Hoe
                ? "[I] 가방   [Space] 앞의 땅 갈기   [E] 상호작용"
                : $"[I] 가방 열기   들고 있음: {GetToolLabel(session.SelectedTool)}";
            var rect = new Rect((HudLayout.ReferenceWidth - 430f) / 2f, HudLayout.ReferenceHeight - 70f, 430f, 54f);
            farmSkin.DrawButtonBackground(rect, true);
            GUI.Label(rect, label, centeredStyle);
        }

        private void DrawInventory(GameSession session)
        {
            var layout = HudLayout.CalculateInventoryLayout();
            farmSkin.DrawWindow(layout.Panel);
            GUI.Label(layout.Title, "농장 가방", titleStyle);

            if (farmSkin.DrawButton(layout.HoeButton, "호미"))
            {
                session.SelectTool(FarmTool.Hoe);
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

            if (farmSkin.DrawTextButton(layout.CloseButton, "가방 닫기 (I)"))
            {
                session.ToggleInventory();
            }
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

            var rect = new Rect(440f, 392f, 400f, 58f);
            farmSkin.DrawBanner(rect);
            GUI.Label(new Rect(rect.x + 35f, rect.y + 9f, rect.width - 70f, 40f), prompt, centeredStyle);
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

            if (farmSkin.DrawButton(layout.CloseButton, "닫기 (P)"))
            {
                session.TogglePhone();
            }

            DrawCloudSync(session, layout.Panel);
        }

        private void DrawQuestBoard(GameSession session)
        {
            var panel = new Rect((HudLayout.ReferenceWidth - 620f) / 2f, 72f, 620f, 520f);
            farmSkin.DrawWindow(panel);
            GUI.Label(new Rect(panel.x + 120f, panel.y + 24f, 380f, 42f), "마을 광장 의뢰 게시판", titleStyle);
            GUI.Label(new Rect(panel.x + 55f, panel.y + 72f, 510f, 48f),
                session.Data.activeQuestId >= 0 ? "진행 중인 의뢰를 먼저 끝내 주세요." : "하고 싶은 의뢰를 하나 골라 보세요.", centeredStyle);

            var quests = StarterQuestProgress.GetAll(session.Data);
            for (var index = 0; index < quests.Length; index++)
            {
                var quest = quests[index];
                var label = quest.Completed
                    ? $"완료 · {quest.Title}"
                    : $"{quest.Title}  ({quest.Current}/{quest.Target})";
                var previousEnabled = GUI.enabled;
                GUI.enabled = !quest.Completed && session.Data.activeQuestId < 0;
                if (farmSkin.DrawButton(new Rect(panel.x + 70f, panel.y + 130f + (index * 60f), 480f, 48f), label,
                        session.Data.activeQuestId == (int)quest.Id))
                {
                    session.AcceptQuest(quest.Id);
                }
                GUI.enabled = previousEnabled;
            }

            if (farmSkin.DrawTextButton(new Rect(panel.x + 210f, panel.y + 450f, 200f, 46f), "게시판 닫기"))
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
                    ? $"연결된 코드  {session.CloudSyncCode}\n저장을 누르면 클라우드에도 저장됩니다."
                    : $"연결 코드  {session.CloudSyncCode}\nPIN  {session.NewlyCreatedCloudPin}  · 지금 메모해 두세요";
                GUI.Label(new Rect(panel.x + 42f, panel.y + 102f, 316f, 62f),
                    connectionText, centeredStyle);

                if (farmSkin.DrawButton(new Rect(panel.x + 40f, panel.y + 184f, 150f, 46f), "최신 저장 받기", true))
                {
                    session.LoadCloudSave();
                }

                if (farmSkin.DrawButton(new Rect(panel.x + 210f, panel.y + 184f, 150f, 46f), "연결 해제"))
                {
                    session.DisconnectCloudSave();
                }

                GUI.Label(new Rect(panel.x + 42f, panel.y + 240f, 316f, 50f),
                    "다른 브라우저에서는 같은 코드와 PIN을 입력하세요.", centeredStyle);
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

        private void DrawShop(GameSession session)
        {
            var rect = new Rect((HudLayout.ReferenceWidth - 620f) / 2f, 92f, 620f, 550f);
            farmSkin.DrawWindow(rect);
            var title = session.ActiveShop == ShopKind.Mining ? "광물 상점" : "씨앗·동물 상점";
            GUI.Label(new Rect(rect.x + 35f, rect.y + 20f, 550f, 42f), title, titleStyle);
            GUI.Label(new Rect(rect.x + 45f, rect.y + 72f, 530f, 32f), $"{GetSeasonLabel(session.CurrentSeason)} · 보유 금액 {session.Data.money}G", centeredStyle);

            if (session.ActiveShop == ShopKind.Mining)
            {
                DrawMiningShop(session, rect);
            }
            else
            {
                DrawFarmShop(session, rect);
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 240f, rect.y + 490f, 140f, 42f), "닫기"))
            {
                session.OpenShop(session.ActiveShop);
            }
        }

        private void DrawFarmShop(GameSession session, Rect rect)
        {
            var crops = CropCatalog.GetForSeason(session.CurrentSeason);
            for (var index = 0; index < crops.Count; index++)
            {
                var crop = crops[index];
                if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 120f + (index * 58f), 530f, 48f), $"{crop.Label} 씨앗 5개 — {crop.PackPrice}G"))
                {
                    session.PurchaseSeed(crop.Seed);
                }
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 245f, 250f, 48f), "동물 사료 3개 — 15G", true))
            {
                session.PurchaseFeed();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 325f, rect.y + 245f, 250f, 48f), "닭 1마리 — 150G"))
            {
                session.PurchaseChicken();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 310f, 530f, 48f), "소 1마리 — 350G", true))
            {
                session.PurchaseCow();
            }

            GUI.Label(new Rect(rect.x + 55f, rect.y + 380f, 510f, 70f), $"현재 보유: 닭 {session.Data.ownedChickens}마리 · 소 {session.Data.ownedCows}마리\n먹이를 주면 다음 날 달걀과 우유를 얻습니다.", centeredStyle);
        }

        private void DrawMiningShop(GameSession session, Rect rect)
        {
            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 120f, 530f, 48f), session.Data.hasPickaxe ? "곡괭이 — 구입 완료" : "곡괭이 — 120G"))
            {
                session.PurchasePickaxe();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 185f, 250f, 48f), "돌 5개 — 30G", true))
            {
                session.PurchaseOre(GameItem.Stone, 5, 30);
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 325f, rect.y + 185f, 250f, 48f), "구리 광석 3개 — 75G"))
            {
                session.PurchaseOre(GameItem.CopperOre, 3, 75);
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 250f, 530f, 48f), "철 광석 2개 — 110G", true))
            {
                session.PurchaseOre(GameItem.IronOre, 2, 110);
            }

            GUI.Label(new Rect(rect.x + 55f, rect.y + 330f, 510f, 90f), "곡괭이를 가방에서 선택한 뒤 마을 서남쪽 광맥을 캐세요.\n채굴한 광물은 판매 상자에 넣거나 이곳에서 바로 구입할 수 있습니다.", centeredStyle);
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
