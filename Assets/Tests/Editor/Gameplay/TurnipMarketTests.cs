using System.Linq;
using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class TurnipMarketTests
    {
        [Test]
        public void TurnipMarket_BuyPriceStaysForTheWeekAndIgnoresSeason()
        {
            const int seed = 736687;
            Assert.That(TurnipMarket.BuyPrice(seed, 1), Is.EqualTo(TurnipMarket.BuyPrice(seed, 7)));
            Assert.That(TurnipMarket.BuyPrice(seed, 1), Is.InRange(TurnipMarket.MinBuyPrice, TurnipMarket.MaxBuyPrice));
            Assert.That(TurnipMarket.IsPurchaseDay(1), Is.True);
            Assert.That(TurnipMarket.IsPurchaseDay(29), Is.True);
            Assert.That(TurnipMarket.IsPurchaseDay(2), Is.False);
        }

        [Test]
        public void TurnipMarket_DeclineWeekFallsAndSpikeWeekJumpsOnce()
        {
            var decline = FindSeed(false);
            var spike = FindSeed(true);
            var declineBuy = TurnipMarket.BuyPrice(decline, 1);
            var previous = int.MaxValue;
            for (var day = 1; day <= 7; day++)
            {
                var sell = TurnipMarket.SellPrice(decline, day);
                Assert.That(sell, Is.LessThan(declineBuy));
                Assert.That(sell, Is.LessThanOrEqualTo(previous));
                previous = sell;
            }

            var spikeBuy = TurnipMarket.BuyPrice(spike, 1);
            var spikes = 0;
            for (var day = 1; day <= 7; day++)
            {
                var sell = TurnipMarket.SellPrice(spike, day);
                if (sell == spikeBuy * 3) spikes++;
                else Assert.That(sell, Is.LessThan(spikeBuy));
            }

            Assert.That(spikes, Is.EqualTo(1));
        }

        [Test]
        public void TurnipMarket_BuysSeedsOnWeekStartAndOnlyHarvestedTurnipsRot()
        {
            var data = GameSaveData.CreateNew(1, 11);
            data.money = 500;
            data.day = 2;
            Assert.That(TurnipMarket.Buy(data, 4), Is.Zero);

            data.day = 1;
            var bought = TurnipMarket.Buy(data, 10);
            Assert.That(bought, Is.GreaterThan(0));
            Assert.That(data.inventory.Count(GameItem.TurnipSeed), Is.EqualTo(bought));
            Assert.That(data.inventory.Count(GameItem.Turnip), Is.Zero);
            Assert.That(data.money, Is.EqualTo(500 - bought * TurnipMarket.BuyPrice(11, 1)));
            Assert.That(TurnipMarket.Sell(data, 1), Is.Zero);

            data.inventory.Add(GameItem.Turnip, 3);
            data.inventory.Add(GameItem.Egg, 2);
            Assert.That(ProduceSale.List(data.inventory).Any(offer =>
                offer.Item is GameItem.Turnip or GameItem.TurnipSeed), Is.False);
            Assert.That(TurnipMarket.Sell(data, 1), Is.EqualTo(TurnipMarket.SellPrice(11, 1)));
            Assert.That(TurnipMarket.Discard(data.inventory), Is.EqualTo(2));
            Assert.That(data.inventory.Count(GameItem.Turnip), Is.Zero);
            Assert.That(data.inventory.Count(GameItem.TurnipSeed), Is.EqualTo(bought));
            Assert.That(data.inventory.Count(GameItem.Egg), Is.EqualTo(2));
        }

        private static int FindSeed(bool spike)
        {
            for (var seed = 1; seed < 400; seed++)
            {
                if (TurnipMarket.IsSpikeWeek(seed, 1) == spike) return seed;
            }

            Assert.Fail("A turnip week of the requested pattern was not found.");
            return 0;
        }
    }
}
