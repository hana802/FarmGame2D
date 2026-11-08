using System.Collections.Generic;
using UnityEngine;

namespace FarmGame2D.Farming
{
    public static class FarmFieldLayout
    {
        public const int Columns = 6;
        public const int Rows = 4;
        public const int PlotCount = Columns * Rows;

        public static Vector2 GetWorldPosition(int gridX, int gridY)
        {
            return new Vector2(-4.5f + (gridX * 1.4f), -10.5f + (gridY * 1.4f));
        }

        public static void EnsurePlots(List<FarmPlotData> plots)
        {
            for (var index = 0; index < plots.Count; index++)
            {
                plots[index].gridX = index % Columns;
                plots[index].gridY = index / Columns;
            }

            while (plots.Count < PlotCount)
            {
                var index = plots.Count;
                plots.Add(new FarmPlotData
                {
                    gridX = index % Columns,
                    gridY = index / Columns
                });
            }
        }
    }
}
