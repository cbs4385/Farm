using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The black cat in the real village scene (owner request, 2026-10-05): it is there by day, walks on open ground, and can be petted.
    public class VillageCatFlowTests : PlayModeFixture
    {

        [UnityTest]
        public IEnumerator TheBlackCat_IsInTheVillageByDay_WalksOnOpenGround_AndCanBePetted()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 10 * 60));
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            var end = Time.realtimeSinceStartup + 30f;
            VillageCat cat = null;
            while (cat == null && Time.realtimeSinceStartup < end) { cat = UnityEngine.Object.FindAnyObjectByType<VillageCat>(); yield return null; }
            Assert.IsNotNull(cat, "the village has a cat");
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsTrue(cat.IsShown, "it is out in the daytime");
            Assert.AreEqual("Black cat", cat.HoverLabel);
            Assert.IsNotNull(cat.GetComponent<BoxCollider2D>(), "the player can reach it");

            var npcs = NpcManager.Current;
            var grid = npcs.Grid();
            var start = cat.Cell;
            var moved = false;
            var until = Time.realtimeSinceStartup + 25f;
            while (Time.realtimeSinceStartup < until)
            {
                yield return null;
                if (cat.Cell != start) moved = true;
                Assert.IsTrue(grid.IsWalkable(cat.Cell.x, cat.Cell.y), "the cat stays on open ground: " + cat.Cell);
                if (moved) break;
            }
            Assert.IsTrue(moved, "the cat walks somewhere");

            Assert.DoesNotThrow(() => cat.Interact(null));
            Assert.IsFalse(cat.IsWalking, "it stops to be petted");

            // Playtest report (2026-10-05): "there is no animation when petting the village cat." It purrs: hearts rise and it bounces.
            Assert.IsTrue(cat.IsPurring, "it purrs");
            var bob = cat.GetComponent<WalkBob>();
            Assert.IsNotNull(bob, "the cat can bounce");
            Assert.Greater(UnityEngine.Object.FindObjectsByType<ActionPuff>(FindObjectsSortMode.None).Length, 0, "hearts rise");
            var bounced = false;
            var purrEnd = Time.realtimeSinceStartup + VillageCat.PurrSeconds + 0.5f;
            while (Time.realtimeSinceStartup < purrEnd) { yield return null; bounced |= bob.IsLunging; }
            Assert.IsTrue(bounced, "it bounces while it purrs");
            Assert.IsFalse(cat.IsPurring, "and the purr ends");
        }
    }
}
