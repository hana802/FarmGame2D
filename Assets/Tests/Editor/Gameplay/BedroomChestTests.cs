using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class BedroomChestTests
    {
        [Test]
        public void MovingItemsBetweenBagAndChestPreservesTheirTotal()
        {
            var data = GameSaveData.CreateNew(0, 12);
            Assert.That(data.inventory.TryMoveTo(data.bedroomChest, GameItem.PotatoSeed, 6), Is.True);
            Assert.That(data.bedroomChest.TryMoveTo(data.inventory, GameItem.PotatoSeed, 1), Is.True);
            Assert.That(data.bedroomChest.TryMoveTo(data.inventory, GameItem.PotatoSeed, 6), Is.False);
            Assert.That(data.inventory.Count(GameItem.PotatoSeed), Is.EqualTo(1));
            Assert.That(data.bedroomChest.Count(GameItem.PotatoSeed), Is.EqualTo(5));
        }

        [Test]
        public void OldSavesGetAnEmptyChestAndNewSavesKeepItsContents()
        {
            var service = new SaveService();
            Assert.That(service.TryDeserialize("{\"version\":3}", out var data), Is.True);
            Assert.That(data.bedroomChest.Count(GameItem.Feed), Is.Zero);
            data.bedroomChest.Add(GameItem.Feed, 9);
            Assert.That(service.TrySerialize(data, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.bedroomChest.Count(GameItem.Feed), Is.EqualTo(9));
        }
    }
}
