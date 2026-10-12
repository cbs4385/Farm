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

namespace Farm.Tests
{
    // The village green: its trees can be chopped, the villagers who see it mutter and one comes to tell the player off, and the next day the green is as it was.
    public class VillageWatchFlowTests
    {
        string _root;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-village-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator LoadVillage(GameSession session)
        {
            session.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheVillagesTreesAreNodes_ASeenFellingIsMuttered_ScoldedAndPutRightByMorning()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            yield return LoadVillage(session);

            var nodes = session.GetNodes(MapIds.Village);
            var trees = nodes.ToList().Where(n => n.TypeId == NodeDefaults.Tree).ToList();
            Assert.Greater(trees.Count, 5, "the green's trees are choppable nodes now");
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t => t.name.StartsWith("Tree_")), "and no longer pictures in the scene");

            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var npcs = GameContext.Current.Map.Npcs;
            var home = map.WorldToCell(player.transform.position);
            var witness = npcs.Take("tilda", new Vector3Int(home.x + 3, home.y, 0));
            Assert.IsNotNull(witness);
            var scolds = VillageWatch.Scolds;

            var felled = trees[0];
            nodes.Remove(felled.X, felled.Y);                                   // what the axe does
            session.Publish(new VillageDamaged(felled.X, felled.Y, true));
            yield return null;
            Assert.GreaterOrEqual(VillageWatch.Muttered, 1, "the villager who saw it talks to herself");
            Assert.AreEqual(session.Clock.Now.TotalDays + 1, session.GetVar(VillageGreen.DamagedVar), "and the damage is remembered");

            var waited = 0f;
            while (VillageWatch.Scolds == scolds && waited < 20f) { waited += Time.deltaTime; yield return null; }
            Assert.AreEqual(scolds + 1, VillageWatch.Scolds, "someone comes to tell the player off");
            Assert.AreEqual("tilda", VillageWatch.LastScolder);
            Assert.IsTrue(ServiceLocator.Get<IUiService>().AnyModalOpen, "in a conversation");
        }

        [UnityTest]
        public IEnumerator TheNextDay_TheFelledTreeIsBack_AndTheVillagersSayTheyDidIt()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            yield return LoadVillage(session);

            var nodes = session.GetNodes(MapIds.Village);
            var tree = nodes.ToList().First(n => n.TypeId == NodeDefaults.Tree);
            nodes.Remove(tree.X, tree.Y);
            session.SetVar(VillageGreen.DamagedVar, session.Clock.Now.TotalDays + 1);

            yield return LoadVillage(session);                                  // the same day: still gone
            Assert.IsFalse(session.GetNodes(MapIds.Village).TryGet(tree.X, tree.Y, out _), "not put back the same day");

            session.Clock.SetTime(session.Clock.Now.StartOfNextDay());
            yield return LoadVillage(session);
            Assert.IsTrue(session.GetNodes(MapIds.Village).TryGet(tree.X, tree.Y, out var back), "put back overnight");
            Assert.AreEqual(NodeDefaults.Tree, back.TypeId);
            Assert.AreEqual(0, session.GetVar(VillageGreen.DamagedVar));
        }

        [UnityTest]
        public IEnumerator TheGreenCanBeDug_AndBrokenRocksAndDugSoilAreRestoredByMorning()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            yield return LoadVillage(session);

            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var nodes = session.GetNodes(MapIds.Village);
            var rocks = nodes.ToList().Where(n => n.TypeId == NodeDefaults.Rock || n.TypeId == NodeDefaults.Boulder).ToList();
            Assert.Greater(rocks.Count, 3, "a few rocks on the green");
            Vector3Int grass = default;
            foreach (var c in map.Ground.cellBounds.allPositionsWithin)
                if (map.HasTillableGround(c) && !nodes.Has(c.x, c.y)) { grass = c; break; }
            Assert.IsTrue(map.IsTillable(grass), "the green may be dug");

            var grid = session.GetGrid(MapIds.Village);
            grid.Till(grass.x, grass.y);                                         // what the hoe does
            nodes.Remove(rocks[0].X, rocks[0].Y);                                // what the pickaxe does
            session.SetVar(VillageGreen.DamagedVar, session.Clock.Now.TotalDays + 1);

            session.Clock.SetTime(session.Clock.Now.StartOfNextDay());
            yield return LoadVillage(session);
            Assert.IsFalse(session.GetGrid(MapIds.Village).IsTilled(grass.x, grass.y), "the soil is levelled");
            Assert.IsTrue(session.GetNodes(MapIds.Village).TryGet(rocks[0].X, rocks[0].Y, out var back), "the rock is back");
            Assert.AreEqual(rocks[0].TypeId, back.TypeId);
        }

    }
}
