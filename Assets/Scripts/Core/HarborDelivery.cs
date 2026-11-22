namespace FarmGame2D.Core
{
    public static class HarborDelivery
    {
        public const int Quantity = 2;
        public const int Reward = 180;
        public const GameItem Catch = GameItem.HarborMackerel;

        public static bool IsComplete(GameSaveData data) =>
            data.lastHarborDeliveryWeek == SeasonCalendar.WeekIndex(data.day) + 1;

        public static bool TryComplete(GameSaveData data)
        {
            if (IsComplete(data) || !data.inventory.TrySpend(Catch, Quantity)) return false;
            data.lastHarborDeliveryWeek = SeasonCalendar.WeekIndex(data.day) + 1;
            data.money += Reward;
            return true;
        }
    }
}
