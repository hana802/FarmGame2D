using System.Linq;
using FarmGame2D.Core;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests
{
    public sealed class FarmhouseKitchenTests
    {
        private GameSession session;

        [SetUp]
        public void SetUp()
        {
            var owner = new GameObject("Farmhouse Kitchen Test Session");
            owner.SetActive(false);
            session = owner.AddComponent<GameSession>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(session.gameObject);

        [TestCase(GameItem.PotatoSoup, GameItem.Potato, GameItem.Milk, 123)]
        [TestCase(GameItem.TomatoOmelet, GameItem.Egg, GameItem.Tomato, 103)]
        [TestCase(GameItem.BakedSweetPotato, GameItem.SweetPotato, null, 82)]
        [TestCase(GameItem.WheatBread, GameItem.Wheat, GameItem.Egg, 99)]
        public void Kitchen_CooksAndSellsOrGiftsEachRecipeInEverySeason(
            GameItem result, GameItem first, GameItem? second, int price)
        {
            var recipe = BuildingUpgradeCatalog.Recipes.Single(entry => entry.Result == result);
            Assert.That(recipe.First, Is.EqualTo(first));
            Assert.That(recipe.Second, Is.EqualTo(second));
            var rawPrice = ItemCatalog.GetSellPrice(first) + (second is GameItem extra ? ItemCatalog.GetSellPrice(extra) : 0);
            Assert.That(price - rawPrice, Is.EqualTo(50));
            Assert.That(ItemCatalog.GetSellPrice(result), Is.EqualTo(price));

            for (var season = 0; season < 4; season++)
            {
                var data = new GameSaveData { houseUpgradeLevel = 2, day = 1 + season * SeasonCalendar.DaysPerSeason };
                UseSave(data);
                session.OpenFarmFacility(FarmFacility.Kitchen);
                Assert.That(session.TryCook(result), Is.False);

                data.inventory.Add(first, 2);
                if (second is GameItem ingredient)
                {
                    Assert.That(session.TryCook(result), Is.False);
                    Assert.That(data.inventory.Count(first), Is.EqualTo(2));
                    data.inventory.Add(ingredient, 2);
                }
                Assert.That(session.TryCook(result), Is.True);
                Assert.That(session.TryCook(result), Is.True);
                Assert.That(data.inventory.Count(first), Is.Zero);
                if (second is GameItem spent) Assert.That(data.inventory.Count(spent), Is.Zero);
                Assert.That(ProduceSale.Sell(data.inventory, result, 1), Is.EqualTo(price));
                Assert.That(NpcFriendshipService.TryGiveGift(data, "Guide", result).Success, Is.True);
                Assert.That(data.inventory.Count(result), Is.Zero);
            }
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        public void ExistingSave_RequestsNoUpgradePaymentAndUnlocksKitchenOnlyAtLevelTwo(int level, bool unlocked)
        {
            var save = JsonUtility.FromJson<GameSaveData>(
                "{\"version\":3,\"houseUpgradeLevel\":" + level +
                ",\"money\":700,\"bedroomChest\":{\"entries\":[{\"item\":38,\"quantity\":2}]}}");
            GameSaveDataNormalizer.Normalize(save);
            UseSave(save);
            save.inventory.Add(GameItem.SweetPotato, 1);

            Assert.That(session.TryCook(GameItem.BakedSweetPotato), Is.False);
            session.OpenFarmFacility(FarmFacility.Kitchen);
            Assert.That(session.OpenFacility, Is.EqualTo(unlocked ? FarmFacility.Kitchen : FarmFacility.None));
            Assert.That(session.TryCook(GameItem.BakedSweetPotato), Is.EqualTo(unlocked));
            Assert.That(save.inventory.Count(GameItem.SweetPotato), Is.EqualTo(unlocked ? 0 : 1));
            Assert.That(save.houseUpgradeLevel, Is.EqualTo(level));
            Assert.That(save.money, Is.EqualTo(700));
            Assert.That(save.bedroomChest.Count(GameItem.BakedSweetPotato), Is.EqualTo(2));
        }

        [TestCase(1, 2.4f, 1.7f, false)]
        [TestCase(2, 2.4f, 1.7f, true)]
        [TestCase(2, 0f, 1.7f, false)]
        [TestCase(2, 3.25f, 1.1f, false)]
        public void ExistingPosition_MovesOnlyIfTheNewCounterBlocksIt(int level, float x, float y, bool relocated)
        {
            var oldPosition = WorldAreaProfile.FarmhouseInteriorCenter + new Vector2(x, y);
            var save = new GameSaveData { houseUpgradeLevel = level, playerX = oldPosition.x, playerY = oldPosition.y };

            GameSaveDataNormalizer.Normalize(save);

            Assert.That(new Vector2(save.playerX, save.playerY),
                Is.EqualTo(relocated ? WorldAreaProfile.FarmhouseEntranceSpawn : oldPosition));
        }

        [Test]
        public void Rhythm_CompletesAfterThreeCenteredStrikes()
        {
            var rhythm = new CookingRhythm(GameItem.PotatoSoup);
            for (var hit = 0; hit < CookingRhythm.RequiredHits; hit++)
            {
                rhythm.Advance(CookingRhythm.SweepSeconds * 0.5f);
                Assert.That(rhythm.TryStrike(), Is.True);
            }

            Assert.That(rhythm.Succeeded, Is.True);
            Assert.That(rhythm.Hits, Is.EqualTo(CookingRhythm.RequiredHits));
        }

        [Test]
        public void Rhythm_MissesOutsideTheWindowWithoutAnotherChance()
        {
            var rhythm = new CookingRhythm(GameItem.WheatBread);
            rhythm.Advance(CookingRhythm.SweepSeconds * 0.1f);
            Assert.That(rhythm.TryStrike(), Is.False);
            Assert.That(rhythm.Resolved, Is.True);
            Assert.That(rhythm.Succeeded, Is.False);
            Assert.That(rhythm.TryStrike(), Is.False);
        }

        [Test]
        public void Rhythm_ExpiresWhenTheMarkerReachesTheEnd()
        {
            var rhythm = new CookingRhythm(GameItem.BakedSweetPotato);
            rhythm.Advance(CookingRhythm.SweepSeconds * 0.5f);
            Assert.That(rhythm.Resolved, Is.False);
            rhythm.Advance(CookingRhythm.SweepSeconds);
            Assert.That(rhythm.Resolved, Is.True);
            Assert.That(rhythm.Succeeded, Is.False);
        }

        private void UseSave(GameSaveData save)
        {
            session.CloseFarmFacility();
            typeof(GameSession).GetProperty(nameof(GameSession.Data)).SetValue(session, save);
        }
    }
}
