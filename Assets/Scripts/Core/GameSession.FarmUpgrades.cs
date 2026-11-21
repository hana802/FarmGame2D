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

        public void PurchaseSprinkler() => Purchase(GameItem.Sprinkler, 1, SprinklerPrice, "스프링클러");

        public bool IsTreeCut(int id) => Data.treeRegrowDays.TryGetTreeRegrowDay(id) > Data.day;

        public bool ChopTree(int id)
        {
            if (SelectedTool != FarmTool.Axe || IsTreeCut(id)) return false;
            Data.treeRegrowDays.SetTreeRegrowDay(id, Data.day + TreeRegrowthDays);
            Data.inventory.Add(GameItem.Wood, WoodPerTree);
            Message = $"목재 {WoodPerTree}개를 얻었습니다.";
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

            var inventory = Data.inventory;
            if (Data.money < GreenhousePrice || inventory.Count(GameItem.Wood) < GreenhouseWood
                || inventory.Count(GameItem.Stone) < GreenhouseStone || inventory.Count(GameItem.CopperOre) < GreenhouseCopper)
            {
                ShowMessage($"온실 재료 · {GreenhousePrice}G / 목재 {GreenhouseWood} / 돌 {GreenhouseStone} / 구리 광석 {GreenhouseCopper}");
                return false;
            }

            Data.money -= GreenhousePrice;
            inventory.TrySpend(GameItem.Wood, GreenhouseWood);
            inventory.TrySpend(GameItem.Stone, GreenhouseStone);
            inventory.TrySpend(GameItem.CopperOre, GreenhouseCopper);
            Data.greenhouseBuilt = true;
            foreach (var position in GreenhouseLayout.PlotPositions())
                Data.farmPlots.Add(new FarmPlotData { state = SoilState.Tilled, freeform = true,
                    greenhouse = true, worldX = position.x, worldY = position.y });

            Message = "온실을 지었습니다. 안에서는 계절과 상관없이 씨앗을 심을 수 있어요.";
            NotifyChanged();
            SaveSilently();
            return true;
        }

        private void WaterSprinklersAtDawn(int day)
        {
            if (Data.sprinklersWateredDay == day) return;
            Data.sprinklersWateredDay = day;
            if (FarmUpgradeRules.WaterWithSprinklers(Data) > 0)
            {
                AppendMorningEvent("스프링클러가 밭에 물을 주었습니다.");
                SaveSilently();
            }
        }
    }
}
