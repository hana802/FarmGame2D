using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    public sealed partial class NpcInteractionOverlay
    {
        private bool choosingFarmHelp;
        private string farmHelpNotice;

        private void OpenFarmHelp()
        {
            choosingFarmHelp = true;
            farmHelpNotice = string.Empty;
        }

        private void DrawFarmHelpMenu(Rect panel, GameSession session)
        {
            GUI.Label(new Rect(panel.x + 24f, panel.y + 22f, panel.width - 48f, 48f), "농장일 부탁하기", titleStyle);

            var restriction = session.GetSpouseFarmHelpRestriction(npcId);
            var notice = !string.IsNullOrEmpty(farmHelpNotice) ? farmHelpNotice : restriction;
            if (string.IsNullOrEmpty(notice)) notice = "아침에 하루 한 번 농장일을 부탁할 수 있어요.";
            GUI.Label(new Rect(panel.x + 25f, panel.y + 72f, panel.width - 50f, 66f), notice, centeredStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && restriction.Length == 0;
            DrawFarmHelpButton(panel, 143f, "물 주기 · 최대 3칸", session, SpouseFarmChore.WaterCrops);
            DrawFarmHelpButton(panel, 203f, "먹이 주기 · 보유 사료 사용", session, SpouseFarmChore.FeedAnimals);
            DrawFarmHelpButton(panel, 263f, "잡초 뽑기 · 최대 3개", session, SpouseFarmChore.PullWeeds);
            GUI.enabled = previousEnabled;

            if (GUI.Button(new Rect(panel.center.x - 100f, panel.y + 346f, 200f, 50f), "뒤로", buttonStyle))
                choosingFarmHelp = false;
        }

        private void DrawFarmHelpButton(Rect panel, float top, string label, GameSession session, SpouseFarmChore chore)
        {
            if (GUI.Button(new Rect(panel.x + 50f, panel.y + top, panel.width - 100f, 50f), label, buttonStyle))
                session.TryRequestSpouseFarmHelp(npcId, chore, out farmHelpNotice);
        }
    }
}
