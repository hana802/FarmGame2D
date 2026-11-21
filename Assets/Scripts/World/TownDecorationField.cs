using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame2D.World
{
    public sealed class TownDecorationField : MonoBehaviour
    {
        private GameSession session;
        private Tilemap ground;
        private Tilemap road;
        private Transform decorationRoot;
        private PrototypePlayerController player;
        private GameObject sprinklerPreview;
        private readonly List<PlacedDecorationData> renderedDecorations = new();

        public void Configure(GameSession gameSession, Tilemap groundTiles, Tilemap roadTiles)
        {
            session = gameSession;
            ground = groundTiles;
            road = roadTiles;
            decorationRoot = new GameObject("Placed Town Decorations").transform;
            decorationRoot.SetParent(transform, false);
            session.StateChanged += Synchronize;
            Synchronize();
        }

        private void OnDestroy()
        {
            if (session != null) session.StateChanged -= Synchronize;
        }

        private void LateUpdate()
        {
            var visible = session != null && session.SelectedDecoration == GameItem.Sprinkler
                && !session.InventoryOpen && !session.ShopOpen && !session.PhoneOpen && !session.QuestBoardOpen;
            if (!visible)
            {
                if (sprinklerPreview != null) sprinklerPreview.SetActive(false);
                return;
            }

            if (player == null) player = FindAnyObjectByType<PrototypePlayerController>();
            if (player == null) return;
            var cell = ground.WorldToCell(player.GetToolTarget());
            var center = (Vector2)ground.GetCellCenterWorld(cell);
            if (sprinklerPreview == null) sprinklerPreview = CreateSprinklerPreview();
            sprinklerPreview.transform.position = center;
            sprinklerPreview.SetActive(!WorldAreaProfile.IsInterior(center));
            var valid = CanPlace(center, cell, GameItem.Sprinkler);
            var marker = sprinklerPreview.transform.GetChild(0).GetComponent<SpriteRenderer>();
            marker.color = valid ? new Color(0.67f, 0.94f, 1f, 0.66f) : new Color(1f, 0.38f, 0.29f, 0.65f);
        }

        private GameObject CreateSprinklerPreview()
        {
            var preview = new GameObject("Sprinkler Placement Preview");
            preview.transform.SetParent(transform, false);
            AddPreviewTile(preview.transform, Vector2.zero);
            foreach (var offset in FarmUpgradeRules.SprinklerOffsets) AddPreviewTile(preview.transform, offset);
            return preview;
        }

        private static void AddPreviewTile(Transform parent, Vector2 offset)
        {
            var marker = PrototypeSpriteFactory.CreateBlock("Sprinkler Coverage", Vector2.zero, Vector2.one * 0.84f,
                new Color(0.37f, 0.79f, 0.92f, 0.4f), parent);
            marker.transform.localPosition = offset;
            marker.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }

        public bool TryPlace(Vector2 target)
        {
            if (session == null || !session.SelectedDecoration.HasValue) return false;
            var cell = ground.WorldToCell(target);
            var center = (Vector2)ground.GetCellCenterWorld(cell);
            if (!CanPlace(center, cell, session.SelectedDecoration.Value))
            {
                session.ShowMessage(session.SelectedDecoration == GameItem.Sprinkler
                    ? "농장의 비어 있는 잔디 위에 스프링클러를 놓아 주세요."
                    : "비어 있는 잔디 위에 설치해 주세요.");
                return false;
            }

            return session.PlaceDecoration(center);
        }

        private bool CanPlace(Vector2 center, Vector3Int cell, GameItem item)
        {
            if (!ground.HasTile(cell) || road.HasTile(cell)) return false;
            if (item == GameItem.Sprinkler && (center.y >= -5f || FarmFieldLayout.HasPlotNear(session.Data.farmPlots, center)))
                return false;

            foreach (var hit in Physics2D.OverlapBoxAll(center, GetFootprint(item), 0f))
            {
                if (hit.GetComponent<PrototypePlayerController>() != null) continue;
                var renderer = hit.GetComponent<SpriteRenderer>();
                if (hit.isTrigger && renderer != null && renderer.color.a < 0.1f)
                    continue;
                return false;
            }

            return true;
        }

        private void Synchronize()
        {
            if (decorationRoot == null || session?.Data?.placedDecorations == null) return;
            var decorations = session.Data.placedDecorations;
            if (renderedDecorations.Count == decorations.Count)
            {
                var unchanged = true;
                for (var index = 0; index < decorations.Count; index++)
                {
                    var current = decorations[index];
                    var previous = renderedDecorations[index];
                    if (current.item == previous.item && current.worldX == previous.worldX && current.worldY == previous.worldY)
                        continue;
                    unchanged = false;
                    break;
                }
                if (unchanged) return;
            }

            for (var index = decorationRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(decorationRoot.GetChild(index).gameObject);
            }

            renderedDecorations.Clear();
            for (var index = 0; index < decorations.Count; index++)
            {
                var data = decorations[index];
                renderedDecorations.Add(new PlacedDecorationData
                {
                    item = data.item,
                    worldX = data.worldX,
                    worldY = data.worldY
                });
                var decoration = new GameObject($"{ItemCatalog.GetLabel(data.item)} {index + 1}");
                decoration.transform.SetParent(decorationRoot);
                decoration.transform.position = new Vector2(data.worldX, data.worldY);
                decoration.AddComponent<SpriteRenderer>().sprite = data.item == GameItem.Sprinkler
                    ? FarmEquipmentArt.Sprinkler : TownDecorationArt.GetSprite(data.item);
                decoration.AddComponent<WorldDepthSorter>().Configure(3);
                var collider = decoration.AddComponent<BoxCollider2D>();
                collider.size = GetFootprint(data.item);
                collider.isTrigger = true;
                decoration.AddComponent<TownDecorationInteractable>().Configure(index, data.item);
            }
        }

        private static Vector2 GetFootprint(GameItem item)
        {
            return item switch
            {
                GameItem.TownBench => new Vector2(1.25f, 0.55f),
                GameItem.StreetLamp => new Vector2(0.55f, 0.55f),
                GameItem.Sprinkler => new Vector2(0.55f, 0.55f),
                _ => new Vector2(0.7f, 0.55f)
            };
        }
    }
}
