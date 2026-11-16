using System.Collections.Generic;

namespace FarmGame2D.Core
{
    public readonly struct TownDecorationDefinition
    {
        public TownDecorationDefinition(GameItem item, int price)
        {
            Item = item;
            Price = price;
        }

        public GameItem Item { get; }
        public int Price { get; }
    }

    public static class TownDecorationCatalog
    {
        private static readonly TownDecorationDefinition[] Items =
        {
            new(GameItem.FlowerPot, 45),
            new(GameItem.StreetLamp, 95),
            new(GameItem.TownBench, 80)
        };

        public static IReadOnlyList<TownDecorationDefinition> GetAll() => Items;

        public static bool TryGet(GameItem item, out TownDecorationDefinition definition)
        {
            foreach (var candidate in Items)
            {
                if (candidate.Item != item) continue;
                definition = candidate;
                return true;
            }

            definition = default;
            return false;
        }
    }
}
