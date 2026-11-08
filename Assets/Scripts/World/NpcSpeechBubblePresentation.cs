using UnityEngine;

namespace FarmGame2D.World
{
    public static class NpcSpeechBubblePresentation
    {
        private const int FrameSize = 12;
        private static readonly Color PaperColor = new(0.94f, 0.84f, 0.62f, 0.98f);
        private static readonly Color InnerBorderColor = new(0.58f, 0.36f, 0.20f, 1f);
        private static readonly Color OuterBorderColor = new(0.28f, 0.15f, 0.09f, 1f);
        private static Texture2D frameTexture;
        private static Texture2D tailTexture;

        public static bool ShouldDraw(string bubbleText, bool modalOpen)
        {
            return !modalOpen && !string.IsNullOrEmpty(bubbleText);
        }

        public static GUIStyle CreateStyle(GUIStyle baseStyle, Font font)
        {
            return new GUIStyle(baseStyle)
            {
                font = font,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(10, 10, 5, 6),
                normal =
                {
                    background = GetFrameTexture(),
                    textColor = new Color(0.22f, 0.12f, 0.07f)
                }
            };
        }

        public static string GetSpokenLine(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var separator = source.IndexOf(": ", System.StringComparison.Ordinal);
            return separator >= 0 && separator + 2 < source.Length
                ? source.Substring(separator + 2)
                : source;
        }

        public static float CalculateHeight(GUIStyle style, string spokenLine, float width)
        {
            if (style == null)
            {
                return 38f;
            }

            return Mathf.Clamp(style.CalcHeight(new GUIContent(spokenLine), width), 36f, 58f);
        }

        public static Rect KeepOnScreen(Rect rect, float screenWidth, float screenHeight)
        {
            const float margin = 8f;
            rect.x = Mathf.Clamp(rect.x, margin, Mathf.Max(margin, screenWidth - rect.width - margin));
            rect.y = Mathf.Clamp(rect.y, margin, Mathf.Max(margin, screenHeight - rect.height - margin));
            return rect;
        }

        public static void Draw(Rect rect, string spokenLine, GUIStyle style)
        {
            var shadow = new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height);
            var previousColor = GUI.color;
            GUI.color = new Color(0.18f, 0.10f, 0.05f, 0.28f);
            GUI.Box(shadow, GUIContent.none, style);
            GUI.color = previousColor;

            var tail = new Rect(rect.center.x - 5f, rect.yMax - 1f, 10f, 8f);
            GUI.DrawTexture(tail, GetTailTexture(), ScaleMode.StretchToFill, true);
            GUI.Box(rect, spokenLine, style);
        }

        private static Texture2D GetFrameTexture()
        {
            if (frameTexture != null)
            {
                return frameTexture;
            }

            frameTexture = CreatePixelTexture(FrameSize, FrameSize, "npc_speech_frame");
            for (var y = 0; y < FrameSize; y++)
            {
                for (var x = 0; x < FrameSize; x++)
                {
                    var cornerCutout = (x < 2 || x >= FrameSize - 2) && (y < 2 || y >= FrameSize - 2);
                    var outerEdge = x == 0 || y == 0 || x == FrameSize - 1 || y == FrameSize - 1
                        || ((x == 1 || x == FrameSize - 2) && (y >= 2 && y < FrameSize - 2))
                        || ((y == 1 || y == FrameSize - 2) && (x >= 2 && x < FrameSize - 2));
                    var innerEdge = x == 2 || y == 2 || x == FrameSize - 3 || y == FrameSize - 3;
                    var color = cornerCutout
                        ? Color.clear
                        : outerEdge
                            ? OuterBorderColor
                            : innerEdge ? InnerBorderColor : PaperColor;
                    frameTexture.SetPixel(x, y, color);
                }
            }

            frameTexture.Apply(false, true);
            return frameTexture;
        }

        private static Texture2D GetTailTexture()
        {
            if (tailTexture != null)
            {
                return tailTexture;
            }

            tailTexture = CreatePixelTexture(10, 8, "npc_speech_tail");
            for (var y = 0; y < tailTexture.height; y++)
            {
                var halfWidth = Mathf.Min(4, y / 2 + 1);
                var left = 4 - halfWidth;
                var right = 5 + halfWidth;
                for (var x = 0; x < tailTexture.width; x++)
                {
                    var inside = x >= left && x <= right;
                    var edge = x == left || x == right || y == 0;
                    tailTexture.SetPixel(x, y, !inside ? Color.clear : edge ? OuterBorderColor : PaperColor);
                }
            }

            tailTexture.Apply(false, true);
            return tailTexture;
        }

        private static Texture2D CreatePixelTexture(int width, int height, string name)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            return texture;
        }
    }
}
