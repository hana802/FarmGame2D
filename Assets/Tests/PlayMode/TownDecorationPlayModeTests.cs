using System.Collections;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using FarmGame2D.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FarmGame2D.Tests
{
    public sealed class TownDecorationPlayModeTests
    {
        [UnityTest]
        public IEnumerator TownDecoration_CanPlaceAndPickUpPurchasedItem()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var session = GameSession.Instance;
            var field = Object.FindFirstObjectByType<TownDecorationField>();
            Assert.That(session, Is.Not.Null);
            Assert.That(field, Is.Not.Null);

            const GameItem item = GameItem.FlowerPot;
            var inventoryBefore = session.Data.inventory.Count(item);
            var placedBefore = session.Data.placedDecorations.Count;
            var moneyBefore = session.Data.money;
            session.Data.money += TownDecorationCatalog.GetAll()[0].Price;
            session.PurchaseDecoration(item);
            Assert.That(session.Data.money, Is.EqualTo(moneyBefore));
            Assert.That(session.Data.inventory.Count(item), Is.EqualTo(inventoryBefore + 1));
            session.SelectDecoration(item);
            var candidates = new[]
            {
                new Vector2(4.5f, 0.5f), new Vector2(8.5f, 0.5f),
                new Vector2(-4.5f, 0.5f), new Vector2(-8.5f, 0.5f)
            };
            var placed = false;
            foreach (var candidate in candidates)
            {
                if (!field.TryPlace(candidate)) continue;
                placed = true;
                break;
            }
            Assert.That(placed, Is.True, "At least one village grass tile should accept a decoration.");
            Assert.That(session.Data.inventory.Count(item), Is.EqualTo(inventoryBefore));
            Assert.That(session.Data.placedDecorations.Count, Is.EqualTo(placedBefore + 1));
            yield return null;

            var placedObject = Object.FindFirstObjectByType<TownDecorationInteractable>();
            Assert.That(placedObject, Is.Not.Null);
            Assert.That(placedObject.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(session.PickUpDecoration(placedBefore), Is.True);
            Assert.That(session.Data.inventory.Count(item), Is.EqualTo(inventoryBefore + 1));
            Assert.That(session.Data.placedDecorations.Count, Is.EqualTo(placedBefore));
        }
    }
}
