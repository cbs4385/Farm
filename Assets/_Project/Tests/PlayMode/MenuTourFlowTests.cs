using System;
using System.Collections;
using System.Collections.Generic;
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
    // Owner, 2026-10-09: an introductory step opens the journal and briefly explains each tab. In the real game, a few seconds into a new game the menu opens on the
    // Journal, the caption box walks through the tabs with Enter, and the tour is never offered again.
    public class MenuTourFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-tour-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            MenuTourScreen.ForceInTests = true;
        }

        public override void TearDown()
        {
            MenuTourScreen.ForceInTests = false;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static UiService Ui => ServiceLocator.Get<UiService>();

        IEnumerator Tap(UnityEngine.InputSystem.Controls.ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheTour_OpensTheJournal_WalksEveryTab_AndComesOnlyOnce()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen, "nothing at first");

            var waited = 0f;
            while (Ui.MenuTour == null || !Ui.MenuTour.IsOpen) { waited += Time.unscaledDeltaTime; if (waited > 30f) break; yield return null; }
            Assert.IsNotNull(Ui.MenuTour);
            Assert.IsTrue(Ui.MenuTour.IsOpen, "the tour starts by itself");
            Assert.IsTrue(session.HasFlag(MenuTourScreen.DoneFlag), "and is remembered");
            Assert.AreEqual(MenuTabs.Journal, Ui.GameMenu.Current.Id, "on the Journal");

            var seen = new List<string> { Ui.GameMenu.Current.Id };
            for (var i = 0; i < 20 && Ui.MenuTour.IsOpen; i++)
            {
                yield return Tap(_keyboard.enterKey);                        // Next: the focused button
                if (Ui.MenuTour.IsOpen) seen.Add(Ui.GameMenu.Current.Id);
            }
            Assert.IsFalse(Ui.MenuTour.IsOpen, "the last step ends it");
            Assert.IsFalse(Ui.AnyModalOpen, "and the menu closes with it");
            CollectionAssert.AreEqual(MenuTourScreen.Order, seen, "every tab, Journal first, in order");

            // Not again.
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(Ui.MenuTour.IsOpen);
        }

        [UnityTest]
        public IEnumerator Skip_EndsTheTour_AtOnce()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            Ui.ShowMenuTour();
            yield return null;
            Assert.IsTrue(Ui.MenuTour.IsOpen);
            UnityEngine.Object.FindObjectsByType<Button>().First(b => b.name == "TourSkip" && b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }
    }
}
