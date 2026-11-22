namespace FarmGame2D.Core
{
    public sealed partial class GameSession
    {
        public bool TurnipMarketOpen { get; private set; }
        public string TurnipNotice { get; private set; } = string.Empty;

        public void OpenTurnipMarket()
        {
            TurnipMarketOpen = true;
            PhoneOpen = false;
            ShopOpen = false;
            InventoryOpen = false;
            QuestBoardOpen = false;
            SellBoxOpen = false;
            OpenFacility = FarmFacility.None;
            NotifyChanged();
        }

        public void CloseTurnipMarket()
        {
            if (!TurnipMarketOpen) return;
            TurnipMarketOpen = false;
            TurnipNotice = string.Empty;
            NotifyChanged();
        }

        public void BuyTurnips(int quantity)
        {
            var price = TurnipMarket.BuyPrice(Data.worldSeed, Data.day);
            var bought = TurnipMarket.Buy(Data, quantity);
            Message = bought > 0
                ? $"순무 {bought}개를 {bought * price}G에 샀어요."
                : TurnipMarket.IsPurchaseDay(Data.day) ? "순무를 살 돈이 부족해요." : "순무는 매주 첫날에만 살 수 있어요.";
            NotifyChanged();
            if (bought > 0) SaveSilently();
        }

        public void SellTurnips(int quantity)
        {
            var before = Data.inventory.Count(GameItem.Turnip);
            var income = TurnipMarket.Sell(Data, quantity);
            if (income <= 0)
            {
                Message = "팔 순무가 없어요.";
                NotifyChanged();
                return;
            }

            var sold = before - Data.inventory.Count(GameItem.Turnip);
            Message = $"순무 {sold}개를 오늘 시세에 팔아 {income}G를 받았어요.";
            NotifyChanged();
            SaveSilently();
        }

        private void DiscardExpiredTurnips()
        {
            if (processedDay <= 1 || !TurnipMarket.IsPurchaseDay(processedDay)) return;
            var rotted = (long)TurnipMarket.Discard(Data.inventory) + TurnipMarket.Discard(Data.bedroomChest);
            if (rotted > 0)
                TurnipNotice = $"지난주의 순무 {rotted}개가 썩어 없어졌어요.";
        }
    }
}
