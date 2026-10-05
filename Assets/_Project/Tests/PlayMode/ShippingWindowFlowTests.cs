using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
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
    // Playtest feedback in the real game: the shipping bin is a window. Pick stacks, set how many, ship them as one lot; Escape leaves with
    // nothing sold.
    public class ShippingWindowFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        UiService _ui;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-ship-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Enter()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _ui = ServiceLocator.Get<UiService>();
        }

        int SlotOf(string itemId)
        {
            for (var i = 0; i < _session.Backpack.Capacity; i++) if (_session.Backpack.Get(i)?.ItemId == itemId) return i;
            return -1;
        }

        // Looks only inside the shipping window: the HUD hotbar has objects called Slot0 to Slot11 too.
        void Click(string objectName)
        {
            var button = _ui.Shipping.Root.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == objectName);
            Assert.IsNotNull(button, objectName + " is in the shipping window");
            button.onClick.Invoke();
        }

        void OpenTheBin()
        {
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            UnityEngine.Object.FindAnyObjectByType<ShippingBin>().Interact(actions);
        }

        [UnityTest]
        public IEnumerator TheBin_OpensAWindow_AndShipsTheLotInOneGo()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 10);
            _session.Backpack.Add("crop.potato", 5);
            var parsnips = SlotOf("crop.parsnip");
            var potatoes = SlotOf("crop.potato");

            OpenTheBin();
            yield return null;
            Assert.IsTrue(_ui.AnyModalOpen, "the bin opens a window");
            Assert.IsNotNull(_ui.Shipping);

            Click("Slot" + parsnips);                       // picking a stack puts one in the lot
            yield return null;
            Click("Qty_+10");                               // and +10 takes it to the whole stack
            yield return null;
            Click("Slot" + potatoes);
            yield return null;
            Click("Qty_" + L.Get("shipping.all"));
            yield return null;
            Click("Qty_-1");
            yield return null;

            var lot = _ui.Shipping.Lot;
            Assert.AreEqual(10 + 4, lot.ItemCount, "10 parsnips and 4 of the 5 potatoes");
            Assert.AreEqual(_session.Backpack.Count("crop.parsnip"), 10, "nothing has left the pack yet");

            Click("ShipLot");
            yield return null;
            Assert.IsFalse(_ui.AnyModalOpen, "the window closes");
            Assert.AreEqual(0, _session.Backpack.Count("crop.parsnip"));
            Assert.AreEqual(1, _session.Backpack.Count("crop.potato"), "the one potato held back is still in the pack");
            Assert.AreEqual(10, _session.State.ShippingBin.Where(x => x.ItemId == "crop.parsnip").Sum(x => x.Count));
            Assert.AreEqual(4, _session.State.ShippingBin.Where(x => x.ItemId == "crop.potato").Sum(x => x.Count));
        }

        [UnityTest]
        public IEnumerator Escape_LeavesTheWindow_WithNothingSold()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 6);
            OpenTheBin();
            yield return null;
            Click("Slot" + SlotOf("crop.parsnip"));
            yield return null;
            Assert.AreEqual(1, _ui.Shipping.Lot.ItemCount);
            Press(_keyboard[Key.Escape]);
            yield return null;
            Release(_keyboard[Key.Escape]);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(_ui.AnyModalOpen);
            Assert.AreEqual(6, _session.Backpack.Count("crop.parsnip"));
            Assert.IsEmpty(_session.State.ShippingBin);
        }
    }
}
