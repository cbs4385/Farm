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
    // Owner, 2026-10-09: when the opening story is closed the game walks the player through the screen (each part of the HUD pointed at, with its commands called out),
    // then the controls, then the menu and each of its tabs. In the real game, with the keyboard.
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

        IEnumerator NewGameOnTheFarm()
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
        }

        [UnityTest]
        public IEnumerator TheWalkthrough_PointsAtEachPartOfTheScreen_ThenTheControls_ThenEveryTab_AndComesOnlyOnce()
        {
            yield return NewGameOnTheFarm();
            var session = ServiceLocator.Get<GameSession>();
            Assert.IsFalse(Ui.AnyModalOpen, "nothing at first");

            var waited = 0f;
            while (Ui.MenuTour == null || !Ui.MenuTour.IsOpen) { waited += Time.unscaledDeltaTime; if (waited > 30f) break; yield return null; }
            Assert.IsNotNull(Ui.MenuTour);
            Assert.IsTrue(Ui.MenuTour.IsOpen, "the walkthrough starts by itself");
            Assert.IsTrue(session.HasFlag(MenuTourScreen.DoneFlag), "and is remembered");

            var seen = new List<string>();
            var screenHeight = (float)Screen.height;
            for (var i = 0; i < 40 && Ui.MenuTour.IsOpen; i++)
            {
                var id = Ui.MenuTour.CurrentStepId;
                seen.Add(id);
                if (MenuTourScreen.HudSteps.Contains(id))
                {
                    Assert.IsTrue(Ui.GameMenu == null || !Ui.GameMenu.IsOpen, id + ": the screen itself is shown, not the menu");
                    if (id == "controls") Assert.IsFalse(Ui.MenuTour.HighlightVisible, "the controls point at nothing");
                    else
                    {
                        Assert.IsTrue(Ui.MenuTour.HighlightVisible, id + " is framed");
                        Assert.Greater(Ui.MenuTour.HighlightRect.width, 10f, id + ": a real rectangle");
                        if (id == "hotbar") { Assert.Less(Ui.MenuTour.HighlightRect.center.y, screenHeight * 0.3f, "the item bar is at the bottom"); Assert.IsTrue(Ui.MenuTour.CaptionOnTop, "so the caption goes to the top"); }
                        if (id == "statusbar") { Assert.Greater(Ui.MenuTour.HighlightRect.center.y, screenHeight * 0.9f, "the status bar is at the top"); Assert.IsFalse(Ui.MenuTour.CaptionOnTop); }
                    }
                }
                else
                {
                    Assert.IsTrue(Ui.GameMenu.IsOpen, id + ": the menu is open");
                    Assert.AreEqual(id, Ui.GameMenu.Current.Id, "on that tab");
                    Assert.IsFalse(Ui.MenuTour.HighlightVisible);
                }
                yield return Tap(_keyboard.enterKey);                        // Next: the focused button
            }
            Assert.IsFalse(Ui.MenuTour.IsOpen, "the last step ends it");
            Assert.IsFalse(Ui.AnyModalOpen, "and the menu closes with it");
            CollectionAssert.AreEqual(seen, Ui.MenuTour.StepIds, "every step, once, in order");
            Assert.AreEqual("statusbar", seen[0]);
            Assert.IsTrue(seen.Contains("hotbar") && seen.Contains("controls"));
            CollectionAssert.AreEqual(MenuTourScreen.Order, seen.Where(s => MenuTourScreen.Order.Contains(s)).ToList(), "the tabs, Journal first");
            Assert.Less(seen.IndexOf("controls"), seen.IndexOf(MenuTourScreen.Order[0]), "the screen and the controls come before the menu");

            // Not again.
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(Ui.MenuTour.IsOpen);
        }

        [UnityTest]
        public IEnumerator Back_GoesFromTheMenuBackToTheScreen_AndSkipEndsTheWalkthroughAtOnce()
        {
            yield return NewGameOnTheFarm();
            Ui.ShowMenuTour();
            yield return null;
            Assert.IsTrue(Ui.MenuTour.IsOpen);
            var steps = Ui.MenuTour.StepIds;
            var firstTab = steps.ToList().FindIndex(s => MenuTourScreen.Order.Contains(s));
            for (var i = 0; i < firstTab; i++) yield return Tap(_keyboard.enterKey);
            Assert.IsTrue(Ui.GameMenu.IsOpen, "the first tab opens the menu");
            UnityEngine.Object.FindObjectsByType<Button>().First(b => b.name == "TourBack" && b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.IsFalse(Ui.GameMenu.IsOpen, "back to the screen closes the menu");
            Assert.IsTrue(Ui.MenuTour.IsOpen);
            Assert.AreEqual("controls", Ui.MenuTour.CurrentStepId);
            UnityEngine.Object.FindObjectsByType<Button>().First(b => b.name == "TourSkip" && b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);
        }

        // Owner / playtester, 2026-10-09: "can a player refer to it later if they forget how to do something?" The pause menu has a button that plays the tour again.
        [UnityTest]
        public IEnumerator ThePauseMenu_CanPlayTheTourAgain_AfterItWasSeen()
        {
            yield return NewGameOnTheFarm();
            var session = ServiceLocator.Get<GameSession>();
            session.SetFlag(MenuTourScreen.DoneFlag);                        // already seen
            Ui.ShowPause();
            yield return null;
            UnityEngine.Object.FindObjectsByType<Button>().First(b => b.name == "MenuTour" && b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.IsTrue(Ui.MenuTour.IsOpen, "the tour plays again");
            Assert.AreEqual("statusbar", Ui.MenuTour.CurrentStepId);
            UnityEngine.Object.FindObjectsByType<Button>().First(b => b.name == "TourSkip" && b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the pause menu is still there after the tour");
            Assert.IsFalse(Ui.MenuTour.IsOpen);
        }
    }
}
