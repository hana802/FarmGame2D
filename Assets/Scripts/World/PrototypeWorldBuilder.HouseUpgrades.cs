using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed partial class PrototypeWorldBuilder
    {
        private GameObject porch, chimney, workshop, kitchen, baseHouseWall, workshopBackWall;

        private void BuildHouseUpgrades(GameSession session, Transform house, Transform farm, Transform interior, GameObject topWall)
        {
            baseHouseWall = topWall;
            var exterior = house.GetComponent<SpriteRenderer>();
            porch = CreateBuildingDetail("Farmhouse Masonry Landing", FarmBuildingArt.Porch, new Vector2(6f, -12.51f), house);
            porch.transform.localScale = new Vector3(1.28f, 1.1f, 1f);
            porch.GetComponent<SpriteRenderer>().sortingOrder = exterior.sortingOrder - 1;
            chimney = CreateBuildingDetail("Farmhouse Kitchen Chimney", FarmBuildingArt.Chimney, new Vector2(4.67f, -8.77f), house);
            chimney.GetComponent<SpriteRenderer>().sortingOrder = exterior.sortingOrder + 1;

            var board = CreateInteractable<FarmFacilityInteractable>("Construction Noticeboard", WorldAreaProfile.FarmConstructionSite,
                new Vector2(0.9f, 0.48f), Color.clear, farm);
            board.Configure(FarmFacility.Construction);
            CreateBuildingDetail("Construction Noticeboard Art", FarmEquipmentArt.Sign, board.transform.position, farm);

            workshop = BuildHouseRoom("Farmhouse Workshop", interior, 1);
            workshopBackWall = workshop.transform.Find("Back Wall").gameObject;
            AddHouseFurniture("Workshop Tool Bench", FarmhouseFurnitureArt.Workbench, new Vector2(44.0f, -5.0f),
                new Vector2(2.15f, 0.66f), workshop.transform);
            AddHouseFurniture("Workshop Tool Chest", FarmhouseFurnitureArt.Chest, new Vector2(35.35f, -4.76f),
                new Vector2(1.15f, 0.62f), workshop.transform);
            CreateArtInteractable<SellBoxInteractable>("Workshop Selling Box", WorldArtKind.SellingBox,
                new Vector2(37.5f, -4.7f), new Vector2(1.2f, 0.65f), workshop.transform);

            kitchen = BuildHouseRoom("Farmhouse Kitchen", interior, 2);
            AddHouseFurniture("Kitchen Sink Counter", FarmhouseFurnitureArt.Sink, new Vector2(35.45f, -1.42f),
                new Vector2(2.45f, 0.65f), kitchen.transform);
            var stovePosition = new Vector2(42.35f, -1.22f);
            AddHouseFurniture("Kitchen Stove", FarmhouseFurnitureArt.Range, stovePosition, new Vector2(1.25f, 0.6f), kitchen.transform);
            CreateInteractable<FarmFacilityInteractable>("Kitchen Worktop", stovePosition - Vector2.up * 0.45f,
                new Vector2(1.2f, 0.68f), Color.clear, kitchen.transform).Configure(FarmFacility.Kitchen);

            session.StateChanged += RefreshHouseUpgrades;
            RefreshHouseUpgrades();
        }

        private void RefreshHouseUpgrades()
        {
            var level = GameSession.Instance.Data.houseUpgradeLevel;
            porch.SetActive(level >= 1);
            workshop.SetActive(level >= 1);
            chimney.SetActive(level >= 2);
            kitchen.SetActive(level >= 2);
            baseHouseWall.SetActive(level == 0);
            workshopBackWall.SetActive(level == 1);
        }

        private static void AddHouseFurniture(string name, Sprite art, Vector2 position, Vector2 footprint, Transform parent)
        {
            var item = CreateBuildingDetail(name, art, position, parent);
            CreateSolidCollider($"{name} Blocker", item.transform, new Vector2(0f, 0.25f), footprint);
        }

        private static GameObject BuildHouseRoom(string name, Transform parent, int stage)
        {
            var floorWidth = WorldAreaProfile.HouseRoomWidth(stage);
            var priorWidth = WorldAreaProfile.HouseRoomWidth(stage - 1);
            var depth = WorldAreaProfile.HouseExpansionDepth;
            var top = -6.5f + stage * depth;
            var bottom = top - depth;
            var right = WorldAreaProfile.FarmhouseInteriorCenter.x + WorldAreaProfile.HouseWorkshopWidth * 0.5f;
            var left = right - floorWidth;
            var center = (left + right) * 0.5f;
            var room = new GameObject(name);
            room.transform.SetParent(parent);
            var wall = new Color32(88, 55, 39, 255);
            var floor = PrototypeSpriteFactory.CreateBlock("Extension Floor", new Vector2(center, (top + bottom) * 0.5f),
                new Vector2(floorWidth, depth), Color.white, room.transform);
            var floorRenderer = floor.GetComponent<SpriteRenderer>();
            floorRenderer.sprite = FarmhouseFurnitureArt.GetFloor((int)floorWidth, stage == 2);
            PrototypeSpriteFactory.CreateBlock("Back Wall", new Vector2(center, top + 0.05f),
                new Vector2(floorWidth + 0.4f, 0.5f), wall, room.transform, true);
            foreach (var x in new[] { left - 0.2f, right + 0.2f })
                PrototypeSpriteFactory.CreateBlock("Side Wall", new Vector2(x, (top + bottom) * 0.5f),
                    new Vector2(0.5f, depth + 0.23f), wall, room.transform, true);

            var oldRight = stage == 1 ? WorldAreaProfile.FarmhouseInteriorCenter.x + priorWidth * 0.5f : right;
            var oldLeft = oldRight - priorWidth;
            AddShoulderWall(room.transform, left, oldLeft, bottom, wall);
            AddShoulderWall(room.transform, oldRight, right, bottom, wall);
            return room;
        }

        private static void AddShoulderWall(Transform parent, float left, float right, float y, Color wall)
        {
            if (right <= left) return;
            PrototypeSpriteFactory.CreateBlock("Side Extension End Wall", new Vector2((left + right) * 0.5f, y - 0.1f),
                new Vector2(right - left + 0.2f, 0.32f), wall, parent, true);
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
