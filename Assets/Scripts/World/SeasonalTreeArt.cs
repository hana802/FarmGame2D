using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public static class SeasonalTreeArt
    {
        private static readonly Dictionary<Season, Sprite> sprites = new();

        public static bool IsFoliage(Color32 pixel)
        {
            if (pixel.a < 16)
            {
                return false;
            }

            var red = pixel.r;
            var green = pixel.g;
            var blue = pixel.b;
            var trunk = red > green * 1.35f && red > blue + 40f && green > blue;
            if (trunk || (blue > 120f && red > 180f))
            {
                return false;
            }

            return green > 40f && green + 15f >= red * 0.72f && green > blue + 25f;
        }

        public static Color32 Recolor(Color32 pixel, Season season)
        {
            if (!IsFoliage(pixel))
            {
                return pixel;
            }

            var red = pixel.r / 255f;
            var green = pixel.g / 255f;
            var blue = pixel.b / 255f;
            var luma = (red * 0.25f) + (green * 0.65f) + (blue * 0.1f);
            var shade = Mathf.Clamp(luma / 0.62f, 0.38f, 1.28f);
            var leaf = LeafColor(season);
            var color = new Color(leaf.r * shade, leaf.g * shade, leaf.b * shade, pixel.a / 255f);
            if (season == Season.Winter && luma > 0.55f)
            {
                color = Color.Lerp(color, new Color(0.96f, 0.98f, 1f, color.a), Mathf.InverseLerp(0.55f, 0.92f, luma));
            }

            return color;
        }

        public static Sprite GetSprite(Season season)
        {
            if (sprites.TryGetValue(season, out var cached))
            {
                return cached;
            }

            var source = WorldArtCatalog.GetSprite(WorldArtKind.Tree);
            if (source == null || source.texture == null || !source.texture.isReadable)
            {
                Debug.LogWarning("Tree texture is not readable, so seasonal leaf colors were skipped.");
                return source;
            }

            var sprite = CreateSeasonSprite(source, season);
            sprites[season] = sprite;
            return sprite;
        }

        private static Sprite CreateSeasonSprite(Sprite source, Season season)
        {
            var rect = source.rect;
            var originX = Mathf.FloorToInt(rect.x);
            var originY = Mathf.FloorToInt(rect.y);
            var width = Mathf.FloorToInt(rect.width);
            var height = Mathf.FloorToInt(rect.height);
            var sourcePixels = source.texture.GetPixels32();
            var sourceWidth = source.texture.width;
            var cropped = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = sourcePixels[((originY + y) * sourceWidth) + originX + x];
                    cropped[(y * width) + x] = Recolor(pixel, season);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = $"Seasonal Tree {season}",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(cropped);
            texture.Apply(false, false);
            var pivot = new Vector2(source.pivot.x / rect.width, source.pivot.y / rect.height);
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                pivot,
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = texture.name;
            return sprite;
        }

        private static Color LeafColor(Season season)
        {
            return season switch
            {
                Season.Spring => new Color(0.95f, 0.46f, 0.64f),
                Season.Summer => new Color(0.46f, 0.74f, 0.24f),
                Season.Autumn => new Color(0.92f, 0.46f, 0.14f),
                Season.Winter => new Color(0.70f, 0.80f, 0.90f),
                _ => Color.white
            };
        }
    }
}
