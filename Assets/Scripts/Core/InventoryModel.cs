using System;
using System.Collections.Generic;

namespace FarmGame2D.Core
{
    [Serializable]
    public sealed class InventoryEntry
    {
        public GameItem item;
        public int quantity;
    }

    [Serializable]
    public sealed class InventoryModel
    {
        public List<InventoryEntry> entries = new();

        public int Count(GameItem item)
        {
            var entry = Find(item);
            return entry?.quantity ?? 0;
        }

        public void Add(GameItem item, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            var entry = Find(item);
            if (entry == null)
            {
                entries.Add(new InventoryEntry { item = item, quantity = quantity });
                return;
            }

            entry.quantity += quantity;
        }

        public bool TrySpend(GameItem item, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var entry = Find(item);
            if (entry == null || entry.quantity < quantity)
            {
                return false;
            }

            entry.quantity -= quantity;
            return true;
        }

        private InventoryEntry Find(GameItem item)
        {
            return entries.Find(entry => entry.item == item);
        }
    }
}
