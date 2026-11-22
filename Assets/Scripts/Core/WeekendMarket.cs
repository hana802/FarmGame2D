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
        private static readonly MarketOffer Bread = new(GameItem.MarketBread, 36);
        private static readonly MarketOffer[] Specialties =
        {
            new(GameItem.PotatoSoup, 75),
            new(GameItem.TomatoOmelet, 95),
            new(GameItem.FlowerPot, 68)
        };

        public static bool IsMarketShop(ShopKind shop) =>
            shop is ShopKind.MarketProduce or ShopKind.MarketBakery or ShopKind.MarketSpecialties;

        public static IEnumerable<MarketOffer> GetOffers(ShopKind shop, int day)
        {
            switch (shop)
            {
                case ShopKind.MarketProduce:
                    foreach (var crop in CropCatalog.GetForSeason(SeasonCalendar.GetSeason(day)))
                        yield return new MarketOffer(crop.Produce, crop.SellPrice + 7);
                    break;
                case ShopKind.MarketBakery:
                    yield return Bread;
                    break;
                case ShopKind.MarketSpecialties:
                    yield return Specialties[SeasonCalendar.WeekIndex(day) % Specialties.Length];
                    break;
            }
        }
    }
}
