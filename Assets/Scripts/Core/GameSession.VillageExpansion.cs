namespace FarmGame2D.Core
{
    public sealed partial class GameSession
    {
        public bool PurchaseMarketItem(GameItem item, int quantity)
        {
            if (!ShopOpen || !SeasonCalendar.IsWeekend(Data.day) || quantity <= 0) return false;
            foreach (var offer in WeekendMarket.GetOffers(ActiveShop, Data.day))
            {
                if (offer.Item != item) continue;
                if (quantity > Data.money / offer.Price) return false;

                Data.money -= quantity * offer.Price;
                Data.inventory.Add(item, quantity);
                SaveSilently();
                ShowMessage($"{ItemCatalog.GetLabel(item)} {quantity}개를 샀어요.");
                return true;
            }

            return false;
        }

        public void DeliverHarborFish()
        {
            if (HarborDelivery.IsComplete(Data))
            {
                ShowMessage("이번 주 납품을 마쳤어요. 월요일에 다음 의뢰가 와요.");
                return;
            }

            if (!HarborDelivery.TryComplete(Data))
            {
                ShowMessage($"부두에서 잡은 고등어 {HarborDelivery.Quantity}마리를 가져오면 {HarborDelivery.Reward} G를 받아요.");
                return;
            }

            ShowMessage($"민서의 납품 의뢰를 마쳤어요. +{HarborDelivery.Reward} G");
            SaveSilently();
        }
    }
}
