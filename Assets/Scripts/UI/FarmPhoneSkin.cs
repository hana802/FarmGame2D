using UnityEngine;

namespace FarmGame2D.UI
{
    internal enum PhoneButtonTone
    {
        Paper,
        Gold,
        Teal,
        Coral
    }

    internal sealed class FarmPhoneSkin
    {
        private static readonly Color32 Cocoa = new(79, 44, 32, 255);
        private static readonly Color32 Cream = new(255, 246, 218, 255);
        private static readonly Color32 Gold = new(255, 190, 65, 255);
        private static readonly Color32 Cover = new(53, 83, 65, 255);
        private readonly GUIStyle button;
        private readonly GUIStyle closeButton;
        private readonly GUIStyle sliderThumb = new() { fixedWidth = 26f, fixedHeight = 26f };

        public FarmPhoneSkin(Font font)
        {
            button = new GUIStyle(GUIStyle.none)
            {
                font = font,
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                padding = new RectOffset(8, 8, 2, 3)
            };
            closeButton = new GUIStyle(button) { fontSize = 34 };
            SetTextColor(closeButton, Color.white);
        }

        public void DrawNotebook(Rect cover, Rect paper)
        {
            if (Event.current.type != EventType.Repaint) return;
            Rounded(new Rect(cover.x, cover.y + 10f, cover.width, cover.height), 31f, new Color(0.12f, 0.09f, 0.05f, 0.54f));
            Rounded(cover, 31f, new Color32(42, 57, 45, 255));
            Rounded(Inset(cover, 4f), 27f, Cover);
            Rounded(new Rect(cover.x + 13f, cover.y + 27f, 19f, cover.height - 54f),
                9f, new Color32(45, 68, 54, 255));
            Rounded(new Rect(paper.x, paper.y + 8f, paper.width, paper.height), 24f, new Color32(179, 149, 103, 255));
            Rounded(new Rect(paper.x, paper.y + 4f, paper.width, paper.height), 24f, new Color32(224, 195, 141, 255));
            Rounded(paper, 24f, new Color32(248, 237, 203, 255));

            var grid = Inset(paper, 18f);
            var gridColor = new Color(0.58f, 0.43f, 0.28f, 0.11f);
            for (var x = grid.xMin; x < grid.xMax; x += 24f)
                FarmGuiSkin.DrawSolid(new Rect(x, grid.y, 1f, grid.height), gridColor);
            for (var y = grid.yMin; y < grid.yMax; y += 24f)
                FarmGuiSkin.DrawSolid(new Rect(grid.x, y, grid.width, 1f), gridColor);

            for (var y = paper.y + 87f; y < paper.yMax - 50f; y += 119f)
            {
                Rounded(new Rect(paper.x + 11f, y - 3f, 14f, 24f), 7f, new Color32(123, 94, 59, 255));
                DrawRaised(new Rect(cover.x + 9f, y, paper.x - cover.x + 14f, 15f),
                    new Color32(216, 172, 91, 255), new Color32(110, 78, 45, 255), 7.5f);
            }
        }

        public void DrawHeader(Rect rect, Rect title)
        {
            if (Event.current.type != EventType.Repaint) return;
            var line = new Color32(195, 157, 104, 255);
            var lineY = title.center.y;
            FarmGuiSkin.DrawSolid(new Rect(rect.x + 25f, lineY, title.xMin - rect.x - 41f, 2f), line);
            FarmGuiSkin.DrawSolid(new Rect(title.xMax + 16f, lineY, rect.xMax - title.xMax - 76f, 2f), line);
            DrawRaised(new Rect(title.x - 12f, title.y - 3f, title.width + 24f, title.height + 6f),
                new Color32(223, 150, 103, 255), new Color32(152, 89, 61, 255), 12f);
        }

        public void DrawPageSeam(Rect leftCard, Rect rightCard)
        {
            if (Event.current.type != EventType.Repaint) return;
            var center = (leftCard.xMax + rightCard.xMin) * 0.5f;
            for (var y = leftCard.y + 8f; y < leftCard.yMax - 5f; y += 13f)
                Rounded(new Rect(center - 2f, y, 4f, 4f), 2f, new Color32(191, 159, 108, 168));
        }

