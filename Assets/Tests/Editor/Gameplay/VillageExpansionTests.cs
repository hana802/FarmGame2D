using System.Linq;
using FarmGame2D.Art;
using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class VillageExpansionTests
    {
        [TestCase(1, "월", false)]
        [TestCase(5, "금", false)]
        [TestCase(6, "토", true)]
        [TestCase(7, "일", true)]
        [TestCase(28, "일", true)]
        [TestCase(29, "월", false)]
        [TestCase(34, "토", true)]
        public void CalendarFollowsTheSaveDayAcrossSeasons(int day, string label, bool weekend)
        {
            Assert.That(SeasonCalendar.GetWeekdayLabel(day), Is.EqualTo(label));
            Assert.That(SeasonCalendar.IsWeekend(day), Is.EqualTo(weekend));
        }

        [Test]
        public void WeekendOffersFollowSeasonAndWeek()
        {
            Assert.That(WeekendMarket.GetOffers(ShopKind.MarketProduce, 6).Select(offer => offer.Item),
                Is.EquivalentTo(new[] { GameItem.Potato, GameItem.Strawberry }));
            Assert.That(WeekendMarket.GetOffers(ShopKind.MarketProduce, 34).Select(offer => offer.Item),
                Is.EquivalentTo(new[] { GameItem.Tomato, GameItem.Corn }));
            Assert.That(WeekendMarket.GetOffers(ShopKind.MarketSpecialties, 6).Single().Item,
                Is.EqualTo(WeekendMarket.GetOffers(ShopKind.MarketSpecialties, 7).Single().Item));
            Assert.That(WeekendMarket.GetOffers(ShopKind.MarketSpecialties, 13).Single().Item,
                Is.Not.EqualTo(WeekendMarket.GetOffers(ShopKind.MarketSpecialties, 7).Single().Item));
        }

        [Test]
        public void HarborDeliverySpendsTwoFishOncePerWeek()
        {
            var save = new GameSaveData { day = 7 };
            save.inventory.Add(GameItem.HarborMackerel, 1);
            Assert.That(HarborDelivery.TryComplete(save), Is.False);
            Assert.That(save.money, Is.EqualTo(200));
            save.inventory.Add(GameItem.HarborMackerel, 3);

            Assert.That(HarborDelivery.TryComplete(save), Is.True);
            Assert.That(HarborDelivery.TryComplete(save), Is.False);
            Assert.That(save.inventory.Count(GameItem.HarborMackerel), Is.EqualTo(2));
            Assert.That(save.money, Is.EqualTo(380));

            save.day = 8;
            Assert.That(HarborDelivery.TryComplete(save), Is.True);
            Assert.That(save.inventory.Count(GameItem.HarborMackerel), Is.Zero);
            Assert.That(save.money, Is.EqualTo(560));
        }

        [TestCase(WorldArtKind.ProduceStall)]
        [TestCase(WorldArtKind.BakeryStall)]
        [TestCase(WorldArtKind.SpecialtiesStall)]
        [TestCase(WorldArtKind.FisherHut)]
        [TestCase(WorldArtKind.HarborPier)]
        [TestCase(WorldArtKind.HarborBoat)]
        [TestCase(WorldArtKind.WeekendMerchant)]
        public void VillageSpritesLoadFromTheirImportedAtlases(WorldArtKind art)
        {
            var sprite = WorldArtCatalog.GetSprite(art);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.rect.xMax, Is.LessThanOrEqualTo(sprite.texture.width));
            Assert.That(sprite.rect.yMax, Is.LessThanOrEqualTo(sprite.texture.height));
        }
    }
}
