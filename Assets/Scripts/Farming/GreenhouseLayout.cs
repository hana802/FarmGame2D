using System.Collections.Generic;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.Farming
{
    public static class GreenhouseLayout
    {
        public static readonly Vector2 Site = new(8.5f, -7.65f);
        public static readonly Vector2 Interior = new(52f, -9.25f);

        public static IEnumerable<Vector2> PlotPositions()
        {
            for (var row = 0; row < 2; row++)
            for (var column = 0; column < 4; column++)
                yield return Interior + new Vector2((column - 1.5f) * 1.6f, row == 0 ? -0.85f : 1.2f);
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
