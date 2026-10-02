using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    public class DayCycleHookTests
    {
        ItemDefinition _parsnip;
        CropDefinition _crop;
        GameState _state;
        GameClock _clock;
        Dictionary<string, FarmGrid> _grids;
        GameHooks _hooks;
        readonly List<string> _log = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _parsnip = ItemDefinition.Create("crop.parsnip", ItemCategory.Crop, sellPrice: 35);
            _crop = CropDefinition.Create("parsnip", new[] { 1, 1 }, SeasonMask.Spring);
            _state = GameState.NewGame("Sam", "Farm", _ => 999, 0);
            _clock = new GameClock(new GameDateTime(1, Season.Spring, 10, 1000));
            _state.SetDate(_clock.Now);
            _grids = new Dictionary<string, FarmGrid> { { MapIds.Farm, new FarmGrid() } };
            _hooks = new GameHooks();
            _log.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_parsnip);
            UnityEngine.Object.DestroyImmediate(_crop);
        }

        DaySummary EndDay(bool passedOut = false) => DayCycle.EndDay(_state, _clock, _grids,
            id => id == "crop.parsnip" ? _parsnip : null, id => id == "parsnip" ? _crop : null, passedOut, _hooks);

        sealed class LogHook : DayCycleHook
        {
            readonly List<string> _log; readonly string _name; readonly int _order;
            public LogHook(List<string> log, string name, int order) { _log = log; _name = name; _order = order; }
            public override int Order => _order;
            public override void OnNightFalls(DayCycleContext c) => _log.Add(_name + ":night");
            public override void OnDawn(DayCycleContext c) => _log.Add(_name + ":dawn");
        }

        sealed class ActionHook : DayCycleHook
        {
            public Action<DayCycleContext> Night, Dawn;
            public override void OnNightFalls(DayCycleContext c) => Night?.Invoke(c);
            public override void OnDawn(DayCycleContext c) => Dawn?.Invoke(c);
        }

        [Test]
        public void Hooks_RunInOrder_NightBeforeDawn()
        {
            _hooks.AddDayCycleHook(new LogHook(_log, "late", 10));
            _hooks.AddDayCycleHook(new LogHook(_log, "early", -5));
            EndDay();
            CollectionAssert.AreEqual(new[] { "early:night", "late:night", "early:dawn", "late:dawn" }, _log);
        }

        [Test]
        public void NightHook_RunsAfterShipping_AndBeforeCropsGrow()
        {
            var grid = _grids[MapIds.Farm];
            grid.Till(0, 0);
            grid.Plant(0, 0, _crop, Season.Spring);
            grid.Water(0, 0);
            _state.ShippingBin.Add(new ItemStack("crop.parsnip", 2));
            var seen = new List<string>();
            _hooks.AddDayCycleHook(new ActionHook
            {
                Night = c =>
                {
                    grid.TryGetTile(0, 0, out var t);
                    seen.Add($"gold={c.State.Gold} stage={t.Crop.Stage} bin={c.State.ShippingBin.Count}");
                    c.Note("test.note", 7);
                },
            });
            var before = _state.Gold;

            var summary = EndDay();

            CollectionAssert.AreEqual(new[] { $"gold={before + 70} stage=0 bin=0" }, seen);
            grid.TryGetTile(0, 0, out var tile);
            Assert.AreEqual(1, tile.Crop.Stage, "crop still grows after the hook");
            Assert.AreEqual(1, summary.Notes.Count);
            Assert.AreEqual("test.note", summary.Notes[0].Key);
            Assert.AreEqual(7, summary.Notes[0].Args[0]);
        }

        [Test]
        public void NightHook_CanKillCrops_AndChangeGold()
        {
            var grid = _grids[MapIds.Farm];
            grid.Till(0, 0);
            grid.Plant(0, 0, _crop, Season.Spring);
            _hooks.AddDayCycleHook(new ActionHook
            {
                Night = c => { grid.ClearCrop(0, 0); c.State.Gold -= 100; },
            });
            var before = _state.Gold;
            EndDay();
            grid.TryGetTile(0, 0, out var tile);
            Assert.IsNull(tile.Crop);
            Assert.AreEqual(before - 100, _state.Gold);
        }

        [Test]
        public void DawnHook_CanMoveWhereThePlayerWakes()
        {
            _hooks.AddDayCycleHook(new ActionHook { Dawn = c => { c.WakeMap = "Woods"; c.WakeSpawn = "clearing"; } });
            EndDay();
            Assert.AreEqual("Woods", _state.CurrentMap);
            Assert.AreEqual("clearing", _state.SpawnPoint);
        }

        [Test]
        public void WithoutHooks_PlayerWakesInBed()
        {
            EndDay();
            Assert.AreEqual(MapIds.FarmHouse, _state.CurrentMap);
            Assert.AreEqual(DayCycle.BedSpawn, _state.SpawnPoint);
        }

        [Test]
        public void ConditionsInHooks_SeeTheNewDay_AtDawn()
        {
            _clock.SetTime(new GameDateTime(1, Season.Spring, 14, 1000));   // tomorrow is day 15: full moon
            _state.SetDate(_clock.Now);
            var moons = new List<MoonPhase>();
            _hooks.AddDayCycleHook(new ActionHook
            {
                Night = c => moons.Add(c.World.Now.MoonPhase),
                Dawn = c => moons.Add(c.World.Now.MoonPhase),
            });
            EndDay();
            CollectionAssert.AreEqual(new[] { MoonPhase.WaxingGibbous, MoonPhase.Full }, moons);
        }

        sealed class FogOnFullMoon : IWeatherModifier
        {
            public int Order => 0;
            public string Modify(GameDateTime date, string weather, GameState state) =>
                date.MoonPhase == MoonPhase.Full ? "fog" : weather;
        }

        sealed class Upper : IWeatherModifier
        {
            public int Order => 5;
            public string Modify(GameDateTime date, string weather, GameState state) => weather.ToUpperInvariant();
        }

        [Test]
        public void WeatherModifiers_ChainInOrder_AndOverrideTheRoll()
        {
            _hooks.AddWeatherModifier(new Upper());
            _hooks.AddWeatherModifier(new FogOnFullMoon());
            _clock.SetTime(new GameDateTime(1, Season.Spring, 14, 1000));
            _state.SetDate(_clock.Now);
            var summary = EndDay();
            Assert.AreEqual("FOG", summary.NewWeather);
            Assert.AreEqual("FOG", _state.Weather);

            var next = EndDay();   // day 16 is still a full moon
            Assert.AreEqual("FOG", next.NewWeather);
        }

        [Test]
        public void ThrowingHook_IsLogged_AndDoesNotBreakTheDay()
        {
            _hooks.AddDayCycleHook(new ActionHook { Night = _ => throw new InvalidOperationException("boom") });
            _hooks.AddDayCycleHook(new ActionHook { Night = c => c.Note("after.boom") });
            LogAssert.Expect(LogType.Error, new Regex("OnNightFalls failed"));

            var summary = EndDay();

            Assert.AreEqual(1, summary.Notes.Count, "later hooks still run");
            Assert.AreEqual(11, _clock.Now.Day, "the day still advanced");
        }

        sealed class Boom : IWeatherModifier
        {
            public int Order => 0;
            public string Modify(GameDateTime d, string w, GameState s) => throw new InvalidOperationException("x");
        }

        [Test]
        public void ThrowingWeatherModifier_FallsBackToTheRolledWeather()
        {
            _hooks.AddWeatherModifier(new Boom());
            LogAssert.Expect(LogType.Error, new Regex("Weather modifier"));
            var summary = EndDay();
            Assert.AreEqual(DayCycle.RollWeather(_clock.Now), summary.NewWeather);
        }
    }

    public class GameHooksTests
    {
        sealed class TestHook : DayCycleHook { }

        [Test]
        public void MapLoaded_RaisesToAllHandlers_AndIsolatesFailures()
        {
            var hooks = new GameHooks();
            var seen = new List<string>();
            hooks.MapLoaded += c => seen.Add("a:" + c.MapId);
            hooks.MapLoaded += _ => throw new InvalidOperationException("bad handler");
            hooks.MapLoaded += c => seen.Add("c:" + c.MapId);
            LogAssert.Expect(LogType.Error, new Regex("MapLoaded handler failed"));

            hooks.RaiseMapLoaded(new MapLoadedContext { MapId = "Farm" });

            CollectionAssert.AreEqual(new[] { "a:Farm", "c:Farm" }, seen);
        }

        [Test]
        public void SameHookRegisteredTwice_RunsOnce()
        {
            var hooks = new GameHooks();
            var hook = new TestHook();
            hooks.AddDayCycleHook(hook);
            hooks.AddDayCycleHook(hook);
            Assert.AreEqual(1, hooks.DayCycleHooks.Count);
        }
    }

    public class AtmosphereStackTests
    {
        [Test]
        public void EmptyStack_LeavesColourUnchanged()
        {
            var s = new AtmosphereStack();
            Assert.AreEqual(Color.white, s.Apply(Color.white));
        }

        [Test]
        public void Layer_TintsTowardsItsColour_ByStrength()
        {
            var s = new AtmosphereStack();
            s.Set("fog", new Color(0.5f, 0.5f, 0.5f), 1f);
            Assert.AreEqual(0.5f, s.Apply(Color.white).r, 0.001f);
            s.Set("fog", new Color(0.5f, 0.5f, 0.5f), 0.5f);
            Assert.AreEqual(0.75f, s.Apply(Color.white).r, 0.001f);
            s.Set("fog", new Color(0.5f, 0.5f, 0.5f), 0f);
            Assert.AreEqual(1f, s.Apply(Color.white).r, 0.001f);
        }

        [Test]
        public void Layers_StackByPriority_AndCanBeRemoved()
        {
            var s = new AtmosphereStack();
            s.Set("b", new Color(0.5f, 1f, 1f), 1f, priority: 2);
            s.Set("a", new Color(0.5f, 1f, 1f), 1f, priority: 1);
            Assert.AreEqual(0.25f, s.Apply(Color.white).r, 0.001f);
            Assert.IsTrue(s.Remove("a"));
            Assert.AreEqual(0.5f, s.Apply(Color.white).r, 0.001f);
            s.Clear();
            Assert.AreEqual(0, s.Count);
        }
    }

    public class LocalizationHookTests
    {
        [TearDown]
        public void TearDown()
        {
            L.SetLanguage("en");
        }

        [Test]
        public void Filters_PostProcessEveryLookup_InOrder()
        {
            L.SetTable(new Dictionary<string, string> { { "a", "hello" } });
            Func<string, string, string> shout = (key, text) => text.ToUpperInvariant();
            Func<string, string, string> bang = (key, text) => text + "!";
            L.AddFilter(shout);
            L.AddFilter(bang);
            try { Assert.AreEqual("HELLO!", L.Get("a")); }
            finally { L.RemoveFilter(shout); L.RemoveFilter(bang); }
            Assert.AreEqual("hello", L.Get("a"));
        }

        [Test]
        public void Filters_ReceiveTheKey_SoTheyCanTargetCategories()
        {
            L.SetTable(new Dictionary<string, string> { { "npc.mara.greeting", "Good day" }, { "ui.close", "Close" } });
            Func<string, string, string> corrupt = (key, text) => key.StartsWith("npc.") ? "~" + text : text;
            L.AddFilter(corrupt);
            try
            {
                Assert.AreEqual("~Good day", L.Get("npc.mara.greeting"));
                Assert.AreEqual("Close", L.Get("ui.close"));
            }
            finally { L.RemoveFilter(corrupt); }
        }

        [Test]
        public void ExtraTables_SurviveLanguageReloads()
        {
            L.AddTable("en", new Dictionary<string, string> { { "weather.testfog", "Test Fog" } });
            L.SetLanguage("en");
            Assert.AreEqual("Test Fog", L.Get("weather.testfog"));
            L.SetLanguage("en");
            Assert.AreEqual("Test Fog", L.Get("weather.testfog"));
        }
    }

    public class ModuleTests
    {
        [SetUp] public void SetUp() => GameModules.ClearForTests();
        [TearDown] public void TearDown() => GameModules.ClearForTests();

        sealed class Counting : IGameModule
        {
            public int Initialized;
            public string Id { get; }
            public Counting(string id) { Id = id; }
            public void Initialize(ModuleContext c) => Initialized++;
        }

        sealed class Exploding : IGameModule
        {
            public string Id => "boom";
            public void Initialize(ModuleContext c) => throw new InvalidOperationException("no");
        }

        [Test]
        public void Register_DedupesById_AndInitializeAllRunsEach()
        {
            var a = new Counting("a");
            GameModules.Register(a);
            GameModules.Register(new Counting("a"));
            GameModules.Register(new Counting("b"));
            Assert.AreEqual(2, GameModules.All.Count);
            GameModules.InitializeAll(new ModuleContext());
            Assert.AreEqual(1, a.Initialized);
        }

        [Test]
        public void FailingModule_DoesNotStopOthers()
        {
            var ok = new Counting("ok");
            GameModules.Register(new Exploding());
            GameModules.Register(ok);
            LogAssert.Expect(LogType.Error, new Regex("failed to initialize"));
            GameModules.InitializeAll(new ModuleContext());
            Assert.AreEqual(1, ok.Initialized);
        }

        [Test]
        public void HorrorLevel_ComesFromSettings_AndDefaultsToFull()
        {
            Assert.AreEqual(2, new ModuleContext().HorrorLevel);
            Assert.AreEqual(0, new ModuleContext { Settings = new SettingsData { HorrorLevel = 0 } }.HorrorLevel);
        }

        [Test]
        public void HorrorLevel_IsClamped()
        {
            var s = new SettingsData { HorrorLevel = 9 };
            s.Clamp();
            Assert.AreEqual(2, s.HorrorLevel);
            s.HorrorLevel = -3;
            s.Clamp();
            Assert.AreEqual(0, s.HorrorLevel);
        }

        [Test]
        public void MythosModule_IsInertButProvidesWeatherNames()
        {
            var hooks = new GameHooks();
            new MythosModule().Initialize(new ModuleContext { Hooks = hooks, Settings = new SettingsData() });
            Assert.AreEqual(0, hooks.DayCycleHooks.Count, "the base game must be unchanged until content is added");
            Assert.AreEqual(0, hooks.HudWidgets.Count);
            L.SetLanguage("en");
            Assert.AreEqual("Fog", L.Get("weather." + MythosIds.Weather.Fog));
            Assert.AreEqual("Blood Moon", L.Get("weather." + MythosIds.Weather.BloodMoon));
        }
    }

    public class ContentPackTests
    {
        [Test]
        public void Merge_AddsItemsAndCrops_AndRejectsIdClashes()
        {
            var core = ItemDefinition.Create("seed.parsnip", ItemCategory.Seed);
            var db = GameDatabase.Create(new[] { core }, new CropDefinition[0]);

            var newItem = ItemDefinition.Create("misc.strangestone", ItemCategory.Misc, sellPrice: 5);
            var clash = ItemDefinition.Create("seed.parsnip", ItemCategory.Seed);
            var crop = CropDefinition.Create("nightbloom", new[] { 2, 2 }, SeasonMask.Fall);
            var pack = ContentPack.Create("test", new[] { newItem, clash }, new[] { crop });

            LogAssert.Expect(LogType.Error, new Regex("redefines item"));
            var rejected = db.Merge(pack);

            Assert.AreEqual(1, rejected);
            Assert.IsTrue(db.TryGetItem("misc.strangestone", out var found));
            Assert.AreSame(newItem, found);
            Assert.AreSame(core, db.GetItem("seed.parsnip"), "core items are never replaced");
            Assert.IsTrue(db.TryGetCrop("nightbloom", out _));
            CollectionAssert.Contains(new List<ItemDefinition>(db.AllItems), newItem);
            Assert.AreEqual(1, db.Items.Count, "the core asset itself is untouched");
        }
    }
}
