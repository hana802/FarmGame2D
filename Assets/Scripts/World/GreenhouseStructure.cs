using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed class GreenhouseStructure : InteractableBehaviour
    {
        private GameSession session;
        private GameObject building, interior, sign;
        private GameObject glassWingLeft, glassWingRight, vent;
        private GameObject baseBackWall, firstExtension, firstBackWall, finalExtension;
        private float confirmUntil;

        public override string Prompt => Time.unscaledTime < confirmUntil
            ? "온실 건설 확정" : $"온실 짓기 · {GameSession.GreenhousePrice}G";

        public void Configure(GameSession gameSession)
        {
            session = gameSession;
            transform.position = GreenhouseLayout.Site;
            var construction = GetComponent<BoxCollider2D>();
            construction.size = new Vector2(1.1f, 0.5f);
            construction.isTrigger = true;
            sign = CreateSprite("Construction Sign", transform.position, FarmEquipmentArt.Sign, transform);
            building = CreateExterior();
            interior = CreateInterior();
            session.StateChanged += Refresh;
            Refresh();
        }

        public override void Interact(PrototypePlayerController player)
        {
            if (Time.unscaledTime >= confirmUntil)
            {
                confirmUntil = Time.unscaledTime + 6f;
                session.ShowMessage(MaterialProject.Describe("온실", MaterialProject.Greenhouse) + ". E로 건설합니다.");
                return;
            }

            confirmUntil = 0f;
            session.BuildGreenhouse();
        }

        private void OnDestroy()
        {
            if (session != null) session.StateChanged -= Refresh;
        }

        private void Refresh()
        {
            var built = session.Data.greenhouseBuilt;
            sign.SetActive(!built);
            GetComponent<BoxCollider2D>().enabled = !built;
            building.SetActive(built);
            interior.SetActive(built);
            var level = session.Data.greenhouseUpgradeLevel;
            glassWingLeft.SetActive(level >= 1);
            glassWingRight.SetActive(level >= 2);
            vent.SetActive(level >= 2);
            baseBackWall.SetActive(level == 0);
            firstExtension.SetActive(level >= 1);
            firstBackWall.SetActive(level == 1);
            finalExtension.SetActive(level >= 2);
        }

        private GameObject CreateExterior()
        {
            var site = GreenhouseLayout.Site;
            var outside = CreateSprite("Greenhouse Exterior", site, FarmEquipmentArt.Greenhouse, transform);
            glassWingLeft = AddExteriorGlass("West glass wing", site + new Vector2(-1.5f, 1.12f), FarmBuildingArt.GlassWing, outside);
            glassWingRight = AddExteriorGlass("East glass wing", site + new Vector2(1.5f, 1.12f), FarmBuildingArt.GlassWing, outside);
            vent = AddExteriorGlass("Roof vent", site + Vector2.up * 2.93f, FarmBuildingArt.RoofVent, outside);
            var wall = outside.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(4.5f, 1.3f);
            wall.offset = Vector2.up * 0.82f;
            AddDoor(outside.transform, site - Vector2.up * 0.25f,
                GreenhouseLayout.Interior - Vector2.up * 2.18f, "온실로 들어가기");
            return outside;
        }

        private GameObject CreateInterior()
        {
            var room = new GameObject("Greenhouse Interior");
            room.transform.SetParent(transform);
            var center = GreenhouseLayout.Interior;
            var jade = new Color32(68, 105, 84, 255);
            var tile = new Color32(193, 169, 126, 255);
            var lightTile = new Color32(203, 181, 137, 255);
            for (var x = -4; x < 4; x++)
            for (var y = -3; y < 3; y++)
                PrototypeSpriteFactory.CreateBlock("Greenhouse Floor Tile", center + new Vector2(x + 0.5f, y + 0.5f),
                    Vector2.one, (x + y) % 2 == 0 ? tile : lightTile, room.transform);

            baseBackWall = AddBackWall(room.transform, center, 0, jade);
            AddWall(room.transform, center + Vector2.left * 3.9f, new Vector2(0.35f, 6f), jade);
            AddWall(room.transform, center + Vector2.right * 3.9f, new Vector2(0.35f, 6f), jade);
            AddWall(room.transform, center + new Vector2(-2.25f, -2.9f), new Vector2(3.5f, 0.36f), jade);
            AddWall(room.transform, center + new Vector2(2.25f, -2.9f), new Vector2(3.5f, 0.36f), jade);
            AddWall(room.transform, center - Vector2.up * 2.98f, new Vector2(1f, 0.2f), jade);
            AddDoor(room.transform, center - Vector2.up * 2.7f,
                GreenhouseLayout.Site - Vector2.up * 1.1f, "온실에서 나가기");
            var shelf = PrototypeSpriteFactory.CreateBlock(
                "Greenhouse Seed Shelf",
                center + new Vector2(0f, 2.15f),
                new Vector2(1.8f, 0.42f),
                new Color32(92, 64, 42, 255),
                room.transform,
                true);
            shelf.GetComponent<BoxCollider2D>().isTrigger = true;
            shelf.AddComponent<ShopInteractable>().Configure(ShopKind.Greenhouse);
            firstExtension = AddGrowingRoom(room.transform, center, 1, jade, tile, lightTile);
            firstBackWall = AddBackWall(firstExtension.transform, center, 1, jade);
            finalExtension = AddGrowingRoom(room.transform, center, 2, jade, tile, lightTile);
            AddBackWall(finalExtension.transform, center, 2, jade);
            var pipe = PrototypeSpriteFactory.CreateBlock("Irrigation main", center + new Vector2(0f, 4.25f),
                new Vector2(7.4f, 0.14f), new Color32(86, 135, 146, 255), finalExtension.transform);
            pipe.GetComponent<SpriteRenderer>().sortingOrder = 1;
            return room;
        }

        private static GameObject AddExteriorGlass(string name, Vector2 position, Sprite sprite, GameObject exterior)
        {
            var detail = CreateSprite(name, position, sprite, exterior.transform);
            detail.GetComponent<SpriteRenderer>().sortingOrder = exterior.GetComponent<SpriteRenderer>().sortingOrder + 1;
            return detail;
        }

        private static GameObject AddGrowingRoom(Transform parent, Vector2 center, int stage, Color wall, Color tile, Color lightTile)
        {
            var room = new GameObject($"Greenhouse Extension {stage}");
            room.transform.SetParent(parent);
            var depth = GreenhouseLayout.ExpansionDepth;
            var lowerEdge = 3f + (stage - 1) * depth;
            for (var x = -4; x < 4; x++)
            for (var y = 0; y < (int)depth; y++)
                PrototypeSpriteFactory.CreateBlock("Extension Floor Tile", center + new Vector2(x + 0.5f, lowerEdge + y + 0.5f),
                    Vector2.one, (x + y) % 2 == 0 ? lightTile : tile, room.transform);

            foreach (var x in new[] { -3.9f, 3.9f })
                AddWall(room.transform, center + new Vector2(x, lowerEdge + depth / 2f), new Vector2(0.35f, depth + 0.14f), wall);
            return room;
        }

        private static GameObject AddBackWall(Transform parent, Vector2 center, int stage, Color jade)
        {
            var wall = new GameObject("Greenhouse Back Wall");
            wall.transform.SetParent(parent);
            var top = 2.9f + stage * GreenhouseLayout.ExpansionDepth;
            AddWall(wall.transform, center + Vector2.up * top, new Vector2(8.2f, 0.36f), jade);
            for (var x = -3; x <= 3; x++)
                PrototypeSpriteFactory.CreateBlock("Glass Pane", center + new Vector2(x, top - 0.35f),
                    new Vector2(0.92f, 0.28f), new Color32(163, 209, 190, 255), wall.transform);
            return wall;
        }

        private static void AddWall(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            PrototypeSpriteFactory.CreateBlock("Greenhouse Frame", position, size, color, parent, true);
        }

        private static void AddDoor(Transform parent, Vector2 position, Vector2 destination, string prompt)
        {
            var door = new GameObject("Greenhouse Door");
            door.transform.SetParent(parent);
            door.transform.position = position;
            var trigger = door.AddComponent<BoxCollider2D>();
            trigger.size = new Vector2(1f, 0.4f);
            trigger.isTrigger = true;
            door.AddComponent<DoorInteractable>().Configure(destination, prompt, prompt);
        }

        private static GameObject CreateSprite(string name, Vector2 position, Sprite sprite, Transform parent)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent);
            item.transform.position = position;
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = WorldDepthSorter.CalculateOrder(position.y, WorldDepthSorter.BuildingDepthOffset);
            return item;
        }
    }
}
