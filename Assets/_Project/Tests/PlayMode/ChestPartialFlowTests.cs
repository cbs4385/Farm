using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtests 2026-10-09: "forced to transfer full stacks, cannot move one at a time" and "tools stuck to the last hotbar spot". In the real chest screen: a
    // right-click moves one item, Shift-click half, and a stack dragged onto a slot lands exactly there.
    public class ChestPartialFlowTests : InputTestFixture
    {
        const int ChestX = 28, ChestY = 8;

        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-chestpart-" + Guid.NewGuid().ToString("N"));
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

        static GameObject Find(string name) =>
            UnityEngine.Object.FindObjectsByType<Button>().FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == name)?.gameObject;

        static PointerEventData Pointer(PointerEventData.InputButton button) =>
            new PointerEventData(EventSystem.current) { button = button, position = new Vector2(100f, 100f) };

        // Playtest 2026-10-10: "right click still does nothing in the chest page". The events were sent by hand in the other test; here the real mouse is used, which
        // only works when the UI module has a right-click action.
        [UnityTest]
        public IEnumerator ARealRightClickWithTheMouse_MovesOneItem()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
            var objects = _session.GetObjects(MapIds.Farm);
            var placed = objects.Place(_session.Db.AllPlaceables.First(p => p.Id == "chest"), ChestX, ChestY, "mousechest");
            var chest = objects.ChestOf(placed);
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++)
                if (_session.Backpack.Get(slot) != null) _session.Backpack.RemoveFromSlot(slot, _session.Backpack.Get(slot).Count);
            _session.Backpack.Add(ItemIds.Stone, 8, preferredSlot: 5);
            ServiceLocator.Get<UiService>().ShowChest(placed.Id);
            yield return null;
            yield return null;

            var mouse = InputSystem.AddDevice<Mouse>();
            var slotRect = Find("PackSlot5").GetComponent<RectTransform>();
            Set(mouse.position, (Vector2)RectTransformUtility.WorldToScreenPoint(null, slotRect.position));
            yield return null;
            yield return null;
            Press(mouse.rightButton);
            yield return null;
            Release(mouse.rightButton);
            yield return null;
            yield return null;
            Assert.AreEqual(7, _session.Backpack.Get(5).Count, "a real right-click moved one");
            Assert.AreEqual(1, chest.Count(ItemIds.Stone));
        }

        [UnityTest]
        public IEnumerator RightClickMovesOne_ShiftClickHalf_AndADragPlacesTheStack()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var objects = _session.GetObjects(MapIds.Farm);
            var def = _session.Db.AllPlaceables.First(p => p.Id == "chest");
            var placed = objects.Place(def, ChestX, ChestY, "testchest");
            var chest = objects.ChestOf(placed);
            var ui = ServiceLocator.Get<UiService>();
            for (var slot = 0; slot < _session.Backpack.Capacity; slot++)
                if (_session.Backpack.Get(slot) != null) _session.Backpack.RemoveFromSlot(slot, _session.Backpack.Get(slot).Count);
            _session.Backpack.Add(ItemIds.Stone, 8, preferredSlot: 5);
            ui.ShowChest(placed.Id);
            yield return null;
            Assert.IsTrue(ui.AnyModalOpen);

            ExecuteEvents.Execute(Find("PackSlot5"), Pointer(PointerEventData.InputButton.Right), ExecuteEvents.pointerClickHandler);
            yield return null;
            Assert.AreEqual(7, _session.Backpack.Get(5).Count, "a right-click moved one");
            Assert.AreEqual(1, chest.Count(ItemIds.Stone));

            var shift = _keyboard.shiftKey;
            Press(shift);
            yield return null;
            Find("PackSlot5").GetComponent<Button>().onClick.Invoke();
            Release(shift);
            yield return null;
            Assert.AreEqual(3, _session.Backpack.Get(5).Count, "Shift-click moved half of seven, rounded up");
            Assert.AreEqual(5, chest.Count(ItemIds.Stone));

            var drag = Pointer(PointerEventData.InputButton.Left);
            ExecuteEvents.Execute(Find("PackSlot5"), drag, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(Find("PackSlot9"), drag, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(Find("PackSlot5"), drag, ExecuteEvents.endDragHandler);
            yield return null;
            Assert.IsNull(_session.Backpack.Get(5));
            Assert.AreEqual(ItemIds.Stone, _session.Backpack.Get(9).ItemId, "dropped exactly on slot 9");
            Assert.AreEqual(3, _session.Backpack.Get(9).Count);

            drag = Pointer(PointerEventData.InputButton.Left);
            ExecuteEvents.Execute(Find("PackSlot9"), drag, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(Find("ChestSlot30"), drag, ExecuteEvents.dropHandler);
            ExecuteEvents.Execute(Find("PackSlot9"), drag, ExecuteEvents.endDragHandler);
            yield return null;
            Assert.IsNull(_session.Backpack.Get(9));
            Assert.AreEqual(3, chest.Get(30).Count, "dragged into the chest slot of choice");
        }
    }
}
