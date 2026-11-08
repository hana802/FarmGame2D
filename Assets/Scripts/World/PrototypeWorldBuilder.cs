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
        private Tilemap roadTilemap;
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
            roadTilemap = CreateTilemap(gridObject.transform, "Main Street", 1);
            FillRectangle(oceanTilemap, IslandOceanBounds, WorldArtCatalog.GetTile(WorldTileKind.Ocean));
            FillRectangle(groundTilemap, IslandGroundBounds, WorldArtCatalog.GetTile(WorldTileKind.Grass));
            oceanTilemap.gameObject.AddComponent<OceanMotionController>();

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

            var freeformField = new GameObject("Freeform Farm Field").AddComponent<FreeformFarmField>();
            freeformField.transform.SetParent(farmRoot);
            freeformField.Configure(session, groundTilemap, roadTilemap, farmRoot);

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
            var villageSquare = CreateWorldArt("Village Square", WorldArtKind.VillageSquare, new Vector2(0f, 0.5f), townRoot, 2);
            CreateVillageSquareNoticeboardArt(villageSquare.transform);
            CreateVillageSquareBlockers(villageSquare.transform);

            CreateNoticeEntrance("Town Hall Entrance", new Vector2(0f, 10.55f), "시청에 들어가기", "시청 내부는 아직 준비 중입니다.", townRoot);
            var miningShop = CreateInteractable<ShopInteractable>("General Store Entrance", new Vector2(-5.8f, 6.25f), new Vector2(1.2f, 0.55f), Color.clear, townRoot);
            miningShop.Configure(ShopKind.Mining);
            var farmShop = CreateInteractable<ShopInteractable>("Seed And Feed Shop Entrance", new Vector2(5.8f, 6.25f), new Vector2(1.2f, 0.55f), Color.clear, townRoot);
            farmShop.Configure(ShopKind.SeedsAndAnimals);
            CreateArtInteractable<SellBoxInteractable>("Selling Box", WorldArtKind.SellingBox, new Vector2(9.35f, 6.05f), new Vector2(1.2f, 0.85f), townRoot);
            CreateNoticeEntrance("West Home Entrance", new Vector2(-11.5f, 7.75f), "서쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            CreateNoticeEntrance("East Home Entrance", new Vector2(11.5f, 7.75f), "동쪽 집에 들어가기", "문이 잠겨 있습니다. 집주인을 먼저 만나보세요.", townRoot);
            CreateInteractable<QuestBoardInteractable>("Quest Board", new Vector2(0f, 3.15f), new Vector2(3.2f, 1.5f), Color.clear, townRoot);
            var villageRoutes = VillageRouteMap.CreateMainStreet();

            var guide = CreateNpc("Guide", new Vector2(3f, 3.5f), new Color(0.8f, 0.58f, 0.25f), townRoot);
            ApplyNpcArt(guide, 0);
            guide.Configure("안내자 하람", new[]
            {
                "I로 가방을 열어 도구를 고르세요. 호미는 Space로 앞의 땅을 갈고, E로 작물과 사물을 다룹니다. P는 휴대폰입니다.",
                "호미로 땅을 간 뒤 씨앗을 심고 물을 주세요. 빈 밭은 삽을 들고 E를 누르면 다시 잔디로 되돌릴 수 있어요.",
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

            var fisher = CreateNpc("Fisher Minseo", new Vector2(-9f, 0f), new Color(0.3f, 0.58f, 0.72f), townRoot);
            ApplyNpcArt(fisher, 1, 4);
            fisher.Configure("어부 민서", new[]
            {
                "낚싯대를 들고 연못이나 바닷가의 낚시 자리를 찾아보세요.",
                "파도가 잔잔한 날에는 농어가 잘 잡히는 것 같아요.",
                "황금 잉어는 보기 드물지만 포기하지 않으면 만날 수 있어요.",
                "낚시는 서두르지 않는 게 중요해요. 찌가 움직일 때까지 기다려 보세요.",
                "잡은 물고기는 판매 상자에 넣어도 되고 가방에 간직해도 돼요.",
                "바닷물 색이 계절마다 조금씩 달라지는 걸 알고 있었나요?"
            }, 4);
            ConfigureNpcMovement(fisher, villageRoutes, 10, 0.68f, seed + 44, new Vector2(-11.5f, 2.5f), "어부 민서");

            var florist = CreateNpc("Florist Taeo", new Vector2(5f, 0f), new Color(0.64f, 0.45f, 0.72f), townRoot);
            ApplyNpcArt(florist, 0, 6);
            florist.Configure("원예가 태오", new[]
            {
                "마을 길과 광장은 모두가 쓰는 곳이라 밭을 만들 수 없어요.",
                "하지만 빈 잔디라면 호미로 새로운 밭을 만들 수 있답니다.",
                "동물 두 마리가 건강하게 자라면 어느 날 새끼가 태어날 수도 있어요.",
                "게시판 의뢰는 순서가 없어요. 마음에 드는 일을 먼저 골라 보세요.",
                "봄 딸기 향은 마을 광장까지 퍼지는 것 같아요.",
                "농장이 넓어질수록 물 주는 동선도 미리 생각해 두는 게 좋아요."
            }, 5);
            ConfigureNpcMovement(florist, villageRoutes, 11, 0.7f, seed + 55, new Vector2(5.8f, 5.4f), "원예가 태오");

            var baker = CreateNpc("Baker Yuna", new Vector2(-3.6f, -0.8f), new Color(0.6f, 0.55f, 0.34f), townRoot);
            ApplyNpcArt(baker, 1);
            baker.Configure("제빵사 유나", new[]
            {
                "아침마다 빵 굽는 냄새가 광장까지 퍼지면 하루가 제대로 시작된 기분이에요.",
                "밀을 수확했다면 조금 남겨 두세요. 언젠가 농장 빵을 함께 구워 보고 싶거든요.",
                "비 오는 날엔 따뜻한 빵이 더 잘 팔려요. 사람 마음이란 참 솔직하죠?",
                "순자 어르신께 새로 구운 빵을 가져다드려야 하는데, 만나면 제 안부도 전해 주세요.",
                "소라는 일하다가 끼니를 자주 잊어요. 광장에서 만나면 빵부터 챙겨 주려고요.",
                "계절 과일을 넣은 빵도 좋아해요. 봄 딸기와 가을 호박은 특히 잘 어울려요.",
                "오늘은 무슨 일을 하고 있어요? 너무 바빠도 잠깐 쉬어 가는 건 잊지 마세요.",
                "마을 게시판에 식재료 부탁을 올릴 때가 있어요. 마음이 내킬 때만 도와줘도 괜찮아요."
            }, 6);
            ConfigureNpcMovement(baker, villageRoutes, 7, 0.66f, seed + 66, new Vector2(-5.8f, 6.1f), "제빵사 유나");

            var rancher = CreateNpc("Rancher Sora", new Vector2(3.6f, -0.8f), new Color(0.68f, 0.34f, 0.28f), townRoot);
            ApplyPlayerStyleNpcArt(rancher, CharacterChoice.Female);
            rancher.Configure("목장 관리인 소라", new[]
            {
                "동물들은 생각보다 표정이 풍부해요. 매일 보다 보면 기분이 눈에 보일 정도라니까요.",
                "같은 동물이 둘 이상이면 편안한 환경에서 새끼가 태어날 수도 있어요. 서두르진 마세요.",
                "닭은 호기심이 많아서 잠깐 한눈팔면 엉뚱한 곳까지 따라와요. 그게 또 귀엽지만요.",
                "소는 덩치가 커도 아주 섬세해요. 천천히 다가가면 먼저 곁을 내어 줄 거예요.",
                "유나가 또 끼니를 챙기라며 빵을 쥐여 줬어요. 덕분에 오늘도 든든하네요.",
                "광장 길목은 동물도 사람도 함께 쓰는 곳이에요. 길을 막지 않게 제가 잘 살펴볼게요.",
                "농장 일은 어때요? 힘에 부치는 날엔 동물들 옆에서 잠깐 쉬어도 괜찮아요.",
                "새끼가 태어난 날은 꼭 알려 주세요. 제일 먼저 달려가서 건강한지 봐 줄게요."
            }, 7);
            ConfigureNpcMovement(rancher, villageRoutes, 9, 0.74f, seed + 77, new Vector2(9.3f, 5.5f), "목장 관리인 소라");

            var water = CreateWorldArt("Pond", WorldArtKind.Pond, new Vector2(-11.5f, 1.3f), townRoot, 2, new Vector2(4.5f, 2.4f));
            waterRenderer = water.GetComponent<SpriteRenderer>();
            CreateInteractable<FishingSpotInteractable>("Pond Fishing Spot", new Vector2(-11.5f, -0.15f), new Vector2(1.4f, 0.8f), Color.clear, townRoot);
            CreateInteractable<FishingSpotInteractable>("West Coast Fishing Spot", new Vector2(-17.25f, -4f), new Vector2(0.8f, 1.6f), Color.clear, townRoot);
            CreateInteractable<FishingSpotInteractable>("South Coast Fishing Spot", new Vector2(12f, -15.3f), new Vector2(1.6f, 0.8f), Color.clear, townRoot);
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
            // Decorative benches and planters remain walkable so they cannot seal the narrow
            // approach to the board. Only the noticeboard itself is a solid obstacle.
            CreateSolidCollider("Village Square Noticeboard Blocker", square, new Vector2(0f, 3.15f), new Vector2(2.15f, 0.72f));
        }

        private static void CreateVillageSquareNoticeboardArt(Transform square)
        {
            var squareSprite = square.GetComponent<SpriteRenderer>().sprite;
            var boardSprite = WorldArtCatalog.GetVillageSquareNoticeboardSprite();
            if (squareSprite == null || boardSprite == null)
            {
                return;
            }

            var squareAnchor = squareSprite.rect.position + squareSprite.pivot;
            var boardCenter = boardSprite.rect.position + boardSprite.pivot;
            var boardArt = new GameObject("Village Square Noticeboard Art");
            boardArt.transform.SetParent(square, false);
            boardArt.transform.localPosition = (boardCenter - squareAnchor) / squareSprite.pixelsPerUnit;
            var renderer = boardArt.AddComponent<SpriteRenderer>();
            renderer.sprite = boardSprite;
            renderer.sortingOrder = WorldDepthSorter.CalculateOrder(
                square.position.y + 3.15f,
                WorldDepthSorter.BuildingDepthOffset);
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

        private static void ApplyNpcArt(DialogueInteractable npc, int characterRowFromTop, int column = 0)
        {
            var artSprite = GameArtCatalog.GetNpcSprite(characterRowFromTop, column);
            ApplyCharacterArt(npc, artSprite);
        }

        private static void ApplyPlayerStyleNpcArt(DialogueInteractable npc, CharacterChoice choice)
        {
            var artSprite = GameArtCatalog.GetPlayerSprite(choice, 3, 0);
            ApplyCharacterArt(npc, artSprite);
        }

        private static void ApplyCharacterArt(DialogueInteractable npc, Sprite artSprite)
        {
            if (artSprite == null)
            {
                return;
            }

            var renderer = npc.GetComponent<SpriteRenderer>();
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
