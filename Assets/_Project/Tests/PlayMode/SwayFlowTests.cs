using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Plants lean in the wind: growing crops lean through their tile matrix, trees through a child that turns about the base.
    public class SwayFlowTests : PlayModeFixture
    {

        static IEnumerator Load(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AGrowingCrop_LeansWithTheWind_AndASeedDoesNot()
        {
            yield return Load(MapIds.Farm);
            var session = ServiceLocator.Get<GameSession>();
            var view = UnityEngine.Object.FindAnyObjectByType<FarmMapView>();
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            var grid = session.GetGrid(MapIds.Farm);
            session.Db.TryGetCrop("parsnip", out var parsnip);
            grid.Till(21, 10); grid.Plant(21, 10, parsnip, Season.Spring);
            grid.Till(23, 10); grid.Plant(23, 10, parsnip, Season.Spring);
            grid.TryGetTile(21, 10, out var grown); grown.Crop.Stage = parsnip.MatureStage;
            view.RefreshAll();

            var grownCell = new Vector3Int(21, 10, 0);
            var seedCell = new Vector3Int(23, 10, 0);
            var leans = false;
            for (var t = 0f; t < Sway.Period; t += 0.1f)
            {
                view.ApplySway(t);
                var m = map.Crops.GetTransformMatrix(grownCell);
                Assert.AreEqual(Sway.LeanPixels(t, 21, 10) / 16f, m.m01, 0.0001f, "the matrix follows the wind");
                if (m != Matrix4x4.identity) leans = true;
                Assert.AreEqual(Matrix4x4.identity, map.Crops.GetTransformMatrix(seedCell), "a seed stays upright");
            }
            Assert.IsTrue(leans, "it leans at some point in a breath");

            view.RefreshCell(grownCell);
            Assert.AreEqual(Matrix4x4.identity, map.Crops.GetTransformMatrix(grownCell), "a redrawn tile starts upright");
            view.ApplySway(0.4f);
            Assert.AreEqual(Sway.LeanPixels(0.4f, 21, 10) / 16f, map.Crops.GetTransformMatrix(grownCell).m01, 0.0001f, "and is leaned again at the next pass");
        }

        [UnityTest]
        public IEnumerator ATree_LeansOnAChild_AndItsColliderStaysPut()
        {
            yield return Load(MapIds.Village);
            var trees = UnityEngine.Object.FindObjectsByType<ObjectSway>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(trees.Length, 4, "the village trees sway");
            var tree = trees.First(t => t.GetComponent<BoxCollider2D>() != null);          // the edge trees have no collider of their own (the band under them blocks)
            var collider = tree.GetComponent<BoxCollider2D>();
            var colliderAt = collider.bounds.center;
            Assert.IsFalse(tree.GetComponent<SpriteRenderer>().enabled, "the picture is on the swaying child");
            var pivot = tree.transform.Find("SwayPivot");
            Assert.IsNotNull(pivot);
            var turned = false;
            var end = Time.realtimeSinceStartup + 4f;
            while (!turned && Time.realtimeSinceStartup < end)
            {
                turned = trees.Any(t => t.transform.Find("SwayPivot").localRotation != Quaternion.identity);
                yield return null;
            }
            Assert.IsTrue(turned, "some tree turns within a few seconds");
            Assert.AreEqual(colliderAt, collider.bounds.center, "the collider does not move");
        }
    }
}
