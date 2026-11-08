using UnityEngine;

namespace FarmGame2D.World
{
    public static class NpcSpeechBubblePresentation
    {
        public static GUIStyle CreateStyle(GUIStyle baseStyle, Font font)
        {
            return new GUIStyle(baseStyle)
            {
                font = font,
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                padding = new RectOffset(8, 8, 4, 4),
                normal =
                {
                    background = Texture2D.whiteTexture,
                    textColor = new Color(0.2f, 0.12f, 0.08f)
                }
            };
        }
    }
}
