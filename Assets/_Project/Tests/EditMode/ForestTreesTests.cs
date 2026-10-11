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
    }
}
