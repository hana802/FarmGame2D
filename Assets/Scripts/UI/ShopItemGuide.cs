using FarmGame2D.Core;

namespace FarmGame2D.UI
{
    public static class ShopItemGuide
    {
        public static string Describe(ShopPurchaseRequest request)
        {
            switch (request.Kind)
            {
                case ShopPurchaseKind.MarketItem:
                    return "주말 장터에서 산 물건이에요. 구매하면 가방에 바로 들어가요.";
                case ShopPurchaseKind.Seed:
                    return CropCatalog.TryGetBySeed(request.Item, out var crop)
                        ? DescribeSeed(crop)
                        : string.Empty;
                case ShopPurchaseKind.Feed:
                    return "동물 사료\n닭과 소에게 주는 먹이입니다. 한 번에 3개를 삽니다.\n동물 한 마리당 하루에 사료 1개가 필요합니다.\n먹이를 주면 다음 날 어른 닭은 달걀을, 어른 소는 우유를 냅니다.";
                case ShopPurchaseKind.Chicken:
                    return "닭\n농장에서 키우는 어른 닭입니다.\n매일 사료를 주면 다음 날 달걀을 낳습니다.\n달걀은 개당 25G에 팔 수 있습니다.";
                case ShopPurchaseKind.ChickenSale:
                    return "닭 판매\n키우던 닭 1마리를 100G에 팝니다.\n마지막 한 마리는 팔 수 없습니다.";
                case ShopPurchaseKind.Cow:
                    return "소\n농장에서 키우는 어른 소입니다.\n매일 사료를 주면 다음 날 우유를 냅니다.\n우유는 개당 55G에 팔 수 있습니다.";
                case ShopPurchaseKind.CowSale:
                    return "소 판매\n키우던 소 1마리를 250G에 팝니다.";
                case ShopPurchaseKind.Pickaxe:
                    return "곡괭이\n광산의 광맥을 캐는 도구입니다.\n한 번 사면 가방에서 꺼내 사용할 수 있습니다.";
                case ShopPurchaseKind.Ore:
                    return DescribeOre(request.Item, request.Quantity);
                case ShopPurchaseKind.Sprinkler:
                    return "스프링클러\n가방에서 선택한 뒤 빈 농장 잔디에 Space로 놓습니다.\n매일 오전 6시에 바로 아래 두 칸 너비로, 세 줄의 작물에 물을 줍니다.\n설치 범위는 바닥에 표시되며, E로 다시 주울 수 있어요.";
                case ShopPurchaseKind.Decoration:
                    return DescribeDecoration(request.Item);
                default:
                    return string.Empty;
            }
        }

        public static string DescribeSeed(CropDefinition crop)
        {
            return string.Join("\n",
                $"{crop.Label} 씨앗",
                $"{SeasonLabel(crop.Season)}에 심는 작물입니다. 한 번에 씨앗 5개를 삽니다.",
                "호미로 갈아 둔 밭에 심습니다.",
                $"물을 준 날이 {crop.GrowthDays}일 쌓이면 열매가 열립니다.",
                "물을 주지 않은 날은 자라지 않아서, 수확할 때까지 매일 물을 줘야 합니다.",
                $"수확한 {crop.Label}은 개당 {crop.SellPrice}G에 팔 수 있습니다.");
        }

        public static string DescribeOre(GameItem ore, int quantity)
        {
            var label = ItemCatalog.GetLabel(ore);
            return $"{label}\n사서 바로 가방에 들어오는 광물입니다. 한 번에 {quantity}개입니다.\n광산에서도 캘 수 있고, 판매 상자에서 개당 {ItemCatalog.GetSellPrice(ore)}G에 팔 수 있습니다.";
        }

        public static string DescribeDecoration(GameItem item)
        {
            return $"{ItemCatalog.GetLabel(item)}\n마을을 꾸미는 장식품입니다.\n가방의 마을 장식 탭에서 고른 뒤, 마을 잔디에 Space로 놓습니다.\nE를 누르면 다시 주울 수 있습니다.";
        }

        private static string SeasonLabel(Season season)
        {
            switch (season)
            {
                case Season.Spring: return "봄";
                case Season.Summer: return "여름";
                case Season.Autumn: return "가을";
                default: return "겨울";
            }
        }
    }
}
