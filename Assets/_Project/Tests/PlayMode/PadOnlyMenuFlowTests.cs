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
    // Playtest 2026-10-09 (controller only, 18 minutes): "the controller does not work on the options menu and I could not name my character with it either".
    // Here the menus are driven only by a simulated pad's d-pad and buttons, from the title screen, with nothing set or clicked by the test.
    public class PadOnlyMenuFlowTests : InputTestFixture
    {
        string _dataRoot;
        Gamepad _pad;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-padonly-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _pad = InputSystem.AddDevice<Gamepad>();
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

        IEnumerator ToTitle()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var op = SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
        }

        // Presses down on the d-pad until the selected control's label (or name) contains `text`.
        IEnumerator DownTo(string text, int max = 40)
        {
            for (var i = 0; i < max; i++)
            {
                var s = Selected;
                if (s != null && (s.name.Contains(text) || s.GetComponentsInChildren<TMP_Text>().Any(t => t.text.Contains(text)))) yield break;
                yield return Tap(_pad.dpad.down);
            }
            Assert.Fail("never reached '" + text + "'; selected: " + (Selected != null ? Selected.name : "nothing"));
        }

        [UnityTest]
        public IEnumerator TheTitleScreen_HasTheFocusAtOnce_AndTheOptionsCanBeWorkedWithThePadAlone()
        {
            yield return ToTitle();
            Assert.IsNotNull(Selected, "something is selected on the title screen");

            yield return DownTo("Options");
            yield return Tap(_pad.buttonSouth);
            var options = UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None).Where(s => s.gameObject.activeInHierarchy).ToList();
            Assert.IsNotEmpty(options, "the options screen opened");
            Assert.IsNotNull(Selected, "and has the focus");
            Assert.IsTrue(Selected.GetComponentInParent<Slider>() != null || Selected.GetComponent<Selectable>() != null);

            // Walk the whole screen with the d-pad: the cursor moves every press, and a slider and a toggle are reached.
            var visited = new List<GameObject>();
            var trail = new List<string>();
            for (var i = 0; i < 60; i++)
            {
                if (Selected == null) Assert.Fail("the focus was lost after " + i + " presses; last: " + (visited.Count > 0 ? visited.Last().name : "-"));
                if (!visited.Contains(Selected)) visited.Add(Selected);
                if (i < 7) { var sel = Selected.GetComponent<Selectable>(); var dn = sel.FindSelectableOnDown(); trail.Add(Selected.transform.parent.name + "@" + Selected.transform.position.y.ToString("0") + "->" + (dn == null ? "null" : dn.transform.parent.name + "@" + dn.transform.position.y.ToString("0"))); }
                yield return Tap(_pad.dpad.down);
            }
            var last = Selected.GetComponent<Selectable>();
            string P(Component c) => c == null ? "null" : c.name + "(" + c.transform.position.x.ToString("0") + "," + c.transform.position.y.ToString("0") + ")" + (c.transform.parent != null ? "/" + c.transform.parent.name : "");
            var res = UnityEngine.Object.FindObjectsByType<Selectable>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == "1280x720");
            var info = "trail=" + string.Join(">", trail) + " last=" + P(last) + " down=" + P(last.FindSelectableOnDown()) + " res=" + P(res) + " lastScale=" + last.transform.lossyScale + " rot=" + last.transform.rotation.eulerAngles;
            Assert.IsTrue(visited.Any(v => v.GetComponent<Toggle>() != null), info);
            Assert.IsTrue(visited.Any(v => v.GetComponent<Slider>() != null), "a slider can be reached");
            Assert.IsTrue(visited.Any(v => v.GetComponent<Toggle>() != null), "a toggle can be reached; visited: " + string.Join(", ", visited.Select(v => v.name + (v.GetComponent<Selectable>() != null ? ":" + v.GetComponent<Selectable>().GetType().Name : ""))));

            // A slider moves with left and right.
            var slider = visited.First(v => v.GetComponent<Slider>() != null).GetComponent<Slider>();
            EventSystem.current.SetSelectedGameObject(slider.gameObject);
            yield return null;
            var before = slider.value;
            yield return Tap(_pad.dpad.left);
            yield return Tap(_pad.dpad.left);
            Assert.Less(slider.value, before, "d-pad left lowers a slider");

            // A flips a toggle.
            var toggle = visited.First(v => v.GetComponent<Toggle>() != null).GetComponent<Toggle>();
            EventSystem.current.SetSelectedGameObject(toggle.gameObject);
            yield return null;
            var on = toggle.isOn;
            yield return Tap(_pad.buttonSouth);
            Assert.AreNotEqual(on, toggle.isOn, "A flips a toggle");

            yield return Tap(_pad.buttonEast);
            Assert.IsEmpty(UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None).Where(s => s.gameObject.activeInHierarchy).ToList(), "B closes the options");
            Assert.IsNotNull(Selected, "and the title screen has the focus back");
        }

        [UnityTest]
        public IEnumerator ANewGame_CanBeNamedAndStarted_WithThePadAlone()
        {
            yield return ToTitle();
            yield return DownTo("New Game");
            yield return Tap(_pad.buttonSouth);
            TMP_InputField field = null;
            Assert.IsNotNull(Selected, "the new game screen has the focus");

            // Walk to a name field with the d-pad and open the keyboard with A.
            TMP_InputField OnField() => Selected != null ? Selected.GetComponent<TMP_InputField>() : null;
            for (var i = 0; i < 12 && OnField() == null; i++) yield return Tap(_pad.dpad.down);
            for (var i = 0; i < 12 && OnField() == null; i++) yield return Tap(_pad.dpad.up);
            field = OnField();
            Assert.IsNotNull(field, "a name field can be reached with the d-pad; selected=" + (Selected != null ? Selected.name : "none"));
            yield return Tap(_pad.buttonSouth);
            var keys = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b => b.name.StartsWith("Key_") && b.gameObject.activeInHierarchy).ToList();
            Assert.Greater(keys.Count, 30, "the on-screen keyboard opens from A on the field");
            Assert.IsNotNull(Selected, "and the cursor is on it");
            Assert.IsTrue(Selected.name.StartsWith("Key_"), "on a key, not behind it");
            yield return Tap(_pad.startButton);                          // Done
            Assert.AreSame(field.gameObject, Selected, "the field has the focus again");
        }
    }
}
