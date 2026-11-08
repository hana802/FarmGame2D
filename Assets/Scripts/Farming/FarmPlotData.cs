using System;
using FarmGame2D.Core;

namespace FarmGame2D.Farming
{
    public enum SoilState
    {
        Empty,
        Tilled,
        Planted,
        Watered,
        Ready
    }

    [Serializable]
    public sealed class FarmPlotData
    {
        public SoilState state;
        public int growthDays;
        public int gridX;
        public int gridY;
        public bool freeform;
        public float worldX;
        public float worldY;
        public GameItem plantedSeed = GameItem.WheatSeed;

        public void AdvanceDay()
        {
            if (state != SoilState.Watered)
            {
                return;
            }

            growthDays++;
            var requiredDays = CropCatalog.TryGetBySeed(plantedSeed, out var crop) ? crop.GrowthDays : 2;
            state = growthDays >= requiredDays ? SoilState.Ready : SoilState.Planted;
        }

        public void Plant(GameItem seed)
        {
            plantedSeed = seed;
            growthDays = 0;
            state = SoilState.Planted;
        }

        public void ResetToTilled()
        {
            state = SoilState.Tilled;
            growthDays = 0;
        }
    }
}
