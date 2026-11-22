using System;

namespace FarmGame2D.Core
{
    public static class TurnipMarket
    {
        public const int WeekLength = 7;
        public const int MinBuyPrice = 24;
        public const int MaxBuyPrice = 60;

        public static int WeekIndex(int day) => Math.Max(0, day - 1) / WeekLength;

        public static int DayIndex(int day) => Math.Max(0, day - 1) % WeekLength;

        public static bool IsPurchaseDay(int day) => day >= 1 && DayIndex(day) == 0;

        public static int BuyPrice(int worldSeed, int day)
        {
            var span = MaxBuyPrice - MinBuyPrice + 1;
            return MinBuyPrice + PositiveHash(worldSeed, WeekIndex(day), 0) % span;
        }

        public static bool IsSpikeWeek(int worldSeed, int day) =>
            PositiveHash(worldSeed, WeekIndex(day), 1) % 4 == 0;

        public static int SellPrice(int worldSeed, int day)
        {
            var buy = BuyPrice(worldSeed, day);
            var index = DayIndex(day);
            if (IsSpikeWeek(worldSeed, day))
            {
                var spikeDay = 2 + PositiveHash(worldSeed, WeekIndex(day), 2) % 4;
                return index == spikeDay ? buy * 3 : Math.Max(8, buy * 40 / 100);
            }

            var price = Math.Max(8, buy * 90 / 100);
            for (var step = 0; step < index; step++)
                price = Math.Max(8, price * 85 / 100);
            return price;
        }

        public static int DaysUntilRot(int day) => WeekLength - DayIndex(day);

        public static string PurchaseNote(int day) => IsPurchaseDay(day)
            ? "오늘은 순무를 사는 날이에요. 계절과 상관없이 매주 첫날입니다."
            : "오늘은 순무를 사지 않아요. 가진 순무는 오늘 시세에 팔 수 있어요.";

        public static string FreshnessNote(int day)
        {
            var left = DaysUntilRot(day);
            return left <= 1 ? "내일 아침이면 썩어요." : $"다음 주 아침까지 {left}일 남았어요.";
        }

        public static int Buy(GameSaveData data, int quantity)
        {
            if (data == null || quantity <= 0 || !IsPurchaseDay(data.day)) return 0;
            var price = BuyPrice(data.worldSeed, data.day);
            quantity = Math.Min(quantity, data.money / price);
            if (quantity <= 0) return 0;
            data.money -= quantity * price;
            data.inventory.Add(GameItem.Turnip, quantity);
            return quantity;
        }

        public static int Sell(GameSaveData data, int quantity)
        {
            if (data == null || quantity <= 0) return 0;
            quantity = Math.Min(quantity, data.inventory.Count(GameItem.Turnip));
            if (quantity <= 0 || !data.inventory.TrySpend(GameItem.Turnip, quantity)) return 0;
            var income = quantity * SellPrice(data.worldSeed, data.day);
            data.money += income;
            return income;
        }

        public static int Discard(InventoryModel inventory)
        {
            var owned = inventory?.Count(GameItem.Turnip) ?? 0;
            if (owned <= 0) return 0;
            inventory.TrySpend(GameItem.Turnip, owned);
            return owned;
        }

        private static int PositiveHash(int seed, int week, int salt)
        {
            unchecked
            {
                var value = (seed * 73856093) ^ (week * 19349663) ^ (salt * 83492791);
                return value & 0x7fffffff;
            }
        }
    }
}
