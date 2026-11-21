using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Farming
{
    public static class FarmUpgradeRules
    {
        // Marked watering shape. +Y is up the screen, so the pattern reaches south:
        // the reference tile and its left and right, both sides one tile south,
        // and all three tiles two tiles south. The tile directly south stays dry.
        public static IReadOnlyList<Vector2> SprinklerOffsets { get; } = new[]
        {
            new Vector2(-1f, 0f), Vector2.zero, Vector2.right,
            new Vector2(-1f, -1f), new Vector2(1f, -1f),
            new Vector2(-1f, -2f), new Vector2(0f, -2f), new Vector2(1f, -2f)
        };

        public static int WaterAround(GameSaveData data, Vector2 center)
        {
            var watered = 0;
            foreach (var plot in data.farmPlots)
            {
                if (plot.state != SoilState.Planted) continue;
                var position = FarmFieldLayout.GetWorldPosition(plot);
                if (plot.greenhouse && (position - center).sqrMagnitude > 0.04f) continue;
                foreach (var offset in SprinklerOffsets)
                {
                    if ((position - center - offset).sqrMagnitude > 0.04f) continue;
                    plot.state = SoilState.Watered;
                    watered++;
                    break;
                }
            }

            return watered;
        }

        public static int WaterWithSprinklers(GameSaveData data)
        {
            var watered = 0;
            foreach (var placement in data.placedDecorations)
            {
                if (placement.item != GameItem.Sprinkler) continue;
                watered += WaterAround(data, new Vector2(placement.worldX, placement.worldY));
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
