using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Drives the game through simulated keyboard/mouse events (the same path a player uses), not by calling methods.
    public class RealInputTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        Mouse _mouse;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-inputtests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Tap(UnityEngine.InputSystem.Controls.ButtonControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Keyboard_Hoe_Then_Seeds_Plants_A_Crop()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.right);
            yield return null;

            var grid = session.GetGrid(MapIds.Farm);
            var seeds = ItemIds.Seed("parsnip");
            var before = session.Backpack.Count(seeds);

            yield return Tap(_keyboard.digit1Key);          // hoe is hotbar slot 1
            Assert.AreEqual(0, session.State.SelectedHotbar);
            yield return Tap(_keyboard.cKey);                // use tool
            Assert.IsTrue(grid.IsTilled(21, 10), "hoe via keyboard should till the tile in front");

            yield return Tap(_keyboard.digit6Key);          // seeds are hotbar slot 6
            Assert.AreEqual(5, session.State.SelectedHotbar, "key 6 should select the seeds");
            Assert.AreEqual(seeds, session.Backpack.Get(5).ItemId);
            yield return Tap(_keyboard.cKey);
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsNotNull(tile.Crop, "seeds via keyboard should plant a crop on the tilled tile");
            Assert.AreEqual(before - 1, session.Backpack.Count(seeds));
        }

        [UnityTest]
        public IEnumerator Seeds_On_Untilled_Ground_Explain_Why_And_Interact_Key_Plants_On_Tilled_Soil()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var toasts = new System.Collections.Generic.List<string>();
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(e => toasts.Add(e.Message));

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.right);
            yield return null;
            var grid = session.GetGrid(MapIds.Farm);

            // Seeds on bare grass: nothing is planted, and the player is told why.
            yield return Tap(_keyboard.digit6Key);
            yield return Tap(_keyboard.cKey);
            CollectionAssert.Contains(toasts, L.Get("toast.plant_needs_soil"));
            Assert.IsFalse(grid.IsTilled(21, 10));

            // Hoe the tile, then plant with the Interact key (E).
            yield return Tap(_keyboard.digit1Key);
            yield return Tap(_keyboard.cKey);
            Assert.IsTrue(grid.IsTilled(21, 10));
            yield return Tap(_keyboard.digit6Key);
            yield return Tap(_keyboard.eKey);
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsNotNull(tile.Crop, "Interact key should plant selected seeds on tilled soil");

            // A second seed on the same tile is refused with an explanation.
            toasts.Clear();
            yield return Tap(_keyboard.cKey);
            CollectionAssert.Contains(toasts, L.Get("toast.already_planted"));
        }

        [UnityTest]
        public IEnumerator Mouse_Click_Hoes_And_Plants()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.right);
            yield return null;
            var grid = session.GetGrid(MapIds.Farm);

            session.State.SelectedHotbar = 0;
            yield return Tap(_mouse.leftButton);
            Assert.IsTrue(grid.IsTilled(21, 10), "left click with the hoe should till");

            session.State.SelectedHotbar = 5;
            yield return Tap(_mouse.leftButton);
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsNotNull(tile.Crop, "left click with seeds should plant");
        }
    }
}
