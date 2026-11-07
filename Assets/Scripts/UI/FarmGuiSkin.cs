using FarmGame2D.Art;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed class FarmGuiSkin
    {
        private static readonly Rect DialogueAtlasRect = new(48f, 28f, 1618f, 356f);
        private static readonly Rect WindowAtlasRect = new(42f, 424f, 700f, 470f);
        private static readonly Rect BannerAtlasRect = new(778f, 440f, 570f, 180f);
        private static readonly Rect GreenButtonAtlasRect = new(770f, 650f, 280f, 82f);
        private static readonly Rect OrangeButtonAtlasRect = new(1058f, 650f, 290f, 82f);

        private readonly Texture2D atlas;
        private readonly Font font;
        private GUIStyle buttonStyle;

        public FarmGuiSkin(Font font)
        {
            atlas = GameArtCatalog.UiAtlas;
            this.font = font;
        }

        public bool IsReady => atlas != null;

        public void DrawDialogue(Rect rect) => DrawAtlas(rect, DialogueAtlasRect);

        public void DrawWindow(Rect rect) => DrawAtlas(rect, WindowAtlasRect);

        public void DrawBanner(Rect rect) => DrawAtlas(rect, BannerAtlasRect);

        public bool DrawButton(Rect rect, string label, bool orange = false)
        {
            DrawButtonBackground(rect, orange);
            return GUI.Button(rect, label, GetButtonStyle());
        }

        public void DrawButtonBackground(Rect rect, bool orange = false)
        {
            DrawAtlas(rect, orange ? OrangeButtonAtlasRect : GreenButtonAtlasRect);
        }

        private GUIStyle GetButtonStyle()
        {
            if (buttonStyle != null)
            {
                return buttonStyle;
            }

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                font = this.font,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = null, textColor = new Color32(76, 45, 28, 255) },
                hover = { background = null, textColor = new Color32(116, 67, 35, 255) },
                active = { background = null, textColor = new Color32(63, 91, 47, 255) }
            };
            return buttonStyle;
        }

        private void DrawAtlas(Rect destination, Rect pixelRectFromTopLeft)
        {
            if (atlas == null)
            {
                GUI.Box(destination, GUIContent.none);
                return;
            }

            var uv = new Rect(
                pixelRectFromTopLeft.x / atlas.width,
                1f - ((pixelRectFromTopLeft.y + pixelRectFromTopLeft.height) / atlas.height),
                pixelRectFromTopLeft.width / atlas.width,
                pixelRectFromTopLeft.height / atlas.height);
            GUI.DrawTextureWithTexCoords(destination, atlas, uv, true);
        }
    }
}
