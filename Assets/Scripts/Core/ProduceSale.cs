using System;
using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public static class ProduceSale
    {
        public readonly struct Offer
        {
            public Offer(GameItem item, int quantity, int unitPrice)
            {
                Item = item;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }

            public GameItem Item { get; }
            public int Quantity { get; }
            public int UnitPrice { get; }
            public int Total => Quantity * UnitPrice;
        }

        public static Offer[] List(InventoryModel inventory)
        {
            var offers = new List<Offer>();
            foreach (GameItem item in Enum.GetValues(typeof(GameItem)))
            {
                var price = ItemCatalog.GetSellPrice(item);
                var quantity = inventory.Count(item);
                if (price <= 0 || quantity <= 0)
                {
                    continue;
                }

                offers.Add(new Offer(item, quantity, price));
            }

            return offers.ToArray();
        }

        public static int Sell(InventoryModel inventory, GameItem item, int quantity)
        {
            var price = ItemCatalog.GetSellPrice(item);
            if (price <= 0 || quantity <= 0 || !inventory.TrySpend(item, quantity))
            {
                return 0;
            }

            return quantity * price;
        }

        public static int SellAll(InventoryModel inventory)
        {
            var income = 0;
            foreach (var offer in List(inventory))
            {
                income += Sell(inventory, offer.Item, offer.Quantity);
            }

            return income;
        }
    }
}
