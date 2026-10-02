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
    // T-033 in the real game: things grow on the maps, and the player picks them up with the Interact key.
    public class ForageFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        readonly List<string> _toasts = new List<string>();

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-foragetests-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator Start(string map, Season season)
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
            _session.State.SpawnPoint = "default";
            ServiceLocator.Get<EventBus>().Subscribe<ToastRequested>(t => _toasts.Add(t.Message));
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        static FarmMapView View => UnityEngine.Object.FindFirstObjectByType<FarmMapView>();

        IEnumerator Interact()
        {
            Press(_keyboard.eKey);
            yield return null;
            Release(_keyboard.eKey);
            for (var i = 0; i < 3; i++) yield return null;
        }

        // Puts forage on the cell east of a spot on the map and stands the player there facing it.
        void Prepare(string nodeId, int px = 10, int py = 12)
        {
            _session.GetNodes(_session.State.CurrentMap).Remove(px + 1, py);
            _session.GetNodes(_session.State.CurrentMap).Add(px + 1, py, _session.Nodes.Get(nodeId));
            View.RefreshNode(new Vector3Int(px + 1, py, 0));
            Player.transform.position = new Vector3(px + 0.5f, py + 0.5f, 0f);
            Player.Face(Vector2Int.right);
        }

        // ---- the maps grow things ----------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator TheForest_HasForageOnItsFloor_ClearOfThePathAndTheEntrance()
        {
            yield return Start(MapIds.Forest, Season.Fall);
            var nodes = _session.GetNodes(MapIds.Forest);
            Assert.Greater(nodes.Count, 5, "something is growing in the wood");
            foreach (var n in nodes.Nodes)
            {
                Assert.IsTrue(_session.Nodes.Get(n.TypeId).Tool == ToolType.None, "only things to pick");
                Assert.Greater(Mathf.Max(Mathf.Abs(n.X - 19), Mathf.Abs(n.Y - 2)), 2, $"clear of the arrival point: {n.X},{n.Y}");
            }
        }

        [UnityTest]
        public IEnumerator TheBeach_HasShells()
        {
            yield return Start(MapIds.Beach, Season.Summer);
            var kinds = _session.GetNodes(MapIds.Beach).Nodes.Select(n => n.TypeId).Distinct().ToList();
            Assert.IsNotEmpty(kinds);
            CollectionAssert.IsSubsetOf(kinds, new[] { "seashell", "clam", "pearl" });
        }

        [UnityTest]
        public IEnumerator ComingBackTheSameDay_AddsNothing()
        {
            yield return Start(MapIds.Forest, Season.Fall);
            var count = _session.GetNodes(MapIds.Forest).Count;
            MapTravel.GoTo(MapIds.Village, "fromForest");
            var start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.Village && Time.realtimeSinceStartup - start < 10f) yield return null;
            MapTravel.GoTo(MapIds.Forest, "fromVillage");
            start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != MapIds.Forest && Time.realtimeSinceStartup - start < 10f) yield return null;
            for (var i = 0; i < 6; i++) yield return null;
            Assert.AreEqual(count, _session.GetNodes(MapIds.Forest).Count);
        }

        // ---- picking it up ------------------------------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Interact_PicksUpForage_ForTheItemAndForagingXp()
        {
            yield return Start(MapIds.Forest, Season.Fall);
            Prepare("hazelnut");
            yield return null;
            var energy = _session.State.Energy;

            yield return Interact();

            Assert.AreEqual(1, _session.Backpack.Count("forage.hazelnut"));
            Assert.IsFalse(_session.GetNodes(MapIds.Forest).Has(11, 12), "it is gone from the ground");
            Assert.AreEqual(7, _session.GetSkillXp(SkillIds.Foraging));
            Assert.AreEqual(energy, _session.State.Energy, "picking things up is free");
        }

        [UnityTest]
        public IEnumerator AFullBackpack_LeavesTheForageWhereItIs()
        {
            yield return Start(MapIds.Forest, Season.Fall);
            var fillers = new[] { "crop.parsnip", "crop.potato", "crop.cauliflower", "crop.greenbean", "crop.strawberry", "crop.kale" };
            foreach (var f in fillers) _session.Backpack.Add(f, 1);
            Prepare("mushroom");
            yield return null;

            yield return Interact();

            CollectionAssert.Contains(_toasts, "Your backpack is full.");
            Assert.IsTrue(_session.GetNodes(MapIds.Forest).Has(11, 12));
            Assert.AreEqual(0, _session.Backpack.Count("forage.mushroom"));
        }

        [UnityTest]
        public IEnumerator ForagingSkill_RaisesTheQuality_OfWhatIsFound()
        {
            yield return Start(MapIds.Forest, Season.Fall);
            _session.AddSkillXp(SkillIds.Foraging, 5000);   // level 10
            var found = new List<int>();
            for (var i = 0; i < 30; i++)
            {
                _session.Clock.SetTime(new GameDateTime(1, Season.Fall, 10 + i % 15, 10 * 60));
                Prepare("seashell", 10 + i % 5, 12 + i % 7);
                yield return null;
                yield return Interact();
            }
            foreach (var stack in Enumerable.Range(0, _session.Backpack.Capacity).Select(_session.Backpack.Get).Where(s => s != null && s.ItemId == "forage.seashell"))
                found.Add(stack.Quality);
            Assert.IsNotEmpty(found);
            Assert.IsTrue(found.Any(q => q > 0), "a level-10 forager sometimes finds better quality");
        }
    }
}
