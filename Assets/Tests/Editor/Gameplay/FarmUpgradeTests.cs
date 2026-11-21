using System.Linq;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Farming;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests
{
    public sealed class FarmUpgradeTests
    {
        [Test]
        public void SprinklerAndWateringCan_WaterTheMarkedSouthPattern()
        {
            var center = new Vector2(2.5f, -8.5f);
            var wateredOffsets = new[]
            {
                new Vector2(-1f, 0f), Vector2.zero, Vector2.right,
                new Vector2(-1f, -1f), new Vector2(1f, -1f),
                new Vector2(-1f, -2f), new Vector2(0f, -2f), new Vector2(1f, -2f)
            };
            var dryOffsets = new[]
            {
                Vector2.up, new Vector2(0f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(2f, 0f)
            };
            var save = new GameSaveData();
            foreach (var offset in wateredOffsets)
                save.farmPlots.Add(Plot(center.x + offset.x, center.y + offset.y));
            foreach (var offset in dryOffsets)
                save.farmPlots.Add(Plot(center.x + offset.x, center.y + offset.y));
            var greenhouse = Plot(center.x + 1f, center.y);
            greenhouse.greenhouse = true;
            save.farmPlots.Add(greenhouse);

            Assert.That(FarmUpgradeRules.SprinklerOffsets, Is.EqualTo(wateredOffsets));
            Assert.That(FarmUpgradeRules.WaterAround(save, center), Is.EqualTo(wateredOffsets.Length));
            Assert.That(save.farmPlots.Take(wateredOffsets.Length).All(plot => plot.state == SoilState.Watered), Is.True);
            Assert.That(save.farmPlots.Skip(wateredOffsets.Length).All(plot => plot.state == SoilState.Planted), Is.True);

            foreach (var plot in save.farmPlots) plot.state = SoilState.Planted;
            save.placedDecorations.Add(new PlacedDecorationData
            {
                item = GameItem.Sprinkler, worldX = center.x, worldY = center.y
            });
            Assert.That(FarmUpgradeRules.WaterWithSprinklers(save), Is.EqualTo(wateredOffsets.Length));
            Assert.That(save.farmPlots.Take(wateredOffsets.Length).All(plot => plot.state == SoilState.Watered), Is.True);
            Assert.That(save.farmPlots.Skip(wateredOffsets.Length).All(plot => plot.state == SoilState.Planted), Is.True);
            Assert.That(FarmUpgradeRules.WaterWithSprinklers(save), Is.Zero);
        }

        [Test]
        public void SprinklerSprite_StandsClearlyOnAFarmTile()
        {
            var sprite = FarmEquipmentArt.Sprinkler;
            var pixels = sprite.texture.GetPixels32();
            var opaque = 0;
            foreach (var pixel in pixels)
            {
                if (pixel.a > 200) opaque++;
            }

            Assert.That(sprite.bounds.size.x, Is.GreaterThan(0.7f));
            Assert.That(sprite.bounds.size.y, Is.GreaterThan(0.7f));
            Assert.That(opaque, Is.GreaterThan(pixels.Length / 4));
        }

        [Test]
        public void Greenhouse_HasEightDistinctPlotsAndChecksForExistingConstructionObstacles()
        {
            var positions = GreenhouseLayout.PlotPositions().ToArray();
            var save = new GameSaveData();
            Assert.That(positions.Distinct().Count(), Is.EqualTo(8));
            Assert.That(GreenhouseLayout.IsSiteOccupied(save), Is.False);
            var site = GreenhouseLayout.Site + Vector2.up;
            save.farmPlots.Add(Plot(site.x, site.y));
            Assert.That(GreenhouseLayout.IsSiteOccupied(save), Is.True);
        }

        [Test]
        public void OldSave_NormalizesMissingTreeDataAndRetainsSprinklers()
        {
            var save = JsonUtility.FromJson<GameSaveData>("{\"treeRegrowDays\":null,\"placedDecorations\":[{\"item\":29}]}");
            GameSaveDataNormalizer.Normalize(save);
            Assert.That(save.treeRegrowDays, Is.Empty);
            Assert.That(save.placedDecorations.Single().item, Is.EqualTo(GameItem.Sprinkler));
            save.treeRegrowDays.SetTreeRegrowDay(2, 8);
            save.treeRegrowDays.SetTreeRegrowDay(2, 12);
            Assert.That(save.treeRegrowDays, Has.Count.EqualTo(1));
            Assert.That(save.treeRegrowDays.TryGetTreeRegrowDay(2), Is.EqualTo(12));
        }

        private static FarmPlotData Plot(float x, float y) => new()
        {
            freeform = true, state = SoilState.Planted, worldX = x, worldY = y
        };
    }
}
