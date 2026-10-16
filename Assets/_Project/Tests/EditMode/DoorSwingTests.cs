using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-16: "the interior doors do not open or close when the player moves through them".
    public class DoorSwingTests
    {
        [Test]
        public void ADoorLeaf_OpensAndClosesInAboutAFifthOfASecond_AndStaysBetweenShutAndOpen()
        {
            var openness = 0f;
            for (var i = 0; i < 10; i++) openness = DoorSwing.Approach(openness, 1f, 0.02f);
            Assert.AreEqual(1f, openness, 0.0001f);
            Assert.AreEqual(1f, DoorSwing.Approach(1f, 1f, 1f), "already open");
            for (var i = 0; i < 10; i++) openness = DoorSwing.Approach(openness, 0f, 0.02f);
            Assert.AreEqual(0f, openness, 0.0001f);
            Assert.AreEqual(0f, DoorSwing.Approach(0f, 0f, 1f), "already shut");
            Assert.AreEqual(0.5f, DoorSwing.Approach(0f, 1f, 0.1f), 0.0001f, "half way after a tenth of a second");
        }

        [Test]
        public void AnOpenLeaf_IsNarrowAndPushedToItsEnd_AndAShutOneFillsItsCell()
        {
            var (shut, shutShift) = DoorSwing.Pose(0f);
            Assert.AreEqual(1f, shut);
            Assert.AreEqual(0f, shutShift);
            var (open, openShift) = DoorSwing.Pose(1f);
            Assert.AreEqual(DoorSwing.OpenScale, open, 0.0001f);
            Assert.AreEqual((1f - DoorSwing.OpenScale) * 0.5f, openShift, 0.0001f, "the leaf's outer edge stays on the edge of its cell");
        }

        [Test]
        public void TheFarmhouse_HasAPairOfLeavesInEachDoorBetweenTheRooms()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{MapIds.FarmHouse}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            var leaves = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<DoorSwing>(true)).ToList();
            Assert.AreEqual(4, leaves.Count, "two doorways, two leaves each");
            foreach (var leaf in leaves) Assert.IsTrue(leaf.GetComponent<Collider2D>() == null || leaf.GetComponent<Collider2D>().isTrigger, leaf.name + ": a doorway is always open to walk through");
            var map = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FarmMap>(true)).Single();
            var doors = map.DoorTiles().Select(c => (c.x, c.y)).ToList();
            foreach (var leaf in leaves)
            {
                var cell = map.WorldToCell(leaf.transform.position);
                CollectionAssert.Contains(doors, (cell.x, cell.y), leaf.name + " stands on a door tile");
            }
        }
    }
}
