using System.Collections.Generic;
using System.Linq;

namespace FarmGame2D.Core
{
    public sealed class CropDefinition
    {
        public CropDefinition(
            GameItem seed,
            GameItem produce,
            string label,
            Season season,
            int growthDays,
            int packPrice,
            int sellPrice)
        {
            Seed = seed;
            Produce = produce;
            Label = label;
            Season = season;
            GrowthDays = growthDays;
            PackPrice = packPrice;
            SellPrice = sellPrice;
        }

        public GameItem Seed { get; }
        public GameItem Produce { get; }
        public string Label { get; }
        public Season Season { get; }
        public int GrowthDays { get; }
        public int PackPrice { get; }
        public int SellPrice { get; }
    }

    public static class CropCatalog
    {
        private static readonly CropDefinition[] Crops =
        {
            new(GameItem.PotatoSeed, GameItem.Potato, "감자", Season.Spring, 2, 25, 18),
            new(GameItem.StrawberrySeed, GameItem.Strawberry, "딸기", Season.Spring, 3, 45, 35),
            new(GameItem.TomatoSeed, GameItem.Tomato, "토마토", Season.Summer, 3, 35, 28),
            new(GameItem.CornSeed, GameItem.Corn, "옥수수", Season.Summer, 4, 50, 42),
            new(GameItem.PumpkinSeed, GameItem.Pumpkin, "호박", Season.Autumn, 4, 55, 48),
            new(GameItem.SweetPotatoSeed, GameItem.SweetPotato, "고구마", Season.Autumn, 3, 40, 32),
            new(GameItem.WheatSeed, GameItem.Wheat, "겨울밀", Season.Winter, 3, 30, 24)
        };

        public static IReadOnlyList<CropDefinition> GetForSeason(Season season)
        {
            return Crops.Where(crop => crop.Season == season).ToArray();
        }

        public static IReadOnlyList<CropDefinition> GetAll()
        {
            return Crops;
        }

        public static bool TryGetBySeed(GameItem seed, out CropDefinition crop)
        {
            crop = Crops.FirstOrDefault(candidate => candidate.Seed == seed);
            return crop != null;
        }

        public static bool TryGetByProduce(GameItem produce, out CropDefinition crop)
        {
            crop = Crops.FirstOrDefault(candidate => candidate.Produce == produce);
            return crop != null;
        }
    }
}
