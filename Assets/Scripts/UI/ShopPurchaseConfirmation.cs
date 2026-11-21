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

    public static class ShopOrderRules
    {
        public const int MaxCount = 99;

        public static bool AllowsMultiple(ShopPurchaseKind kind) => kind != ShopPurchaseKind.Pickaxe;

        public static int Maximum(ShopPurchaseRequest request, int money, int ownedChickens, int ownedCows)
        {
            if (request.Kind == ShopPurchaseKind.Pickaxe) return 1;
            if (request.Kind == ShopPurchaseKind.ChickenSale) return Math.Max(0, ownedChickens - 1);
            if (request.Kind == ShopPurchaseKind.CowSale) return Math.Max(0, ownedCows);
            if (request.Price <= 0) return 1;
            var affordable = Math.Min(MaxCount, Math.Max(0, money / request.Price));
            if (request.Kind == ShopPurchaseKind.Chicken)
                return Math.Min(affordable, Math.Max(0, AnimalBreedingRules.PopulationCap - ownedChickens));
            if (request.Kind == ShopPurchaseKind.Cow)
                return Math.Min(affordable, Math.Max(0, AnimalBreedingRules.PopulationCap - ownedCows));
            return affordable;
        }

        public static string Summary(ShopPurchaseRequest request, int count)
        {
            count = Math.Max(1, count);
            var total = request.Price * count;
            var amount = request.IsSale ? "받는 금액" : "결제 금액";
            var money = $"{(request.IsSale ? "+" : "")}{total:N0} G";
            return count == 1 ? $"{amount}   {money}" : $"×{count}   {amount}   {money}";
        }
    }
}
