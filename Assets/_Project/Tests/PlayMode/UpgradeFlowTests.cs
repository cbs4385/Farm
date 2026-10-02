using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-032 part B in the real game: the blacksmith, general store and clinic counters, used with a simulated
    // keyboard, and the upgrade screen's buttons.
    public class UpgradeFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-upgradetests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _toasts.Clear();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        // Starts in a shop on `day` (1 = Monday) at 10:00, with plenty of gold.
        IEnumerator Start(string map, int day)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, day, 10 * 60));
            _session.State.SetDate(_session.Clock.Now);
            _session.State.Gold = 50000;
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = "default";
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        static IUiService Ui => ServiceLocator.Get<IUiService>();

        IEnumerator UseCounterAt(float x, float y)
        {
            Player.transform.position = new Vector3(x, y, 0f);
            Player.Face(Vector2Int.up);
            for (var i = 0; i < 5; i++) yield return null;
            Press(_keyboard.eKey);
            yield return null;
            Release(_keyboard.eKey);
            for (var i = 0; i < 4; i++) yield return null;
        }

        static Button Find(string row, string text) =>
            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == text && b.transform.parent.name == row);

        static void CloseAll() => UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .First(b => b.gameObject.activeInHierarchy && b.name == "Close").onClick.Invoke();

        // ---- the blacksmith ---------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheBlacksmith_UpgradesAnAxe_OverTwoDays()
        {
            yield return Start(MapIds.Blacksmith, 2);   // Tuesday
            _session.Backpack.Add(ItemIds.CopperBar, 5);

            yield return UseCounterAt(4.5f, 3.5f);
            Assert.IsTrue(Ui.AnyModalOpen, "the upgrade screen opens");
            var buy = Find("tool.axe.copper", "Buy");
            Assert.IsNotNull(buy, "a copper axe is on offer");
            buy.onClick.Invoke();
            yield return null;

            Assert.IsFalse(_session.Backpack.Has(ItemIds.Axe), "the axe is handed in");
            Assert.AreEqual(50000 - 1500, _session.State.Gold);
            Assert.AreEqual(0, _session.Backpack.Count(ItemIds.CopperBar));
            Assert.AreEqual(1, _session.State.PendingUpgrades.Count);
            Assert.IsNull(Find("tool.axe.copper", "Buy"), "it is no longer offered");
            Assert.IsNull(Find("tool.axe.copper", "Collect"), "and not ready yet");

            CloseAll();
            yield return null;

            _session.EndDay(false);
            _session.EndDay(false);
            Assert.AreEqual(4, _session.Clock.Now.Day);

            // The counter is open again on Thursday (the blacksmith's day off is Monday).
            _session.Clock.SetTime(_session.Clock.Now.WithMinuteOfDay(10 * 60));
            yield return UseCounterAt(4.5f, 3.5f);
            Assert.IsTrue(Ui.AnyModalOpen);
            var collect = Find("tool.axe", "Collect");
            Assert.IsNotNull(collect, "the finished axe is waiting");
            collect.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_session.Backpack.Has(ItemIds.Axe));
            Assert.AreEqual(1, _session.ToolTier(ItemIds.Axe));
            Assert.AreEqual("Copper Axe", _session.ToolTitle(ItemIds.Axe, _session.ToolTier(ItemIds.Axe)));
            Assert.IsEmpty(_session.State.PendingUpgrades);
            Assert.IsNotNull(Find("tool.axe.iron", "Buy"), "the next tier is now on offer");
        }

        [UnityTest]
        public IEnumerator WithoutTheBars_TheBlacksmithRefuses()
        {
            yield return Start(MapIds.Blacksmith, 2);
            yield return UseCounterAt(4.5f, 3.5f);
            Find("tool.axe.copper", "Buy").onClick.Invoke();
            yield return null;
            CollectionAssert.Contains(_toasts, "You do not have the materials for that.");
            Assert.IsTrue(_session.Backpack.Has(ItemIds.Axe));
            Assert.AreEqual(50000, _session.State.Gold);
        }

        [UnityTest]
        public IEnumerator TheBlacksmithCounter_IsClosedOnMondays()
        {
            yield return Start(MapIds.Blacksmith, 1);   // Monday: the day off
            yield return UseCounterAt(4.5f, 3.5f);
            Assert.IsFalse(Ui.AnyModalOpen);
            Assert.IsTrue(_toasts.Any(t => t.StartsWith("The Blacksmith is closed")));
        }

        // ---- the general store and the clinic ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheGeneralStore_SellsABiggerBackpack()
        {
            yield return Start(MapIds.GeneralStore, 2);
            yield return UseCounterAt(9.5f, 3.5f);
            Assert.IsTrue(Ui.AnyModalOpen);
            Find("backpack.24", "Buy").onClick.Invoke();
            yield return null;
            Assert.AreEqual(24, _session.Backpack.Capacity);
            Assert.AreEqual(48000, _session.State.Gold);
            CloseAll();
            yield return null;

            Ui.ToggleInventory();
            yield return null;
            yield return null;
            var slots = UnityEngine.Object.FindObjectsByType<Farm.UI.InventorySlotDrag>(FindObjectsSortMode.None);
            Assert.AreEqual(24, slots.Count(s => s.gameObject.activeInHierarchy), "the backpack screen shows every slot");
        }

        [UnityTest]
        public IEnumerator TheClinic_SellsMoreEnergy()
        {
            yield return Start(MapIds.Clinic, 2);
            var max = _session.State.MaxEnergy;
            yield return UseCounterAt(7.5f, 2.5f);
            Assert.IsTrue(Ui.AnyModalOpen);
            Find("energy.1", "Buy").onClick.Invoke();
            yield return null;
            Assert.AreEqual(max + 20, _session.State.MaxEnergy);
            Assert.IsNull(Find("energy.1", "Buy"));
            Assert.IsNotNull(Find("energy.2", "Buy"), "the next step is offered");
        }
    }
}
