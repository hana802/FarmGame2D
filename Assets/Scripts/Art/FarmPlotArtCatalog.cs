using System.Collections.Generic;
using FarmGame2D.Farming;
using UnityEngine;

namespace FarmGame2D.Art
{
    public static class FarmPlotArtCatalog
    {
        private static readonly Dictionary<SoilState, Sprite> sprites = new();

        public static Sprite GetSprite(SoilState state)
        {
            if (sprites.TryGetValue(state, out var cached))
            {
                return cached;
            }

            var path = GetResourcePath(state);
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogError($"Farm plot texture not found: {path}");
                return null;
            }

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width,
                0,
                SpriteMeshType.FullRect);
            sprite.name = $"farm_plot_{state}";
            sprites[state] = sprite;
            return sprite;
        }

        public static string GetResourcePath(SoilState state)
        {
            return state switch
            {
                SoilState.Empty => "Art/World/FarmPlots/farm_plot_empty_v1",
                SoilState.Tilled => "Art/World/FarmPlots/farm_plot_tilled_v1",
                SoilState.Planted => "Art/World/FarmPlots/farm_plot_planted_v1",
                SoilState.Watered => "Art/World/FarmPlots/farm_plot_watered_v1",
                SoilState.Ready => "Art/World/FarmPlots/farm_plot_ready_v1",
                _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}
