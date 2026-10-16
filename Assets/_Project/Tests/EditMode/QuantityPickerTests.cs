using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The "how many?" window's arithmetic (playtest 2026-10-16: putting things in the shipping bin took too many clicks).
    public class QuantityPickerTests
    {
        [Test]
        public void TheNumber_IsKeptBetweenTheMinimumAndTheMaximum()
        {
            Assert.AreEqual(5, QuantityPicker.Clamp(5, 1, 10));
            Assert.AreEqual(10, QuantityPicker.Clamp(50, 1, 10));
            Assert.AreEqual(1, QuantityPicker.Clamp(-3, 1, 10));
            Assert.AreEqual(0, QuantityPicker.Clamp(-3, 0, 10));
            Assert.AreEqual(3, QuantityPicker.Clamp(9, 3, 2), "a maximum below the minimum gives the minimum");
        }

        [Test]
        public void TheButtons_StepByOneFiveAndAll_AndStopAtTheEnds()
        {
            Assert.AreEqual(6, QuantityPicker.Nudge(5, 1, 0, 12));
            Assert.AreEqual(10, QuantityPicker.Nudge(5, 5, 0, 12));
            Assert.AreEqual(12, QuantityPicker.Nudge(10, 5, 0, 12));
            Assert.AreEqual(0, QuantityPicker.Nudge(3, -5, 0, 12));
            Assert.AreEqual(12, QuantityPicker.Nudge(0, int.MaxValue / 2, 0, 12), "+All");
        }

        [Test]
        public void TypedText_IsReadAsDigitsOnly_AndNeverAboveTheLimit()
        {
            Assert.AreEqual(12, QuantityPicker.Parse("12", 1, 99));
            Assert.AreEqual(1, QuantityPicker.Parse("", 1, 99), "nothing typed is the minimum");
            Assert.AreEqual(1, QuantityPicker.Parse("abc", 1, 99));
            Assert.AreEqual(7, QuantityPicker.Parse("-7", 1, 99), "a minus sign is ignored");
            Assert.AreEqual(99, QuantityPicker.Parse("4000", 1, 99));
            Assert.AreEqual(99, QuantityPicker.Parse("99999999999999999999", 1, 99), "a number too big for an int is the maximum");
            Assert.AreEqual(0, QuantityPicker.Parse("0", 0, 99));
        }

        [Test]
        public void ForSale_NeverMoreThanTheStackHolds_AndAnEmptySlotGivesNothing()
        {
            using (var f = new TestSessionFixture(new[] { Item("crop.parsnip", 20, sell: 35, buy: 0) }))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 9);
                var slot = 0;
                while (s.Backpack.Get(slot)?.ItemId != "crop.parsnip") slot++;          // the new game starts with tools in the first slots
                Assert.AreEqual(9, QuantityPicker.MaxToGive(s, slot));
                var empty = 0;
                while (s.Backpack.Get(empty) != null) empty++;
                Assert.AreEqual(0, QuantityPicker.MaxToGive(s, empty), "an empty slot");
                Assert.AreEqual(0, QuantityPicker.MaxToGive(s, -1));
                Assert.AreEqual(0, QuantityPicker.MaxToGive(s, 999));
            }
        }

        [Test]
        public void ToBuy_IsWhatTheGoldPaysFor_AndWhatTheBackpackHasRoomFor()
        {
            var seed = Item("seed.test", 99, sell: 1, buy: 20);
            using (var f = new TestSessionFixture(new[] { seed }))
            {
                var s = f.Session;
                s.State.Gold = 205;
                Assert.AreEqual(10, QuantityPicker.MaxToBuy(s, seed), "205 gold buys ten at 20");
                s.State.Gold = 19;
                Assert.AreEqual(0, QuantityPicker.MaxToBuy(s, seed));
                s.State.Gold = 100000;
                var room = QuantityPicker.MaxToBuy(s, seed);
                Assert.IsTrue(s.Backpack.CanAdd(seed.Id, room), "what is offered fits");
                Assert.IsFalse(s.Backpack.CanAdd(seed.Id, room + 1), "and one more would not");
                Assert.AreEqual(0, QuantityPicker.MaxToBuy(s, Item("free", 10, sell: 1, buy: 0)), "an item that is not for sale");
            }
        }

        static ItemDefinition Item(string id, int maxStack, int sell, int buy)
        {
            return ItemDefinition.Create(id, ItemCategory.Seed, maxStack, sell, buy);
        }
    }
}
