using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.Interactions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame2D.World
{
    public sealed class PrototypeWorldBuilder : MonoBehaviour
    {
        private readonly List<SpriteRenderer> trees = new();
        private Tilemap groundTilemap;
        private SpriteRenderer waterRenderer;

        public void Build(GameSession session)
        {
            BuildTilemaps();
            BuildFarm(session);
            BuildMainStreetTown(session.Data.worldSeed);
            BuildBoundaries();
            session.StateChanged += RefreshSeason;
            RefreshSeason();
        }

        private void OnDestroy()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.StateChanged -= RefreshSeason;
            }
        }

        private void BuildTilemaps()
        {
            var gridObject = new GameObject("World Grid");
            gridObject.transform.SetParent(transform);
            gridObject.AddComponent<Grid>();

            groundTilemap = CreateTilemap(gridObject.transform, "Seasonal Ground", 0);
            var roadTilemap = CreateTilemap(gridObject.transform, "Main Street", 1);
            FillRectangle(groundTilemap, new BoundsInt(-18, -16, 0, 37, 35, 1), PrototypeSpriteFactory.CreateTile(Color.white));

            var roadTile = PrototypeSpriteFactory.CreateTile(new Color(0.72f, 0.57f, 0.35f));
            FillRectangle(roadTilemap, new BoundsInt(-1, -16, 0, 3, 30, 1), roadTile);
            FillRectangle(roadTilemap, new BoundsInt(-6, 2, 0, 13, 6, 1), roadTile);
            FillRectangle(roadTilemap, new BoundsInt(-10, 5, 0, 21, 2, 1), roadTile);
        }

        private void BuildFarm(GameSession session)
        {
            var farmRoot = new GameObject("Farm").transform;
            farmRoot.SetParent(transform);
            PrototypeSpriteFactory.CreateBlock("Farm House", new Vector2(6f, -10f), new Vector2(5f, 3f), new Color(0.55f, 0.28f, 0.18f), farmRoot, true);
            PrototypeSpriteFactory.CreateBlock("Chicken Coop", new Vector2(9f, -7f), new Vector2(3f, 2f), new Color(0.65f, 0.45f, 0.2f), farmRoot, true);

            CreateInteractable<BedInteractable>("Bed", new Vector2(3.2f, -11.6f), new Vector2(1.2f, 0.8f), new Color(0.55f, 0.2f, 0.25f), farmRoot);
            CreateInteractable<SellBoxInteractable>("Selling Box", new Vector2(3.5f, -7.5f), Vector2.one, new Color(0.4f, 0.2f, 0.08f), farmRoot);
            CreateInteractable<ChickenInteractable>("Chicken", new Vector2(7.2f, -7f), new Vector2(0.8f, 0.8f), new Color(0.95f, 0.86f, 0.55f), farmRoot);

            for (var index = 0; index < session.Data.farmPlots.Count; index++)
            {
                var column = index % 3;
                var row = index / 3;
                var plot = PrototypeSpriteFactory.CreateBlock(
                    $"Wheat Plot {index + 1}",
                    new Vector2(-4.5f + (column * 1.4f), -10.5f + (row * 1.4f)),
                    new Vector2(1.1f, 1.1f),
                    Color.white,
                    farmRoot,
                    true);
                plot.GetComponent<BoxCollider2D>().isTrigger = true;
                plot.AddComponent<FarmPlotView>().Configure(index);
            }
        }

        private void BuildMainStreetTown(int seed)
        {
            var townRoot = new GameObject("C Main Street Village").transform;
            townRoot.SetParent(transform);

            CreateBuilding("Town Hall", new Vector2(0f, 12f), new Vector2(5f, 3f), new Color(0.7f, 0.55f, 0.32f), townRoot);
            CreateBuilding("General Store", new Vector2(-5f, 7.5f), new Vector2(4f, 3f), new Color(0.68f, 0.35f, 0.22f), townRoot);
            CreateBuilding("Seed And Feed Shop", new Vector2(5f, 7.5f), new Vector2(4f, 3f), new Color(0.4f, 0.55f, 0.28f), townRoot);
            CreateBuilding("West Home", new Vector2(-10f, 10f), new Vector2(3.5f, 2.5f), new Color(0.58f, 0.34f, 0.24f), townRoot);
            CreateBuilding("East Home", new Vector2(10f, 10f), new Vector2(3.5f, 2.5f), new Color(0.48f, 0.5f, 0.25f), townRoot);

            CreateInteractable<ShopInteractable>("Shop Counter", new Vector2(5f, 5.6f), new Vector2(1.2f, 0.5f), new Color(0.85f, 0.7f, 0.3f), townRoot);
            var board = CreateInteractable<DialogueInteractable>("Quest Board", new Vector2(0f, 4f), new Vector2(1.4f, 1f), new Color(0.45f, 0.25f, 0.12f), townRoot);
            board.Configure("광장 게시판", "밀 3개를 수확하면 100G를 드립니다. 휴대폰에서 진행도를 확인하세요.");

            var guide = CreateNpc("Guide", new Vector2(3f, 3.5f), new Color(0.8f, 0.58f, 0.25f), townRoot);
            ApplyNpcArt(guide, 0);
            guide.Configure("안내자 하람", "1·2·3번으로 도구를 고르고 E로 밭과 물건을 사용하세요. P는 휴대폰입니다.");
            var elder = CreateNpc("Elder", new Vector2(-7f, 5f), new Color(0.55f, 0.38f, 0.42f), townRoot);
            ApplyNpcArt(elder, 2);
            elder.Configure("주민 순자", "계절마다 나무와 들판의 색이 달라진답니다.");
            var villager = CreateNpc("Young Villager", new Vector2(8f, 5f), new Color(0.35f, 0.48f, 0.62f), townRoot);
            ApplyNpcArt(villager, 3);
            villager.Configure("주민 도윤", "수확한 밀과 달걀은 농장의 판매 상자에 넣어보세요.");

            var water = PrototypeSpriteFactory.CreateBlock("Pond", new Vector2(-11f, 2f), new Vector2(5f, 3f), Color.cyan, townRoot, true);
            waterRenderer = water.GetComponent<SpriteRenderer>();
            BuildRandomTrees(seed, townRoot);
        }

        private void BuildRandomTrees(int seed, Transform parent)
        {
            var random = new System.Random(seed);
            var created = 0;
            while (created < 28)
            {
                var x = random.Next(-16, 17);
                var y = random.Next(-14, 17);
                if (!CanPlaceTree(new Vector2(x, y)))
                {
                    continue;
                }

                var tree = PrototypeSpriteFactory.CreateBlock(
                    $"Seeded Tree {created + 1}",
                    new Vector2(x, y),
                    new Vector2(1.2f, 1.6f),
                    Color.white,
                    parent,
                    true);
                trees.Add(tree.GetComponent<SpriteRenderer>());
                created++;
            }
        }

        private static bool CanPlaceTree(Vector2 position)
        {
            if (Mathf.Abs(position.x) < 3f || (position.y < -6f && position.x > -7f && position.x < 11f))
            {
                return false;
            }

            var reservedAreas = new[]
            {
                new Bounds(new Vector3(-12f, -2f), new Vector3(4f, 4f)),
                new Bounds(new Vector3(-11f, 2f), new Vector3(7f, 5f)),
                new Bounds(new Vector3(0f, 12f), new Vector3(7f, 5f)),
                new Bounds(new Vector3(-5f, 7.5f), new Vector3(6f, 5f)),
                new Bounds(new Vector3(5f, 7.5f), new Vector3(6f, 5f)),
                new Bounds(new Vector3(-10f, 10f), new Vector3(5.5f, 4.5f)),
                new Bounds(new Vector3(10f, 10f), new Vector3(5.5f, 4.5f))
            };

            foreach (var area in reservedAreas)
            {
                if (area.Contains(position))
                {
                    return false;
                }
            }

            return true;
        }

        private void BuildBoundaries()
        {
            PrototypeSpriteFactory.CreateBlock("North Boundary", new Vector2(0f, 18f), new Vector2(38f, 1f), Color.clear, transform, true);
            PrototypeSpriteFactory.CreateBlock("South Boundary", new Vector2(0f, -16f), new Vector2(38f, 1f), Color.clear, transform, true);
            PrototypeSpriteFactory.CreateBlock("West Boundary", new Vector2(-18f, 1f), new Vector2(1f, 35f), Color.clear, transform, true);
            PrototypeSpriteFactory.CreateBlock("East Boundary", new Vector2(18f, 1f), new Vector2(1f, 35f), Color.clear, transform, true);
        }

        private void RefreshSeason()
        {
            if (groundTilemap == null || GameSession.Instance == null)
            {
                return;
            }

            var palette = GetSeasonPalette(GameSession.Instance.CurrentSeason);
            groundTilemap.color = palette.ground;
            if (waterRenderer != null)
            {
                waterRenderer.color = palette.water;
            }

            foreach (var tree in trees)
            {
                tree.color = palette.trees;
            }
        }

        private static (Color ground, Color trees, Color water) GetSeasonPalette(Season season)
        {
            return season switch
            {
                Season.Spring => (new Color(0.62f, 0.78f, 0.45f), new Color(0.38f, 0.65f, 0.35f), new Color(0.3f, 0.65f, 0.82f)),
                Season.Summer => (new Color(0.42f, 0.68f, 0.3f), new Color(0.2f, 0.5f, 0.22f), new Color(0.22f, 0.58f, 0.78f)),
                Season.Autumn => (new Color(0.72f, 0.58f, 0.3f), new Color(0.75f, 0.35f, 0.16f), new Color(0.28f, 0.55f, 0.7f)),
                Season.Winter => (new Color(0.82f, 0.88f, 0.9f), new Color(0.62f, 0.72f, 0.72f), new Color(0.65f, 0.82f, 0.9f)),
                _ => (Color.white, Color.green, Color.cyan)
            };
        }

        private static Tilemap CreateTilemap(Transform grid, string name, int sortingOrder)
        {
            var mapObject = new GameObject(name);
            mapObject.transform.SetParent(grid);
            var tilemap = mapObject.AddComponent<Tilemap>();
            var renderer = mapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        private static void FillRectangle(Tilemap tilemap, BoundsInt bounds, TileBase tile)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                tilemap.SetTile(position, tile);
            }
        }

        private static void CreateBuilding(string name, Vector2 position, Vector2 size, Color color, Transform parent)
        {
            var building = PrototypeSpriteFactory.CreateBlock(name, position, size, color, parent, true);
            building.GetComponent<SpriteRenderer>().sortingOrder = 3;
        }

        private static T CreateInteractable<T>(string name, Vector2 position, Vector2 size, Color color, Transform parent)
            where T : InteractableBehaviour
        {
            var target = PrototypeSpriteFactory.CreateBlock(name, position, size, color, parent, true);
            target.GetComponent<BoxCollider2D>().isTrigger = true;
            target.GetComponent<SpriteRenderer>().sortingOrder = 4;
            return target.AddComponent<T>();
        }

        private static DialogueInteractable CreateNpc(string name, Vector2 position, Color color, Transform parent)
        {
            return CreateInteractable<DialogueInteractable>(name, position, new Vector2(0.8f, 1f), color, parent);
        }

        private static void ApplyNpcArt(DialogueInteractable npc, int characterRowFromTop)
        {
            var renderer = npc.GetComponent<SpriteRenderer>();
            var artSprite = GameArtCatalog.GetNpcSprite(characterRowFromTop);
            if (artSprite == null)
            {
                return;
            }

            renderer.sprite = artSprite;
            renderer.color = Color.white;
        }
    }
}
