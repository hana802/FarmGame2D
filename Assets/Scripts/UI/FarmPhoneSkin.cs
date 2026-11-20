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

        public void DrawDevice(Rect device, Rect screen)
        {
            if (Event.current.type != EventType.Repaint) return;
            Rounded(new Rect(device.x + 145f, device.y - 4f, 55f, 16f), 6f, Cocoa);
            Rounded(new Rect(device.x + 211f, device.y - 4f, 36f, 16f), 6f, Cocoa);
            Rounded(new Rect(device.x, device.y + 9f, device.width, device.height), 51f, new Color(0.13f, 0.06f, 0.04f, 0.54f));
            Rounded(device, 51f, Cocoa);
            Rounded(Inset(device, 3f), 48f, new Color32(179, 101, 67, 255));
            Rounded(Inset(device, 7f), 44f, new Color32(210, 133, 84, 255));
            Rounded(new Rect(device.x + 48f, device.y + 9f, 168f, 5f), 2.5f, new Color32(241, 172, 115, 255));
            Rounded(Inset(screen, -3f), 32f, Cocoa);
            Rounded(screen, 29f, new Color32(247, 229, 180, 255));
            Rounded(Inset(screen, 3f), 26f, new Color32(255, 243, 206, 255));

            var grid = new Rect(screen.x + 18f, screen.y + 21f, screen.width - 36f, screen.height - 42f);
            var gridColor = new Color(0.67f, 0.43f, 0.24f, 0.12f);
            for (var x = grid.xMin; x < grid.xMax; x += 22f)
                FarmGuiSkin.DrawSolid(new Rect(x, grid.y, 1f, grid.height), gridColor);
            for (var y = grid.yMin; y < grid.yMax; y += 22f)
                FarmGuiSkin.DrawSolid(new Rect(grid.x, y, grid.width, 1f), gridColor);

            Rounded(new Rect(device.x + 12f, device.center.y - 22f, 8f, 45f), 4f, Cocoa);
            Rounded(new Rect(device.x + 12f, device.center.y + 33f, 8f, 8f), 4f, new Color32(67, 79, 78, 255));
            Rounded(new Rect(screen.center.x - 47f, screen.yMax - 10f, 94f, 4f), 2f, new Color32(162, 117, 77, 255));
        }

        public void DrawHeader(Rect rect)
        {
            Rounded(rect, 14f, new Color32(227, 151, 77, 255));
            Rounded(Inset(rect, 2f), 12f, new Color32(255, 207, 121, 255));
            Rounded(new Rect(rect.x + 14f, rect.y + 4f, rect.width - 28f, 3f), 1.5f, new Color32(255, 229, 166, 255));
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
