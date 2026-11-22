namespace FarmGame2D.Core
{
    public static class SeasonCalendar
    {
        public const int DaysPerSeason = 28;
        public const int DaysPerWeek = 7;
        private static readonly string[] Weekdays = { "월", "화", "수", "목", "금", "토", "일" };

        public static int WeekIndex(int day) => (System.Math.Max(1, day) - 1) / DaysPerWeek;
        public static int WeekdayIndex(int day) => (System.Math.Max(1, day) - 1) % DaysPerWeek;
        public static string GetWeekdayLabel(int day) => Weekdays[WeekdayIndex(day)];
        public static bool IsWeekend(int day) => WeekdayIndex(day) >= 5;

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
