using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Art
{
    internal static class InventoryItemArt
    {
        private static readonly Dictionary<GameItem, Sprite> Sprites = new();

        public static Sprite GetSprite(GameItem item)
        {
            if (Sprites.TryGetValue(item, out var sprite)) return sprite;

            var (rows, middle, light, shine) = item switch
            {
                GameItem.Egg => (Egg, 0xb77b51, 0xecc28a, 0xffeed2),
                GameItem.Milk => (Milk, 0x6c8295, 0xc7dae1, 0xfff8dc),
                GameItem.Wood => (Wood, 0x995536, 0xc38145, 0xf6b972),
                GameItem.Stone => (Ore, 0x687c80, 0x98ada9, 0xd2d7c0),
                GameItem.CopperOre => (Ore, 0x965941, 0xc97a56, 0xffae75),
                GameItem.IronOre => (Ore, 0x596c79, 0x8ba6b6, 0xd2e7ee),
                GameItem.GoldOre => (Ore, 0xa47732, 0xd8a444, 0xffe284),
                GameItem.RiverFish => (Fish, 0x4e807b, 0x82ad83, 0xc1e4ac),
                GameItem.SeaBass => (Fish, 0x4f7e9c, 0x82b3ca, 0xc9e9ee),
                GameItem.GoldenCarp => (Fish, 0xb16b38, 0xeaa44c, 0xffe28e),
                GameItem.PotatoSoup => (Soup, 0x9c6241, 0xeca752, 0xffe2ad),
                GameItem.TomatoOmelet => (Omelet, 0xbb623c, 0xf4bc52, 0xfff0a6),
                GameItem.MarketBread => (Bread, 0x743e25, 0xbb753b, 0xf6c575),
                GameItem.HarborMackerel => (Fish, 0x334d62, 0x648697, 0xd0e2dc),
                GameItem.Turnip => (Turnip, 0xc4a07a, 0xf3e2c4, 0x6e9a45),
                _ => (null, 0, 0, 0)
            };
            if (rows == null) return null;

            var pixels = new Color32[rows[0].Length * rows.Length];
            var palette = new[] { Rgb(0x373536), Rgb(middle), Rgb(light), Rgb(shine) };
            for (var y = 0; y < rows.Length; y++)
            for (var x = 0; x < rows[y].Length; x++)
            {
                var shade = rows[y][x];
                if (shade != '.') pixels[(rows.Length - y - 1) * rows[0].Length + x] = palette[shade - '0'];
            }

            var texture = new Texture2D(rows[0].Length, rows.Length, TextureFormat.RGBA32, false)
            {
                name = item.ToString(), filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprites[item] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 16f);
        }

        private static Color32 Rgb(int rgb) => new((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        private static readonly string[] Egg =
        {
            "....000.....", "...03320....", "..0333220...", "..0332220...", ".033222210..",
            ".032222210..", ".022222110..", ".022211110..", "..0111110...", "...00000...."
        };
        private static readonly string[] Milk =
        {
            "...00000....", "...02110....", "...00000....", "...03220....", "..0332220...",
            "..0332220...", "..0211120...", "..0233120...", "..0233120...", "..0222210...",
            "..0111110...", "...00000...."
        };
        private static readonly string[] Wood =
        {
            ".....000000.", "...00222110.", ".0022212110.", "01112122110.", "0233102110..",
            "032321110...", "03232110....", "0233110.....", ".00000......"
        };
        private static readonly string[] Ore =
        {
            ".....000....", "...003320...", "..03222210..", ".0322232110.", ".0222322210.",
            "022112221110", "021122211110", ".0111111110.", "..00000000.."
        };
        private static readonly string[] Bread =
        {
            "...0000...",
            "..022220..",
            ".0223220..",
            "023322320.",
            "0232232200",
            "0112222110",
            ".00000000."
        };

        private static readonly string[] Fish =
        {
            ".....00.....", "....0210...0", "..0032220.00", ".03222021010", "032223221210",
            "022222221210", ".01222111010", "..0011100.00", ".....010...0", "......0....."
        };
        private static readonly string[] Soup =
        {
            "..3...3.....", "...3...3....", "..3...3.....", "..00000000..", ".0223332220.",
            "032222222230", "021111111120", ".0222222220.", "..01111110..", "...000000..."
        };
        private static readonly string[] Turnip =
        {
            "....3333....", "...332233...", "....3333....", "...022220...", "..02222220..",
            "..02111220..", "..02222220..", "...022220...", "....0000...."
        };
        private static readonly string[] Omelet =
        {
            "............", "....0000....", "..00333200..", ".0333222220.", "032322121220",
            "022221221220", ".0111222110.", "..00111100..", "....0000....", "............"
        };
    }
}
