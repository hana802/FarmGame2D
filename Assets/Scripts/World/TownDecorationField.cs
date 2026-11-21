using System.Collections.Generic;
using FarmGame2D.Core;
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

        public bool TryPlace(Vector2 target)
        {
            if (session == null || !session.SelectedDecoration.HasValue) return false;
            var cell = ground.WorldToCell(target);
            var center = (Vector2)ground.GetCellCenterWorld(cell);
            if (!CanPlace(center, cell, session.SelectedDecoration.Value))
            {
                session.ShowMessage("비어 있는 잔디 위에 설치해 주세요.");
                return false;
            }

            return session.PlaceDecoration(center);
        }

        private bool CanPlace(Vector2 center, Vector3Int cell, GameItem item)
        {
            if (!ground.HasTile(cell) || road.HasTile(cell)) return false;

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
                decoration.AddComponent<SpriteRenderer>().sprite = TownDecorationArt.GetSprite(data.item);
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
                _ => new Vector2(0.7f, 0.55f)
            };
        }
    }
}
