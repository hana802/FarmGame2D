using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    public readonly struct SeasonalWeatherProfile
    {
        public SeasonalWeatherProfile(float emissionRate, Color color, Vector2 velocity, Vector2 sizeRange)
        {
            EmissionRate = emissionRate;
            Color = color;
            Velocity = velocity;
            SizeRange = sizeRange;
        }

        public float EmissionRate { get; }
        public Color Color { get; }
        public Vector2 Velocity { get; }
        public Vector2 SizeRange { get; }
    }

    public static class SeasonalAtmosphereProfile
    {
        private static readonly Color Clear = new(1f, 1f, 1f, 0f);
        private static readonly Color Dawn = new(0.98f, 0.48f, 0.3f, 0.2f);
        private static readonly Color Sunset = new(0.92f, 0.3f, 0.16f, 0.28f);
        private static readonly Color Night = new(0.03f, 0.08f, 0.24f, 0.55f);

        public static Color GetLighting(int hour, int minute)
        {
            var time = Mathf.Repeat(hour + (minute / 60f), 24f);
            if (time < 5f)
            {
                return Night;
            }

            if (time < 6.5f)
            {
                return Color.Lerp(Night, Dawn, Mathf.InverseLerp(5f, 6.5f, time));
            }

            if (time < 8f)
            {
                return Color.Lerp(Dawn, Clear, Mathf.InverseLerp(6.5f, 8f, time));
            }

            if (time < 16f)
            {
                return Clear;
            }

            if (time < 19f)
            {
                return Color.Lerp(Clear, Sunset, Mathf.InverseLerp(16f, 19f, time));
            }

            if (time < 21f)
            {
                return Color.Lerp(Sunset, Night, Mathf.InverseLerp(19f, 21f, time));
            }

            return Night;
        }

        public static SeasonalWeatherProfile GetWeather(Season season)
        {
            return season switch
            {
                Season.Spring => new SeasonalWeatherProfile(
                    7f,
                    new Color(1f, 0.9f, 0.96f, 0.98f),
                    new Vector2(0.38f, -0.46f),
                    new Vector2(0.2f, 0.38f)),
                Season.Autumn => new SeasonalWeatherProfile(
                    12f,
                    new Color(0.9f, 0.34f, 0.08f, 0.95f),
                    new Vector2(0.35f, -0.9f),
                    new Vector2(0.12f, 0.24f)),
                Season.Winter => new SeasonalWeatherProfile(
                    28f,
                    new Color(0.96f, 0.98f, 1f, 0.96f),
                    new Vector2(0.12f, -0.72f),
                    new Vector2(0.07f, 0.16f)),
                _ => new SeasonalWeatherProfile(0f, Color.clear, Vector2.zero, Vector2.zero)
            };
        }

        public static bool ShouldShowSummerRemark(
            Season season,
            int day,
            int hour,
            int lastRemarkDay,
            bool seasonJustStarted)
        {
            if (season != Season.Summer || day == lastRemarkDay)
            {
                return false;
            }

            return seasonJustStarted || hour is >= 12 and < 16;
        }
    }
}
