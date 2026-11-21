using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Farming
{
    public static class GreenhouseLayout
    {
        public static readonly Vector2 Site = new(8.5f, -7.65f);
        public static readonly Vector2 Interior = new(52f, -9.25f);

        public const float ExpansionDepth = 2f;

        public static IEnumerable<Vector2> PlotPositions(int upgradeLevel = 0)
        {
            for (var row = 0; row < 2 + upgradeLevel; row++)
            for (var column = 0; column < 4; column++)
                yield return Interior + new Vector2((column - 1.5f) * 1.6f, row == 0 ? -0.85f : 1.2f + (row - 1) * ExpansionDepth);
        }

        public static void EnsurePlots(GameSaveData data)
        {
            if (!data.greenhouseBuilt) return;
            foreach (var position in PlotPositions(data.greenhouseUpgradeLevel))
            {
                if (data.farmPlots.Exists(plot => plot.greenhouse && (FarmFieldLayout.GetWorldPosition(plot) - position).sqrMagnitude < 0.01f))
                    continue;
                data.farmPlots.Add(new FarmPlotData
                {
                    state = SoilState.Tilled, freeform = true, greenhouse = true, worldX = position.x, worldY = position.y
                });
            }
        }

        public static int WaterUpgradedGreenhouse(GameSaveData data)
        {
            if (!data.greenhouseBuilt || data.greenhouseUpgradeLevel < 2) return 0;
            var watered = 0;
            foreach (var plot in data.farmPlots)
            {
                if (!plot.greenhouse || plot.state != SoilState.Planted) continue;
                plot.state = SoilState.Watered;
                watered++;
            }
            return watered;
        }

        public static bool IsSiteOccupied(GameSaveData data)
        {
            foreach (var plot in data.farmPlots)
                if (OverlapsSite(FarmFieldLayout.GetWorldPosition(plot))) return true;
            foreach (var placed in data.placedDecorations)
                if (OverlapsSite(new Vector2(placed.worldX, placed.worldY))) return true;
            return false;
        }

        private static bool OverlapsSite(Vector2 position) =>
            Mathf.Abs(position.x - Site.x) <= 2.6f && position.y >= Site.y - 0.1f && position.y <= Site.y + 2.3f;
    }
}
