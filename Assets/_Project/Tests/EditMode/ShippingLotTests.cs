using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The shipping window's lot: amounts per stack, kept within what is in the pack; shipping moves exactly the lot into the bin.
    public class ShippingLotTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        static int SlotOf(GameSession s, string itemId, int quality = 0)
        {
            for (var i = 0; i < s.Backpack.Capacity; i++)
            {
                var st = s.Backpack.Get(i);
                if (st != null && st.ItemId == itemId && st.Quality == quality) return i;
            }
            return -1;
        }

        [Test]
        public void TheAmount_StaysBetweenNoneAndTheWholeStack_AndToolsCannotGoInTheLot()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 10);
                var slot = SlotOf(s, "crop.parsnip");
                var lot = new ShippingLot();
                Assert.IsTrue(lot.IsEmpty);
                Assert.AreEqual(4, lot.Set(s, slot, 4));
                Assert.AreEqual(10, lot.Add(s, slot, 50), "more than the stack is kept to the stack");
                Assert.AreEqual(0, lot.Add(s, slot, -50), "less than nothing is kept to nothing");
                Assert.IsTrue(lot.IsEmpty);
                Assert.AreEqual(10, lot.All(s, slot));
                Assert.AreEqual(0, lot.None(slot));

                var hoe = SlotOf(s, ItemIds.Hoe);
                Assert.IsFalse(ShippingLot.CanShip(s, hoe), "a hoe is not for sale");
                Assert.AreEqual(0, lot.Set(s, hoe, 1));
            }
        }

        [Test]
        public void TheLotIsWorth_WhatTheBinWouldPay()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 4);
                s.Backpack.Add("crop.parsnip", 2, 2);        // gold quality, a separate stack
                var lot = new ShippingLot();
                lot.All(s, SlotOf(s, "crop.parsnip", 0));
                lot.All(s, SlotOf(s, "crop.parsnip", 2));
                Assert.AreEqual(6, lot.ItemCount);
                Assert.AreEqual(140 + 104, lot.Gold(s), "4 x 35 and 2 x 52, as the day-cycle test pays them");
            }
        }

        [Test]
        public void Shipping_MovesExactlyTheLot_AndNothingElse()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 10);
                s.Backpack.Add("crop.potato", 5);
                var lot = new ShippingLot();
                lot.Set(s, SlotOf(s, "crop.parsnip"), 4);
                lot.Set(s, SlotOf(s, "crop.potato"), 5);
                var shipped = lot.Ship(s);
                Assert.AreEqual(9, shipped);
                Assert.IsTrue(lot.IsEmpty, "the lot is spent");
                Assert.AreEqual(6, s.Backpack.Count("crop.parsnip"), "the rest of the parsnips stay");
                Assert.AreEqual(0, s.Backpack.Count("crop.potato"));
                Assert.AreEqual(4, s.State.ShippingBin.Where(x => x.ItemId == "crop.parsnip").Sum(x => x.Count));
                Assert.AreEqual(5, s.State.ShippingBin.Where(x => x.ItemId == "crop.potato").Sum(x => x.Count));
            }
        }

        [Test]
        public void ShippingAnEmptyLot_DoesNothing()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 3);
                Assert.AreEqual(0, new ShippingLot().Ship(s));
                Assert.AreEqual(3, s.Backpack.Count("crop.parsnip"));
                Assert.IsEmpty(s.State.ShippingBin);
            }
        }
    }
}
