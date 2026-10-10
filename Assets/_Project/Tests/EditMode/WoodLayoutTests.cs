using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-09: the forest trees were too regular, and the edges of the maps plain walls.
    public class WoodLayoutTests
    {
        [Test]
        public void The_forest_has_thickets_and_clearings_and_no_lattice()
        {
            // count the trees in each 8 x 8 block of a 40 x 30 forest: the blocks differ a lot
            var counts = new System.Collections.Generic.List<int>();
            for (var by = 0; by + 8 <= 30; by += 8)
                for (var bx = 0; bx + 8 <= 40; bx += 8)
                {
                    var n = 0;
                    for (var y = by; y < by + 8; y++)
                        for (var x = bx; x < bx + 8; x++)
                            if (WoodLayout.ForestTreeAt(x, y)) n++;
                    counts.Add(n);
                }
            Assert.Greater(counts.Max() - counts.Min(), 8, "some blocks are thickets and some clearings");

            // the old lattice put a tree at every nine cells of a diagonal pattern: the new layout does not repeat
            var rowSpacings = new System.Collections.Generic.HashSet<int>();
            for (var y = 3; y < 28; y++)
            {
                var last = -1;
                for (var x = 2; x < 38; x++)
                    if (WoodLayout.ForestTreeAt(x, y)) { if (last >= 0) rowSpacings.Add(x - last); last = x; }
            }
            Assert.Greater(rowSpacings.Count, 5, "trees are spaced unevenly");
        }

        [Test]
        public void The_layout_is_the_same_every_time()
        {
            for (var i = 0; i < 20; i++) Assert.AreEqual(WoodLayout.ForestTreeAt(i, 10), WoodLayout.ForestTreeAt(i, 10));
        }

        [Test]
        public void The_edge_band_is_two_cells_thick_and_leaves_the_way_out_open()
        {
            const int w = 40, h = 30;
            var cells = WoodLayout.BandCells(w, h, (bx, by) => bx >= 18 && bx <= 20 && (by == 0 || by == h - 1));
            Assert.IsFalse(cells.Contains(new UnityEngine.Vector2Int(19, 0)));
            Assert.IsFalse(cells.Contains(new UnityEngine.Vector2Int(19, 1)), "the second row of the way out is open too");
            Assert.IsTrue(cells.Contains(new UnityEngine.Vector2Int(17, 1)));
            Assert.IsTrue(cells.Contains(new UnityEngine.Vector2Int(0, 15)));
            Assert.IsTrue(cells.Contains(new UnityEngine.Vector2Int(1, 15)));
            Assert.IsFalse(cells.Contains(new UnityEngine.Vector2Int(2, 15)), "the band is two cells thick");
        }

        [Test]
        public void Most_of_the_band_shows_a_tree()
        {
            var cells = WoodLayout.BandCells(88, 64, null);
            var shown = cells.Count(c => WoodLayout.BandShowsTree(c.x, c.y));
            Assert.Greater(shown / (float)cells.Count, 0.5f);
            Assert.Less(shown, cells.Count, "not every cell: some are only collision under the crowns");
        }
    }
}
