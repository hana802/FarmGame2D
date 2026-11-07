using UnityEngine;

namespace FarmGame2D.UI
{
    public readonly struct HudViewport
    {
        public HudViewport(float scale, float offsetX, float offsetY)
        {
            Scale = scale;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public float Scale { get; }
        public float OffsetX { get; }
        public float OffsetY { get; }
    }

    public readonly struct DialogueHudLayout
    {
        public DialogueHudLayout(Rect panel, Rect portrait, Rect name, Rect body)
        {
            Panel = panel;
            Portrait = portrait;
            Name = name;
            Body = body;
        }

        public Rect Panel { get; }
        public Rect Portrait { get; }
        public Rect Name { get; }
        public Rect Body { get; }
    }

    public static class HudLayout
    {
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;
        private const float DialogueAtlasWidth = 1618f;
        private const float DialogueAtlasHeight = 356f;

        public static HudViewport CalculateViewport(float screenWidth, float screenHeight)
        {
            var scale = Mathf.Min(screenWidth / ReferenceWidth, screenHeight / ReferenceHeight);
            var offsetX = (screenWidth - (ReferenceWidth * scale)) * 0.5f;
            var offsetY = (screenHeight - (ReferenceHeight * scale)) * 0.5f;
            return new HudViewport(scale, offsetX, offsetY);
        }

        public static DialogueHudLayout CalculateDialogueLayout()
        {
            const float panelWidth = 1100f;
            var panelHeight = panelWidth * DialogueAtlasHeight / DialogueAtlasWidth;
            var panel = new Rect((ReferenceWidth - panelWidth) * 0.5f, 640f - panelHeight, panelWidth, panelHeight);
            return new DialogueHudLayout(
                panel,
                ScaleDialogueSlot(panel, new Rect(80f, 107f, 201f, 193f)),
                ScaleDialogueSlot(panel, new Rect(362f, 27f, 335f, 50f)),
                ScaleDialogueSlot(panel, new Rect(355f, 118f, 1170f, 165f)));
        }

        private static Rect ScaleDialogueSlot(Rect panel, Rect atlasSlot)
        {
            var scale = panel.width / DialogueAtlasWidth;
            return new Rect(
                panel.x + (atlasSlot.x * scale),
                panel.y + (atlasSlot.y * scale),
                atlasSlot.width * scale,
                atlasSlot.height * scale);
        }
    }
}
