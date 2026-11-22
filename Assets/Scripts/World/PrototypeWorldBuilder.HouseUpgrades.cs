using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed partial class PrototypeWorldBuilder
    {
        private GameObject firstFloorBed, firstFloorTable, firstFloorWorkshop, upstairs, stove;
        private GameObject porch, upperExterior, chimney;
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
            AddHouseFurniture("Workshop Tool Bench", FarmhouseFurnitureArt.Workbench, center + new Vector2(0.1f, 0.15f),
                new Vector2(2.15f, 0.66f), workshop);
            CreateArtInteractable<SellBoxInteractable>("Workshop Selling Box", WorldArtKind.SellingBox,
                center + new Vector2(-2.85f, 0.1f), new Vector2(1.2f, 0.8f), workshop);
            AddStair("Stairs to Bedroom", center + new Vector2(0f, 2.03f), WorldAreaProfile.FarmhouseBedroomEntry,
                "2층으로 올라가기", workshop);

            stove = new GameObject("Farmhouse Kitchen");
            stove.transform.SetParent(interior);
            var stovePosition = center + new Vector2(2.8f, 0.15f);
            AddHouseFurniture("Kitchen Stove", FarmhouseFurnitureArt.Range, stovePosition, new Vector2(1.25f, 0.6f), stove.transform);
            CreateInteractable<FarmFacilityInteractable>("Kitchen Worktop", stovePosition - Vector2.up * 0.4f,
                new Vector2(1.2f, 0.62f), Color.clear, stove.transform).Configure(FarmFacility.Kitchen);

            var bedroomCenter = WorldAreaProfile.FarmhouseBedroomCenter;
            var bedroom = BuildFarmhouseRoom("Farmhouse Upstairs Bedroom", bedroomCenter, hasOutsideDoor: false);
            upstairs = bedroom.gameObject;
            CreateWorldArt("Bedroom Rug", WorldArtKind.InteriorRug, bedroomCenter + new Vector2(0f, -0.35f), bedroom, 2);
            CreateArtInteractable<BedInteractable>("Upstairs Bed", WorldArtKind.InteriorBed,
                bedroomCenter + new Vector2(2.45f, 0.65f), new Vector2(1.3f, 2.2f), bedroom);
            AddHouseFurniture("Bedroom Chest", FarmhouseFurnitureArt.Chest, bedroomCenter + new Vector2(-2.5f, 1.4f),
                new Vector2(1.15f, 0.62f), bedroom);
            AddStair("Stairs to Ground Floor", bedroomCenter + new Vector2(0f, -2.75f), center + new Vector2(0f, 1.25f),
                "1층으로 내려가기", bedroom);

            session.StateChanged += RefreshHouseUpgrades;
            RefreshHouseUpgrades();
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
            firstFloorBed.SetActive(!expanded);
            firstFloorTable.SetActive(!expanded);
            firstFloorWorkshop.SetActive(expanded);
            upstairs.SetActive(expanded);
            stove.SetActive(level >= 2);
            porch.SetActive(expanded);
            upperExterior.SetActive(expanded);
            chimney.SetActive(level >= 2);
            houseExterior.sprite = expanded ? FarmBuildingArt.LowerFloor : WorldArtCatalog.GetSprite(WorldArtKind.FarmHouse);
        }

        private Transform BuildFarmhouseRoom(string name, Vector2 center, bool hasOutsideDoor)
        {
            var room = new GameObject(name).transform;
            room.SetParent(transform);
            var floor = PrototypeSpriteFactory.CreateBlock("Interior Floor", center + new Vector2(0f, -0.25f),
                new Vector2(8f, 6f), new Color(0.77f, 0.62f, 0.42f), room);
            if (!hasOutsideDoor) floor.GetComponent<SpriteRenderer>().sprite = FarmhouseFurnitureArt.GetFloor(8, false);
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

        private static void AddHouseFurniture(string name, Sprite art, Vector2 position, Vector2 footprint, Transform parent)
        {
            var item = CreateBuildingDetail(name, art, position, parent);
            CreateSolidCollider($"{name} Blocker", item.transform, new Vector2(0f, 0.25f), footprint);
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
