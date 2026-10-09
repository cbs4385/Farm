using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest report (2026-10-05): "after putting my tools into the chest and then retrieving them, I was unable to use the hoe: the tools
    // were no longer in their original location on the item bar."
    public class ChestTransferTests
    {
        static int MaxStack(string id) => id.StartsWith("tool.") ? 1 : 10;

        static Inventory Pack()
        {
            var pack = new Inventory(24, MaxStack);
            pack.Add("tool.hoe", 1); pack.Add("tool.can", 1); pack.Add("tool.axe", 1);
            return pack;
        }

        [Test]
        public void ToolsReturnToTheirOwnSlots_WhateverOrderTheyWentInAndCameOut()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            ChestTransfer.Move(pack, 2, chest, true);     // axe first, hoe last
            ChestTransfer.Move(pack, 1, chest, true);
            ChestTransfer.Move(pack, 0, chest, true);
            for (var i = 0; i < chest.Capacity; i++) ChestTransfer.Move(chest, i, pack, false);   // taken back in the chest's order

            Assert.AreEqual("tool.hoe", pack.Get(0).ItemId);
            Assert.AreEqual("tool.can", pack.Get(1).ItemId);
            Assert.AreEqual("tool.axe", pack.Get(2).ItemId);
        }

        [Test]
        public void ATool_ReturnsToItsSlot_EvenWhenSomethingElseTookAnEarlierOne()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            ChestTransfer.Move(pack, 0, chest, true);                  // the hoe goes in ...
            pack.Add("seed.parsnip", 5);                         // ... a pickup takes its slot ...
            ChestTransfer.Move(chest, 0, pack, false);                  // ... and the hoe comes out again
            Assert.AreEqual("seed.parsnip", pack.Get(0).ItemId);
            Assert.AreEqual(1, pack.Count("tool.hoe"), "nothing is lost");
        }

        [Test]
        public void AStackedItem_WithNoFreeHomeSlot_StillComesBack_AndAHomeIsNotKeptForever()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            ChestTransfer.Move(pack, 0, chest, true);
            ChestTransfer.Move(chest, 0, pack, false);
            Assert.AreEqual(0, pack.Get(0).Home, "back in the pack it has no memory of a home");
            ChestTransfer.Move(pack, 0, chest, true);
            ChestTransfer.Move(pack, 1, chest, true);
            for (var i = 0; i < 2; i++) ChestTransfer.Move(chest, i, pack, false);
            Assert.AreEqual(1, pack.Count("tool.hoe"));
            Assert.AreEqual(1, pack.Count("tool.can"));
        }

        [Test]
        public void TheHome_IsSavedWithTheChest()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            ChestTransfer.Move(pack, 1, chest, true);                  // the can goes in first and the hoe second ...
            ChestTransfer.Move(pack, 0, chest, true);
            var loaded = Inventory.FromData(chest.ToData(), MaxStack);
            ChestTransfer.Move(loaded, 0, pack, false);                 // ... so the can comes out first, into a pack with slots 0 and 1 free
            Assert.AreEqual("tool.can", pack.Get(1).ItemId, "still remembered after a save and load");
        }

        // Playtests 2026-10-09: "forced to transfer full stacks", and stacks could not be put where the player wanted them.
        [Test]
        public void MovingPartOfAStack_LeavesTheRestWhereItWas()
        {
            var pack = new Inventory(24, MaxStack);
            pack.Add("seed.parsnip", 8);
            var chest = new Inventory(36, MaxStack);
            Assert.AreEqual(0, ChestTransfer.Move(pack, 0, chest, true, 1));
            Assert.AreEqual(7, pack.Get(0).Count);
            Assert.AreEqual(1, chest.Count("seed.parsnip"));
            ChestTransfer.Move(pack, 0, chest, true, ChestTransfer.Half(pack.Get(0).Count));
            Assert.AreEqual(3, pack.Get(0).Count, "half of seven, rounded up, went");
            Assert.AreEqual(5, chest.Count("seed.parsnip"));
            Assert.AreEqual(8, pack.Count("seed.parsnip") + chest.Count("seed.parsnip"), "nothing is lost");
        }

        [Test]
        public void Half_RoundsUp_AndALoneItemIsMoved()
        {
            Assert.AreEqual(1, ChestTransfer.Half(1));
            Assert.AreEqual(1, ChestTransfer.Half(2));
            Assert.AreEqual(5, ChestTransfer.Half(9));
        }

        [Test]
        public void Place_IntoAnEmptySlot_PutsTheStackExactlyThere()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            Assert.IsTrue(ChestTransfer.Place(pack, 1, chest, 20, true));
            Assert.IsNull(pack.Get(1));
            Assert.AreEqual("tool.can", chest.Get(20).ItemId);
            Assert.IsTrue(ChestTransfer.Place(chest, 20, pack, 7, false));
            Assert.AreEqual("tool.can", pack.Get(7).ItemId, "the player chose slot 7, not the old home");
        }

        [Test]
        public void Place_OnTheSameKind_MergesAsFarAsThereIsRoom()
        {
            var pack = new Inventory(24, MaxStack);
            pack.Add("seed.parsnip", 8);
            var chest = new Inventory(36, MaxStack);
            chest.Add("seed.parsnip", 5, preferredSlot: 3);
            Assert.IsTrue(ChestTransfer.Place(pack, 0, chest, 3, true));
            Assert.AreEqual(10, chest.Get(3).Count, "the chest stack is full (ten at most)");
            Assert.AreEqual(3, pack.Get(0).Count, "the rest stays");
        }

        [Test]
        public void Place_OnADifferentStack_SwapsThem_AndInsideOneInventoryItMoves()
        {
            var pack = Pack();
            var chest = new Inventory(36, MaxStack);
            chest.Add("seed.parsnip", 4, preferredSlot: 5);
            Assert.IsTrue(ChestTransfer.Place(pack, 0, chest, 5, true));
            Assert.AreEqual("tool.hoe", chest.Get(5).ItemId);
            Assert.AreEqual("seed.parsnip", pack.Get(0).ItemId);
            Assert.AreEqual(1, chest.Get(5).Home, "the hoe remembers the slot it left");
            Assert.IsTrue(ChestTransfer.Place(pack, 0, pack, 9, false));
            Assert.IsNull(pack.Get(0));
            Assert.AreEqual("seed.parsnip", pack.Get(9).ItemId);
            Assert.IsFalse(ChestTransfer.Place(pack, 9, pack, 9, false));
        }
    }
}
