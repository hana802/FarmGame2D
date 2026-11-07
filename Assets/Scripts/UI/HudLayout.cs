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

        public static HudViewport CalculateViewport(float screenWidth, float screenHeight)
        {
            var scale = Mathf.Min(screenWidth / ReferenceWidth, screenHeight / ReferenceHeight);
            var offsetX = (screenWidth - (ReferenceWidth * scale)) * 0.5f;
            var offsetY = (screenHeight - (ReferenceHeight * scale)) * 0.5f;
            return new HudViewport(scale, offsetX, offsetY);
        }

        public static DialogueHudLayout CalculateDialogueLayout()
        {
            var panel = new Rect(112f, 482f, 1056f, 158f);
            return new DialogueHudLayout(
                panel,
                new Rect(panel.x + 30f, panel.y + 22f, 116f, 116f),
                new Rect(panel.x + 168f, panel.y + 16f, 260f, 38f),
                new Rect(panel.x + 168f, panel.y + 58f, 840f, 76f));
        }
    }
}
