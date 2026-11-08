using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.World;
using System.Collections.Generic;
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
        private readonly List<GameObject> visiblePlots = new();

        public void Configure(GameSession gameSession, Tilemap groundTiles, Tilemap roadTiles, Transform parent)
        {
            session = gameSession;
            ground = groundTiles;
            road = roadTiles;
            plotParent = parent;
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
            if (visiblePlots.Count > session.Data.farmPlots.Count)
            {
                RebuildPlots();
                return;
            }

            while (visiblePlots.Count < session.Data.farmPlots.Count)
            {
                visiblePlots.Add(CreatePlot(visiblePlots.Count));
            }
        }

        private void RebuildPlots()
        {
            foreach (var plot in visiblePlots)
            {
                Destroy(plot);
            }

            visiblePlots.Clear();
            for (var index = 0; index < session.Data.farmPlots.Count; index++)
            {
                visiblePlots.Add(CreatePlot(index));
            }
        }

        private GameObject CreatePlot(int index)
        {
            var plotData = session.Data.farmPlots[index];
            var plot = PrototypeSpriteFactory.CreateBlock(
                $"Free Plot {index + 1}", FarmFieldLayout.GetWorldPosition(plotData), new Vector2(0.9f, 0.9f),
                Color.white, plotParent, true);
            plot.GetComponent<BoxCollider2D>().isTrigger = true;
            plot.AddComponent<FarmPlotView>().Configure(index);
            return plot;
        }
    }
}
