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
        private static readonly BoundsInt IslandGroundBounds = new(-18, -16, 0, 37, 35, 1);
        private static readonly BoundsInt IslandOceanBounds = new(-28, -26, 0, 57, 55, 1);
        private Tilemap groundTilemap;
        private Tilemap oceanTilemap;
        private SpriteRenderer waterRenderer;

        public void Build(GameSession session)
        {
            BuildTilemaps();
            BuildFarm(session);
            BuildMainStreetTown(session.Data.worldSeed);
            BuildWeeds(session);
            BuildMiningArea(session);
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

            oceanTilemap = CreateTilemap(gridObject.transform, "Island Ocean", -10);
            groundTilemap = CreateTilemap(gridObject.transform, "Seasonal Ground", 0);
            var roadTilemap = CreateTilemap(gridObject.transform, "Main Street", 1);
            FillRectangle(oceanTilemap, IslandOceanBounds, WorldArtCatalog.GetTile(WorldTileKind.Ocean));
            FillRectangle(groundTilemap, IslandGroundBounds, WorldArtCatalog.GetTile(WorldTileKind.Grass));

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
            CreateDepthSortedStructureArt("Chicken Coop", WorldArtKind.ChickenCoop, new Vector2(-6.3f, -9.6f), farmRoot, new Vector2(3.1f, 0.85f), new Vector2(0f, 1.05f));

            var entrance = CreateInteractable<DoorInteractable>("Farm House Door", new Vector2(6f, -12.25f), new Vector2(1.1f, 0.55f), Color.clear, farmRoot);
            entrance.Configure(new Vector2(40f, -11.2f), "집에 들어가기", "농장 집 안으로 들어왔습니다.");
            CreateAnimalHerd(
                "Chicken Herd",
                session,
                FarmAnimalKind.Chicken,
                new WanderArea(new Vector2(-6.2f, -12.35f), new Vector2(1.8f, 0.9f)),
                0.7f,
                101,
                farmRoot);
            CreateAnimalHerd(
                "Cow Herd",
                session,
                FarmAnimalKind.Cow,
                new WanderArea(new Vector2(-9.8f, -11.2f), new Vector2(1.4f, 1.25f)),
                0.42f,
                202,
                farmRoot);

            for (var index = 0; index < session.Data.farmPlots.Count; index++)
            {
                var plotData = session.Data.farmPlots[index];
                var plot = PrototypeSpriteFactory.CreateBlock(
                    $"Wheat Plot {index + 1}",
                    FarmFieldLayout.GetWorldPosition(plotData.gridX, plotData.gridY),
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
            var villageSquare = CreateDepthSortedStructureArt("Village Square", WorldArtKind.VillageSquare, new Vector2(0f, 0.5f), townRoot);
            CreateVillageSquareBlockers(villageSquare.transform);

            CreateNoticeEntrance("Town Hall Entrance", new Vector2(0f, 10.55f), "시청에 들어가기", "시청 내부는 아직 준비 중입니다.", townRoot);
            var miningShop = CreateInteractable<ShopInteractable>("General Store Entrance", new Vector2(-5.8f, 6.25f), new Vector2(1.2f, 0.55f), Color.clear, townRoot);
            miningShop.Configure(ShopKind.Mining);
            var farmShop = CreateInteractable<ShopInteractable>("Seed And Feed Shop Entrance", new Vector2(5.8f, 6.25f), new Vector2(1.2f, 0.55f), Color.clear, townRoot);
            farmShop.Configure(ShopKind.SeedsAndAnimals);
            CreateArtInteractable<SellBoxInteractable>("Selling Box", WorldArtKind.SellingBox, new Vector2(9.35f, 6.05f), new Vector2(1.2f, 0.85f), townRoot);
            CreateNoticeEntrance("West Home Entrance", new Vector2(-11.5f, 7.75f), "서쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            CreateNoticeEntrance("East Home Entrance", new Vector2(11.5f, 7.75f), "동쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            var board = CreateInteractable<DialogueInteractable>("Quest Board", new Vector2(0f, 4.2f), new Vector2(1.4f, 1f), Color.clear, townRoot);
            board.Configure("광장 게시판", "초보 농부 퀘스트 3개가 준비되어 있습니다. 휴대폰에서 전체 진행도를 확인하세요.");
            var villageRoutes = CreateVillageRouteNetwork();

            var guide = CreateNpc("Guide", new Vector2(3f, 3.5f), new Color(0.8f, 0.58f, 0.25f), townRoot);
            ApplyNpcArt(guide, 0);
            guide.Configure("안내자 하람", new[]
            {
                "I로 가방을 열어 씨앗과 도구를 고르고, E로 땅을 갈거나 광맥을 캐세요. P는 휴대폰입니다.",
                "호미로 땅을 간 뒤 씨앗을 들고 심어 보세요. 마지막에는 물뿌리개로 물을 주면 됩니다.",
                "씨앗은 계절에 맞아야 잘 자라요. 상점에서 지금 계절의 씨앗을 확인해 보세요.",
                "작물은 하루아침에 자라지 않아요. 물을 준 뒤 집에서 자고 다음 날 다시 살펴보세요.",
                "판매 상자는 씨앗·동물 상점 오른쪽에 있어요. 수확물을 들고 멀리 돌아갈 필요가 없답니다.",
                "광산에서는 곡괭이가 필요해요. 먼저 서쪽 광물 상점에서 준비하는 게 좋습니다."
            }, 0);
            ConfigureNpcMovement(guide, villageRoutes, 6, 0.72f, seed + 11, new Vector2(0f, 9.5f), "안내자 하람");
            var elder = CreateNpc("Elder", new Vector2(-7f, 5f), new Color(0.55f, 0.38f, 0.42f), townRoot);
            ApplyNpcArt(elder, 2);
            elder.Configure("주민 순자", new[]
            {
                "계절마다 나무와 들판의 색이 달라진답니다.",
                "봄에는 감자와 딸기가 잘 자라요. 밭을 너무 빽빽하게만 쓰지 마세요.",
                "여름 햇볕에는 토마토와 옥수수가 힘을 내지요.",
                "가을 호박은 기다린 만큼 든든한 수확을 안겨 준답니다.",
                "겨울에도 밀은 심을 수 있어요. 빈 밭이라고 너무 걱정하지 마세요.",
                "동물들은 매일 살펴봐 주면 좋은 선물을 건네곤 한답니다."
            }, 2);
            ConfigureNpcMovement(elder, villageRoutes, 12, 0.6f, seed + 22, new Vector2(-11.5f, 7.2f), "주민 순자");
            var villager = CreateNpc("Young Villager", new Vector2(8f, 5f), new Color(0.35f, 0.48f, 0.62f), townRoot);
            ApplyNpcArt(villager, 3);
            villager.Configure("주민 도윤", new[]
            {
                "작물, 축산물, 광석은 상점 옆 판매 상자에 한꺼번에 넣을 수 있어요.",
                "판매 상자는 씨앗·동물 상점 오른쪽에 있어요. 이제 밭 주변이 훨씬 넓어졌죠?",
                "돈이 모이면 닭부터 키워 보세요. 달걀은 꾸준히 팔기 좋아요.",
                "광석도 판매할 수 있지만 필요한 물건을 만들 날을 위해 조금 남겨 두는 것도 좋아요.",
                "가방에서 씨앗 종류를 바꿀 수 있어요. 계절 표시를 꼭 확인하세요.",
                "마을 사람들에게 자주 말을 걸어 보세요. 그날그날 다른 이야기를 들을 수 있을 거예요."
            }, 3);
            ConfigureNpcMovement(villager, villageRoutes, 13, 0.75f, seed + 33, new Vector2(11.5f, 7.2f), "주민 도윤");

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
                if (!WorldPlacementRules.CanPlaceTree(new Vector2(x, y)))
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

        private void BuildMiningArea(GameSession session)
        {
            var miningRoot = new GameObject("Mining Area").transform;
            miningRoot.SetParent(transform);
            var positions = new[]
            {
                new Vector2(-15f, -11.5f),
                new Vector2(-13.2f, -11.8f),
                new Vector2(-11.5f, -10.2f),
                new Vector2(-15.3f, -8.8f),
                new Vector2(-13.4f, -8.4f),
                new Vector2(-11.2f, -7.8f)
            };

            for (var index = 0; index < session.Data.miningNodes.Count && index < positions.Length; index++)
            {
                var rock = CreateArtInteractable<MiningNodeInteractable>(
                    $"Mining Node {index + 1}",
                    WorldArtKind.MiningRock,
                    positions[index],
                    new Vector2(1.1f, 0.9f),
                    miningRoot);
                rock.Configure(index);
            }
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
            if (oceanTilemap != null)
            {
                oceanTilemap.color = Color.Lerp(Color.white, palette.water, 0.25f);
            }
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
                Season.Spring => (new Color(1f, 0.94f, 0.96f), new Color(1f, 0.82f, 0.9f), new Color(0.82f, 0.96f, 1f)),
                Season.Summer => (new Color(0.78f, 1f, 0.68f), new Color(0.62f, 0.94f, 0.52f), new Color(0.72f, 0.94f, 1f)),
                Season.Autumn => (new Color(1f, 0.72f, 0.38f), new Color(1f, 0.46f, 0.16f), new Color(0.86f, 0.9f, 1f)),
                Season.Winter => (new Color(0.8f, 0.9f, 1f), new Color(0.66f, 0.78f, 0.9f), new Color(0.78f, 0.92f, 1f)),
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
            renderer.sortingOrder = sortingOrder >= 3
                ? WorldDepthSorter.CalculateOrder(position.y, sortingOrder - 3)
                : sortingOrder;

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
            var building = CreateDepthSortedStructureArt(
                name,
                artKind,
                position,
                parent);
            var sideWidth = (footprintWidth - doorWidth) * 0.5f;
            var sideOffset = (doorWidth + sideWidth) * 0.5f;
            CreateSolidCollider($"{name} Wall Left", building.transform, new Vector2(-sideOffset, 0.4f), new Vector2(sideWidth, 0.8f));
            CreateSolidCollider($"{name} Wall Right", building.transform, new Vector2(sideOffset, 0.4f), new Vector2(sideWidth, 0.8f));
            CreateSolidCollider($"{name} Door Blocker", building.transform, new Vector2(0f, 0.4f), new Vector2(doorWidth, 0.8f));
            return building;
        }

        private static GameObject CreateDepthSortedStructureArt(
            string name,
            WorldArtKind artKind,
            Vector2 position,
            Transform parent,
            Vector2? colliderSize = null,
            Vector2? colliderOffset = null)
        {
            return CreateWorldArt(
                name,
                artKind,
                position,
                parent,
                3 + WorldDepthSorter.BuildingDepthOffset,
                colliderSize,
                colliderOffset);
        }

        private static void CreateSolidCollider(string name, Transform parent, Vector2 localPosition, Vector2 size)
        {
            var blocker = new GameObject(name);
            blocker.transform.SetParent(parent, false);
            blocker.transform.localPosition = localPosition;
            blocker.AddComponent<BoxCollider2D>().size = size;
        }

        private static void CreateVillageSquareBlockers(Transform square)
        {
            // The square artwork contains solid furniture. Keep the open stone paths walkable,
            // while preventing characters from clipping through the noticeboard and benches.
            CreateSolidCollider("Village Square Noticeboard Blocker", square, new Vector2(0f, 3.15f), new Vector2(2.15f, 0.72f));
            CreateSolidCollider("Village Square Left Bench Blocker", square, new Vector2(-1.65f, 1.55f), new Vector2(1.2f, 0.48f));
            CreateSolidCollider("Village Square Right Bench Blocker", square, new Vector2(1.65f, 1.55f), new Vector2(1.2f, 0.48f));
            CreateSolidCollider("Village Square Flower Bed Blocker", square, new Vector2(0f, 1.45f), new Vector2(0.9f, 0.85f));
            CreateSolidCollider("Village Square Upper Left Planter Blocker", square, new Vector2(-1.35f, 2.45f), new Vector2(0.82f, 0.56f));
            CreateSolidCollider("Village Square Upper Right Planter Blocker", square, new Vector2(1.35f, 2.45f), new Vector2(0.82f, 0.56f));
            CreateSolidCollider("Village Square Lower Left Planter Blocker", square, new Vector2(-1.55f, 0.35f), new Vector2(0.95f, 0.58f));
            CreateSolidCollider("Village Square Lower Right Planter Blocker", square, new Vector2(1.55f, 0.35f), new Vector2(0.95f, 0.58f));
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

        private static void CreateAnimalHerd(
            string name,
            GameSession session,
            FarmAnimalKind kind,
            WanderArea area,
            float speed,
            int separationGroup,
            Transform parent)
        {
            var herdObject = new GameObject(name);
            herdObject.transform.SetParent(parent);
            herdObject.AddComponent<FarmAnimalHerd>().Configure(session, kind, area, speed, separationGroup);
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
            npc.transform.localScale = Vector3.one;
            npc.gameObject.AddComponent<WorldDepthSorter>().Configure(2);

            var interactionCollider = npc.GetComponent<BoxCollider2D>();
            if (interactionCollider != null)
            {
                interactionCollider.size = new Vector2(0.8f, 1f);
            }
        }

        private static VillageWaypointNetwork CreateVillageRouteNetwork()
        {
            var points = new[]
            {
                new Vector2(-3.6f, 3.5f),
                new Vector2(-6f, 3.5f),
                new Vector2(-10f, 5.7f),
                new Vector2(3.6f, 3.5f),
                new Vector2(6f, 3.5f),
                new Vector2(10f, 5.7f),
                new Vector2(0f, 6f),
                new Vector2(-3.6f, -0.8f),
                new Vector2(0f, -0.8f),
                new Vector2(3.6f, -0.8f),
                new Vector2(-6f, 0f),
                new Vector2(6f, 0f),
                new Vector2(-11.5f, 7.2f),
                new Vector2(11.5f, 7.2f)
            };
            var links = new[]
            {
                new[] { 1, 6, 7 },
                new[] { 0, 2, 10 },
                new[] { 1, 12 },
                new[] { 4, 6, 9 },
                new[] { 3, 5, 11 },
                new[] { 4, 13 },
                new[] { 0, 3 },
                new[] { 0, 8, 10 },
                new[] { 7, 9 },
                new[] { 3, 8, 11 },
                new[] { 1, 7 },
                new[] { 4, 9 },
                new[] { 2 },
                new[] { 5 }
            };
            return new VillageWaypointNetwork(points, links);
        }

        private static void ConfigureNpcMovement(
            DialogueInteractable npc,
            VillageWaypointNetwork routeNetwork,
            int homeNode,
            float speed,
            int seed,
            Vector2 nightAnchor,
            string displayName)
        {
            var mover = npc.gameObject.AddComponent<WanderingMover>();
            mover.Configure(new WanderArea(new Vector2(0f, 4f), new Vector2(12.5f, 4.5f)), speed, seed, 301, 0.5f, 1.8f);
            mover.ConfigureRoute(routeNetwork, homeNode);
            mover.ConfigureNightAnchor(nightAnchor);
            npc.gameObject.AddComponent<NpcSocialAgent>().Configure(displayName, seed + 901);
        }
    }
}
