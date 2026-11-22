using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private Vector2 sellScrollPosition;
        private bool sellBoxWasOpen;
        private bool sellAllConfirmationOpen;
        private string saleNotice;
        private GUIStyle saleTitle, saleTotal, salePrompt;

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
            var panel = new Rect(164f, 75f, 952f, 570f);
            if (DrawContainerFrame(panel, WorldArtCatalog.GetSprite(WorldArtKind.SellingBox), "판매 상자"))
            {
                session.CloseSellBox();
                return;
            }

            var offers = ProduceSale.List(session.Data.inventory);
            var list = new Rect(panel.x + 22f, panel.y + 94f, 600f, 427f);
            var summary = new Rect(list.xMax + 16f, list.y, 292f, list.height);
            DrawSellOffers(session, list, offers);

            var total = 0;
            foreach (var offer in offers) total += offer.Total;
            DrawSaleSummary(session, summary, offers.Length > 0, total);
            if (sellAllConfirmationOpen)
                DrawSellAllConfirmation(session, panel, total);
        }

        private void DrawSellOffers(GameSession session, Rect column, ProduceSale.Offer[] offers)
        {
            ShopFill(column, BagWell, 12f);
            GUI.Label(new Rect(column.x + 15f, column.y + 8f, 200f, 35f), "판매할 아이템", shopName);
            if (offers.Length == 0)
            {
                sellScrollPosition = Vector2.zero;
                GUI.Label(new Rect(column.x + 15f, column.y + 57f, column.width - 30f, 34f),
                    "지금 팔 수 있는 아이템이 없어요.", shopBody);
                return;
            }

            const float stride = 60f;
            var viewport = new Rect(column.x + 10f, column.y + 49f, column.width - 20f, column.height - 60f);
            var rowsHeight = offers.Length * stride - 6f;
            var rowWidth = viewport.width - (rowsHeight > viewport.height ? 19f : 0f);
            sellScrollPosition = GUI.BeginScrollView(viewport, sellScrollPosition,
                new Rect(0f, 0f, rowWidth, Mathf.Max(viewport.height, rowsHeight)));
            for (var index = 0; index < offers.Length; index++)
                DrawSellOffer(session, new Rect(0f, index * stride, rowWidth, 54f), offers[index]);
            GUI.EndScrollView();
        }

        private void DrawSellOffer(GameSession session, Rect row, ProduceSale.Offer offer)
        {
            DrawContainerItem(row, offer.Item, offer.Quantity, row.width - 374f);
            GUI.Label(new Rect(row.xMax - 310f, row.y + 8f, 90f, 38f), $"{offer.UnitPrice:N0} G / 개", shopSmall);
            var single = new Rect(row.xMax - 205f, row.y + 8f, 91f, 38f);
            var all = new Rect(row.xMax - 106f, row.y + 8f, 98f, 38f);
            var interactive = !sellAllConfirmationOpen;
            if (SaleButton(single, "1개 판매", ShopLine, shopSecondary, interactive))
            {
                session.SellProduce(offer.Item, 1);
                saleNotice = session.Message;
            }
            else if (offer.Quantity > 1 && SaleButton(all, "전량 판매", ShopGreen, shopAction, interactive))
            {
                session.SellProduce(offer.Item, offer.Quantity);
                saleNotice = session.Message;
            }
        }

        private void DrawSaleSummary(GameSession session, Rect column, bool hasItems, int total)
        {
            ShopFill(column, BagWell, 12f);
            GUI.Label(new Rect(column.x + 15f, column.y + 8f, column.width - 30f, 35f), "판매 정산", shopName);
            var totals = new Rect(column.x + 10f, column.y + 49f, column.width - 20f, 150f);
            ShopFill(totals, ShopCard, 8f);
            GUI.Label(new Rect(totals.x + 15f, totals.y + 9f, totals.width - 30f, 24f), "보유 금액", shopSmall);
            GUI.Label(new Rect(totals.x + 15f, totals.y + 34f, totals.width - 30f, 30f), $"{session.Data.money:N0} G", shopName);
            FarmGuiSkin.DrawSolid(new Rect(totals.x + 15f, totals.y + 73f, totals.width - 30f, 1f), ShopLine);
            GUI.Label(new Rect(totals.x + 15f, totals.y + 83f, totals.width - 30f, 24f), "전체 판매 금액", shopSmall);
            GUI.Label(new Rect(totals.x + 15f, totals.y + 105f, totals.width - 30f, 35f), $"{total:N0} G", saleTotal);

            if (saleNotice != null)
                GUI.Label(new Rect(column.x + 15f, totals.yMax + 14f, column.width - 30f, 66f), saleNotice, shopBody);
            if (!hasItems) return;
            if (SaleButton(new Rect(column.x + 10f, column.yMax - 56f, column.width - 20f, 46f),
                    "모두 판매", ShopGreen, shopAction, !sellAllConfirmationOpen))
                sellAllConfirmationOpen = true;
        }

        private void DrawSellAllConfirmation(GameSession session, Rect panel, int total)
        {
            var dim = Rect.MinMaxRect(panel.x, panel.y + 76f, panel.xMax, panel.yMax);
            FarmGuiSkin.DrawSolid(dim, new Color32(22, 34, 28, 148));
            var card = new Rect(panel.center.x - 250f, panel.center.y - 86f, 500f, 188f);
            ShopFill(new Rect(card.x, card.y + 6f, card.width, card.height), new Color32(20, 24, 20, 90), 16f);
            ShopFill(card, ShopPaper, 16f);

            GUI.Label(new Rect(card.x + 24f, card.y + 22f, card.width - 48f, 42f), "모두 판매하시겠습니까?", salePrompt);
            GUI.Label(new Rect(card.x + 24f, card.y + 64f, card.width - 48f, 34f), $"{total:N0} G", salePrompt);

            var cancel = new Rect(card.x + 24f, card.yMax - 68f, 214f, 48f);
            var confirm = new Rect(card.xMax - 238f, card.yMax - 68f, 214f, 48f);
            if (ShopButton(cancel, "취소", ShopLine, shopSecondary))
                sellAllConfirmationOpen = false;
            else if (ShopButton(confirm, "모두 판매", ShopGreen, shopAction))
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
            saleTotal = ShopStyle(27, ShopGreen, FontStyle.Bold);
            salePrompt = ShopStyle(26, ShopInk, FontStyle.Bold, TextAnchor.MiddleCenter);
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
