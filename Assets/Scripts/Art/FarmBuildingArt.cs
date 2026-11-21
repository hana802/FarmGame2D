using System.Linq;
using UnityEngine;

namespace FarmGame2D.Art
{
    internal static class FarmBuildingArt
    {
        private static Sprite porch, chimney, glassWing, roofVent, stove;

        public static Sprite Porch => porch ??= Make("Timber porch", 9f, new[]
        {
            "......rrr.......", "....rrRRRrr.....", "..rrRRRRRRRrr...",
            ".rRRrrRRrrRRRr..", "rrrrrrrrrrrrrrrr", "wWw..........wWw",
            "wWw..........wWw", "wWd..........dWw", "wWd..........dWw",
            "wWd..........dWw", "wWw..........wWw", "dddddddddddddddd", ".WWWwWWWWwWWWW.",
            "..wwwwwwwwwwww.."
        });

        public static Sprite Chimney => chimney ??= Make("Stone chimney", 11f, new[]
        {
            "........b..", "......bb...", ".....bb....", "......b....", "...bbbbbb..",
            "..mmmmmmmm.", "..msMmmMsm.", "...sMmmmM..", "...mmsMmm..", "...smMmmm..",
            "...msMmsM..", "..mmmmmmmm."
        });

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

        public static Sprite Stove => stove ??= Make("Cast iron stove", 15f, new[]
        {
            "..........mmm...", "..........msm...", "..........msm...", ".mmmmmmmmmmmmmm.",
            "mssmmssmmssmmssm", "mmmmmmmmmmmmmmmm", "mMsm........msMm",
            "mMsm..hhHH..msMm", "mMsm.HhRRhH.msMm", "mMsm..HhhH..msMm",
            "mMsm........msMm", "mmmmmmmmmmmmmmmm", "..mm........mm.."
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
            'R' => new(196, 98, 65, 255), 'r' => new(121, 56, 41, 255),
            'W' => new(218, 154, 92, 255), 'w' => new(145, 87, 55, 255), 'd' => new(75, 51, 46, 255),
            'g' => new(49, 96, 72, 255), 'b' => new(147, 196, 185, 255), 'h' => new(222, 235, 201, 255),
            'H' => new(232, 152, 62, 255), 'm' => new(54, 54, 55, 255), 's' => new(112, 114, 103, 255),
            'M' => new(166, 156, 135, 255), _ => new(0, 0, 0, 0)
        };
    }
}
