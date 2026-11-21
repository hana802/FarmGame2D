using FarmGame2D.Audio;
using FarmGame2D.Farming;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed partial class GameSession
    {
        public const int SprinklerPrice = 90;
        public const int GreenhousePrice = 450;
        public const int GreenhouseWood = 12;
        public const int GreenhouseStone = 6;
        public const int GreenhouseCopper = 3;
        public const int WoodPerTree = 4;
        public const int TreeRegrowthDays = 4;

        public void PurchaseSprinkler(int count = 1)
        {
            count = Mathf.Max(1, count);
            Purchase(GameItem.Sprinkler, count, SprinklerPrice * count,
                count == 1 ? "스프링클러" : $"스프링클러 {count}개");
        }

        public bool IsTreeCut(int id) => Data.treeRegrowDays.TryGetTreeRegrowDay(id) > Data.day;

        public bool ChopTree(int id)
        {
            if (SelectedTool != FarmTool.Axe || IsTreeCut(id)) return false;
            Data.treeRegrowDays.SetTreeRegrowDay(id, Data.day + TreeRegrowthDays);
            Data.inventory.Add(GameItem.Wood, WoodPerTree);
            Message = $"목재 {WoodPerTree}개를 얻었습니다.";
            CompleteCurrentStarterQuest();
            GameAudioController.Play(GameAudioCue.TillSoil);
            NotifyChanged();
            SaveSilently();
            return true;
        }

        public bool BuildGreenhouse()
        {
            if (Data.greenhouseBuilt) return false;
            if (GreenhouseLayout.IsSiteOccupied(Data))
            {
                ShowMessage("온실 자리에 있는 밭이나 설치물을 먼저 옮겨 주세요.");
                return false;
            }

            if (!MaterialProject.TryPay(Data, MaterialProject.Greenhouse))
            {
                ShowMessage(MaterialProject.Describe("온실", MaterialProject.Greenhouse) + ". 재료가 부족합니다.");
                return false;
            }

            Data.greenhouseBuilt = true;
            GreenhouseLayout.EnsurePlots(Data);

            Message = "온실을 지었습니다. 안쪽 선반에서 사계절 씨앗을 사고, 밭은 아침에 스스로 물을 머금어요.";
            NotifyChanged();
            SaveSilently();
            return true;
        }

        private void WaterSprinklersAtDawn(int day)
        {
            if (Data.sprinklersWateredDay == day) return;
            Data.sprinklersWateredDay = day;
            var greenhouseWatered = FarmUpgradeRules.WaterGreenhousePlots(Data) > 0;
            var sprinklersWatered = FarmUpgradeRules.WaterWithSprinklers(Data) > 0;
            if (greenhouseWatered || sprinklersWatered)
            {
                SaveSilently();
            }
        }
    }

    public static class MaterialProject
    {
        public readonly struct Cost
        {
            public Cost(int money, int wood, int stone, int copper, int iron)
            {
                Money = money;
                Wood = wood;
                Stone = stone;
                Copper = copper;
                Iron = iron;
            }

            public int Money { get; }
            public int Wood { get; }
            public int Stone { get; }
            public int Copper { get; }
            public int Iron { get; }
        }

        public static Cost Greenhouse { get; } = new(
            GameSession.GreenhousePrice,
            GameSession.GreenhouseWood,
            GameSession.GreenhouseStone,
            GameSession.GreenhouseCopper,
            0);

        public static Cost House { get; } = BuildingUpgradeCatalog.Get(FarmBuilding.House, 0).Cost;

        public static bool HasMaterials(GameSaveData data, Cost cost)
        {
            var inventory = data.inventory;
            return data.money >= cost.Money
                && inventory.Count(GameItem.Wood) >= cost.Wood
                && inventory.Count(GameItem.Stone) >= cost.Stone
                && inventory.Count(GameItem.CopperOre) >= cost.Copper
                && inventory.Count(GameItem.IronOre) >= cost.Iron;
        }

        public static bool TryPay(GameSaveData data, Cost cost)
        {
            if (!HasMaterials(data, cost))
            {
                return false;
            }

            data.money -= cost.Money;
            Spend(data, GameItem.Wood, cost.Wood);
            Spend(data, GameItem.Stone, cost.Stone);
            Spend(data, GameItem.CopperOre, cost.Copper);
            Spend(data, GameItem.IronOre, cost.Iron);
            return true;
        }

        public static string Describe(string name, Cost cost)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (cost.Money > 0) parts.Add($"{cost.Money}G");
            if (cost.Wood > 0) parts.Add($"목재 {cost.Wood}");
            if (cost.Stone > 0) parts.Add($"돌 {cost.Stone}");
            if (cost.Copper > 0) parts.Add($"구리 광석 {cost.Copper}");
            if (cost.Iron > 0) parts.Add($"철 광석 {cost.Iron}");
            return $"{name} · {string.Join(" / ", parts)}";
        }

        private static void Spend(GameSaveData data, GameItem item, int amount)
        {
            if (amount > 0)
            {
                data.inventory.TrySpend(item, amount);
            }
        }
    }
}
