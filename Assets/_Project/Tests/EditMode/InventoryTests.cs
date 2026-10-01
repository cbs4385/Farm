using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class InventoryTests
    {
        static int MaxStack(string id) => id.StartsWith("tool.") ? 1 : 10;

        static Inventory Make(int slots = 4) => new Inventory(slots, MaxStack);

        [Test]
        public void Add_FillsExistingStackThenNewSlots()
        {
            var inv = Make();
            Assert.AreEqual(0, inv.Add("seed.a", 7));
            Assert.AreEqual(0, inv.Add("seed.a", 7));
            Assert.AreEqual(10, inv.Get(0).Count);
            Assert.AreEqual(4, inv.Get(1).Count);
            Assert.AreEqual(14, inv.Count("seed.a"));
        }

        [Test]
        public void Add_ReturnsOverflowWhenFull()
        {
            var inv = Make(2);
            Assert.AreEqual(5, inv.Add("seed.a", 25));
            Assert.AreEqual(20, inv.Count("seed.a"));
            Assert.IsFalse(inv.CanAdd("seed.a", 1));
        }

        [Test]
        public void DifferentQuality_DoesNotStack()
        {
            var inv = Make();
            inv.Add("crop.a", 2, 0);
            inv.Add("crop.a", 2, 1);
            Assert.IsNotNull(inv.Get(1));
            Assert.AreEqual(2, inv.Get(0).Count);
            Assert.AreEqual(1, inv.Get(1).Quality);
        }

        [Test]
        public void Remove_TakesAcrossSlots_AndReportsActual()
        {
            var inv = Make();
            inv.Add("seed.a", 15);
            Assert.AreEqual(12, inv.Remove("seed.a", 12));
            Assert.AreEqual(3, inv.Count("seed.a"));
            Assert.AreEqual(3, inv.Remove("seed.a", 99));
            Assert.IsNull(inv.Get(0));
            Assert.IsNull(inv.Get(1));
        }

        [Test]
        public void Move_SwapsDifferentItems_MergesSameItems()
        {
            var inv = Make();
            inv.Add("tool.hoe", 1);
            inv.Add("seed.a", 6);
            inv.Move(0, 1);
            Assert.AreEqual("seed.a", inv.Get(0).ItemId);
            Assert.AreEqual("tool.hoe", inv.Get(1).ItemId);

            inv.Move(0, 3);          // into empty slot
            inv.Add("seed.a", 3);    // goes into slot 3's stack? slot 0 empty now -> existing stack in slot 3
            Assert.AreEqual(9, inv.Get(3).Count);
            inv.Add("seed.a", 5);    // 14 total: slot 3 -> 10, new stack 4 in slot 0
            inv.Move(0, 3);          // 4 into a full stack: nothing moves
            Assert.AreEqual(10, inv.Get(3).Count);
            Assert.AreEqual(4, inv.Get(0).Count);
        }

        [Test]
        public void Resize_OnlyGrows()
        {
            var inv = Make(4);
            inv.Resize(2);
            Assert.AreEqual(4, inv.Capacity);
            inv.Resize(8);
            Assert.AreEqual(8, inv.Capacity);
        }

        [Test]
        public void Changed_FiresOnMutation()
        {
            var inv = Make();
            var fired = 0;
            inv.Changed += () => fired++;
            inv.Add("seed.a", 1);
            inv.Remove("seed.a", 1);
            Assert.AreEqual(2, fired);
        }

        [Test]
        public void DataRoundTrip_PreservesSlotsIncludingEmpty()
        {
            var inv = Make();
            inv.Add("tool.hoe", 1);
            inv.Add("seed.a", 7, 2);
            inv.Move(1, 3);

            var copy = Inventory.FromData(inv.ToData(), MaxStack);

            Assert.AreEqual(4, copy.Capacity);
            Assert.IsNull(copy.Get(1));
            Assert.AreEqual("seed.a", copy.Get(3).ItemId);
            Assert.AreEqual(7, copy.Get(3).Count);
            Assert.AreEqual(2, copy.Get(3).Quality);
        }
    }
}
