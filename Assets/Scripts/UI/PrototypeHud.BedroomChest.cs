using FarmGame2D.Art;
using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private Vector2 chestBagScroll;
        private Vector2 chestStorageScroll;

        private void DrawBedroomChest(GameSession session)
        {
            EnsureShopStyles();

            var panel = new Rect(164f, 75f, 952f, 570f);
            var header = new Rect(panel.x, panel.y, panel.width, 76f);
            FarmGuiSkin.DrawSolid(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight), BagScrim);
            ShopFill(panel, ShopPaper, 18f);
            ShopFill(header, ShopGreen, 18f);
            FarmGuiSkin.DrawSolid(new Rect(header.x, header.y + 26f, header.width, 50f), ShopGreen);
            DrawShopArt(new Rect(header.x + 23f, header.y + 14f, 57f, 48f), FarmhouseFurnitureArt.Chest);
            GUI.Label(new Rect(header.x + 94f, header.y + 10f, 275f, 56f), "침실 보관함", shopHeading);
            if (ShopButton(new Rect(header.xMax - 98f, header.y + 18f, 76f, 40f), "닫기", ShopPaper, shopSecondary))
            {
                session.CloseBedroomChest();
                return;
            }

            var bag = new Rect(panel.x + 22f, header.yMax + 18f, 446f, 427f);
            var chest = new Rect(bag.xMax + 16f, bag.y, bag.width, bag.height);
            DrawChestColumn(session, bag, session.Data.inventory, ref chestBagScroll, deposit: true);
            DrawChestColumn(session, chest, session.Data.bedroomChest, ref chestStorageScroll, deposit: false);
        }

        private void DrawChestColumn(GameSession session, Rect column, InventoryModel inventory, ref Vector2 scroll, bool deposit)
        {
            ShopFill(column, BagWell, 12f);
            GUI.Label(new Rect(column.x + 15f, column.y + 8f, column.width - 30f, 35f),
                deposit ? "가방" : "보관함", shopName);
            var occupied = inventory.entries.FindAll(entry => entry != null && entry.quantity > 0);
            if (occupied.Count == 0)
            {
                scroll = Vector2.zero;
                GUI.Label(new Rect(column.x + 15f, column.y + 57f, column.width - 30f, 34f),
                    deposit ? "가방에 옮길 아이템이 없어요." : "보관한 아이템이 없어요.", shopBody);
                return;
            }

            const float stride = 60f;
            var viewport = new Rect(column.x + 10f, column.y + 49f, column.width - 20f, column.height - 60f);
            var rowsHeight = occupied.Count * stride - 6f;
            var rowWidth = viewport.width - (rowsHeight > viewport.height ? 19f : 0f);
            scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0f, 0f, rowWidth, Mathf.Max(viewport.height, rowsHeight)));
            for (var i = 0; i < occupied.Count; i++)
            {
                var row = new Rect(0f, i * stride, rowWidth, 54f);
                DrawChestStack(session, row, occupied[i], deposit);
            }
            GUI.EndScrollView();
        }

        private void DrawChestStack(GameSession session, Rect row, InventoryEntry entry, bool deposit)
        {
            ShopFill(row, ShopCard, 8f);
            DrawShopArt(new Rect(row.x + 9f, row.y + 7f, 40f, 40f), GetInventoryArt(entry.item));
            GUI.Label(new Rect(row.x + 56f, row.y + 4f, row.width - 239f, 26f), ItemCatalog.GetLabel(entry.item), shopBody);
            GUI.Label(new Rect(row.x + 56f, row.y + 27f, 146f, 23f), $"×{entry.quantity:N0}", shopSmall);
            var single = new Rect(row.xMax - 205f, row.y + 8f, 91f, 38f);
            var all = new Rect(row.xMax - 106f, row.y + 8f, 98f, 38f);
            var oneLabel = deposit ? "1개 넣기" : "1개 꺼내기";
            var allLabel = deposit ? "모두 넣기" : "모두 꺼내기";
            if (ShopButton(single, oneLabel, ShopLine, shopSecondary))
                session.MoveBedroomChestItem(entry.item, 1, deposit);
            else if (entry.quantity > 1 && ShopButton(all, allLabel, ShopGreen, shopAction))
                session.MoveBedroomChestItem(entry.item, entry.quantity, deposit);
        }
    }
}
