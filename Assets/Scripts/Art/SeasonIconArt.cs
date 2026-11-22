using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Art
{
    internal static class SeasonIconArt
    {
        private static Texture2D spring, summer, autumn, winter;

        public static Texture2D Get(Season season) => season switch
        {
            Season.Spring => spring ??= CreateSpring(),
            Season.Summer => summer ??= CreateSummer(),
            Season.Autumn => autumn ??= CreateAutumn(),
            _ => winter ??= CreateWinter()
        };

        private static Texture2D CreateSpring() => Create("Spring blossom", new[]
        {
            ".....oo.oo.....",
            "....oxxoxxo....",
            "....ox+xxxo....",
            "..oooxx+xxooo..",
            ".oxxxxoxxxxxxo.",
            "oxx+xoao+xx+xxo",
            "oxxxoaaao+xxxxo",
            ".oxxoaaao+xxxo.",
            "..ooxoaooxxoo..",
            "...oxxoxxxo....",
            "...ox+xxxxxo...",
            "....oxxxoxxo...",
            ".....ooo.oo....",
            "...............",
            "..............."
        }, new Color32(127, 54, 73, 255), new Color32(226, 98, 132, 255),
            new Color32(255, 190, 198, 255), new Color32(254, 226, 132, 255));

        private static Texture2D CreateSummer() => Create("Summer sun", new[]
        {
            ".......o.......",
            "...o...x...o...",
            "...x.......x...",
            ".....ooooo.....",
            "....oxxx++o....",
            ".o.oxxx++++o.o.",
            "...oxxxxx++o...",
            "oxooxxxxxxxooxo",
            "...oxxxxxxxo...",
            ".o.oxxxxxxxo.o.",
            "....oxxxxxo....",
            ".....ooooo.....",
            "...x.......x...",
            "...o...x...o...",
            ".......o......."
        }, new Color32(137, 71, 20, 255), new Color32(248, 163, 47, 255),
            new Color32(255, 223, 114, 255), new Color32(255, 239, 158, 255));

        private static Texture2D CreateAutumn() => Create("Autumn leaf", new[]
        {
            "...............",
            ".........ooo...",
            "........oxxxo..",
            ".......oxx+xo..",
            ".....ooox++xo..",
            "....oxxao++xo..",
            "...oxxxxaxxo...",
            "..ox++xaxxoo...",
            "..ox+xaxxxxo...",
            "...oxaxxxxo....",
            "...oaxxxoo.....",
            "..oaoooo.......",
            ".oa............",
            "..o............",
            "..............."
        }, new Color32(111, 46, 25, 255), new Color32(209, 91, 46, 255),
            new Color32(250, 157, 67, 255), new Color32(255, 201, 99, 255));

        private static Texture2D CreateWinter() => Create("Winter snowflake", new[]
        {
            ".......x.......",
            ".....x.x.x.....",
            "......xxx......",
            "..x....x....x..",
            "...x...x...x...",
            "x...x..x..x...x",
            ".x...x.x.x...x.",
            "..xxxxxaxxxxx..",
            ".x...x.x.x...x.",
            "x...x..x..x...x",
            "...x...x...x...",
            "..x....x....x..",
            "......xxx......",
            ".....x.x.x.....",
            ".......x......."
        }, new Color32(31, 74, 106, 255), new Color32(49, 117, 157, 255),
            new Color32(255, 255, 255, 255), new Color32(220, 248, 255, 255));

        private static Texture2D Create(string name, string[] rows, Color32 outline, Color32 fill,
            Color32 highlight, Color32 accent)
        {
            var size = rows.Length;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                pixels[(size - 1 - y) * size + x] = rows[y][x] switch
                {
                    'o' => outline,
                    'x' => fill,
                    '+' => highlight,
                    'a' => accent,
                    _ => default
                };
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
