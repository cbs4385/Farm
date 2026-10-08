using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using Farm.Platform;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Milestone 4 (polish and platform): crash log, platform and achievements, settings (accessibility, quality of life),
    // archived saves from earlier milestones, and the soak runs.
    public class Milestone4Tests
    {
        const string SavesDir = "Assets/_Project/Tests/EditMode/Saves/";

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            PlatformServices.Reset();
            Achievements.ClearExtraForTests();
            Conditions.ClearCustomForTests();
            RealDb().ClearMergedPacks();
        }

        // ---- crash log -----------------------------------------------------------------------------------------------------

        [Test]
        public void CrashLog_WritesErrors_SkipsPlainLogs_AndKeepsThePreviousFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), "farm-log-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                using (var first = new CrashLog(dir))
                {
                    first.OnLogMessage("hello", "", LogType.Log);
                    first.OnLogMessage("it broke", "at Foo.Bar", LogType.Exception);
                    var text = File.ReadAllText(first.FilePath);
                    StringAssert.Contains("it broke", text);
                    StringAssert.Contains("at Foo.Bar", text);
                    StringAssert.DoesNotContain("hello", text);
                }
                using (var second = new CrashLog(dir))
                {
                    Assert.IsTrue(File.Exists(Path.Combine(dir, CrashLog.PreviousName)));
                    StringAssert.Contains("it broke", File.ReadAllText(Path.Combine(dir, CrashLog.PreviousName)));
                    StringAssert.DoesNotContain("it broke", File.ReadAllText(second.FilePath));
                }
            }
            finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
        }

        // ---- platform and achievements -------------------------------------------------------------------------------------

        sealed class RecordingPlatform : IPlatformServices
        {
            public readonly System.Collections.Generic.List<string> Unlocked = new System.Collections.Generic.List<string>();
            public string Name => "test";
            public bool IsOnline => true;
            public void UnlockAchievement(string id) => Unlocked.Add(id);
        }

        [Test]
        public void ThePlatformDefaultsToNone_AndAchievementsUnlockOnce()
        {
            Assert.AreEqual("none", PlatformServices.Current.Name);
            var platform = new RecordingPlatform();
            PlatformServices.Use(platform);
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                Assert.IsEmpty(Achievements.Check(s));
                s.SetFlag("hall.restored");                       // a flag change checks at once
                Assert.IsEmpty(Achievements.Check(s), "once only");
                CollectionAssert.AreEqual(new[] { "hall" }, platform.Unlocked);
                Assert.IsTrue(Achievements.IsEarned(s, "hall"));
                Assert.That(f.Toasts, Has.Some.Contains("Achievement"));
            }
        }

        [Test]
        public void EarnedAchievements_AreResentToThePlatformAfterLoading()
        {
            var platform = new RecordingPlatform();
            PlatformServices.Use(platform);
            using (var f = new TestSessionFixture())
            {
                f.Session.SetFlag(Achievements.FlagFor("second_year"));
                Achievements.SyncToPlatform(f.Session);
                CollectionAssert.AreEqual(new[] { "second_year" }, platform.Unlocked);
            }
        }

        [Test]
        public void EveryAchievement_HasSpoilerFreeText_AndTheLayerRegistersItsOwn()
        {
            GameSession.HorrorLevelOverride = 2;
            using (var f = new TestSessionFixture())
            {
                f.Session.Story = null;
                MythosModule.Install(f.Session, f.Session.Hooks, f.Bus);
                var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
                Assert.IsTrue(Achievements.All.Any(a => a.Id == "end_sealed"));
                foreach (var a in Achievements.All)
                {
                    Assert.IsTrue(table.ContainsKey(a.TitleKey), a.Id);
                    Assert.IsTrue(table.ContainsKey(a.DescriptionKey), a.Id);
                    StringAssert.DoesNotContain("Nharoth", table[a.TitleKey] + table[a.DescriptionKey]);
                    StringAssert.DoesNotContain("Keeper", table[a.TitleKey] + table[a.DescriptionKey]);
                }
            }
        }

        // ---- settings ------------------------------------------------------------------------------------------------------

        [Test]
        public void OldSettingsFiles_GetSafeDefaultsForTheNewOptions()
        {
            var s = JsonUtility.FromJson<SettingsData>("{\"Version\":1,\"MasterVolume\":0.5,\"HorrorLevel\":1}");
            s.Clamp();
            Assert.IsFalse(s.ColorblindPalette);
            Assert.IsFalse(s.RelaxedEnergy);
            Assert.AreEqual(1, s.DayLength);
            Assert.AreEqual(GameClock.DefaultSecondsPerStep, s.SecondsPerStep);
            s.DayLength = 9;
            s.Clamp();
            Assert.AreEqual(SettingsData.DayLengthCount - 1, s.DayLength);
        }

        [Test]
        public void RelaxedEnergy_HalvesTheCost_AndNeverMakesAnActionFree()
        {
            var store = new SettingsStore(Path.GetTempPath());
            ServiceLocator.Register(store);
            try
            {
                using (var f = new TestSessionFixture())
                {
                    var s = f.Session;
                    s.SetEnergy(100);
                    s.TrySpendEnergy(10);
                    Assert.AreEqual(90, s.State.Energy);
                    store.Current.RelaxedEnergy = true;
                    s.TrySpendEnergy(10);
                    Assert.AreEqual(85, s.State.Energy);
                    s.TrySpendEnergy(1);
                    Assert.AreEqual(84, s.State.Energy, "a cost of 1 stays 1");
                }
            }
            finally { ServiceLocator.Clear(); }
        }

        // A tester who plays from an exercise bike wanted a day of about 45 minutes (2026-10-08).
        [Test]
        public void ThereIsADayLength_OfAboutFortyFiveMinutes_AndTheOthersAreInOrder()
        {
            Assert.AreEqual(4, SettingsData.DayLengthCount);
            Assert.That(SettingsData.RealMinutesPerDay(3), Is.InRange(43f, 47f), "very long: about 45 minutes");
            Assert.Greater(SettingsData.RealMinutesPerDay(3), SettingsData.RealMinutesPerDay(0), "longer than the long day");
            Assert.Greater(SettingsData.RealMinutesPerDay(0), SettingsData.RealMinutesPerDay(1));
            Assert.Greater(SettingsData.RealMinutesPerDay(1), SettingsData.RealMinutesPerDay(2));
            var s = new SettingsData { DayLength = 3 };
            s.Clamp();
            Assert.AreEqual(3, s.DayLength, "the new setting survives");
            Assert.AreEqual(19f, s.SecondsPerStep);
            foreach (var key in new[] { "options.day_length.0", "options.day_length.1", "options.day_length.2", "options.day_length.3" })
                Assert.IsTrue(L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json")).ContainsKey(key), key);
        }

        [Test]
        public void DayLength_ChangesTheClockSpeed()
        {
            var store = new SettingsStore(Path.GetTempPath());
            ServiceLocator.Register(store);
            try
            {
                store.Current.DayLength = 0;
                using (var f = new TestSessionFixture())
                {
                    Assert.AreEqual(10f, f.Session.Clock.SecondsPerStep);
                    store.Current.DayLength = 2;
                    f.Session.ApplySettings();
                    Assert.AreEqual(5f, f.Session.Clock.SecondsPerStep);
                }
            }
            finally { ServiceLocator.Clear(); }
        }

        [Test]
        public void TheColourBlindPalette_SeparatesTheBars()
        {
            var store = new SettingsStore(Path.GetTempPath());
            ServiceLocator.Register(store);
            try
            {
                var normal = new[] { UI.UiPalette.Energy, UI.UiPalette.Health, UI.UiPalette.Fatigue, UI.UiPalette.Low };
                store.Current.ColorblindPalette = true;
                var alt = new[] { UI.UiPalette.Energy, UI.UiPalette.Health, UI.UiPalette.Fatigue, UI.UiPalette.Low };
                for (var i = 0; i < normal.Length; i++) Assert.AreNotEqual(normal[i], alt[i]);
                Assert.AreEqual(alt.Length, alt.Distinct().Count());
            }
            finally { ServiceLocator.Clear(); }
        }

        // ---- archived saves --------------------------------------------------------------------------------------------------

        static void PlaceSave(string root, string fixture)
        {
            var saves = new SaveService(root);
            AtomicFile.Write(saves.SlotPath(0), File.ReadAllText(SavesDir + fixture));
        }

        [TestCase("save_m1.json", 1, "Old")]
        [TestCase("save_m2_m3.json", 2, "Mid")]
        public void SavesFromEarlierMilestones_Load_AndPlayOnWithTheLayerAtEveryLevel(string fixture, int season, string name)
        {
            var root = Path.Combine(Path.GetTempPath(), "farm-compat-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var plain = new SaveService(root);
                PlaceSave(root, fixture);
                Assert.IsTrue(plain.TryLoad(0, out var state, out var error), error);
                Assert.AreEqual(name, state.PlayerName);
                Assert.AreEqual(season, state.SeasonIndex);
                Assert.IsNotNull(state.Vars);
                Assert.IsNotNull(state.ModuleData);

                foreach (var level in new[] { 0, 1, 2 })
                {
                    GameSession.HorrorLevelOverride = level;
                    using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops))
                    {
                        f.Session.Story = StoryContent.LoadFromResources();
                        foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) f.Session.Db.Merge(pack);
                        MythosModule.Install(f.Session, f.Session.Hooks, f.Bus);
                        Assert.IsTrue(plain.TryLoad(0, out var fresh, out error), error);
                        f.Session.BeginState(fresh);
                        for (var d = 0; d < 20; d++) f.Session.EndDay(false);
                        Assert.GreaterOrEqual(f.Session.State.Gold, 0);
                        if (level == 0) Assert.AreEqual(0, f.Session.GetVar(MythosIds.Vars.Wakefulness));
                    }
                }
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }

        [Test]
        public void ASaveFromANewerGame_IsRefused_NotCorrupted()
        {
            var root = Path.Combine(Path.GetTempPath(), "farm-compat-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var saves = new SaveService(root);
                AtomicFile.Write(saves.SlotPath(0), "{\"SaveVersion\":99}");
                Assert.IsFalse(saves.TryLoad(0, out _, out var error));
                StringAssert.Contains("newer", error);
                Assert.IsTrue(File.Exists(saves.SlotPath(0)), "the file is left alone");
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }

        // ---- soak ------------------------------------------------------------------------------------------------------------

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ABotPlaysAYear_WithTheLayerAtEachLevel(int level)
        {
            GameSession.HorrorLevelOverride = level;
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, NpcDefaults.CreateAll()))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) s.Db.Merge(pack);
                MythosModule.Install(s, s.Hooks, f.Bus);
                for (var d = 0; d < 4 * 28; d++)
                {
                    var now = s.Clock.Now;
                    if (RitualDirector.IsRitualDay(now))
                    {
                        s.Clock.SetTime(new GameDateTime(now.Year, now.Season, now.Day, RitualDirector.StartMinute - 10));
                        for (var m = 0; m < 20 && !s.Clock.Now.IsDayOver; m++) s.Clock.AdvanceMinutes(10);
                    }
                    s.EndDay(false);
                    Assert.GreaterOrEqual(s.State.Gold, 0);
                    Assert.IsNotNull(s.State.Weather);
                }
                Assert.AreEqual(2, s.Clock.Now.Year);
                var json = JsonConvert.SerializeObject(s.State);
                Assert.IsNotNull(JsonConvert.DeserializeObject<GameState>(json));
                if (level == 0) Assert.AreEqual(0, s.GetVar(MythosIds.Vars.Wakefulness));
                else Assert.Greater(s.GetVar(MythosIds.Vars.Wakefulness), 0);
            }
        }
    }
}
