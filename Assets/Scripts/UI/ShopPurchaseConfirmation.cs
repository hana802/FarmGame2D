using System;
using FarmGame2D.Core;

namespace FarmGame2D.UI
{
    public enum ShopPurchaseKind
    {
        Seed,
        Feed,
        Chicken,
        ChickenSale,
        Cow,
        CowSale,
        Pickaxe,
        Ore,
        Decoration,
        Sprinkler
    }

    public readonly struct ShopPurchaseRequest
    {
        public ShopPurchaseRequest(
            ShopPurchaseKind kind,
            string displayName,
            GameItem item = default,
            int quantity = 0,
            int price = 0)
        {
            Kind = kind;
            DisplayName = displayName;
            Item = item;
            Quantity = quantity;
            Price = price;
        }

        public ShopPurchaseKind Kind { get; }
        public string DisplayName { get; }
        public GameItem Item { get; }
        public int Quantity { get; }
        public int Price { get; }
        public bool IsSale => Kind == ShopPurchaseKind.ChickenSale || Kind == ShopPurchaseKind.CowSale;
    }

    public sealed class ShopPurchaseConfirmation
    {
        public ShopPurchaseRequest? Pending { get; private set; }

        public static string GetPrompt(string displayName)
        {
            return $"\"{displayName}\"를 구매하시겠습니까?";
        }

        public static string GetPrompt(ShopPurchaseRequest request)
        {
            return request.IsSale
                ? $"\"{request.DisplayName}\"를 판매하시겠습니까?"
                : GetPrompt(request.DisplayName);
        }

        public void Request(ShopPurchaseRequest request)
        {
            Pending = request;
        }

        public void Cancel()
        {
            Pending = null;
        }

        public bool Confirm(Action<ShopPurchaseRequest> purchase)
        {
            if (!Pending.HasValue)
            {
                return false;
            }

            var request = Pending.Value;
            Pending = null;
            purchase?.Invoke(request);
            return true;
        }
    }

    public static class ShopBulkOrder
    {
        public const int MaxCount = 99;

        public static bool AllowsCount(ShopPurchaseKind kind) => kind != ShopPurchaseKind.Pickaxe;

        public static int MaxCountFor(ShopPurchaseRequest request, int money, int ownedChickens, int ownedCows)
        {
            if (!AllowsCount(request.Kind))
            {
                return 1;
            }

            if (request.Kind == ShopPurchaseKind.ChickenSale)
            {
                return System.Math.Max(0, ownedChickens - 1);
            }

            if (request.Kind == ShopPurchaseKind.CowSale)
            {
                return System.Math.Max(0, ownedCows);
            }

            if (request.Price <= 0)
            {
                return 1;
            }

            var affordable = money / request.Price;
            if (affordable < 1)
            {
                return 1;
            }

            return System.Math.Min(MaxCount, affordable);
        }

        public static int ClampCount(ShopPurchaseRequest request, int count, int money, int ownedChickens, int ownedCows)
        {
            var max = MaxCountFor(request, money, ownedChickens, ownedCows);
            if (max < 1)
            {
                return 0;
            }

            if (count < 1)
            {
                return 1;
            }

            return count > max ? max : count;
        }

        public static string DescribeTotal(ShopPurchaseRequest request, int count)
        {
            var total = request.Price * count;
            var amount = request.IsSale ? $"+{total:N0} G" : $"{total:N0} G";
            if (request.Kind is ShopPurchaseKind.Chicken or ShopPurchaseKind.Cow
                or ShopPurchaseKind.ChickenSale or ShopPurchaseKind.CowSale)
            {
                return $"{count}마리  ·  {amount}";
            }

            if (request.Quantity > 0)
            {
                return $"{request.Quantity * count}개  ·  {amount}";
            }

            return amount;
        }
    }
}
