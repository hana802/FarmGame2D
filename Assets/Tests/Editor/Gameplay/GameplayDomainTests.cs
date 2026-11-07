using System;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using NUnit.Framework;

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
        public void WateredWheat_BecomesReadyAfterTwoDays()
        {
            var plot = new FarmPlotData { state = SoilState.Watered };

            plot.AdvanceDay();
            plot.state = SoilState.Watered;
            plot.AdvanceDay();

            Assert.That(plot.state, Is.EqualTo(SoilState.Ready));
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
    }
}
