using System;
using System.IO;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.Interactions;
using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.Art;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests
{
    public sealed class GameplayDomainTests
    {
        [Test]
        public void Inventory_AddAndSpend_TracksQuantity()
        {
            var inventory = new InventoryModel();
            inventory.Add(GameItem.WheatSeed, 5);

            var spent = inventory.TrySpend(GameItem.WheatSeed, 2);

            Assert.That(spent, Is.True);
            Assert.That(inventory.Count(GameItem.WheatSeed), Is.EqualTo(3));
        }

        [Test]
        public void QuestBoard_AllowsChosenQuestAndPaysRewardOnce()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            var startingMoney = data.money;

            Assert.That(StarterQuestProgress.TryAccept(data, StarterQuestId.Harvest), Is.True);
            data.wheatHarvested = 3;
            var harvest = StarterQuestProgress.TryCompleteCurrent(data);
            Assert.That(harvest.Reward, Is.EqualTo(100));
            Assert.That(data.money, Is.EqualTo(startingMoney + 100));

            var duplicate = StarterQuestProgress.TryCompleteCurrent(data);
            Assert.That(duplicate.Reward, Is.Zero);
            Assert.That(StarterQuestProgress.TryAccept(data, StarterQuestId.Harvest), Is.False);
            Assert.That(StarterQuestProgress.TryAccept(data, StarterQuestId.Fishing), Is.True);
            Assert.That(data.money, Is.EqualTo(startingMoney + 100));
        }

        [Test]
        public void FreeformPlot_PositionSurvivesNormalization()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 99);
            data.farmPlots.Add(new FarmPlotData { freeform = true, worldX = 14.5f, worldY = -3.5f, state = SoilState.Tilled });

            GameSaveDataNormalizer.Normalize(data);

            Assert.That(data.farmPlots, Has.Count.EqualTo(FarmFieldLayout.PlotCount + 1));
            Assert.That(FarmFieldLayout.GetWorldPosition(data.farmPlots[^1]), Is.EqualTo(new Vector2(14.5f, -3.5f)));
        }

        [Test]
        public void AnimalBreeding_IsDeterministicAndRequiresTwoAdults()
        {
            Assert.That(AnimalBreedingRules.ShouldBreed(1234, 4, FarmAnimalKind.Chicken, 1, 2), Is.False);
            var first = AnimalBreedingRules.ShouldBreed(1234, 17, FarmAnimalKind.Chicken, 2, 2);
            var repeated = AnimalBreedingRules.ShouldBreed(1234, 17, FarmAnimalKind.Chicken, 2, 2);
            Assert.That(repeated, Is.EqualTo(first));
        }

        [Test]
        public void Fish_AreSellableInventoryItems()
        {
            Assert.That(ItemCatalog.GetSellPrice(GameItem.RiverFish), Is.GreaterThan(0));
            Assert.That(ItemCatalog.GetSellPrice(GameItem.SeaBass), Is.GreaterThan(ItemCatalog.GetSellPrice(GameItem.RiverFish)));
            Assert.That(ItemCatalog.GetSellPrice(GameItem.GoldenCarp), Is.GreaterThan(ItemCatalog.GetSellPrice(GameItem.SeaBass)));
        }

        [Test]
        public void WateredPotato_BecomesReadyAfterTwoDays()
        {
            var plot = new FarmPlotData { state = SoilState.Watered, plantedSeed = GameItem.PotatoSeed };

            plot.AdvanceDay();
            plot.state = SoilState.Watered;
            plot.AdvanceDay();

            Assert.That(plot.state, Is.EqualTo(SoilState.Ready));
        }

        [TestCase(Season.Spring, GameItem.PotatoSeed, GameItem.StrawberrySeed)]
        [TestCase(Season.Summer, GameItem.TomatoSeed, GameItem.CornSeed)]
        [TestCase(Season.Autumn, GameItem.PumpkinSeed, GameItem.SweetPotatoSeed)]
        public void SeasonalShop_OffersExpectedSeeds(Season season, GameItem first, GameItem second)
        {
            var crops = CropCatalog.GetForSeason(season);

            Assert.That(crops.Count, Is.EqualTo(2));
            Assert.That(crops[0].Seed, Is.EqualTo(first));
            Assert.That(crops[1].Seed, Is.EqualTo(second));
        }

        [Test]
        public void WinterShop_OffersWinterWheat()
        {
            var crops = CropCatalog.GetForSeason(Season.Winter);

            Assert.That(crops.Count, Is.EqualTo(1));
            Assert.That(crops[0].Seed, Is.EqualTo(GameItem.WheatSeed));
        }

        [Test]
        public void SaveNormalizer_ExpandsLegacyFarmAndCreatesMiningNodes()
        {
            var data = GameSaveData.CreateNew(6, 1234);

            GameSaveDataNormalizer.Normalize(data);

            Assert.That(data.farmPlots, Has.Count.EqualTo(FarmFieldLayout.PlotCount));
            Assert.That(data.miningNodes, Has.Count.EqualTo(6));
            Assert.That(data.farmPlots[23].gridX, Is.EqualTo(5));
            Assert.That(data.farmPlots[23].gridY, Is.EqualTo(3));
        }

        [Test]
        public void SaveService_LoadsCurrentVersionWithoutResettingPlayerProgress()
        {
            var savePath = Path.Combine(Path.GetTempPath(), $"farmgame2d-save-{Guid.NewGuid():N}.json");
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.money = 100;
            data.ownedChickens = 2;
            data.ownedCows = 1;
            File.WriteAllText(savePath, JsonUtility.ToJson(data));

            try
            {
                var loaded = new SaveService(savePath).TryLoad(out var restored);

                Assert.That(loaded, Is.True);
                Assert.That(restored.money, Is.EqualTo(100));
                Assert.That(restored.ownedChickens, Is.EqualTo(2));
                Assert.That(restored.ownedCows, Is.EqualTo(1));
            }
            finally
            {
                File.Delete(savePath);
            }
        }

        [Test]
        public void SaveService_RejectsUnknownFutureVersionWithoutChangingTheFile()
        {
            var savePath = Path.Combine(Path.GetTempPath(), $"farmgame2d-future-{Guid.NewGuid():N}.json");
            var originalJson = $"{{\"version\":{GameSaveData.CurrentVersion + 1},\"money\":777}}";
            File.WriteAllText(savePath, originalJson);

            try
            {
                var loaded = new SaveService(savePath).TryLoad(out _);

                Assert.That(loaded, Is.False);
                Assert.That(File.ReadAllText(savePath), Is.EqualTo(originalJson));
            }
            finally
            {
                File.Delete(savePath);
            }
        }

        [Test]
        public void CloudSaveClient_NormalizesHumanFriendlyCodeAndPin()
        {
            Assert.That(CloudSaveClient.NormalizeCode("r6fx-2haq"), Is.EqualTo("R6FX2HAQ"));
            Assert.That(CloudSaveClient.NormalizePin("12 34-56"), Is.EqualTo("123456"));
            Assert.That(CloudSaveClient.IsValidCode("R6FX2HAQ"), Is.True);
            Assert.That(CloudSaveClient.IsValidPin("123456"), Is.True);
        }

        [TestCase("O0IL2HAQ")]
        [TestCase("TOO-SHORT")]
        [TestCase("")]
        public void CloudSaveClient_RejectsAmbiguousOrMalformedCodes(string code)
        {
            Assert.That(CloudSaveClient.IsValidCode(code), Is.False);
        }

        [Test]
        public void CloudSaveClient_DeviceSignatureProducesStableOpaqueFingerprint()
        {
            var first = CloudSaveClient.HashDeviceSignature("Windows|8|1920|1080|Korean|540");
            var second = CloudSaveClient.HashDeviceSignature("Windows|8|1920|1080|Korean|540");
            var different = CloudSaveClient.HashDeviceSignature("Windows|8|2560|1440|Korean|540");

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.Not.EqualTo(different));
            Assert.That(CloudSaveClient.IsValidDeviceFingerprint(first), Is.True);
            Assert.That(first, Has.Length.EqualTo(64));
            Assert.That(first, Does.Not.Contain("Windows"));
        }

        [Test]
        public void SaveService_CloudRoundTripPreservesPlayerProgress()
        {
            var service = new SaveService();
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.money = 100;
            data.ownedChickens = 2;
            data.ownedCows = 1;

            Assert.That(service.TrySerialize(data, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.money, Is.EqualTo(100));
            Assert.That(restored.ownedChickens, Is.EqualTo(2));
            Assert.That(restored.ownedCows, Is.EqualTo(1));
        }

        [Test]
        public void VillageRoutes_FindConnectedStepTowardHome()
        {
            var network = new VillageWaypointNetwork(
                new[] { Vector2.zero, Vector2.right, Vector2.right * 2f, Vector2.up },
                new[] { new[] { 1, 3 }, new[] { 0, 2 }, new[] { 1 }, new[] { 0 } });

            Assert.That(network.GetNextNodeToward(3, 2), Is.EqualTo(0));
            Assert.That(network.GetNextNodeToward(0, 2), Is.EqualTo(1));
        }

        [Test]
        public void MiningNode_RegrowsAfterTwoDays()
        {
            var node = new MiningNodeData { ore = GameItem.CopperOre };

            node.Mine();
            node.AdvanceDay();
            Assert.That(node.IsAvailable, Is.False);
            node.AdvanceDay();

            Assert.That(node.IsAvailable, Is.True);
        }

        [Test]
        public void TreePlacement_ReservesCanopySpaceAroundMiningArea()
        {
            Assert.That(WorldPlacementRules.CanPlaceTree(new Vector2(-13f, -13f)), Is.False,
                "A tree rooted just below the mine overlaps the lower rocks with its canopy.");
        }

        [Test]
        public void TreePlacement_ReservesCanopySpaceAroundAnimalPens()
        {
            Assert.That(WorldPlacementRules.CanPlaceTree(new Vector2(-7f, -12f)), Is.False,
                "Trees beside the animal pens let chickens and cows walk through their canopy.");
        }

        [Test]
        public void WanderArea_ClampsActorsInsideTheirAllowedSpace()
        {
            var area = new WanderArea(new Vector2(2f, 3f), new Vector2(1f, 2f));

            var clamped = area.Clamp(new Vector2(10f, -10f));

            Assert.That(clamped, Is.EqualTo(new Vector2(3f, 1f)));
            Assert.That(area.Contains(clamped), Is.True);
            Assert.That(area.Contains(new Vector2(3.1f, 1f)), Is.False);
        }

        [Test]
        public void WanderArea_DoesNotTeleportActorWhenScheduleChanges()
        {
            var nighttimeArea = new WanderArea(new Vector2(10f, 10f), Vector2.one);
            var current = Vector2.zero;
            var oneMovementStep = new Vector2(0.05f, 0.05f);

            var constrained = nighttimeArea.ConstrainStep(current, oneMovementStep);

            Assert.That(constrained, Is.EqualTo(oneMovementStep));
        }

        [Test]
        public void WorldDepth_LowerActorsRenderInFront()
        {
            var upperOrder = WorldDepthSorter.CalculateOrder(5f);
            var lowerOrder = WorldDepthSorter.CalculateOrder(2f);

            Assert.That(lowerOrder, Is.GreaterThan(upperOrder));
        }

        [Test]
        public void CropAndAnimalProducts_HaveSellPrices()
        {
            Assert.That(ItemCatalog.GetSellPrice(GameItem.Strawberry), Is.EqualTo(35));
            Assert.That(ItemCatalog.GetSellPrice(GameItem.Milk), Is.EqualTo(55));
            Assert.That(ItemCatalog.GetSellPrice(GameItem.GoldOre), Is.EqualTo(60));
        }

        [TestCase(1, Season.Spring)]
        [TestCase(29, Season.Summer)]
        [TestCase(57, Season.Autumn)]
        [TestCase(85, Season.Winter)]
        public void SeasonCalendar_ChangesEveryTwentyEightDays(int day, Season expected)
        {
            Assert.That(SeasonCalendar.GetSeason(day), Is.EqualTo(expected));
        }

        [Test]
        public void GameClock_Set_RestoresSavedTime()
        {
            var clock = new FarmGame2D.TimeSystem.GameClock();

            clock.Set(4, 13, 25);

            Assert.That(clock.Day, Is.EqualTo(4));
            Assert.That(clock.Hour, Is.EqualTo(13));
            Assert.That(clock.Minute, Is.EqualTo(25));
        }

        [Test]
        public void Inventory_RejectsInvalidAddQuantity()
        {
            var inventory = new InventoryModel();
            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add(GameItem.Wheat, 0));
        }

        [Test]
        public void Inventory_StoresPulledWeeds()
        {
            var inventory = new InventoryModel();

            inventory.Add(GameItem.Weed, 2);

            Assert.That(inventory.Count(GameItem.Weed), Is.EqualTo(2));
        }

        [TestCase(0f, 1)]
        [TestCase(0.17f, 2)]
        [TestCase(0.34f, 3)]
        [TestCase(0.51f, 2)]
        public void WalkingAnimation_DoesNotMixIdleFrame(float elapsed, int expectedFrame)
        {
            Assert.That(PlayerMotionPresentation.GetAnimationFrame(elapsed, true), Is.EqualTo(expectedFrame));
            Assert.That(PlayerMotionPresentation.GetAnimationFrame(elapsed, false), Is.Zero);
        }

        [TestCase(1f, 1f, 1)]
        [TestCase(-1f, 1f, 2)]
        [TestCase(1f, -1f, 1)]
        [TestCase(-1f, -1f, 2)]
        public void DiagonalMovement_UsesSideFacingSprite(float horizontal, float vertical, int expectedRow)
        {
            var row = PlayerMotionPresentation.GetFacingRow(new Vector2(horizontal, vertical), 3);

            Assert.That(row, Is.EqualTo(expectedRow));
        }

        [Test]
        public void NpcSpeechBubble_UsesWhiteBackgroundAndDarkText()
        {
            var darkBase = new GUIStyle();
            darkBase.normal.background = Texture2D.blackTexture;

            var style = NpcSpeechBubblePresentation.CreateStyle(darkBase, null);

            Assert.That(style.normal.background, Is.SameAs(Texture2D.whiteTexture));
            Assert.That(style.normal.textColor.grayscale, Is.LessThan(0.5f));
        }

        [Test]
        public void HudMessageVisibility_ExpiresUntilMessageRevisionChanges()
        {
            var visibility = new HudMessageVisibility();

            Assert.That(visibility.ShouldShow(0, 0f, 1f), Is.True);
            Assert.That(visibility.ShouldShow(0, 1.1f, 1f), Is.False);
            Assert.That(visibility.ShouldShow(1, 1.1f, 4f), Is.True);
            Assert.That(visibility.ShouldShow(1, 4.9f, 4f), Is.True);
        }

        [Test]
        public void FrontFacingSprite_UsesMeasuredRowAndKeepsFeet()
        {
            var sprite = GameArtCatalog.GetPlayerSprite(CharacterChoice.Male, 3, 0);

            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.rect.x, Is.EqualTo(89f));
            Assert.That(sprite.rect.y, Is.EqualTo(712f));
            Assert.That(sprite.rect.width, Is.EqualTo(160f));
            Assert.That(sprite.rect.height, Is.EqualTo(208f));
        }

        [Test]
        public void HudLayout_FitsAndCentersReferenceCanvas()
        {
            var viewport = HudLayout.CalculateViewport(1000f, 1000f);

            Assert.That(viewport.Scale, Is.EqualTo(0.78125f).Within(0.0001f));
            Assert.That(viewport.OffsetX, Is.Zero.Within(0.0001f));
            Assert.That(viewport.OffsetY, Is.EqualTo(218.75f).Within(0.0001f));
        }

        [Test]
        public void DialogueLayout_SeparatesPortraitNameAndBody()
        {
            var layout = HudLayout.CalculateDialogueLayout();

            Assert.That(layout.Panel.width, Is.LessThan(HudLayout.ReferenceWidth));
            Assert.That(layout.Panel.yMax, Is.LessThan(650f), "The dialogue card must not cover the hotbar.");
            Assert.That(layout.Panel.width / layout.Panel.height,
                Is.EqualTo(1618f / 356f).Within(0.01f),
                "The dialogue atlas must keep its original aspect ratio so its visual slots stay aligned.");
            Assert.That(layout.Portrait.xMax, Is.LessThan(layout.Body.xMin));
            Assert.That(layout.Name.yMax, Is.LessThanOrEqualTo(layout.Body.yMin));
            Assert.That(layout.Name.width, Is.GreaterThanOrEqualTo(220f));
            Assert.That(layout.Body.width, Is.GreaterThanOrEqualTo(760f));
        }

        [Test]
        public void InventoryLayout_KeepsTextAndControlsInsideArtworkSlots()
        {
            var layout = HudLayout.CalculateInventoryLayout();

            Assert.That(layout.Title.xMin, Is.GreaterThanOrEqualTo(layout.Panel.xMin + 120f),
                "The title must clear the decorative icon on the left side of the header.");
            Assert.That(layout.Title.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 28f),
                "The title must sit inside the green header instead of over the top border.");
            Assert.That(layout.Title.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 58f),
                "The title baseline must clear the illustrated top frame.");
            Assert.That(layout.Description.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 115f),
                "The description must sit below the header border inside the white content area.");
            Assert.That(layout.ToolButtonsY, Is.GreaterThanOrEqualTo(layout.Description.yMax + 10f));
            Assert.That(layout.GetSeedButton(0).yMin, Is.GreaterThanOrEqualTo(layout.HoeButton.yMax + 18f));
            Assert.That(layout.FirstStatsY, Is.GreaterThanOrEqualTo(layout.GetSeedButton(6).yMax + 8f));
            Assert.That(layout.GetStatsLabel(0, 0).yMax, Is.LessThanOrEqualTo(layout.CloseButton.yMin),
                "The final inventory row must clear the close button.");
            Assert.That(layout.GetStatsLabel(0, 0).yMax, Is.LessThanOrEqualTo(layout.Panel.yMin + 432f),
                "Owned-item text must clear the illustrated body/footer divider.");
            Assert.That(layout.CloseButton.xMin, Is.InRange(layout.Panel.xMin + 225f, layout.Panel.xMin + 255f),
                "The close action must use the illustrated lower-left button slot.");
            Assert.That(layout.CloseButton.yMin, Is.InRange(layout.Panel.yMin + 442f, layout.Panel.yMin + 462f),
                "The close action must sit inside the illustrated footer slot.");
            Assert.That(layout.CloseButton.yMax, Is.LessThanOrEqualTo(layout.Panel.yMax - 18f));
        }

        [Test]
        public void PhoneLayout_KeepsQuestTextBelowHeaderArtwork()
        {
            var layout = HudLayout.CalculatePhoneLayout();

            Assert.That(layout.Title.xMin, Is.GreaterThanOrEqualTo(layout.Panel.xMin + 120f),
                "The phone title must clear the illustrated portrait slot.");
            Assert.That(layout.QuestBody.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 100f),
                "Quest lines must start below the illustrated header.");
            Assert.That(layout.QuestBody.yMax, Is.LessThanOrEqualTo(layout.SaveButton.yMin),
                "Quest lines must clear the phone action buttons.");
            Assert.That(layout.CloseButton.yMax, Is.LessThanOrEqualTo(layout.Panel.yMax - 18f));
        }

        [Test]
        public void ToastLayout_KeepsMessageClearOfBannerDecoration()
        {
            var layout = HudLayout.CalculateToastLayout();

            Assert.That(layout.Message.xMin, Is.GreaterThanOrEqualTo(layout.Panel.xMin + 140f),
                "Toast text must clear the decorative badge on the left side of the banner.");
            Assert.That(layout.Message.xMax, Is.LessThanOrEqualTo(layout.Panel.xMax - 32f));
            Assert.That(layout.Message.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 12f));
            Assert.That(layout.Message.yMax, Is.LessThanOrEqualTo(layout.Panel.yMax - 12f));
        }

        [Test]
        public void ShopLayout_KeepsHeaderTextInsideArtworkSafeArea()
        {
            var layout = HudLayout.CalculateShopLayout();

            Assert.That(layout.Title.xMin, Is.GreaterThanOrEqualTo(layout.Panel.xMin + 120f),
                "The shop title must clear the illustrated decoration on the left side of the header.");
            Assert.That(layout.Title.yMin, Is.GreaterThanOrEqualTo(layout.Panel.yMin + 58f),
                "The shop title must sit below the illustrated top frame.");
            Assert.That(layout.GetProductButton(0).yMin, Is.GreaterThanOrEqualTo(layout.Title.yMax + 24f),
                "The first product must start below the header.");
            Assert.That(layout.CloseButton.yMax, Is.LessThanOrEqualTo(layout.Panel.yMax - 18f));
        }

        [Test]
        public void ShopLayout_UsesCenteredHeaderAndSingleFooterAction()
        {
            var layout = HudLayout.CalculateShopLayout();

            Assert.That(PrototypeHud.ShopTitle, Is.EqualTo("상점"));
            Assert.That(layout.Title.center.x, Is.EqualTo(layout.Panel.center.x).Within(0.01f),
                "The shop title must be centered on the whole panel.");
            Assert.That(layout.CloseButton.center.x, Is.EqualTo(layout.Panel.center.x).Within(0.01f));
            Assert.That(layout.CloseButton.width, Is.GreaterThanOrEqualTo(270f),
                "One close button must cover both baked footer slots.");
            Assert.That(layout.CloseButton.yMin, Is.InRange(layout.Panel.yMin + 440f, layout.Panel.yMin + 462f),
                "The close button must sit over the two illustrated footer slots.");
        }

        [Test]
        public void CharacterChoice_UsesRequestedHeaderAndPrompt()
        {
            Assert.That(PrototypeHud.CharacterChoiceHeader, Is.EqualTo("선택해주세요."));
            Assert.That(PrototypeHud.CharacterChoicePrompt, Is.EqualTo("당신의 성별을 골라주세요"));
        }

        [Test]
        public void NpcDialogueSequence_CyclesThroughEveryLineBeforeRepeating()
        {
            var sequence = new NpcDialogueSequence(new[] { "첫 번째", "두 번째", "세 번째" });

            Assert.That(sequence.Count, Is.EqualTo(3));
            Assert.That(sequence.TakeNext(), Is.EqualTo("첫 번째"));
            Assert.That(sequence.TakeNext(), Is.EqualTo("두 번째"));
            Assert.That(sequence.TakeNext(), Is.EqualTo("세 번째"));
            Assert.That(sequence.TakeNext(), Is.EqualTo("첫 번째"));
        }
    }
}
