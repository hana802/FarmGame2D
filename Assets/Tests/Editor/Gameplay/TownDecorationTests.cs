using FarmGame2D.Core;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;

namespace FarmGame2D.Tests.Editor.Gameplay
{
    public sealed class TownDecorationTests
    {
        [Test]
        public void Catalog_OffersThreeDistinctPurchasableDecorations()
        {
            var decorations = TownDecorationCatalog.GetAll();
            Assert.That(decorations.Count, Is.EqualTo(3));
            foreach (var decoration in decorations)
            {
                Assert.That(decoration.Price, Is.GreaterThan(0));
                Assert.That(TownDecorationCatalog.TryGet(decoration.Item, out _), Is.True);
                Assert.That(ItemCatalog.GetLabel(decoration.Item), Is.Not.Empty);
            }
            Assert.That(TownDecorationCatalog.TryGet(GameItem.PotatoSeed, out _), Is.False);
        }

        [Test]
        public void SaveRoundTrip_PreservesOwnedAndPlacedDecorations()
        {
            var save = GameSaveData.CreateNew(0, 1234);
            save.inventory.Add(GameItem.FlowerPot, 2);
            save.placedDecorations.Add(new PlacedDecorationData
            {
                item = GameItem.TownBench,
                worldX = 4.5f,
                worldY = 0.5f
            });

            var service = new SaveService();
            Assert.That(service.TrySerialize(save, out var json), Is.True);
            Assert.That(service.TryDeserialize(json, out var restored), Is.True);
            GameSaveDataNormalizer.Normalize(restored);
            Assert.That(restored.inventory.Count(GameItem.FlowerPot), Is.EqualTo(2));
            Assert.That(restored.placedDecorations, Has.Count.EqualTo(1));
            Assert.That(restored.placedDecorations[0].item, Is.EqualTo(GameItem.TownBench));
            Assert.That(restored.placedDecorations[0].worldX, Is.EqualTo(4.5f));
        }

        [Test]
        public void OlderSaveWithoutDecorations_NormalizesToEmptyCollection()
        {
            var service = new SaveService();
            Assert.That(service.TryDeserialize("{\"version\":7,\"money\":200}", out var restored), Is.True);
            GameSaveDataNormalizer.Normalize(restored);
            Assert.That(restored.placedDecorations, Is.Not.Null);
            Assert.That(restored.placedDecorations, Is.Empty);
        }

        [Test]
        public void DecorationArt_IsPixelSpriteWithTransparency()
        {
            foreach (var definition in TownDecorationCatalog.GetAll())
            {
                var sprite = TownDecorationArt.GetSprite(definition.Item);
                Assert.That(sprite, Is.Not.Null);
                Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(sprite.texture.width, Is.GreaterThan(32));
                Assert.That(sprite.texture.height, Is.GreaterThan(32));
            }
        }
    }
}
