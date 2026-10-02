using System;
using System.Collections;
using System.Collections.Generic;
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
    // T-037/T-038 in the real game: crafting from the menu, placing and using chests and machines, cooking in the kitchen,
    // the greenhouse door, and trees: all driven with a simulated keyboard and real button clicks.
    public class CraftingFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-crafttests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _toasts.Clear();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Start(string map = MapIds.Farm, string spawn = "default", Season season = Season.Spring)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            _session.Clock.SetTime(new GameDateTime(1, season, 10, 10 * 60));
            _session.State.SetDate(_session.Clock.Now);
            _session.State.CurrentMap = map;
            _session.State.SpawnPoint = spawn;
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        static UiService Ui => ServiceLocator.Get<UiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        // Stands the player on a cell, facing a direction, and selects the first backpack slot holding `itemId`.
        IEnumerator Face(int cellX, int cellY, Vector2Int facing, string selectItem = null)
        {
            Player.transform.position = new Vector3(cellX + 0.5f, cellY + 0.5f, 0f);
            Player.Face(facing);
            if (selectItem != null)
                _session.State.SelectedHotbar = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == selectItem);
            for (var i = 0; i < 4; i++) yield return null;
        }

        static Button Find(string parent, string name) =>
            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name == name && (parent == null || b.transform.parent.name == parent));

        // ---- crafting from the menu -------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator CraftingAChest_FromTheMenu_ClickingCraft()
        {
            yield return Start();
            _session.Backpack.Add(ItemIds.Wood, 60);
            yield return Tap(Key.M);
            while (Ui.GameMenu.Current.Id != MenuTabs.Crafting) yield return Tap(Key.E);

            var craft = Find("chest", "Craft");
            Assert.IsNotNull(craft, "the chest recipe is listed");
            Assert.IsTrue(craft.interactable, "and affordable");
            craft.onClick.Invoke();
            yield return null;

            Assert.AreEqual(1, _session.Backpack.Count("machine.chest"));
            Assert.AreEqual(10, _session.Backpack.Count(ItemIds.Wood));
            Assert.IsFalse(Find("chest", "Craft").interactable, "not enough wood for a second");
        }

        [UnityTest]
        public IEnumerator RecipesNeedingAHigherSkill_ShowAsLocked()
        {
            yield return Start();
            yield return Tap(Key.M);
            while (Ui.GameMenu.Current.Id != MenuTabs.Crafting) yield return Tap(Key.E);
            Assert.IsNull(Find("sprinkler1", "Craft"), "no Craft button while it is locked");
            _session.AddSkillXp(SkillIds.Farming, 100);
            yield return Tap(Key.Q);
            yield return Tap(Key.E);                       // back to the tab: it refreshes
            Assert.IsNotNull(Find("sprinkler1", "Craft"), "unlocked at farming level 2 (it is listed, though not affordable yet)");
        }

        // ---- placing, using and picking up ------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator AChest_CanBePlaced_Filled_AndPickedUpAgainWhenEmpty()
        {
            yield return Start();
            _session.Backpack.Add("machine.chest", 1);
            _session.Backpack.Add("crop.parsnip", 9);

            yield return Face(20, 8, Vector2Int.right, "machine.chest");      // faces (21, 8): open grass
            yield return Tap(Key.C);
            var placed = _session.GetObjects(MapIds.Farm).At(21, 8);
            Assert.IsNotNull(placed, "the chest is placed in front of the player");
            Assert.AreEqual(0, _session.Backpack.Count("machine.chest"));
            Assert.IsNotNull(PlacedObjectsView.Current.At(new Vector3Int(21, 8, 0)), "and drawn");

            // Using it opens the chest screen; clicking a pack slot moves the stack in.
            yield return Face(20, 8, Vector2Int.right);
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the chest screen opens");
            var slot = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == "crop.parsnip");
            Find(null, "PackSlot" + slot).onClick.Invoke();
            yield return null;
            Assert.AreEqual(0, _session.Backpack.Count("crop.parsnip"));
            Assert.AreEqual(9, _session.GetObjects(MapIds.Farm).ChestOf(placed).Count("crop.parsnip"));
            yield return Tap(Key.Escape);
            Assert.IsFalse(Ui.AnyModalOpen);

            // A full chest cannot be picked up; an empty one can, with the axe.
            yield return Face(20, 8, Vector2Int.right, ItemIds.Axe);
            yield return Tap(Key.C);
            Assert.IsTrue(_session.GetObjects(MapIds.Farm).Has(21, 8), "it still holds parsnips");
            _session.GetObjects(MapIds.Farm).ChestOf(placed).Remove("crop.parsnip", 9);
            yield return Tap(Key.C);
            Assert.IsFalse(_session.GetObjects(MapIds.Farm).Has(21, 8));
            Assert.AreEqual(1, _session.Backpack.Count("machine.chest"));
        }

        [UnityTest]
        public IEnumerator ThingsCannotBePlaced_OnTilledSoil_OnOtherObjects_OrIndoorsIfFarmingOnly()
        {
            yield return Start();
            _session.Backpack.Add("machine.chest", 2);
            _session.Backpack.Add("machine.sprinkler1", 1);
            _session.GetGrid(MapIds.Farm).Till(21, 8);
            yield return Face(20, 8, Vector2Int.right, "machine.chest");
            yield return Tap(Key.C);
            Assert.IsFalse(_session.GetObjects(MapIds.Farm).Has(21, 8), "not on tilled soil");

            yield return Face(20, 9, Vector2Int.right, "machine.chest");
            yield return Tap(Key.C);
            Assert.IsTrue(_session.GetObjects(MapIds.Farm).Has(21, 9));
            yield return Tap(Key.C);
            Assert.AreEqual(1, _session.Backpack.Count("machine.chest"), "the second chest was not put on top of the first");
        }

        [UnityTest]
        public IEnumerator AKeg_TakesAnIngredient_AndGivesJuiceTwoDaysLater()
        {
            yield return Start();
            _session.Backpack.Add("machine.keg", 1);
            _session.Backpack.Add("crop.parsnip", 2);
            yield return Face(20, 8, Vector2Int.right, "machine.keg");
            yield return Tap(Key.C);
            var keg = _session.GetObjects(MapIds.Farm).At(21, 8);
            Assert.IsNotNull(keg);

            yield return Face(20, 8, Vector2Int.right, "crop.parsnip");
            yield return Tap(Key.E);
            yield return null;
            Assert.AreEqual(1, _session.Backpack.Count("crop.parsnip"), "one parsnip went in");
            Assert.IsTrue(CraftingRules.IsBusy(keg));

            yield return Tap(Key.E);                                         // too early
            yield return null;
            Assert.AreEqual(0, _session.Backpack.Count("artisan.juice"));
            Assert.That(_toasts, Has.Some.Contains("Ready in"));

            // Two days pass (the real overnight code), then the keg is ready.
            _session.Clock.SetTime(new GameDateTime(1, Season.Spring, 12, 10 * 60));
            yield return Tap(Key.E);
            yield return null;
            Assert.AreEqual(1, _session.Backpack.Count("artisan.juice"), "the juice is collected");
            Assert.IsFalse(CraftingRules.IsBusy(keg));
        }

        // ---- the kitchen -----------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheKitchen_CooksADish_FromTheBackpack()
        {
            yield return Start(MapIds.FarmHouse);
            _session.Backpack.Add("crop.kale", 1);
            _session.Backpack.Add("crop.parsnip", 1);
            _session.Backpack.Add("crop.potato", 1);                          // not enough for the mash (needs two)
            yield return Face(9, 6, Vector2Int.up);
            yield return Tap(Key.E);
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(Ui.AnyModalOpen, "the cooking list opens");

            Assert.IsTrue(Find("cook_salad", "Craft").interactable);
            Assert.IsFalse(Find("cook_mashed_potato", "Craft").interactable, "one potato is not enough");
            Find("cook_salad", "Craft").onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, _session.Backpack.Count("food.salad"));
            Assert.AreEqual(0, _session.Backpack.Count("crop.kale"));

            // Eating restores energy.
            _session.SetEnergy(100);
            yield return Tap(Key.Escape);
            _session.State.SelectedHotbar = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == "food.salad");
            yield return Tap(Key.C);
            Assert.AreEqual(140, _session.State.Energy);
            Assert.AreEqual(0, _session.Backpack.Count("food.salad"));
        }

        // ---- the greenhouse --------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheGreenhouseDoor_IsLockedUntilBuilt_ThenLeadsToAFarmingRoom()
        {
            yield return Start();
            var warp = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == MapIds.Greenhouse);
            Player.transform.position = warp.transform.position;
            var wait = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - wait < 0.5f) yield return null;        // a few physics steps
            Assert.AreEqual(MapIds.Farm, SceneManager.GetActiveScene().name, "the door is locked");
            Assert.That(_toasts, Has.Some.Contains("greenhouse"));

            _session.SetFlag(MapIds.GreenhouseFlag);
            Player.transform.position = warp.transform.position + Vector3.down * 2f;
            wait = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - wait < 0.3f) yield return null;
            Player.transform.position = warp.transform.position;
            var start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.Greenhouse && Time.realtimeSinceStartup - start < 10f) yield return null;
            for (var i = 0; i < 6; i++) yield return null;
            Assert.AreEqual(MapIds.Greenhouse, SceneManager.GetActiveScene().name, "the built greenhouse opens");
            Assert.IsTrue(UnityEngine.Object.FindFirstObjectByType<FarmMap>().AllowFarming);
        }

        [UnityTest]
        public IEnumerator InTheGreenhouse_APumpkinCanBePlantedInWinter()
        {
            yield return Start(MapIds.Greenhouse, "default", Season.Winter);
            _session.Backpack.Add("seed.pumpkin", 1);
            _session.State.SelectedHotbar = Enumerable.Range(0, _session.Backpack.Capacity).First(i => _session.Backpack.Get(i)?.ItemId == ItemIds.Hoe);
            yield return Face(7, 4, Vector2Int.up);
            yield return Tap(Key.C);                                           // till (7, 5)
            Assert.IsTrue(_session.GetGrid(MapIds.Greenhouse).IsTilled(7, 5));
            yield return Face(7, 4, Vector2Int.up, "seed.pumpkin");
            yield return Tap(Key.C);
            _session.GetGrid(MapIds.Greenhouse).TryGetTile(7, 5, out var tile);
            Assert.IsNotNull(tile.Crop, "pumpkins (a fall crop) grow in the winter greenhouse");
        }

        // ---- trees -----------------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator ASapling_GoesInTilledSoilInAnySeason_AndBearsFruitToPick()
        {
            yield return Start(MapIds.Farm, "default", Season.Winter);
            _session.Backpack.Add("seed.tree_apple", 1);
            _session.GetGrid(MapIds.Farm).Till(21, 8);
            yield return Face(20, 8, Vector2Int.right, "seed.tree_apple");
            yield return Tap(Key.C);
            _session.GetGrid(MapIds.Farm).TryGetTile(21, 8, out var tile);
            Assert.IsNotNull(tile.Crop, "saplings are planted in winter");

            tile.Crop.Stage = 4;                                               // fully grown, in fruit
            tile.Crop.Fruit = true;
            yield return Face(20, 8, Vector2Int.right, ItemIds.Hoe);
            yield return Tap(Key.E);
            Assert.AreEqual(1, _session.Backpack.Count("crop.tree_apple"), "the fruit is picked");
            Assert.IsNotNull(tile.Crop, "the tree stays");
        }
    }
}
