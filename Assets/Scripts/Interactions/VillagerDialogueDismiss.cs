namespace FarmGame2D.Interactions
{
    public static class VillagerDialogueDismiss
    {
        public const float Seconds = 2.4f;

        public static bool ShouldClose(bool choosingGift, bool choosingFarmHelp, bool bondEventOpen, float visibleUntil, float now)
        {
            return !choosingGift && !choosingFarmHelp && !bondEventOpen && now >= visibleUntil;
        }
    }
}
