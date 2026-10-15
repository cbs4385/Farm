using System.Collections;
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
    // Playtest 2026-10-15: the house's furniture goes into the backpack, and a villager on a bench sits on it instead of behind its back.
    public class Playtest1015FlowTests : PlayModeFixture
    {
        GameSession _s;

        IEnumerator Start(string map)
        {
            if ((_s = GameContext.Current?.Session) == null || !_s.InGame)
            {
                Bootstrapper.InitializeServices();
                yield return null;
                _s = GameContext.Current.Session;
                _s.BeginNewGame("Tester", "Test Farm", 0);
                _s.SetFlag(FatigueModel.WarnedFlag);
            }
            _s.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 8; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AHouseChair_GoesIntoTheBackpack_StaysGone_AndCanBeSetDownAgain()
        {
            yield return Start(MapIds.FarmHouse);
            var map = Object.FindAnyObjectByType<FarmMap>();
            var hammer = Object.FindAnyObjectByType<PlayerActions>().GetComponent<HammerMode>();
            var chair = Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_chairwest");
            var id = ItemIds.Machine("chair");
            var before = _s.Backpack.Count(id);

            hammer.Use(chair.Cell(map));
            Assert.IsFalse(hammer.Carrying, "it is in the backpack, not in the arms");
            Assert.AreEqual(before + 1, _s.Backpack.Count(id));
            Assert.IsFalse(chair.gameObject.activeSelf);

            yield return Start(MapIds.FarmHouse);
            var again = Object.FindObjectsByType<MovableFixture>(FindObjectsInactive.Include).First(f => f.Id == "furn_chairwest");
            Assert.IsFalse(again.gameObject.activeSelf, "still gone after leaving and coming back");

            var couch = Object.FindObjectsByType<MovableFixture>().First(f => f.Id == "furn_couch");
            Assert.IsNull(couch.ItemId, "a couch is wider than a cell: still carried");
        }

        [UnityTest]
        public IEnumerator AVillagerOnABench_IsDrawnOverIt_AndBackToNormalOnLeavingTheSeat()
        {
            yield return Start(MapIds.Village);
            var map = Object.FindAnyObjectByType<FarmMap>();
            var bench = SitSpot.All.First(b => b.name.Contains("Bench"));
            var cell = map.WorldToCell(bench.transform.position);
            var villager = new GameObject("Npc_visitor", typeof(SpriteRenderer)).AddComponent<NpcActor>();
            var renderer = villager.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = 9;

            villager.Apply(NpcPlacement.Standing(MapIds.Village, cell.x, cell.y, "down"), map, null);
            Assert.IsTrue(villager.Sitting, "standing beside the bench, the villager sits on it");
            Assert.Greater(renderer.sortingOrder, bench.SeatSortingOrder, "drawn over the bench");
            Assert.Less(renderer.sortingOrder, 9, "and under a villager on their feet, with no tie that could flicker");

            villager.Apply(NpcPlacement.Standing(MapIds.Village, cell.x, cell.y, NpcSchedule.SleepFacing), map, null);
            Assert.IsFalse(villager.Sitting);
            Assert.AreEqual(10, renderer.sortingOrder, "asleep: the usual order for a sleeper");
        }
    }
}
