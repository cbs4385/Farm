using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest report: villagers and animals could stand on the same tile, which made it hard to choose what to interact with.
    // At most one villager, cat or animal may occupy a tile.
    public class ActorOverlapTests
    {
        TestSessionFixture _f;

        [SetUp]
        public void SetUp()
        {
            CellOccupants.ResetForTests();
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            _f = new TestSessionFixture(db.AllItems, null, NpcCatalog.From(db).All);
        }

        [TearDown]
        public void TearDown()
        {
            CellOccupants.ResetForTests();
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        // ---- the schedules ----

        [Test]
        public void NoTwoVillagers_StandOnTheSameTile_AtAnyTime_OnAnyDay()
        {
            var s = _f.Session;
            var clash = new SortedSet<string>();
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter })
                foreach (var day in new[] { 1, 2, 3, 4, 5, 6, 7, 15, 16, 22 })
                    foreach (var weather in new[] { WeatherIds.Sunny, WeatherIds.Rain })
                    {
                        s.State.Weather = weather;
                        s.Clock.SetTime(new GameDateTime(2, season, day, 12 * 60));
                        var plans = s.Npcs.All.ToDictionary(n => n.Id, n => NpcSchedule.PlanFor(n, s.World, s.Hooks.ScheduleEntriesFor(n)));
                        for (var minute = GameDateTime.DayStartMinute; minute < GameDateTime.DayEndMinute; minute += 5)
                        {
                            var standing = new Dictionary<(string, int, int), string>();
                            foreach (var npc in s.Npcs.All)
                            {
                                var p = NpcSchedule.Where(npc, plans[npc.Id], minute);
                                if (p.Walking) continue;
                                var key = (p.Map, p.ToX, p.ToY);
                                if (standing.TryGetValue(key, out var other))
                                {
                                    var pair = string.CompareOrdinal(other, npc.Id) < 0 ? other + "+" + npc.Id : npc.Id + "+" + other;
                                    clash.Add($"{pair} at {p.Map} ({p.ToX},{p.ToY}) ({plans[npc.Id].Id}/{plans[other].Id})");
                                }
                                else standing[key] = npc.Id;
                            }
                        }
                    }
            Assert.IsEmpty(clash, "villagers sharing a tile:\n" + string.Join("\n", clash));
        }

        // ---- the occupancy rules (pure) ----

        sealed class Spot : ICellOccupant
        {
            public Vector3Int Cell { get; set; }
            public bool IsPlaced { get; set; } = true;
            public Vector3Int? Claim { get; set; }
        }

        [Test]
        public void ACellIsTaken_ByAnOccupantStandingOnIt_OrWalkingIntoIt_ButNotByItself()
        {
            var a = new Spot { Cell = new Vector3Int(3, 3, 0) };
            var b = new Spot { Cell = new Vector3Int(5, 5, 0), Claim = new Vector3Int(6, 5, 0) };
            CellOccupants.Add(a);
            CellOccupants.Add(b);
            Assert.IsTrue(CellOccupants.IsTaken(new Vector3Int(3, 3, 0)));
            Assert.IsFalse(CellOccupants.IsTaken(new Vector3Int(3, 3, 0), a), "not by itself");
            Assert.IsTrue(CellOccupants.IsTaken(new Vector3Int(6, 5, 0)), "held by the one walking into it");
            Assert.IsFalse(CellOccupants.IsTaken(new Vector3Int(4, 4, 0)));
            b.IsPlaced = false;
            Assert.IsFalse(CellOccupants.IsTaken(new Vector3Int(5, 5, 0)), "an actor that has no position yet holds nothing");
        }

        [Test]
        public void SharingWith_CountsTheOthersOnTheSameCell()
        {
            var a = new Spot { Cell = new Vector3Int(1, 1, 0) };
            var b = new Spot { Cell = new Vector3Int(1, 1, 0) };
            var c = new Spot { Cell = new Vector3Int(2, 1, 0) };
            foreach (var s in new[] { a, b, c }) CellOccupants.Add(s);
            Assert.AreEqual(1, CellOccupants.SharingWith(a));
            Assert.AreEqual(0, CellOccupants.SharingWith(c));
        }

        [Test]
        public void TryFindFree_ReturnsTheNearestFloorNobodyHolds_AndFailsWhenBoxedIn()
        {
            var mid = new Vector3Int(5, 5, 0);
            var me = new Spot { Cell = mid };
            CellOccupants.Add(me);
            CellOccupants.Add(new Spot { Cell = mid + Vector3Int.up });
            Assert.IsTrue(CellOccupants.TryFindFree(mid, c => true, me, out var free));
            Assert.AreNotEqual(mid, free);
            Assert.AreEqual(1, Mathf.Abs(free.x - mid.x) + Mathf.Abs(free.y - mid.y), "an adjacent cell");
            Assert.AreNotEqual(mid + Vector3Int.up, free);

            Assert.IsFalse(CellOccupants.TryFindFree(mid, c => c == mid, me, out _), "no floor around it");
        }

        [Test]
        public void Pathfinding_RoutesAroundOccupiedCells_AndFallsBackWhenNoWayExists()
        {
            // a 5 x 3 field; a villager stands in the middle of the straight route
            var grid = new WalkGrid(0, 0, 5, 3, (x, y) => true);
            var straight = grid.FindPath(0, 1, 4, 1);
            Assert.AreEqual(5, straight.Count);
            System.Func<int, int, bool> occupied = (x, y) => x == 2 && y == 1;
            var around = grid.FindPath(0, 1, 4, 1, occupied);
            Assert.IsNotNull(around);
            CollectionAssert.DoesNotContain(around, (2, 1));
            Assert.AreEqual(7, around.Count, "one step either way and back");

            // a corridor one cell wide with someone in it: no way through when occupied counts, so the caller falls back to the plain route
            var corridor = new WalkGrid(0, 0, 5, 1, (x, y) => true);
            Assert.IsNull(corridor.FindPath(0, 0, 4, 0, occupied == null ? null : (x, y) => x == 2));
            Assert.IsNotNull(corridor.FindPath(0, 0, 4, 0));

            // the goal itself may be occupied (somebody standing where you are walking to is the caller's business)
            Assert.IsNotNull(grid.FindPath(0, 1, 2, 1, occupied));
        }

        [Test]
        public void TheLag_GrowsWhileHeld_AndIsCaughtUpAtTwiceTheSpeedAfterwards_NeverBelowZero()
        {
            var lag = 0f;
            for (var i = 0; i < 5; i++) lag = NpcManager.NextLag(lag, 1f, true);
            Assert.AreEqual(5f, lag, 1e-5f, "five minutes held, five minutes behind");
            lag = NpcManager.NextLag(lag, 1f, false);
            Assert.AreEqual(4f, lag, 1e-5f, "a minute of catching up recovers a minute");
            for (var i = 0; i < 10; i++) lag = NpcManager.NextLag(lag, 1f, false);
            Assert.AreEqual(0f, lag);
        }
    }
}
