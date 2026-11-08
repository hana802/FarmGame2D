using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame2D.Farming
{
    public sealed class FreeformFarmField : MonoBehaviour
    {
        private GameSession session;
        private Tilemap ground;
        private Tilemap road;
        private Transform plotParent;
        private int visibleCount;

        public void Configure(GameSession gameSession, Tilemap groundTiles, Tilemap roadTiles, Transform parent, int existingCount)
        {
            session = gameSession;
            ground = groundTiles;
            road = roadTiles;
            plotParent = parent;
            visibleCount = existingCount;
            session.StateChanged += Synchronize;
            Synchronize();
        }

        private void OnDestroy()
        {
            if (session != null)
            {
                session.StateChanged -= Synchronize;
            }
        }

        public bool TryTill(Vector2 target)
        {
            if (session == null || session.SelectedTool != FarmTool.Hoe)
            {
                return false;
            }

            var cell = ground.WorldToCell(target);
            var center = (Vector2)ground.GetCellCenterWorld(cell);
            if (!ground.HasTile(cell) || road.HasTile(cell) || IsBlocked(center))
            {
                session.ShowMessage("그 자리에는 밭을 만들 수 없습니다.");
                return false;
            }

            return session.AddFreeformPlot(center);
        }

        private bool IsBlocked(Vector2 center)
        {
            if (FarmFieldLayout.HasPlotNear(session.Data.farmPlots, center, 0.72f))
            {
                return true;
            }

            foreach (var hit in Physics2D.OverlapBoxAll(center, new Vector2(0.72f, 0.72f), 0f))
            {
                if (hit.GetComponent<PrototypePlayerController>() != null)
                {
                    continue;
                }

                if (!hit.isTrigger || hit.GetComponent<InteractableBehaviour>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void Synchronize()
        {
            while (visibleCount < session.Data.farmPlots.Count)
            {
                CreatePlot(visibleCount);
                visibleCount++;
            }
        }

        private void CreatePlot(int index)
        {
            var plotData = session.Data.farmPlots[index];
            var plot = PrototypeSpriteFactory.CreateBlock(
                $"Free Plot {index + 1}", FarmFieldLayout.GetWorldPosition(plotData), new Vector2(0.9f, 0.9f),
                Color.white, plotParent, true);
            plot.GetComponent<BoxCollider2D>().isTrigger = true;
            plot.AddComponent<FarmPlotView>().Configure(index);
        }
    }
}
