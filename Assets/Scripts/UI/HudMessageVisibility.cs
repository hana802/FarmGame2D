namespace FarmGame2D.UI
{
    public sealed class HudMessageVisibility
    {
        private int observedRevision = -1;
        private float visibleUntil;

        public bool ShouldShow(int messageRevision, float currentTime, float displayDuration)
        {
            if (messageRevision != observedRevision)
            {
                observedRevision = messageRevision;
                visibleUntil = currentTime + UnityEngine.Mathf.Max(0f, displayDuration);
            }

            return currentTime < visibleUntil;
        }
    }
}
