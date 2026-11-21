using System.Linq;
using UnityEngine;

namespace FarmGame2D.Art
{
    internal static class FarmBuildingArt
    {
        private static Sprite porch, chimney, glassWing, roofVent;

        public static Sprite Porch => porch ??= HouseDetail("Farmhouse masonry landing", new Rect(618f, 115f, 300f, 84f));

        public static Sprite Chimney => chimney ??= HouseDetail("Farmhouse kitchen chimney", new Rect(884f, 685f, 120f, 143f));

        private static Sprite HouseDetail(string name, Rect rect)
        {
            var house = WorldArtCatalog.GetSprite(WorldArtKind.FarmHouse);
            var sprite = Sprite.Create(house.texture, rect, new Vector2(0.5f, 0f),
                house.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        public static Sprite GlassWing => glassWing ??= Make("Greenhouse glass wing", 10f, new[]
        {
            ".......gggg.....", ".....ggbbhbgg...", "...ggbgbhhgbbgg.", ".ggbbbgbhhgbbbgg",
            "gggggggggggggggg", "gbbhgbbhgbbhgbbg", "gbhbgbhbgbhbgbhg", "ghbbghbbghbbghbg",
            "gbbhgbbhgbbhgbbg", "gggggggggggggggg", "ssMssssMssssMsss", "mmmmmmmmmmmmmmmm"
        });

        public static Sprite RoofVent => roofVent ??= Make("Greenhouse roof vent", 12f, new[]
        {
            "..gggggggggggg..", ".gbhbbghhgbhbbg.", "gbhbbbghhgbhbbbg", "gggggggggggggggg",
            "gmssssssssssssmg", "gggggggggggggggg"
        });

        private static Sprite Make(string name, float pixelsPerUnit, string[] rows)
        {
            var width = rows.Max(row => row.Length);
            var texture = new Texture2D(width, rows.Length, TextureFormat.RGBA32, false)
            {
                name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[width * rows.Length];
            for (var y = 0; y < rows.Length; y++)
            for (var x = 0; x < rows[y].Length; x++)
                pixels[(rows.Length - 1 - y) * width + x] = Pigment(rows[y][x]);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, rows.Length), new Vector2(0.5f, 0.08f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Color32 Pigment(char key) => key switch
        {
            'g' => new(49, 96, 72, 255), 'b' => new(147, 196, 185, 255), 'h' => new(222, 235, 201, 255),
            'm' => new(54, 54, 55, 255), 's' => new(112, 114, 103, 255),
            'M' => new(166, 156, 135, 255), _ => new(0, 0, 0, 0)
        };
    }
}
