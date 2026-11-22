using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.UI
{
    public sealed partial class PrototypeHud
    {
        private void DrawTurnipMarket(GameSession session)
        {
            EnsureShopStyles();
            EnsureSaleStyles();
            var panel = new Rect(250f, 118f, 780f, 470f);
            ShopFill(new Rect(0f, 0f, HudLayout.ReferenceWidth, HudLayout.ReferenceHeight), BagScrim, 0f);
            ShopFill(panel, ShopPaper, 18f);
            var header = new Rect(panel.x, panel.y, panel.width, 78f);
            ShopFill(header, ShopGreen, 18f);
            FarmGuiSkin.DrawSolid(new Rect(header.x, header.y + 24f, header.width, header.height - 24f), ShopGreen);
            DrawShopArt(new Rect(header.x + 22f, header.y + 12f, 54f, 54f), GetInventoryArt(GameItem.Turnip));
            GUI.Label(new Rect(header.x + 90f, header.y + 16f, 280f, 46f), "순무 장수", saleTitle);
            GUI.Label(new Rect(header.xMax - 330f, header.y + 22f, 230f, 34f), $"보유 금액  {session.Data.money:N0} G", shopLight);
            if (ShopButton(new Rect(header.xMax - 76f, header.y + 16f, 52f, 46f), "×", ShopGreen, shopAction))
            {
                session.CloseTurnipMarket();
                return;
            }

            var data = session.Data;
            var buying = TurnipMarket.IsPurchaseDay(data.day);
            var buy = TurnipMarket.BuyPrice(data.worldSeed, data.day);
            var sell = TurnipMarket.SellPrice(data.worldSeed, data.day);
            var owned = data.inventory.Count(GameItem.Turnip);
            var card = new Rect(panel.x + 24f, header.yMax + 18f, panel.width - 48f, 214f);
            ShopFill(card, ShopCard, 12f);
            GUI.Label(new Rect(card.x + 22f, card.y + 16f, card.width - 44f, 58f), TurnipMarket.PurchaseNote(data.day), shopBody);
            var priceLine = buying ? $"이번 주 구매가  {buy:N0} G    ·    오늘 시세  {sell:N0} G" : $"오늘 시세  {sell:N0} G";
            GUI.Label(new Rect(card.x + 22f, card.y + 84f, card.width - 44f, 34f), priceLine, shopName);
            GUI.Label(new Rect(card.x + 22f, card.y + 124f, card.width - 44f, 30f), $"가진 순무  {owned:N0}개", shopSmall);
            GUI.Label(new Rect(card.x + 22f, card.y + 158f, card.width - 44f, 30f), TurnipMarket.FreshnessNote(data.day), shopSmall);

            var notice = string.IsNullOrEmpty(session.TurnipNotice)
                ? "판매 상자에는 순무가 안 팔려요. 시세는 하루가 바뀔 때 한 번 움직입니다."
                : session.TurnipNotice;
            GUI.Label(new Rect(panel.x + 28f, card.yMax + 12f, panel.width - 56f, 48f), notice, shopSmall);

            var buttonY = panel.yMax - 78f;
            DrawTurnipButton(new Rect(panel.x + 24f, buttonY, 174f, 52f), buying && data.money >= buy, "1개 사기", () => session.BuyTurnips(1));
            DrawTurnipButton(new Rect(panel.x + 210f, buttonY, 174f, 52f), buying && data.money >= buy, "10개 사기", () => session.BuyTurnips(10));
            DrawTurnipButton(new Rect(panel.x + 396f, buttonY, 174f, 52f), owned > 0, "1개 팔기", () => session.SellTurnips(1));
            DrawTurnipButton(new Rect(panel.x + 582f, buttonY, 174f, 52f), owned > 0, "모두 팔기", () => session.SellTurnips(owned));
        }

        private void DrawTurnipButton(Rect rect, bool enabled, string label, System.Action action)
        {
            if (!enabled)
            {
                ShopFill(rect, ShopLine, 8f);
                GUI.Label(rect, label, shopSecondary);
                return;
            }

            if (ShopButton(rect, label, ShopGreen, shopAction)) action();
        }
    }
}
