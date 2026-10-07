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
    // Playtest 2026-10-06 in the real game, with a simulated mouse: the status bar, selecting from the hotbar by clicking, a click doing what E does,
    // and arriving by the right stairs in the mine.
    public class PlaytestRound6FlowTests : InputTestFixture
    {
        string _dataRoot;
        Mouse _mouse;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-round6-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Enter(string map, string spawn = "default")
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = spawn;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        static Vector2 ScreenCentre(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var c = (corners[0] + corners[2]) * 0.5f;
            return new Vector2(c.x, c.y);
        }

        // A real click: down for a frame, then up (the game reads the press as an action, which needs the frame).
        IEnumerator Tap(UnityEngine.InputSystem.Controls.ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
        }

        IEnumerator MoveMouse(Vector2 position)
        {
            Set(_mouse.position, position);
            yield return null;
            Set(_mouse.position, position + new Vector2(1f, 0f));
            for (var i = 0; i < 3; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheStatusBar_RunsAcrossTheTopOfTheScreen_AndIsTwentyPercentSeeThrough()
        {
            yield return Enter(MapIds.Farm);
            var bar = GameObject.Find("ClockPanel");
            var rt = (RectTransform)bar.transform;
            Assert.AreEqual(0f, rt.anchorMin.x); Assert.AreEqual(1f, rt.anchorMax.x); Assert.AreEqual(1f, rt.anchorMin.y);
            Assert.Less(rt.rect.height, 60f, "a bar, not a box");
            Assert.AreEqual(0.8f, bar.GetComponent<Image>().color.a, 0.01f, "20% see-through by default");
            Assert.AreEqual(0.2f, ServiceLocator.Get<SettingsStore>().Current.HudTransparency, 0.001f);
            var texts = bar.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t => t.gameObject.activeInHierarchy).Select(t => t.text).ToList();
            Assert.IsTrue(texts.Any(t => t.Contains("Spring")), "the date is on the bar");
            Assert.IsTrue(texts.Any(t => t.Contains("g")), "and the gold");
        }

        [UnityTest]
        public IEnumerator ClickingAHotbarSlot_SelectsIt_WithoutSwingingTheTool()
        {
            yield return Enter(MapIds.Farm);
            var energy = _session.State.Energy;
            var from = _session.State.SelectedHotbar;
            var target = Enumerable.Range(0, InputNames.HotbarSlots).First(i => i != from && _session.Backpack.Get(i) != null);
            var slot = (RectTransform)GameObject.Find("Hotbar").transform.Find("Slot" + target);
            yield return MoveMouse(ScreenCentre(slot));
            yield return Tap(_mouse.leftButton);
            for (var i = 0; i < 4; i++) yield return null;
            Assert.AreEqual(target, _session.State.SelectedHotbar, "the clicked item is now in hand");
            Assert.AreEqual(energy, _session.State.Energy, "and the click did not also use it on the ground");
        }

        [UnityTest]
        public IEnumerator ClickingTheShippingBin_WithAToolInHand_OpensIt_LikeTheEKey()
        {
            yield return Enter(MapIds.Farm);
            var bin = UnityEngine.Object.FindAnyObjectByType<ShippingBin>();
            _session.State.SelectedHotbar = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == ItemIds.Hoe);      // a tool in hand
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            player.Teleport(bin.transform.position + new Vector3(-1.5f, -0.5f, 0f));          // beside it: a click reaches as far as the E key does
            player.Face(Vector2Int.right);
            for (var i = 0; i < 60; i++) yield return null;                    // the camera follows the player: let it settle before aiming
            var cam = Camera.main;
            var screen = cam.WorldToScreenPoint(bin.transform.position);
            yield return MoveMouse(new Vector2(screen.x, screen.y));
            var ui = ServiceLocator.Get<UiService>();
            Assert.IsFalse(ui.AnyModalOpen);
            Assert.AreEqual(L.Get("hover.shipping_bin"), HoverInspector.Current, "the mouse is over the bin");
            var energy = _session.State.Energy;
            yield return Tap(_mouse.leftButton);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.IsTrue(ui.AnyModalOpen, $"the shipping window opened from a click (energy {energy} -> {_session.State.Energy}, selected slot {_session.State.SelectedHotbar}, hover {HoverInspector.Current}, blocked {ServiceLocator.Get<InputService>().GameplayBlocked})");
        }

        [UnityTest]
        public IEnumerator ComingUpFromADeeperFloor_ArrivesBesideTheLadderDown_NotBesideTheWayUp()
        {
            yield return Enter(MapIds.Mine, "fromBelow");
            // (a fresh game starts on floor 0 and the controller raises it to 1: go to a deeper floor and load the scene again)
            _session.State.Mine.Floor = 4;
            _session.State.Mine.Deepest = 4;
            _session.State.SpawnPoint = "fromBelow";
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var mine = MineController.Current;
            Assert.IsNotNull(mine);
            Assert.IsTrue(mine.Floor.Ladder.HasValue, "floor 4 has a ladder down");
            var ladder = mine.Floor.Ladder.Value;
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var cell = map.WorldToCell(player.transform.position);
            Assert.AreEqual(1, Mathf.Abs(cell.x - ladder.x) + Mathf.Abs(cell.y - ladder.y), "one step from the ladder down");
        }
    }
}
