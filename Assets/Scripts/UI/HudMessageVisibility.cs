namespace FarmGame2D.UI
{
    public sealed class HudMessageVisibility
    {
        private readonly float displayDuration;
        private int observedRevision = -1;
        private float visibleUntil;

        public HudMessageVisibility(float displayDuration)
        {
            this.displayDuration = displayDuration;
        }

        public bool ShouldShow(int messageRevision, float currentTime)
        {
            if (messageRevision != observedRevision)
            {
                observedRevision = messageRevision;
                visibleUntil = currentTime + displayDuration;
            }

            return currentTime < visibleUntil;
        }
    }
}
