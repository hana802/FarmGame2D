using FarmGame2D.Core;
using NUnit.Framework;

namespace FarmGame2D.Tests
{
    public sealed class SellBoxAndDialogueTests
    {
        [Test]
        public void SellBox_ListsOnlySellableStacks()
        {
            var inventory = new InventoryModel();
            inventory.Add(GameItem.Egg, 4);
            inventory.Add(GameItem.Milk, 1);
            inventory.Add(GameItem.WheatSeed, 6);

            var offers = ProduceSale.List(inventory);

            Assert.That(offers, Has.Length.EqualTo(2));
            Assert.That(offers[0].Item, Is.EqualTo(GameItem.Egg));
            Assert.That(offers[0].Total, Is.EqualTo(100));
            Assert.That(offers[1].Item, Is.EqualTo(GameItem.Milk));
        }

        [Test]
        public void SellBox_SellsChosenQuantityAndLeavesOtherItems()
        {
            var inventory = new InventoryModel();
            inventory.Add(GameItem.Egg, 4);
            inventory.Add(GameItem.Milk, 2);
            inventory.Add(GameItem.WheatSeed, 6);

            var income = ProduceSale.Sell(inventory, GameItem.Egg, 1);

            Assert.That(income, Is.EqualTo(25));
            Assert.That(inventory.Count(GameItem.Egg), Is.EqualTo(3));
            Assert.That(inventory.Count(GameItem.Milk), Is.EqualTo(2));
            Assert.That(inventory.Count(GameItem.WheatSeed), Is.EqualTo(6));
            Assert.That(ProduceSale.Sell(inventory, GameItem.WheatSeed, 1), Is.Zero);
        }

        [Test]
        public void SellBox_SellAllKeepsSeeds()
        {
            var inventory = new InventoryModel();
            inventory.Add(GameItem.Egg, 2);
            inventory.Add(GameItem.WheatSeed, 3);

            var income = ProduceSale.SellAll(inventory);

            Assert.That(income, Is.EqualTo(50));
            Assert.That(inventory.Count(GameItem.Egg), Is.Zero);
            Assert.That(inventory.Count(GameItem.WheatSeed), Is.EqualTo(3));
        }
    }
}
