using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-033: forage, spawn tables and the slow regrowth of clutter.
    public class ForageTests
    {
        readonly List<UnityEngine.Object> _objects = new List<UnityEngine.Object>();
        GameState _state;
        GameClock _clock;
        IWorldQuery _world;

        [SetUp]
        public void SetUp()
        {
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 3);
            _clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 600));
            _state.SetDate(_clock.Now);
            _world = new StateWorldQuery(_state, _clock);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _objects) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            _objects.Clear();
        }

        static NodeCatalog Nodes => NodeCatalog.BuiltIn;

        static List<(int x, int y)> Field(int w, int h) =>
            Enumerable.Range(0, w).SelectMany(x => Enumerable.Range(0, h).Select(y => (x, y))).ToList();

        SpawnTableDefinition Table(string id, int attempts, int max, params SpawnEntry[] entries)
        {
            var t = SpawnTableDefinition.Create(id, "Test", new[] { "tile_grass" }, attempts, max, entries);
            _objects.Add(t);
            return t;
        }

        static SpawnEntry Entry(string node, float weight = 1f, SeasonMask seasons = SeasonMask.All, string condition = null, bool luck = false) =>
            new SpawnEntry { NodeId = node, Weight = weight, Seasons = seasons, Condition = condition, LuckSensitive = luck };

        int Spawn(NodeGrid grid, SpawnTableDefinition table, int day = 100, int seed = 5, Season season = Season.Spring, float luck = 0f,
            List<(int x, int y)> cells = null) =>
            SpawnModel.Spawn(grid, table, cells ?? Field(20, 20), Nodes, _world, season, luck, seed, day);

        // ---- the data ---------------------------------------------------------------------------------------------

        [Test]
        public void TheForageTable_HasRowsForEverySeason_AndShoresAreAlwaysOpen()
        {
            foreach (SeasonMask season in new[] { SeasonMask.Spring, SeasonMask.Summer, SeasonMask.Fall, SeasonMask.Winter })
            {
                Assert.GreaterOrEqual(ForageDefaults.Rows.Count(r => r.Seasons.HasFlag(season) && r.Where == ForageDefaults.Place.Meadow || r.Seasons.HasFlag(season) && r.Where == ForageDefaults.Place.Wood), 2, season.ToString());
            }
            Assert.IsTrue(ForageDefaults.Rows.Where(r => r.Where == ForageDefaults.Place.Shore).All(r => r.Seasons == SeasonMask.All));
        }

        [Test]
        public void EveryTableEntry_NamesARealNode_AndEveryForageNodeDropsItsItem()
        {
            foreach (var table in SpawnCatalog.BuiltIn.All)
                foreach (var e in table.Entries) Assert.IsNotNull(Nodes.Get(e.NodeId), $"{table.Id}: {e.NodeId}");
            foreach (var row in ForageDefaults.Rows)
            {
                var node = Nodes.Get(row.Id);
                Assert.AreEqual(ToolType.None, node.Tool, row.Id);
                Assert.AreEqual(row.ItemId, node.DropItemId);
                Assert.AreEqual(0f, node.SpawnWeight, "forage is not part of the starting clutter");
            }
        }

        [Test]
        public void TheGeneratedAssets_MatchTheDefaults_AndTheirItemsExist()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            Assert.AreEqual(ForageDefaults.CreateTables().Length, db.AllSpawnTables.Count(), "regenerate content: Farm/Setup/Generate Content");
            foreach (var row in ForageDefaults.Rows)
            {
                Assert.IsTrue(db.TryGetItem(row.ItemId, out var item), row.ItemId);
                Assert.AreEqual(ItemCategory.Forage, item.Category);
                Assert.AreEqual(row.Price, item.SellPrice);
                Assert.IsNotNull(item.Icon, row.ItemId + " has an icon");
            }
            foreach (var node in db.AllNodes) Assert.IsNotNull(node.Sprite, node.Id + " has a sprite");
        }

        // ---- spawning ---------------------------------------------------------------------------------------------

        [Test]
        public void Spawning_IsRepeatableForASeedAndDay_AndDiffersBetweenDays()
        {
            var table = Table("t", 5, 100, Entry("dandelion"), Entry("weed"));
            string Key(NodeGrid g) => string.Join(";", g.Nodes.OrderBy(n => n.X).ThenBy(n => n.Y).Select(n => $"{n.X},{n.Y},{n.TypeId}"));
            var a = new NodeGrid(); var b = new NodeGrid(); var c = new NodeGrid();
            Spawn(a, table, day: 50); Spawn(b, table, day: 50); Spawn(c, table, day: 51);
            Assert.AreEqual(Key(a), Key(b));
            Assert.AreNotEqual(Key(a), Key(c));
        }

        [Test]
        public void ADayMakesAtMostTheDailyAttempts_AndNeverExceedsTheCap()
        {
            var table = Table("t", 4, 100, Entry("weed"));
            var grid = new NodeGrid();
            Assert.LessOrEqual(Spawn(grid, table), 4);

            var capped = Table("c", 50, 6, Entry("weed"));
            var many = new NodeGrid();
            for (var day = 0; day < 20; day++) Spawn(many, capped, day: day);
            Assert.AreEqual(6, many.Count, "the map fills up to the cap and no further");
        }

        [Test]
        public void TheCap_CountsOnlyTheTablesOwnKinds()
        {
            var table = Table("t", 10, 3, Entry("weed"));
            var grid = new NodeGrid();
            for (var i = 0; i < 10; i++) grid.Add(i, 19, Nodes.Get("tree"));   // unrelated clutter
            for (var day = 0; day < 10; day++) Spawn(grid, table, day: day);
            Assert.AreEqual(3, grid.Nodes.Count(n => n.TypeId == "weed"));
        }

        [Test]
        public void OnlyTheSeasonsEntriesSpawn()
        {
            var table = Table("t", 30, 1000, Entry("dandelion", seasons: SeasonMask.Spring), Entry("snowdrop", seasons: SeasonMask.Winter));
            var spring = new NodeGrid(); Spawn(spring, table, season: Season.Spring);
            var winter = new NodeGrid(); Spawn(winter, table, season: Season.Winter);
            var summer = new NodeGrid();
            Assert.IsTrue(spring.Nodes.All(n => n.TypeId == "dandelion") && spring.Count > 0);
            Assert.IsTrue(winter.Nodes.All(n => n.TypeId == "snowdrop") && winter.Count > 0);
            Assert.AreEqual(0, Spawn(summer, table, season: Season.Summer), "nothing grows then");
        }

        [Test]
        public void AnEntryWithACondition_SpawnsOnlyWhileItHolds()
        {
            var table = Table("t", 30, 1000, Entry("weed", condition: "flag:overgrown"));
            var closed = new NodeGrid();
            Assert.AreEqual(0, Spawn(closed, table));

            _state.Flags.Add("overgrown");
            Assert.Greater(Spawn(new NodeGrid(), table), 0);
        }

        [Test]
        public void SpawningSkipsOccupiedCells_AndHandlesNoCandidates()
        {
            var table = Table("t", 30, 1000, Entry("weed"));
            var grid = new NodeGrid();
            grid.Add(0, 0, Nodes.Get("rock"));
            Spawn(grid, table, cells: new List<(int x, int y)> { (0, 0), (1, 0) });
            Assert.AreEqual("rock", grid.TryGet(0, 0, out var n) ? n.TypeId : null, "the rock is untouched");
            Assert.AreEqual(0, Spawn(new NodeGrid(), table, cells: new List<(int x, int y)>()));
        }

        [Test]
        public void AnUnknownNodeInATable_IsIgnored()
        {
            var table = Table("t", 30, 1000, Entry("no-such-node"));
            Assert.AreEqual(0, Spawn(new NodeGrid(), table));
        }

        [Test]
        public void RareFinds_FollowLuck()
        {
            var table = Table("t", 1, 100000, Entry("seashell", 10f), Entry("pearl", 10f, luck: true));
            int Pearls(float luck)
            {
                var pearls = 0;
                for (var day = 0; day < 2000; day++)
                {
                    var g = new NodeGrid();
                    Spawn(g, table, day: day, luck: luck);
                    pearls += g.Nodes.Count(n => n.TypeId == "pearl");
                }
                return pearls;
            }
            var unlucky = Pearls(-1f);
            var neutral = Pearls(0f);
            var lucky = Pearls(1f);
            Assert.AreEqual(0, unlucky, "no luck at all: no rare finds");
            Assert.Greater(lucky, neutral * 1.3f);
            Assert.Greater(neutral, 700);
        }

        // ---- picking and quality ----------------------------------------------------------------------------------

        [Test]
        public void Forage_IsPickedByHand_ButToolNodesAreNot()
        {
            var grid = new NodeGrid();
            grid.Add(1, 1, Nodes.Get("dandelion"));
            grid.Add(2, 2, Nodes.Get("weed"));

            var pick = grid.Gather(1, 1, Nodes.Get, 0.3f);
            Assert.AreEqual(NodeHit.Cleared, pick.Outcome);
            Assert.AreEqual("forage.dandelion", pick.DropItemId);
            Assert.AreEqual(1, pick.DropCount);
            Assert.AreEqual(SkillIds.Foraging, pick.Skill);
            Assert.IsFalse(grid.Has(1, 1));

            Assert.AreEqual(NodeHit.None, grid.Gather(2, 2, Nodes.Get, 0f).Outcome, "a weed needs the scythe");
            Assert.IsTrue(grid.Has(2, 2));
            Assert.AreEqual(NodeHit.None, grid.Gather(9, 9, Nodes.Get, 0f).Outcome);
        }

        [Test]
        public void Quality_ImprovesWithLevelAndLuck()
        {
            Assert.AreEqual(ForageModel.Normal, ForageModel.Quality(1, 0f, 0.0001f), "a level-1 forager with no luck finds plain things");
            Assert.AreEqual(0f, ForageModel.GoldChance(1, 0f));
            Assert.Greater(ForageModel.GoldChance(10, 0f), ForageModel.GoldChance(5, 0f));
            Assert.Greater(ForageModel.SilverChance(10, 0f), ForageModel.SilverChance(5, 0f));
            Assert.Greater(ForageModel.GoldChance(5, 0.5f), ForageModel.GoldChance(5, 0f));
            Assert.AreEqual(ForageModel.GoldChance(5, 0f), ForageModel.GoldChance(5, -1f), "bad luck is no worse than none");
        }

        [Test]
        public void Quality_FollowsTheRoll()
        {
            var gold = ForageModel.GoldChance(10, 0f);
            var silver = ForageModel.SilverChance(10, 0f);
            Assert.AreEqual(ForageModel.Gold, ForageModel.Quality(10, 0f, gold * 0.5f));
            Assert.AreEqual(ForageModel.Silver, ForageModel.Quality(10, 0f, gold + silver * 0.5f));
            Assert.AreEqual(ForageModel.Normal, ForageModel.Quality(10, 0f, 0.999f));
            Assert.Less(gold + silver, 1f);
        }

        // ---- the session: catching up after days away -------------------------------------------------------------

        sealed class SessionFixture : IDisposable
        {
            public readonly GameObject Go = new GameObject("session");
            public readonly GameSession Session;
            readonly string _root = Path.Combine(Path.GetTempPath(), "farm-forage-" + Guid.NewGuid().ToString("N"));

            public SessionFixture()
            {
                Directory.CreateDirectory(_root);
                Session = Go.AddComponent<GameSession>();
                Session.Init(new EventBus(), GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]), new SaveService(_root));
                Session.BeginDevGame();
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Go);
                if (Directory.Exists(_root)) Directory.Delete(_root, true);
            }
        }

        static IReadOnlyList<(int x, int y)> Meadow(SpawnTableDefinition _) => Field(40, 30);

        [Test]
        public void ANewMap_GetsThreeMornings_ThenOneADay_AndNeverMoreThanThreeAtOnce()
        {
            using (var f = new SessionFixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 10, 600));
                var first = s.RunSpawns("Village", Meadow);
                Assert.Greater(first, 0, "a first visit finds things growing");
                Assert.AreEqual(0, s.RunSpawns("Village", Meadow), "the same day again adds nothing");
                var after = s.GetNodes("Village").Count;

                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 11, 600));
                var next = s.RunSpawns("Village", Meadow);
                Assert.That(next, Is.InRange(1, ForageDefaults.CreateTables().Where(t => t.MapId == "Village").Sum(t => t.DailyAttempts)), "one morning's worth");

                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 25, 600));
                var away = s.RunSpawns("Village", Meadow);
                Assert.That(away, Is.InRange(1, 3 * ForageDefaults.CreateTables().Where(t => t.MapId == "Village").Sum(t => t.DailyAttempts)), "two weeks away counts as three mornings at most");
                Assert.AreEqual(after + next + away, s.GetNodes("Village").Count);
                Assert.AreEqual(s.Clock.Now.TotalDays, s.State.GetMap("Village").LastSpawnDay);
            }
        }

        [Test]
        public void EachMapUsesItsOwnTables()
        {
            using (var f = new SessionFixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 10, 600));
                s.RunSpawns("Forest", Meadow);
                s.RunSpawns("Beach", Meadow);
                var forest = s.GetNodes("Forest").Nodes.Select(n => n.TypeId).Distinct().ToList();
                var beach = s.GetNodes("Beach").Nodes.Select(n => n.TypeId).Distinct().ToList();
                CollectionAssert.IsSubsetOf(forest, new[] { "hazelnut", "mushroom", "truffle", "wildgarlic", "raspberry", "winterroot" });
                CollectionAssert.IsSubsetOf(beach, new[] { "seashell", "clam", "pearl" });
                Assert.IsNotEmpty(forest);
                Assert.IsNotEmpty(beach);
                Assert.AreEqual(0, s.RunSpawns("Nowhere", Meadow), "a map with no table gets nothing");
            }
        }

        [Test]
        public void TheFarmGrowsBackOverSlowly_ButNotInWinter()
        {
            using (var f = new SessionFixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Winter, 10, 600));
                s.RunSpawns("Farm", Meadow);
                Assert.IsFalse(s.GetNodes("Farm").Nodes.Any(n => n.TypeId == "weed"), "no weeds in winter");

                s.Clock.SetTime(new GameDateTime(2, Season.Spring, 10, 600));
                var before = s.GetNodes("Farm").Count;
                s.RunSpawns("Farm", Meadow);
                Assert.Greater(s.GetNodes("Farm").Count, before);
                Assert.IsTrue(s.GetNodes("Farm").Nodes.Any(n => n.TypeId == "weed"));
            }
        }

        // ---- packs --------------------------------------------------------------------------------------------------

        [Test]
        public void PacksCanAddSpawnTables_ButNotRedefineThem()
        {
            var core = ForageDefaults.CreateTables();
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0], null, null, null, core);
            var extra = SpawnTableDefinition.Create("woods.things", "Woods", new[] { "tile_forest" }, 2, 5, new[] { Entry("weed") });
            var clash = SpawnTableDefinition.Create("beach.forage", "Beach", new[] { "tile_sand" }, 1, 1, new[] { Entry("weed") });
            var pack = ContentPack.Create("test", null, null, null);
            typeof(ContentPack).GetField("_spawnTables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(pack, new List<SpawnTableDefinition> { extra, clash });
            _objects.AddRange(new UnityEngine.Object[] { db, extra, clash, pack });
            _objects.AddRange(core);

            LogAssert.Expect(LogType.Error, new Regex("redefines spawn table 'beach.forage'"));
            Assert.AreEqual(1, db.Merge(pack));
            var catalog = SpawnCatalog.From(db);
            Assert.AreEqual(1, catalog.For("Woods").Count());
            Assert.AreEqual(ForageDefaults.CreateTables().First(t => t.Id == "beach.forage").DailyAttempts, catalog.For("Beach").Single().DailyAttempts, "the core beach table stays as it was");
        }

        [Test]
        public void ACatalogFromADatabaseWithNoTables_UsesTheBuiltInOnes()
        {
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]);
            _objects.Add(db);
            Assert.AreSame(SpawnCatalog.BuiltIn, SpawnCatalog.From(db));
        }
    }
}
