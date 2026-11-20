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
            if (Sprites.TryGetValue(item, out var sprite)) return sprite;

            var (assetName, pixelsPerUnit, basePivot) = item switch
            {
                GameItem.FlowerPot => ("town_flower_pot_v2", 176f, 0.04f),
                GameItem.StreetLamp => ("town_street_lamp_v2", 160f, 0.03f),
                GameItem.TownBench => ("town_bench_v2", 176f, 0.045f),
                _ => (null, 0f, 0f)
            };
            if (assetName == null) return null;

            var texture = Resources.Load<Texture2D>($"Art/World/{assetName}");
            if (texture == null)
            {
                Debug.LogError($"Town decoration texture not found: {assetName}");
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, basePivot), pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = assetName;
            Sprites[item] = sprite;
            return sprite;
        }
    }
}
