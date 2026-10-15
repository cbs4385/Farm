using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = System.Random;

namespace Farm.Tests
{
    // Playtest 2026-10-15: "my pickaxe is far from my sword", "I had to go into the inventory menu to move things around", and a pad player who could not select an item
    // and move it on the main screen. The rules of rearranging the item bar, its keys, and the pad's way across a grid of slots.
    public class HotbarRearrangeTests
    {
        static int MaxStack(string id) => id.StartsWith("tool.") ? 1 : 20;

        static Inventory Pack(int capacity = 24)
        {
            var pack = new Inventory(capacity, MaxStack);
            pack.Add("tool.hoe", 1); pack.Add("tool.axe", 1); pack.Add("tool.pickaxe", 1); pack.Add("tool.sword", 1);
            pack.Add("seed.parsnip", 15); pack.Add("crop.parsnip", 7);
            return pack;
        }

        static Dictionary<string, int> Totals(Inventory inv)
        {
            var totals = new Dictionary<string, int>();
            for (var i = 0; i < inv.Capacity; i++)
            {
                var stack = inv.Get(i);
                if (stack == null) continue;
                var key = stack.ItemId + "/" + stack.Quality + "/" + stack.Mark;
                totals[key] = (totals.TryGetValue(key, out var n) ? n : 0) + stack.Count;
            }
            return totals;
        }

        // ---- the move ----------------------------------------------------------------------------------------------

        [Test]
        public void Dropping_OnAnEmptySlot_MovesTheStack_AndOnAFullOne_SwapsThem()
        {
            var pack = Pack();
            var rearrange = new HotbarRearrange();
            Assert.IsTrue(rearrange.Begin(pack, 2));
            rearrange.Goto(pack, 9);
            Assert.AreEqual(9, rearrange.Drop(pack));
            Assert.AreEqual("tool.pickaxe", pack.Get(9).ItemId);
            Assert.IsNull(pack.Get(2));
            Assert.IsFalse(rearrange.Active);

            rearrange.Begin(pack, 9);
            rearrange.Goto(pack, 3);
            rearrange.Drop(pack);
            Assert.AreEqual("tool.pickaxe", pack.Get(3).ItemId, "swapped with the sword");
            Assert.AreEqual("tool.sword", pack.Get(9).ItemId);
        }

        [Test]
        public void Dropping_OnTheSameKind_JoinsTheStacks()
        {
            var pack = new Inventory(12, MaxStack);
            pack.Put(0, new ItemStack("seed.parsnip", 5));
            pack.Put(4, new ItemStack("seed.parsnip", 6));
            var rearrange = new HotbarRearrange();
            rearrange.Begin(pack, 0);
            rearrange.Goto(pack, 4);
            rearrange.Drop(pack);
            Assert.IsNull(pack.Get(0));
            Assert.AreEqual(11, pack.Get(4).Count);
        }

        [Test]
        public void AnEmptySlot_CannotBePickedUp_AndCancelPutsNothingDown()
        {
            var pack = Pack();
            var rearrange = new HotbarRearrange();
            Assert.IsFalse(rearrange.Begin(pack, 11), "nothing there");
            Assert.IsFalse(rearrange.Active);
            Assert.IsTrue(rearrange.Begin(pack, 1));
            rearrange.Step(pack, 3);
            Assert.AreEqual(1, rearrange.Cancel(), "the selection goes back to the slot that was picked");
            Assert.AreEqual("tool.axe", pack.Get(1).ItemId);
            Assert.IsFalse(rearrange.Active);
        }

