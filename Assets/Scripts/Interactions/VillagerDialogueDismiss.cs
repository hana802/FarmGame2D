namespace FarmGame2D.Interactions
{
    public static class VillagerDialogueDismiss
    {
        public const float Seconds = 0.9f;

        public static bool ShouldClose(bool choosingGift, bool bondEventOpen, float visibleUntil, float now)
        {
            return !choosingGift && !bondEventOpen && now >= visibleUntil;
        }
    }
}
