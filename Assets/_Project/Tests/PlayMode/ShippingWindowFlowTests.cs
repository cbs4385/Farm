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
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            _ui = ((UiService)GameContext.Current.Ui);
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

        // Looks inside the "how many?" window.
        void Click2(string objectName)
        {
            var button = _ui.Quantity.Root.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == objectName);
            Assert.IsNotNull(button, objectName + " is in the quantity window");
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

            Click("Slot" + parsnips);                       // picking a stack opens the "how many?" window
            yield return null;
            Assert.IsTrue(_ui.Quantity.IsOpen);
            Click2("Quantity_+5"); Click2("Quantity_+5");   // +5 twice takes it past the stack: it stops at the whole stack
            Assert.AreEqual(10, _ui.Quantity.Value, "never more than the backpack holds");
            Click2("Ok");
            yield return null;
            Click("Slot" + potatoes);
            yield return null;
            Click2("Quantity_" + L.Get("quantity.plus_all"));
            Click2("Quantity_-1");
            Click2("Ok");
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
            Assert.IsTrue(_ui.Quantity.IsOpen, "a stack asks how many");
            Press(_keyboard[Key.Escape]);               // the first Escape closes the "how many?" window
            yield return null;
            Release(_keyboard[Key.Escape]);
            yield return null;
            Assert.IsTrue(_ui.Shipping.IsOpen, "the shipping window is still there");
            Press(_keyboard[Key.Escape]);
            yield return null;
            Release(_keyboard[Key.Escape]);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(_ui.AnyModalOpen);
            Assert.AreEqual(6, _session.Backpack.Count("crop.parsnip"));
            Assert.IsEmpty(_session.State.ShippingBin);
        }

        // Playtest 2026-10-16: "the whole process of putting things into the shipping container seems overly laborious". A stack opens a window with a number box.
        [UnityTest]
        public IEnumerator TheQuantityWindow_SetsTheAmountInOneGo_AndNeverAsksForMoreThanThePackHolds()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 12);
            var slot = SlotOf("crop.parsnip");
            OpenTheBin();
            yield return null;
            Click("Slot" + slot);
            yield return null;
            Assert.IsTrue(_ui.Quantity.IsOpen);
            Assert.AreEqual(12, _ui.Quantity.Max);
            _ui.Quantity.Set(500);
            Assert.AreEqual(12, _ui.Quantity.Value, "typing too many gives the whole stack");
            _ui.Quantity.Set(-4);
            Assert.AreEqual(0, _ui.Quantity.Value, "and never less than none");
            _ui.Quantity.Set(7);
            Click2("Ok");
            yield return null;
            Assert.IsFalse(_ui.Quantity.IsOpen);
            Assert.AreEqual(7, _ui.Shipping.Lot.Get(slot));
            Click("Close");
        }

        [UnityTest]
        public IEnumerator Selling_ToTheShopkeeper_AsksHowMany_AndPaysForThatMany()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 9);
            var slot = SlotOf("crop.parsnip");
            var gold = _session.State.Gold;
            _ui.ShowSell();
            yield return null;
            _ui.Sell.Root.GetComponentsInChildren<Button>().First(b => b.name == "Slot" + slot).onClick.Invoke();
            yield return null;
            Assert.IsTrue(_ui.Quantity.IsOpen, "a stack asks how many");
            Assert.AreEqual(9, _ui.Quantity.Max);
            _ui.Quantity.Submit(4);
            yield return null;
            Assert.AreEqual(5, _session.Backpack.Count("crop.parsnip"), "four were sold");
            Assert.Greater(_session.State.Gold, gold);
            _ui.Sell.Close();
        }

        [UnityTest]
        public IEnumerator Buying_AsksHowMany_AndStopsAtWhatTheGoldPaysFor()
        {
            yield return Enter();
            var seed = ItemIds.Seed("parsnip");
            _ui.ShowShop("general");
            yield return null;
            _session.State.Gold = 10 * _session.Db.GetItem(seed).BuyPrice + 3;
            var buy = UnityEngine.Object.FindObjectsByType<Button>().First(b => b.gameObject.activeInHierarchy && b.name == "Buy_" + seed);
            buy.onClick.Invoke();
            yield return null;
            Assert.IsTrue(_ui.Quantity.IsOpen);
            Assert.AreEqual(10, _ui.Quantity.Max, "ten is what the gold pays for");
            _ui.Quantity.Set(99);
            Assert.AreEqual(10, _ui.Quantity.Value);
            var before = _session.Backpack.Count(seed);
            _ui.Quantity.Confirm();
            yield return null;
            Assert.AreEqual(before + 10, _session.Backpack.Count(seed));
        }

        // Playtest 2026-10-13: "it looks like you cannot sell more than one type at once". Every stack in the lot has a contour of its own, not only the one being edited, and the
        // total is big and bold.
        [UnityTest]
        public IEnumerator EveryStackInTheLot_HasAContour_AndTheTotalIsBold()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 10);
            _session.Backpack.Add("crop.potato", 5);
            var parsnips = SlotOf("crop.parsnip");
            var potatoes = SlotOf("crop.potato");
            var kale = SlotOf(ItemIds.Seed("parsnip"));          // a stack that stays out of the lot
            OpenTheBin();
            yield return null;

            Click("Slot" + parsnips);
            yield return null;
            _ui.Quantity.Close();
            Click("Slot" + potatoes);
            yield return null;
            _ui.Quantity.Close();
            var root = _ui.Shipping.Root.transform;
            Transform SlotButton(int slot) => root.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "Slot" + slot)?.transform;
            bool Contour(int slot) { var s = SlotButton(slot); Assert.IsNotNull(s, "Slot" + slot); return s.Find("InLot") != null; }
            Assert.IsTrue(Contour(parsnips), "the first kind keeps its contour when another is picked");
            Assert.IsTrue(Contour(potatoes), "the kind being edited has one");
            Assert.IsFalse(Contour(kale), "a stack that is not in the lot has none");
            var outline = SlotButton(parsnips).Find("InLot");
            Assert.AreEqual(4, outline.childCount, "four edges");

            var total = root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true).First(x => x.text.StartsWith("In the lot"));
            Assert.GreaterOrEqual(total.fontSize, 24f, "the total is big");
            Assert.IsTrue((total.fontStyle & TMPro.FontStyles.Bold) != 0, "and bold");
            Click("Close");
        }
    }
}
