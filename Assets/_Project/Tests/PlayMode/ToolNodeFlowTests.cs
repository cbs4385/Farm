using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    // T-032 in the real game: the axe, pickaxe and scythe clear trees, rocks and weeds, driven by a simulated keyboard.
    public class ToolNodeFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-nodetests-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator StartFarm()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            // A clear patch to the east of the player, away from the starting clutter.
            var player = Player;
            player.transform.position = new Vector3(30.5f, 6.5f, 0f);
            player.Face(Vector2Int.right);
            var nodes = _session.GetNodes(MapIds.Farm);
            for (var dx = -3; dx <= 4; dx++)
                for (var dy = -3; dy <= 3; dy++) nodes.Remove(30 + dx, 6 + dy);
            View.RefreshAll();
            yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        static FarmMapView View => UnityEngine.Object.FindAnyObjectByType<FarmMapView>();

        static Vector3Int Target => new Vector3Int(31, 6, 0);

        void Place(string id, int x = 31, int y = 6)
        {
            _session.GetNodes(MapIds.Farm).Add(x, y, _session.Nodes.Get(id));
            View.RefreshNode(new Vector3Int(x, y, 0));
        }

        IEnumerator Tap(UnityEngine.InputSystem.Controls.ButtonControl button)
        {
            Press(button);
            yield return null;
            Release(button);
            yield return null;
            yield return null;
        }

        IEnumerator Swing() => Tap(_keyboard.cKey);

        // Hotbar order in a new game: hoe, watering can, axe, pickaxe, scythe.
        IEnumerator UseAxe() { yield return Tap(_keyboard.digit3Key); }
        IEnumerator UsePickaxe() { yield return Tap(_keyboard.digit4Key); }
        IEnumerator UseScythe() { yield return Tap(_keyboard.digit5Key); }

        // ---- clearing ---------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheScythe_CutsAWeed_ForFiberAndForagingXp()
        {
            yield return StartFarm();
            Place("weed");
            yield return UseScythe();
            var energy = _session.State.Energy;

            yield return Swing();

            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6), "the weed is gone");
            Assert.That(_session.Backpack.Count(ItemIds.Fiber), Is.InRange(1, 2));
            Assert.AreEqual(energy - PlayerActions.ScytheEnergy, _session.State.Energy, "a swing costs energy");
            Assert.AreEqual(1, _session.GetSkillXp(SkillIds.Foraging));
        }

        [UnityTest]
        public IEnumerator TheAxe_FellsATree_ThenClearsTheStump()
        {
            yield return StartFarm();
            Place("tree");
            yield return UseAxe();
            var energy = _session.State.Energy;

            for (var i = 0; i < 6; i++) yield return Swing();
            Assert.That(_session.Backpack.Count(ItemIds.Wood), Is.InRange(8, 12), "the tree gives wood");
            Assert.IsTrue(_session.GetNodes(MapIds.Farm).TryGet(31, 6, out var stump) && stump.TypeId == "stump", "a stump is left");
            Assert.AreEqual(energy - 6 * PlayerActions.AxeEnergy, _session.State.Energy);

            var before = _session.Backpack.Count(ItemIds.Wood);
            for (var i = 0; i < 3; i++) yield return Swing();
            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6), "the stump is cleared too");
            Assert.Greater(_session.Backpack.Count(ItemIds.Wood), before);
            Assert.AreEqual(12 + 4, _session.GetSkillXp(SkillIds.Foraging));
        }

        [UnityTest]
        public IEnumerator ThePickaxe_BreaksARock_ForStoneAndMiningXp()
        {
            yield return StartFarm();
            Place("rock");
            yield return UsePickaxe();
            yield return Swing();
            Assert.IsTrue(_session.GetNodes(MapIds.Farm).Has(31, 6), "the first blow only cracks it");
            yield return Swing();
            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6));
            Assert.That(_session.Backpack.Count(ItemIds.Stone), Is.InRange(1, 2));
            Assert.AreEqual(3, _session.GetSkillXp(SkillIds.Mining));
        }

        [UnityTest]
        public IEnumerator ACopperPickaxe_BreaksARockInOneBlow_AndCostsLess()
        {
            yield return StartFarm();
            _session.State.ToolTiers[ItemIds.Pickaxe] = 1;
            Place("rock");
            yield return UsePickaxe();
            var energy = _session.State.Energy;
            yield return Swing();
            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6));
            Assert.AreEqual(energy - ToolModel.EnergyCost(PlayerActions.PickaxeEnergy, 1), _session.State.Energy);
        }

        // ---- refusals ---------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheWrongTool_DoesNothing_AndCostsNothing()
        {
            yield return StartFarm();
            Place("weed");
            yield return UseAxe();
            var energy = _session.State.Energy;
            yield return Swing();
            Assert.IsTrue(_session.GetNodes(MapIds.Farm).Has(31, 6));
            Assert.AreEqual(energy, _session.State.Energy);
        }

        [UnityTest]
        public IEnumerator TheHoe_WillNotTillUnderAWeed()
        {
            yield return StartFarm();
            Place("weed");
            yield return Tap(_keyboard.digit1Key);
            yield return Swing();
            Assert.IsFalse(_session.GetGrid(MapIds.Farm).IsTilled(31, 6));
            CollectionAssert.Contains(_toasts, "Clear this first.");
        }

        [UnityTest]
        public IEnumerator ABoulder_NeedsABetterPickaxe()
        {
            yield return StartFarm();
            Place("boulder");
            yield return UsePickaxe();
            var energy = _session.State.Energy;
            yield return Swing();
            CollectionAssert.Contains(_toasts, "Your tool is not strong enough for this.");
            Assert.AreEqual(energy, _session.State.Energy);

            _session.State.ToolTiers[ItemIds.Pickaxe] = 1;
            for (var i = 0; i < 3; i++) yield return Swing();
            Assert.IsFalse(_session.GetNodes(MapIds.Farm).Has(31, 6), "three copper blows break it");
            Assert.That(_session.Backpack.Count(ItemIds.Stone), Is.InRange(4, 6));
        }

        [UnityTest]
        public IEnumerator WhenTooTired_TheSwingIsRefused()
        {
            yield return StartFarm();
            Place("rock");
            yield return UsePickaxe();
            _session.State.Energy = 1;
            yield return Swing();
            CollectionAssert.Contains(_toasts, "You are too tired.");
            Assert.IsTrue(_session.GetNodes(MapIds.Farm).Has(31, 6));
            Assert.AreEqual(1, _session.State.Energy);
        }

        [UnityTest]
        public IEnumerator AFullBackpack_RefusesTheFinalBlow_SoNothingIsLost()
        {
            yield return StartFarm();
            // Fill every free slot with something that is not wood.
            var fillers = new[] { "crop.parsnip", "crop.potato", "crop.cauliflower", "crop.greenbean", "crop.strawberry", "crop.kale" };
            foreach (var f in fillers) _session.Backpack.Add(f, 1);
            Assert.IsFalse(_session.Backpack.CanAdd(ItemIds.Wood, 12), "the pack really is full");

            Place("stump");
            yield return UseAxe();
            yield return Swing();
            yield return Swing();   // the stump has taken 2 of its 3 hit points
            var energy = _session.State.Energy;
            yield return Swing();   // the final blow would need room for the wood
            CollectionAssert.Contains(_toasts, "Your backpack is full.");
            Assert.IsTrue(_session.GetNodes(MapIds.Farm).Has(31, 6));
            Assert.AreEqual(energy, _session.State.Energy, "no energy spent on a refused blow");
        }

        // ---- solidity and the starting clutter --------------------------------------------------------------------

        [UnityTest]
        public IEnumerator ATree_BlocksTheWay_ButAWeedDoesNot()
        {
            yield return StartFarm();
            Place("tree", 32, 6);
            Place("weed", 31, 6);
            for (var i = 0; i < 3; i++) yield return null;
            Press(_keyboard.dKey);
            yield return new WaitForSeconds(1.5f);
            Release(_keyboard.dKey);
            yield return null;
            var x = Player.transform.position.x;
            Assert.Greater(x, 31.0f, "walked over the weed");
            Assert.Less(x, 32f, "stopped by the tree");
        }

        [UnityTest]
        public IEnumerator ANewFarm_StartsWithClutter_ButNotWhereThePlayerNeedsRoom()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;

            var nodes = _session.GetNodes(MapIds.Farm);
            Assert.That(nodes.Count, Is.InRange(80, 500), "trees, rocks and weeds (the farm is about twice as big as it was)");
            var kinds = nodes.Nodes.Select(n => n.TypeId).Distinct().ToList();
            CollectionAssert.IsSupersetOf(kinds, new[] { "weed", "rock", "tree" });

            // Nothing within three cells of the farmhouse door, the bin, or the spawn points.
            foreach (var n in nodes.Nodes)
            {
                Assert.Greater(Mathf.Max(Mathf.Abs(n.X - 7), Mathf.Abs(n.Y - 20)), 3, $"clutter at the door: {n.X},{n.Y}");
                Assert.Greater(Mathf.Max(Mathf.Abs(n.X - 13), Mathf.Abs(n.Y - 17)), 3, $"clutter at the bin: {n.X},{n.Y}");
            }
            Assert.IsTrue(_session.State.GetMap(MapIds.Farm).ClutterSeeded);

            // Entering the farm again does not pile on more.
            var count = nodes.Count;
            MapTravel.GoTo(MapIds.FarmHouse, "default");
            var start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.FarmHouse && Time.realtimeSinceStartup - start < 10f) yield return null;
            MapTravel.GoTo(MapIds.Farm, "fromHouse");
            start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.Farm && Time.realtimeSinceStartup - start < 10f) yield return null;
            for (var i = 0; i < 6; i++) yield return null;
            Assert.AreEqual(count, _session.GetNodes(MapIds.Farm).Count);
        }
    }
}
