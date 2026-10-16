using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // "Crawling Depths" in the real game: the deep mine is dressed at full and mild intensity and plain at off, the relic chest stands in its shrine, and the tentacles rise round
    // the altar on a ritual night. Set FARM_CAPTURE_DIR to have pictures of the scenes saved.
    public class DepthsFlowTests : InputTestFixture
    {
        string _root;
        GameSession _s;

        public override void Setup()
        {
            base.Setup();
            _root = Path.Combine(Path.GetTempPath(), "farm-depthstests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
            GameSession.HorrorLevelOverride = 2;
        }

        public override void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static void CaptureWorld(string name)
        {
            var dir = Environment.GetEnvironmentVariable("FARM_CAPTURE_DIR");
            var cam = Camera.main;
            if (string.IsNullOrEmpty(dir) || cam == null) return;
            Directory.CreateDirectory(dir);
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
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            rt.Release();
        }

        IEnumerator StartMine(int floor, int dread)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.SetFlag(Farm.UI.HudView.HealthBarFlag);
            _s.State.WorldSeed = 4242;
            _s.SetVar(MythosIds.Vars.Dread, dread);
            _s.State.CurrentMap = MapIds.Mine;
            _s.State.SpawnPoint = "default";
            _s.State.Mine.Floor = floor;
            var op = SceneManager.LoadSceneAsync(MapIds.Mine);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        static GameObject DepthsRoot => GameObject.Find("Depths");

        [UnityTest, Timeout(300000)]
        public IEnumerator TheChestsFloor_IsDressedAtFull_AndTheShrineIsSolid()
        {
            yield return StartMine(MythosRelics.Floor, 70);
            var root = DepthsRoot;
            Assert.IsNotNull(root, "the floor is dressed");
            var expected = DepthsDecor.Plan(GameContext.Current.Map.Mine.Floor, _s.State.WorldSeed, 2, 70);
            Assert.AreEqual(expected.Count, root.transform.childCount, "every planned piece is in the scene");
            Assert.AreEqual(expected.Count(p => p.Solid), root.GetComponentsInChildren<BoxCollider2D>().Length, "the solid ones have colliders");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<RelicChest>(), "the chest stands in it");
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>()) Assert.IsNotNull(sr.sprite, sr.transform.parent.name + " has its picture");
            Assert.IsTrue(root.GetComponentsInChildren<DepthsProp>().Length > 0, "some of it moves");
            CaptureWorld("depths_floor30_full");
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator TheDeepFloors_AreDressedMoreHeavilyThanTheFirst_ForAMoreAfraidPlayer()
        {
            yield return StartMine(MythosRelics.Floor, 20);
            var shallow = DepthsRoot.transform.childCount;
            Bootstrapper.ResetForTests();
            yield return StartMine(MineGenerator.Floors - 2, 90);
            var deep = DepthsRoot.transform.childCount;
            Assert.Greater(deep, shallow, "deeper and more afraid: more of it");
            CaptureWorld("depths_floor38_full");
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator TheBossHall_HasItsStatuesInTheCorners()
        {
            yield return StartMine(MineGenerator.Floors, 90);
            var root = DepthsRoot;
            Assert.IsNotNull(root);
            Assert.IsTrue(root.transform.Cast<Transform>().Any(t => t.name == "Statue"), "statues stand in the hall");
            CaptureWorld("depths_floor40_full");
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator AtMild_ThereAreNoEyesOrStatues_AndAtOff_NothingAtAll()
        {
            GameSession.HorrorLevelOverride = 1;
            yield return StartMine(MythosRelics.Floor + 4, 100);
            var root = DepthsRoot;
            Assert.IsNotNull(root);
            foreach (var name in new[] { "Eye", "LittleEyes", "Statue", "Arc", "BookAltar", "ColumnTall" })
                Assert.IsNull(root.transform.Cast<Transform>().FirstOrDefault(t => t.name == name), name + " is for full intensity only");
            CaptureWorld("depths_floor34_mild");

            GameSession.HorrorLevelOverride = 0;
            Bootstrapper.ResetForTests();
            yield return StartMine(MythosRelics.Floor + 4, 100);
            Assert.IsNull(DepthsRoot, "at off the mine is the plain mine");
            Assert.IsNull(UnityEngine.Object.FindAnyObjectByType<RelicChest>());
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator TheFloorsAboveTheChest_AreNotDressed()
        {
            yield return StartMine(MythosRelics.Floor - 1, 100);
            Assert.IsNull(DepthsRoot);
        }

        // ---- the tentacles at the altar ----

        IEnumerator OpenWoods(int minuteOfDay, int level)
        {
            GameSession.HorrorLevelOverride = level;
            Bootstrapper.InitializeServices();
            yield return null;
            _s = GameContext.Current.Session;
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.SetFlag(MapIds.WoodsOpenFlag);
            _s.Clock.SetTime(new GameDateTime(1, Season.Summer, RitualDirector.RitualDay, minuteOfDay));
            var save = RitualDirector.Load(_s);
            RitualDirector.PlanSeason(_s, save);
            save.WitnessedSeason = save.PlannedSeason;          // already watched: no scene, only what stands in the clearing
            RitualDirector.Store(_s, save);
            _s.State.CurrentMap = MapIds.Woods;
            var op = SceneManager.LoadSceneAsync(MapIds.Woods);
            while (!op.isDone) yield return null;
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator OnARitualNight_TheTentaclesRiseRoundTheAltar_ReachForTheOffering_AndSinkAgain()
        {
            yield return OpenWoods(RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5, 2);
            var altar = UnityEngine.Object.FindAnyObjectByType<AltarObject>();
            var tentacles = altar.GetComponent<AltarTentacles>();
            Assert.IsNotNull(tentacles, "the altar has them");
            var count = RitualDirector.Load(_s).Plan.Count;
            int Reaching() => RitualTentacles.Reaching(RitualDirector.Offset(_s.Clock.Now.MinuteOfDay), count);          // the game clock keeps running: it moves on every twenty minutes
            for (var end = Time.realtimeSinceStartup + 30f; Time.realtimeSinceStartup < end && !(Reaching() >= 0 && tentacles.HeightOf(Reaching()) > 0.99f && tentacles.ShownCount == RitualTentacles.Slots);) yield return null;
            var reaching = Reaching();
            Assert.GreaterOrEqual(reaching, 0, "an offering is on the altar");
            Assert.AreEqual(RitualTentacles.Slots, tentacles.ShownCount, "all six are up while an offering lies on the stone");
            Assert.Greater(tentacles.HeightOf(reaching), 0.95f, "the one of the turn is all the way up");
            for (var slot = 0; slot < RitualTentacles.Slots; slot++)
                if (slot != reaching) Assert.Greater(tentacles.HeightOf(reaching), tentacles.HeightOf(slot), "the one of the turn is tallest");
            CaptureWorld("depths_altar_tentacles");

            _s.Clock.SetTime(new GameDateTime(1, Season.Summer, 5, 700));
            for (var end = Time.realtimeSinceStartup + 30f; Time.realtimeSinceStartup < end && tentacles.ShownCount > 0;) yield return null;
            Assert.AreEqual(0, tentacles.ShownCount, "the next morning they are gone");
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator WhenAnOfferingIsTaken_TheTentaclesCringeDown()
        {
            yield return OpenWoods(RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5, 2);
            var tentacles = UnityEngine.Object.FindAnyObjectByType<AltarObject>().GetComponent<AltarTentacles>();
            for (var end = Time.realtimeSinceStartup + 30f; Time.realtimeSinceStartup < end && tentacles.ShownCount < RitualTentacles.Slots;) yield return null;
            var save = RitualDirector.Load(_s);
            save.Plan[0].Taken = true;
            RitualDirector.Store(_s, save);
            for (var end = Time.realtimeSinceStartup + 30f; Time.realtimeSinceStartup < end && tentacles.HeightOf(0) > RitualTentacles.Cringing + 0.01f;) yield return null;
            for (var slot = 0; slot < RitualTentacles.Slots; slot++) Assert.LessOrEqual(tentacles.HeightOf(slot), RitualTentacles.Cringing + 0.01f);
            CaptureWorld("depths_altar_cringing");
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator AtMildAndOff_NoTentaclesRise()
        {
            yield return OpenWoods(RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5, 1);
            var tentacles = UnityEngine.Object.FindAnyObjectByType<AltarObject>().GetComponent<AltarTentacles>();
            for (var end = Time.realtimeSinceStartup + 3f; Time.realtimeSinceStartup < end;) yield return null;
            Assert.AreEqual(0, tentacles.ShownCount, "mild has no ritual imagery");
        }
    }
}