        [Test]
        public void TheCursor_StepsRoundTheRow_ChangesRows_AndTakesANumber()
        {
            var pack = Pack(24);
            var rearrange = new HotbarRearrange();
            rearrange.Begin(pack, 0);
            Assert.AreEqual(11, rearrange.Step(pack, -1), "round to the end of the row");
            Assert.AreEqual(0, rearrange.Step(pack, 1));
            Assert.AreEqual(12, rearrange.ChangeRow(pack, 1), "the same place in the next row");
            Assert.AreEqual(12 + 4, rearrange.PickColumn(pack, 4), "a number key picks a place of the row the bar shows");
            Assert.AreEqual(4, rearrange.ChangeRow(pack, 1), "and round to the first row");
        }

        [Test]
        public void TheSelectedSlot_FollowsItsItem_WhenDraggedAcrossTheBar()
        {
            var pack = Pack();
            Assert.AreEqual(8, HotbarRearrange.Move(pack, 3, 8, 3), "the sword was selected and went to slot 8");
            Assert.AreEqual(5, HotbarRearrange.Move(pack, 0, 9, 5), "another slot selected: it stays");
            Assert.AreEqual(2, HotbarRearrange.Move(pack, 7, 7, 2), "dropped where it was: nothing happens");
            Assert.AreEqual(2, HotbarRearrange.Move(pack, 10, 11, 2), "an empty slot cannot be dragged");
        }

        [Test]
        public void ManyRandomMoves_NeverMakeOrLoseAnItem()
        {
            var random = new Random(20261015);
            var pack = Pack(36);
            var before = Totals(pack);
            var rearrange = new HotbarRearrange();
            var selected = 0;
            for (var n = 0; n < 4000; n++)
            {
                switch (random.Next(4))
                {
                    case 0: selected = HotbarRearrange.Move(pack, random.Next(36), random.Next(36), selected); break;
                    case 1: pack.Move(random.Next(36), random.Next(36)); break;
                    case 2:
                        if (rearrange.Begin(pack, random.Next(36)))
                        {
                            for (var s = random.Next(6); s > 0; s--) { rearrange.Step(pack, random.Next(-3, 4)); if (random.Next(3) == 0) rearrange.ChangeRow(pack, 1); }
                            if (random.Next(5) == 0) rearrange.Cancel(); else rearrange.Drop(pack);
                        }
                        break;
                    default: rearrange.Goto(pack, random.Next(36)); rearrange.PickColumn(pack, random.Next(12)); break;
                }
                Assert.IsTrue(Totals(pack).OrderBy(p => p.Key).SequenceEqual(before.OrderBy(p => p.Key)), "move " + n);
                Assert.That(Enumerable.Range(0, 36).All(i => pack.Get(i) == null || pack.Get(i).Count > 0), "no empty stack is left in a slot");
            }
        }

        // ---- the keys ----------------------------------------------------------------------------------------------

        [Test]
        public void Rearrange_HasAKey_APadButton_AnOptionsRow_AndPrompts()
        {
            var asset = Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            var gameplay = asset.FindActionMap(InputNames.GameplayMap, true);
            var action = gameplay.FindAction(InputNames.Rearrange);
            Assert.IsNotNull(action, "the input asset has been regenerated (InputAssetGenerator)");
            var paths = action.bindings.Select(b => b.path).ToList();
            CollectionAssert.Contains(paths, "<Keyboard>/v");
            CollectionAssert.Contains(paths, "<Gamepad>/leftStickPress");
            CollectionAssert.Contains(InputNames.Rebindable, InputNames.Rearrange);
            CollectionAssert.Contains(InputNames.PadRebindable, InputNames.Rearrange);
            foreach (var key in new[] { "action.Rearrange", "prompt.kb.Rearrange", "prompt.pad.Rearrange", "hud.rearrange.hint", "help.rearrange", "help.rearrange.title", "help.control.rearrange", "help.control.rearrange.title", "toast.rearrange_empty" })
                Assert.IsTrue(L.Has(key), key);
            var others = gameplay.actions.Where(a => a.name != InputNames.Move && a.name != InputNames.Rearrange).SelectMany(a => a.bindings.Select(b => b.path)).ToList();
            Assert.IsFalse(others.Contains("<Keyboard>/v") || others.Contains("<Gamepad>/leftStickPress"), "no other action shares its buttons");
            Assert.IsTrue(HelpPage.Topics.Contains("help.rearrange"), "a Help topic");
            Assert.IsTrue(HelpPage.Controls.Contains("help.control.rearrange"), "a line in the Help controls");
        }

