using System.Collections.Generic;
using FarmGame2D.Farming;
using FarmGame2D.World;

namespace FarmGame2D.Core
{
    public static class GameSaveDataNormalizer
    {
        private static readonly GameItem[] MiningOres =
        {
            GameItem.Stone,
            GameItem.CopperOre,
            GameItem.Stone,
            GameItem.IronOre,
            GameItem.CopperOre,
            GameItem.GoldOre
        };

        public static void Normalize(GameSaveData data)
        {
            data.version = GameSaveData.CurrentVersion;
            data.inventory ??= new InventoryModel();
            data.pulledWeedIds ??= new List<int>();
            data.farmPlots ??= new List<FarmPlotData>();
            data.miningNodes ??= new List<MiningNodeData>();
            data.ownedChickens = System.Math.Max(1, data.ownedChickens);

            if (data.eggReady && data.readyEggs == 0)
            {
                data.readyEggs = 1;
                data.eggReady = false;
            }

            FarmFieldLayout.EnsurePlots(data.farmPlots);
            while (data.miningNodes.Count < MiningOres.Length)
            {
                var id = data.miningNodes.Count;
                data.miningNodes.Add(new MiningNodeData { id = id, ore = MiningOres[id] });
            }
        }
    }
}
