using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-06: the bed was tiny, the mailbox hard to see and the farmhouse bare.
    public class HomeFurnishingTests
    {
        const string SceneDir = "Assets/_Project/Scenes";

        static string[] SpritesIn(string mapId)
        {
            var scene = EditorSceneManager.OpenScene($"{SceneDir}/{mapId}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
                .Where(r => r.sprite != null).Select(r => r.sprite.name).ToArray();
        }

        [Test]
        public void TheFarmhouse_StartsFurnished_WithTheKindsOfThingsAHomeHas()
        {
            var sprites = SpritesIn(MapIds.FarmHouse);
            foreach (var kind in new[] { "obj_fireplace", "obj_painting", "obj_plant", "obj_dining_table", "obj_chair", "obj_couch", "obj_armchair", "obj_rug_large", "obj_bookshelf", "obj_lamp" })
                Assert.Contains(kind, sprites, kind + " is missing from the farmhouse");
            Assert.GreaterOrEqual(sprites.Count(s => s.StartsWith("obj_")), 15, "the farmhouse is barren");
        }

        [Test]
        public void TheBed_IsADoubleBed_TwoCellsSquare()
        {
            var scene = EditorSceneManager.OpenScene($"{SceneDir}/{MapIds.FarmHouse}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            var bed = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Bed>(true)).Single();
            Assert.AreEqual("obj_bed_double", bed.GetComponent<SpriteRenderer>().sprite.name);
            Assert.AreEqual(new Vector2(2f, 2f), bed.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(new Vector2Int(2, 2), bed.GetComponent<MovableFixture>().Size);
        }

        [Test]
        public void EveryFurnishing_CanBeMovedWithTheMallet_WithItsOwnStableId()
        {
            var scene = EditorSceneManager.OpenScene($"{SceneDir}/{MapIds.FarmHouse}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            var fixtures = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MovableFixture>(true)).ToList();
            var pieces = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
                .Where(r => r.sprite != null && r.sprite.name.StartsWith("obj_") && r.GetComponentInParent<Warp>() == null).ToList();
            foreach (var piece in pieces)
                Assert.IsNotNull(piece.GetComponent<MovableFixture>(), piece.name + " cannot be moved");
            Assert.AreEqual(fixtures.Count, fixtures.Select(f => f.Id).Distinct().Count(), "ids are unique");
            Assert.IsFalse(fixtures.Any(f => string.IsNullOrEmpty(f.Id)));
            Assert.IsTrue(fixtures.Single(f => f.Id == "furn_ruglarge").Walkable, "the rug can be walked over");
            Assert.IsFalse(fixtures.Single(f => f.Id == "furn_couch").Walkable);
        }

        [Test]
        public void TheMailbox_IsTallAndBold()
        {
            var scene = EditorSceneManager.OpenScene($"{SceneDir}/{MapIds.Farm}.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
            var box = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Mailbox>(true)).Single();
            var sprite = box.GetComponent<SpriteRenderer>().sprite;
            Assert.AreEqual("obj_mailbox_tall", sprite.name);
            Assert.GreaterOrEqual(sprite.bounds.size.y, 2f, "the mailbox reaches up two cells");
        }

        [Test]
        public void ABigFixture_CoversAllItsCells()
        {
            var go = new GameObject("fixture");
            try
            {
                var f = go.AddComponent<MovableFixture>();
                f.Size = new Vector2Int(2, 2);
                CollectionAssert.AreEquivalent(
                    new[] { new Vector3Int(3, 4, 0), new Vector3Int(4, 4, 0), new Vector3Int(3, 5, 0), new Vector3Int(4, 5, 0) },
                    f.Footprint(new Vector3Int(3, 4, 0)).ToArray());
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
