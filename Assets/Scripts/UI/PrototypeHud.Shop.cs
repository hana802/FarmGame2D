using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.World;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private static readonly Color ShopGreen = new Color32(39, 77, 64, 255);
        private static readonly Color ShopPaper = new Color32(249, 244, 230, 255);
        private static readonly Color ShopCard = new Color32(255, 252, 243, 255);
        private static readonly Color ShopLine = new Color32(225, 214, 189, 255);
        private static readonly Color ShopGold = new Color32(247, 212, 137, 255);
        private static readonly Color ShopInk = new Color32(43, 49, 39, 255);
        private static readonly Color ShopMuted = new Color32(98, 94, 74, 255);

        private readonly ShopPurchaseConfirmation shopPurchaseConfirmation = new();
        private int shopOrderCount = 1;
        private bool shopWasOpen;
        private ShopKind shownShop;
        private int shopTab;
        private string shopNotice;
        private GUIStyle shopHeading, shopLight, shopName, shopBody, shopSmall, shopAction, shopSecondary, shopQuantity;

        private void DrawShop(GameSession session)
        {
            if (!shopWasOpen || shownShop != session.ActiveShop)
            {
                shopWasOpen = true;
                shownShop = session.ActiveShop;
                shopTab = 0;
                shopNotice = null;
                shopOrderCount = 1;
                shopPurchaseConfirmation.Cancel();
            }

            EnsureShopStyles();
            var layout = HudLayout.CalculateShopLayout();
            var mining = shownShop == ShopKind.Mining;
            var input = Event.current;
            if (input.type == EventType.KeyDown && input.keyCode == KeyCode.Escape)
            {
                input.Use();
                if (shopPurchaseConfirmation.Pending.HasValue) shopPurchaseConfirmation.Cancel();
                else CloseShop(session);
                return;
            }

            FarmGuiSkin.DrawSolid(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight),
                new Color32(21, 33, 27, 158));
            ShopFill(new Rect(layout.Panel.x, layout.Panel.y + 8f, layout.Panel.width, layout.Panel.height),
                new Color32(20, 33, 27, 135), 20f);
            ShopFill(layout.Panel, ShopPaper, 20f);
            ShopFill(layout.Header, ShopGreen, 20f);
            FarmGuiSkin.DrawSolid(new Rect(layout.Header.x, layout.Header.y + 24f, layout.Header.width,
                layout.Header.height - 24f), ShopGreen);
            DrawShopArt(new Rect(layout.Header.x + 23f, layout.Header.y + 12f, 56f, 50f),
                WorldArtCatalog.GetSprite(mining ? WorldArtKind.GeneralStore : WorldArtKind.SeedFeedShop));
            GUI.Label(layout.Title, mining ? "마을 잡화점" : "씨앗과 가축 상점", shopHeading);
            GUI.Label(layout.Balance, $"{session.Data.money:N0} G", shopLight);
            if (ShopButton(layout.CloseButton, "×", new Color32(61, 98, 82, 255), shopAction))
            {
                CloseShop(session);
                return;
            }

            if (shopPurchaseConfirmation.Pending is ShopPurchaseRequest request)
                DrawShopDetail(session, layout, request);
            else
                DrawShopList(session, layout, mining);
        }

        private void CloseShop(GameSession session)
        {
            shopWasOpen = false;
            shopPurchaseConfirmation.Cancel();
            session.OpenShop(session.ActiveShop);
        }

        private void DrawShopList(GameSession session, ShopHudLayout layout, bool mining)
        {
            var tabs = mining ? GeneralShopTabs : FarmShopTabs;
            ShopFill(layout.Tabs, ShopLine, 12f);
            for (var index = 0; index < tabs.Length; index++)
            {
                var selected = shopTab == index;
                if (!ShopButton(layout.GetTab(index, tabs.Length), tabs[index], selected ? ShopGreen : ShopLine,
                        selected ? shopAction : shopSecondary)) continue;
                shopTab = index;
                shopNotice = null;
            }

            var row = 0;
            foreach (var product in GetShopProducts(session, mining, shopTab))
            {
                var card = layout.GetProduct(row++);
                var hover = card.Contains(Event.current.mousePosition);
                ShopFill(card, hover ? ShopLine : ShopCard, 10f);
                ShopFill(new Rect(card.x + 10f, card.y + 8f, 62f, 58f), ShopLine, 8f);
                DrawShopArt(new Rect(card.x + 16f, card.y + 12f, 50f, 50f), GetShopArt(product));
                GUI.Label(new Rect(card.x + 88f, card.y + 9f, 372f, 29f), product.DisplayName, shopName);
                GUI.Label(new Rect(card.x + 88f, card.y + 37f, 372f, 25f), ShopProductNote(session, product), shopSmall);
                GUI.Label(new Rect(card.xMax - 186f, card.y + 17f, 88f, 40f), ShopPrice(product), shopName);
                ShopFill(new Rect(card.xMax - 88f, card.y + 17f, 72f, 40f), ShopPaper, 8f);
                GUI.Label(new Rect(card.xMax - 88f, card.y + 17f, 72f, 40f), "보기 ›", shopSecondary);
                if (GUI.Button(card, GUIContent.none, GUIStyle.none))
                {
                    shopOrderCount = 1;
                    shopPurchaseConfirmation.Request(product);
                    shopNotice = null;
                }
            }

            var footer = shopNotice ?? (mining ? string.Empty : $"보유 가축  ·  닭 {session.Data.ownedChickens}마리   소 {session.Data.ownedCows}마리");
            GUI.Label(layout.ListFooter, footer, shopSmall);
        }

        private void DrawShopDetail(GameSession session, ShopHudLayout layout, ShopPurchaseRequest request)
        {
            if (ShopButton(layout.BackButton, "‹  상품 목록", ShopLine, shopSecondary))
            {
                shopOrderCount = 1;
                shopPurchaseConfirmation.Cancel();
                return;
            }

            ShopFill(layout.Detail, ShopCard, 12f);
            var icon = new Rect(layout.Detail.x + 22f, layout.Detail.y + 18f, 86f, 76f);
            ShopFill(icon, ShopLine, 10f);
            DrawShopArt(new Rect(icon.x + 9f, icon.y + 7f, 68f, 62f), GetShopArt(request));
            GUI.Label(new Rect(icon.xMax + 18f, icon.y + 1f, 400f, 38f), request.DisplayName, shopName);
            GUI.Label(new Rect(icon.xMax + 18f, icon.y + 38f, 400f, 28f), ShopProductNote(session, request), shopSmall);
            FarmGuiSkin.DrawSolid(new Rect(layout.Detail.x + 22f, icon.yMax + 11f, layout.Detail.width - 44f, 1f), ShopLine);

            var guide = ShopItemGuide.Describe(request).Split('\n');
            var y = icon.yMax + 20f;
            for (var index = 1; index < guide.Length; index++)
            {
                var content = new GUIContent(guide[index]);
                var width = layout.Detail.width - 64f;
                var height = Mathf.Max(27f, shopBody.CalcHeight(content, width));
                ShopFill(new Rect(layout.Detail.x + 23f, y + 10f, 5f, 5f), ShopGreen, 2.5f);
                GUI.Label(new Rect(layout.Detail.x + 37f, y, width, height), content, shopBody);
                y += height + 3f;
            }

            var maximum = ShopOrderRules.Maximum(request, session.Data.money, session.Data.ownedChickens, session.Data.ownedCows);
            var chooseQuantity = ShopOrderRules.AllowsMultiple(request.Kind) && maximum > 1;
            shopOrderCount = chooseQuantity ? Mathf.Clamp(shopOrderCount, 1, maximum) : 1;

            var unavailable = ShopUnavailableReason(session, request);
            var summaryRect = layout.DetailFooter;
            if (chooseQuantity) summaryRect.width -= 148f;
            GUI.Label(summaryRect, unavailable ?? ShopOrderRules.Summary(request, shopOrderCount), shopName);
            if (chooseQuantity) DrawShopQuantity(layout.DetailFooter, maximum);

            var label = request.IsSale ? "판매하기" : "구매하기";
            if (unavailable == null && ShopButton(layout.ConfirmButton, label, ShopGreen, shopAction))
            {
                var count = shopOrderCount;
                shopOrderCount = 1;
                shopPurchaseConfirmation.Confirm(confirmed => ExecutePurchase(session, confirmed, count));
                shopNotice = session.Message;
            }
        }

        private void DrawShopQuantity(Rect footer, int maximum)
        {
            var height = 40f;
            var y = footer.y + (footer.height - height) * 0.5f;
            var minus = new Rect(footer.xMax - 132f, y, 40f, height);
            var value = new Rect(minus.xMax, y, 52f, height);
            var plus = new Rect(value.xMax, y, 40f, height);
            var idle = new Color32(236, 230, 214, 255);
            if (ShopButton(minus, "−", shopOrderCount > 1 ? ShopLine : idle, shopSecondary) && shopOrderCount > 1)
                shopOrderCount--;
            GUI.Label(value, shopOrderCount.ToString(), shopQuantity);
            var canIncrease = shopOrderCount < maximum;
            if (ShopButton(plus, "+", canIncrease ? ShopGreen : idle, canIncrease ? shopAction : shopSecondary) && canIncrease)
                shopOrderCount++;
        }

        private void EnsureShopStyles()
        {
            if (shopHeading != null) return;
            shopHeading = ShopStyle(28, ShopPaper, FontStyle.Bold);
            shopLight = ShopStyle(20, ShopGold, FontStyle.Bold, TextAnchor.MiddleRight);
            shopName = ShopStyle(22, ShopInk, FontStyle.Bold);
            shopBody = ShopStyle(19, ShopInk, anchor: TextAnchor.UpperLeft);
            shopBody.wordWrap = true;
            shopSmall = ShopStyle(16, ShopMuted);
            shopAction = ShopStyle(20, ShopPaper, FontStyle.Bold, TextAnchor.MiddleCenter);
            shopSecondary = ShopStyle(17, ShopGreen, FontStyle.Bold, TextAnchor.MiddleCenter);
            shopQuantity = ShopStyle(22, ShopInk, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        private GUIStyle ShopStyle(int size, Color color, FontStyle weight = FontStyle.Normal,
            TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            return new GUIStyle(GUIStyle.none)
            {
                font = koreanFont, fontSize = size, fontStyle = weight, alignment = anchor,
                clipping = TextClipping.Clip, normal = { textColor = color }
            };
        }

        private static void ShopFill(Rect rect, Color color, float radius)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
        }

        private static bool ShopButton(Rect rect, string label, Color background, GUIStyle text)
        {
            if (rect.Contains(Event.current.mousePosition)) background = Color.Lerp(background, Color.white, 0.10f);
            ShopFill(rect, background, 8f);
            GUI.Label(rect, label, text);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        private void DrawShopArt(Rect rect, Sprite sprite)
        {
            if (sprite == null) return;
            var scale = Mathf.Min(rect.width / sprite.rect.width, rect.height / sprite.rect.height);
            var size = sprite.rect.size * scale;
            farmSkin.DrawSprite(new Rect(rect.center - size * 0.5f, size), sprite);
        }
    }
}
