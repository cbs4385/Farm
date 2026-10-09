using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-09 (the owner's wife): not enough trees to chop, trees should be plantable or grow back faster, and wild plants should spread from the
    // ones left standing.
    public class TreeAndForageTests
    {
        static NodeCatalog Catalog() => NodeCatalog.BuiltIn;

        [Test]
        public void ASapling_BecomesATree_AfterItsDays_AndOnlyThen()
        {
            var catalog = Catalog();
            var grid = new NodeGrid();
            Assert.IsTrue(grid.Add(3, 4, catalog.Get(NodeDefaults.Sapling)));
            for (var day = 1; day < NodeDefaults.SaplingDays; day++)
            {
                Assert.IsEmpty(grid.Grow(catalog.Get), "day " + day);
                grid.TryGet(3, 4, out var node);
                Assert.AreEqual(NodeDefaults.Sapling, node.TypeId);
            }
            var grown = grid.Grow(catalog.Get);
            Assert.AreEqual(new[] { (3, 4) }, grown.ToArray());
            grid.TryGet(3, 4, out var tree);
            Assert.AreEqual(NodeDefaults.Tree, tree.TypeId);
            Assert.AreEqual(catalog.Get(NodeDefaults.Tree).HitPoints, tree.Hp);
            Assert.IsEmpty(grid.Grow(catalog.Get), "a tree does not grow into anything");
        }

        [Test]
        public void TheSapling_IsSavedWithItsAge()
        {
            var catalog = Catalog();
            var grid = new NodeGrid();
            grid.Add(1, 1, catalog.Get(NodeDefaults.Sapling));
            grid.Grow(catalog.Get);
            grid.Grow(catalog.Get);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grid.ToList());
            var back = NodeGrid.FromNodes(Newtonsoft.Json.JsonConvert.DeserializeObject<List<NodeInstance>>(json));
            back.TryGet(1, 1, out var node);
            Assert.AreEqual(2, node.Age);
        }

        [Test]
        public void ASaplingIsClearedWithTheAxe_AndGivesTheAcornBack()
        {
            var catalog = Catalog();
            var grid = new NodeGrid();
            grid.Add(2, 2, catalog.Get(NodeDefaults.Sapling));
            var result = grid.Hit(2, 2, ToolType.Axe, 0, catalog.Get, 0f);
            Assert.AreEqual(NodeHit.Cleared, result.Outcome);
            Assert.AreEqual(ItemIds.Acorn, result.DropItemId);
            Assert.IsFalse(catalog.Get(NodeDefaults.Sapling).Solid, "you can walk past a sapling");
        }

        [Test]
        public void FelledTrees_DropAcorns_AboutHalfTheTime()
        {
            Assert.AreEqual(2, TreeRules.AcornsFor(0.05f));
            Assert.AreEqual(1, TreeRules.AcornsFor(0.3f));
            Assert.AreEqual(0, TreeRules.AcornsFor(0.9f));
            var average = Enumerable.Range(0, 1000).Average(i => TreeRules.AcornsFor(i / 1000f));
            Assert.Greater(average, 0.6f);
        }

        [Test]
        public void TheFarm_GrowsTreesBackQuickly_AndForageComesBackFasterThanBefore()
        {
            var tables = ForageDefaults.CreateTables().ToDictionary(t => t.Id);
            var clutter = tables["farm.clutter"];
            var tree = clutter.Entries.First(e => e.NodeId == NodeDefaults.Tree).Weight;
            var total = clutter.Entries.Sum(e => e.Weight);
            Assert.GreaterOrEqual(clutter.DailyAttempts * tree / total, 0.8f, "about a tree a day while there is room");
            Assert.GreaterOrEqual(tables["forest.forage"].DailyAttempts, 7);
            Assert.GreaterOrEqual(tables["village.forage"].DailyAttempts, 5);
            Assert.IsTrue(tables["forest.forage"].Spreads && tables["village.forage"].Spreads && tables["beach.forage"].Spreads);
            Assert.IsFalse(clutter.Spreads);
        }

        [Test]
        public void ForageRules_KnowsTheLastOfItsKind()
        {
            var catalog = Catalog();
            var grid = new NodeGrid();
            grid.Add(1, 1, catalog.Get("dandelion"));
            Assert.IsTrue(ForageRules.IsLastOfItsKind(grid, "dandelion"));
            grid.Add(2, 2, catalog.Get("dandelion"));
            Assert.IsFalse(ForageRules.IsLastOfItsKind(grid, "dandelion"));
            Assert.AreEqual(2, ForageRules.Standing(grid, "dandelion"));
        }

        [Test]
        public void SpreadingFavoursAKindThatStands_WhenTwoKindsCompete()
        {
            var catalog = Catalog();
            var table = SpawnTableDefinition.Create("test.spread", "Village", new[] { "tile_grass" }, 1, 500, new[]
            {
                new SpawnEntry { NodeId = "dandelion", Weight = 1f, Seasons = SeasonMask.All },
                new SpawnEntry { NodeId = "seashell", Weight = 1f, Seasons = SeasonMask.All },
            }, spreads: true);
            var cells = Enumerable.Range(0, 30).SelectMany(x => Enumerable.Range(0, 30).Select(y => (x, y))).ToList();
            int dandelions = 0, shells = 0;
            for (var seed = 1; seed <= 300; seed++)
            {
                var grid = new NodeGrid();
                grid.Add(29, 29, catalog.Get("dandelion"));
                SpawnModel.Spawn(grid, table, cells, catalog, null, Season.Spring, 0f, seed, 1);
                foreach (var n in grid.Nodes) { if (n.TypeId == "dandelion" && n.X != 29) dandelions++; if (n.TypeId == "seashell") shells++; }
            }
            Assert.Greater(dandelions, shells * 5, "the kind still standing comes back about twelve times as often");
        }
    }
}
