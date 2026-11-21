using FarmGame2D.Core;
using FarmGame2D.Audio;
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
        private List<FarmPlotData> boundPlots;
        private readonly List<GameObject> visiblePlots = new();
        private PrototypePlayerController player;
        private GameObject wateringPreview;

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

        private void LateUpdate()
        {
            var show = session != null
                && session.SelectedTool == FarmTool.WateringCan
                && !session.SelectedDecoration.HasValue
                && !session.InventoryOpen
                && !session.ShopOpen
                && !session.PhoneOpen
                && !session.QuestBoardOpen
                && !session.SellBoxOpen;
            if (!show || !TryGetWateringCenter(out var center))
            {
                if (wateringPreview != null) wateringPreview.SetActive(false);
                return;
            }

            if (wateringPreview == null) wateringPreview = CreateWateringPreview();
            wateringPreview.transform.position = center;
            wateringPreview.SetActive(true);
        }

        private bool TryGetWateringCenter(out Vector2 center)
        {
            center = default;
            if (player == null) player = FindAnyObjectByType<PrototypePlayerController>();
            if (player == null || session?.Data?.farmPlots == null) return false;

            var origin = (Vector2)player.transform.position;
            var best = 1.35f * 1.35f;
            FarmPlotData nearest = null;
            foreach (var plot in session.Data.farmPlots)
            {
                if (plot.greenhouse) continue;
                var position = FarmFieldLayout.GetWorldPosition(plot);
                var distance = (position - origin).sqrMagnitude;
                if (distance > best) continue;
                best = distance;
                nearest = plot;
                center = position;
            }

            return nearest != null && nearest.state == SoilState.Planted;
        }

        private GameObject CreateWateringPreview()
        {
            var preview = new GameObject("Watering Coverage Preview");
            preview.transform.SetParent(transform, false);
            foreach (var offset in FarmUpgradeRules.SprinklerOffsets)
            {
                var marker = PrototypeSpriteFactory.CreateBlock(
                    "Watering Coverage", Vector2.zero, Vector2.one * 0.84f,
                    new Color(0.37f, 0.79f, 0.92f, 0.4f), preview.transform);
                marker.transform.localPosition = offset;
                marker.GetComponent<SpriteRenderer>().sortingOrder = 5;
            }

            return preview;
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

            var added = session.AddFreeformPlot(center);
            if (added)
            {
                GameAudioController.Play(GameAudioCue.TillSoil);
            }

            return added;
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
            if (boundPlots != session.Data.farmPlots || visiblePlots.Count > session.Data.farmPlots.Count)
            {
                boundPlots = session.Data.farmPlots;
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
                plot.SetActive(false);
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
            if (plotData.greenhouse) plot.GetComponent<SpriteRenderer>().sortingOrder = 2;
            plot.AddComponent<FarmPlotView>().Configure(index);
            return plot;
        }
    }
}
