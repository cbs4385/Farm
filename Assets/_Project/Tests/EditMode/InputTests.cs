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
                Assert.IsTrue(paths.Any(p => p.StartsWith("<Gamepad>")), $"{action.name}: no gamepad binding");
            }
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
