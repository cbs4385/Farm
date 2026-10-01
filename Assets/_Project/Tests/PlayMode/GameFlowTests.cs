using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // End-to-end flows through the real scenes and services (Farm scene, UI, save files in a temp folder).
    public class GameFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-playtests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static IEnumerator WaitFrames(int n)
        {
            for (var i = 0; i < n; i++) yield return null;
        }

        static IEnumerator LoadScene(string name)
        {
            var op = SceneManager.LoadSceneAsync(name);
            while (!op.isDone) yield return null;
            yield return WaitFrames(3);
        }

        static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, string what)
        {
            var start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > timeoutSeconds) Assert.Fail($"Timed out waiting for: {what}");
                yield return null;
            }
        }

        static void Select(GameSession session, string itemId)
        {
            for (var i = 0; i < InputNames.HotbarSlots; i++)
            {
                var s = session.Backpack.Get(i);
                if (s != null && s.ItemId == itemId) { session.State.SelectedHotbar = i; return; }
            }
            Assert.Fail($"{itemId} not on the hotbar");
        }

        [UnityTest]
        public IEnumerator Farming_Loop_Till_Water_Plant_Sleep_Grow_Save_Load()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 1);
            yield return LoadScene(MapIds.Farm);

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindFirstObjectByType<PlayerActions>();
            Assert.IsNotNull(player);
            Assert.IsNotNull(actions);
            Assert.IsTrue(session.State.CurrentMap == MapIds.Farm);

            // Stand on open grass and face east: target cell is (21, 10).
            player.transform.position = new Vector3(20.5f, 10.1f, 0f);
            player.Face(Vector2Int.right);
            yield return null;
            var energyBefore = session.State.Energy;

            Select(session, ItemIds.Hoe);
            actions.UseSelected();
            var grid = session.GetGrid(MapIds.Farm);
            Assert.IsTrue(grid.IsTilled(21, 10), "hoe should till the tile in front");

            Select(session, ItemIds.WateringCan);
            actions.UseSelected();
            grid.TryGetTile(21, 10, out var tile);
            Assert.IsTrue(tile.Watered);

            Select(session, ItemIds.Seed("parsnip"));
            var seedsBefore = session.Backpack.Count(ItemIds.Seed("parsnip"));
            actions.UseSelected();
            Assert.IsNotNull(tile.Crop, "parsnip should be planted");
            Assert.AreEqual(seedsBefore - 1, session.Backpack.Count(ItemIds.Seed("parsnip")));
            Assert.AreEqual(energyBefore - PlayerActions.HoeEnergy - PlayerActions.WateringEnergy, session.State.Energy);

            // Overnight: the watered crop grows, energy refills, the date advances, and we wake up in bed.
            var summary = session.EndDay(false);
            Assert.IsNotNull(summary);
            Assert.AreEqual(1, tile.Crop.Stage, "watered crop should advance one stage overnight");
            Assert.AreEqual(2, session.Clock.Now.Day);
            Assert.AreEqual(session.State.MaxEnergy, session.State.Energy);
            Assert.AreEqual(MapIds.FarmHouse, session.State.CurrentMap);
            Assert.AreEqual(DayCycle.BedSpawn, session.State.SpawnPoint);

            // Save, drop the game, and load it back.
            Assert.IsTrue(session.Save());
            session.EndGame();
            Assert.IsTrue(session.BeginLoad(1, out var error), error);
            var loaded = session.GetGrid(MapIds.Farm);
            Assert.IsTrue(loaded.IsTilled(21, 10));
            loaded.TryGetTile(21, 10, out var loadedTile);
            Assert.AreEqual("parsnip", loadedTile.Crop.CropId);
            Assert.AreEqual(2, session.Clock.Now.Day);
        }

        [UnityTest]
        public IEnumerator Interact_Harvest_And_ShippingBin_Pays_Out()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            yield return LoadScene(MapIds.Farm);

            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var actions = UnityEngine.Object.FindFirstObjectByType<PlayerActions>();
            player.transform.position = new Vector3(20.5f, 10.1f, 0f);
            player.Face(Vector2Int.right);

            // A fully grown parsnip in front of the player.
            var grid = session.GetGrid(MapIds.Farm);
            grid.Till(21, 10);
            session.Db.TryGetCrop("parsnip", out var parsnip);
            grid.Plant(21, 10, parsnip, Season.Spring);
            grid.TryGetTile(21, 10, out var tile);
            tile.Crop.Stage = parsnip.MatureStage;

            var interact = typeof(PlayerActions).GetMethod("Interact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            interact.Invoke(actions, null);
            Assert.AreEqual(1, session.Backpack.Count("crop.parsnip"));
            Assert.IsNull(tile.Crop);

            // Ship it and sleep: gold goes up by the sell price.
            Select(session, "crop.parsnip");
            Assert.IsTrue(session.ShipSlot(session.State.SelectedHotbar));
            Assert.AreEqual(0, session.Backpack.Count("crop.parsnip"));
            var goldBefore = session.State.Gold;
            var result = session.EndDay(false);
            Assert.AreEqual(35, result.Earnings);
            Assert.AreEqual(goldBefore + 35, session.State.Gold);
        }

        [UnityTest]
        public IEnumerator Sleeping_Through_The_Ui_Shows_Summary_And_Wakes_In_Farmhouse()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 2);
            yield return LoadScene(MapIds.Farm);
            yield return WaitFrames(2);
            var ui = ServiceLocator.Get<IUiService>();

            session.StartSleep(false);
            yield return WaitUntil(() => ui.AnyModalOpen, 10f, "day summary screen");

            var cont = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(b => b.gameObject.activeInHierarchy && b.name == L.Get("ui.continue"));
            cont.onClick.Invoke();

            yield return WaitUntil(() => SceneManager.GetActiveScene().name == MapIds.FarmHouse && !session.IsSleeping, 10f, "farmhouse after sleeping");
            yield return WaitFrames(5);
            Assert.AreEqual(2, session.Clock.Now.Day);
            Assert.IsFalse(ui.AnyModalOpen);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var bedSpawn = UnityEngine.Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).First(s => s.Id == "bed");
            Assert.Less(Vector3.Distance(player.transform.position, bedSpawn.transform.position), 0.5f);
            Assert.IsTrue(session.Clock.IsPaused == false);
        }

        [UnityTest]
        public IEnumerator Warp_Between_Farm_And_Farmhouse_Keeps_State()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            yield return LoadScene(MapIds.Farm);
            session.GetGrid(MapIds.Farm).Till(30, 8);

            MapTravel.GoTo(MapIds.FarmHouse, "default");
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == MapIds.FarmHouse, 10f, "farmhouse scene");
            var loader = ServiceLocator.Get<SceneLoader>();
            yield return WaitUntil(() => !loader.IsLoading, 10f, "fade-in to finish");
            Assert.AreEqual(MapIds.FarmHouse, session.State.CurrentMap);

            MapTravel.GoTo(MapIds.Farm, "fromHouse");
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == MapIds.Farm, 10f, "farm scene");
            yield return WaitUntil(() => !loader.IsLoading, 10f, "fade-in to finish");
            Assert.IsTrue(session.GetGrid(MapIds.Farm).IsTilled(30, 8));
        }

        [UnityTest]
        public IEnumerator PassingOut_At_Two_Am_Triggers_Sleep()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            yield return LoadScene(MapIds.Farm);
            yield return WaitFrames(2);
            var ui = ServiceLocator.Get<IUiService>();

            session.Clock.AdvanceMinutes(GameDateTime.DayEndMinute);   // jump to 2:00 AM
            yield return WaitUntil(() => ui.AnyModalOpen, 10f, "summary after passing out");
            Assert.IsTrue(session.IsSleeping);

            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .First(b => b.gameObject.activeInHierarchy && b.name == L.Get("ui.continue")).onClick.Invoke();
            yield return WaitUntil(() => !session.IsSleeping && SceneManager.GetActiveScene().name == MapIds.FarmHouse, 10f, "wake up");
            Assert.AreEqual(2, session.Clock.Now.Day);
            Assert.AreEqual((int)(session.State.MaxEnergy * 0.75f), session.State.Energy);
        }

        [UnityTest]
        public IEnumerator MainMenu_Builds_Without_Errors_And_Continue_Is_Disabled_With_No_Saves()
        {
            yield return LoadScene(SceneNames.Bootstrap);
            yield return WaitUntil(() => SceneManager.GetActiveScene().name == SceneNames.MainMenu, 10f, "main menu");
            yield return WaitFrames(5);
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            var cont = buttons.First(b => b.name == L.Get("menu.continue"));
            Assert.IsFalse(cont.interactable);
            Assert.IsTrue(buttons.Any(b => b.name == L.Get("menu.new_game") && b.interactable));
        }
    }
}