        // ---- the pad's way across a grid of slots ------------------------------------------------------------------

        [TestCase(12), TestCase(24), TestCase(30), TestCase(36)]
        public void EveryStep_OfTheGrid_LandsOnTheSlotNextToIt_OrStays(int count)
        {
            const int columns = 12;
            for (var i = 0; i < count; i++)
            {
                var row = i / columns;
                var column = i % columns;
                var right = SlotGridNav.Next(i, count, columns, SlotGridNav.Direction.Right);
                var left = SlotGridNav.Next(i, count, columns, SlotGridNav.Direction.Left);
                var up = SlotGridNav.Next(i, count, columns, SlotGridNav.Direction.Up);
                var down = SlotGridNav.Next(i, count, columns, SlotGridNav.Direction.Down);
                Assert.AreEqual(column < columns - 1 && i + 1 < count ? i + 1 : -1, right, $"right of {i}");
                Assert.AreEqual(column > 0 ? i - 1 : -1, left, $"left of {i}");
                Assert.AreEqual(row > 0 ? i - columns : -1, up, $"up from {i}");
                if (i + columns < count) Assert.AreEqual(i + columns, down, $"down from {i}");
                else if (row < (count - 1) / columns) Assert.AreEqual(count - 1, down, $"down from {i} onto a short last row");
                else Assert.AreEqual(-1, down, $"down from the last row ({i})");
                foreach (var next in new[] { right, left, up, down }) Assert.Less(next, count);
            }
        }

        [TestCase(12), TestCase(24), TestCase(36)]
        public void WalkingTheGrid_WithTheLinkedButtons_VisitsEverySlotInOrder_AndNeverLeavesIt(int count)
        {
            var root = new GameObject("Grid", typeof(RectTransform));
            try
            {
                var slots = new List<Selectable>();
                for (var i = 0; i < count; i++)
                {
                    var go = new GameObject("Slot" + i, typeof(RectTransform), typeof(Image), typeof(Button));
                    go.transform.SetParent(root.transform, false);
                    slots.Add(go.GetComponent<Button>());
                }
                var below = new GameObject("Below", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
                below.transform.SetParent(root.transform, false);
                SlotGridNav.Link(slots, 12, below);

                // along every row, left to right and back, then down the first column
                var at = 0;
                var visited = new List<int> { 0 };
                for (var row = 0; row * 12 < count; row++)
                {
                    var last = Mathf.Min(12, count - row * 12) - 1;
                    for (var c = 0; c < last; c++) { at = slots.IndexOf(slots[at].FindSelectableOnRight()); visited.Add(at); }
                    Assert.IsNull(slots[at].FindSelectableOnRight(), "no wrapping at the end of a row");
                    for (var c = 0; c < last; c++) at = slots.IndexOf(slots[at].FindSelectableOnLeft());
                    Assert.AreEqual(row * 12, at);
                    if ((row + 1) * 12 < count) { at = slots.IndexOf(slots[at].FindSelectableOnDown()); Assert.AreEqual((row + 1) * 12, at); visited.Add(at); }
                }
                CollectionAssert.AreEqual(Enumerable.Range(0, count).ToList(), visited.Distinct().OrderBy(v => v).ToList(), "every slot is reached");
                var lastRow = (count - 1) / 12 * 12;
                Assert.AreSame(below, slots[lastRow].FindSelectableOnDown(), "down from the last row goes to the button under the grid");
                Assert.IsNull(slots[0].FindSelectableOnUp());
                Assert.IsNull(slots[0].FindSelectableOnLeft());
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
