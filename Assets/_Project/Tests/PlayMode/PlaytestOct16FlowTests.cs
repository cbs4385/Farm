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
    // Playtest 2026-10-16 (controller): a borrowed book was handed back by a stray button press, and the pad cursor in the shipping window jumped about.
    public class PlaytestOct16FlowTests : InputTestFixture
    {
        string _dataRoot;
        GameSession _session;
        UiService _ui;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-oct16-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            InputSystem.AddDevice<Keyboard>();
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
            _ui = (UiService)GameContext.Current.Ui;
        }

        [UnityTest]
        public IEnumerator HandingABookBack_AsksFirst_AndNoKeepsIt()
        {
            yield return Enter();
            var book = LibraryDesk.Books[0];
            _session.SetFlag(book.Flag);
            _session.Backpack.Add(book.ItemId, 1);
            var desk = new GameObject("desk").AddComponent<LibraryDesk>();
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 12 * 60));
            if (!BusinessHoursRegistry.IsOpen("library", _session.Clock.Now)) Assert.Ignore("the library keeps hours the test clock cannot reach");
            desk.Interact(UnityEngine.Object.FindAnyObjectByType<PlayerActions>());
            yield return null;
            Assert.IsTrue(_ui.AnyModalOpen, "a question is asked");
            Assert.AreEqual(1, _session.Backpack.Count(book.ItemId), "nothing has left the pack yet");
            Assert.IsFalse(_session.HasFlag(LibraryDesk.ReturnedFlag(book)));

            _ui.Confirm.Root.GetComponentsInChildren<Button>().First(b => b.name == "No").onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, _session.Backpack.Count(book.ItemId), "No keeps the book");

            desk.Interact(UnityEngine.Object.FindAnyObjectByType<PlayerActions>());
            yield return null;
            _ui.Confirm.Root.GetComponentsInChildren<Button>().First(b => b.name == "Yes").onClick.Invoke();
            yield return null;
            Assert.AreEqual(0, _session.Backpack.Count(book.ItemId), "Yes hands it over");
            Assert.IsTrue(_session.HasFlag(LibraryDesk.ReturnedFlag(book)));
        }

        [UnityTest]
        public IEnumerator TheShippingGrid_KeepsEverySlotInTheCursorsPath_SoItNeverJumps()
        {
            yield return Enter();
            _session.Backpack.Add("crop.parsnip", 3);        // among tools and seeds that cannot be shipped
            UnityEngine.Object.FindAnyObjectByType<ShippingBin>().Interact(UnityEngine.Object.FindAnyObjectByType<PlayerActions>());
            yield return null;
            var slots = _ui.Shipping.Root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Slot")).OrderBy(b => b.transform.GetSiblingIndex()).ToList();
            Assert.GreaterOrEqual(slots.Count, 12);
            Assert.IsTrue(slots.All(b => b.interactable), "a slot that cannot be shipped is still a stop for the cursor");
            for (var i = 0; i + 1 < slots.Count; i++)
                Assert.AreSame(slots[i + 1], slots[i].navigation.selectOnRight, "right from slot " + i + " is the next slot");
            for (var i = 12; i < slots.Count; i++)
                Assert.AreSame(slots[i - 12], slots[i].navigation.selectOnUp, "up from slot " + i + " is the slot above");
            Assert.IsNotNull(slots[slots.Count - 1].navigation.selectOnDown, "the last row leads on to the quantity buttons");

            var unshippable = slots.First(b => _session.Backpack.Get(b.transform.GetSiblingIndex()) != null && !ShippingLot.CanShip(_session, b.transform.GetSiblingIndex()));
            unshippable.onClick.Invoke();
            Assert.IsTrue(_ui.Shipping.Lot.IsEmpty, "clicking one that cannot be shipped puts nothing in the lot");
            Assert.AreEqual(-1, _ui.Shipping.Selected);
            _ui.Shipping.Root.GetComponentsInChildren<Button>().First(b => b.name == "Close").onClick.Invoke();
        }

        [UnityTest]
        public IEnumerator ABookInHand_IsRead_AtTheFastestSpeed_UntilDone_OrTheFarmerMoves()
        {
            yield return Enter();
            var book = LibraryDesk.Books[0];
            _session.Backpack.Add(book.ItemId, 1);
            for (var i = 0; i < _session.Backpack.Capacity; i++) if (_session.Backpack.Get(i)?.ItemId == book.ItemId) _session.State.SelectedHotbar = i;
            var actions = UnityEngine.Object.FindAnyObjectByType<PlayerActions>();
            actions.UseSelected();
            yield return null;
            Assert.IsTrue(actions.Controller.Reading, "the farmer sits with the book");
            Assert.AreEqual(BookReading.FastSpeed, _session.Clock.SpeedMultiplier);
            Assert.IsNotNull(actions.Controller.GetComponentInChildren<ReadingBook>(), "the book is drawn in their hands");

            actions.Controller.StopReading();
            yield return null;
            Assert.IsFalse(actions.Controller.Reading);
            Assert.AreEqual(1f, _session.Clock.SpeedMultiplier);
            Assert.IsNull(actions.Controller.GetComponentInChildren<ReadingBook>());
            Assert.IsFalse(BookReading.HasRead(_session, book));

            actions.UseSelected();
            yield return null;
            for (var i = 0; i < 4; i++) _session.Clock.AdvanceMinutes(GameClock.MinutesPerStep);
            yield return null;
            Assert.IsFalse(actions.Controller.Reading, "thirty minutes went by");
            Assert.IsTrue(BookReading.HasRead(_session, book));
            Assert.AreEqual(1f, _session.Clock.SpeedMultiplier);
        }
    }
}
