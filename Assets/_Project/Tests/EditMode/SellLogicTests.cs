using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-09: sell to the general store's keeper in conversation, for gold at once; right-click moves one item.
    public class SellLogicTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        static int SlotOf(GameSession s, string itemId)
        {
            for (var i = 0; i < s.Backpack.Capacity; i++)
                if (s.Backpack.Get(i) != null && s.Backpack.Get(i).ItemId == itemId) return i;
            return -1;
        }

        [Test]
        public void Selling_pays_at_once_what_the_bin_would_pay()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 5);
                var slot = SlotOf(s, "crop.parsnip");
                var worth = ShippingLot.ValueOf(s, slot, 2);
                var gold = s.State.Gold;
                Assert.AreEqual(worth, SellLogic.Sell(s, slot, 2));
                Assert.AreEqual(gold + worth, s.State.Gold);
                Assert.AreEqual(3, s.Backpack.Count("crop.parsnip"));
                SellLogic.Sell(s, slot, int.MaxValue);
                Assert.AreEqual(0, s.Backpack.Count("crop.parsnip"));
            }
        }

        [Test]
        public void A_tool_cannot_be_sold()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                var hoe = SlotOf(s, ItemIds.Hoe);
                var gold = s.State.Gold;
                Assert.AreEqual(0, SellLogic.Sell(s, hoe, 1));
                Assert.AreEqual(gold, s.State.Gold);
                Assert.IsNotNull(s.Backpack.Get(hoe));
            }
        }

        [Test]
        public void MoveOne_moves_a_single_item_onto_an_empty_slot_or_the_same_kind()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                s.Backpack.Add("crop.parsnip", 3);
                var from = SlotOf(s, "crop.parsnip");
                var to = -1;
                for (var i = 0; i < s.Backpack.Capacity; i++) if (s.Backpack.Get(i) == null) { to = i; break; }
                Assert.IsTrue(s.Backpack.MoveOne(from, to));
                Assert.AreEqual(2, s.Backpack.Get(from).Count);
                Assert.AreEqual(1, s.Backpack.Get(to).Count);
                Assert.IsTrue(s.Backpack.MoveOne(from, to), "onto the same kind");
                Assert.AreEqual(2, s.Backpack.Get(to).Count);
                Assert.IsFalse(s.Backpack.MoveOne(from, SlotOf(s, ItemIds.Hoe)), "not onto another kind");
                Assert.IsFalse(s.Backpack.MoveOne(from, from));
            }
        }
    }
}
