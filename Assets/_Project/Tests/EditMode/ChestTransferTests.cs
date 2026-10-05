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
    }
}
