using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The witnessed ritual, in the real game with real keys (2026-10-10): the player is in Harrow Wood at ten past ten on the fourth night of the summer, the Keepers stand
    // robed round the altar, and the scene plays to its end: once for a player who stays hidden (the ritual succeeds) and once for one who steps out and takes the offering
    // (it fails). Either way the stage is left as it was found.
    public class RitualWitnessFlowTests : InputTestFixture
    {
        string _root;
        Keyboard _keyboard;
        GameSession _session;
        IUiService Ui => GameContext.Current.Ui;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-ritualwitness-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            GameSession.HorrorLevelOverride = 2;
        }

        public override void TearDown()
        {
            Time.timeScale = 1f;
            GameSession.HorrorLevelOverride = null;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        // The world as the camera sees it (not the windows over it), saved as a picture.
        static void CaptureWorld(string path)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var rt = new RenderTexture(1280, 720, 24);
            var before = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = before;
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            rt.Release();
        }

        IEnumerator Tap(Key key) { Press(_keyboard[key]); yield return null; Release(_keyboard[key]); yield return null; }

        IEnumerator OpenWoodsAtTheRitual()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.SetFlag(MapIds.WoodsOpenFlag);
            _session.SetFlag(MythosIds2.Interference);
            _session.Clock.SetTime(new GameDateTime(1, Season.Summer, RitualDirector.RitualDay, RitualDirector.StartMinute + 10));
            var save = RitualDirector.Load(_session);
            RitualDirector.PlanSeason(_session, save);
            RitualDirector.Store(_session, save);
            _session.State.CurrentMap = MapIds.Woods;
            _session.State.SpawnPoint = "default";
            var op = SceneManager.LoadSceneAsync(MapIds.Woods);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        // Plays the scene by tapping Enter (and, with `snatch`, choosing the first answer at the choice). Returns what the Keepers wore when the first line came up.
        IEnumerator Watch(bool snatch, Action<string> robes)
        {
            var finished = false;
            GameContext.Current.Bus.Subscribe<EventFinished>(e => { if (e.EventId == "mythos_ritual_witness") finished = true; });
            string worn = null;
            var shots = 0;
            var captureTo = Environment.GetEnvironmentVariable("FARM_CAPTURE_DIR");          // for looking at the scene: set it and the frames are saved
            if (!string.IsNullOrEmpty(captureTo)) Directory.CreateDirectory(captureTo);
            var end = Time.realtimeSinceStartup + 150f;
            while (!finished && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen)
                {
                    worn ??= string.Join(",", MythosCast.Keepers.Select(k => GameContext.Current.Map.Npcs.Actors.TryGetValue(k, out var a) ? a.Outfit : "absent"));
                    if (!string.IsNullOrEmpty(captureTo)) CaptureWorld(Path.Combine(captureTo, $"ritual_{(snatch ? "snatch" : "hidden")}_{shots++:00}.png"));
                    yield return Tap(snatch ? Key.Digit1 : Key.Digit2);          // at the choice: step out (1) or stay hidden (2); nothing at an ordinary line
                    yield return Tap(Key.Enter);
                }
                else yield return null;
            }
            Assert.IsTrue(finished, "the scene ran to its end");
            robes(worn);
            for (var i = 0; i < 30; i++) yield return null;
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator StayingHidden_TheRitualSucceeds_TheClockReachesItsEnd_AndTheStageIsClean()
        {
            yield return OpenWoodsAtTheRitual();
            string worn = null;
            yield return Watch(false, w => worn = w);
            Assert.AreEqual("robe,robe,robe,robe,robe", worn, "every Keeper is robed at the altar");

            var save = RitualDirector.Load(_session);
            var duration = RitualModel.DurationMinutes(save.Plan.Count);
            Assert.GreaterOrEqual(_session.Clock.Now.MinuteOfDay, RitualDirector.StartMinute + duration, "the clock moved through the whole ritual");
            Assert.AreEqual(save.PlannedSeason, save.ResolvedSeason, "the ritual was resolved");
            Assert.IsTrue(save.LastSucceeded, "nobody interfered");
            Assert.AreEqual(1, save.RitualsWitnessed);
            Assert.IsFalse(RitualScene.Fresh(_session), "it is not played again this season");
            Assert.IsFalse(((UiService)Ui).LetterboxOn, "the bars are gone");
            Assert.IsEmpty(_session.PendingEvents);
            Assert.Greater(_session.GetVar(MythosIds.Vars.Lore), 0);
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator SteppingOutToTakeTheOffering_MakesTheRitualFail_AndTheOfferingIsTheirs()
        {
            yield return OpenWoodsAtTheRitual();
            var save0 = RitualDirector.Load(_session);
            var item = save0.Plan.Last().ItemId;
            var before = _session.Backpack.Count(item);
            yield return Watch(true, w => { });

            var save = RitualDirector.Load(_session);
            Assert.AreEqual(save.PlannedSeason, save.ResolvedSeason, "the ritual was resolved");
            Assert.IsFalse(save.LastSucceeded, "a ritual with an offering missing fails");
            Assert.IsTrue(save.Plan.Last().Taken);
            Assert.Greater(_session.Backpack.Count(item), before, "the offering is in the pack");
            Assert.Less(_session.GetVar(MythosIds.Vars.CultStanding), 0, "the Keepers think less of the one who took it");
            Assert.IsFalse(((UiService)Ui).LetterboxOn);
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator AtMildIntensity_ThereIsNoSceneAndNoRobes()
        {
            GameSession.HorrorLevelOverride = 1;
            yield return OpenWoodsAtTheRitual();
            for (var i = 0; i < 120; i++) yield return null;
            Assert.IsFalse(GameContext.Current.Map.Events != null && GameContext.Current.Map.Events.IsPlaying, "no scene at mild");
            foreach (var keeper in MythosCast.Keepers)
                if (GameContext.Current.Map.Npcs.Actors.TryGetValue(keeper, out var actor)) Assert.IsNull(actor.Outfit, keeper + " is not robed at mild");
            Assert.IsTrue(RitualScene.Fresh(_session));
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator OnARitualNight_TheAltarBurnsItsCandles_ShowsTheOfferingAndLightsTheClearing()
        {
            // the scene is not wanted here: only what stands in the clearing
            Bootstrapper.InitializeServices();
            yield return null;
            _session = GameContext.Current.Session;
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.SetFlag(MapIds.WoodsOpenFlag);
            _session.Clock.SetTime(new GameDateTime(1, Season.Summer, RitualDirector.RitualDay, RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5));
            var save = RitualDirector.Load(_session);
            RitualDirector.PlanSeason(_session, save);
            save.WitnessedSeason = save.PlannedSeason;          // already watched: no scene
            RitualDirector.Store(_session, save);
            _session.State.CurrentMap = MapIds.Woods;
            var op = SceneManager.LoadSceneAsync(MapIds.Woods);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 40; i++) yield return null;

            var altar = UnityEngine.Object.FindAnyObjectByType<AltarObject>();
            Assert.IsNotNull(altar);
            Assert.IsTrue(altar.CandlesLit, "the candles are lit");
            Assert.IsTrue(altar.OfferingShown, "the offering lies on the stone as its own picture");
            var light = altar.GetComponentInChildren<FlickerLight>();
            Assert.IsNotNull(light, "the clearing is lit by the candles");
            Assert.Greater(light.CurrentIntensity, 0.5f);
            foreach (var keeper in MythosCast.Keepers)
                if (GameContext.Current.Map.Npcs.Actors.TryGetValue(keeper, out var actor)) Assert.AreEqual("robe", actor.Outfit, keeper);

            // the next morning the clearing is dark again
            _session.Clock.SetTime(new GameDateTime(1, Season.Summer, 5, 700));
            for (var i = 0; i < 20; i++) yield return null;
            Assert.IsFalse(altar.CandlesLit);
        }
    }
}
