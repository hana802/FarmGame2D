using FarmGame2D.Core;
using FarmGame2D.Farming;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class NpcFriendshipTests
    {
        [Test]
        public void ReadingAnUnknownResidentStartsAtZeroWithoutAddingASaveRecord()
        {
            var data = new GameSaveData();

            Assert.That(NpcFriendshipService.GetPoints(data, "Guide"), Is.Zero);
            Assert.That(NpcFriendshipService.GetHeartCount(data, "Guide"), Is.Zero);
            Assert.That(data.npcFriendships, Is.Empty);
        }

        [Test]
        public void FavoriteGift_ConsumesItemAndRaisesFriendship()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.inventory.Add(GameItem.Wheat, 2);

            var result = NpcFriendshipService.TryGiveGift(data, "Baker Yuna", GameItem.Wheat);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Reaction, Is.EqualTo(NpcGiftReaction.Loved));
            Assert.That(result.PointsAdded, Is.EqualTo(20));
            Assert.That(data.inventory.Count(GameItem.Wheat), Is.EqualTo(1));
            Assert.That(NpcFriendshipService.GetPoints(data, "Baker Yuna"), Is.EqualTo(20));
        }

        [Test]
        public void SecondGiftOnSameDay_IsRejectedWithoutConsumingItem()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.inventory.Add(GameItem.Milk, 2);
            NpcFriendshipService.TryGiveGift(data, "Rancher Sora", GameItem.Milk);

            var secondGift = NpcFriendshipService.TryGiveGift(data, "Rancher Sora", GameItem.Milk);

            Assert.That(secondGift.Success, Is.False);
            Assert.That(data.inventory.Count(GameItem.Milk), Is.EqualTo(1));
            Assert.That(NpcFriendshipService.GetPoints(data, "Rancher Sora"), Is.EqualTo(20));
        }

        [Test]
        public void DifferentNpc_CanReceiveGiftOnSameDay()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.inventory.Add(GameItem.Strawberry, 2);

            var guideGift = NpcFriendshipService.TryGiveGift(data, "Guide", GameItem.Strawberry);
            var floristGift = NpcFriendshipService.TryGiveGift(data, "Florist Taeo", GameItem.Strawberry);

            Assert.That(guideGift.Success, Is.True);
            Assert.That(floristGift.Success, Is.True);
        }

        [Test]
        public void PreferenceList_NamesTheGiftsThatRaiseFriendship()
        {
            CollectionAssert.AreEqual(new[] { GameItem.Potato },
                NpcFriendshipService.GetPreferredItems("Guide", NpcGiftReaction.Loved));
            CollectionAssert.AreEqual(new[] { GameItem.Strawberry, GameItem.GoldenCarp },
                NpcFriendshipService.GetPreferredItems("Guide", NpcGiftReaction.Liked));
            CollectionAssert.AreEqual(new[] { GameItem.Wheat, GameItem.MarketBread },
                NpcFriendshipService.GetPreferredItems("Baker Yuna", NpcGiftReaction.Loved));
            CollectionAssert.AreEqual(new[] { GameItem.MarketBread },
                FoodItems(NpcFriendshipService.GetFoodPreferences("Baker Yuna")));
            CollectionAssert.AreEqual(new[] { GameItem.PotatoSoup, GameItem.BakedSweetPotato },
                FoodItems(NpcFriendshipService.GetFoodPreferences("Elder")));
            Assert.That(NpcFriendshipService.GetPreferredItems("Guide", NpcGiftReaction.Neutral), Is.Empty);
        }

        private static GameItem[] FoodItems(System.Collections.Generic.IReadOnlyList<FoodPreference> foods)
        {
            var items = new GameItem[foods.Count];
            for (var index = 0; index < foods.Count; index++) items[index] = foods[index].Item;
            return items;
        }

        [Test]
        public void GiftableInventory_ExcludesSeedsAndFeed()
        {
            var inventory = new InventoryModel();
            inventory.Add(GameItem.PotatoSeed, 2);
            inventory.Add(GameItem.Feed, 1);
            inventory.Add(GameItem.Potato, 1);

            var giftableItems = NpcFriendshipService.GetOwnedGiftableItems(inventory);

            CollectionAssert.Contains(giftableItems, GameItem.Potato);
            CollectionAssert.DoesNotContain(giftableItems, GameItem.PotatoSeed);
            CollectionAssert.DoesNotContain(giftableItems, GameItem.Feed);
        }

        [Test]
        public void Normalizer_InitializesMissingFriendshipList()
        {
            var data = GameSaveData.CreateNew(FarmFieldLayout.PlotCount, 1234);
            data.npcFriendships = null;

            GameSaveDataNormalizer.Normalize(data);

            Assert.That(data.npcFriendships, Is.Not.Null);
            Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        }
    }
}
