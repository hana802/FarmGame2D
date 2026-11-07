using System;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.Player;
using FarmGame2D.UI;
using FarmGame2D.Art;
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

        [TestCase(0f, 1)]
        [TestCase(0.17f, 2)]
        [TestCase(0.34f, 3)]
        [TestCase(0.51f, 2)]
        public void WalkingAnimation_DoesNotMixIdleFrame(float elapsed, int expectedFrame)
        {
            Assert.That(PlayerMotionPresentation.GetAnimationFrame(elapsed, true), Is.EqualTo(expectedFrame));
            Assert.That(PlayerMotionPresentation.GetAnimationFrame(elapsed, false), Is.Zero);
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
            Assert.That(sprite.rect.y, Is.EqualTo(704f));
            Assert.That(sprite.rect.height, Is.EqualTo(256f));
        }

        [Test]
        public void HudLayout_FitsAndCentersReferenceCanvas()
        {
            var viewport = HudLayout.CalculateViewport(1000f, 1000f);

            Assert.That(viewport.Scale, Is.EqualTo(0.78125f).Within(0.0001f));
            Assert.That(viewport.OffsetX, Is.Zero.Within(0.0001f));
            Assert.That(viewport.OffsetY, Is.EqualTo(218.75f).Within(0.0001f));
        }
    }
}
