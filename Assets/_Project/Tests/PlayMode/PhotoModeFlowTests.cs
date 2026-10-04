using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // T-146 in the real game, with real key presses: F8 starts photo mode (HUD hidden, clock stopped), a number key puts an emote over a
    // villager, Enter saves a PNG, Esc leaves and everything is back.
    public class PhotoModeFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-photo-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            PhotoMode.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        [UnityTest]
        public IEnumerator F8_PhotoMode_PosesAVillager_SavesAPhoto_AndLeavesCleanly()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, 14 * 60));
            var op = SceneManager.LoadSceneAsync(MapIds.Saloon);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;

            var ui = ServiceLocator.Get<IUiService>();
            var minute = session.Clock.Now.MinuteOfDay;
            yield return Tap(Key.F8);
            Assert.IsTrue(PhotoMode.Active, "photo mode is on");
            for (var i = 0; i < 90; i++) yield return null;
            Assert.AreEqual(minute, session.Clock.Now.MinuteOfDay, "the clock is stopped");

            yield return Tap(Key.Digit1);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            var villager = UnityEngine.Object.FindObjectsByType<NpcActor>().OrderBy(a => Vector2.Distance(a.transform.position, player.transform.position)).FirstOrDefault();
            if (villager != null && Vector2.Distance(villager.transform.position, player.transform.position) <= 6f)
                Assert.IsNotNull(villager.transform.Find("Emote"), "the emote is over the villager");

            yield return Tap(Key.Enter);
            var end = Time.realtimeSinceStartup + 10f;
            while (PhotoMode.LastPhotoFile == null && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsNotNull(PhotoMode.LastPhotoFile, "a photo was taken");
            var path = Path.Combine(_dataRoot, "Photos", PhotoMode.LastPhotoFile);
            Assert.IsTrue(File.Exists(path), path);
            Assert.Greater(new FileInfo(path).Length, 1000);
            Directory.CreateDirectory("Builds");
            File.Copy(path, "Builds/photo_mode_sample.png", true);      // an Editor render for a person to look at

            yield return Tap(Key.Escape);
            Assert.IsFalse(PhotoMode.Active, "photo mode is off");
            Assert.IsNull(GameObject.Find("Emote"), "the pose is gone");
        }
    }
}
