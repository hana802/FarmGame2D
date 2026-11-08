using FarmGame2D.World;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class VillageRouteMapTests
    {
        [Test]
        public void MainStreetRoute_DetoursAroundVillageSquareDecorations()
        {
            var network = VillageRouteMap.CreateMainStreet();

            var leftDetour = network.GetNextNodeToward(0, 6);
            var rightDetour = network.GetNextNodeToward(3, 6);

            Assert.That(leftDetour, Is.Not.EqualTo(6),
                "The west route must not cut diagonally through the square's lamp and planter.");
            Assert.That(rightDetour, Is.Not.EqualTo(6),
                "The east route must not cut diagonally through the square's lamp and planter.");
            Assert.That(network.GetPoint(leftDetour).x, Is.LessThanOrEqualTo(-4.2f));
            Assert.That(network.GetPoint(rightDetour).x, Is.GreaterThanOrEqualTo(4.2f));
        }
    }
}
