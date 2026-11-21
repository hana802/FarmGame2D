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
            porch = CreateBuildingDetail("Farmhouse Porch", FarmBuildingArt.Porch, new Vector2(6f, -12.33f), house);
            chimney = CreateBuildingDetail("Farmhouse Stone Chimney", FarmBuildingArt.Chimney, new Vector2(7.5f, -10.13f), house);
            chimney.GetComponent<SpriteRenderer>().sortingOrder = porch.GetComponent<SpriteRenderer>().sortingOrder;

            var board = CreateInteractable<FarmFacilityInteractable>("Construction Noticeboard", WorldAreaProfile.FarmConstructionSite,
                new Vector2(0.9f, 0.48f), Color.clear, farm);
            board.Configure(FarmFacility.Construction);
            CreateBuildingDetail("Construction Noticeboard Art", FarmEquipmentArt.Sign, board.transform.position, farm);

            workshop = BuildHouseRoom("Farmhouse Workshop", interior, 1, new Color32(185, 134, 82, 255));
            workshopBackWall = workshop.transform.Find("Back Wall").gameObject;
            CreateWorldArt("Workshop Bench", WorldArtKind.InteriorTable, new Vector2(42.4f, -4.6f),
                workshop.transform, 4, new Vector2(1.5f, 0.65f));
            CreateArtInteractable<SellBoxInteractable>("Workshop Selling Box", WorldArtKind.SellingBox,
                new Vector2(37.5f, -4.7f), new Vector2(1.2f, 0.65f), workshop.transform);

            kitchen = BuildHouseRoom("Farmhouse Kitchen", interior, 2, new Color32(190, 157, 114, 255));
            for (var x = -3; x <= 3; x++)
            for (var y = 0; y < 2; y++)
            {
                var color = (x + y) % 2 == 0 ? new Color32(168, 181, 146, 255) : new Color32(210, 201, 164, 255);
                var tile = PrototypeSpriteFactory.CreateBlock("Kitchen Tile", new Vector2(40f + x, -1.0f + 0.8f * y),
                    new Vector2(1f, 0.8f), color, kitchen.transform);
                tile.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
            CreateWorldArt("Kitchen Cupboard", WorldArtKind.InteriorCabinet, new Vector2(37.1f, -0.7f), kitchen.transform,
                4, new Vector2(1.15f, 0.65f));
            var stovePosition = new Vector2(42.35f, -0.7f);
            var stoveArt = CreateBuildingDetail("Kitchen Stove", FarmBuildingArt.Stove, stovePosition, kitchen.transform);
            CreateSolidCollider("Stove Base", stoveArt.transform, new Vector2(0f, 0.22f), new Vector2(1.05f, 0.62f));
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

        private static GameObject BuildHouseRoom(string name, Transform parent, int stage, Color floor)
        {
            const float floorWidth = 8f;
            var depth = WorldAreaProfile.HouseExpansionDepth;
            var top = -6.5f + stage * depth;
            var room = new GameObject(name);
            room.transform.SetParent(parent);
            var wall = new Color32(88, 55, 39, 255);
            PrototypeSpriteFactory.CreateBlock("Extension Floor", new Vector2(40f, top - depth * 0.5f),
                new Vector2(floorWidth, depth), floor, room.transform);
            for (var row = 0; row < 5; row++)
            {
                var trim = PrototypeSpriteFactory.CreateBlock("Floorboard Seam", new Vector2(40f, top - (row + 0.5f) * depth / 5f),
                    new Vector2(floorWidth, 0.025f), Color.Lerp(floor, wall, 0.22f), room.transform);
                trim.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
            PrototypeSpriteFactory.CreateBlock("Back Wall", new Vector2(40f, top + 0.05f), new Vector2(8.4f, 0.5f), wall, room.transform, true);
            foreach (var x in new[] { 35.8f, 44.2f })
                PrototypeSpriteFactory.CreateBlock("Side Wall", new Vector2(x, top - depth * 0.49f),
                    new Vector2(0.5f, depth + 0.23f), wall, room.transform, true);
            return room;
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
