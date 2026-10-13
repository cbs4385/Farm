using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Tests
{
    public class InputTests
    {
        InputActionAsset _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            Assert.IsNotNull(_asset, "Resources/FarmInput.inputactions is missing; run Farm/Setup/Generate Input Actions");
        }

        [Test]
        public void Asset_HasEveryGameplayAndUiAction()
        {
            var gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            var names = new[]
            {
                InputNames.Move, InputNames.UseTool, InputNames.Interact, InputNames.HotbarNext, InputNames.HotbarPrev,
                InputNames.Inventory, InputNames.Pause,
            };
            foreach (var n in names) Assert.IsNotNull(gameplay.FindAction(n), n);
            for (var i = 1; i <= InputNames.HotbarSlots; i++) Assert.IsNotNull(gameplay.FindAction(InputNames.HotbarPrefix + i), "Hotbar" + i);

            var ui = _asset.FindActionMap(InputNames.UiMap, true);
            foreach (var n in new[] { InputNames.Navigate, InputNames.Submit, InputNames.Cancel, InputNames.Point, InputNames.Click })
                Assert.IsNotNull(ui.FindAction(n), n);
        }

        [Test]
        public void EveryGameplayAction_IsUsableFromKeyboardAndGamepad_ExceptDirectHotbarKeys()
        {
            var gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            foreach (var action in gameplay.actions)
            {
                var paths = action.bindings.Select(b => b.effectivePath).ToList();
                Assert.IsTrue(paths.Any(p => p.StartsWith("<Keyboard>") || p.StartsWith("<Mouse>")), $"{action.name}: no keyboard/mouse binding");
                if (action.name.StartsWith(InputNames.HotbarPrefix) && action.name != InputNames.HotbarNext && action.name != InputNames.HotbarPrev) continue;
                if (action.name == InputNames.RowPrev) continue;                       // a pad has one row button: RT goes round
                Assert.IsTrue(paths.Any(p => p.StartsWith("<Gamepad>")), $"{action.name}: no gamepad binding");
            }
        }

        [Test]
        public void CraftingHasItsOwnKey_C_AndNoOtherKeyIsSharedByTwoGameplayButtons()
        {
            var gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            Assert.AreEqual("<Keyboard>/c", gameplay[InputNames.Crafting].bindings.First().path);
            Assert.IsFalse(gameplay[InputNames.UseTool].bindings.Any(b => b.path == "<Keyboard>/c"), "C was the tool key: the tool has X now, like the pad");
            var keys = gameplay.actions.Where(a => a.name != InputNames.Move).SelectMany(a => a.bindings.Select(b => b.path)).Where(p => p.StartsWith("<Keyboard>")).ToList();
            CollectionAssert.AreEquivalent(keys.Distinct().ToList(), keys, "a key does one thing");
        }

        [Test]
        public void TheOptionsList_CoversEveryKeyboardAction_OfTheGameplayAndMenuMaps()
        {
            var gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            foreach (var action in gameplay.actions)
            {
                if (action.name == InputNames.Move || action.name.StartsWith(InputNames.HotbarPrefix) && action.name != InputNames.HotbarNext && action.name != InputNames.HotbarPrev) continue;
                CollectionAssert.Contains(InputNames.Rebindable, action.name, action.name + " is missing from the key list");
            }
            Assert.AreEqual(InputNames.HotbarSlots, gameplay.actions.Count(a => a.name.StartsWith(InputNames.HotbarPrefix) && a.name != InputNames.HotbarNext && a.name != InputNames.HotbarPrev));
            foreach (var name in InputNames.PadRebindable) Assert.GreaterOrEqual(BindingNames.IndexOf(gameplay[name], BindingNames.Gamepad), 0, name);
            var ui = _asset.FindActionMap(InputNames.UiMap, true);
            foreach (var name in InputNames.UiRebindable) Assert.IsNotNull(ui.FindAction(name), name);
            foreach (var name in InputNames.UiPadRebindable) Assert.GreaterOrEqual(BindingNames.IndexOf(ui[name], BindingNames.Gamepad), 0, name);
        }

        [Test]
        public void ARebound_PadButton_IsNamedInTheHints_AndAFreshOneKeepsItsCuratedText()
        {
            var input = new InputService(_asset);
            Assert.AreEqual("press X", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.Gamepad), "untouched: the curated words");
            var pad = BindingNames.IndexOf(input.UseTool, BindingNames.Gamepad);
            input.UseTool.ApplyBindingOverride(pad, "<Gamepad>/rightTrigger");
            Assert.AreEqual("press RT", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.Gamepad));
            Assert.AreEqual("Press RT", Farm.Core.ControlPrompts.Expand("[[UseToolCap]]", Farm.Core.InputKind.Gamepad));
            Assert.AreEqual("press RT again", Farm.Core.ControlPrompts.Expand("[[UseToolAgain]]", Farm.Core.InputKind.Gamepad));
            Assert.AreEqual("click (or press X)", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.KeyboardMouse), "the keyboard is separate");
            input.UseTool.ApplyBindingOverride(BindingNames.IndexOf(input.UseTool, BindingNames.Keyboard), "<Keyboard>/v");
            Assert.AreEqual("click (or press V)", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.KeyboardMouse));
            input.Interact.ApplyBindingOverride(BindingNames.IndexOf(input.Interact, BindingNames.Gamepad), "<Gamepad>/buttonNorth");
            Assert.AreEqual("Y", Farm.Core.ControlPrompts.Expand("[[Interact]]", Farm.Core.InputKind.Gamepad));
            input.ResetPadBindings();
            Assert.AreEqual("press X", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.Gamepad), "reset: back to the curated words");
            Assert.AreEqual("click (or press V)", Farm.Core.ControlPrompts.Expand("[[UseTool]]", Farm.Core.InputKind.KeyboardMouse), "and the keyboard key stays as it was set");
            Farm.Core.ControlPrompts.ResetForTests();
        }

        [Test]
        public void MovingAKeyOntoAnotherActionsKey_SwapsTheTwo_SoNoKeyDoesTwoThings()
        {
            var input = new InputService(_asset);
            var interact = BindingNames.IndexOf(input.Interact, BindingNames.Keyboard);
            var rotate = BindingNames.IndexOf(input.Rotate, BindingNames.Keyboard);
            var before = input.Interact.bindings[interact].effectivePath;        // E
            var rotateKey = input.Rotate.bindings[rotate].effectivePath;         // R
            input.Interact.ApplyBindingOverride(interact, rotateKey);            // the player gives Interact the turn key
            var swapped = BindingNames.SwapIfClash(input.Interact, interact, before);
            Assert.AreSame(input.Rotate, swapped);
            Assert.AreEqual(rotateKey, input.Interact.bindings[interact].effectivePath);
            Assert.AreEqual(before, input.Rotate.bindings[rotate].effectivePath, "Rotate got the key Interact gave up");

            Assert.IsNull(BindingNames.SwapIfClash(input.Interact, interact, rotateKey), "a key that did not move clashes with nothing");
            input.Interact.ApplyBindingOverride(interact, "<Keyboard>/v");
            Assert.IsNull(BindingNames.SwapIfClash(input.Interact, interact, rotateKey), "a free key clashes with nothing");
        }

        [Test]
        public void ThePadButtons_SwapToo()
        {
            var input = new InputService(_asset);
            var use = BindingNames.IndexOf(input.UseTool, BindingNames.Gamepad);
            var interact = BindingNames.IndexOf(input.Interact, BindingNames.Gamepad);
            var before = input.UseTool.bindings[use].effectivePath;               // X
            input.UseTool.ApplyBindingOverride(use, input.Interact.bindings[interact].effectivePath);        // onto A
            Assert.AreSame(input.Interact, BindingNames.SwapIfClash(input.UseTool, use, before));
            Assert.AreEqual("<Gamepad>/buttonWest", input.Interact.bindings[interact].effectivePath);
        }

        [Test]
        public void Rebinding_RoundTripsThroughOverridesJson()
        {
            var first = new InputService(_asset);
            var interact = first.Interact;
            var index = interact.bindings.ToList().FindIndex(b => b.path == "<Keyboard>/e");
            Assert.GreaterOrEqual(index, 0);

            interact.ApplyBindingOverride(index, "<Keyboard>/f");
            var json = first.ExportOverrides();

            var second = new InputService(_asset);
            Assert.AreEqual("<Keyboard>/e", second.Interact.bindings[index].effectivePath, "overrides must not leak into the shared asset");
            second.ApplyOverrides(json);
            Assert.AreEqual("<Keyboard>/f", second.Interact.bindings[index].effectivePath);

            second.ResetBindings();
            Assert.AreEqual("<Keyboard>/e", second.Interact.bindings[index].effectivePath);
        }

        [Test]
        public void GameplayBlocking_IsRefCounted()
        {
            var input = new InputService(_asset);
            input.EnableGameplay();
            Assert.IsTrue(input.Gameplay.enabled);
            input.BlockGameplay();
            input.BlockGameplay();
            Assert.IsFalse(input.Gameplay.enabled);
            input.UnblockGameplay();
            Assert.IsFalse(input.Gameplay.enabled);
            input.UnblockGameplay();
            Assert.IsTrue(input.Gameplay.enabled);
        }
    }
}
