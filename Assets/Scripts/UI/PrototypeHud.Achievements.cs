using System.Linq;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private Vector2 achievementScroll;

        private static Rect GetPhoneTab(PhoneHudLayout layout, int index)
        {
            var first = layout.FarmTabButton;
            const float gap = 8f;
            var span = Mathf.Max(360f, layout.SettingsTabButton.xMax - first.x);
            var width = (span - 2f * gap) / 3f;
            return new Rect(first.x + index * (width + gap), first.y, width, first.height);
        }

        private void DrawAchievements(GameSession session, Rect area)
        {
            var entries = FarmAchievements.GetAll(session.Data).ToArray();
            var cream = new Color32(253, 243, 216, 255);
            var ink = new Color32(70, 50, 35, 255);
            FarmGuiSkin.DrawSolid(area, cream);
            GUI.Label(new Rect(area.x + 14f, area.y + 8f, area.width - 28f, 28f),
                $"달성 {entries.Count(entry => entry.Earned)} / {entries.Length}", bodyStyle);

            var viewport = new Rect(area.x + 12f, area.y + 42f, area.width - 24f, area.height - 50f);
            const float rowHeight = 53f;
            const float gap = 4f;
            var width = viewport.width - 17f;
            achievementScroll = GUI.BeginScrollView(viewport, achievementScroll,
                new Rect(0f, 0f, width, Mathf.Max(viewport.height, (rowHeight + gap) * entries.Length)));
            for (var index = 0; index < entries.Length; index++)
            {
                var entry = entries[index];
                var y = (rowHeight + gap) * index;
                var background = entry.Earned ? new Color32(229, 235, 202, 255) : new Color32(247, 231, 199, 255);
                FarmGuiSkin.DrawSolid(new Rect(0f, y, width, rowHeight), background);
                FarmGuiSkin.DrawSolid(new Rect(0f, y, 4f, rowHeight),
                    entry.Earned ? new Color32(62, 95, 63, 255) : new Color32(168, 136, 100, 255));
                GUI.Label(new Rect(11f, y + 2f, width - 97f, 23f), entry.Title, bodyStyle);
                GUI.Label(new Rect(11f, y + 25f, width - 23f, 24f), entry.Detail, smallStyle);
                var previous = GUI.contentColor;
                GUI.contentColor = ink;
                GUI.Label(new Rect(width - 77f, y + 2f, 74f, 23f), entry.Earned ? "달성" : "미달성", centeredStyle);
                GUI.contentColor = previous;
            }
            GUI.EndScrollView();
        }
    }
}