        public void DrawCard(Rect rect, bool warm = false)
        {
            Rounded(new Rect(rect.x, rect.y + 3f, rect.width, rect.height), 17f, new Color(0.47f, 0.27f, 0.13f, 0.16f));
            Rounded(rect, 17f, new Color32(220, 182, 130, 255));
            Rounded(Inset(rect, 2f), 15f, warm ? new Color32(255, 238, 199, 255) : Cream);
        }

        public bool DrawButton(Rect rect, string title, PhoneButtonTone tone)
        {
            var (fill, edge) = tone switch
            {
                PhoneButtonTone.Gold => (Gold, new Color32(173, 98, 37, 255)),
                PhoneButtonTone.Teal => (new Color32(36, 145, 145, 255), new Color32(25, 92, 92, 255)),
                PhoneButtonTone.Coral => (new Color32(220, 85, 60, 255), new Color32(147, 61, 42, 255)),
                _ => (Cream, new Color32(157, 110, 68, 255))
            };
            if (rect.Contains(Event.current.mousePosition) && GUI.enabled)
                fill = Color32.Lerp(fill, Color.white, 0.16f);

            DrawRaised(rect, fill, edge, 12f);
            var content = new GUIContent(title);
            for (button.fontSize = 19; button.fontSize > 14 && button.CalcSize(content).x > rect.width - 12f;)
                button.fontSize--;
            SetTextColor(button, tone is PhoneButtonTone.Teal or PhoneButtonTone.Coral ? Color.white : Cocoa);
            return GUI.Button(rect, content, button);
        }

        public bool DrawClose(Rect rect)
        {
            var fill = rect.Contains(Event.current.mousePosition)
                ? new Color32(247, 117, 83, 255) : new Color32(224, 80, 56, 255);
            DrawRaised(rect, fill, new Color32(147, 56, 38, 255), rect.width * 0.5f);
            return GUI.Button(rect, "×", closeButton);
        }

        public void DrawField(Rect rect, bool focused)
        {
            Rounded(rect, 8f, focused ? new Color32(196, 109, 45, 255) : new Color32(208, 159, 112, 255));
            Rounded(Inset(rect, 2f), 6f, new Color32(255, 251, 236, 255));
        }

        public void DrawProgress(Rect rect, float fraction)
        {
            Rounded(rect, rect.height * 0.5f, new Color32(195, 160, 114, 255));
            if (fraction <= 0f) return;
            var fill = Inset(rect, 2f);
            fill.width *= Mathf.Clamp01(fraction);
            Rounded(fill, Mathf.Min(fill.height, fill.width) * 0.5f, new Color32(65, 158, 137, 255));
        }

        public float DrawVolumeSlider(Rect rect, float volume)
        {
            const float knobSize = 26f;
            var track = new Rect(rect.x + 9f, rect.center.y - 5f, rect.width - 18f, 10f);
            Rounded(track, 5f, new Color32(122, 77, 53, 255));
            if (volume > 0f)
            {
                track.width *= Mathf.Clamp01(volume);
                Rounded(track, Mathf.Min(track.width * 0.5f, 5f), Gold);
            }
            var knob = new Rect(rect.x + (rect.width - knobSize) * volume, rect.center.y - knobSize * 0.5f, knobSize, knobSize);
            DrawRaised(knob, Gold, new Color32(173, 98, 37, 255), 9f);
            return GUI.HorizontalSlider(rect, volume, 0f, 1f, GUIStyle.none, sliderThumb);
        }

        private static void DrawRaised(Rect rect, Color fill, Color edge, float radius)
        {
            if (Event.current.type != EventType.Repaint) return;
            Rounded(new Rect(rect.x, rect.y + 3f, rect.width, rect.height), radius, new Color(0.39f, 0.21f, 0.11f, 0.25f));
            Rounded(rect, radius, edge);
            Rounded(Inset(rect, 2f), Mathf.Max(0f, radius - 2f), fill);
            Rounded(new Rect(rect.x + radius * 0.5f + 3f, rect.y + 4f, rect.width - radius - 6f, 3f),
                1.5f, Color.Lerp(fill, Color.white, 0.40f));
        }

        private static void SetTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = color;
        }

        private static Rect Inset(Rect rect, float pad) =>
            new(rect.x + pad, rect.y + pad, rect.width - 2f * pad, rect.height - 2f * pad);

        private static void Rounded(Rect rect, float radius, Color color) =>
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
    }
}
