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
    // T-131 in the real game, with real key presses: a scene's `anim` step puts the villager in a pose sprite (Piper points at the umbrella),
    // and the idle sprite is back when the scene is over.
    public class PosesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-poses-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
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
        public IEnumerator PiperPointsAtTheUmbrella_AndGoesBackToIdle()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            var session = ServiceLocator.Get<GameSession>();
            session.BeginNewGame("Tester", "Test Farm", 0);
            session.SetFlag(FatigueModel.WarnedFlag);
            session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.DialogueSpeed = DialoguePacing.InstantSpeed;
            session.Clock.SetTime(new GameDateTime(1, Season.Spring, 5, 12 * 60));
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 15; i++) yield return null;

            var finished = false;
            ServiceLocator.Get<EventBus>().Subscribe<EventFinished>(e => { if (e.EventId == "story_umbrella") finished = true; });
            Assert.IsTrue(EventRunner.Trigger(session, "story_umbrella"));
            var ui = ServiceLocator.Get<IUiService>();
            string posed = null;
            var end = Time.realtimeSinceStartup + 60f;
            while (!finished && Time.realtimeSinceStartup < end)
            {
                var piper = UnityEngine.Object.FindObjectsByType<NpcActor>().FirstOrDefault(a => a.name == "Npc_piper");
                if (piper != null && piper.PoseShown != null && posed == null)
                {
                    posed = piper.PoseShown;
                    Directory.CreateDirectory("Builds");
                    var cam = Camera.main;
                    var camPos = cam.transform.position;
                    cam.transform.position = new Vector3(piper.transform.position.x, piper.transform.position.y, camPos.z);
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
                    File.WriteAllBytes("Builds/pose_piper_point.png", tex.EncodeToPNG());      // an Editor render for a person to look at
                    UnityEngine.Object.Destroy(tex);
                }
                if (ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null;
            }
            Assert.IsTrue(finished, "the scene ran to its end");
            Assert.IsNotNull(posed, "Piper was in a pose sprite during the scene");
            StringAssert.Contains("pose_point", posed);
            for (var i = 0; i < 30; i++) yield return null;
            var after = UnityEngine.Object.FindObjectsByType<NpcActor>().FirstOrDefault(a => a.name == "Npc_piper");
            if (after != null) Assert.IsNull(after.PoseShown, "back to the idle sprite");
        }
    }
}
