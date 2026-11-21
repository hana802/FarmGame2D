using UnityEngine;

namespace FarmGame2D.Interactions
{
    public static class FishingPresentation
    {
        public const float WaitingSeconds = 1.15f;
        public const float BiteSeconds = 0.4f;

        public static float TotalSeconds => WaitingSeconds + BiteSeconds;

        private static Sprite bobberSprite;
        private static Sprite rippleSprite;

        public static Sprite BobberSprite => bobberSprite ??= CreateBobberSprite();
        public static Sprite RippleSprite => rippleSprite ??= CreateRippleSprite();

        private static Sprite CreateBobberSprite()
        {
            const int width = 16;
            const int height = 22;
            var pixels = new Color32[width * height];
            var outline = new Color32(62, 36, 32, 255);
            var red = new Color32(214, 54, 48, 255);
            var redDark = new Color32(158, 36, 38, 255);
            var white = new Color32(255, 244, 230, 255);
            var whiteShade = new Color32(214, 186, 170, 255);
            var stick = new Color32(92, 64, 48, 255);
            FillCircle(pixels, width, height, 8f, 14f, 6.2f, outline);
            FillCircle(pixels, width, height, 8f, 14.4f, 5f, white);
            FillRect(pixels, width, 4, 15, 11, 20, red);
            FillRect(pixels, width, 5, 16, 10, 19, redDark);
            FillRect(pixels, width, 6, 17, 8, 19, white);
            FillRect(pixels, width, 7, 6, 8, 14, stick);
            FillRect(pixels, width, 6, 4, 9, 7, outline);
            return CreateSprite("Fishing Bobber", pixels, width, height, 34f, new Vector2(0.5f, 0.28f));
        }

        private static Sprite CreateRippleSprite()
        {
            const int width = 32;
            const int height = 14;
            var pixels = new Color32[width * height];
            var centerX = (width - 1) * 0.5f;
            var centerY = (height - 1) * 0.5f;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var nx = (x - centerX) / centerX;
                    var ny = (y - centerY) / centerY;
                    var distance = Mathf.Sqrt((nx * nx) + (ny * ny));
                    if (distance < 0.68f || distance > 1f)
                    {
                        continue;
                    }

                    var edge = Mathf.InverseLerp(1f, 0.68f, distance);
                    pixels[(y * width) + x] = new Color32(236, 246, 255, (byte)Mathf.Lerp(90f, 210f, edge));
                }
            }

            return CreateSprite("Fishing Ripple", pixels, width, height, 32f, new Vector2(0.5f, 0.5f));
        }

        private static void FillCircle(Color32[] pixels, int width, int height, float centerX, float centerY, float radius, Color32 color)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    if ((dx * dx) + (dy * dy) <= radius * radius)
                    {
                        pixels[(y * width) + x] = color;
                    }
                }
            }
        }

        private static void FillRect(Color32[] pixels, int width, int left, int bottom, int right, int top, Color32 color)
        {
            for (var y = bottom; y <= top; y++)
            {
                for (var x = left; x <= right; x++)
                {
                    pixels[(y * width) + x] = color;
                }
            }
        }

        private static Sprite CreateSprite(string name, Color32[] pixels, int width, int height, float pixelsPerUnit, Vector2 pivot)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }
    }
}
