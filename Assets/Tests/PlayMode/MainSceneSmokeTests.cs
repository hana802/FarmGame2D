using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace FarmGame2D.Tests
{
    public sealed class MainSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator MainScene_CreatesPlayableFarmWorld()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<GameSession>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PrototypePlayerController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PrototypeWorldBuilder>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PixelCameraFollow>(), Is.Not.Null);
            var hud = Object.FindFirstObjectByType<PrototypeHud>();
            Assert.That(hud, Is.Not.Null);

            var player = Object.FindFirstObjectByType<PrototypePlayerController>();
            var playerTexture = player.GetComponent<SpriteRenderer>().sprite.texture;
            Assert.That(playerTexture.width, Is.GreaterThan(1), "Player must use the character art sheet, not a 1x1 block.");
            Assert.That(player.HeldItemPresenter, Is.Not.Null);
            Assert.That(player.HeldItemPresenter.ItemRenderer.sprite, Is.Not.Null,
                "Selected farm tool must be visible in the player's hand.");
            Assert.That(player.HeldItemPresenter.DisplayedTool, Is.EqualTo(FarmTool.Hoe));

            GameSession.Instance.SelectTool(FarmTool.WateringCan);
            yield return null;
            Assert.That(player.HeldItemPresenter.DisplayedTool, Is.EqualTo(FarmTool.WateringCan));
            Assert.That(hud.HasArtSkin, Is.True, "HUD must load the illustrated UI atlas.");

            var ground = GameObject.Find("Seasonal Ground").GetComponent<Tilemap>();
            var road = GameObject.Find("Main Street").GetComponent<Tilemap>();
            Assert.That(ground.GetSprite(new Vector3Int(0, 0, 0)).texture.width, Is.GreaterThan(1),
                "Ground must use illustrated grass instead of a 1x1 color block.");
            Assert.That(road.GetSprite(new Vector3Int(0, 0, 0)).texture.width, Is.GreaterThan(1),
                "Roads must use illustrated stone instead of a 1x1 color block.");

            var guide = GameObject.Find("Guide");
            Assert.That(guide, Is.Not.Null);
            Assert.That(guide.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "NPC must use the village NPC art sheet, not a 1x1 block.");

            var farmHouse = GameObject.Find("Farm House");
            Assert.That(farmHouse, Is.Not.Null);
            Assert.That(farmHouse.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "Farm house must use illustrated building art, not a 1x1 block.");
            var distinctVillageArt = new[]
            {
                "Town Hall",
                "General Store",
                "Seed And Feed Shop",
                "West Home",
                "East Home",
                "Village Square",
                "Pond",
                "Chicken Coop",
                "Chicken",
                "Seeded Tree 1"
            };

            foreach (var objectName in distinctVillageArt)
            {
                var worldObject = GameObject.Find(objectName);
                Assert.That(worldObject, Is.Not.Null, $"{objectName} must exist in the prototype world.");
                Assert.That(worldObject.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                    $"{objectName} must use illustrated art, not a 1x1 block.");
            }

            var farmHouseTexture = farmHouse.GetComponent<SpriteRenderer>().sprite.texture;
            Assert.That(GameObject.Find("West Home").GetComponent<SpriteRenderer>().sprite.texture, Is.Not.SameAs(farmHouseTexture));
            Assert.That(GameObject.Find("East Home").GetComponent<SpriteRenderer>().sprite.texture, Is.Not.SameAs(farmHouseTexture));
            Assert.That(GameObject.Find("West Home").GetComponent<SpriteRenderer>().sprite.texture,
                Is.Not.SameAs(GameObject.Find("East Home").GetComponent<SpriteRenderer>().sprite.texture));

            foreach (var interiorProp in new[] { "Interior Bed Art", "Interior Rug Art", "Interior Table", "Interior Cabinet" })
            {
                var prop = GameObject.Find(interiorProp);
                Assert.That(prop, Is.Not.Null, $"{interiorProp} must be placed inside the farm house.");
                Assert.That(prop.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1));
            }

            var weed = GameObject.Find("Weed 1");
            Assert.That(weed, Is.Not.Null, "The world must contain pullable weeds.");
            Assert.That(weed.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1));
            var weedCount = GameSession.Instance.Data.inventory.Count(GameItem.Weed);
            weed.GetComponent<WeedInteractable>().Interact(player);
            yield return null;
            Assert.That(GameSession.Instance.Data.inventory.Count(GameItem.Weed), Is.EqualTo(weedCount + 1));
            Assert.That(GameSession.Instance.Data.pulledWeedIds, Does.Contain(0));

            GameSession.Instance.ToggleInventory();
            Assert.That(GameSession.Instance.InventoryOpen, Is.True);
            GameSession.Instance.SelectTool(FarmTool.Hoe);
            Assert.That(GameSession.Instance.InventoryOpen, Is.False, "Taking an item out of the bag should close it.");

            var entrance = GameObject.Find("Farm House Door").GetComponent<DoorInteractable>();
            Assert.That(entrance, Is.Not.Null);
            Assert.That(entrance.Prompt, Is.EqualTo("집에 들어가기"));
            Assert.That(GameObject.Find("Farm House Wall Left").GetComponent<BoxCollider2D>().isTrigger, Is.False);
            Assert.That(GameObject.Find("Farm House Wall Right").GetComponent<BoxCollider2D>().isTrigger, Is.False);
            Assert.That(GameObject.Find("Farm House Door Blocker").GetComponent<BoxCollider2D>().isTrigger, Is.False);
            Assert.That(GameObject.Find("Town Hall Entrance").GetComponent<InteractableBehaviour>(), Is.Not.Null,
                "Town buildings must explain what happens at their front door instead of allowing wall penetration.");
            entrance.Interact(player);
            Assert.That(player.transform.position.x, Is.EqualTo(40f).Within(0.1f));

            guide.GetComponent<DialogueInteractable>().Interact(player);
            Assert.That(GameSession.Instance.ActivePortraitRow, Is.EqualTo(0));
            Assert.That(GameSession.Instance.ActiveSpeaker, Is.EqualTo("안내자 하람"));
        }
    }
}
