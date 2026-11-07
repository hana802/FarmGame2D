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
            BuildWeeds(session);
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
            FillRectangle(groundTilemap, new BoundsInt(-18, -16, 0, 37, 35, 1), WorldArtCatalog.GetTile(WorldTileKind.Grass));

            var roadTile = WorldArtCatalog.GetTile(WorldTileKind.StonePath);
            FillRectangle(roadTilemap, new BoundsInt(-1, -16, 0, 3, 30, 1), roadTile);
            FillRectangle(roadTilemap, new BoundsInt(-6, 2, 0, 13, 6, 1), roadTile);
            FillRectangle(roadTilemap, new BoundsInt(-10, 5, 0, 21, 2, 1), roadTile);
        }

        private void BuildFarm(GameSession session)
        {
            var farmRoot = new GameObject("Farm").transform;
            farmRoot.SetParent(transform);
            CreateBuildingArt("Farm House", WorldArtKind.FarmHouse, new Vector2(6f, -12f), farmRoot, 4.6f, 1.1f);
            CreateWorldArt("Chicken Coop", WorldArtKind.ChickenCoop, new Vector2(-6.3f, -9.6f), farmRoot, 3, new Vector2(3.1f, 0.85f), new Vector2(0f, 1.05f));

            var entrance = CreateInteractable<DoorInteractable>("Farm House Door", new Vector2(6f, -12.25f), new Vector2(1.1f, 0.55f), Color.clear, farmRoot);
            entrance.Configure(new Vector2(40f, -11.2f), "집에 들어가기", "농장 집 안으로 들어왔습니다.");
            CreateInteractable<SellBoxInteractable>("Selling Box", new Vector2(3.5f, -7.5f), Vector2.one, new Color(0.4f, 0.2f, 0.08f), farmRoot);
            CreateArtInteractable<ChickenInteractable>("Chicken", WorldArtKind.Chicken, new Vector2(-6.1f, -12.4f), new Vector2(0.8f, 0.8f), farmRoot);

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

            BuildFarmHouseInterior();
        }

        private void BuildFarmHouseInterior()
        {
            var interior = new GameObject("Farm House Interior").transform;
            interior.SetParent(transform);
            PrototypeSpriteFactory.CreateBlock("Interior Floor", new Vector2(40f, -9.5f), new Vector2(8f, 6f), new Color(0.77f, 0.62f, 0.42f), interior);
            CreateWorldArt("Interior Rug Art", WorldArtKind.InteriorRug, new Vector2(40f, -9.6f), interior, 2);
            PrototypeSpriteFactory.CreateBlock("Interior Top Wall", new Vector2(40f, -6.45f), new Vector2(8.4f, 0.5f), new Color(0.34f, 0.2f, 0.14f), interior, true);
            PrototypeSpriteFactory.CreateBlock("Interior Left Wall", new Vector2(35.8f, -9.5f), new Vector2(0.5f, 6.5f), new Color(0.34f, 0.2f, 0.14f), interior, true);
            PrototypeSpriteFactory.CreateBlock("Interior Right Wall", new Vector2(44.2f, -9.5f), new Vector2(0.5f, 6.5f), new Color(0.34f, 0.2f, 0.14f), interior, true);
            PrototypeSpriteFactory.CreateBlock("Interior Bottom Wall Left", new Vector2(37.8f, -12.55f), new Vector2(3.9f, 0.5f), new Color(0.34f, 0.2f, 0.14f), interior, true);
            PrototypeSpriteFactory.CreateBlock("Interior Bottom Wall Right", new Vector2(42.2f, -12.55f), new Vector2(3.9f, 0.5f), new Color(0.34f, 0.2f, 0.14f), interior, true);

            CreateArtInteractable<BedInteractable>("Interior Bed Art", WorldArtKind.InteriorBed, new Vector2(37.4f, -8.6f), new Vector2(1.3f, 2.2f), interior);
            CreateWorldArt("Interior Table", WorldArtKind.InteriorTable, new Vector2(40.6f, -8.8f), interior, 4, new Vector2(1.7f, 0.8f), new Vector2(0f, 0.35f));
            CreateWorldArt("Interior Cabinet", WorldArtKind.InteriorCabinet, new Vector2(42.4f, -7.2f), interior, 3, new Vector2(1.3f, 0.65f), new Vector2(0f, 0.6f));
            var exit = CreateInteractable<DoorInteractable>("Farm House Exit", new Vector2(40f, -12.2f), new Vector2(1.1f, 0.6f), new Color(0.5f, 0.28f, 0.13f), interior);
            exit.Configure(new Vector2(6f, -13f), "밖으로 나가기", "농장으로 나왔습니다.");
        }

        private void BuildMainStreetTown(int seed)
        {
            var townRoot = new GameObject("C Main Street Village").transform;
            townRoot.SetParent(transform);

            CreateBuildingArt("Town Hall", WorldArtKind.TownHall, new Vector2(0f, 11f), townRoot, 5.2f, 1.1f);
            CreateBuildingArt("General Store", WorldArtKind.GeneralStore, new Vector2(-5.8f, 6.7f), townRoot, 4.4f, 1f);
            CreateBuildingArt("Seed And Feed Shop", WorldArtKind.SeedFeedShop, new Vector2(5.8f, 6.7f), townRoot, 4.2f, 1f);
            CreateBuildingArt("West Home", WorldArtKind.WestCottage, new Vector2(-11.5f, 8.2f), townRoot, 3.7f, 0.9f);
            CreateBuildingArt("East Home", WorldArtKind.EastCottage, new Vector2(11.5f, 8.2f), townRoot, 3.7f, 0.9f);
            CreateWorldArt("Village Square", WorldArtKind.VillageSquare, new Vector2(0f, 0.5f), townRoot, 2);

            CreateNoticeEntrance("Town Hall Entrance", new Vector2(0f, 10.55f), "시청에 들어가기", "시청 내부는 아직 준비 중입니다.", townRoot);
            CreateNoticeEntrance("General Store Entrance", new Vector2(-5.8f, 6.25f), "잡화점에 들어가기", "잡화점 내부는 아직 준비 중입니다.", townRoot);
            CreateInteractable<ShopInteractable>("Seed And Feed Shop Entrance", new Vector2(5.8f, 6.25f), new Vector2(1.2f, 0.55f), Color.clear, townRoot);
            CreateNoticeEntrance("West Home Entrance", new Vector2(-11.5f, 7.75f), "서쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            CreateNoticeEntrance("East Home Entrance", new Vector2(11.5f, 7.75f), "동쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            var board = CreateInteractable<DialogueInteractable>("Quest Board", new Vector2(0f, 4.2f), new Vector2(1.4f, 1f), Color.clear, townRoot);
            board.Configure("광장 게시판", "밀 3개를 수확하면 100G를 드립니다. 휴대폰에서 진행도를 확인하세요.");

            var guide = CreateNpc("Guide", new Vector2(3f, 3.5f), new Color(0.8f, 0.58f, 0.25f), townRoot);
            ApplyNpcArt(guide, 0);
            guide.Configure("안내자 하람", "I로 가방을 열어 도구를 꺼내고, E로 밭과 물건을 사용하세요. P는 휴대폰입니다.", 0);
            var elder = CreateNpc("Elder", new Vector2(-7f, 5f), new Color(0.55f, 0.38f, 0.42f), townRoot);
            ApplyNpcArt(elder, 2);
            elder.Configure("주민 순자", "계절마다 나무와 들판의 색이 달라진답니다.", 2);
            var villager = CreateNpc("Young Villager", new Vector2(8f, 5f), new Color(0.35f, 0.48f, 0.62f), townRoot);
            ApplyNpcArt(villager, 3);
            villager.Configure("주민 도윤", "수확한 밀과 달걀은 농장의 판매 상자에 넣어보세요.", 3);

            var water = CreateWorldArt("Pond", WorldArtKind.Pond, new Vector2(-11.5f, 1.3f), townRoot, 2, new Vector2(4.5f, 2.4f));
            waterRenderer = water.GetComponent<SpriteRenderer>();
            BuildRandomTrees(seed, townRoot);
        }

        private void BuildRandomTrees(int seed, Transform parent)
        {
            var random = new System.Random(seed);
            var created = 0;
            while (created < 18)
            {
                var x = random.Next(-16, 17);
                var y = random.Next(-14, 17);
                if (!CanPlaceTree(new Vector2(x, y)))
                {
                    continue;
                }

                var tree = CreateWorldArt(
                    $"Seeded Tree {created + 1}",
                    WorldArtKind.Tree,
                    new Vector2(x, y),
                    parent,
                    3,
                    new Vector2(1.35f, 0.7f),
                    new Vector2(0f, 0.45f));
                trees.Add(tree.GetComponent<SpriteRenderer>());
                created++;
            }
        }

        private void BuildWeeds(GameSession session)
        {
            var weedRoot = new GameObject("Pullable Weeds").transform;
            weedRoot.SetParent(transform);
            var positions = new[]
            {
                new Vector2(2.5f, -9.5f),
                new Vector2(-10f, -5.5f),
                new Vector2(13.5f, -10.5f),
                new Vector2(14f, -3.5f),
                new Vector2(-14.5f, 12f),
                new Vector2(14.5f, 14f),
                new Vector2(-15f, 3f),
                new Vector2(12.5f, 2f)
            };

            for (var index = 0; index < positions.Length; index++)
            {
                if (session.Data.pulledWeedIds != null && session.Data.pulledWeedIds.Contains(index))
                {
                    continue;
                }

                var weed = CreateArtInteractable<WeedInteractable>(
                    $"Weed {index + 1}",
                    WorldArtKind.Weed,
                    positions[index],
                    new Vector2(0.75f, 0.65f),
                    weedRoot);
                weed.Configure(index);
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
                new Bounds(new Vector3(0f, 1.2f), new Vector3(10f, 9f)),
                new Bounds(new Vector3(0f, 11f), new Vector3(8f, 6f)),
                new Bounds(new Vector3(-5.8f, 6.7f), new Vector3(6f, 5f)),
                new Bounds(new Vector3(5.8f, 6.7f), new Vector3(6f, 5f)),
                new Bounds(new Vector3(-11.5f, 8.2f), new Vector3(5.5f, 5f)),
                new Bounds(new Vector3(11.5f, 8.2f), new Vector3(5.5f, 5f))
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
                Season.Spring => (Color.white, Color.white, new Color(0.9f, 1f, 1f)),
                Season.Summer => (new Color(0.9f, 1f, 0.88f), new Color(0.82f, 0.98f, 0.78f), new Color(0.85f, 0.97f, 1f)),
                Season.Autumn => (new Color(1f, 0.82f, 0.58f), new Color(1f, 0.68f, 0.36f), new Color(0.9f, 0.94f, 1f)),
                Season.Winter => (new Color(0.9f, 0.95f, 1f), new Color(0.78f, 0.9f, 1f), new Color(0.85f, 0.95f, 1f)),
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

        private static GameObject CreateWorldArt(
            string name,
            WorldArtKind artKind,
            Vector2 position,
            Transform parent,
            int sortingOrder,
            Vector2? colliderSize = null,
            Vector2? colliderOffset = null)
        {
            var target = new GameObject(name);
            target.transform.SetParent(parent);
            target.transform.position = position;
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = WorldArtCatalog.GetSprite(artKind);
            renderer.sortingOrder = sortingOrder;

            if (colliderSize.HasValue)
            {
                var collider = target.AddComponent<BoxCollider2D>();
                collider.size = colliderSize.Value;
                collider.offset = colliderOffset ?? Vector2.zero;
            }

            return target;
        }

        private static GameObject CreateBuildingArt(
            string name,
            WorldArtKind artKind,
            Vector2 position,
            Transform parent,
            float footprintWidth,
            float doorWidth)
        {
            var building = CreateWorldArt(name, artKind, position, parent, 3);
            var sideWidth = (footprintWidth - doorWidth) * 0.5f;
            var sideOffset = (doorWidth + sideWidth) * 0.5f;
            CreateSolidCollider($"{name} Wall Left", building.transform, new Vector2(-sideOffset, 0.4f), new Vector2(sideWidth, 0.8f));
            CreateSolidCollider($"{name} Wall Right", building.transform, new Vector2(sideOffset, 0.4f), new Vector2(sideWidth, 0.8f));
            CreateSolidCollider($"{name} Door Blocker", building.transform, new Vector2(0f, 0.4f), new Vector2(doorWidth, 0.8f));
            return building;
        }

        private static void CreateSolidCollider(string name, Transform parent, Vector2 localPosition, Vector2 size)
        {
            var blocker = new GameObject(name);
            blocker.transform.SetParent(parent, false);
            blocker.transform.localPosition = localPosition;
            blocker.AddComponent<BoxCollider2D>().size = size;
        }

        private static void CreateNoticeEntrance(
            string name,
            Vector2 position,
            string prompt,
            string message,
            Transform parent)
        {
            var entrance = CreateInteractable<NoticeInteractable>(name, position, new Vector2(1.2f, 0.55f), Color.clear, parent);
            entrance.Configure(prompt, message);
        }

        private static T CreateArtInteractable<T>(
            string name,
            WorldArtKind artKind,
            Vector2 position,
            Vector2 size,
            Transform parent)
            where T : InteractableBehaviour
        {
            var target = CreateWorldArt(name, artKind, position, parent, 4, size);
            target.GetComponent<BoxCollider2D>().isTrigger = true;
            return target.AddComponent<T>();
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
