using System;
using System.IO;
using System.Linq;
using System.Text;
using Farm.Core;
using Farm.Data;
using Farm.Editor;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    public class DebugCommandTests
    {
        GameObject _go;
        GameSession _session;
        DebugCommandProcessor _cmd;
        GameDatabase _db;
        ItemDefinition _parsnipSeed;

        [SetUp]
        public void SetUp()
        {
            _parsnipSeed = ItemDefinition.Create("seed.parsnip", ItemCategory.Seed, buyPrice: 20);
            _db = GameDatabase.Create(new[] { _parsnipSeed }, new CropDefinition[0]);
            _go = new GameObject("session");
            _session = _go.AddComponent<GameSession>();
            _session.Init(new EventBus(), _db, new SaveService(Path.GetTempPath()));
            _session.BeginDevGame();
            _cmd = new DebugCommandProcessor(_session, name => name == "Farm" || name == "FarmHouse");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_go);
            UnityEngine.Object.DestroyImmediate(_db);
            UnityEngine.Object.DestroyImmediate(_parsnipSeed);
        }

        DebugCommandResult Run(string line) => _cmd.Execute(line);

        [Test]
        public void HelpListsEveryCommand_AndUnknownCommandsAreRejected()
        {
            var help = Run("help");
            Assert.IsTrue(help.Ok);
            foreach (var name in _cmd.CommandNames) StringAssert.Contains(name, help.Message);
            var bad = Run("frobnicate");
            Assert.IsFalse(bad.Ok);
            StringAssert.Contains("Unknown command", bad.Message);
            Assert.IsTrue(Run("").Ok, "an empty line does nothing");
        }

        [Test]
        public void Time_SetsTheClock_AndUnderstandsTheSmallHours()
        {
            Assert.IsTrue(Run("time 22:30").Ok);
            Assert.AreEqual(22 * 60 + 30, _session.Clock.Now.MinuteOfDay);
            Assert.IsTrue(Run("time 01:30").Ok);
            Assert.AreEqual(25 * 60 + 30, _session.Clock.Now.MinuteOfDay, "01:30 is after midnight");
            Assert.IsTrue(Run("time 3:00").Ok);
            Assert.AreEqual(GameDateTime.DayEndMinute, _session.Clock.Now.MinuteOfDay, "clamped to the end of the day");
            Assert.IsTrue(Run("time 8:15").Ok);
            Assert.AreEqual(8 * 60 + 15, _session.Clock.Now.MinuteOfDay);
            Assert.IsFalse(Run("time banana").Ok);
            Assert.IsFalse(Run("time 25:99").Ok);
        }

        [Test]
        public void Skip_AdvancesTheClock()
        {
            Run("time 8:00");
            Assert.IsTrue(Run("skip 90").Ok);
            Assert.AreEqual(9 * 60 + 30, _session.Clock.Now.MinuteOfDay);
            Assert.IsFalse(Run("skip 0").Ok);
            Assert.IsFalse(Run("skip x").Ok);
        }

        [Test]
        public void Day_RunsTheOvernightLogic_AndRequestsAReload()
        {
            var before = _session.Clock.Now.Day;
            var result = Run("day");
            Assert.IsTrue(result.Ok);
            Assert.IsTrue(result.ReloadScene);
            Assert.AreEqual(before + 1, _session.Clock.Now.Day);

            Run("day 3");
            Assert.AreEqual(before + 4, _session.Clock.Now.Day);
            Assert.AreEqual(MapIds.FarmHouse, _session.State.CurrentMap, "you wake up in bed after sleeping");
            Assert.IsFalse(Run("day 0").Ok);
            Assert.IsFalse(Run("day 99999").Ok);
        }

        [Test]
        public void Date_JumpsForwardToASeasonAndDay_RollingCropsAndWeatherEachNight()
        {
            Assert.IsTrue(Run("date summer").Ok);
            Assert.AreEqual(Season.Summer, _session.Clock.Now.Season);
            Assert.AreEqual(1, _session.Clock.Now.Day);

            Assert.IsTrue(Run("date summer 15").Ok);
            Assert.AreEqual(15, _session.Clock.Now.Day);
            Assert.AreEqual(MoonPhase.Full, _session.Clock.Now.MoonPhase, "day 15 is the first full-moon day");

            Assert.IsTrue(Run("date spring 2").Ok, "wraps through winter into next year");
            Assert.AreEqual(2, _session.Clock.Now.Year);
            Assert.IsFalse(Run("date monsoon").Ok);
            Assert.IsFalse(Run("date fall 40").Ok);
        }

        [Test]
        public void Date_ToTheCurrentDate_DoesNothing()
        {
            var result = Run("date spring 1");
            Assert.IsTrue(result.Ok);
            Assert.IsFalse(result.ReloadScene);
        }

        [Test]
        public void Weather_SetsTodaysWeather_AndRainWatersTheSoil()
        {
            var grid = _session.GetGrid(MapIds.Farm);
            grid.Till(4, 4);
            Assert.IsTrue(Run("weather rain").Ok);
            Assert.AreEqual("rain", _session.State.Weather);
            grid.TryGetTile(4, 4, out var tile);
            Assert.IsTrue(tile.Watered);
            Assert.IsFalse(Run("weather").Ok);
        }

        [Test]
        public void Flags_AndVariables_CanBeSetClearedAndAdjusted()
        {
            Assert.IsTrue(Run("flag mythos.cult_known").Ok);
            Assert.IsTrue(_session.HasFlag("mythos.cult_known"));
            Assert.IsTrue(Run("flag mythos.cult_known off").Ok);
            Assert.IsFalse(_session.HasFlag("mythos.cult_known"));
            Assert.IsFalse(Run("flag x maybe").Ok);

            Assert.IsTrue(Run("var dread 40").Ok);
            Assert.AreEqual(40, _session.GetVar("dread"));
            Assert.IsTrue(Run("var dread +15").Ok);
            Assert.AreEqual(55, _session.GetVar("dread"));
            Assert.IsTrue(Run("var dread -60").Ok);
            Assert.AreEqual(-5, _session.GetVar("dread"));
            Assert.IsFalse(Run("var dread lots").Ok);
            Assert.IsFalse(Run("var dread").Ok);
        }

        [Test]
        public void ConditionsSeeWhatTheDebugToolsSet()
        {
            Run("flag mythos.woods_open");
            Run("var dread 30");
            Run("date summer 16");
            Run("time 22:30");
            Assert.IsTrue(Conditions.Evaluate("flag:mythos.woods_open && var:dread>=30 && moon:full && hour>=22", _session.World));
        }

        [Test]
        public void Gold_AndEnergy_SetOrAdjust()
        {
            Assert.IsTrue(Run("gold 2500").Ok);
            Assert.AreEqual(2500, _session.State.Gold);
            Run("gold +500");
            Assert.AreEqual(3000, _session.State.Gold);
            Run("gold -3500");
            Assert.AreEqual(3000, _session.State.Gold, "refused: would go below zero");
            Assert.IsFalse(Run("gold -3500").Ok);

            Run("energy 10");
            Assert.AreEqual(10, _session.State.Energy);
            Run("energy full");
            Assert.AreEqual(_session.State.MaxEnergy, _session.State.Energy);
            Run("energy 99999");
            Assert.AreEqual(_session.State.MaxEnergy, _session.State.Energy, "clamped to the maximum");
        }

        [Test]
        public void Give_AddsKnownItems_AndReportsUnknownOnes()
        {
            var before = _session.Backpack.Count("seed.parsnip");
            Assert.IsTrue(Run("give seed.parsnip 5").Ok);
            Assert.AreEqual(before + 5, _session.Backpack.Count("seed.parsnip"));
            Assert.IsTrue(Run("give seed.parsnip").Ok);
            Assert.AreEqual(before + 6, _session.Backpack.Count("seed.parsnip"));
            var unknown = Run("give seed.nothing");
            Assert.IsFalse(unknown.Ok);
            StringAssert.Contains("Unknown item", unknown.Message);
            Assert.IsFalse(Run("give seed.parsnip 0").Ok);
        }

        [Test]
        public void Teleport_SetsTheMapAndSpawn_ForKnownScenesOnly()
        {
            var ok = Run("tp FarmHouse bed");
            Assert.IsTrue(ok.Ok);
            Assert.IsTrue(ok.ReloadScene);
            Assert.AreEqual("FarmHouse", _session.State.CurrentMap);
            Assert.AreEqual("bed", _session.State.SpawnPoint);

            Run("tp Farm");
            Assert.AreEqual("default", _session.State.SpawnPoint);
            var bad = Run("tp Nowhere");
            Assert.IsFalse(bad.Ok);
            Assert.AreEqual("Farm", _session.State.CurrentMap, "unchanged after a bad teleport");
        }

        [Test]
        public void State_PrintsASummary()
        {
            var result = Run("state");
            Assert.IsTrue(result.Ok);
            StringAssert.Contains("weather", result.Message);
            StringAssert.Contains("gold", result.Message);
        }

        [Test]
        public void CommandsNeedARunningGame_ExceptHelp()
        {
            _session.EndGame();
            Assert.IsTrue(Run("help").Ok);
            var r = Run("gold 5");
            Assert.IsFalse(r.Ok);
            StringAssert.Contains("No game", r.Message);
        }

        [Test]
        public void ParseClock_Boundaries()
        {
            Assert.IsTrue(DebugCommandProcessor.TryParseClock("6:00", out var m));
            Assert.AreEqual(360, m);
            Assert.IsTrue(DebugCommandProcessor.TryParseClock("00:00", out m));
            Assert.AreEqual(24 * 60, m);
            Assert.IsFalse(DebugCommandProcessor.TryParseClock("12", out _));
            Assert.IsFalse(DebugCommandProcessor.TryParseClock("12:60", out _));
        }
    }

    public class ReleaseGuardTests
    {
        string _dir;

        [SetUp] public void SetUp() { _dir = Path.Combine(Path.GetTempPath(), "guard-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_dir); }
        [TearDown] public void TearDown() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

        [Test]
        public void FindsMarkersInManagedAndWideEncodings()
        {
            File.WriteAllBytes(Path.Combine(_dir, "Farm.UI.dll"), Encoding.UTF8.GetBytes("xx DebugConsoleScreen yy"));
            File.WriteAllBytes(Path.Combine(_dir, "global-metadata.dat"), Encoding.Unicode.GetBytes("..DebugCommandProcessor.."));
            var found = ReleaseGuard.Scan(_dir);
            Assert.AreEqual(2, found.Count);
            Assert.IsTrue(found.Any(f => f.Contains("Farm.UI.dll") && f.Contains("DebugConsoleScreen")));
            Assert.IsTrue(found.Any(f => f.Contains("global-metadata.dat") && f.Contains("DebugCommandProcessor")));
        }

        [Test]
        public void ACleanOutputPasses_AndOtherFileTypesAreIgnored()
        {
            File.WriteAllBytes(Path.Combine(_dir, "Farm.Gameplay.dll"), Encoding.UTF8.GetBytes("nothing to see"));
            File.WriteAllText(Path.Combine(_dir, "notes.txt"), "DebugConsoleScreen mentioned in a text file");
            Assert.IsEmpty(ReleaseGuard.Scan(_dir));
        }

        [Test]
        public void SearchesSubdirectories()
        {
            var sub = Directory.CreateDirectory(Path.Combine(_dir, "Farm_Data", "Managed")).FullName;
            File.WriteAllBytes(Path.Combine(sub, "Farm.Gameplay.dll"), Encoding.UTF8.GetBytes("DebugCommandProcessor"));
            Assert.AreEqual(1, ReleaseGuard.Scan(_dir).Count);
        }

        [Test]
        public void TheMarkersAreTheRealTypeNames()
        {
            // If a debug type is renamed, the guard must be updated with it.
            CollectionAssert.AreEquivalent(
                new[] { nameof(DebugCommandProcessor), "DebugConsoleScreen" },
                ReleaseGuard.DebugMarkers);
        }
    }
}
