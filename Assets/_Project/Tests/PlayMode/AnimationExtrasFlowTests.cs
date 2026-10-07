using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    public class AnimationExtrasFlowTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-animextras-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static IEnumerator Load(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 10; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator AWalker_StepsOnTheLeftFootThenTheRight_AndStandsNormallyWhenItStops()
        {
            // a 6 x 8 figure on two legs
            var tex = new Texture2D(6, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[48];
            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 6; x++)
                    if ((y >= 3 && y <= 6 && x >= 1 && x <= 4) || (y <= 2 && (x == 1 || x == 4))) px[y * 6 + x] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();
            StepFrames.ClearCache();
            var go = new GameObject("walker");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 6, 8), new Vector2(0.5f, 0f), 16f);
            renderer.sprite.name = "walker";
            go.AddComponent<WalkBob>();
            yield return null;

            var seen = new HashSet<string>();
            var until = Time.realtimeSinceStartup + 1.2f;                    // a step lasts WalkBob.StepSeconds: run in time, not in frames
            while (Time.realtimeSinceStartup < until)
            {
                go.transform.position += Vector3.right * 2f * Time.deltaTime;
                yield return null;
                seen.Add(renderer.sprite.name);
            }
            Assert.IsTrue(seen.Contains("walker_stepL"), "the left foot steps: " + string.Join(", ", seen));
            Assert.IsTrue(seen.Contains("walker_stepR"), "the right foot steps: " + string.Join(", ", seen));
            Assert.IsTrue(seen.Any(n => n == "walker_up"), "and it rises between steps");

            for (var i = 0; i < 12; i++) yield return null;                  // standing still
            Assert.AreEqual("walker", renderer.sprite.name, "back to the plain picture");
            UnityEngine.Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator TheFarmer_WalksWithSteps()
        {
            yield return Load(MapIds.Farm);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var renderer = player.GetComponentInChildren<SpriteRenderer>();
            player.Face(Vector2Int.right);
            var seen = new HashSet<string>();
            var end = Time.realtimeSinceStartup + 3f;
            var start = player.transform.position;
            while (Time.realtimeSinceStartup < end && !(seen.Any(n => n.EndsWith("_stepL")) && seen.Any(n => n.EndsWith("_stepR"))))
            {
                player.transform.position += Vector3.up * 0f + Vector3.right * 1.6f * Time.deltaTime;
                yield return null;
                seen.Add(renderer.sprite.name);
            }
            Assert.IsTrue(seen.Any(n => n.EndsWith("_stepL")) && seen.Any(n => n.EndsWith("_stepR")), "the farmer lifts each foot in turn: " + string.Join(", ", seen));
        }

        [UnityTest]
        public IEnumerator TheForestBrambles_Sway_AndTheForestPondGlints()
        {
            yield return Load(MapIds.Forest);
            var brambles = UnityEngine.Object.FindObjectsByType<ObjectSway>(FindObjectsSortMode.None).Where(s => s.name.StartsWith("Bramble")).ToList();
            Assert.GreaterOrEqual(brambles.Count, 3, "the gate's brambles sway too");

            var sparkle = UnityEngine.Object.FindAnyObjectByType<WaterSparkle>();
            Assert.IsNotNull(sparkle, "the pond has glints");
            Assert.Greater(sparkle.Count, 3);
            var map = UnityEngine.Object.FindAnyObjectByType<FarmMap>();
            foreach (var glint in sparkle.GetComponentsInChildren<SpriteRenderer>())
                Assert.IsTrue(map.IsWater(map.Ground.WorldToCell(glint.transform.position)), "every glint is over water");
            var bright = false;
            var end = Time.realtimeSinceStartup + 4f;
            while (!bright && Time.realtimeSinceStartup < end)
            {
                yield return null;
                bright = sparkle.GetComponentsInChildren<SpriteRenderer>().Any(r => r.color.a > 0.3f);
            }
            Assert.IsTrue(bright, "a glint flashes");
        }

        [UnityTest]
        public IEnumerator WalkingThroughADoor_FlashesItOpen()
        {
            yield return Load(MapIds.Farm);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var warp = UnityEngine.Object.FindObjectsByType<Warp>(FindObjectsSortMode.None).First(w => w.TargetMap == MapIds.FarmHouse && w.IsOpen(out _));      // a real door: the road to the village shows none
            var enter = typeof(Warp).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
            enter.Invoke(warp, new object[] { player.GetComponent<Collider2D>() });
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<DoorFlash>(), "the doorway opens as the farmer steps in");
        }

        [UnityTest]
        public IEnumerator TheFarmer_CarriesWhatIsSelected_HoistsAFind_AndFishesWithARod()
        {
            yield return Load(MapIds.Farm);
            var session = ServiceLocator.Get<GameSession>();
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var held = HeldItem.For(player.gameObject);
            yield return null;

            session.Backpack.Add("crop.parsnip", 3);
            var slot = Enumerable.Range(0, InputNames.HotbarSlots).First(i => session.Backpack.Get(i) != null && session.Backpack.Get(i).ItemId == "crop.parsnip");
            session.State.SelectedHotbar = slot;
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsTrue(held.IsCarrying, "a crop is carried");
            Assert.IsNotNull(held.ShownSprite);

            session.State.SelectedHotbar = Enumerable.Range(0, InputNames.HotbarSlots).First(i => session.Backpack.Get(i) != null && session.Backpack.Get(i).ItemId == "tool.hoe");
            for (var i = 0; i < 3; i++) yield return null;
            Assert.IsFalse(held.IsCarrying, "a tool is not");
            Assert.IsNull(held.ShownSprite);

            held.Hoist(session.Db.GetItem("crop.parsnip").Icon);
            yield return null;
            Assert.IsTrue(held.IsHoisting);
            Assert.IsNotNull(held.ShownSprite, "the find is held up");

            var pose = FishingPose.For(player.gameObject);
            var target = player.transform.position + new Vector3(2f, 0f, 0f);
            pose.Show(target);
            yield return null;
            Assert.IsTrue(pose.IsActive);
            Assert.That(Vector3.Distance(pose.BobberPosition, target), Is.LessThan(0.1f), "the bobber sits where the cast landed");
            Assert.IsTrue(pose.GetComponentsInChildren<SpriteRenderer>().Count(r => r.enabled && r.name.StartsWith("Rod")) == 1, "the rod is drawn");
            pose.Hide();
            Assert.IsFalse(pose.IsActive);
            Assert.IsFalse(pose.GetComponentsInChildren<SpriteRenderer>().Any(r => r.enabled && (r.name == "Rod" || r.name == "Line")), "and gone again");
        }
    }
}
