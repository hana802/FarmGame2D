using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public readonly struct MarketOffer
    {
        public MarketOffer(GameItem item, int price)
        {
            Item = item;
            Price = price;
        }

        public GameItem Item { get; }
        public int Price { get; }
    }

    public static class WeekendMarket
    {
        public static bool IsMarketShop(ShopKind shop) =>
            shop is ShopKind.MarketProduce or ShopKind.MarketBakery or ShopKind.MarketSpecialties;

        public static IEnumerable<MarketOffer> GetOffers(ShopKind shop, int day)
        {
            switch (shop)
            {
                case ShopKind.MarketProduce:
                    yield return SeasonalFood(SeasonCalendar.GetSeason(day));
                    break;
                case ShopKind.MarketBakery:
                    yield return new MarketOffer(GameItem.MarketBread, 36);
                    break;
                case ShopKind.MarketSpecialties:
                    yield return new MarketOffer(GameItem.PotatoSoup, 124);
                    yield return new MarketOffer(GameItem.TomatoOmelet, 104);
                    break;
            }
        }

        private static MarketOffer SeasonalFood(Season season)
        {
            return season switch
            {
                Season.Spring => new MarketOffer(GameItem.StrawberryJam, 80),
                Season.Summer => new MarketOffer(GameItem.RoastedCorn, 72),
                Season.Autumn => new MarketOffer(GameItem.PumpkinPie, 110),
                _ => new MarketOffer(GameItem.BakedSweetPotato, 92)
            };
        }
    }
}
