using System;

namespace FarmGame2D.TimeSystem
{
    public sealed class GameClock
    {
        private const int MinutesPerHour = 60;
        private const int MinutesPerDay = 24 * MinutesPerHour;

        private int minuteOfDay;

        public GameClock(int startHour = 6, int startMinute = 0)
        {
            if (startHour is < 0 or > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(startHour));
            }

            if (startMinute is < 0 or > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(startMinute));
            }

            Day = 1;
            minuteOfDay = (startHour * MinutesPerHour) + startMinute;
        }

        public int Day { get; private set; }
        public int Hour => minuteOfDay / MinutesPerHour;
        public int Minute => minuteOfDay % MinutesPerHour;
        public GameTimeSnapshot Current => new(Day, Hour, Minute);

        public void AdvanceMinutes(int minutes)
        {
            if (minutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes));
            }

            var totalMinutes = minuteOfDay + minutes;
            Day += totalMinutes / MinutesPerDay;
            minuteOfDay = totalMinutes % MinutesPerDay;
        }
    }
}
