using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-11: the trees of the forest could not be chopped.
    public class ForestTreesTests
    {
        [Test]
        public void TheThickets_LeaveThePath_ThePond_AndTheWayToTheMine_Clear()
        {
            var cells = ForestTrees.Cells().ToList();
            Assert.Greater(cells.Count, 60, "a wood");
            Assert.IsFalse(cells.Any(c => c.x >= ForestTrees.PathX0 - 2 && c.x <= ForestTrees.PathX1 + 2), "the path");
            Assert.IsFalse(cells.Any(c => c.x >= 2 && c.x <= 16 && c.y >= 3 && c.y <= 15), "the pond");
            Assert.IsFalse(cells.Any(c => c.x >= 21 && c.x <= 36 && c.y >= 3 && c.y <= 7), "the way to the mine");
        }

        [Test]
        public void Seeding_PutsInTreesToChop_OnceOnly_SoAFelledTreeStaysFelled()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var added = ForestTrees.Seed(s);
                Assert.Greater(added, 60);
                var nodes = s.GetNodes(MapIds.Forest);
                var (x, y) = ForestTrees.Cells().First();
                Assert.IsTrue(nodes.TryGet(x, y, out var tree));
                Assert.AreEqual(NodeDefaults.Tree, tree.TypeId);
                var def = s.Nodes.Get(NodeDefaults.Tree);
                Assert.AreEqual(ToolType.Axe, def.Tool, "chopped with the axe");

                nodes.Remove(x, y);                                   // felled (and its stump cleared)
                Assert.AreEqual(0, ForestTrees.Seed(s), "not planted again");
                Assert.IsFalse(nodes.TryGet(x, y, out _));
            }
        }

        [Test]
        public void TheForest_GrowsOneTreeADay_InAnEmptyThicketCell_AndNotOnTheFirstVisit()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var tree = s.Nodes.Get(NodeDefaults.Tree);
                var nodes = s.GetNodes(MapIds.Forest);
                Assert.AreEqual(0, ForestTrees.Regrow(s, 10), "the first visit grows nothing");
                ForestTrees.Seed(s);
                var cells = ForestTrees.Cells().ToList();
                foreach (var c in cells.Take(20)) nodes.Remove(c.x, c.y);              // twenty felled
                s.SetVar(ForestTrees.LastGrowthVar, 10 + 1);
                Assert.AreEqual(1, ForestTrees.Regrow(s, 11), "one day, one tree");
                Assert.AreEqual(ForestTrees.MostPerVisit, ForestTrees.Regrow(s, 11 + 30), "a long absence adds at most three");
                Assert.AreEqual(0, ForestTrees.Regrow(s, 11 + 30), "and none twice on the same day");
                var grown = cells.Take(20).Count(c => nodes.Has(c.x, c.y));
                Assert.AreEqual(4, grown, "all of them in thicket cells that were empty");
                Assert.IsFalse(nodes.Nodes.Any(n => n.TypeId == NodeDefaults.Tree && !cells.Contains((n.X, n.Y))), "never anywhere else");
            }
        }

        [Test]
        public void ARegrownTree_NeverTakesTheCellOfASapling_OrAStump()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var nodes = s.GetNodes(MapIds.Forest);
                ForestTrees.Seed(s);
                var cells = ForestTrees.Cells().ToList();
                foreach (var c in cells) { nodes.Remove(c.x, c.y); nodes.Add(c.x, c.y, s.Nodes.Get(NodeDefaults.Stump)); }
                s.SetVar(ForestTrees.LastGrowthVar, 1);
                Assert.AreEqual(0, ForestTrees.Regrow(s, 5), "every cell is taken: nothing grows");
                Assert.AreEqual(-1, ForestTrees.GrowthChoice(0, 5, 0));
                Assert.That(ForestTrees.GrowthChoice(7, 5, 0), Is.InRange(0, 6));
            }
        }
    }
}
