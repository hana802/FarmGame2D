using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public static class ItemCatalog
    {
        private static readonly Dictionary<GameItem, string> Labels = new()
        {
            { GameItem.WheatSeed, "겨울밀 씨앗" },
            { GameItem.Wheat, "겨울밀" },
            { GameItem.Feed, "동물 사료" },
            { GameItem.Egg, "달걀" },
            { GameItem.Weed, "잡초" },
            { GameItem.PotatoSeed, "감자 씨앗" },
            { GameItem.Potato, "감자" },
            { GameItem.StrawberrySeed, "딸기 씨앗" },
            { GameItem.Strawberry, "딸기" },
            { GameItem.TomatoSeed, "토마토 씨앗" },
            { GameItem.Tomato, "토마토" },
            { GameItem.CornSeed, "옥수수 씨앗" },
            { GameItem.Corn, "옥수수" },
            { GameItem.PumpkinSeed, "호박 씨앗" },
            { GameItem.Pumpkin, "호박" },
            { GameItem.SweetPotatoSeed, "고구마 씨앗" },
            { GameItem.SweetPotato, "고구마" },
            { GameItem.Milk, "우유" },
            { GameItem.Stone, "돌" },
            { GameItem.CopperOre, "구리 광석" },
            { GameItem.IronOre, "철 광석" },
            { GameItem.GoldOre, "금 광석" }
            ,{ GameItem.RiverFish, "민물고기" }
            ,{ GameItem.SeaBass, "농어" }
            ,{ GameItem.GoldenCarp, "황금 잉어" }
        };

        private static readonly Dictionary<GameItem, int> SellPrices = new()
        {
            { GameItem.Egg, 25 },
            { GameItem.Milk, 55 },
            { GameItem.Weed, 2 },
            { GameItem.Stone, 4 },
            { GameItem.CopperOre, 18 },
            { GameItem.IronOre, 32 },
            { GameItem.GoldOre, 60 },
            { GameItem.RiverFish, 28 },
            { GameItem.SeaBass, 45 },
            { GameItem.GoldenCarp, 120 }
        };

        public static string GetLabel(GameItem item)
        {
            return Labels.TryGetValue(item, out var label) ? label : item.ToString();
        }

        public static int GetSellPrice(GameItem item)
        {
            if (CropCatalog.TryGetByProduce(item, out var crop))
            {
                return crop.SellPrice;
            }

            return SellPrices.TryGetValue(item, out var price) ? price : 0;
        }

        public static bool IsSeed(GameItem item)
        {
            return CropCatalog.TryGetBySeed(item, out _);
        }
    }
}
