using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed partial class PrototypeWorldBuilder
    {
        private GameObject marketRoot;
        private SpriteRenderer visitingMerchant;
        private GameObject turnipPriceBoard;
        private WanderingMover minseo;
        private WanderingMover yuna;
        private WanderingMover sora;
        private GameSession villageSession;

        private const float DockY = 2.15f;
        private const float DockStart = 17.4f;
        private const float DockEnd = 21.95f;
        private const float DockPlatformStart = 19.82f;
        private const float DockLaneHalfWidth = 0.54f;
        private const float DockPlatformHalfWidth = 1.12f;

        private void BuildVillageExpansion(GameSession session, Transform town, DialogueInteractable fisher,
            DialogueInteractable baker, DialogueInteractable rancher)
        {
            villageSession = session;
            minseo = fisher.GetComponent<WanderingMover>();
            yuna = baker.GetComponent<WanderingMover>();
            sora = rancher.GetComponent<WanderingMover>();

            var path = WorldArtCatalog.GetTile(WorldTileKind.StonePath);
            FillRectangle(roadTilemap, new BoundsInt(6, 1, 0, 13, 2, 1), path);
            FillRectangle(roadTilemap, new BoundsInt(7, 3, 0, 6, 1, 1), path);

            marketRoot = new GameObject("Weekend Market");
            marketRoot.transform.SetParent(town, false);
            BuildMarketStall("Seasonal Produce", WorldArtKind.ProduceStall, ShopKind.MarketProduce, 8.0f);
            BuildMarketStall("Weekend Bakery", WorldArtKind.BakeryStall, ShopKind.MarketBakery, 9.95f);
            BuildMarketStall("Island Specialties", WorldArtKind.SpecialtiesStall, ShopKind.MarketSpecialties, 11.9f);
            var merchant = CreateWorldArt("Weekend Merchant", WorldArtKind.WeekendMerchant,
                new Vector2(12.89f, 2.62f), town, 5, new Vector2(0.75f, 1.4f), new Vector2(0f, 0.7f));
            merchant.GetComponent<BoxCollider2D>().isTrigger = true;
            merchant.AddComponent<TurnipMarketInteractable>();
            visitingMerchant = merchant.GetComponent<SpriteRenderer>();
            turnipPriceBoard = new GameObject("Turnip Price Board");
            turnipPriceBoard.transform.SetParent(merchant.transform, false);
            turnipPriceBoard.transform.localPosition = new Vector3(0f, 0.42f);
            turnipPriceBoard.transform.localScale = Vector3.one * 0.38f;
            var boardRenderer = turnipPriceBoard.AddComponent<SpriteRenderer>();
            boardRenderer.sprite = WorldArtCatalog.GetVillageSquareNoticeboardSprite();
            boardRenderer.sortingOrder = visitingMerchant.sortingOrder;
            BuildHarbor(town);

            session.StateChanged += RefreshVillageSchedule;
            RefreshVillageSchedule();
        }

        private void BuildMarketStall(string name, WorldArtKind art, ShopKind shop, float x)
        {
            CreateDepthSortedStructureArt($"{name} Stall", art, new Vector2(x, 3.1f), marketRoot.transform,
                new Vector2(1.5f, 0.6f), new Vector2(0f, 0.27f));
            CreateInteractable<ShopInteractable>($"{name} Counter", new Vector2(x, 2.73f),
                new Vector2(0.96f, 0.45f), Color.clear, marketRoot.transform).Configure(shop);
        }

        private void BuildHarbor(Transform town)
        {
            var harbor = new GameObject("East Harbor").transform;
            harbor.SetParent(town, false);
            CreateBuildingArt("Minseo's Harbor Hut", WorldArtKind.FisherHut, new Vector2(16.55f, 4.22f),
                harbor, 2.18f, 0.85f);
            CreateInteractable<HarborDeliveryInteractable>("Harbor Delivery", new Vector2(16.55f, 3.75f),
                new Vector2(1.0f, 0.65f), Color.clear, harbor);
            CreateWorldArt("Harbor Pier", WorldArtKind.HarborPier, new Vector2(DockStart, DockY), harbor, 2);
            CreateWorldArt("Harbor Rowboat", WorldArtKind.HarborBoat, new Vector2(20.0f, 3.85f), harbor, 2);
            var fishing = CreateInteractable<FishingSpotInteractable>("Harbor Fishing Spot", new Vector2(21.48f, DockY),
                new Vector2(0.7f, 1.2f), Color.clear, harbor);
            fishing.ConfigureHarbor(new Vector2(22.7f, DockY));
        }

        private void RefreshVillageSchedule()
        {
            if (villageSession == null) return;
            var data = villageSession.Data;
            var weekend = SeasonCalendar.IsWeekend(data.day);
            if (marketRoot.activeSelf != weekend) marketRoot.SetActive(weekend);
            visitingMerchant.enabled = weekend;
            if (turnipPriceBoard.activeSelf == weekend) turnipPriceBoard.SetActive(!weekend);

            var morning = data.hour >= 6 && data.hour < 12;
            minseo.SetWorkplace(morning ? new Vector2(15.35f, 3.55f) : null);
            sora.SetWorkplace(weekend && morning ? new Vector2(7.22f, 2.62f) : null);
            yuna.SetWorkplace(weekend ? new Vector2(9.15f, 2.62f) : null);
        }

        private static bool OccupiesExpansion(Vector2 tree) =>
            (tree.x >= 6f && tree.y >= 0f && tree.y <= 4f)
            || (tree.x >= 14f && tree.y >= 4f && tree.y <= 6f);

        private void BuildEastBoundary()
        {
            var laneTop = DockY + DockLaneHalfWidth;
            var laneBottom = DockY - DockLaneHalfWidth;
            BuildDockWall("East Boundary", 17.5f, 18.5f, laneTop, 18.5f);
            BuildDockWall("East Boundary South", 17.5f, 18.5f, -16.5f, laneBottom);
            BuildDockWall("Pier Lane North", 18f, DockPlatformStart, laneTop, laneTop + 0.32f);
            BuildDockWall("Pier Lane South", 18f, DockPlatformStart, laneBottom - 0.32f, laneBottom);
            var platformTop = DockY + DockPlatformHalfWidth;
            var platformBottom = DockY - DockPlatformHalfWidth;
            BuildDockWall("Pier Platform North", DockPlatformStart, DockEnd, platformTop, platformTop + 0.3f);
            BuildDockWall("Pier Platform South", DockPlatformStart, DockEnd, platformBottom - 0.3f, platformBottom);
            BuildDockWall("Pier Platform West North", DockPlatformStart, DockPlatformStart + 0.22f, laneTop, platformTop);
            BuildDockWall("Pier Platform West South", DockPlatformStart, DockPlatformStart + 0.22f, platformBottom, laneBottom);
            BuildDockWall("Pier Platform East", DockEnd, DockEnd + 0.3f, platformBottom, platformTop);
        }

        private void BuildDockWall(string name, float left, float right, float bottom, float top)
        {
            CreateSolidCollider(name, transform, new Vector2((left + right) * 0.5f, (bottom + top) * 0.5f),
                new Vector2(right - left, top - bottom));
        }
    }
}
