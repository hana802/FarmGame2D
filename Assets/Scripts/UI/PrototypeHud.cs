using FarmGame2D.Core;
using FarmGame2D.Player;
using FarmGame2D.TimeSystem;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private PrototypePlayerController player;
        private GUIStyle bodyStyle;
        private GUIStyle titleStyle;
        private Font koreanFont;

        public void Configure(PrototypePlayerController controlledPlayer)
        {
            player = controlledPlayer;
            koreanFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 18);
        }

        private void OnGUI()
        {
            var session = GameSession.Instance;
            if (session == null)
            {
                return;
            }

            EnsureStyles();
            DrawStatus(session);
            DrawQuest(session);
            DrawHotbar(session);
            DrawDialogue(session);

            if (session.Data.characterChoice == CharacterChoice.None)
            {
                DrawCharacterChoice(session);
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

        private void DrawStatus(GameSession session)
        {
            var time = GameClockController.Instance?.CurrentTime;
            var timeText = time.HasValue ? $"{time.Value.Hour:00}:{time.Value.Minute:00}" : "--:--";
            var rect = new Rect(Screen.width - 250f, 18f, 232f, 105f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, 210f, 88f),
                $"{session.CurrentSeason} {SeasonCalendar.GetDayOfSeason(session.Data.day)}일  {timeText}\n{session.Data.money}G\n씨앗 {session.Data.inventory.Count(GameItem.WheatSeed)} · 밀 {session.Data.inventory.Count(GameItem.Wheat)} · 사료 {session.Data.inventory.Count(GameItem.Feed)} · 달걀 {session.Data.inventory.Count(GameItem.Egg)}",
                bodyStyle);
        }

        private void DrawQuest(GameSession session)
        {
            var progress = Mathf.Min(session.Data.wheatHarvested, 3);
            var quest = session.Data.harvestQuestCompleted
                ? "완료: 첫 밀 수확"
                : $"퀘스트: 밀 3개 수확 ({progress}/3)";
            GUI.Box(new Rect(18f, 18f, 285f, 76f), string.Empty);
            GUI.Label(new Rect(30f, 28f, 260f, 56f), quest + "\nF6: 계절 미리보기", bodyStyle);
        }

        private void DrawHotbar(GameSession session)
        {
            var label = $"[1] 호미  [2] 밀 씨앗  [3] 물뿌리개   선택: {session.SelectedTool}";
            GUI.Box(new Rect((Screen.width - 470f) / 2f, Screen.height - 66f, 470f, 48f), label);
        }

        private void DrawDialogue(GameSession session)
        {
            var prompt = string.IsNullOrEmpty(player?.NearbyPrompt) ? "WASD 이동 · E 상호작용 · P 휴대폰 · F5 저장 · F9 불러오기" : player.NearbyPrompt;
            var rect = new Rect(18f, Screen.height - 150f, Screen.width - 36f, 68f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width - 28f, 48f), $"{session.Message}\n{prompt}", bodyStyle);
        }

        private void DrawCharacterChoice(GameSession session)
        {
            var rect = new Rect((Screen.width - 420f) / 2f, (Screen.height - 210f) / 2f, 420f, 210f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 20f, 380f, 42f), "플레이어 캐릭터 선택", titleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 66f, 380f, 38f), "외형만 다르며 능력 차이는 없습니다.", bodyStyle);
            if (GUI.Button(new Rect(rect.x + 35f, rect.y + 120f, 160f, 55f), "남성형 (M)"))
            {
                session.ChooseCharacter(CharacterChoice.Male);
            }

            if (GUI.Button(new Rect(rect.x + 225f, rect.y + 120f, 160f, 55f), "여성형 (F)"))
            {
                session.ChooseCharacter(CharacterChoice.Female);
            }
        }

        private void DrawPhone(GameSession session)
        {
            var rect = new Rect((Screen.width - 360f) / 2f, 130f, 360f, 330f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 16f, 320f, 40f), "농장 휴대폰", titleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 62f, 320f, 88f),
                $"퀘스트: 밀 수확 {Mathf.Min(session.Data.wheatHarvested, 3)}/3\n첫 수확: {(session.Data.firstHarvestAchievement ? "달성" : "미달성")}\n첫 달걀: {(session.Data.firstEggAchievement ? "달성" : "미달성")}", bodyStyle);

            if (GUI.Button(new Rect(rect.x + 30f, rect.y + 170f, 140f, 48f), "저장"))
            {
                session.Save(false);
            }

            if (GUI.Button(new Rect(rect.x + 190f, rect.y + 170f, 140f, 48f), "불러오기"))
            {
                session.Load();
            }

            if (GUI.Button(new Rect(rect.x + 110f, rect.y + 242f, 140f, 48f), "닫기 (P)"))
            {
                session.TogglePhone();
            }
        }

        private void DrawShop(GameSession session)
        {
            var rect = new Rect((Screen.width - 390f) / 2f, 150f, 390f, 270f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 18f, 350f, 42f), "씨앗·사료 상점", titleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 62f, 350f, 32f), $"보유 금액: {session.Data.money}G", bodyStyle);
            if (GUI.Button(new Rect(rect.x + 35f, rect.y + 110f, 320f, 45f), "밀 씨앗 5개 — 20G"))
            {
                session.PurchaseSeeds();
            }

            if (GUI.Button(new Rect(rect.x + 35f, rect.y + 165f, 320f, 45f), "닭 사료 3개 — 15G"))
            {
                session.PurchaseFeed();
            }

            if (GUI.Button(new Rect(rect.x + 135f, rect.y + 220f, 120f, 34f), "닫기"))
            {
                session.ToggleShop();
            }
        }

        private void EnsureStyles()
        {
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                font = koreanFont,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            titleStyle ??= new GUIStyle(bodyStyle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.skin.font = koreanFont;
        }
    }
}
