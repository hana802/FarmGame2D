using FarmGame2D.Core;
using FarmGame2D.World;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class SeasonalAtmosphereProfileTests
    {
        [Test]
        public void TimeLighting_IsClearAtNoonAndDarkAtMidnight()
        {
            var noon = SeasonalAtmosphereProfile.GetLighting(12, 0);
            var midnight = SeasonalAtmosphereProfile.GetLighting(0, 0);

            Assert.That(noon.a, Is.EqualTo(0f).Within(0.001f));
            Assert.That(midnight.a, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(midnight.b, Is.GreaterThan(midnight.r));
        }

        [Test]
        public void SeasonalWeather_UsesDistinctVisibleEffectsOutsideSummer()
        {
            var spring = SeasonalAtmosphereProfile.GetWeather(Season.Spring);
            var summer = SeasonalAtmosphereProfile.GetWeather(Season.Summer);
            var autumn = SeasonalAtmosphereProfile.GetWeather(Season.Autumn);
            var winter = SeasonalAtmosphereProfile.GetWeather(Season.Winter);

            Assert.That(spring.EmissionRate, Is.GreaterThan(0f));
            Assert.That(spring.SizeRange.x, Is.GreaterThanOrEqualTo(0.2f),
                "Cherry blossom petals must read as petals rather than tiny flying insects.");
            Assert.That(spring.SizeRange.y, Is.GreaterThanOrEqualTo(0.36f));
            Assert.That(autumn.EmissionRate, Is.GreaterThan(0f));
            Assert.That(winter.EmissionRate, Is.GreaterThan(autumn.EmissionRate));
            Assert.That(summer.EmissionRate, Is.EqualTo(0f));
            Assert.That(spring.Color, Is.Not.EqualTo(autumn.Color));
            Assert.That(winter.Color, Is.Not.EqualTo(autumn.Color));
        }

        [Test]
        public void SummerRemark_ShowsOncePerDayWhenSeasonStartsOrAtMidday()
        {
            Assert.That(SeasonalAtmosphereProfile.ShouldShowSummerRemark(Season.Summer, 29, 6, -1, true), Is.True);
            Assert.That(SeasonalAtmosphereProfile.ShouldShowSummerRemark(Season.Summer, 29, 13, -1, false), Is.True);
            Assert.That(SeasonalAtmosphereProfile.ShouldShowSummerRemark(Season.Summer, 29, 13, 29, false), Is.False);
            Assert.That(SeasonalAtmosphereProfile.ShouldShowSummerRemark(Season.Spring, 1, 13, -1, true), Is.False);
        }
    }
}
