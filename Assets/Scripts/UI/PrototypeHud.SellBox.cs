using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private static readonly Color SaleHeader = new Color32(34, 67, 57, 255);
        private static readonly Color SaleAccent = new Color32(168, 104, 57, 255);
        private static readonly Color SaleSummary = new Color32(239, 224, 192, 255);
        private Vector2 sellScrollPosition;
        private bool sellBoxWasOpen;
        private bool sellAllConfirmationOpen;
        private string saleNotice;
        private GUIStyle saleTitle, saleName, salePrice, saleCount, saleTotal, saleSecondary, salePrompt, saleAmount;

        private void DrawSellBox(GameSession session)
        {
            if (!sellBoxWasOpen)
            {
                sellBoxWasOpen = true;
                sellScrollPosition = Vector2.zero;
                saleNotice = null;
                sellAllConfirmationOpen = false;
            }

            EnsureSaleStyles();
            var panel = new Rect(210f, 72f, 860f, 576f);
            var header = new Rect(panel.x, panel.y, panel.width, 82f);
            FarmGuiSkin.DrawSolid(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight),
                new Color32(22, 34, 28, 166));
            ShopFill(new Rect(panel.x, panel.y + 8f, panel.width, panel.height), new Color32(20, 24, 20, 130), 18f);
            ShopFill(panel, ShopPaper, 18f);
            ShopFill(header, SaleHeader, 18f);
            FarmGuiSkin.DrawSolid(new Rect(header.x, header.y + 24f, header.width, header.height - 24f), SaleHeader);
            DrawShopArt(new Rect(header.x + 24f, header.y + 12f, 55f, 56f), WorldArtCatalog.GetSprite(WorldArtKind.SellingBox));
            GUI.Label(new Rect(header.x + 93f, header.y + 17f, 312f, 48f), "판매 상자", saleTitle);
            GUI.Label(new Rect(header.xMax - 338f, header.y + 24f, 246f, 34f), $"보유 금액  {session.Data.money:N0} G", shopLight);
            if (ShopButton(new Rect(header.xMax - 67f, header.y + 17f, 48f, 48f), "×", ShopGreen, shopAction))
            {
                session.CloseSellBox();
                return;
            }

            var offers = ProduceSale.List(session.Data.inventory);
            var content = new Rect(panel.x + 24f, header.yMax + 21f, panel.width - 48f, 350f);
            if (offers.Length == 0)
                DrawEmptySellBox(content);
            else
                DrawSellOffers(session, content, offers, !sellAllConfirmationOpen);

            DrawSaleSummary(session, new Rect(panel.x + 24f, panel.yMax - 105f, panel.width - 48f, 82f), offers);
            if (sellAllConfirmationOpen)
                DrawSellAllConfirmation(session, panel, header.yMax, offers);
        }

        private void DrawEmptySellBox(Rect content)
        {
            ShopFill(content, ShopCard, 12f);
            DrawShopArt(new Rect(content.center.x - 40f, content.y + 67f, 80f, 86f), WorldArtCatalog.GetSprite(WorldArtKind.SellingBox));
            GUI.Label(new Rect(content.x + 30f, content.y + 168f, content.width - 60f, 44f),
                "지금 팔 수 있는 물품이 없어요", saleCount);
        }

        private void DrawSellOffers(GameSession session, Rect content, ProduceSale.Offer[] offers, bool interactive)
        {
            GUI.Label(new Rect(content.x + 13f, content.y, 255f, 25f), "품목", salePrice);
            GUI.Label(new Rect(content.x + 414f, content.y, 140f, 25f), "개당 가격", salePrice);
            var viewport = new Rect(content.x, content.y + 32f, content.width, content.height - 27f);
            const float stride = 80f;
            var width = viewport.width - (offers.Length * stride - 8f > viewport.height ? 18f : 0f);
            sellScrollPosition = GUI.BeginScrollView(viewport, sellScrollPosition,
                new Rect(0f, 0f, width, Mathf.Max(viewport.height, offers.Length * stride - 8f)));
            for (var index = 0; index < offers.Length; index++)
                DrawSellOffer(session, new Rect(0f, index * stride, width, 72f), offers[index], interactive);
            GUI.EndScrollView();
        }

        private void DrawSellOffer(GameSession session, Rect row, ProduceSale.Offer offer, bool interactive)
        {
            ShopFill(row, ShopCard, 10f);
            ShopFill(new Rect(row.x + 12f, row.y + 11f, 91f, 50f), ShopLine, 8f);
            GUI.Label(new Rect(row.x + 12f, row.y + 11f, 91f, 50f), $"×{offer.Quantity:N0}", saleCount);
            GUI.Label(new Rect(row.x + 121f, row.y + 11f, 267f, 50f), ItemCatalog.GetLabel(offer.Item), saleName);
            GUI.Label(new Rect(row.x + 414f, row.y + 11f, 116f, 50f), $"{offer.UnitPrice:N0} G", salePrice);
            var one = new Rect(row.xMax - 267f, row.y + 14f, 108f, 44f);
            var all = new Rect(row.xMax - 148f, row.y + 14f, 134f, 44f);
            if (SaleButton(offer.Quantity == 1 ? all : one, "1개 판매", offer.Quantity == 1 ? SaleHeader : ShopLine,
                offer.Quantity == 1 ? shopAction : saleSecondary, interactive))
            {
                session.SellProduce(offer.Item, 1);
                saleNotice = session.Message;
            }
            else if (offer.Quantity > 1 && SaleButton(all, "전량 판매", SaleHeader, shopAction, interactive))
            {
                session.SellProduce(offer.Item, offer.Quantity);
                saleNotice = session.Message;
            }
        }

        private void DrawSaleSummary(GameSession session, Rect footer, ProduceSale.Offer[] offers)
        {
            ShopFill(footer, SaleSummary, 10f);
            if (offers.Length == 0)
            {
                if (saleNotice != null) GUI.Label(new Rect(footer.x + 16f, footer.y + 12f, footer.width - 32f, 58f), saleNotice, salePrice);
                return;
            }

            var total = 0;
            foreach (var offer in offers) total += offer.Total;
            GUI.Label(new Rect(footer.x + 16f, footer.y + 10f, 158f, 32f), "전체 판매 금액", salePrice);
            GUI.Label(new Rect(footer.x + 174f, footer.y + 8f, 215f, 35f), $"{total:N0} G", saleTotal);
            if (saleNotice != null) GUI.Label(new Rect(footer.x + 16f, footer.y + 43f, footer.width - 230f, 27f), saleNotice, salePrice);
            if (SaleButton(new Rect(footer.xMax - 191f, footer.y + 15f, 174f, 52f), "모두 판매", SaleAccent, shopAction,
                !sellAllConfirmationOpen))
            {
                sellAllConfirmationOpen = true;
            }
        }

        private void DrawSellAllConfirmation(GameSession session, Rect panel, float headerBottom, ProduceSale.Offer[] offers)
        {
            var dim = Rect.MinMaxRect(panel.x, headerBottom, panel.xMax, panel.yMax);
            FarmGuiSkin.DrawSolid(dim, new Color32(22, 34, 28, 148));
            var card = new Rect(panel.center.x - 250f, panel.center.y - 86f, 500f, 188f);
            ShopFill(new Rect(card.x, card.y + 6f, card.width, card.height), new Color32(20, 24, 20, 90), 16f);
            ShopFill(card, ShopPaper, 16f);

            var total = 0;
            foreach (var offer in offers) total += offer.Total;
            GUI.Label(new Rect(card.x + 24f, card.y + 22f, card.width - 48f, 42f), "모두 판매하시겠습니까?", salePrompt);
            GUI.Label(new Rect(card.x + 24f, card.y + 64f, card.width - 48f, 34f), $"{total:N0} G", saleAmount);

            var no = new Rect(card.x + 24f, card.yMax - 68f, 214f, 48f);
            var yes = new Rect(card.xMax - 238f, card.yMax - 68f, 214f, 48f);
            if (ShopButton(no, "아니요", ShopLine, saleSecondary))
                sellAllConfirmationOpen = false;
            else if (ShopButton(yes, "네", SaleAccent, shopAction))
            {
                session.SellAllProduce();
                saleNotice = session.Message;
                sellAllConfirmationOpen = false;
            }

            if (GUI.Button(dim, GUIContent.none, GUIStyle.none) && !card.Contains(Event.current.mousePosition))
                sellAllConfirmationOpen = false;
        }

        private static bool SaleButton(Rect rect, string label, Color background, GUIStyle text, bool interactive)
        {
            if (!interactive)
            {
                ShopFill(rect, background, 8f);
                GUI.Label(rect, label, text);
                return false;
            }

            return ShopButton(rect, label, background, text);
        }

        private void EnsureSaleStyles()
        {
            EnsureShopStyles();
            if (saleTitle != null) return;
            saleTitle = ShopStyle(32, ShopPaper, FontStyle.Bold);
            saleName = ShopStyle(24, ShopInk, FontStyle.Bold);
            salePrice = ShopStyle(20, ShopInk);
            saleCount = ShopStyle(23, ShopInk, FontStyle.Bold, TextAnchor.MiddleCenter);
            saleTotal = ShopStyle(27, SaleHeader, FontStyle.Bold);
            saleSecondary = ShopStyle(19, SaleHeader, FontStyle.Bold, TextAnchor.MiddleCenter);
            salePrompt = ShopStyle(26, ShopInk, FontStyle.Bold, TextAnchor.MiddleCenter);
            saleAmount = ShopStyle(22, SaleHeader, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        private bool TryOpenClickedContainer<T>(Vector2 screenPosition) where T : InteractableBehaviour
        {
            if (!(Camera.main is Camera worldCamera)) return false;
            var point = worldCamera.ScreenToWorldPoint(screenPosition);
            foreach (var container in FindObjectsByType<T>())
            {
                if (Vector2.Distance(player.transform.position, container.transform.position) > 1.8f
                    || !container.TryGetComponent<SpriteRenderer>(out var boxRenderer)) continue;
                point.z = boxRenderer.bounds.center.z;
                if (!boxRenderer.bounds.Contains(point)) continue;
                container.Interact(player);
                return true;
            }
            return false;
        }
    }
}
