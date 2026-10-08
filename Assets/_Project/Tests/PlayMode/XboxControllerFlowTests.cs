using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // An Xbox controller (the Input System's XInputController, standing in for the hardware): the game follows whichever control was touched last, so
    // the hints say A and B or E and Esc; a pad player can type a name with the on-screen keyboard; photo mode works from the pad; and unplugging the pad
    // pauses the game.
    public class XboxControllerFlowTests : InputTestFixture
    {
        string _dataRoot;
        XInputController _pad;
        Keyboard _keyboard;
        Mouse _mouse;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-xbox-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            ControlPrompts.ResetForTests();
            PhotoMode.ResetForTests();
            OnScreenKeyboardOpener.Reset();
            _pad = InputSystem.AddDevice<XInputController>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            ControlPrompts.ResetForTests();
            PhotoMode.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
        }

        IEnumerator StartFarm()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        static string AllText(GameObject root) => string.Join("\n", root.GetComponentsInChildren<TextMeshProUGUI>(true).Select(t => t.text));

        [UnityTest]
        public IEnumerator TheHints_FollowTheLastControlTouched_PadOrKeyboard()
        {
            yield return StartFarm();
            Assert.AreEqual(InputKind.KeyboardMouse, ControlPrompts.Kind, "the keyboard until a pad is touched");

            yield return Tap(_pad.buttonSouth);
            Assert.AreEqual(InputKind.Gamepad, ControlPrompts.Kind, "A was pressed");

            var ui = ServiceLocator.Get<UiService>();
            yield return Tap(_pad.selectButton);                         // View opens the game menu
            Assert.IsTrue(ui.AnyModalOpen, "the menu is open");
            var menu = ui.GameMenu.Root;
            StringAssert.Contains("LB / RB: switch tab   B: close", AllText(menu), "the footer names the pad's buttons");
            StringAssert.DoesNotContain("Esc: close", AllText(menu));

            yield return Tap(_keyboard.digit5Key);                       // any key: back to the keyboard's names
            Assert.AreEqual(InputKind.KeyboardMouse, ControlPrompts.Kind);
            StringAssert.Contains("Q / E: switch tab   Esc: close", AllText(menu), "the open menu changes its hint at once");

            Set(_pad.leftStick, new Vector2(0.9f, 0f));                  // a pushed stick counts too
            yield return null;
            Set(_pad.leftStick, Vector2.zero);
            Assert.AreEqual(InputKind.Gamepad, ControlPrompts.Kind);
        }

        [UnityTest]
        public IEnumerator ThePad_CanTypeAName_WithTheOnScreenKeyboard()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var op = SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "New Game").onClick.Invoke();
            yield return null;
            var name = UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).First(f => f.gameObject.activeInHierarchy);
            name.text = "Rin";
            EventSystem.current.SetSelectedGameObject(name.gameObject);
            yield return null;

            yield return Tap(_pad.buttonSouth);                          // A on the field
            var ui = ServiceLocator.Get<UiService>();
            var keys = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b => b.name.StartsWith("Key_")).ToList();
            Assert.Greater(keys.Count, 30, "the on-screen keyboard is up");
            Assert.AreEqual("Rin", name.text);

            Button Key(string id) => keys.First(b => b.name == "Key_" + id);
            EventSystem.current.SetSelectedGameObject(Key("a").gameObject);
            yield return null;
            yield return Tap(_pad.buttonSouth);                          // A presses the key under the cursor
            Assert.AreEqual("Rina", name.text, "a letter in the middle of a name is lower case");

            yield return Tap(_pad.buttonWest);                           // X deletes
            Assert.AreEqual("Rin", name.text);
            yield return Tap(_pad.buttonNorth);                          // Y puts in a space; the next letter is a capital
            EventSystem.current.SetSelectedGameObject(Key("o").gameObject);
            yield return null;
            yield return Tap(_pad.buttonSouth);
            Assert.AreEqual("Rin O", name.text);

            yield return Tap(_pad.startButton);                          // Menu is Done
            Assert.IsEmpty(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b => b.name == "Key_a" && b.gameObject.activeInHierarchy).ToList(), "the keyboard closed");
            Assert.AreEqual("Rin O", name.text, "and the name stayed");
            Assert.AreSame(name.gameObject, EventSystem.current.currentSelectedGameObject, "the field has the focus again");

            // B backs out and puts the old text back.
            yield return Tap(_pad.buttonSouth);
            EventSystem.current.SetSelectedGameObject(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "Key_z" && b.gameObject.activeInHierarchy).gameObject);
            yield return null;
            yield return Tap(_pad.buttonSouth);
            Assert.AreEqual("Rin Oz", name.text);
            yield return Tap(_pad.buttonEast);
            Assert.AreEqual("Rin O", name.text, "B undoes what was typed since the keyboard opened");
        }

        [UnityTest]
        public IEnumerator OnTheKeyboard_AFieldIsStillTypedInDirectly_AndOpensNoPadKeyboard()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var op = SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.name == "New Game").onClick.Invoke();
            yield return null;
            yield return Tap(_keyboard.spaceKey);                        // keyboard in use
            var name = UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).First(f => f.gameObject.activeInHierarchy);
            EventSystem.current.SetSelectedGameObject(name.gameObject);
            yield return null;
            yield return Tap(_keyboard.enterKey);
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b => b.name == "Key_a" && b.gameObject.activeInHierarchy), "no pad keyboard for a keyboard player");
        }

        [UnityTest]
        public IEnumerator PhotoMode_WorksFromThePad()
        {
            yield return StartFarm();
            Assert.IsFalse(PhotoMode.Active);
            yield return Tap(_pad.leftStickButton);                      // the left stick pressed in
            Assert.IsTrue(PhotoMode.Active, "photo mode is on");
            yield return Tap(_pad.buttonNorth);                          // Y: next villager (nothing breaks with none near)
            yield return Tap(_pad.rightShoulder);                        // RB: an emote
            yield return Tap(_pad.buttonWest);                           // X: clear it
            yield return Tap(_pad.buttonEast);                           // B leaves
            Assert.IsFalse(PhotoMode.Active, "B leaves photo mode");
        }

        [UnityTest]
        public IEnumerator UnpluggingThePad_WhileItIsInUse_PausesTheGame()
        {
            yield return StartFarm();
            yield return Tap(_pad.buttonSouth);
            Assert.AreEqual(InputKind.Gamepad, ControlPrompts.Kind);
            var ui = ServiceLocator.Get<UiService>();
            Assert.IsFalse(ui.AnyModalOpen);

            InputSystem.RemoveDevice(_pad);
            yield return null;
            yield return null;
            Assert.AreEqual(InputKind.KeyboardMouse, ControlPrompts.Kind, "back to the keyboard's hints");
            Assert.IsTrue(ui.AnyModalOpen, "the pause menu came up");
        }

        // Playtest 2026-10-08: "the only place the controller does not work is the end of day pop up when you go to sleep: you have to move the mouse to click
        // Continue" (a tester who plays from an exercise bike). The way a player really gets there: A on the bed, A on Yes, then A on Continue.
        [UnityTest]
        public IEnumerator TheEndOfDayPopUp_IsClosedWithA_AfterChoosingToSleepWithA()
        {
            yield return StartFarm();
            var session = ServiceLocator.Get<GameSession>();
            var ui = ServiceLocator.Get<UiService>();
            yield return Tap(_pad.buttonSouth);                          // the pad is the control in use

            ui.ShowConfirm("confirm.sleep", () => session.StartSleep(false));        // what the bed does
            yield return null;
            Assert.IsTrue(ui.AnyModalOpen, "the question is up");
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject, "and a choice has the focus");
            yield return Tap(_pad.buttonSouth);                          // A on Yes

            bool SummaryUp() { var go = GameObject.Find("DaySummary"); return go != null && go.activeInHierarchy; }
            var waited = 0f;
            while (!SummaryUp() && waited < 10f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsTrue(SummaryUp(), "the end of day summary is up");
            for (var i = 0; i < 8; i++) yield return null;

            var focused = EventSystem.current.currentSelectedGameObject;
            Assert.IsNotNull(focused, "the Continue button has the focus, without a mouse");
            Assert.IsTrue(focused.transform.IsChildOf(GameObject.Find("DaySummary").transform), "and it is on the summary");
            yield return Tap(_pad.buttonSouth);                          // A on Continue
            waited = 0f;
            while (SummaryUp() && waited < 10f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsFalse(SummaryUp(), "A closed the summary");
        }

        // Even with nothing focused (the focus lost as the screen came up), A, Enter or Space continues: nobody is stuck on this screen.
        [UnityTest]
        public IEnumerator TheEndOfDayPopUp_ContinuesWithA_EvenWithNothingFocused()
        {
            yield return StartFarm();
            var session = ServiceLocator.Get<GameSession>();
            var ui = ServiceLocator.Get<UiService>();
            yield return Tap(_pad.buttonSouth);
            session.StartSleep(false);
            bool SummaryUp() { var go = GameObject.Find("DaySummary"); return go != null && go.activeInHierarchy; }
            var waited = 0f;
            while (!SummaryUp() && waited < 10f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsTrue(SummaryUp());
            for (var i = 0; i < 5; i++) yield return null;
            EventSystem.current.SetSelectedGameObject(null);             // the focus is gone
            yield return null;
            yield return Tap(_pad.buttonSouth);
            waited = 0f;
            while (SummaryUp() && waited < 10f) { waited += Time.unscaledDeltaTime; yield return null; }
            Assert.IsFalse(SummaryUp(), "A continued with nothing focused");
        }

        // A screen with nothing focused gives the focus back as soon as the stick or the D-pad is touched.
        [UnityTest]
        public IEnumerator AScreenWithNothingFocused_GetsItsFocusBack_WhenThePadIsPushed()
        {
            yield return StartFarm();
            var ui = ServiceLocator.Get<UiService>();
            yield return Tap(_pad.buttonSouth);
            yield return Tap(_pad.startButton);                          // Menu: the pause screen
            Assert.IsTrue(ui.AnyModalOpen);
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
            Assert.IsNull(EventSystem.current.currentSelectedGameObject);
            Set(_pad.leftStick, new Vector2(0f, -1f));
            yield return null;
            yield return null;
            Set(_pad.leftStick, Vector2.zero);
            yield return null;
            var focused = EventSystem.current.currentSelectedGameObject;
            Assert.IsNotNull(focused, "a control has the focus again");
            Assert.IsTrue(focused.GetComponent<Button>() != null, "a button of the pause screen");
        }
    }
}
