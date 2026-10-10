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
    public class ActorOverlapFlowTests : PlayModeFixture
    {
        GameSession _s;

        [SetUp]
        public void SetUpMore()
        {
            CellOccupants.ResetForTests();
        }

        [TearDown]
        public void TearDownMore()
        {
            CellOccupants.ResetForTests();
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

        [UnityTest]
        public IEnumerator AVillagerIsASolidBody_TheTileIsNotPlaceable_ButTheRouteGridStillSeesFloor()
        {
            yield return Start(MapIds.Village, 12);
            var npcs = UnityEngine.Object.FindAnyObjectByType<NpcManager>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var cell = new Vector3Int(10, 17, 0);
            Assert.IsTrue(map.CanPlaceAt(cell), "the road is free");
            var grid = npcs.Grid();
            Assert.IsTrue(grid.IsWalkable(cell.x, cell.y));

            var actor = npcs.Take("tilda", cell);
            Assert.IsNotNull(actor);
            Physics2D.SyncTransforms();
            Assert.IsFalse(map.CanPlaceAt(cell), "nothing can be set down on a villager");
            var solid = false;
            foreach (var hit in Physics2D.OverlapPointAll(map.CellCenter(cell))) if (!hit.isTrigger && hit.GetComponentInParent<NpcActor>() == actor) solid = true;
            Assert.IsTrue(solid, "the villager has a solid body the player cannot walk through");

            var other = new WalkGrid(0, 0, 3, 1, (x, y) => true);
            Assert.IsNotNull(other.FindPath(0, 0, 2, 0));
        }

        [UnityTest]
        public IEnumerator AClickedVillager_StandsStill_ThenCatchesUpToTheSchedule()
        {
            yield return Start(MapIds.Village, 12);
            var npcs = UnityEngine.Object.FindAnyObjectByType<NpcManager>();
            for (var hour = 8; hour <= 21 && npcs.Actors.Count == 0; hour++)          // find a time when somebody is out in the village
            {
                _s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, hour * 60));
                for (var i = 0; i < 4; i++) yield return null;
            }
            Assert.Greater(npcs.Actors.Count, 0, "somebody is in the village at some hour");
            var actor = System.Linq.Enumerable.First(npcs.Actors.Values);
            var id = actor.Definition.Id;

            npcs.Hold(id, 60f);
            yield return null;
            var where = actor.transform.position;
            _s.Clock.AdvanceMinutes(3);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.AreEqual(where.x, actor.transform.position.x, 0.01f, "held: it did not move");
            Assert.AreEqual(where.y, actor.transform.position.y, 0.01f);
            Assert.GreaterOrEqual(npcs.LagMinutes(id), 2.5f, "and it is now behind the schedule");

            npcs.Hold(id, 0f);                                          // released
            var before = npcs.LagMinutes(id);
            _s.Clock.AdvanceMinutes(1);
            for (var i = 0; i < 6; i++) yield return null;
            Assert.Less(npcs.LagMinutes(id), before, "it catches up once released");
        }
    }
}
