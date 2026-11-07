using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.TimeSystem;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        public const string CharacterChoiceTitle = "당신의 성별을 골라주세요";

        private PrototypePlayerController player;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle centeredStyle;
        private GUIStyle titleStyle;
        private GUIStyle speakerStyle;
        private Font koreanFont;
        private FarmGuiSkin farmSkin;
        private readonly HudMessageVisibility messageVisibility = new();

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
            var rect = new Rect(HudLayout.ReferenceWidth - 300f, 14f, 282f, 118f);
            farmSkin.DrawBanner(rect);
            GUI.Label(new Rect(rect.x + 82f, rect.y + 18f, 178f, 88f),
                $"{GetSeasonLabel(session.CurrentSeason)} {SeasonCalendar.GetDayOfSeason(session.Data.day)}일  {timeText}\n{session.Data.money}G\n씨앗 {session.Data.inventory.Count(GameItem.WheatSeed)} · 밀 {session.Data.inventory.Count(GameItem.Wheat)}\n사료 {session.Data.inventory.Count(GameItem.Feed)} · 달걀 {session.Data.inventory.Count(GameItem.Egg)}",
                smallStyle);
        }

        private void DrawQuest(GameSession session)
        {
            var progress = Mathf.Min(session.Data.wheatHarvested, 3);
            var quest = session.Data.harvestQuestCompleted
                ? "완료: 첫 밀 수확"
                : $"퀘스트: 밀 3개 수확 ({progress}/3)";
            var rect = new Rect(14f, 14f, 320f, 100f);
            farmSkin.DrawBanner(rect);
            GUI.Label(new Rect(rect.x + 84f, rect.y + 18f, 208f, 62f), quest, bodyStyle);
        }

        private void DrawHotbar(GameSession session)
        {
            var label = $"[1] 호미  [2] 밀 씨앗  [3] 물뿌리개   선택: {GetToolLabel(session.SelectedTool)}";
            var rect = new Rect((HudLayout.ReferenceWidth - 520f) / 2f, HudLayout.ReferenceHeight - 70f, 520f, 54f);
            farmSkin.DrawButtonBackground(rect, true);
            GUI.Label(rect, label, centeredStyle);
        }

        private void DrawMessage(GameSession session)
        {
            if (session.ActivePortraitRow < 0)
            {
                var toast = new Rect((HudLayout.ReferenceWidth - 620f) / 2f, 132f, 620f, 72f);
                farmSkin.DrawBanner(toast);
                GUI.Label(new Rect(toast.x + 48f, toast.y + 12f, toast.width - 96f, 48f), session.Message, centeredStyle);
                return;
            }

            var rect = new Rect(90f, 466f, 1100f, 174f);
            farmSkin.DrawDialogue(rect);
            var portrait = GameArtCatalog.GetNpcPortrait(session.ActivePortraitRow);
            farmSkin.DrawSprite(new Rect(rect.x + 42f, rect.y + 24f, 126f, 126f), portrait);
            GUI.Label(new Rect(rect.x + 192f, rect.y + 30f, rect.width - 245f, 32f), session.ActiveSpeaker, speakerStyle);
            GUI.Label(new Rect(rect.x + 192f, rect.y + 72f, rect.width - 246f, 76f), session.Message, bodyStyle);
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
            GUI.Label(new Rect(rect.x + 32f, rect.y + 20f, 356f, 42f), CharacterChoiceTitle, titleStyle);
            GUI.Label(new Rect(rect.x + 38f, rect.y + 70f, 344f, 38f), "외형만 다르며 능력 차이는 없습니다.", bodyStyle);
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
            var rect = new Rect((HudLayout.ReferenceWidth - 400f) / 2f, 150f, 400f, 350f);
            farmSkin.DrawWindow(rect);
            GUI.Label(new Rect(rect.x + 35f, rect.y + 18f, 330f, 40f), "농장 휴대폰", titleStyle);
            GUI.Label(new Rect(rect.x + 42f, rect.y + 72f, 316f, 88f),
                $"퀘스트: 밀 수확 {Mathf.Min(session.Data.wheatHarvested, 3)}/3\n첫 수확: {(session.Data.firstHarvestAchievement ? "달성" : "미달성")}\n첫 달걀: {(session.Data.firstEggAchievement ? "달성" : "미달성")}", bodyStyle);

            if (farmSkin.DrawButton(new Rect(rect.x + 40f, rect.y + 190f, 140f, 48f), "저장"))
            {
                session.Save(false);
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 220f, rect.y + 190f, 140f, 48f), "불러오기", true))
            {
                session.Load();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 130f, rect.y + 270f, 140f, 48f), "닫기 (P)"))
            {
                session.TogglePhone();
            }
        }

        private void DrawShop(GameSession session)
        {
            var rect = new Rect((HudLayout.ReferenceWidth - 420f) / 2f, 180f, 420f, 300f);
            farmSkin.DrawWindow(rect);
            GUI.Label(new Rect(rect.x + 35f, rect.y + 20f, 350f, 42f), "씨앗·사료 상점", titleStyle);
            GUI.Label(new Rect(rect.x + 45f, rect.y + 72f, 330f, 32f), $"보유 금액: {session.Data.money}G", bodyStyle);
            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 115f, 330f, 46f), "밀 씨앗 5개 — 20G"))
            {
                session.PurchaseSeeds();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 45f, rect.y + 172f, 330f, 46f), "닭 사료 3개 — 15G", true))
            {
                session.PurchaseFeed();
            }

            if (farmSkin.DrawButton(new Rect(rect.x + 150f, rect.y + 238f, 120f, 38f), "닫기"))
            {
                session.ToggleShop();
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
            speakerStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
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
                FarmTool.Seeds => "밀 씨앗",
                FarmTool.WateringCan => "물뿌리개",
                _ => tool.ToString()
            };
        }
    }
}
