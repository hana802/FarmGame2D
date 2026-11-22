using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private enum InventoryPage { Tools, Seeds, Produce, Materials, Decorations }

        private static readonly string[] InventoryTabs = { "도구", "씨앗", "수확물", "재료", "장식·설비" };
        private static readonly string[] InventoryHints =
        {
            "도구를 선택하면 바로 장착해요.",
            "심을 씨앗을 선택하세요. 카드에서 심는 계절을 확인할 수 있어요.",
            "수확한 작물, 동물 생산품, 물고기와 요리를 확인할 수 있어요.",
            "농장에 보관 중인 사료, 광물, 건축 재료의 수량이에요.",
            "스프링클러는 농장 밭 근처에, 마을 장식은 빈 잔디에 Space로 놓으세요."
        };
        private static readonly FarmTool[] InventoryTools =
        {
            FarmTool.Hoe, FarmTool.Shovel, FarmTool.WateringCan, FarmTool.Pickaxe, FarmTool.FishingRod, FarmTool.Axe
        };
        private static readonly GameItem[] AnimalAndFishProducts =
        {
            GameItem.Turnip, GameItem.Egg, GameItem.Milk, GameItem.RiverFish, GameItem.SeaBass, GameItem.GoldenCarp,
            GameItem.HarborMackerel, GameItem.MarketBread
        };
        private static readonly GameItem[] InventoryMaterials =
        {
            GameItem.Feed, GameItem.Stone, GameItem.CopperOre, GameItem.IronOre, GameItem.GoldOre,
            GameItem.Weed, GameItem.Wood
        };
        private static readonly Color BagAccent = new Color32(180, 215, 185, 255);
        private static readonly Color BagWell = new Color32(225, 233, 215, 255);
        private static readonly Color BagDark = new Color32(29, 55, 46, 255);
        private static readonly Color BagScrim = new Color32(18, 38, 30, 85);
        private static readonly Color BagShadow = new Color32(18, 38, 30, 110);

        private InventoryPage inventoryPage;
        private GUIStyle bagTitle, bagTab, bagItem, bagMutedItem, bagBadge, bagActiveBadge;
        private GUIStyle summaryHeading, summaryLabel, summaryCount, summaryItem;
        private static readonly Color SummaryInk = new Color32(74, 49, 36, 255);
        private static readonly Color SummaryBanner = new Color32(235, 205, 160, 255);
        private static readonly Color SummaryStripe = new Color32(253, 243, 218, 255);

        private void DrawInventory(GameSession session)
        {
            EnsureInventoryStyles();
            ShopFill(new Rect(0f, 92f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight - 92f), BagScrim, 0f);
            DrawInventorySummary(session);

            var panel = new Rect(304f, 110f, 946f, 540f);
            var header = new Rect(panel.x + 2f, panel.y + 2f, panel.width - 4f, 60f);
            DrawInventoryPanel(panel);
            ShopFill(header, ShopGreen, 12f);
            GUI.Label(new Rect(header.x + 20f, header.y, 200f, header.height), InventoryTitle, bagTitle);
            if (ShopButton(new Rect(header.xMax - 94f, header.y + 12f, 76f, 36f), "닫기", ShopPaper, bagTab))
            {
                session.ToggleInventory();
                return;
            }

            for (var index = 0; index < InventoryTabs.Length; index++)
            {
                var tab = new Rect(panel.x + 18f + index * 183f, panel.y + 73f, 178f, 40f);
                var selected = (int)inventoryPage == index;
                if (ShopButton(tab, InventoryTabs[index], selected ? ShopGreen : ShopLine, selected ? shopAction : bagTab))
                    inventoryPage = (InventoryPage)index;
            }

            var well = new Rect(panel.x + 16f, panel.y + 123f, panel.width - 32f, 360f);
            ShopFill(well, BagWell, 10f);
            DrawInventoryItems(session, well);
            GUI.Label(new Rect(panel.x + 20f, panel.y + 492f, panel.width - 40f, 36f),
                InventoryHints[(int)inventoryPage], shopSmall);
        }

        private void DrawInventorySummary(GameSession session)
        {
            EnsureInventoryStyles();
            var quest = HudLayout.QuestTracker;
            var panel = new Rect(quest.x, quest.yMax + 10f, 220f, 300f);
            farmSkin.DrawPixelPanel(panel, Paper, PaperEdge);
            var profile = new Rect(panel.x + 3f, panel.y + 3f, panel.width - 6f, 57f);
            FarmGuiSkin.DrawSolid(profile, SummaryBanner);
            var portrait = new Rect(panel.x + 10f, panel.y + 8f, 46f, 46f);
            ShopFill(portrait, SummaryStripe, 6f);
            DrawShopArt(portrait,
                GameArtCatalog.GetPlayerPortrait(session.Data.characterChoice));

            var item = session.SelectedDecoration;
            if (!item.HasValue && session.SelectedTool == FarmTool.Seeds) item = session.SelectedSeed;
            var activeArt = item.HasValue ? GetInventoryArt(item.Value) : GameArtCatalog.GetHeldToolSprite(session.SelectedTool);
            var activeLabel = item.HasValue ? ItemCatalog.GetLabel(item.Value) : GetToolLabel(session.SelectedTool);
            GUI.Label(new Rect(panel.x + 65f, panel.y + 7f, 143f, 25f), "보유 현황", summaryHeading);
            DrawShopArt(new Rect(panel.x + 65f, panel.y + 33f, 20f, 20f), activeArt);
            GUI.Label(new Rect(panel.x + 90f, panel.y + 32f, 119f, 22f), activeLabel, summaryLabel);

            var data = session.Data;
            var inventory = data.inventory;
            var fishCount = inventory.Count(GameItem.RiverFish) + inventory.Count(GameItem.SeaBass)
                + inventory.Count(GameItem.GoldenCarp) + inventory.Count(GameItem.HarborMackerel);
            var juvenileCount = data.chickenJuvenileDays.Count + data.cowJuvenileDays.Count;
            var x = panel.x + 10f;
            var right = x + 103f;
            var y = panel.y;
            DrawSummaryGroup(x, y + 66f, "가축 · 생산품");
            DrawSummaryCount(x, y + 91f, "닭", data.ownedChickens);
            DrawSummaryCount(right, y + 91f, "소", data.ownedCows);
            DrawSummaryCount(x, y + 116f, "새끼", juvenileCount);
            DrawSummaryCount(right, y + 116f, "사료", inventory.Count(GameItem.Feed));
            DrawSummaryCount(x, y + 141f, "달걀", inventory.Count(GameItem.Egg));
            DrawSummaryCount(right, y + 141f, "우유", inventory.Count(GameItem.Milk));
            DrawSummaryGroup(x, y + 167f, "채집 · 낚시");
            DrawSummaryCount(x, y + 192f, "돌", inventory.Count(GameItem.Stone));
            DrawSummaryCount(right, y + 192f, "구리", inventory.Count(GameItem.CopperOre));
            DrawSummaryCount(x, y + 217f, "철", inventory.Count(GameItem.IronOre));
            DrawSummaryCount(right, y + 217f, "금", inventory.Count(GameItem.GoldOre));
            DrawSummaryCount(x, y + 242f, "잡초", inventory.Count(GameItem.Weed));
            DrawSummaryCount(right, y + 242f, "목재", inventory.Count(GameItem.Wood));
            DrawSummaryCount(x, y + 267f, "물고기", fishCount);
        }

        private void DrawSummaryGroup(float x, float y, string title)
        {
            FarmGuiSkin.DrawSolid(new Rect(x, y + 7f, 3f, 12f), Clay);
            GUI.Label(new Rect(x + 8f, y, 192f, 25f), title, summaryLabel);
        }

        private void DrawSummaryCount(float x, float y, string label, int count)
        {
            ShopFill(new Rect(x, y, 97f, 22f), SummaryStripe, 3f);
            GUI.Label(new Rect(x + 5f, y, 44f, 22f), label, summaryItem);
            GUI.Label(new Rect(x + 39f, y, 53f, 22f), count.ToString(), summaryCount);
        }

        private void DrawInventoryItems(GameSession session, Rect well)
        {
            var index = 0;
            var decoration = session.SelectedDecoration;
            var crops = CropCatalog.GetAll();
            switch (inventoryPage)
            {
                case InventoryPage.Tools:
                    foreach (var tool in InventoryTools)
                    {
                        var owned = tool != FarmTool.Pickaxe || session.Data.hasPickaxe;
                        var selected = !decoration.HasValue && session.SelectedTool == tool;
                        if (DrawBagSlot(well, index++, GetToolLabel(tool), GameArtCatalog.GetHeldToolSprite(tool),
                            owned ? null : "미보유", selected ? "사용 중" : null, owned, selected, owned))
                            session.SelectTool(tool);
                    }
                    break;
                case InventoryPage.Seeds:
                    foreach (var crop in crops)
                    {
                        var count = session.Data.inventory.Count(crop.Seed);
                        var selected = !decoration.HasValue && session.SelectedTool == FarmTool.Seeds && session.SelectedSeed == crop.Seed;
                        if (DrawBagSlot(well, index++, ItemCatalog.GetLabel(crop.Seed),
                            FarmPlotArtCatalog.GetCropSprite(crop.Seed, true), $"×{count}", null,
                            count > 0, selected, count > 0, crop.Season))
                            session.SelectSeed(crop.Seed);
                    }
                    break;
                case InventoryPage.Produce:
                    foreach (var crop in crops) DrawBagStack(session, well, index++, crop.Produce);
                    foreach (var item in AnimalAndFishProducts) DrawBagStack(session, well, index++, item);
                    if (session.Data.houseUpgradeLevel >= 2)
                        foreach (var recipe in BuildingUpgradeCatalog.Recipes)
                            DrawBagStack(session, well, index++, recipe.Result);
                    break;
                case InventoryPage.Materials:
                    foreach (var item in InventoryMaterials) DrawBagStack(session, well, index++, item);
                    break;
                case InventoryPage.Decorations:
                    foreach (var entry in TownDecorationCatalog.GetAll())
                        DrawBagDecoration(session, well, index++, entry.Item);
                    DrawBagDecoration(session, well, index, GameItem.Sprinkler);
                    break;
            }
        }

        private void DrawBagStack(GameSession session, Rect well, int index, GameItem item)
        {
            var count = session.Data.inventory.Count(item);
            DrawBagSlot(well, index, ItemCatalog.GetLabel(item), GetInventoryArt(item), $"×{count}", null, count > 0);
        }

        private void DrawBagDecoration(GameSession session, Rect well, int index, GameItem item)
        {
            var count = session.Data.inventory.Count(item);
            var selected = session.SelectedDecoration == item;
            if (DrawBagSlot(well, index, ItemCatalog.GetLabel(item), GetInventoryArt(item), $"×{count}",
                selected ? "사용 중" : null, count > 0, selected, count > 0))
                session.SelectDecoration(item);
        }

        private bool DrawBagSlot(Rect well, int index, string label, Sprite art, string countBadge, string tag,
            bool owned, bool selected = false, bool actionable = false, Season? season = null)
        {
            const int columns = 6;
            const float gap = 8f;
            var cardHeight = inventoryPage == InventoryPage.Produce ? 109f : 140f;
            var width = (well.width - (columns + 1) * gap) / columns;
            var slot = new Rect(well.x + gap + index % columns * (width + gap),
                well.y + gap + index / columns * (cardHeight + gap), width, cardHeight);
            var surface = selected ? BagAccent : owned ? ShopCard : ShopPaper;
            if (actionable && slot.Contains(Event.current.mousePosition))
                surface = Color.Lerp(surface, BagAccent, 0.36f);
            ShopFill(slot, selected ? ShopGreen : ShopLine, 8f);
            ShopFill(new Rect(slot.x + 2f, slot.y + 2f, slot.width - 4f, slot.height - 4f), surface, 6f);

            DrawInventoryIcon(new Rect(slot.x + 25f, slot.y + 25f, slot.width - 50f, cardHeight - 63f), art, owned || selected);
            if (countBadge != null)
            {
                var badgeWidth = Mathf.Max(32f, bagBadge.CalcSize(new GUIContent(countBadge)).x + 12f);
                var badgeRect = new Rect(slot.xMax - badgeWidth - 5f, slot.y + 6f, badgeWidth, 22f);
                ShopFill(badgeRect, owned ? BagDark : ShopLine, 6f);
                GUI.Label(badgeRect, countBadge, owned ? bagActiveBadge : bagBadge);
            }
            if (tag != null)
            {
                var tagWidth = Mathf.Max(28f, bagBadge.CalcSize(new GUIContent(tag)).x + 10f);
                var tagRect = new Rect(slot.x + 5f, slot.y + 6f, tagWidth, 22f);
                ShopFill(tagRect, selected ? ShopGreen : BagWell, 6f);
                GUI.Label(tagRect, tag, selected ? bagActiveBadge : bagBadge);
            }
            if (season.HasValue)
            {
                var seasonBadge = new Rect(slot.x + 5f, slot.y + 6f, 26f, 22f);
                ShopFill(seasonBadge, BagWell, 6f);
                DrawSeasonIcon(seasonBadge, season.Value, 18f);
            }
            GUI.Label(new Rect(slot.x + 4f, slot.yMax - 33f, slot.width - 8f, 27f),
                label, owned || selected ? bagItem : bagMutedItem);
            return actionable && GUI.Button(slot, GUIContent.none, GUIStyle.none);
        }

        private void DrawInventoryIcon(Rect rect, Sprite art, bool owned)
        {
            var previousColor = GUI.color;
            if (!owned) GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * 0.60f);
            DrawShopArt(rect, art);
            GUI.color = previousColor;
        }

        private static Sprite GetInventoryArt(GameItem item)
        {
            if (CropCatalog.TryGetByProduce(item, out var produce)) return FarmPlotArtCatalog.GetCropSprite(produce.Seed, true);
            if (ItemCatalog.IsSeed(item)) return GameArtCatalog.GetHeldToolSprite(FarmTool.Seeds);
            if (TownDecorationCatalog.TryGet(item, out _)) return TownDecorationArt.GetSprite(item);
            return item switch
            {
                GameItem.Feed => FarmEquipmentArt.FeedSack,
                GameItem.Sprinkler => FarmEquipmentArt.Sprinkler,
                GameItem.Weed => WorldArtCatalog.GetSprite(WorldArtKind.Weed),
                _ => InventoryItemArt.GetSprite(item)
            };
        }

        private static void DrawInventoryPanel(Rect panel)
        {
            ShopFill(new Rect(panel.x, panel.y + 6f, panel.width, panel.height), BagShadow, 14f);
            ShopFill(panel, ShopGreen, 14f);
            ShopFill(new Rect(panel.x + 2f, panel.y + 2f, panel.width - 4f, panel.height - 4f), ShopPaper, 12f);
        }

        private void EnsureInventoryStyles()
        {
            EnsureShopStyles();
            if (bagTitle != null) return;
            bagTitle = ShopStyle(InventoryTitleFontSize, ShopPaper, FontStyle.Bold);
            bagTab = ShopStyle(19, ShopGreen, FontStyle.Bold, TextAnchor.MiddleCenter);
            bagItem = ShopStyle(18, ShopInk, FontStyle.Bold, TextAnchor.MiddleCenter);
            bagMutedItem = ShopStyle(18, ShopMuted, FontStyle.Normal, TextAnchor.MiddleCenter);
            bagBadge = ShopStyle(14, ShopGreen, FontStyle.Bold, TextAnchor.MiddleCenter);
            bagActiveBadge = ShopStyle(14, ShopPaper, FontStyle.Bold, TextAnchor.MiddleCenter);
            summaryHeading = ShopStyle(18, SummaryInk, FontStyle.Bold);
            summaryLabel = ShopStyle(14, SummaryInk, FontStyle.Bold);
            summaryItem = ShopStyle(14, SummaryInk);
            summaryCount = ShopStyle(16, SummaryInk, FontStyle.Bold, TextAnchor.MiddleRight);
        }
    }
}
