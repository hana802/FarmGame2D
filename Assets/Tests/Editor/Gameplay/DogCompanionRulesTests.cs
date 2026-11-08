using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class DogCompanionRulesTests
    {
        [Test]
        public void MorningCare_FeedsBothHerdsWhenEnoughFeedExists()
        {
            var plan = DogCompanionRules.PlanMorningCare(5, 2, 3);

            Assert.That(plan.ChickensFed, Is.True);
            Assert.That(plan.CowsFed, Is.True);
            Assert.That(plan.FeedSpent, Is.EqualTo(5));
        }

        [Test]
        public void MorningCare_PrioritizesChickensWithoutCreatingFreeFeed()
        {
            var plan = DogCompanionRules.PlanMorningCare(2, 1, 2);

            Assert.That(plan.ChickensFed, Is.True);
            Assert.That(plan.CowsFed, Is.False);
            Assert.That(plan.FeedSpent, Is.EqualTo(1));
        }

        [Test]
        public void MorningCare_DoesNothingWhenFeedIsInsufficient()
        {
            var plan = DogCompanionRules.PlanMorningCare(-2, 1, 1);

            Assert.That(plan.Helped, Is.False);
            Assert.That(plan.FeedSpent, Is.Zero);
        }
    }
}
