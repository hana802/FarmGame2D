using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private void DrawFarmFacility(GameSession session)
        {
            EnsureShopStyles();
            var kitchen = session.OpenFacility == FarmFacility.Kitchen;
            var panel = new Rect(220f, 143f, 840f, 438f);
            ShopFill(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight), BagScrim, 0f);
            DrawInventoryPanel(panel);
            var header = new Rect(panel.x + 2f, panel.y + 2f, panel.width - 4f, 62f);
            ShopFill(header, ShopGreen, 11f);
            GUI.Label(new Rect(header.x + 20f, header.y, 340f, header.height), kitchen ? "농가 주방" : "농가·온실 공사", shopHeading);
            if (!kitchen)
                GUI.Label(new Rect(header.xMax - 250f, header.y, 132f, header.height), $"{session.Data.money:N0} G", shopLight);
            if (ShopButton(new Rect(header.xMax - 94f, header.y + 12f, 76f, 38f), "닫기", ShopPaper, shopSecondary))
            {
                session.CloseFarmFacility();
                return;
            }

            if (kitchen) DrawKitchenRecipes(session, panel);
            else
            {
                DrawConstructionCard(session, FarmBuilding.House, new Rect(panel.x + 18f, panel.y + 81f, 394f, 292f));
                DrawConstructionCard(session, FarmBuilding.Greenhouse, new Rect(panel.x + 428f, panel.y + 81f, 394f, 292f));
            }

            var footer = new Rect(panel.x + 18f, panel.yMax - 51f, panel.width - 36f, 34f);
            ShopFill(footer, BagWell, 7f);
            GUI.Label(new Rect(footer.x + 11f, footer.y, footer.width - 22f, footer.height),
                session.FacilityNotice.Length > 0 ? session.FacilityNotice : kitchen
                    ? "만든 요리는 가방에 들어가며 판매함에서 팔 수 있어요."
                    : "비용을 내면 공사가 완료되고 새 공간이 열려요.", shopSmall);
        }

        private void DrawConstructionCard(GameSession session, FarmBuilding building, Rect card)
        {
            ShopFill(card, ShopCard, 10f);
            var x = card.x + 16f;
            var width = card.width - 32f;
            var house = building == FarmBuilding.House;
            var level = session.GetBuildingLevel(building);
            GUI.Label(new Rect(x, card.y + 10f, width, 32f), house ? "농가" : "온실", shopName);
            var progress = !house && !session.Data.greenhouseBuilt ? "건설 전" : $"확장 {level} / {BuildingUpgradeCatalog.MaximumLevel}단계";
            GUI.Label(new Rect(x, card.y + 44f, width, 28f), progress, shopSmall);

            var blocker = session.GetUpgradeBlocker(building);
            if (level >= BuildingUpgradeCatalog.MaximumLevel || (!house && !session.Data.greenhouseBuilt))
            {
                GUI.Label(new Rect(x, card.y + 112f, width, 34f), blocker, shopSmall);
                return;
            }

            var upgrade = BuildingUpgradeCatalog.Get(building, level);
            GUI.Label(new Rect(x, card.y + 82f, width, 29f), upgrade.Benefit, shopSmall);
            ShopFill(new Rect(x, card.y + 120f, width, 92f), ShopPaper, 7f);
            var inventory = session.Data.inventory;
            DrawFacilityCost(x + 10f, card.y + 126f, $"공사비  {upgrade.Price:N0} G");
            DrawFacilityCost(x + 186f, card.y + 126f, $"목재  {inventory.Count(GameItem.Wood)} / {upgrade.Wood}");
            DrawFacilityCost(x + 10f, card.y + 164f, $"돌  {inventory.Count(GameItem.Stone)} / {upgrade.Stone}");
            DrawFacilityCost(x + 186f, card.y + 164f, $"{ItemCatalog.GetLabel(upgrade.Metal)}  {inventory.Count(upgrade.Metal)} / {upgrade.MetalCount}");
            var button = new Rect(x, card.y + 231f, width, 44f);
            if (blocker.Length == 0)
            {
                if (ShopButton(button, $"{level + 1}단계 공사하기", ShopGreen, shopAction))
                    session.TryUpgradeBuilding(building);
            }
            else
            {
                ShopFill(button, ShopLine, 8f);
                GUI.Label(button, blocker, shopSecondary);
            }
        }

        private void DrawFacilityCost(float x, float y, string label) =>
            GUI.Label(new Rect(x, y, 168f, 37f), label, shopSmall);

        private void DrawKitchenRecipes(GameSession session, Rect panel)
        {
            var y = panel.y + 87f;
            foreach (var recipe in BuildingUpgradeCatalog.Recipes)
            {
                var card = new Rect(panel.x + 18f, y, panel.width - 36f, 123f);
                ShopFill(card, ShopCard, 10f);
                var icon = new Rect(card.x + 15f, card.y + 22f, 80f, 80f);
                ShopFill(icon, BagWell, 9f);
                DrawShopArt(new Rect(icon.x + 12f, icon.y + 12f, 56f, 56f), GetInventoryArt(recipe.Result));
                GUI.Label(new Rect(card.x + 111f, card.y + 14f, 380f, 32f), ItemCatalog.GetLabel(recipe.Result), shopName);
                var inventory = session.Data.inventory;
                var first = inventory.Count(recipe.First);
                var second = inventory.Count(recipe.Second);
                var ingredients = $"{ItemCatalog.GetLabel(recipe.First)} {first}/1   ·   {ItemCatalog.GetLabel(recipe.Second)} {second}/1";
                GUI.Label(new Rect(card.x + 111f, card.y + 46f, 400f, 25f), ingredients, shopSmall);
                GUI.Label(new Rect(card.x + 111f, card.y + 74f, 330f, 25f), $"판매가 {recipe.SellPrice:N0} G", shopSmall);
                var button = new Rect(card.xMax - 160f, card.y + 37f, 144f, 48f);
                if (first > 0 && second > 0)
                {
                    if (ShopButton(button, "1개 만들기", ShopGreen, shopAction)) session.TryCook(recipe.Result);
                }
                else
                {
                    ShopFill(button, ShopLine, 8f);
                    GUI.Label(button, "재료 부족", shopSecondary);
                }
                y += 136f;
            }
        }
    }
}
