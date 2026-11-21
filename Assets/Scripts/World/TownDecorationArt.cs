using System;
using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public static class TownDecorationArt
    {
        private static readonly Dictionary<GameItem, Sprite> Sprites = new();

        public static Sprite GetSprite(GameItem item)
        {
            if (Sprites.TryGetValue(item, out var sprite))
            {
                return sprite;
            }

            sprite = item switch
            {
                GameItem.FlowerPot => FromRows(item, 42f, FlowerPotRows),
                GameItem.StreetLamp => FromRows(item, 46f, StreetLampRows),
                GameItem.TownBench => FromRows(item, 48f, BenchRows),
                _ => null
            };
            if (sprite != null)
            {
                Sprites[item] = sprite;
            }

            return sprite;
        }

        private static readonly string[] FlowerPotRows =
        {
            "............dddd................",
            "...........dLLLLd...............",
            "..........dLLbBbLLd.............",
            ".........dLLbBBBbLLd............",
            "..........dLbYYbLLd.............",
            ".........dLLLbBbLLLd............",
            "..........ddLLLLdd..............",
            ".........dLLyYyYLLd.............",
            "........dLLyYYYYYyLd............",
            ".........dLLyYYyLLd.............",
            "..........ddLLLLdd..............",
            "...........sSSSs................",
            "..........sSSSSSss..............",
            ".........osSSSSSSSso............",
            "........oPPPPPPPPPPPPo..........",
            ".......oPDDDDDDDDDDDDPo.........",
            ".......oPDppppppppppDPo.........",
            ".......oPDppppppppppDPo.........",
            ".......oPDppppppppppDPo.........",
            ".......oPDppppppppppDPo.........",
            "........oPPPPPPPPPPPPo..........",
            ".........oooooooooooo...........",
            "..........aaaaaaaaaaaa.........."
        };

        private static readonly string[] StreetLampRows =
        {
            ".........oooooooooo.........",
            "........oMMMMMMMMMMo........",
            "........oMooooooooMo........",
            "........oMoGGGGGGoMo........",
            "........oMoGggggGoMo........",
            "........oMoGggggGoMo........",
            "........oMoGggggGoMo........",
            "........oMoGGGGGGoMo........",
            "........oMooooooooMo........",
            ".........oMMMMMMMMo.........",
            "..........oWWWWWWo..........",
            "...........oWWWWo...........",
            "............oWWo............",
            "............oWWo............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............nWWn............",
            "............oWWo............",
            "...........oWWWWo...........",
            "..........oWWWWWWo..........",
            ".........oooooooooo.........",
            "........aaaaaaaaaaaa........"
        };

        private static readonly string[] BenchRows =
        {
            "...o..............................o...",
            "..oWo............................oWo..",
            "..oWo............................oWo..",
            "..oWo............................oWo..",
            ".oWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWo.",
            ".owwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwo.",
            ".oWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWo.",
            "..o..............................o..",
            "..oWo.oWo.oWo.oWo.oWo.oWo.oWo.oWWo..",
            "..owo.owo.owo.owo.owo.owo.owo.owoo..",
            "...o..............................o.",
            "...oWWWWWWWWWWWWWWWWWWWWWWWWWWWWWo..",
            "...owwwwwwwwwwwwwwwwwwwwwwwwwwwwo...",
            "...oWWWWWWWWWWWWWWWWWWWWWWWWWWWWWo..",
            "...owwwwwwwwwwwwwwwwwwwwwwwwwwwwo...",
            "....o............................o..",
            "....oWWo......................oWWo..",
            "....oWWo......................oWWo..",
            "....oWWo......................oWWo..",
            "....oooo......................oooo..",
            "...aaaaaaaa..................aaaa..."
        };

        private static Color32 Palette(char pixel)
        {
            return pixel switch
            {
                '.' => new Color32(0, 0, 0, 0),
                'a' => new Color32(48, 36, 28, 70),
                'o' => new Color32(62, 36, 28, 255),
                'd' => new Color32(34, 92, 42, 255),
                'l' => new Color32(72, 140, 58, 255),
                'L' => new Color32(118, 176, 72, 255),
                'b' => new Color32(214, 86, 122, 255),
                'B' => new Color32(255, 176, 196, 255),
                'y' => new Color32(214, 132, 48, 255),
                'Y' => new Color32(255, 214, 96, 255),
                's' => new Color32(92, 58, 36, 255),
                'S' => new Color32(132, 86, 48, 255),
                'p' => new Color32(176, 96, 58, 255),
                'P' => new Color32(214, 142, 86, 255),
                'D' => new Color32(122, 64, 40, 255),
                'n' => new Color32(92, 58, 40, 255),
                'w' => new Color32(156, 102, 58, 255),
                'W' => new Color32(204, 146, 82, 255),
                'm' => new Color32(78, 70, 66, 255),
                'M' => new Color32(168, 156, 146, 255),
                'g' => new Color32(255, 186, 64, 255),
                'G' => new Color32(255, 236, 170, 255),
                _ => throw new ArgumentOutOfRangeException(nameof(pixel), pixel, "Unknown decoration pixel.")
            };
        }

        private static Sprite FromRows(GameItem item, float pixelsPerUnit, string[] rowsTopToBottom)
        {
            var height = rowsTopToBottom.Length;
            var width = rowsTopToBottom[0].Length;
            for (var index = 1; index < height; index++)
            {
                if (rowsTopToBottom[index].Length != width)
                {
                    throw new InvalidOperationException(
                        $"{item} row {index} is {rowsTopToBottom[index].Length} wide; expected {width}.");
                }
            }

            var pixels = new Color32[width * height];
            for (var row = 0; row < height; row++)
            {
                var y = height - 1 - row;
                var line = rowsTopToBottom[row];
                for (var x = 0; x < width; x++)
                {
                    pixels[(y * width) + x] = Palette(line[x]);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Town Decoration {item}"
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.08f),
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = texture.name;
            return sprite;
        }
    }
}
