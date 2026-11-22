using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed partial class PrototypeWorldBuilder
    {
        private GameObject firstFloorBed, firstFloorTable, firstFloorWorkshop, workbench, firstFloorCabinet, upstairs, kitchen;
        private GameObject porch, upperExterior, chimney, kitchenWindow;
        private SpriteRenderer houseExterior;

        private void BuildHouseUpgrades(GameSession session, Transform house, Transform farm, Transform interior)
        {
            BuildHouseExterior(house);
            var board = CreateInteractable<FarmFacilityInteractable>("Construction Noticeboard", WorldAreaProfile.FarmConstructionSite,
                new Vector2(0.9f, 0.48f), Color.clear, farm);
            board.Configure(FarmFacility.Construction);
            CreateBuildingDetail("Construction Noticeboard Art", FarmEquipmentArt.Sign, board.transform.position, farm);

            var center = WorldAreaProfile.FarmhouseInteriorCenter;
            firstFloorWorkshop = new GameObject("Farmhouse Workshop");
            firstFloorWorkshop.transform.SetParent(interior);
            var workshop = firstFloorWorkshop.transform;
            workbench = AddHouseFurniture("Workshop Tool Bench", FarmhouseFurnitureArt.Workbench, center + new Vector2(0.1f, 0.15f),
                new Vector2(2.15f, 0.66f), workshop);
            CreateArtInteractable<SellBoxInteractable>("Workshop Selling Box", WorldArtKind.SellingBox,
                center + new Vector2(-2.85f, 0.1f), new Vector2(1.2f, 0.8f), workshop);
            AddStair("Stairs to Bedroom", center + new Vector2(0f, 2.03f), WorldAreaProfile.FarmhouseBedroomEntry,
                "2층으로 올라가기", workshop);

            BuildFarmhouseKitchen(interior, center);

            var bedroomCenter = WorldAreaProfile.FarmhouseBedroomCenter;
            var bedroom = BuildFarmhouseRoom("Farmhouse Upstairs Bedroom", bedroomCenter, hasOutsideDoor: false);
            upstairs = bedroom.gameObject;
            CreateWorldArt("Bedroom Rug", WorldArtKind.InteriorRug, bedroomCenter + new Vector2(0f, -0.35f), bedroom, 2);
            CreateArtInteractable<BedInteractable>("Upstairs Bed", WorldArtKind.InteriorBed,
                bedroomCenter + new Vector2(2.45f, 0.65f), new Vector2(1.3f, 2.2f), bedroom);
            var chestPosition = bedroomCenter + new Vector2(-2.5f, 1.4f);
            var chest = AddHouseFurniture("Bedroom Chest", FarmhouseFurnitureArt.Chest, chestPosition,
                new Vector2(1.15f, 0.62f), bedroom);
            var handle = chest.AddComponent<BoxCollider2D>();
            handle.isTrigger = true;
            handle.size = new Vector2(1.15f, 0.6f);
            handle.offset = Vector2.down * 0.25f;
            chest.AddComponent<BedroomChestInteractable>();
            AddStair("Stairs to Ground Floor", bedroomCenter + new Vector2(0f, -2.75f), center + new Vector2(0f, 1.25f),
                "1층으로 내려가기", bedroom);

            session.StateChanged += RefreshHouseUpgrades;
            RefreshHouseUpgrades();
        }

        private void BuildFarmhouseKitchen(Transform interior, Vector2 center)
        {
            kitchen = new GameObject("Farmhouse Kitchen");
            var root = kitchen.transform;
            root.SetParent(interior);
            root.position = center;

            var floor = PrototypeSpriteFactory.CreateBlock("Kitchen Checkered Tiles", center + new Vector2(2.55f, 0.72f),
                new Vector2(2.7f, 3.6f), Color.clear, root).GetComponent<SpriteRenderer>();
            floor.sprite = FarmhouseFurnitureArt.KitchenFloor;
            floor.sortingOrder = 1;

            var stovePosition = center + new Vector2(3.25f, 1.47f);
            CreateBuildingDetail("Kitchen Prep Counter", FarmBuildingArt.KitchenCounter, center + new Vector2(1.91f, 1.47f), root);
            CreateBuildingDetail("Kitchen Stove", FarmBuildingArt.KitchenStove, stovePosition, root);
            CreateBuildingDetail("Kitchen Wall Shelf", FarmBuildingArt.KitchenShelf, center + new Vector2(2.55f, 2.27f), root);

            var counter = WorldAreaProfile.FarmhouseKitchenCounter;
            CreateSolidCollider("Kitchen Counter Blocker", root, counter.center, counter.size);
            CreateInteractable<FarmFacilityInteractable>("Kitchen Worktop", stovePosition - Vector2.up * 0.32f,
                new Vector2(1.2f, 0.62f), Color.clear, root).Configure(FarmFacility.Kitchen);
        }

        private void BuildHouseExterior(Transform house)
        {
            houseExterior = house.GetComponent<SpriteRenderer>();
            var origin = (Vector2)house.position;
            var depth = houseExterior.sortingOrder;
            porch = CreateBuildingDetail("Farmhouse Masonry Landing", FarmBuildingArt.Porch, origin + new Vector2(0f, -0.51f), house);
            porch.transform.localScale = new Vector3(1.28f, 1.1f, 1f);
            porch.GetComponent<SpriteRenderer>().sortingOrder = depth - 1;

            upperExterior = new GameObject("Farmhouse Second Story");
            upperExterior.transform.SetParent(house, false);
            var wallY = FarmBuildingArt.UpperWallOffset;
            var roofY = wallY + FarmBuildingArt.UpperStoryHeight;
            AddFacadeLayer("Upper Hall", FarmBuildingArt.UpperWall, origin + Vector2.up * wallY, depth + 1);
            AddFacadeLayer("Left Upper Window", FarmBuildingArt.UpstairsWindow, origin + new Vector2(-1.18f, wallY + 0.15f), depth + 2);
            AddFacadeLayer("Right Upper Window", FarmBuildingArt.UpstairsWindow, origin + new Vector2(1.18f, wallY + 0.15f), depth + 2);
            AddFacadeLayer("Raised Farmhouse Roof", FarmBuildingArt.UpperRoof, origin + Vector2.up * roofY, depth + 3);

            chimney = CreateBuildingDetail("Farmhouse Kitchen Chimney", FarmBuildingArt.Chimney,
                origin + new Vector2(-1.33f, 3.23f + FarmBuildingArt.UpperStoryHeight), house);
            chimney.GetComponent<SpriteRenderer>().sortingOrder = depth + 4;
            kitchenWindow = CreateBuildingDetail("Farmhouse Kitchen Window", FarmBuildingArt.KitchenWindow,
                origin + new Vector2(1.37f, 0.99f), house);
            kitchenWindow.GetComponent<SpriteRenderer>().sortingOrder = depth + 1;
        }

        private void AddFacadeLayer(string name, Sprite art, Vector2 point, int sortingOrder)
        {
            var layer = CreateBuildingDetail(name, art, point, upperExterior.transform);
            layer.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        }

        private void RefreshHouseUpgrades()
        {
            var level = GameSession.Instance.Data.houseUpgradeLevel;
            var expanded = level >= 1;
            var kitchenOpen = level >= 2;
            firstFloorBed.SetActive(!expanded);
            firstFloorTable.SetActive(!expanded);
            firstFloorWorkshop.SetActive(expanded);
            workbench.SetActive(!kitchenOpen);
            var cabinetPosition = WorldAreaProfile.FarmhouseInteriorCenter + new Vector2(kitchenOpen ? -2.4f : 2.4f, 2.05f);
            firstFloorCabinet.transform.position = cabinetPosition;
            upstairs.SetActive(expanded);
            kitchen.SetActive(kitchenOpen);
            porch.SetActive(expanded);
            upperExterior.SetActive(expanded);
            chimney.SetActive(kitchenOpen);
            kitchenWindow.SetActive(kitchenOpen);
            houseExterior.sprite = expanded ? FarmBuildingArt.LowerFloor : WorldArtCatalog.GetSprite(WorldArtKind.FarmHouse);
        }

        private Transform BuildFarmhouseRoom(string name, Vector2 center, bool hasOutsideDoor)
        {
            var room = new GameObject(name).transform;
            room.SetParent(transform);
            var floor = PrototypeSpriteFactory.CreateBlock("Interior Floor", center + new Vector2(0f, -0.25f),
                new Vector2(8f, 6f), new Color(0.77f, 0.62f, 0.42f), room);
            floor.GetComponent<SpriteRenderer>().sprite = FarmhouseFurnitureArt.WoodFloor;
            var wall = new Color(0.34f, 0.2f, 0.14f);
            PrototypeSpriteFactory.CreateBlock("Interior Top Wall", center + new Vector2(0f, 2.8f),
                new Vector2(8.4f, 0.5f), wall, room, true);
            foreach (var side in new[] { -1f, 1f })
            {
                PrototypeSpriteFactory.CreateBlock("Interior Side Wall", center + new Vector2(side * 4.2f, -0.25f),
                    new Vector2(0.5f, 6.5f), wall, room, true);
                if (hasOutsideDoor)
                    PrototypeSpriteFactory.CreateBlock("Interior Bottom Wall", center + new Vector2(side * 2.2f, -3.3f),
                        new Vector2(3.9f, 0.5f), wall, room, true);
            }
            if (!hasOutsideDoor)
                PrototypeSpriteFactory.CreateBlock("Interior Bottom Wall", center + new Vector2(0f, -3.3f),
                    new Vector2(8.4f, 0.5f), wall, room, true);
            return room;
        }

        private static void AddStair(string name, Vector2 point, Vector2 destination, string prompt, Transform parent)
        {
            var stair = CreateBuildingDetail(name, FarmBuildingArt.Stairs, point, parent);
            var trigger = stair.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.1f, 0.55f);
            stair.AddComponent<DoorInteractable>().Configure(destination, prompt, string.Empty);
        }

        private static GameObject AddHouseFurniture(string name, Sprite art, Vector2 position, Vector2 footprint, Transform parent)
        {
            var item = CreateBuildingDetail(name, art, position, parent);
            CreateSolidCollider($"{name} Blocker", item.transform, new Vector2(0f, 0.25f), footprint);
            return item;
        }

        private static GameObject CreateBuildingDetail(string name, Sprite sprite, Vector2 point, Transform parent)
        {
            var detail = new GameObject(name);
            detail.transform.SetParent(parent);
            detail.transform.position = point;
            var renderer = detail.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = WorldDepthSorter.CalculateOrder(point.y, WorldDepthSorter.BuildingDepthOffset + 1);
            return detail;
        }
    }
}
