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
    // Playtest report (2026-10-15): "when going back up the mine, there is nothing there, not even the ore I didn't mine."
    public class MineRevisitFlowTests
    {
        string _root;
        GameSession _s;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-minerevisit-" + Guid.NewGuid().ToString("N"));
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

        IEnumerator WaitForFloor(int floor)
        {
            var until = Time.realtimeSinceStartup + 15f;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
                var mine = UnityEngine.Object.FindAnyObjectByType<MineController>();
                if (!_s.Game.Scenes.IsLoading && mine != null && mine.Floor != null && mine.Floor.Floor == floor) { for (var k = 0; k < 5; k++) yield return null; yield break; }
            }
            Assert.Fail("floor " + floor + " never loaded");
        }

        [UnityTest]
        public IEnumerator AFloorComesBackAsItWasLeft_AfterGoingDownAndUpAgain()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.State.WorldSeed = 4242;
            _s.State.CurrentMap = MapIds.Mine;
            _s.State.SpawnPoint = "default";
            var day = _s.Clock.Now.TotalDays;
            var floor = Enumerable.Range(2, 30).OrderByDescending(f => MineGenerator.Generate(MineGenerator.FloorSeed(4242, day, f), f).Nodes.Count).First();          // a floor with plenty of ore
            _s.State.Mine.Floor = floor;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            yield return WaitForFloor(floor);

            var nodes = _s.GetNodes(MapIds.Mine);
            var all = nodes.ToList();
            Assert.Greater(all.Count, 3, "the floor has ore to mine");
            var walls = (bool[])UnityEngine.Object.FindAnyObjectByType<MineController>().Floor.Wall.Clone();
            foreach (var gone in all.Take(2)) nodes.Remove(gone.X, gone.Y);          // two mined
            var expected = nodes.ToList().Select(n => (n.X, n.Y, n.TypeId)).OrderBy(t => t).ToList();

            MineTravel.GoToFloor(_s, floor + 1);
            yield return WaitForFloor(floor + 1);
            MineTravel.GoToFloor(_s, floor, fromBelow: true);
            yield return WaitForFloor(floor);

            var back = _s.GetNodes(MapIds.Mine).ToList().Select(n => (n.X, n.Y, n.TypeId)).OrderBy(t => t).ToList();
            CollectionAssert.AreEqual(expected, back, "the unmined ore is still there and the mined ore still gone");
            CollectionAssert.AreEqual(walls, UnityEngine.Object.FindAnyObjectByType<MineController>().Floor.Wall, "the layout is the same");
        }

        // Playtest 2026-10-15: "the mine does not survive game reloads; mine levels should remain the same until the player exits the mine."
        [UnityTest]
        public IEnumerator AGameSavedInTheMine_ComesBackWithTheSameFloorOreMonstersAndSpot()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.State.WorldSeed = 4242;
            _s.State.CurrentMap = MapIds.Mine;
            _s.State.SpawnPoint = "default";
            var day = _s.Clock.Now.TotalDays;
            var floor = Enumerable.Range(2, 30).OrderByDescending(f => MineGenerator.Generate(MineGenerator.FloorSeed(4242, day, f), f).Nodes.Count).First();
            _s.State.Mine.Floor = floor;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            yield return WaitForFloor(floor);

            var mine = UnityEngine.Object.FindAnyObjectByType<MineController>();
            var nodes = _s.GetNodes(MapIds.Mine);
            foreach (var gone in nodes.ToList().Take(2)) nodes.Remove(gone.X, gone.Y);                      // two mined
            mine.RecordDead(0);                                                                           // a monster killed
            var expected = nodes.ToList().Select(n => (n.X, n.Y, n.TypeId)).OrderBy(t => t).ToList();
            var walls = (bool[])mine.Floor.Wall.Clone();
            var map = _s.Game.Map.Map;
            var spot = map.CellCenter(map.WorldToCell(_s.Game.Map.Player.transform.position) + new Vector3Int(1, 0, 0));
            _s.Game.Map.Player.transform.position = spot;
            yield return null;

            Assert.IsTrue(_s.Save());
            Assert.IsTrue(_s.BeginLoad(0, out var error), error);                                         // the game is loaded from the file, as after a restart
            Assert.AreEqual(floor, _s.State.Mine.Floor);
            op = SceneManager.LoadSceneAsync(_s.State.CurrentMap);
            while (!op.isDone) yield return null;
            yield return WaitForFloor(floor);

            var back = _s.GetNodes(MapIds.Mine).ToList().Select(n => (n.X, n.Y, n.TypeId)).OrderBy(t => t).ToList();
            CollectionAssert.AreEqual(expected, back, "the same ore is left: the mined ore is still gone, the rest is still there");
            CollectionAssert.AreEqual(walls, UnityEngine.Object.FindAnyObjectByType<MineController>().Floor.Wall, "the same layout");
            CollectionAssert.Contains(_s.State.Mine.MemoryOf(floor).Dead, 0, "the killed monster is still dead");
            Assert.AreEqual(spot.x, _s.Game.Map.Player.transform.position.x, 0.05f, "and the player is where the save was made");
            Assert.AreEqual(spot.y, _s.Game.Map.Player.transform.position.y, 0.05f);
        }
    }
}
