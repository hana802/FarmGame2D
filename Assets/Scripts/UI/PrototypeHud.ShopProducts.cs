using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private static readonly string[] FarmShopTabs = { "씨앗", "가축 · 사료", "가축 판매", "농장 설비" };
        private static readonly string[] GeneralShopTabs = { "도구 · 광물", "마을 장식" };

        private static readonly ShopPurchaseRequest[] LivestockProducts =
        {
            new(ShopPurchaseKind.Feed, "동물 사료 3개", GameItem.Feed, 3, 15),
            new(ShopPurchaseKind.Chicken, "닭 1마리", price: 150),
            new(ShopPurchaseKind.Cow, "소 1마리", price: 350)
        };

        private static readonly ShopPurchaseRequest[] FarmEquipment =
        {
            new(ShopPurchaseKind.Sprinkler, "스프링클러", GameItem.Sprinkler, 1, GameSession.SprinklerPrice)
        };

        private static readonly ShopPurchaseRequest[] LivestockSales =
        {
            new(ShopPurchaseKind.ChickenSale, "닭 1마리", price: 100),
            new(ShopPurchaseKind.CowSale, "소 1마리", price: 250)
        };

        private static readonly ShopPurchaseRequest[] MiningProducts =
        {
            new(ShopPurchaseKind.Pickaxe, "곡괭이", price: 120),
            new(ShopPurchaseKind.Ore, "돌 5개", GameItem.Stone, 5, 30),
            new(ShopPurchaseKind.Ore, "구리 광석 3개", GameItem.CopperOre, 3, 75),
            new(ShopPurchaseKind.Ore, "철 광석 2개", GameItem.IronOre, 2, 110)
        };

        private static IEnumerable<ShopPurchaseRequest> GetShopProducts(GameSession session, bool mining, int tab)
        {
            if (mining)
            {
                if (tab == 0) return MiningProducts;
                return GetDecorationProducts();
            }

            return tab switch
            {
                1 => LivestockProducts,
                2 => LivestockSales,
                3 => FarmEquipment,
                _ => GetSeedProducts(session.CurrentSeason)
            };
        }

        private static IEnumerable<ShopPurchaseRequest> GetSeedProducts(Season season)
        {
            foreach (var crop in CropCatalog.GetForSeason(season))
                yield return new ShopPurchaseRequest(ShopPurchaseKind.Seed, $"{crop.Label} 씨앗 5개", crop.Seed, 5, crop.PackPrice);
        }

        private static IEnumerable<ShopPurchaseRequest> GetDecorationProducts()
        {
            foreach (var decoration in TownDecorationCatalog.GetAll())
                yield return new ShopPurchaseRequest(ShopPurchaseKind.Decoration, ItemCatalog.GetLabel(decoration.Item),
                    decoration.Item, 1, decoration.Price);
        }

        private static string ShopPrice(ShopPurchaseRequest request) => $"{(request.IsSale ? "+" : "")}{request.Price:N0} G";

        private static string ShopProductNote(GameSession session, ShopPurchaseRequest request)
        {
            return request.Kind switch
            {
                ShopPurchaseKind.Seed when CropCatalog.TryGetBySeed(request.Item, out var crop) =>
                    $"{GetSeasonLabel(crop.Season)} 작물  ·  물 준 날 {crop.GrowthDays}일 뒤 수확",
                ShopPurchaseKind.Feed => "닭과 소의 하루 먹이",
                ShopPurchaseKind.Chicken => "달걀을 낳는 어른 닭",
                ShopPurchaseKind.Cow => "우유를 내는 어른 소",
                ShopPurchaseKind.ChickenSale => $"보유 {session.Data.ownedChickens}마리  ·  마지막 한 마리는 판매할 수 없어요",
                ShopPurchaseKind.CowSale => $"보유 {session.Data.ownedCows}마리",
                ShopPurchaseKind.Pickaxe => session.Data.hasPickaxe ? "구입 완료" : "광맥을 캐는 도구",
                ShopPurchaseKind.Ore => "광물  ·  가방에 바로 보관",
                ShopPurchaseKind.Sprinkler => "매일 아침 인접한 밭 네 칸에 물주기",
                _ => "빈 잔디에 놓는 장식품"
            };
        }

        private static string ShopUnavailableReason(GameSession session, ShopPurchaseRequest request)
        {
            if (request.Kind == ShopPurchaseKind.ChickenSale && session.Data.ownedChickens <= 1)
                return "마지막 닭은 판매할 수 없어요.";
            if (request.Kind == ShopPurchaseKind.CowSale && session.Data.ownedCows == 0)
                return "판매할 소가 없어요.";
            if (request.Kind == ShopPurchaseKind.Pickaxe && session.Data.hasPickaxe)
                return "이미 곡괭이를 가지고 있어요.";
            if (!request.IsSale && session.Data.money < request.Price)
                return $"{ShopPrice(request)} 필요  ·  잔액이 부족해요.";
            return null;
        }

        private static Sprite GetShopArt(ShopPurchaseRequest request)
        {
            return request.Kind switch
            {
                ShopPurchaseKind.Seed => FarmPlotArtCatalog.GetCropSprite(request.Item, true),
                ShopPurchaseKind.Chicken or ShopPurchaseKind.ChickenSale => WorldArtCatalog.GetSprite(WorldArtKind.Chicken),
                ShopPurchaseKind.Cow or ShopPurchaseKind.CowSale => WorldArtCatalog.GetSprite(WorldArtKind.Cow),
                ShopPurchaseKind.Pickaxe => GameArtCatalog.GetHeldToolSprite(FarmTool.Pickaxe),
                ShopPurchaseKind.Ore => WorldArtCatalog.GetSprite(WorldArtKind.MiningRock),
                ShopPurchaseKind.Decoration => TownDecorationArt.GetSprite(request.Item),
                ShopPurchaseKind.Sprinkler => FarmEquipmentArt.Sprinkler,
                _ => WorldArtCatalog.GetSprite(WorldArtKind.SellingBox)
            };
        }
    }
}
