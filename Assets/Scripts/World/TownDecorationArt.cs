using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public static class TownDecorationArt
    {
        private static readonly Dictionary<GameItem, Sprite> Sprites = new();
        private static readonly Color32 Outline = new(65, 39, 36, 255);
        private static readonly Color32 Wood = new(154, 86, 49, 255);
        private static readonly Color32 WoodLight = new(218, 146, 78, 255);
        private static readonly Color32 Leaf = new(47, 125, 62, 255);
        private static readonly Color32 Blossom = new(245, 120, 142, 255);

        public static Sprite GetSprite(GameItem item)
        {
            if (Sprites.TryGetValue(item, out var sprite)) return sprite;
            sprite = item switch
            {
                GameItem.FlowerPot => CreateFlowerPot(),
                GameItem.StreetLamp => CreateStreetLamp(),
                GameItem.TownBench => CreateBench(),
                _ => null
            };
            if (sprite != null) Sprites[item] = sprite;
            return sprite;
        }

        private static Sprite CreateFlowerPot()
        {
            var pixels = Blank(18, 22);
            Fill(pixels, 4, 1, 13, 10, Outline);
            Fill(pixels, 5, 3, 12, 9, Wood);
            Fill(pixels, 3, 9, 14, 11, WoodLight);
            Fill(pixels, 2, 11, 15, 12, Outline);
            Fill(pixels, 8, 11, 9, 17, Leaf);
            Fill(pixels, 5, 14, 12, 17, Leaf);
            Fill(pixels, 3, 16, 6, 19, Blossom);
            Fill(pixels, 10, 17, 14, 20, new Color32(255, 212, 116, 255));
            return Create(GameItem.FlowerPot, pixels);
        }

        private static Sprite CreateStreetLamp()
        {
            var pixels = Blank(16, 32);
            Fill(pixels, 4, 1, 11, 3, Outline);
            Fill(pixels, 6, 3, 9, 24, Outline);
            Fill(pixels, 7, 4, 8, 23, Wood);
            Fill(pixels, 3, 23, 12, 30, Outline);
            Fill(pixels, 4, 24, 11, 28, new Color32(255, 210, 102, 255));
            Fill(pixels, 5, 25, 10, 27, new Color32(255, 243, 178, 255));
            Fill(pixels, 2, 29, 13, 30, WoodLight);
            return Create(GameItem.StreetLamp, pixels);
        }

        private static Sprite CreateBench()
        {
            var pixels = Blank(30, 19);
            Fill(pixels, 2, 7, 27, 15, Outline);
            Fill(pixels, 3, 8, 26, 10, WoodLight);
            Fill(pixels, 3, 12, 26, 14, Wood);
            Fill(pixels, 4, 4, 25, 7, Outline);
            Fill(pixels, 5, 5, 24, 6, WoodLight);
            Fill(pixels, 5, 1, 8, 5, Outline);
            Fill(pixels, 21, 1, 24, 5, Outline);
            return Create(GameItem.TownBench, pixels);
        }

        private static Color32[,] Blank(int width, int height) => new Color32[width, height];

        private static void Fill(Color32[,] pixels, int left, int bottom, int right, int top, Color32 color)
        {
            for (var y = bottom; y <= top; y++)
            for (var x = left; x <= right; x++) pixels[x, y] = color;
        }

        private static Sprite Create(GameItem item, Color32[,] pixels)
        {
            var width = pixels.GetLength(0);
            var height = pixels.GetLength(1);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Town Decoration {item}"
            };
            var flat = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) flat[y * width + x] = pixels[x, y];
            texture.SetPixels32(flat);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.08f), 22f);
        }
    }
}
