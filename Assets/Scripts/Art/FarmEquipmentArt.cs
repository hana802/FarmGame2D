using UnityEngine;

namespace FarmGame2D.Art
{
    public static class FarmEquipmentArt
    {
        private static Sprite axe, sprinkler, stump, greenhouse, sign, feedSack;

        public static Sprite FeedSack => feedSack ??= PixelSprite("Animal Feed Sack", 32f, new[]
        {
            "......................",
            "......................",
            ".......#######........",
            "......#htthtwt#.......",
            ".......#hhttw#........",
            "........#tww#.........",
            ".......#ggggg#........",
            "......#httwgtw#.......",
            ".....#hhtttgttw#......",
            "....#hhttttttttw#.....",
            "...#hhtttgggttttw#....",
            "...#httgggggggttw#....",
            "..#hhttgggygggtttw#...",
            "..#httggyygygggttw#...",
            "..#httgggyyggggttw#...",
            "..#httggyygygggttw#...",
            "..#httgggyyggggttw#...",
            "..#htttgggygggttww#...",
            "..#thttttgggtttwww#...",
            "...#thttttttttwww#....",
            "....#wwttttwwwww#.....",
            ".....###########......",
            "......................",
            "......................",
        });

        public static Sprite Axe => axe ??= PixelSprite("Held Axe", 20f, new[]
        {
            "..............", ".....####.....", "....#ww#s###..", "....#ww#ssll#.",
            "....#ww#sssl#.", ".....###sssl#.", ".....#w#sss#..", ".....#w####...",
            ".....#w#......", ".....#w#......", ".....#w#......", ".....#w#......",
            ".....#w#......", ".....#w#......", ".....#w#......", ".....###......"
        });

        public static Sprite Sprinkler => sprinkler ??= PixelSprite("Four-Way Sprinkler", 32f, new[]
        {
            "........########........",
            "......##cccccccc##......",
            ".....#ccc######ccc#.....",
            ".....#cc#cccccc#cc#.....",
            "......##cc#cc#cc##......",
            "........###cc###........",
            ".........#cccc#.........",
            ".........##cc##.........",
            "..........#cc#..........",
            "..........#mm#..........",
            "..........#mm#..........",
            "..........#mm#..........",
            "..........#mm#..........",
            ".........##mm##.........",
            "........#mmmmmm#........",
            ".......#mm#mm#mm#.......",
            ".......#mm#mm#mm#.......",
            "........########........",
            ".......##mmmmmm##.......",
            "......#mm######mm#......",
            "......#mm#....#mm#......",
            ".......#mm####mm#.......",
            "........########........",
            ".........######........."
        });

        public static Sprite Stump => stump ??= PixelSprite("Tree Stump", 21f, new[]
        {
            "...########...", "..#wttttttw#..", ".#wttwwwtttw#.", ".#wttwtwtttw#.",
            "..#wwttttww#..", "..#w######w#..", "..#w#ww##ww#..", ".#ww#ww##www#.",
            "###ww####www##", "...########..."
        });

        public static Sprite Sign => sign ??= PixelSprite("Greenhouse Sign", 22f, new[]
        {
            "..########..", ".#tttttttt#.", "#ttggggggtt#", "#ttgbbbgttt#", "#ttggggggtt#",
            ".#tttttttt#.", "..########..", ".....#w#....", ".....#w#....", ".....###...."
        });

        public static Sprite Greenhouse => greenhouse ??= CreateGreenhouse();

        private static Sprite PixelSprite(string name, float pixelsPerUnit, string[] rows)
        {
            var texture = CreateTexture(name, rows[0].Length, rows.Length);
            for (var y = 0; y < rows.Length; y++)
            for (var x = 0; x < rows[y].Length; x++)
                texture.SetPixel(x, rows.Length - 1 - y, Pixel(rows[y][x]));
            return Finish(texture, pixelsPerUnit);
        }

        private static Sprite CreateGreenhouse()
        {
            const int width = 120, height = 94;
            var texture = CreateTexture("Glass Greenhouse", width, height);
            for (var y = 2; y < 91; y++)
            {
                var inset = y <= 52 ? 3 : 3 + (y - 52) * 55 / 39;
                for (var x = inset; x < width - inset; x++)
                {
                    var color = GreenhousePixel(x, y, inset, width);
                    texture.SetPixel(x, y, color);
                }
            }
            return Finish(texture, 25f);
        }

        private static Color32 GreenhousePixel(int x, int y, int inset, int width)
        {
            if (y <= 3 || x <= inset + 2 || x >= width - inset - 3) return Pixel('#');
            if (y < 11) return Pixel(y < 6 || (x + (y < 8 ? 6 : 0)) % 15 == 0 ? 'w' : 't');
            if (y < 49 && x >= 46 && x <= 73)
            {
                if (x is <= 49 or >= 70 || y > 45) return Pixel('w');
                if (x == 67 && y == 26) return Pixel('t');
                if (y < 19 || x == 59) return Pixel('g');
            }
            if (y is 11 or 12 or 29 or 30 or 49 or 50 or 51 || x % 19 is 2 or 3 || x is 58 or 59 or 60)
                return Pixel(y is 30 or 51 ? 'l' : 'g');
            if (y < 26 && ((x * 7 + y) % 23 < 8)) return Pixel('g');
            if ((x + 2 * y) % 37 <= 2) return Pixel('l');
            return Pixel(y > 51 ? 'b' : 's');
        }

        private static Texture2D CreateTexture(string name, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(new Color32[width * height]);
            return texture;
        }

        private static Sprite Finish(Texture2D texture, float pixelsPerUnit)
        {
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.1f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Color32 Pixel(char value) => value switch
        {
            '#' => new(50, 39, 35, 255), 'w' => new(147, 85, 49, 255), 't' => new(222, 168, 101, 255),
            's' => new(116, 162, 151, 255), 'l' => new(215, 227, 193, 255),
            'b' => new(156, 196, 190, 255), 'g' => new(54, 96, 72, 255),
            'c' => new(42, 168, 214, 255), 'm' => new(90, 98, 108, 255),
            'h' => new(250, 207, 145, 255), 'y' => new(249, 218, 108, 255),
            _ => new(0, 0, 0, 0)
        };
    }
}
