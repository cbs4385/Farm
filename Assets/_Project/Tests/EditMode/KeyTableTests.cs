using System.Collections.Generic;
using System.Linq;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Tests
{
    // The Options key table: an action per row, its primary key and its secondary key.
    public class KeyTableTests
    {
        InputService _input;

        [SetUp]
        public void SetUp()
        {
            var asset = Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            Assert.IsNotNull(asset);
            _input = new InputService(asset);
        }

        static string PathAt(KeyTable.Spec row, int index) => row.Action.bindings[index].path;

        [Test]
        public void EveryKeyboardAndMouseBinding_OfBothMaps_IsInExactlyOneCell()
        {
            var cells = new List<(string map, string action, int index)>();
            foreach (var row in KeyTable.Rows(_input))
            {
                cells.Add((row.Action.actionMap.name, row.Action.name, row.Primary));
                if (row.Secondary >= 0) cells.Add((row.Action.actionMap.name, row.Action.name, row.Secondary));
            }
            Assert.AreEqual(cells.Count, cells.Distinct().Count(), "no binding sits in two cells");

            var pointer = new[] { InputNames.Point, InputNames.Click, InputNames.RightClick, InputNames.ScrollWheel };
            foreach (var map in new[] { _input.Gameplay, _input.Ui })
                foreach (var action in map.actions)
                {
                    if (pointer.Contains(action.name)) continue;
                    for (var i = 0; i < action.bindings.Count; i++)
                    {
                        var b = action.bindings[i];
                        if (b.isComposite || b.path == null || !(b.path.StartsWith("<Keyboard>") || b.path.StartsWith("<Mouse>"))) continue;
                        CollectionAssert.Contains(cells, (map.name, action.name, i), $"{map.name}/{action.name}[{i}] {b.path} has no cell");
                    }
                }
        }

        [Test]
        public void TheWalkingKeys_AreFourRows_WithTheLettersFirstAndTheArrowsSecond()
        {
            var walking = KeyTable.Rows(_input).Where(r => r.Group == KeyTable.Group.Walking).ToList();
            Assert.AreEqual(4, walking.Count);
            CollectionAssert.AreEqual(new[] { "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d" }, walking.Select(r => PathAt(r, r.Primary)).ToArray());
            CollectionAssert.AreEqual(new[] { "<Keyboard>/upArrow", "<Keyboard>/downArrow", "<Keyboard>/leftArrow", "<Keyboard>/rightArrow" }, walking.Select(r => PathAt(r, r.Secondary)).ToArray());
        }

        [Test]
        public void TheMenuMovement_IsFourRows_WithTheArrowsFirstAndTheLettersSecond()
        {
            var nav = KeyTable.Rows(_input).Where(r => r.Action.name == InputNames.Navigate).ToList();
            Assert.AreEqual(4, nav.Count);
            CollectionAssert.AreEqual(new[] { "<Keyboard>/upArrow", "<Keyboard>/downArrow", "<Keyboard>/leftArrow", "<Keyboard>/rightArrow" }, nav.Select(r => PathAt(r, r.Primary)).ToArray());
            CollectionAssert.AreEqual(new[] { "<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d" }, nav.Select(r => PathAt(r, r.Secondary)).ToArray());
        }

        [Test]
        public void ItemSlots_AreTwelveRows_WithNoSecondKey()
        {
            var slots = KeyTable.Rows(_input).Where(r => r.Group == KeyTable.Group.ItemSlots).ToList();
            Assert.AreEqual(InputNames.HotbarSlots, slots.Count);
            Assert.IsTrue(slots.All(r => r.Secondary < 0), "a slot has one key: the secondary cell is a dash");
        }

        [Test]
        public void AnActionWithAKeyAndAMouseButton_HasTheKeyFirstAndTheMouseButtonSecond()
        {
            var rows = KeyTable.Rows(_input);
            var interact = rows.First(r => r.Action.name == InputNames.Interact);
            Assert.AreEqual("<Keyboard>/e", PathAt(interact, interact.Primary));
            Assert.AreEqual("<Mouse>/rightButton", PathAt(interact, interact.Secondary));
            var inventory = rows.First(r => r.Action.name == InputNames.Inventory);
            Assert.AreEqual("<Keyboard>/tab", PathAt(inventory, inventory.Primary));
            Assert.AreEqual("<Keyboard>/i", PathAt(inventory, inventory.Secondary));
            var pause = rows.First(r => r.Action.name == InputNames.Pause);
            Assert.AreEqual(-1, pause.Secondary, "Escape has no second key");
        }

        [Test]
        public void RebindingAKey_DoesNotMoveItToAnotherCell()
        {
            var before = KeyTable.Rows(_input).Select(r => (r.Action.name, r.Primary, r.Secondary)).ToList();
            _input.Interact.ApplyBindingOverride(KeyTable.Rows(_input).First(r => r.Action.name == InputNames.Interact).Primary, "<Keyboard>/g");
            _input.Interact.ApplyBindingOverride(KeyTable.Rows(_input).First(r => r.Action.name == InputNames.Interact).Secondary, "<Keyboard>/h");
            var after = KeyTable.Rows(_input).Select(r => (r.Action.name, r.Primary, r.Secondary)).ToList();
            CollectionAssert.AreEqual(before, after);
        }
    }
}
