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
    // T-032: skill levels, tool tiers, and the trees, rocks and weeds the tools clear.
    public class SkillAndNodeTests
    {
        readonly List<UnityEngine.Object> _objects = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _objects) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            _objects.Clear();
        }

        static ResourceNodeDefinition Def(string id) => NodeCatalog.BuiltIn.Get(id);

        // ---- skills -----------------------------------------------------------------------------------------------

        [Test]
        public void Skills_StartAtLevelOne_AndGrowToTen()
        {
            Assert.AreEqual(1, SkillModel.LevelForXp(0));
            Assert.AreEqual(1, SkillModel.LevelForXp(99));
            Assert.AreEqual(2, SkillModel.LevelForXp(100));
            Assert.AreEqual(5, SkillModel.LevelForXp(700));
            Assert.AreEqual(10, SkillModel.LevelForXp(3200));
            Assert.AreEqual(10, SkillModel.LevelForXp(999999), "capped at 10");
        }

        [Test]
        public void EachLevelNeedsMoreXpThanTheLast()
        {
            for (var level = 2; level < SkillModel.MaxLevel; level++)
                Assert.Greater(SkillModel.XpForLevel(level + 1) - SkillModel.XpForLevel(level),
                    SkillModel.XpForLevel(level) - SkillModel.XpForLevel(level - 1) - 1, $"level {level}");
            for (var level = 1; level <= SkillModel.MaxLevel; level++)
                Assert.AreEqual(level, SkillModel.LevelForXp(SkillModel.XpForLevel(level)), "round trip");
        }

        [Test]
        public void Progress_RunsFromZeroToOneWithinALevel()
        {
            Assert.AreEqual(0f, SkillModel.Progress(0), 0.001f);
            Assert.AreEqual(0.5f, SkillModel.Progress(50), 0.001f);
            Assert.AreEqual(0f, SkillModel.Progress(100), 0.001f);
            Assert.AreEqual(1f, SkillModel.Progress(5000), 0.001f, "full at the cap");
        }

        [Test]
        public void TheFiveSkillsAreKnown()
        {
            CollectionAssert.AreEquivalent(new[] { "farming", "foraging", "mining", "fishing", "combat" }, SkillIds.All);
            Assert.IsTrue(SkillModel.IsKnown("mining"));
            Assert.IsFalse(SkillModel.IsKnown("cooking"));
        }

        // ---- tool tiers -------------------------------------------------------------------------------------------

        [TestCase(2, 0, 2)] [TestCase(2, 1, 2)] [TestCase(2, 2, 1)] [TestCase(2, 3, 1)]
        [TestCase(3, 0, 3)] [TestCase(3, 1, 3)] [TestCase(3, 2, 2)] [TestCase(3, 3, 2)]
        public void BetterToolsCostLessEnergy(int baseCost, int tier, int expected) =>
            Assert.AreEqual(expected, ToolModel.EnergyCost(baseCost, tier));

        [Test]
        public void ToolsNeverCostNothing_AndTiersAreClamped()
        {
            Assert.AreEqual(1, ToolModel.EnergyCost(1, 3));
            Assert.AreEqual(0, ToolModel.EnergyCost(0, 3));
            Assert.AreEqual(ToolModel.EnergyCost(3, 3), ToolModel.EnergyCost(3, 99));
            Assert.AreEqual(1, ToolModel.Damage(0));
            Assert.AreEqual(4, ToolModel.Damage(3));
            Assert.AreEqual("tier.basic", ToolModel.TierKey(-5));
        }

        // ---- nodes ------------------------------------------------------------------------------------------------

        static NodeGrid GridWith(string id, int x = 3, int y = 4)
        {
            var grid = new NodeGrid();
            Assert.IsTrue(grid.Add(x, y, Def(id)));
            return grid;
        }

        static NodeHitResult Swing(NodeGrid grid, ToolType tool, int tier = 0, int x = 3, int y = 4, float roll = 0f) =>
            grid.Hit(x, y, tool, tier, NodeCatalog.BuiltIn.Get, roll);

        [Test]
        public void ANodeNeedsItsOwnTool()
        {
            var grid = GridWith("weed");
            Assert.AreEqual(NodeHit.WrongTool, Swing(grid, ToolType.Axe).Outcome);
            Assert.AreEqual(NodeHit.WrongTool, Swing(grid, ToolType.Pickaxe).Outcome);
            Assert.IsTrue(grid.Has(3, 4));
            Assert.AreEqual(NodeHit.None, Swing(grid, ToolType.Scythe, x: 9, y: 9).Outcome, "nothing there");
        }

        [Test]
        public void AWeed_FallsToOneSwingOfTheScythe_AndDropsFiber()
        {
            var grid = GridWith("weed");
            var result = Swing(grid, ToolType.Scythe);
            Assert.AreEqual(NodeHit.Cleared, result.Outcome);
            Assert.AreEqual(ItemIds.Fiber, result.DropItemId);
            Assert.AreEqual(SkillIds.Foraging, result.Skill);
            Assert.IsFalse(grid.Has(3, 4));
        }

        [Test]
        public void ARock_TakesTwoBasicSwings_ButOneCopperSwing()
        {
            var basic = GridWith("rock");
            Assert.AreEqual(NodeHit.Damaged, Swing(basic, ToolType.Pickaxe).Outcome);
            var done = Swing(basic, ToolType.Pickaxe);
            Assert.AreEqual(NodeHit.Cleared, done.Outcome);
            Assert.AreEqual(ItemIds.Stone, done.DropItemId);
            Assert.AreEqual(SkillIds.Mining, done.Skill);

            var copper = GridWith("rock");
            Assert.AreEqual(NodeHit.Cleared, Swing(copper, ToolType.Pickaxe, tier: 1).Outcome);
        }

        [Test]
        public void ABoulder_NeedsAtLeastACopperPickaxe()
        {
            var grid = GridWith("boulder");
            Assert.AreEqual(NodeHit.TooWeak, Swing(grid, ToolType.Pickaxe, tier: 0).Outcome);
            Assert.AreEqual(6, grid.TryGet(3, 4, out var node) ? node.Hp : -1, "a weak swing does no damage");
            Assert.AreEqual(NodeHit.Damaged, Swing(grid, ToolType.Pickaxe, tier: 1).Outcome);
            Assert.AreEqual(4, node.Hp);
            Assert.AreEqual(NodeHit.Cleared, Swing(grid, ToolType.Pickaxe, tier: 3).Outcome, "a gold pickaxe finishes it");
        }

        [Test]
        public void ATree_LeavesAStump_ThatLeavesNothing()
        {
            var grid = GridWith("tree");
            for (var i = 0; i < 5; i++) Assert.AreEqual(NodeHit.Damaged, Swing(grid, ToolType.Axe).Outcome);
            var felled = Swing(grid, ToolType.Axe);
            Assert.AreEqual(NodeHit.Cleared, felled.Outcome);
            Assert.AreEqual("stump", felled.LeftBehind);
            Assert.IsTrue(grid.TryGet(3, 4, out var stump));
            Assert.AreEqual("stump", stump.TypeId);
            Assert.AreEqual(3, stump.Hp);

            Swing(grid, ToolType.Axe); Swing(grid, ToolType.Axe);
            var last = Swing(grid, ToolType.Axe);
            Assert.AreEqual(NodeHit.Cleared, last.Outcome);
            Assert.IsNull(last.LeftBehind);
            Assert.IsFalse(grid.Has(3, 4));
        }

        [Test]
        public void TheDropCount_FollowsTheRollWithinTheRange()
        {
            Assert.AreEqual(8, Swing(TreeAt(1), ToolType.Axe, tier: 3, roll: 0f).DropCount);
            Assert.AreEqual(12, Swing(TreeAt(1), ToolType.Axe, tier: 3, roll: 0.9999f).DropCount);
            var mid = Swing(TreeAt(1), ToolType.Axe, tier: 3, roll: 0.5f).DropCount;
            Assert.That(mid, Is.InRange(8, 12));
        }

        static NodeGrid TreeAt(int hp)
        {
            var grid = GridWith("tree");
            grid.TryGet(3, 4, out var n);
            n.Hp = hp;
            return grid;
        }

        [Test]
        public void Nodes_SurviveSaveAndLoad()
        {
            var grid = GridWith("rock");
            grid.Add(5, 5, Def("tree"));
            Swing(grid, ToolType.Pickaxe);   // damage the rock
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grid.ToList());
            var back = NodeGrid.FromNodes(Newtonsoft.Json.JsonConvert.DeserializeObject<List<NodeInstance>>(json));
            Assert.AreEqual(2, back.Count);
            Assert.IsTrue(back.TryGet(3, 4, out var rock));
            Assert.AreEqual("rock", rock.TypeId);
            Assert.AreEqual(1, rock.Hp, "the damage was kept");
        }

        // ---- the clutter generator --------------------------------------------------------------------------------

        static List<(int x, int y)> Field(int w, int h) =>
            Enumerable.Range(0, w).SelectMany(x => Enumerable.Range(0, h).Select(y => (x, y))).ToList();

        static NodeGrid Scatter(int seed, string map = "Farm", float density = 0.1f)
        {
            var grid = new NodeGrid();
            NodeSpawner.Generate(grid, Field(40, 30), NodeCatalog.BuiltIn.All, seed, map, density);
            return grid;
        }

        [Test]
        public void Clutter_IsRepeatableForASeed_AndDiffersBetweenSeedsAndMaps()
        {
            string Key(NodeGrid g) => string.Join(";", g.Nodes.OrderBy(n => n.X).ThenBy(n => n.Y).Select(n => $"{n.X},{n.Y},{n.TypeId}"));
            Assert.AreEqual(Key(Scatter(5)), Key(Scatter(5)));
            Assert.AreNotEqual(Key(Scatter(5)), Key(Scatter(6)));
            Assert.AreNotEqual(Key(Scatter(5)), Key(Scatter(5, "Forest")));
        }

        [Test]
        public void Clutter_FollowsTheDensity_AndTheMixFollowsTheWeights()
        {
            var grid = Scatter(11, density: 0.1f);
            Assert.That(grid.Count, Is.InRange(80, 160), "about 10% of 1200 cells");
            var byType = grid.Nodes.GroupBy(n => n.TypeId).ToDictionary(g => g.Key, g => g.Count());
            Assert.Greater(byType["weed"], byType["rock"]);
            Assert.Greater(byType["rock"], byType.GetValueOrDefault("boulder"));
            Assert.AreEqual(0, Scatter(11, density: 0f).Count);
        }

        [Test]
        public void Clutter_OnlyUsesTheCellsItIsGiven_AndSkipsOccupiedOnes()
        {
            var grid = new NodeGrid();
            grid.Add(0, 0, Def("rock"));
            var placed = NodeSpawner.Generate(grid, new[] { (0, 0), (1, 1), (2, 2) }, NodeCatalog.BuiltIn.All, 3, "Farm", 1f);
            Assert.AreEqual(2, placed, "the occupied cell is left alone");
            Assert.IsFalse(grid.Has(5, 5));
            Assert.AreEqual("rock", (grid.TryGet(0, 0, out var n), n.TypeId).TypeId);
        }

        [Test]
        public void ADefinitionWithNoSpawnWeight_NeverSpawns()
        {
            var grid = new NodeGrid();
            NodeSpawner.Generate(grid, Field(30, 30), new[] { Def("stump"), Def("tree") }.Select(d => d), 4, "x", 0.5f);
            Assert.IsTrue(grid.Count > 0);
            var never = ResourceNodeDefinition.Create("never", ToolType.Axe, 1, 0, ItemIds.Wood, 1, 1, "foraging", 1, null, true, 0f);
            _objects.Add(never);
            var only = new NodeGrid();
            Assert.AreEqual(0, NodeSpawner.Generate(only, Field(10, 10), new[] { never }, 4, "x", 1f));
        }

        // ---- the database and packs -------------------------------------------------------------------------------

        [Test]
        public void ACatalogFromADatabaseWithNoNodes_UsesTheBuiltInOnes()
        {
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]);
            _objects.Add(db);
            Assert.AreSame(NodeCatalog.BuiltIn, NodeCatalog.From(db));
        }

        [Test]
        public void PacksCanAddNodes_ButNotRedefineThem()
        {
            var core = NodeDefaults.CreateAll();
            var db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0], null, core);
            var fungus = ResourceNodeDefinition.Create("fungus", ToolType.Scythe, 1, 0, ItemIds.Fiber, 1, 1, "foraging", 1, null, false, 1f);
            var clash = ResourceNodeDefinition.Create("rock", ToolType.Axe, 1, 0, ItemIds.Wood, 1, 1, "foraging", 1, null, false, 1f);
            var pack = ContentPack.Create("test", null, null, null);
            typeof(ContentPack).GetField("_nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(pack, new List<ResourceNodeDefinition> { fungus, clash });
            _objects.AddRange(new UnityEngine.Object[] { db, fungus, clash, pack });
            _objects.AddRange(core);

            LogAssert.Expect(LogType.Error, new Regex("redefines resource node 'rock'"));
            Assert.AreEqual(1, db.Merge(pack));
            var catalog = NodeCatalog.From(db);
            Assert.IsNotNull(catalog.Get("fungus"));
            Assert.AreEqual(ToolType.Pickaxe, catalog.Get("rock").Tool, "the core rock stays as it was");
        }

        // ---- the session ------------------------------------------------------------------------------------------

        sealed class SessionFixture : IDisposable
        {
            public readonly GameObject Go = new GameObject("session");
            public readonly GameSession Session;
            public readonly EventBus Bus = new EventBus();
            readonly string _root = Path.Combine(Path.GetTempPath(), "farm-skill-" + Guid.NewGuid().ToString("N"));

            public SessionFixture()
            {
                Directory.CreateDirectory(_root);
                Session = Go.AddComponent<GameSession>();
                Session.Init(Bus, GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]), new SaveService(_root));
                Session.BeginDevGame();
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Go);
                if (Directory.Exists(_root)) Directory.Delete(_root, true);
            }
        }

        [Test]
        public void XpAddsUp_AndEveryLevelGainedIsAnnounced()
        {
            using (var f = new SessionFixture())
            {
                var levels = new List<SkillLevelUp>();
                var toasts = new List<string>();
                f.Bus.Subscribe<SkillLevelUp>(levels.Add);
                f.Bus.Subscribe<ToastRequested>(t => toasts.Add(t.Message));
                L.SetLanguage("en");

                f.Session.AddSkillXp(SkillIds.Mining, 99);
                Assert.IsEmpty(levels);
                f.Session.AddSkillXp(SkillIds.Mining, 1);
                Assert.AreEqual(1, levels.Count);
                Assert.AreEqual(2, f.Session.GetSkillLevel(SkillIds.Mining));
                Assert.AreEqual(100, f.Session.GetSkillXp(SkillIds.Mining));
                CollectionAssert.Contains(toasts, "Mining level 2!");

                f.Session.AddSkillXp(SkillIds.Mining, 700);   // jumps several levels at once
                Assert.AreEqual(new[] { 2, 3, 4, 5 }, levels.Select(l => l.Level).ToArray());
                Assert.AreEqual(1, f.Session.GetSkillLevel(SkillIds.Farming), "other skills are untouched");
            }
        }

        [Test]
        public void ToolTiers_DefaultToBasic()
        {
            using (var f = new SessionFixture())
            {
                Assert.AreEqual(0, f.Session.ToolTier(ItemIds.Axe));
                f.Session.State.ToolTiers[ItemIds.Axe] = 2;
                Assert.AreEqual(2, f.Session.ToolTier(ItemIds.Axe));
                Assert.AreEqual(0, f.Session.ToolTier(ItemIds.Hoe));
            }
        }

        [Test]
        public void ClutterIsScatteredOnce_AndSavedWithTheMap()
        {
            using (var f = new SessionFixture())
            {
                f.Session.EnsureClutter("Farm", Field(30, 20), 0.2f);
                var first = f.Session.GetNodes("Farm").Count;
                Assert.Greater(first, 50);

                f.Session.GetNodes("Farm").Remove(first > 0 ? f.Session.GetNodes("Farm").Nodes.First().X : 0, f.Session.GetNodes("Farm").Nodes.First().Y);
                f.Session.EnsureClutter("Farm", Field(30, 20), 0.2f);
                Assert.AreEqual(first - 1, f.Session.GetNodes("Farm").Count, "visiting again does not re-seed");

                f.Session.SyncToState();
                Assert.AreEqual(first - 1, f.Session.State.GetMap("Farm").Nodes.Count);
                Assert.IsTrue(f.Session.State.GetMap("Farm").ClutterSeeded);
            }
        }
    }
}
