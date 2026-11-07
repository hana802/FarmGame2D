namespace FarmGame2D.TimeSystem
{
    public readonly struct GameTimeSnapshot
    {
        public GameTimeSnapshot(int day, int hour, int minute)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
        }

        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }

        public override string ToString()
        {
            return $"DAY {Day}\n{Hour:00}:{Minute:00}";
        }
    }
}
