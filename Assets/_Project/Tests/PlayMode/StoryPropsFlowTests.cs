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
    // The story props in the real game: a storyline scene puts its prop on the map (a cat, an umbrella ...), a capture is saved for a
    // person to look at, and the prop is gone when the scene ends.
    public class StoryPropsFlowTests
    {
        string _dataRoot;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-props-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Show(string sceneId, string capture)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, 12 * 60));
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;
            Assert.IsTrue(EventRunner.Trigger(session, sceneId), sceneId);
            var end = Time.realtimeSinceStartup + 20f;
            GameObject prop = null;
            while (prop == null && Time.realtimeSinceStartup < end) { prop = GameObject.Find("Prop_story_prop_0"); yield return null; }
            Assert.IsNotNull(prop, sceneId + " shows its prop");
            Assert.IsNotNull(prop.GetComponent<SpriteRenderer>().sprite, "the prop has a sprite");
            for (var i = 0; i < 40; i++) yield return null;
            Directory.CreateDirectory("Builds");
            var cam = Camera.main;
            var camPos = cam.transform.position;
            cam.transform.position = new Vector3(prop.transform.position.x, prop.transform.position.y, camPos.z);      // look at the scene, not at the player
            var rt = RenderTexture.GetTemporary(960, 540, 24);
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;
            cam.transform.position = camPos;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes("Builds/" + capture, tex.EncodeToPNG());      // an Editor render for a person to look at
            UnityEngine.Object.Destroy(tex);
        }

        [UnityTest] public IEnumerator TheCat_IsOnTheVillageGreen() { yield return Show("story_cat", "prop_cat.png"); }
        [UnityTest] public IEnumerator TheUmbrella_IsOnTheVillageGreen() { yield return Show("story_umbrella", "prop_umbrella.png"); }
        [UnityTest] public IEnumerator ThePumpkin_IsOnTheVillageGreen() { yield return Show("story_pumpkin_dorian", "prop_pumpkin.png"); }
    }
}
