using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Flags, variables and module-owned data: the story-state hooks that optional layers build on.
    public class SessionStateTests
    {
        GameObject _go;
        GameSession _session;
        EventBus _bus;
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-session-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _bus = new EventBus();
            _go = new GameObject("session");
            _session = _go.AddComponent<GameSession>();
            _session.Init(_bus, GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]), new SaveService(_root));
            _session.BeginDevGame();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_go);
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void Flags_SetAndClear_PublishOnlyOnChange()
        {
            var events = new List<FlagChanged>();
            _bus.Subscribe<FlagChanged>(events.Add);

            _session.SetFlag("a");
            _session.SetFlag("a");          // no change
            Assert.IsTrue(_session.HasFlag("a"));
            _session.SetFlag("a", false);
            _session.SetFlag("a", false);   // no change

            Assert.AreEqual(2, events.Count);
            Assert.IsTrue(events[0].Value);
            Assert.IsFalse(events[1].Value);
            Assert.IsFalse(_session.HasFlag("a"));
        }

        [Test]
        public void Vars_DefaultToZero_AndPublishOldAndNew()
        {
            var events = new List<VarChanged>();
            _bus.Subscribe<VarChanged>(events.Add);

            Assert.AreEqual(0, _session.GetVar("dread"));
            _session.SetVar("dread", 3);
            _session.SetVar("dread", 3);   // unchanged: no event
            _session.SetVar("dread", 5);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual((0, 3), (events[0].OldValue, events[0].NewValue));
            Assert.AreEqual((3, 5), (events[1].OldValue, events[1].NewValue));
        }

        [Test]
        public void AddVar_ClampsToRange()
        {
            Assert.AreEqual(10, _session.AddVar("dread", 10, 0, 100));
            Assert.AreEqual(100, _session.AddVar("dread", 500, 0, 100));
            Assert.AreEqual(0, _session.AddVar("dread", -1000, 0, 100));
        }

        [Test]
        public void World_ReflectsLiveState_ForConditions()
        {
            _session.SetFlag("mythos.cult_known");
            _session.SetVar("dread", 4);
            _session.State.Weather = "fog";
            Assert.IsTrue(Conditions.Evaluate("flag:mythos.cult_known && var:dread>=4 && weather:fog && map:Farm", _session.World));
            _session.SetVar("dread", 1);
            Assert.IsFalse(Conditions.Evaluate("var:dread>=4", _session.World));
        }

        sealed class SampleData
        {
            public int Chapter;
            public List<string> Seen = new List<string>();
        }

        [Test]
        public void ModuleData_RoundTrips_AndMissingDataGivesAFreshObject()
        {
            Assert.AreEqual(0, _session.GetModuleData<SampleData>("mod").Chapter);

            _session.SetModuleData("mod", new SampleData { Chapter = 3, Seen = { "a", "b" } });
            var again = _session.GetModuleData<SampleData>("mod");

            Assert.AreEqual(3, again.Chapter);
            CollectionAssert.AreEqual(new[] { "a", "b" }, again.Seen);
        }

        [Test]
        public void CorruptModuleData_IsLogged_AndReplacedByAFreshObject()
        {
            _session.State.ModuleData["mod"] = "{ not json";
            LogAssert.Expect(LogType.Error, new Regex("unreadable"));
            Assert.AreEqual(0, _session.GetModuleData<SampleData>("mod").Chapter);
        }

        [Test]
        public void FlagsVarsAndModuleData_SurviveSaveAndLoad()
        {
            _session.SetFlag("mythos.initiated");
            _session.SetVar("cult.standing", -2);
            _session.SetModuleData("mod", new SampleData { Chapter = 7 });
            var saves = new SaveService(_root);
            _session.SyncToState();
            saves.Save(0, _session.State);

            Assert.IsTrue(saves.TryLoad(0, out var loaded, out var error), error);

            Assert.IsTrue(loaded.Flags.Contains("mythos.initiated"));
            Assert.AreEqual(-2, loaded.Vars["cult.standing"]);
            Assert.AreEqual(7, JObject.Parse(loaded.ModuleData["mod"]).Value<int>("Chapter"));
        }

        [Test]
        public void SavesFromBeforeTheHooksExisted_StillLoad_WithEmptyCollections()
        {
            var saves = new SaveService(_root);
            _session.SyncToState();
            saves.Save(1, _session.State);
            var path = saves.SlotPath(1);
            var json = JObject.Parse(File.ReadAllText(path));
            json.Remove("Vars");
            json.Remove("ModuleData");
            File.WriteAllText(path, json.ToString());

            Assert.IsTrue(saves.TryLoad(1, out var loaded, out var error), error);
            Assert.IsNotNull(loaded.Vars);
            Assert.IsNotNull(loaded.ModuleData);
            Assert.AreEqual(0, loaded.Vars.Count);
        }
    }
}
