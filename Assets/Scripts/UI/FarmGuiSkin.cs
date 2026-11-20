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
        private GUIStyle dialogueSpeakerStyle;
        private GUIStyle dialogueBodyStyle;

        public FarmGuiSkin(Font font)
        {
            atlas = GameArtCatalog.UiAtlas;
            this.font = font;
        }

        public bool IsReady => atlas != null;

        public void DrawDialogue(Rect rect) => DrawAtlas(rect, DialogueAtlasRect);

        public void DrawDialogue(DialogueHudLayout layout, string speaker, string line, int portraitRow)
        {
            dialogueSpeakerStyle ??= new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip, normal = { textColor = new Color32(91, 64, 43, 255) }
            };
            dialogueBodyStyle ??= new GUIStyle(dialogueSpeakerStyle)
            {
                fontSize = 19, fontStyle = FontStyle.Normal, alignment = TextAnchor.MiddleLeft,
                wordWrap = true, padding = new RectOffset(8, 8, 4, 4)
            };

            DrawAtlas(layout.Panel, DialogueAtlasRect);
            DrawSprite(layout.Portrait, GameArtCatalog.GetNpcPortrait(portraitRow));
            GUI.Label(layout.Name, speaker, dialogueSpeakerStyle);
            GUI.Label(layout.Body, line, dialogueBodyStyle);
        }

        public void DrawWindow(Rect rect) => DrawAtlas(rect, WindowAtlasRect);

        public void DrawBanner(Rect rect) => DrawAtlas(rect, BannerAtlasRect);

        public bool DrawButton(Rect rect, string label, bool orange = false)
        {
            DrawButtonBackground(rect, orange);
            return GUI.Button(rect, label, GetButtonStyle());
        }

        public bool DrawTextButton(Rect rect, string label)
        {
            return GUI.Button(rect, label, GetButtonStyle());
        }

        public void DrawButtonBackground(Rect rect, bool orange = false)
        {
            DrawAtlas(rect, orange ? OrangeButtonAtlasRect : GreenButtonAtlasRect);
        }

        public void DrawPixelPanel(Rect rect, Color fill, Color edge, bool shadow = true)
        {
            const float corner = 4f;
            if (shadow)
            {
                var offset = new Rect(rect.x, rect.y + 4f, rect.width, rect.height);
                DrawPixelShape(offset, corner, new Color(0.12f, 0.08f, 0.06f, 0.35f));
            }
            DrawPixelShape(rect, corner, edge);
            DrawPixelShape(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f),
                2f, fill);
        }

        private static void DrawPixelShape(Rect rect, float corner, Color color)
        {
            DrawSolid(new Rect(rect.x + corner, rect.y, rect.width - corner * 2f, corner), color);
            DrawSolid(new Rect(rect.x, rect.y + corner, rect.width, rect.height - corner * 2f), color);
            DrawSolid(new Rect(rect.x + corner, rect.yMax - corner, rect.width - corner * 2f, corner), color);
        }

        public void DrawMutedPrompt(Rect rect, Rect keycap)
        {
            DrawSolid(new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.18f));
            DrawSolid(rect, new Color(0.12f, 0.13f, 0.14f, 0.74f));
            DrawSolid(new Rect(rect.x, rect.y, rect.width, 1f), new Color(1f, 1f, 1f, 0.16f));
            DrawSolid(keycap, new Color(0.06f, 0.07f, 0.08f, 0.82f));
        }

        public void DrawSprite(Rect destination, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            var texture = sprite.texture;
            var source = sprite.rect;
            var uv = new Rect(
                source.x / texture.width,
                source.y / texture.height,
                source.width / texture.width,
                source.height / texture.height);
            GUI.DrawTextureWithTexCoords(destination, texture, uv, true);
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
                clipping = TextClipping.Clip,
                wordWrap = true,
                padding = new RectOffset(9, 9, 4, 4),
                normal = { background = null, textColor = new Color32(76, 45, 28, 255) },
                hover = { background = null, textColor = new Color32(116, 67, 35, 255) },
                active = { background = null, textColor = new Color32(63, 91, 47, 255) }
            };
            return buttonStyle;
        }

        public static void DrawSolid(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
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
