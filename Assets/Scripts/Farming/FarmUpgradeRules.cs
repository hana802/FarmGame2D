using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Farming
{
    public static class FarmUpgradeRules
    {
        public static IReadOnlyList<Vector2> SprinklerOffsets { get; } = new[]
        {
            Vector2.up, Vector2.right, Vector2.down, Vector2.left
        };

        public static int WaterWithSprinklers(GameSaveData data)
        {
            var watered = 0;
            foreach (var placement in data.placedDecorations)
            {
                if (placement.item != GameItem.Sprinkler) continue;
                var center = new Vector2(placement.worldX, placement.worldY);
                foreach (var plot in data.farmPlots)
                {
                    if (plot.greenhouse || plot.state != SoilState.Planted) continue;
                    var position = FarmFieldLayout.GetWorldPosition(plot);
                    foreach (var offset in SprinklerOffsets)
                    {
                        if ((position - center - offset).sqrMagnitude > 0.04f) continue;
                        plot.state = SoilState.Watered;
                        watered++;
                        break;
                    }
                }
            }
            return watered;
        }

        public static int TryGetTreeRegrowDay(this List<TreeRegrowth> trees, int id)
        {
            var cutTree = trees.Find(tree => tree.id == id);
            return cutTree?.day ?? 0;
        }

        public static void SetTreeRegrowDay(this List<TreeRegrowth> trees, int id, int day)
        {
            var cutTree = trees.Find(tree => tree.id == id);
            if (cutTree == null) trees.Add(new TreeRegrowth { id = id, day = day });
            else cutTree.day = day;
        }
    }

    [System.Serializable]
    public sealed class TreeRegrowth
    {
        public int id;
        public int day;
    }
}
