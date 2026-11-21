using System.Linq;
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
        public void Sprinkler_WatersExactlyTheFourAdjacentPlantedPlots()
        {
            var save = new GameSaveData();
            save.placedDecorations.Add(new PlacedDecorationData { item = GameItem.Sprinkler, worldX = 2.5f, worldY = -8.5f });
            foreach (var offset in FarmUpgradeRules.SprinklerOffsets)
                save.farmPlots.Add(Plot(2.5f + offset.x, -8.5f + offset.y));
            save.farmPlots.Add(Plot(3.5f, -7.5f));
            save.farmPlots.Add(Plot(2.5f, -8.5f));

            Assert.That(FarmUpgradeRules.WaterWithSprinklers(save), Is.EqualTo(4));
            Assert.That(save.farmPlots.Take(4).All(plot => plot.state == SoilState.Watered), Is.True);
            Assert.That(save.farmPlots.Skip(4).All(plot => plot.state == SoilState.Planted), Is.True);
            Assert.That(FarmUpgradeRules.WaterWithSprinklers(save), Is.Zero);
        }

        [Test]
        public void GreenhousePlot_GrowsAfterTheMorningWateringWithoutACan()
        {
            var save = new GameSaveData();
            var indoor = new FarmPlotData
            {
                greenhouse = true,
                state = SoilState.Planted,
                plantedSeed = GameItem.PotatoSeed
            };
            var outdoor = new FarmPlotData { state = SoilState.Planted, plantedSeed = GameItem.PotatoSeed };
            save.farmPlots.Add(indoor);
            save.farmPlots.Add(outdoor);

            Assert.That(FarmUpgradeRules.WaterGreenhousePlots(save), Is.EqualTo(1));
            Assert.That(indoor.state, Is.EqualTo(SoilState.Watered));
            Assert.That(outdoor.state, Is.EqualTo(SoilState.Planted));

            indoor.AdvanceDay();
            Assert.That(indoor.growthDays, Is.EqualTo(1));
        }

        [Test]
        public void HouseRepair_SpendsWoodStoneAndIronOnce()
        {
            var save = new GameSaveData();
            save.inventory.Add(GameItem.Wood, MaterialProject.House.Wood);
            save.inventory.Add(GameItem.Stone, MaterialProject.House.Stone);
            save.inventory.Add(GameItem.IronOre, MaterialProject.House.Iron);

            Assert.That(MaterialProject.TryPay(save, MaterialProject.House), Is.True);
            Assert.That(save.inventory.Count(GameItem.Wood), Is.Zero);
            Assert.That(save.inventory.Count(GameItem.Stone), Is.Zero);
            Assert.That(save.inventory.Count(GameItem.IronOre), Is.Zero);
            Assert.That(MaterialProject.HasMaterials(save, MaterialProject.House), Is.False);
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

        [TestCase(0, 8)]
        [TestCase(1, 12)]
        [TestCase(2, 16)]
        public void GreenhouseSave_FillsMissingPlotsWithoutReplacingPlantedCrops(int level, int expectedPlots)
        {
            var original = GreenhouseLayout.PlotPositions().First();
            var save = JsonUtility.FromJson<GameSaveData>(
                "{\"greenhouseBuilt\":true,\"farmPlots\":[{\"greenhouse\":true,\"freeform\":true}]}");
            var crop = save.farmPlots.Single();
            crop.worldX = original.x;
            crop.worldY = original.y;
            crop.state = SoilState.Watered;
            save.greenhouseUpgradeLevel = level;

            GameSaveDataNormalizer.Normalize(save);
            GreenhouseLayout.EnsurePlots(save);
            Assert.That(save.farmPlots, Has.Count.EqualTo(expectedPlots));
            Assert.That(save.farmPlots, Does.Contain(crop));
            Assert.That(crop.state, Is.EqualTo(SoilState.Watered));

            var restored = JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(save));
            GameSaveDataNormalizer.Normalize(restored);
            Assert.That(restored.greenhouseUpgradeLevel, Is.EqualTo(level));
            Assert.That(restored.farmPlots, Has.Count.EqualTo(expectedPlots));
            Assert.That(restored.farmPlots.Count(plot => plot.state == SoilState.Watered), Is.EqualTo(1));
        }

        [Test]
        public void OldHouseRepair_OpensTheWorkshopWithoutChargingAgain()
        {
            var save = JsonUtility.FromJson<GameSaveData>("{\"houseUpgraded\":true,\"money\":700}");
            GameSaveDataNormalizer.Normalize(save);

            Assert.That(save.houseUpgradeLevel, Is.EqualTo(1));
            Assert.That(save.money, Is.EqualTo(700));
        }

        private static FarmPlotData Plot(float x, float y) => new()
        {
            freeform = true, state = SoilState.Planted, worldX = x, worldY = y
        };
    }
}
