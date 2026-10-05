using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest report (2026-10-05): after putting the tools into a chest and taking them out again the hoe could not be used. The scenario,
    // driven with a simulated keyboard and real clicks on the chest screen: everything goes into a chest; every backpack slot but the "="
    // slot (the last of the item bar) is filled with rocks; then each tool in turn is taken out of the chest into the only free slot (never
    // its original one), picked with its key, and used successfully; then it goes back into the chest.
    public class ChestToolsFlowTests : InputTestFixture
    {
        const int EqualsSlot = 11;                                   // the item bar's twelfth slot, key "="
        const int ChestX = 28, ChestY = 8;

        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-chesttools-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static FarmMapView View => UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
        static UiService Ui => ServiceLocator.Get<UiService>();

        IEnumerator Tap(UnityEngine.InputSystem.Controls.ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
            yield return null;
        }

        IEnumerator Stand(int x, int y, Vector2Int facing)
        {
            Player.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);
            Player.Face(facing);
            for (var i = 0; i < 4; i++) yield return null;
        }

        static Button Find(string name) =>
            UnityEngine.Object.FindObjectsByType<Button>().FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == name);

        Inventory Chest => _session.GetObjects(MapIds.Farm).ChestOf(_session.GetObjects(MapIds.Farm).At(ChestX, ChestY));

        IEnumerator OpenChest()
        {
            yield return Stand(ChestX - 1, ChestY, Vector2Int.right);
            yield return Tap(_keyboard.eKey);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the chest screen opens");
        }

        IEnumerator CloseChest()
        {
            yield return Tap(_keyboard.escapeKey);
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        IEnumerator Click(string button)
        {
            var b = Find(button);
            Assert.IsNotNull(b, button + " is on the chest screen");
            b.onClick.Invoke();
            yield return null;
        }

        int SlotOf(Inventory inv, string itemId) => Enumerable.Range(0, inv.Capacity).First(i => inv.Get(i)?.ItemId == itemId);

        [UnityTest]
        public IEnumerator EveryTool_TakenFromTheChestIntoTheOnlyFreeSlot_CanBeUsed()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var nodes = _session.GetNodes(MapIds.Farm);                  // a clear patch to work in
            for (var dx = -4; dx <= 5; dx++)
                for (var dy = -4; dy <= 4; dy++) nodes.Remove(30 + dx, 6 + dy);
            View.RefreshAll();
            yield return null;

            var tools = new[] { ItemIds.Hoe, ItemIds.WateringCan, ItemIds.Axe, ItemIds.Pickaxe, ItemIds.Scythe, ItemIds.Sword };
            var original = tools.ToDictionary(t => t, t => SlotOf(_session.Backpack, t));

            // 1. Put the starter chest down, then move everything in the backpack into it.
            _session.State.SelectedHotbar = SlotOf(_session.Backpack, ItemIds.Machine("chest"));
            yield return Stand(ChestX - 1, ChestY, Vector2Int.right);
            yield return Tap(_keyboard.cKey);
            Assert.IsNotNull(_session.GetObjects(MapIds.Farm).At(ChestX, ChestY), "the chest is placed");
            yield return OpenChest();
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++)
                if (_session.Backpack.Get(slot) != null) yield return Click("PackSlot" + slot);
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++) Assert.IsNull(_session.Backpack.Get(slot), "backpack slot " + slot + " is empty");
            yield return CloseChest();

            // 2. Rocks in every slot but "=".
            _session.Backpack.Add(ItemIds.Stone, 99999);
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++)
            {
                var stack = _session.Backpack.Get(slot);
                Assert.AreEqual(ItemIds.Stone, stack.ItemId);
                _session.Backpack.RemoveFromSlot(slot, stack.Count - 1);            // one rock each: room is left for what a tool gathers
            }
            _session.Backpack.RemoveFromSlot(EqualsSlot, 1);
            _session.Backpack.RemoveFromSlot(EqualsSlot - 1, 1);                    // clearing a weed or tree needs room for its loot (a full pack
            _session.Backpack.Add(ItemIds.Fiber, 1, preferredSlot: EqualsSlot - 1);   // refuses, by design), so one slot holds a fibre stack with room
            Assert.IsNull(_session.Backpack.Get(EqualsSlot), "the = slot is the only free one");

            // 3. Each tool: out of the chest into the = slot, picked with the = key, used, put back.
            foreach (var tool in tools)
            {
                yield return OpenChest();
                yield return Click("ChestSlot" + SlotOf(Chest, tool));
                Assert.AreEqual(tool, _session.Backpack.Get(EqualsSlot)?.ItemId, tool + " lands in the only free slot");
                Assert.AreNotEqual(EqualsSlot, original[tool], "which is not where it started");
                Assert.AreEqual(ItemIds.Stone, _session.Backpack.Get(original[tool])?.ItemId, "its old slot is full of rocks");
                yield return CloseChest();

                yield return Tap(_keyboard.equalsKey);
                Assert.AreEqual(EqualsSlot, _session.State.SelectedHotbar, "the = key picks the slot");

                yield return Use(tool);

                yield return OpenChest();
                yield return Click("PackSlot" + EqualsSlot);
                Assert.IsNull(_session.Backpack.Get(EqualsSlot), "the tool is back in the chest");
                yield return CloseChest();
            }
        }

        // Uses the picked tool on a cell east of the player and checks that it did its job.
        IEnumerator Use(string tool)
        {
            var grid = _session.GetGrid(MapIds.Farm);
            var energy = _session.State.Energy;
            if (tool == ItemIds.Hoe)
            {
                yield return Stand(30, 3, Vector2Int.right);
                yield return Tap(_keyboard.cKey);
                Assert.IsTrue(grid.IsTilled(31, 3), "the hoe tills");
            }
            else if (tool == ItemIds.WateringCan)
            {
                yield return Stand(30, 3, Vector2Int.right);
                yield return Tap(_keyboard.cKey);
                Assert.IsTrue(grid.TryGetTile(31, 3, out var tile) && tile.Watered, "the can waters");
            }
            else if (tool == ItemIds.Axe)
            {
                _session.GetNodes(MapIds.Farm).Add(31, 4, _session.Nodes.Get("tree")); View.RefreshNode(new Vector3Int(31, 4, 0));
                yield return Stand(30, 4, Vector2Int.right);
                _session.GetNodes(MapIds.Farm).TryGet(31, 4, out var tree);
                var hp = tree.Hp;
                yield return Tap(_keyboard.cKey);
                Assert.Less(tree.Hp, hp, "the axe hurts the tree");
            }
            else if (tool == ItemIds.Pickaxe)
            {
                _session.GetNodes(MapIds.Farm).Add(31, 5, _session.Nodes.Get("rock")); View.RefreshNode(new Vector3Int(31, 5, 0));
                yield return Stand(30, 5, Vector2Int.right);
                for (var i = 0; i < 2; i++) yield return Tap(_keyboard.cKey);
                Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 5), "the pickaxe breaks the rock");
            }
            else if (tool == ItemIds.Scythe)
            {
                _session.GetNodes(MapIds.Farm).Add(31, 6, _session.Nodes.Get("weed")); View.RefreshNode(new Vector3Int(31, 6, 0));
                yield return Stand(30, 6, Vector2Int.right);
                yield return Tap(_keyboard.cKey);
                Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6), "the scythe cuts the weed; toasts: " + string.Join(" | ", _toasts));
            }
            else if (tool == ItemIds.Sword)
            {
                yield return Stand(30, 7, Vector2Int.right);
                yield return Tap(_keyboard.cKey);
                Assert.Less(_session.State.Energy, energy, "the sword swings");
            }
        }
    }
}
