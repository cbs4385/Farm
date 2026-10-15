using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-15: "the trees in the forest still look strange". Trees, the farmer and the villagers are sorted together by height: the one lower down is nearer.
    public class WorldSortingTests
    {
        [Test]
        public void ThingsLowerOnTheScreen_AreDrawnOverThoseAboveThem_WhateverTheirColumn()
        {
            var cells = Enumerable.Range(0, 40).SelectMany(y => Enumerable.Range(0, 40).Select(x => new Vector3(x + 0.5f, y + 0.5f, 0f))).ToList();
            foreach (var a in cells.Where((_, i) => i % 7 == 0))
                foreach (var b in cells.Where((_, i) => i % 11 == 0))
                {
                    if (Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.x, b.x)) continue;
                    Assert.AreNotEqual(WorldSorting.DrawnOver(a, b), WorldSorting.DrawnOver(b, a), "exactly one of two things is in front: " + a + " " + b);
                    if (a.y < b.y) Assert.IsTrue(WorldSorting.DrawnOver(a, b), "lower down is nearer: " + a + " over " + b);
                }
        }

        [Test]
        public void TheFarmerAtTheFootOfATree_IsInFrontOfIt_AndBehindItFromAbove()
        {
            var tree = new Vector3(10.5f, 10.5f, 0f);                                  // a tree's cell centre
            Assert.IsTrue(WorldSorting.DrawnOver(new Vector3(10.5f, 9.6f, 0f), tree), "standing in the cell below, the farmer is over the crown's lower edge");
            Assert.IsTrue(WorldSorting.DrawnOver(tree, new Vector3(10.5f, 11.1f, 0f)), "the farmer in the cell above is behind the tree");
            Assert.IsTrue(WorldSorting.DrawnOver(new Vector3(9.5f, 10.5f, 0f), tree) != WorldSorting.DrawnOver(tree, new Vector3(9.5f, 10.5f, 0f)), "neighbours in a row have a fixed order, never a tie");
        }

        [Test]
        public void TheRendererSortsByTheAxis_AndTheActorsShareTheTreesOrder()
        {
            var yaml = System.IO.File.ReadAllText("Assets/Settings/Renderer2D.asset");          // read as text: the test assembly does not reference URP
            StringAssert.Contains("m_TransparencySortMode: 3", yaml, "custom axis: without it, equal orders are drawn in no height order");
            StringAssert.Contains("m_TransparencySortAxis: {x: 0.001, y: 1, z: 0}", yaml);
            Assert.AreEqual(new Vector3(0.001f, 1f, 0f), WorldSorting.Axis);
            Assert.AreEqual(WorldSorting.ActorOrder, NpcActor.StandingOrder, "a villager is sorted with the trees");
            Assert.Less(WorldSorting.ActorOrder, WorldSorting.TallOrder);
        }
    }
}
