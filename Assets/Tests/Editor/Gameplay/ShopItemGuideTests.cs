using FarmGame2D.Core;
using FarmGame2D.UI;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class ShopItemGuideTests
    {
        [Test]
        public void TomatoSeed_ExplainsGrowthDaysAndDailyWatering()
        {
            Assert.That(CropCatalog.TryGetBySeed(GameItem.TomatoSeed, out var tomato), Is.True);

            var guide = ShopItemGuide.DescribeSeed(tomato);

            Assert.That(guide, Does.Contain("토마토 씨앗"));
            Assert.That(guide, Does.Contain("여름"));
            Assert.That(guide, Does.Contain("3일"));
            Assert.That(guide, Does.Contain("매일 물"));
            Assert.That(guide, Does.Contain("28G"));
        }

        [Test]
        public void PotatoSeed_UsesItsOwnGrowthDays()
        {
            Assert.That(CropCatalog.TryGetBySeed(GameItem.PotatoSeed, out var potato), Is.True);

            var guide = ShopItemGuide.Describe(new ShopPurchaseRequest(
                ShopPurchaseKind.Seed,
                "감자 씨앗 5개",
                potato.Seed));

            Assert.That(guide, Does.Contain("2일"));
            Assert.That(guide, Does.Contain("18G"));
        }

        [Test]
        public void BoughtOre_ExplainsThatItCanBeSold()
        {
            var guide = ShopItemGuide.Describe(new ShopPurchaseRequest(
                ShopPurchaseKind.Ore,
                "구리 광석 3개",
                GameItem.CopperOre,
                3,
                75));

            Assert.That(guide, Does.Contain("구리 광석"));
            Assert.That(guide, Does.Contain("3개"));
            Assert.That(guide, Does.Contain("18G"));
        }
    }
}
