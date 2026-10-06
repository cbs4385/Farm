using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest report: villagers and animals could share a tile. With real scenes, nobody ends up sharing and whoever can move steps aside.
    public class ActorOverlapFlowTests
    {
        string _root;
        GameSession _s;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-overlap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            CellOccupants.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            CellOccupants.ResetForTests();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator Start(string map, int hour)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.Story = new StoryContent();
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, hour * 60));
            _s.State.SetDate(_s.Clock.Now);
            _s.State.GetMap(map).LastSpawnDay = _s.Clock.Now.TotalDays;
            _s.State.CurrentMap = map;
            _s.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TwoAnimalsSpawnedOnOneCell_EndUpOnTwoCells_AndNeverShareAgainWhileWandering()
        {
            yield return Start(MapIds.Coop, 12);
            _s.SetFlag(AnimalRules.BuildingFlag(MapIds.Coop));
            var manager = AnimalManager.Current;
            Assert.IsNotNull(manager);
            var a = manager.Spawn(AnimalRules.Add(_s.State, "chicken", "a1", "Ann"), 4, 4);
            var b = manager.Spawn(AnimalRules.Add(_s.State, "chicken", "a2", "Bea"), 4, 4);
            var c = manager.Spawn(AnimalRules.Add(_s.State, "chicken", "a3", "Cy"), 4, 4);
            yield return null;
            Assert.AreNotEqual(a.Cell, b.Cell);
            Assert.AreNotEqual(a.Cell, c.Cell);
            Assert.AreNotEqual(b.Cell, c.Cell);

            var end = Time.realtimeSinceStartup + 20f;
            var shared = 0;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
                if (a.Cell == b.Cell || a.Cell == c.Cell || b.Cell == c.Cell) shared++;
            }
            Assert.AreEqual(0, shared, "frames in which two animals shared a cell");
        }

        [UnityTest]
        public IEnumerator AVillagerWhoArrivesOnAnAnimal_MakesItStepAside()
        {
            yield return Start(MapIds.Coop, 12);
            _s.SetFlag(AnimalRules.BuildingFlag(MapIds.Coop));
            var animal = AnimalManager.Current.Spawn(AnimalRules.Add(_s.State, "chicken", "a1", "Ann"), 4, 4);
            yield return null;

            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var visitor = new GameObject("Npc_visitor", typeof(SpriteRenderer)).AddComponent<NpcActor>();
            visitor.SetCell(map, animal.Cell);                         // a villager is suddenly standing exactly where the animal is
            Assert.AreEqual(1, CellOccupants.SharingWith(animal));

            var end = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < end && animal.Cell == visitor.Cell) yield return null;
            Assert.AreNotEqual(visitor.Cell, animal.Cell, "the animal moved off the villager cell");
            Assert.AreEqual(0, CellOccupants.SharingWith(animal));
        }

        [UnityTest]
        public IEnumerator TheVillageCat_StepsOffAVillagerWhoStandsOnIt_AndNeverWalksOntoOne()
        {
            yield return Start(MapIds.Village, 12);
            var cat = UnityEngine.Object.FindAnyObjectByType<VillageCat>();
            Assert.IsNotNull(cat);
            for (var i = 0; i < 30 && !cat.IsShown; i++) yield return null;
            Assert.IsTrue(cat.IsShown, "the cat is out by day");

            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var visitor = new GameObject("Npc_visitor", typeof(SpriteRenderer)).AddComponent<NpcActor>();
            visitor.SetCell(map, cat.Cell);
            var end = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < end && cat.Cell == visitor.Cell) yield return null;
            Assert.AreNotEqual(visitor.Cell, cat.Cell, "the cat hopped off");

            // And with a villager standing still, the cat never ends up on that cell however long it wanders.
            end = Time.realtimeSinceStartup + 25f;
            var shared = 0;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
                if (cat.Cell == visitor.Cell) shared++;
            }
            Assert.AreEqual(0, shared, "frames in which the cat stood on the villager cell");
        }
    }
}
