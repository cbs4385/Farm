using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
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
        public IEnumerator FinishingAnEnergyAction_PublishesTheEvent_OncePerSuccessfulAction()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var events = new System.Collections.Generic.List<EnergyActionCompleted>();
            ServiceLocator.Get<EventBus>().Subscribe<EnergyActionCompleted>(events.Add);

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            player.Face(Vector2Int.right);
            yield return null;

            yield return Tap(_keyboard.digit1Key);   // hoe
            yield return Tap(_keyboard.cKey);        // tills: spends energy
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(PlayerActions.HoeEnergy, events[0].Cost);
            Assert.AreEqual("Hoe", events[0].Action);

            yield return Tap(_keyboard.cKey);        // already tilled: refused, no energy spent
            Assert.AreEqual(1, events.Count, "a refused action is not an energy action");

            yield return Tap(_keyboard.digit2Key);   // watering can
            yield return Tap(_keyboard.cKey);
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual("WateringCan", events[1].Action);

            yield return Tap(_keyboard.digit3Key);   // axe has no target: nothing happens
            yield return Tap(_keyboard.cKey);
            Assert.AreEqual(2, events.Count);
        }

        [UnityTest]
        public IEnumerator F1_Opens_The_Developer_Console_And_Commands_Change_The_Running_Game()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;

            var ui = ServiceLocator.Get<UiService>();
            var input = ServiceLocator.Get<InputService>();
            Assert.IsFalse(ui.AnyModalOpen);

            yield return Tap(_keyboard.f1Key);
            Assert.IsTrue(ui.AnyModalOpen, "F1 opens the console");
            Assert.IsTrue(ui.DebugConsole.IsOpen);
            Assert.IsTrue(input.GameplayBlocked, "the player cannot walk around while typing");

            Assert.IsTrue(ui.DebugConsole.Submit("gold 1234").Ok);
            Assert.AreEqual(1234, session.State.Gold);
            Assert.IsTrue(ui.DebugConsole.Submit("var dread 12").Ok);
            Assert.AreEqual(12, session.GetVar("dread"));
            Assert.IsFalse(ui.DebugConsole.Submit("nonsense").Ok);

            yield return Tap(_keyboard.f1Key);
            Assert.IsFalse(ui.AnyModalOpen, "F1 closes it again");
            Assert.IsFalse(input.GameplayBlocked);

            // F1 never stacks on top of another screen.
            ui.ToggleInventory();
            yield return null;
            yield return Tap(_keyboard.f1Key);
            Assert.IsFalse(ui.DebugConsole.IsOpen);
            ui.ToggleInventory();
            yield return null;

            // Skipping a day runs the real overnight logic and reloads the map into the farmhouse.
            yield return Tap(_keyboard.f1Key);
            var startDay = session.Clock.Now.Day;
            Assert.IsTrue(ui.DebugConsole.Submit("day 2").Ok);
            Assert.AreEqual(startDay + 2, session.Clock.Now.Day);
            var loader = ServiceLocator.Get<SceneLoader>();
            var start = Time.realtimeSinceStartup;
            while ((SceneManager.GetActiveScene().name != MapIds.FarmHouse || loader.IsLoading) && Time.realtimeSinceStartup - start < 10f)
                yield return null;
            Assert.AreEqual(MapIds.FarmHouse, SceneManager.GetActiveScene().name);
            Assert.IsFalse(ui.AnyModalOpen, "the console closed itself before the reload");
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
