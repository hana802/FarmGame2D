using System;
using FarmGame2D.TimeSystem;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class GameClockTests
    {
        [Test]
        public void NewClock_StartsOnDayOneAtSix()
        {
            var clock = new GameClock();

            Assert.That(clock.Day, Is.EqualTo(1));
            Assert.That(clock.Hour, Is.EqualTo(6));
            Assert.That(clock.Minute, Is.Zero);
        }

        [Test]
        public void AdvanceMinutes_AddsGameTime()
        {
            var clock = new GameClock();

            clock.AdvanceMinutes(10);

            Assert.That(clock.Hour, Is.EqualTo(6));
            Assert.That(clock.Minute, Is.EqualTo(10));
        }

        [Test]
        public void AdvanceMinutes_PassesMidnightAndStartsNextDay()
        {
            var clock = new GameClock(23, 50);

            clock.AdvanceMinutes(10);

            Assert.That(clock.Day, Is.EqualTo(2));
            Assert.That(clock.Hour, Is.Zero);
            Assert.That(clock.Minute, Is.Zero);
        }

        [Test]
        public void AdvanceMinutes_RejectsNonPositiveValues()
        {
            var clock = new GameClock();

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceMinutes(0));
        }
    }
}
