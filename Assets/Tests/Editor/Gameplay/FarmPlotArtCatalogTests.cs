using FarmGame2D.Art;
using FarmGame2D.Farming;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class FarmPlotArtCatalogTests
    {
        [TestCase(SoilState.Empty)]
        [TestCase(SoilState.Tilled)]
        [TestCase(SoilState.Planted)]
        [TestCase(SoilState.Watered)]
        [TestCase(SoilState.Ready)]
        public void EverySoilState_HasAWorldSprite(SoilState state)
        {
            var sprite = FarmPlotArtCatalog.GetSprite(state);

            Assert.That(sprite, Is.Not.Null, $"Missing farm plot art for {state}.");
            Assert.That(sprite.texture, Is.Not.Null);
        }
    }
}
