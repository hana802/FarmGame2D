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
        private float confirmUntil;

        public override string Prompt => Time.unscaledTime < confirmUntil
            ? "온실 건설 확정" : $"온실 짓기 · {GameSession.GreenhousePrice}G";

        public void Configure(GameSession gameSession)
        {
            session = gameSession;
            transform.position = GreenhouseLayout.Site;
            var construction = gameObject.AddComponent<BoxCollider2D>();
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
                session.ShowMessage($"온실 · {GameSession.GreenhousePrice}G / 목재 {GameSession.GreenhouseWood} / 돌 {GameSession.GreenhouseStone} / 구리 광석 {GameSession.GreenhouseCopper}. E로 건설합니다.");
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
        }

        private GameObject CreateExterior()
        {
            var site = GreenhouseLayout.Site;
            var outside = CreateSprite("Greenhouse Exterior", site, FarmEquipmentArt.Greenhouse, transform);
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

            AddWall(room.transform, center + Vector2.up * 2.9f, new Vector2(8.2f, 0.36f), jade);
            AddWall(room.transform, center + Vector2.left * 3.9f, new Vector2(0.35f, 6f), jade);
            AddWall(room.transform, center + Vector2.right * 3.9f, new Vector2(0.35f, 6f), jade);
            AddWall(room.transform, center + new Vector2(-2.25f, -2.9f), new Vector2(3.5f, 0.36f), jade);
            AddWall(room.transform, center + new Vector2(2.25f, -2.9f), new Vector2(3.5f, 0.36f), jade);
            AddWall(room.transform, center - Vector2.up * 2.98f, new Vector2(1f, 0.2f), jade);
            for (var x = -3; x <= 3; x++)
                PrototypeSpriteFactory.CreateBlock("Glass Pane", center + new Vector2(x, 2.55f),
                    new Vector2(0.92f, 0.28f), new Color32(163, 209, 190, 255), room.transform);
            AddDoor(room.transform, center - Vector2.up * 2.7f,
                GreenhouseLayout.Site - Vector2.up * 1.1f, "온실에서 나가기");
            return room;
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
