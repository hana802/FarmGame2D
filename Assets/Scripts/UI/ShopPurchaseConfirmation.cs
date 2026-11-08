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
        Pickaxe,
        Ore
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
        public bool IsSale => Kind == ShopPurchaseKind.ChickenSale;
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
}
