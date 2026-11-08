using System.Collections;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.Farming;
using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.World;
using FarmGame2D.TimeSystem;
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
            var atmosphere = Object.FindFirstObjectByType<SeasonalAtmosphereController>();
            Assert.That(atmosphere, Is.Not.Null);
            Assert.That(atmosphere.LightingRenderer, Is.Not.Null);
            Assert.That(atmosphere.WeatherParticles, Is.Not.Null);
            var hud = Object.FindFirstObjectByType<PrototypeHud>();
            Assert.That(hud, Is.Not.Null);

            var player = Object.FindFirstObjectByType<PrototypePlayerController>();
            Assert.That(player.GetComponent<WorldDepthSorter>(), Is.Not.Null);
            var playerTexture = player.GetComponent<SpriteRenderer>().sprite.texture;
            Assert.That(playerTexture.width, Is.GreaterThan(1), "Player must use the character art sheet, not a 1x1 block.");
            Assert.That(player.HeldItemPresenter, Is.Not.Null);
            Assert.That(player.HeldItemPresenter.ItemRenderer.sprite, Is.Not.Null,
                "Selected farm tool must be visible in the player's hand.");
            Assert.That(player.HeldItemPresenter.DisplayedTool, Is.EqualTo(FarmTool.Hoe));

            var dog = Object.FindFirstObjectByType<DogCompanionController>();
            Assert.That(dog, Is.Not.Null, "The farm needs a companion dog.");
            Assert.That(dog.FollowTarget, Is.EqualTo(player.transform));
            Assert.That(dog.DogRenderer.sprite.name, Is.EqualTo("world_Dog"));
            Assert.That(dog.DogRenderer.sprite.texture.width, Is.GreaterThan(1));
            var dogInteraction = dog.GetComponent<DogInteractable>();
            Assert.That(dogInteraction, Is.Not.Null);
            Assert.That(dogInteraction.Prompt, Is.EqualTo("강아지 쓰다듬기"));
            var affectionBeforePetting = GameSession.Instance.Data.dogAffection;
            dogInteraction.Interact(player);
            Assert.That(GameSession.Instance.Data.dogAffection, Is.EqualTo(affectionBeforePetting + 1));
            Assert.That(dog.IsCelebrating, Is.True);
            Assert.That(Object.FindObjectsByType<DogHeartEffect>(FindObjectsSortMode.None).Length, Is.EqualTo(3));
            var celebrationStartOffset = (Vector2)(dog.transform.position - player.transform.position);
            yield return new WaitForSeconds(0.35f);
            var celebrationMidOffset = (Vector2)(dog.transform.position - player.transform.position);
            Assert.That(Quaternion.Angle(dog.DogRenderer.transform.localRotation, Quaternion.identity), Is.LessThan(0.1f),
                "The dog sprite must stay upright instead of spinning like a top.");
            Assert.That(Vector2.Angle(celebrationStartOffset, celebrationMidOffset), Is.GreaterThan(20f),
                "Petting should make the dog run around the player, changing its orbit angle.");
            Assert.That(celebrationMidOffset.magnitude, Is.InRange(0.75f, 1.5f),
                "The celebration should keep the dog on a readable circle around the player.");
            yield return new WaitForSeconds(2.2f);
            Assert.That(dog.IsCelebrating, Is.False);

            var dogStartDistance = Vector2.Distance(dog.transform.position, player.transform.position);
            dog.transform.position = player.transform.position + new Vector3(3f, 0f, 0f);
            var dogPlacedDistance = Vector2.Distance(dog.transform.position, player.transform.position);
            yield return new WaitForSeconds(0.5f);
            Assert.That(Vector2.Distance(dog.transform.position, player.transform.position), Is.LessThan(dogPlacedDistance),
                "The companion dog should follow the player instead of staying behind.");
            Assert.That(dogStartDistance, Is.LessThan(2f));

            GameSession.Instance.SelectTool(FarmTool.WateringCan);
            yield return null;
            Assert.That(player.HeldItemPresenter.DisplayedTool, Is.EqualTo(FarmTool.WateringCan));
            Assert.That(hud.HasArtSkin, Is.True, "HUD must load the illustrated UI atlas.");

            var ground = GameObject.Find("Seasonal Ground").GetComponent<Tilemap>();
            var road = GameObject.Find("Main Street").GetComponent<Tilemap>();
            var ocean = GameObject.Find("Island Ocean").GetComponent<Tilemap>();
            Assert.That(ground.GetSprite(new Vector3Int(0, 0, 0)).texture.width, Is.GreaterThan(1),
                "Ground must use illustrated grass instead of a 1x1 color block.");
            Assert.That(road.GetSprite(new Vector3Int(0, 0, 0)).texture.width, Is.GreaterThan(1),
                "Roads must use illustrated stone instead of a 1x1 color block.");
            Assert.That(ocean.GetSprite(new Vector3Int(-22, 0, 0)), Is.Not.Null,
                "The world outside the island boundary must be filled with ocean tiles.");
            Assert.That(ocean.GetSprite(new Vector3Int(-22, 0, 0)).texture.width, Is.GreaterThan(1),
                "Ocean tiles must include visible pixel-art waves instead of a flat color block.");
            Assert.That(ocean.GetSprite(new Vector3Int(0, 22, 0)), Is.Not.Null);
            Assert.That(ocean.GetComponent<OceanMotionController>(), Is.Not.Null,
                "Ocean tiles need a looping motion controller instead of remaining static.");

            foreach (var boundaryName in new[] { "North Boundary", "South Boundary", "West Boundary", "East Boundary" })
            {
                var boundary = GameObject.Find(boundaryName);
                Assert.That(boundary, Is.Not.Null, $"{boundaryName} must keep characters on the island.");
                Assert.That(boundary.GetComponent<BoxCollider2D>().isTrigger, Is.False);
            }

            var guide = GameObject.Find("Guide");
            Assert.That(guide, Is.Not.Null);
            Assert.That(guide.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "NPC must use the village NPC art sheet, not a 1x1 block.");
            Assert.That(guide.GetComponent<WanderingMover>(), Is.Not.Null);
            Assert.That(guide.GetComponent<WorldDepthSorter>(), Is.Not.Null);
            Assert.That(guide.GetComponent<BoxCollider2D>().isTrigger, Is.False,
                "NPCs need solid bodies so they cannot walk through scenery or the player.");
            Assert.That(guide.GetComponent<Rigidbody2D>(), Is.Not.Null,
                "NPC movement must use the physics engine instead of writing directly to the transform.");
            Assert.That(guide.GetComponent("NpcSocialAgent"), Is.Not.Null,
                "Villagers need a social agent so they can stop and talk to one another.");
            var fisher = GameObject.Find("Fisher Minseo");
            var florist = GameObject.Find("Florist Taeo");
            var baker = GameObject.Find("Baker Yuna");
            var rancher = GameObject.Find("Rancher Sora");
            Assert.That(fisher, Is.Not.Null);
            Assert.That(florist, Is.Not.Null);
            Assert.That(baker, Is.Not.Null);
            Assert.That(rancher, Is.Not.Null);
            Assert.That(baker.GetComponent<WanderingMover>(), Is.Not.Null);
            Assert.That(rancher.GetComponent<WanderingMover>(), Is.Not.Null);
            Assert.That(baker.GetComponent<NpcSocialAgent>(), Is.Not.Null);
            Assert.That(rancher.GetComponent<NpcSocialAgent>(), Is.Not.Null);
            var minseoPortrait = GameArtCatalog.GetNpcPortrait(4);
            var taeoPortrait = GameArtCatalog.GetNpcPortrait(5);
            var yunaPortrait = GameArtCatalog.GetNpcPortrait(6);
            var soraPortrait = GameArtCatalog.GetNpcPortrait(7);
            Assert.That(minseoPortrait, Is.Not.Null);
            Assert.That(taeoPortrait, Is.Not.Null);
            Assert.That(yunaPortrait, Is.Not.Null);
            Assert.That(soraPortrait, Is.Not.Null);
            Assert.That(minseoPortrait.name, Is.EqualTo("npc_portrait_minseo"));
            Assert.That(taeoPortrait.name, Is.EqualTo("npc_portrait_taeo"));
            Assert.That(yunaPortrait.name, Is.EqualTo("npc_portrait_yuna"));
            Assert.That(soraPortrait.name, Is.EqualTo("npc_portrait_sora"));
            Assert.That(minseoPortrait.texture, Is.Not.SameAs(taeoPortrait.texture),
                "Romance candidates need distinct, dedicated dialogue portraits.");
            Assert.That(yunaPortrait.texture, Is.Not.SameAs(soraPortrait.texture),
                "New female villagers need distinct, dedicated dialogue portraits.");
            Assert.That(guide.transform.localScale, Is.EqualTo(Vector3.one),
                "NPC artwork must use the same world scale as the player instead of inheriting prototype block scale.");
            GameClockController.Instance.SetTime(GameSession.Instance.Data.day, 12, 0);
            var guideStart = guide.transform.position;
            yield return new WaitForSeconds(4f);
            Assert.That(Vector2.Distance(guideStart, guide.transform.position), Is.GreaterThan(0.01f),
                "NPCs should alternate between waiting and walking around their daytime area.");
            Assert.That(guide.GetComponent<WanderingMover>().RoamingArea.Contains(guide.transform.position), Is.True);
            Assert.That(guide.GetComponent<WanderingMover>().UsesWaypointNetwork, Is.True,
                "Villagers should travel through the village waypoint network instead of orbiting one small rectangle.");

            var guideSocial = guide.GetComponent<NpcSocialAgent>();
            var elderSocial = GameObject.Find("Elder").GetComponent<NpcSocialAgent>();
            Assert.That(guideSocial.TryStartConversation(elderSocial), Is.True);
            Assert.That(guideSocial.IsConversing, Is.True);
            Assert.That(elderSocial.IsConversing, Is.True);
            Assert.That(guideSocial.BubbleText, Is.Not.Empty);
            yield return new WaitForSeconds(2.2f);
            Assert.That(elderSocial.BubbleText, Is.Not.Empty,
                "NPC conversations should alternate visible speech bubbles.");
            guideSocial.CancelConversation();
            Assert.That(guideSocial.IsConversing, Is.False);
            Assert.That(elderSocial.IsConversing, Is.False);

            var farmHouse = GameObject.Find("Farm House");
            Assert.That(farmHouse, Is.Not.Null);
            Assert.That(farmHouse.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "Farm house must use illustrated building art, not a 1x1 block.");
            Assert.That(
                farmHouse.GetComponent<SpriteRenderer>().sortingOrder,
                Is.EqualTo(WorldDepthSorter.CalculateOrder(farmHouse.transform.position.y, WorldDepthSorter.BuildingDepthOffset)),
                "Building depth must cover the player when walking behind the roof while still allowing the player in front at the door.");

            var chickenCoopStructure = GameObject.Find("Chicken Coop");
            Assert.That(
                chickenCoopStructure.GetComponent<SpriteRenderer>().sortingOrder,
                Is.EqualTo(WorldDepthSorter.CalculateOrder(chickenCoopStructure.transform.position.y, WorldDepthSorter.BuildingDepthOffset)),
                "Chicken Coop must cover characters walking behind its roof.");

            var sellingBox = GameObject.Find("Selling Box");
            Assert.That(sellingBox, Is.Not.Null);
            Assert.That(sellingBox.GetComponent<SellBoxInteractable>(), Is.Not.Null);
            Assert.That(sellingBox.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("world_SellingBox"));
            Assert.That(sellingBox.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "Selling box must use its illustrated world art, not a 1x1 block.");
            Assert.That(sellingBox.GetComponent<BoxCollider2D>().isTrigger, Is.True);
            var seedShop = GameObject.Find("Seed And Feed Shop");
            Assert.That(sellingBox.transform.parent, Is.EqualTo(seedShop.transform.parent),
                "The selling box should live in the village beside the seed shop, not on the farm field.");
            Assert.That(sellingBox.transform.position.x, Is.GreaterThan(seedShop.transform.position.x));
            Assert.That(Vector2.Distance(sellingBox.transform.position, seedShop.transform.position), Is.LessThan(4f));

            Assert.That(GameSession.Instance.Data.farmPlots, Is.Empty,
                "A new farm should not include prebuilt plots.");
            Assert.That(GameObject.Find("Wheat Plot 1"), Is.Null);
            Assert.That(Object.FindFirstObjectByType<FreeformFarmField>(), Is.Not.Null);
            var plotCountBeforeExpansion = GameSession.Instance.Data.farmPlots.Count;
            Assert.That(GameSession.Instance.AddFreeformPlot(new Vector2(14.5f, -3.5f)), Is.True);
            yield return null;
            Assert.That(GameSession.Instance.Data.farmPlots.Count, Is.EqualTo(plotCountBeforeExpansion + 1));
            var freePlot = GameObject.Find($"Free Plot {plotCountBeforeExpansion + 1}");
            Assert.That(freePlot, Is.Not.Null);
            Assert.That(freePlot.transform.localScale.x, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(freePlot.transform.localScale.y, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(freePlot.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("farm_plot_Tilled"));
            GameSession.Instance.SelectTool(FarmTool.Shovel);
            yield return null;
            Assert.That(player.HeldItemPresenter.DisplayedTool, Is.EqualTo(FarmTool.Shovel));
            Assert.That(player.HeldItemPresenter.ItemRenderer.sprite, Is.Not.Null);
            GameSession.Instance.UsePlot(0);
            yield return null;
            Assert.That(GameSession.Instance.Data.farmPlots, Is.Empty);
            Assert.That(GameObject.Find("Free Plot 1"), Is.Null);

            Assert.That(GameSession.Instance.AddFreeformPlot(new Vector2(14.5f, -3.5f)), Is.True);
            GameSession.Instance.Data.farmPlots[0].Plant(GameItem.PotatoSeed);
            GameSession.Instance.UsePlot(0);
            Assert.That(GameSession.Instance.Data.farmPlots, Has.Count.EqualTo(1),
                "The shovel must not remove a plot while a crop is planted.");
            GameSession.Instance.Data.money = 9999;
            GameSession.Instance.PurchaseChicken();
            GameSession.Instance.PurchaseCow();
            var chickensBeforeSale = GameSession.Instance.Data.ownedChickens;
            var moneyBeforeChickenSale = GameSession.Instance.Data.money;
            Assert.That(GameSession.Instance.SellChicken(), Is.True);
            yield return null;

            Assert.That(GameSession.Instance.Data.ownedChickens, Is.EqualTo(chickensBeforeSale - 1));
            Assert.That(GameSession.Instance.Data.money, Is.EqualTo(moneyBeforeChickenSale + 100));
            Assert.That(GameSession.Instance.SellChicken(), Is.False, "The last chicken must remain on the farm.");
            Assert.That(GameSession.Instance.Data.ownedChickens, Is.EqualTo(1));

            var chickenHerd = GameObject.Find("Chicken Herd").GetComponent<FarmAnimalHerd>();
            var cowHerd = GameObject.Find("Cow Herd").GetComponent<FarmAnimalHerd>();
            Assert.That(chickenHerd.VisibleCount, Is.EqualTo(GameSession.Instance.Data.ownedChickens));
            Assert.That(cowHerd.VisibleCount, Is.EqualTo(GameSession.Instance.Data.ownedCows));
            Assert.That(Object.FindObjectsByType<ChickenInteractable>(FindObjectsSortMode.None).Length,
                Is.EqualTo(GameSession.Instance.Data.ownedChickens));
            Assert.That(Object.FindObjectsByType<CowInteractable>(FindObjectsSortMode.None).Length,
                Is.EqualTo(GameSession.Instance.Data.ownedCows));

            var cow = GameObject.Find("Cow");
            Assert.That(cow.GetComponent<CowInteractable>(), Is.Not.Null);
            Assert.That(cow.GetComponent<WanderingMover>(), Is.Not.Null);
            Assert.That(cow.GetComponent<WorldDepthSorter>(), Is.Not.Null);
            Assert.That(cow.GetComponent<WanderingMover>().RoamingArea.Contains(cow.transform.position), Is.True);
            Assert.That(cow.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("world_Cow"));
            Assert.That(cow.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "Cow must use illustrated art instead of a prototype block.");
            var miningNode = GameObject.Find("Mining Node 1");
            Assert.That(miningNode.GetComponent<MiningNodeInteractable>(), Is.Not.Null);
            Assert.That(miningNode.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("world_MiningRock"));
            Assert.That(miningNode.GetComponent<SpriteRenderer>().sprite.texture.width, Is.GreaterThan(1),
                "Mining nodes must use illustrated rock art instead of prototype blocks.");
            var miningRenderers = Object.FindObjectsByType<MiningNodeInteractable>(FindObjectsSortMode.None);
            for (var treeIndex = 1; treeIndex <= 18; treeIndex++)
            {
                var treeRenderer = GameObject.Find($"Seeded Tree {treeIndex}").GetComponent<SpriteRenderer>();
                foreach (var node in miningRenderers)
                {
                    Assert.That(treeRenderer.bounds.Intersects(node.GetComponent<SpriteRenderer>().bounds), Is.False,
                        $"{treeRenderer.name} canopy must not overlap {node.name}.");
                }
            }
            Assert.That(GameObject.Find("General Store Entrance").GetComponent<ShopInteractable>(), Is.Not.Null);
            var villageSquare = GameObject.Find("Village Square");
            Assert.That(villageSquare, Is.Not.Null);
            Assert.That(villageSquare.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(2),
                "The blue plaza artwork must render as walkable ground beneath the player.");
            Assert.That(villageSquare.GetComponent<Collider2D>(), Is.Null,
                "The plaza floor itself must not block the player.");
            var boardArt = GameObject.Find("Village Square Noticeboard Art");
            Assert.That(boardArt, Is.Not.Null,
                "The red noticeboard artwork must be separated from the walkable plaza layer.");
            Assert.That(boardArt.GetComponent<SpriteRenderer>().sprite.name,
                Is.EqualTo("world_VillageSquareNoticeboard"));
            Assert.That(boardArt.GetComponent<SpriteRenderer>().sortingOrder,
                Is.EqualTo(WorldDepthSorter.CalculateOrder(villageSquare.transform.position.y + 3.15f,
                    WorldDepthSorter.BuildingDepthOffset)),
                "Only the red noticeboard section should use foreground depth sorting.");
            Assert.That(GameObject.Find("Village Square Noticeboard Blocker").GetComponent<BoxCollider2D>().isTrigger,
                Is.False, "The physical noticeboard must remain solid.");
            Assert.That(GameObject.Find("Quest Board").GetComponent<QuestBoardInteractable>(), Is.Not.Null);
            var questBoard = GameObject.Find("Quest Board").GetComponent<QuestBoardInteractable>();
            Assert.That(questBoard.GetComponent<Collider2D>().isTrigger, Is.True,
                "The interaction area in front of the noticeboard must not block movement.");
            Assert.That(questBoard.GetComponent<Collider2D>().bounds.min.y, Is.LessThanOrEqualTo(2.6f),
                "The quest-board interaction area must extend down to the player's approach path.");
            player.Teleport(new Vector2(0f, 2.65f));
            yield return new WaitForSeconds(1.25f);
            Assert.That(player.VisibleNearbyPrompt, Does.Contain("의뢰 게시판"),
                "The E interaction hint must remain visible while the player stays near the board.");
            questBoard.Interact(player);
            Assert.That(GameSession.Instance.QuestBoardOpen, Is.True);
            Assert.That(StarterQuestProgress.GetAll(GameSession.Instance.Data), Has.Length.EqualTo(5));
            GameSession.Instance.CloseQuestBoard();

            var fishingSpot = GameObject.Find("Pond Fishing Spot").GetComponent<FishingSpotInteractable>();
            Assert.That(fishingSpot, Is.Not.Null);
            var fishBefore = GameSession.Instance.Data.fishCaught;
            GameSession.Instance.SelectTool(FarmTool.FishingRod);
            fishingSpot.Interact(player);
            yield return new WaitForSeconds(1.8f);
            Assert.That(GameSession.Instance.Data.fishCaught, Is.EqualTo(fishBefore + 1));

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

            var squareNoticeboardBlocker = GameObject.Find("Village Square Noticeboard Blocker");
            Assert.That(squareNoticeboardBlocker, Is.Not.Null,
                "The village noticeboard needs a physical blocker so NPCs cannot walk through it.");
            Assert.That(squareNoticeboardBlocker.GetComponent<BoxCollider2D>().isTrigger, Is.False);
            foreach (var decorationBlocker in new[]
                     {
                         "Village Square Left Bench Blocker",
                         "Village Square Right Bench Blocker",
                         "Village Square Flower Bed Blocker",
                         "Village Square Upper Left Planter Blocker",
                         "Village Square Upper Right Planter Blocker",
                         "Village Square Lower Left Planter Blocker",
                         "Village Square Lower Right Planter Blocker"
                     })
            {
                Assert.That(GameObject.Find(decorationBlocker), Is.Null,
                    $"{decorationBlocker} must not block the route to the quest board.");
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
            yield return null;
            var interiorCamera = Camera.main;
            Assert.That(interiorCamera.orthographicSize, Is.LessThanOrEqualTo(3.6f),
                "Entering the farmhouse should switch to a close interior camera so the room fills the screen.");
            Assert.That(interiorCamera.transform.position.y, Is.EqualTo(WorldAreaProfile.FarmhouseInteriorCenter.y).Within(0.1f),
                "The interior camera should frame the room center instead of following the player at the doorway.");
            Assert.That(interiorCamera.GetComponent<PixelCameraFollow>().ReferenceHeight,
                Is.EqualTo(WorldAreaProfile.InteriorReferenceHeight),
                "The farmhouse should use a closer pixel-perfect reference resolution.");
            var interiorLeftEdge = interiorCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x;
            Assert.That(interiorLeftEdge, Is.GreaterThan(29f),
                "The indoor camera must not reveal the outdoor ocean tilemap at the left edge.");
            var farmhouseExit = GameObject.Find("Farm House Exit").GetComponent<DoorInteractable>();
            farmhouseExit.Interact(player);
            yield return null;
            Assert.That(interiorCamera.orthographicSize, Is.EqualTo(6.75f).Within(0.01f),
                "Leaving the farmhouse should restore the outdoor camera view.");
            Assert.That(interiorCamera.GetComponent<PixelCameraFollow>().ReferenceHeight,
                Is.EqualTo(WorldAreaProfile.OutdoorReferenceHeight));

            var guideDialogue = guide.GetComponent<DialogueInteractable>();
            Assert.That(guideDialogue.DialogueCount, Is.GreaterThanOrEqualTo(5));
            guideDialogue.Interact(player);
            Assert.That(GameSession.Instance.ActivePortraitRow, Is.EqualTo(0));
            Assert.That(GameSession.Instance.ActiveSpeaker, Is.EqualTo("안내자 하람"));
            Assert.That(GameSession.Instance.Message, Does.StartWith("I로 가방"));
            var firstGuideLine = GameSession.Instance.Message;
            guideDialogue.Interact(player);
            Assert.That(GameSession.Instance.Message, Is.Not.EqualTo(firstGuideLine),
                "Talking to an NPC repeatedly should reveal more than one line.");

            fisher.GetComponent<DialogueInteractable>().Interact(player);
            Assert.That(GameSession.Instance.ActivePortraitRow, Is.EqualTo(4));
            Assert.That(GameSession.Instance.ActiveSpeaker, Is.EqualTo("어부 민서"));
            florist.GetComponent<DialogueInteractable>().Interact(player);
            Assert.That(GameSession.Instance.ActivePortraitRow, Is.EqualTo(5));
            Assert.That(GameSession.Instance.ActiveSpeaker, Is.EqualTo("원예가 태오"));
        }
    }
}
