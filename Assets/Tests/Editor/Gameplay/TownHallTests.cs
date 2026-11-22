using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.World;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class TownHallTests
    {
        [Test]
        public void SaveReopensTownHallBeforeFarmhouseIsUpgraded()
        {
            var save = GameSaveData.CreateNew(0, 12);
            var entry = WorldAreaProfile.TownHallEntry;
            save.playerX = entry.x;
            save.playerY = entry.y;
            var service = new SaveService();

            Assert.That(service.TrySerialize(save, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.playerX, Is.EqualTo(entry.x));
            Assert.That(restored.playerY, Is.EqualTo(entry.y));
            Assert.That(WorldAreaProfile.IsInterior(entry), Is.True);
            Assert.That(WorldAreaProfile.IsFarmhouseBedroom(entry), Is.False);
        }

        [Test]
        public void TownHallBackdropUsesTheApprovedArtDimensions()
        {
            var sprite = WorldArtCatalog.GetSprite(WorldArtKind.TownHallInterior);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.texture.width, Is.EqualTo(1672));
            Assert.That(sprite.texture.height, Is.EqualTo(941));
        }
    }
}
