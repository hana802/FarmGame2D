using System.Collections.Generic;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using UnityEngine;

namespace FarmGame2D.Art
{
    public static class FarmPlotArtCatalog
    {
        private const int CropCellSize = 128;
        private const int CropColumns = 4;
        private static readonly Dictionary<SoilState, Sprite> sprites = new();
        private static readonly Dictionary<(GameItem Seed, bool Ripe), Sprite> cropSprites = new();
        private static Texture2D cropSheet;

        public static Sprite GetCropSprite(GameItem seed, bool ripe)
        {
            var cell = seed switch
            {
                GameItem.PotatoSeed => 0,
                GameItem.StrawberrySeed => 1,
                GameItem.TomatoSeed => 2,
                GameItem.CornSeed => 3,
                GameItem.PumpkinSeed => 4,
                GameItem.SweetPotatoSeed => 5,
                GameItem.WheatSeed => 6,
                _ => -1
            };
            if (cell < 0) return null;

            var key = (seed, ripe);
            if (cropSprites.TryGetValue(key, out var cached)) return cached;

            cropSheet ??= Resources.Load<Texture2D>("Art/World/FarmPlots/crop_growth_v1");
            if (cropSheet == null) return null;

            var rowFromTop = cell / CropColumns + (ripe ? 2 : 0);
            var frame = new Rect(cell % CropColumns * CropCellSize,
                cropSheet.height - (rowFromTop + 1) * CropCellSize, CropCellSize, CropCellSize);
            var sprite = Sprite.Create(cropSheet, frame, new Vector2(0.5f, 0.27f),
                CropCellSize, 0, SpriteMeshType.FullRect);
            sprite.name = $"crop_{seed}_{(ripe ? "ripe" : "young")}";
            cropSprites.Add(key, sprite);
            return sprite;
        }

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
