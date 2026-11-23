namespace FarmGame2D.Core
{
    public sealed class CookingRhythm
    {
        public const int RequiredHits = 3;
        public const float SweepSeconds = 1.2f;
        public const float WindowStart = 0.40f;
        public const float WindowEnd = 0.60f;

        public CookingRhythm(GameItem dish) => Dish = dish;

        public GameItem Dish { get; }
        public int Hits { get; private set; }
        public bool Resolved { get; private set; }
        public bool Succeeded => Resolved && Hits >= RequiredHits;
        public float Marker { get; private set; }

        public void Advance(float seconds)
        {
            if (Resolved || seconds <= 0f) return;
            if (seconds > SweepSeconds) seconds = SweepSeconds;

            var elapsed = Marker * SweepSeconds + seconds;
            if (elapsed >= SweepSeconds)
            {
                Resolved = true;
                return;
            }

            Marker = elapsed / SweepSeconds;
        }

        public bool TryStrike()
        {
            if (Resolved) return false;
            if (Marker < WindowStart || Marker > WindowEnd)
            {
                Resolved = true;
                return false;
            }

            Hits++;
            if (Hits >= RequiredHits)
            {
                Resolved = true;
                return true;
            }

            Marker = 0f;
            return true;
        }
    }
}
