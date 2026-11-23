using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private CookingRhythm cookingRhythm;

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
                cookingRhythm = null;
                session.CloseFarmFacility();
                return;
            }

            if (!kitchen) cookingRhythm = null;
            else UpdateCookingRhythm(session, panel);
            if (kitchen && cookingRhythm != null) DrawCookingRhythm(session, panel);
            else if (kitchen) DrawKitchenRecipes(session, panel);
            else
            {
                DrawConstructionCard(session, FarmBuilding.House, new Rect(panel.x + 18f, panel.y + 81f, 394f, 292f));
                DrawConstructionCard(session, FarmBuilding.Greenhouse, new Rect(panel.x + 428f, panel.y + 81f, 394f, 292f));
            }

            var footer = new Rect(panel.x + 18f, panel.yMax - 51f, panel.width - 36f, 34f);
            ShopFill(footer, BagWell, 7f);
            GUI.Label(new Rect(footer.x + 11f, footer.y, footer.width - 22f, footer.height),
                session.FacilityNotice.Length > 0 ? session.FacilityNotice : kitchen
                    ? "초록 칸에서 스페이스를 세 번 누르면 완성돼요. 놓쳐도 재료는 그대로예요."
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
            var recipes = BuildingUpgradeCatalog.Recipes;
            var inventory = session.Data.inventory;
            for (var index = 0; index < recipes.Count; index++)
            {
                var recipe = recipes[index];
                var card = new Rect(panel.x + 18f + index % 2 * 410f, panel.y + 81f + index / 2 * 147f, 394f, 133f);
                ShopFill(card, ShopCard, 10f);
                var icon = new Rect(card.x + 14f, card.y + 14f, 66f, 66f);
                ShopFill(icon, BagWell, 9f);
                DrawShopArt(new Rect(icon.x + 9f, icon.y + 9f, 48f, 48f), GetInventoryArt(recipe.Result));
                GUI.Label(new Rect(card.x + 94f, card.y + 14f, 280f, 32f), ItemCatalog.GetLabel(recipe.Result), shopName);
                var ingredients = $"{ItemCatalog.GetLabel(recipe.First)} {inventory.Count(recipe.First)}/1";
                if (recipe.Second is GameItem second)
                    ingredients += $"   ·   {ItemCatalog.GetLabel(second)} {inventory.Count(second)}/1";
                GUI.Label(new Rect(card.x + 94f, card.y + 47f, 280f, 25f), ingredients, shopSmall);
                GUI.Label(new Rect(card.x + 14f, card.y + 91f, 200f, 34f), $"판매가 {recipe.SellPrice:N0} G", shopSmall);
                var button = new Rect(card.xMax - 148f, card.y + 84f, 134f, 38f);
                if (recipe.HasIngredients(inventory))
                {
                    if (ShopButton(button, "박자 맞추기", ShopGreen, shopAction))
                    {
                        cookingRhythm = new CookingRhythm(recipe.Result);
                        session.SetFacilityNotice("표시가 초록 칸에 있을 때 스페이스를 누르세요.");
                    }
                }
                else
                {
                    ShopFill(button, ShopLine, 8f);
                    GUI.Label(button, "재료 부족", shopSecondary);
                }
            }
        }

        private void UpdateCookingRhythm(GameSession session, Rect panel)
        {
            if (cookingRhythm == null) return;

            var current = Event.current;
            var track = CookingTrackRect(panel);
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Space)
            {
                cookingRhythm.TryStrike();
                current.Use();
            }
            else if (current.type == EventType.MouseDown && track.Contains(current.mousePosition))
            {
                cookingRhythm.TryStrike();
                current.Use();
            }

            if (cookingRhythm.Resolved)
            {
                FinishCookingRhythm(session);
                return;
            }

            if (current.type == EventType.Repaint)
                cookingRhythm.Advance(Time.unscaledDeltaTime);
            if (cookingRhythm != null && cookingRhythm.Resolved)
                FinishCookingRhythm(session);
        }

        private void FinishCookingRhythm(GameSession session)
        {
            var rhythm = cookingRhythm;
            cookingRhythm = null;
            if (rhythm.Succeeded)
            {
                session.TryCook(rhythm.Dish);
                return;
            }

            session.SetFacilityNotice("박자가 어긋났어요. 재료는 그대로예요.");
        }

        private void DrawCookingRhythm(GameSession session, Rect panel)
        {
            var rhythm = cookingRhythm;
            var dish = ItemCatalog.GetLabel(rhythm.Dish);
            GUI.Label(new Rect(panel.x + 36f, panel.y + 96f, panel.width - 72f, 42f), dish, shopHeading);
            GUI.Label(new Rect(panel.x + 36f, panel.y + 146f, panel.width - 72f, 30f),
                $"{rhythm.Hits} / {CookingRhythm.RequiredHits}  ·  초록 칸에서 스페이스", shopSmall);

            var track = CookingTrackRect(panel);
            ShopFill(track, ShopPaper, 8f);
            var window = new Rect(
                track.x + track.width * CookingRhythm.WindowStart,
                track.y + 7f,
                track.width * (CookingRhythm.WindowEnd - CookingRhythm.WindowStart),
                track.height - 14f);
            ShopFill(window, new Color32(120, 168, 96, 255), 6f);
            var marker = new Rect(track.x + track.width * rhythm.Marker - 5f, track.y + 5f, 10f, track.height - 10f);
            ShopFill(marker, ShopGreen, 3f);

            var stop = new Rect(panel.x + panel.width * 0.5f - 90f, track.yMax + 28f, 180f, 42f);
            if (ShopButton(stop, "그만두기", ShopPaper, shopSecondary))
            {
                cookingRhythm = null;
                session.SetFacilityNotice("요리를 멈췄어요. 재료는 그대로예요.");
            }
        }

        private static Rect CookingTrackRect(Rect panel) =>
            new(panel.x + 48f, panel.y + 214f, panel.width - 96f, 52f);
    }
}
