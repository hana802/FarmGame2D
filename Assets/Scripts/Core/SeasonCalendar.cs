namespace FarmGame2D.Core
{
    public static class SeasonCalendar
    {
        public const int DaysPerSeason = 28;

        public static Season GetSeason(int day)
        {
            var safeDay = day < 1 ? 1 : day;
            return (Season)(((safeDay - 1) / DaysPerSeason) % 4);
        }

        public static int GetDayOfSeason(int day)
        {
            var safeDay = day < 1 ? 1 : day;
            return ((safeDay - 1) % DaysPerSeason) + 1;
        }
    }
}
