using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void EveryCrop_HasItsOwnYoungAndRipeSprite()
        {
            var frames = new HashSet<Rect>();
            foreach (var crop in CropCatalog.GetAll())
            {
                var young = FarmPlotArtCatalog.GetCropSprite(crop.Seed, false);
                var ripe = FarmPlotArtCatalog.GetCropSprite(crop.Seed, true);
                Assert.That(young, Is.Not.Null, crop.Seed.ToString());
                Assert.That(ripe, Is.Not.Null, crop.Seed.ToString());
                Assert.That(frames.Add(young.rect), Is.True, young.name);
                Assert.That(frames.Add(ripe.rect), Is.True, ripe.name);
            }

            Assert.That(FarmPlotArtCatalog.GetCropSprite(GameItem.Feed, false), Is.Null);
        }
    }
}
