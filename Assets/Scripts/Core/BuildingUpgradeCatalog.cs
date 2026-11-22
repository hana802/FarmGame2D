using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public enum FarmBuilding { House, Greenhouse }
    public enum FarmFacility { None, Construction, Kitchen }

    public readonly struct BuildingUpgrade
    {
        public BuildingUpgrade(int price, int wood, int stone, GameItem metal, int metalCount, string benefit)
        {
            Price = price;
            Wood = wood;
            Stone = stone;
            Metal = metal;
            MetalCount = metalCount;
            Benefit = benefit;
        }

        public int Price { get; }
        public int Wood { get; }
        public int Stone { get; }
        public GameItem Metal { get; }
        public int MetalCount { get; }
        public string Benefit { get; }
        public MaterialProject.Cost Cost => new(Price, Wood, Stone,
            Metal == GameItem.CopperOre ? MetalCount : 0, Metal == GameItem.IronOre ? MetalCount : 0);
    }

    public readonly struct FarmRecipe
    {
        public FarmRecipe(GameItem result, GameItem first, GameItem second, int sellPrice)
        {
            Result = result;
            First = first;
            Second = second;
            SellPrice = sellPrice;
        }

        public GameItem Result { get; }
        public GameItem First { get; }
        public GameItem Second { get; }
        public int SellPrice { get; }
    }

    public static class BuildingUpgradeCatalog
    {
        public const int MaximumLevel = 2;
        private static readonly BuildingUpgrade[] HouseUpgrades =
        {
            new(0, 10, 8, GameItem.IronOre, 4, "2층 침실 · 1층 작업대와 판매함"),
            new(700, 28, 12, GameItem.IronOre, 4, "굴뚝 · 1층 난로에서 요리하기")
        };
        private static readonly BuildingUpgrade[] GreenhouseUpgrades =
        {
            new(300, 12, 8, GameItem.CopperOre, 3, "유리 지붕 확장 · 재배 12칸"),
            new(650, 20, 12, GameItem.IronOre, 4, "유리 채광창 · 16칸, 아침과 파종 직후 자동 급수")
        };

        public static IReadOnlyList<FarmRecipe> Recipes { get; } = new FarmRecipe[]
        {
            new(GameItem.PotatoSoup, GameItem.Potato, GameItem.Milk, 95),
            new(GameItem.TomatoOmelet, GameItem.Egg, GameItem.Tomato, 68)
        };

        public static BuildingUpgrade Get(FarmBuilding building, int level) =>
            (building == FarmBuilding.House ? HouseUpgrades : GreenhouseUpgrades)[level];
    }
}
