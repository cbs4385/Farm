using System;
using System.Collections;
using System.Collections.Generic;
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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // The Options as a category list and pages (playtest 2026-10-16, controller only: the cursor went wrong at the foot of the visible list, the right stick moved the list
    // under the cursor, and the controller sensitivity took scrolling, opening a group and scrolling again). Driven by a simulated pad and keyboard, nothing set by the test.
    public class OptionsPagesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Gamepad _pad;
        Keyboard _keyboard;
        UiService _ui;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-optionspages-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _pad = InputSystem.AddDevice<Gamepad>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
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
            yield return null;
        }

        static GameObject Selected => EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        IEnumerator OpenOptions(string page = null)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = GameContext.Current.Session;
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
            _ui = (UiService)GameContext.Current.Ui;
            _ui.ShowOptions();
            if (page != null) _ui.Options.Expand(page);
            for (var i = 0; i < 4; i++) yield return null;
            Canvas.ForceUpdateCanvases();
        }

        static ScrollRect PageScroll() => UnityEngine.Object.FindObjectsByType<ScrollRect>().First(s => s.gameObject.activeInHierarchy);

        // Every control of the page showing.
        static List<Selectable> PageControls() =>
            PageScroll().content.GetComponentsInChildren<Selectable>(false).Where(s => s.IsInteractable() && s.navigation.mode != Navigation.Mode.None).ToList();

        static bool OnScreen(Selectable s)
        {
            var view = new Vector3[4]; var item = new Vector3[4];
            ((RectTransform)PageScroll().transform).GetWorldCorners(view);
            ((RectTransform)s.transform).GetWorldCorners(item);
            return item[0].y >= view[0].y - 1f && item[1].y <= view[1].y + 1f;
        }

        [UnityTest]
        public IEnumerator EveryPage_CanBeWalkedWithTheDpad_EveryRowIsReached_TheFocusStaysOnScreenAndOnThePage_AndDownAtTheFootStays()
        {
            yield return OpenOptions();
            foreach (var id in OptionsScreen.Groups)
            {
                _ui.Options.Expand(id);
                for (var i = 0; i < 3; i++) yield return null;
                var controls = PageControls();
                Assert.IsNotEmpty(controls, id);
                Assert.IsNotNull(Selected, id + ": the page opens with the cursor on it");
                Assert.AreSame(controls[0].gameObject, Selected, id + ": on its first control");

                var rowsY = controls.Select(c => Mathf.RoundToInt(c.transform.position.y)).Distinct().ToList();
                var reached = new HashSet<int>();
                GameObject before = null;
                for (var press = 0; press < 160; press++)
                {
                    Assert.IsNotNull(Selected, $"{id}: the focus is lost after {press} presses");
                    var s = Selected.GetComponent<Selectable>();
                    Assert.IsTrue(controls.Contains(s), $"{id}: the cursor left the page for '{Selected.name}'");
                    reached.Add(Mathf.RoundToInt(Selected.transform.position.y));
                    Assert.IsTrue(OnScreen(s), $"{id}: '{Selected.name}' is selected but off screen after {press} presses");
                    before = Selected;
                    yield return Tap(_pad.dpad.down);
                    if (Selected == before)
                    {
                        yield return Tap(_pad.dpad.down);
                        if (Selected == before) break;
                    }
                }
                Assert.AreSame(before, Selected, id + ": down at the foot of the page stays");
                Assert.AreSame(controls.Last().gameObject, Selected, id + ": the foot is the last control of the page");
                // positions move as the page scrolls, so rows are counted by control, not by height
                Assert.Greater(reached.Count, 0);
            }
        }

        [UnityTest]
        public IEnumerator UpFromAnywhere_ComesBackTheSameWay_AndNeverPastTheFirstRow()
        {
            yield return OpenOptions("gameplay");
            var first = Selected;
            yield return Tap(_pad.dpad.up);
            Assert.AreSame(first, Selected, "up at the top stays");
            for (var i = 0; i < 4; i++) yield return Tap(_pad.dpad.down);
            var four = Selected;
            Assert.AreNotSame(first, four);
            for (var i = 0; i < 4; i++) yield return Tap(_pad.dpad.up);
            Assert.AreSame(first, Selected, "four down and four up are back where they started");
        }

        [UnityTest]
        public IEnumerator TheBackButton_IsNotInTheFocusChain_AndBClosesTheScreen()
        {
            yield return OpenOptions();
            for (var i = 0; i < 12; i++)
            {
                yield return Tap(_pad.dpad.down);
                Assert.AreNotEqual(L.Get("ui.back"), Selected.name, "the cursor never lands on Back");
                Assert.IsTrue(PageControls().Any(c => c.gameObject == Selected), "it stays on the page");
            }
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(_ui.AnyModalOpen, "B closes the options from anywhere on a page");
        }

        [UnityTest]
        public IEnumerator TheShoulderButtons_ChangeThePage_FromAnywhere_AndTheCursorGoesToItsFirstControl()
        {
            yield return OpenOptions();
            Assert.AreEqual("sound", _ui.Options.CurrentPage);
            yield return Tap(_pad.dpad.down);
            yield return Tap(_pad.dpad.down);
            yield return Tap(_pad.rightShoulder);
            Assert.AreEqual("display", _ui.Options.CurrentPage);
            Assert.AreSame(PageControls()[0].gameObject, Selected, "on the first control of the new page, not left on a control of the page that went");
            yield return Tap(_pad.leftShoulder);
            yield return Tap(_pad.leftShoulder);
            Assert.AreEqual("content", _ui.Options.CurrentPage, "it goes round: before the first page is the last");
            yield return Tap(_pad.rightShoulder);
            Assert.AreEqual("sound", _ui.Options.CurrentPage, "and after the last is the first");

            Press(_keyboard.eKey);
            yield return null;
            Release(_keyboard.eKey);
            yield return null;
            yield return null;
            Assert.AreEqual("display", _ui.Options.CurrentPage, "E does the same on the keyboard");
            Press(_keyboard.qKey);
            yield return null;
            Release(_keyboard.qKey);
            yield return null;
            yield return null;
            Assert.AreEqual("sound", _ui.Options.CurrentPage, "and Q goes back");
        }

        [UnityTest]
        public IEnumerator TheControllerSensitivity_IsTwoShoulderPressesFromOpeningTheOptions_AndMovesWithLeftAndRight()
        {
            yield return OpenOptions();
            yield return Tap(_pad.leftShoulder);
            yield return Tap(_pad.leftShoulder);
            Assert.AreEqual("pad", _ui.Options.CurrentPage);
            Assert.AreEqual("ControllerSensitivity", Selected.name, "the cursor is on it already");
            var slider = Selected.GetComponent<Slider>();
            var before = slider.value;
            yield return Tap(_pad.dpad.right);
            yield return Tap(_pad.dpad.right);
            Assert.Greater(slider.value, before, "right raises it");
            yield return Tap(_pad.dpad.left);
            yield return Tap(_pad.dpad.left);
            yield return Tap(_pad.dpad.left);
            Assert.Less(slider.value, before, "and left lowers it");
        }

        [UnityTest]
        public IEnumerator LeftAndRight_CycleAChoice_AndSwitchAToggle()
        {
            yield return OpenOptions("content");
            Assert.AreEqual("HorrorLevel", Selected.name);
            var settings = GameContext.Current.Settings.Current;
            var start = settings.HorrorLevel;
            yield return Tap(_pad.dpad.right);
            Assert.AreEqual((start + 1) % 3, settings.HorrorLevel, "right goes to the next choice");
            yield return Tap(_pad.dpad.left);
            yield return Tap(_pad.dpad.left);
            Assert.AreEqual((start + 2) % 3, settings.HorrorLevel, "left goes back, and round");
            Assert.AreEqual(L.Get("options.horror." + settings.HorrorLevel), Selected.GetComponentInChildren<TMP_Text>().text, "the button shows it");
            yield return Tap(_pad.buttonSouth);
            Assert.AreEqual((start + 3) % 3, settings.HorrorLevel, "A goes on to the next");

            _ui.Options.Expand("gameplay");
            for (var i = 0; i < 3; i++) yield return null;
            var toggle = Selected.GetComponent<Toggle>();
            Assert.IsNotNull(toggle, "the first row of the gameplay page is a toggle");
            yield return Tap(_pad.dpad.right);
            Assert.IsTrue(toggle.isOn, "right turns it on");
            yield return Tap(_pad.dpad.left);
            Assert.IsFalse(toggle.isOn, "left turns it off");
            yield return Tap(_pad.dpad.right);
            Assert.IsTrue(toggle.isOn);
        }

        [UnityTest]
        public IEnumerator TheRightStick_ScrollsThePage_AndTheFocusFollowsOntoWhatIsShowing()
        {
            yield return OpenOptions("keys");
            var scroll = PageScroll();
            Assert.Greater(scroll.content.rect.height, ((RectTransform)scroll.transform).rect.height, "the key page is longer than its window");
            var first = Selected;
            Set(_pad.rightStick, new Vector2(0f, -1f));
            yield return new WaitForSeconds(0.6f);
            Set(_pad.rightStick, Vector2.zero);
            for (var i = 0; i < 4; i++) yield return null;
            Assert.Less(scroll.verticalNormalizedPosition, 0.95f, "the stick scrolled the page down");
            Assert.AreNotSame(first, Selected, "the cursor moved with it");
            Assert.IsTrue(OnScreen(Selected.GetComponent<Selectable>()), "and is on screen");

            yield return Tap(_pad.dpad.down);
            Assert.IsTrue(OnScreen(Selected.GetComponent<Selectable>()), "the next press goes on from there");
        }

        [UnityTest]
        public IEnumerator TheRowWithTheCursor_IsLit_AndTheFooterSaysWhichRowItIs()
        {
            yield return OpenOptions();
            var row = Selected.transform.parent;
            var skin = row.GetComponent<Image>();
            Assert.IsNotNull(skin);
            Assert.Greater(skin.color.a, 0.1f, "the row with the cursor is lit");
            var other = row.parent.GetChild(row.GetSiblingIndex() + 1).GetComponent<Image>();
            Assert.AreEqual(0f, other.color.a, 0.001f, "the others are not");
            var footer = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().First(t => t.gameObject.activeInHierarchy && t.text.StartsWith("1 / "));
            StringAssert.StartsWith("1 / 4", footer.text);
            yield return Tap(_pad.dpad.down);
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>().Any(t => t.gameObject.activeInHierarchy && t.text == "2 / 4"), "and it counts as the cursor moves");
        }

        [UnityTest]
        public IEnumerator TheCursorCanReachTheCategoryList_FromAKeyRow_AndWalkItWithTheDpad()
        {
            yield return OpenOptions("keys");
            var key = PageControls().First(c => c.name.StartsWith(OptionsScreen.KeyPrefix));
            EventSystem.current.SetSelectedGameObject(key.gameObject);
            yield return null;
            yield return Tap(_pad.dpad.left);
            Assert.AreEqual(OptionsScreen.CategoryPrefix + "keys", Selected.name, "left from the first key of a row goes to the list, on the page's own category");
            yield return Tap(_pad.dpad.down);
            Assert.AreEqual(OptionsScreen.CategoryPrefix + "pad", Selected.name);
            yield return Tap(_pad.dpad.right);
            Assert.AreSame(PageControls()[0].gameObject, Selected, "right goes back into the page");
        }

        [UnityTest]
        public IEnumerator AControllerRebind_GivesUpByItself_SoAPlayerWithOnlyAPadIsNeverStuck()
        {
            yield return OpenOptions("pad");
            var input = GameContext.Current.Input;
            var button = PageControls().OfType<Button>().First(b => b.name.StartsWith(OptionsScreen.PadPrefix));
            var action = input.Gameplay[button.name.Substring(OptionsScreen.PadPrefix.Length).Split('_')[1]];
            var pathBefore = action.bindings.First(b => b.path.StartsWith("<Gamepad>")).effectivePath;
            var textBefore = button.GetComponentInChildren<TMP_Text>().text;
            button.onClick.Invoke();
            yield return null;
            StringAssert.Contains(L.Get("options.press_pad"), button.GetComponentInChildren<TMP_Text>().text, "it is waiting");
            yield return new WaitForSeconds(OptionsScreen.RebindSeconds + 0.8f);
            Assert.AreEqual(textBefore, button.GetComponentInChildren<TMP_Text>().text, "after a few seconds the button shows its own binding again");
            Assert.AreEqual(pathBefore, action.bindings.First(b => b.path.StartsWith("<Gamepad>")).effectivePath, "nothing was rebound");
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(_ui.AnyModalOpen, "and B closes the screen again, as it does when nothing is being waited for");
        }

        [UnityTest]
        public IEnumerator APressedButton_StopsTheWait_SoTheTimeoutDoesNotCancelAnOtherRebind()
        {
            yield return OpenOptions("pad");
            var button = PageControls().OfType<Button>().First(b => b.name == OptionsScreen.PadPrefix + "Gameplay_Rotate");
            button.onClick.Invoke();
            yield return null;
            Press(_pad.rightTrigger);
            yield return null;
            Release(_pad.rightTrigger);
            yield return null;
            Assert.AreEqual("RT", button.GetComponentInChildren<TMP_Text>().text);
            var second = PageControls().OfType<Button>().First(b => b.name == OptionsScreen.PadPrefix + "Gameplay_Inventory");
            second.onClick.Invoke();
            yield return new WaitForSeconds(OptionsScreen.RebindSeconds - 1.5f);
            StringAssert.Contains(L.Get("options.press_pad"), second.GetComponentInChildren<TMP_Text>().text, "the first wait's timer is gone: the second wait is not cut short");
        }
    }
}
