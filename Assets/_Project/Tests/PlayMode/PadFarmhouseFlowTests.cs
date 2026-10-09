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
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // Playtest 2026-10-09 (controller only): "I went in my house but could not select anything". With a simulated pad alone, inside the farmhouse: the bed and the
    // kitchen answer A, their screens can be worked with the d-pad, and the item bar can be picked with the shoulder buttons.
    public class PadFarmhouseFlowTests : InputTestFixture
    {
        string _dataRoot;
        Gamepad _pad;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-padhouse-" + Guid.NewGuid().ToString("N"));
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

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static UiService Ui => ServiceLocator.Get<UiService>();
        static GameObject Selected => EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
            yield return null;
        }

        IEnumerator Stand(int x, int y, Vector2Int facing)
        {
            Player.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);
            Player.Face(facing);
            for (var i = 0; i < 4; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator InTheFarmhouse_TheBedAndTheKitchenAnswerA_AndTheirScreensWorkWithThePad()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.CurrentMap = MapIds.FarmHouse;
            var op = SceneManager.LoadSceneAsync(MapIds.FarmHouse);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
            Assert.IsFalse(Ui.AnyModalOpen);

            // The item bar: the shoulder buttons pick items (nothing needs the mouse).
            var before = session.State.SelectedHotbar;
            yield return Tap(_pad.rightShoulder);
            Assert.AreNotEqual(before, session.State.SelectedHotbar, "RB picks the next item");

            // The kitchen: stand below it, face it, press A.
            session.Backpack.Add(ItemIds.Crop("parsnip"), 3);
            yield return Stand(9, 6, Vector2Int.up);
            yield return Tap(_pad.buttonSouth);
            Assert.IsTrue(Ui.AnyModalOpen, "A at the kitchen opens it");
            Assert.IsNotNull(Selected, "and the cursor is on something");
            var first = Selected;
            yield return Tap(_pad.dpad.down);
            yield return Tap(_pad.dpad.up);
            Assert.IsNotNull(Selected, "the d-pad keeps the cursor on the screen");
            yield return Tap(_pad.buttonEast);                           // B
            Assert.IsFalse(Ui.AnyModalOpen, "B closes it");

            // The label of what the player faces shows without a mouse (the hover labels follow the cell faced when a pad is in use).
            yield return Stand(9, 6, Vector2Int.up);
            yield return Tap(_pad.buttonSouth);
            yield return Tap(_pad.buttonEast);
            Assert.IsFalse(Ui.AnyModalOpen);
            yield return Stand(9, 6, Vector2Int.up);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.IsNotNull(HoverInspector.Current, "a label names the kitchen the pad player faces");
            StringAssert.Contains("itchen", HoverInspector.Current);

            // The bed: stand east of it, face it, press A: the bed asks, and A / B answer.
            yield return Stand(3, 6, Vector2Int.left);
            yield return Tap(_pad.buttonSouth);
            Assert.IsTrue(Ui.AnyModalOpen, "A at the bed asks about sleeping");
            Assert.IsNotNull(Selected, "with a button under the cursor");
            yield return Tap(_pad.buttonEast);                           // B: not now
            Assert.IsFalse(Ui.AnyModalOpen, "B says no");
            Assert.AreEqual(MapIds.FarmHouse, session.State.CurrentMap, "and nothing else happened");
        }
    }
}
