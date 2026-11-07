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
    }
}
