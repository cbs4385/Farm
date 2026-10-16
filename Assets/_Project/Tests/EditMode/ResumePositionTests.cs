using System;
using System.IO;
using Farm.Data;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-15: a loaded game started at the map's door, not where the player had been saved. The save now carries the player's spot.
    public class ResumePositionTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-resume-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static GameState NewState() => GameState.NewGame("Sam", "Meadow", id => 999);

        [Test]
        public void TheSpot_SurvivesASaveAndALoad()
        {
            var svc = new SaveService(_root);
            var state = NewState();
            ResumePosition.Record(state, MapIds.Farm, 12.5f, -3.25f);
            svc.Save(0, state);

            Assert.IsTrue(svc.TryLoad(0, out var loaded, out var error), error);
            Assert.AreEqual(MapIds.Farm, loaded.ResumeMap);
            Assert.AreEqual(12.5f, loaded.ResumeX, 0.0001f);
            Assert.AreEqual(-3.25f, loaded.ResumeY, 0.0001f);
        }

        [Test]
        public void ASaveMadeBeforeThis_HasNoSpot_AndLoadsUnchanged()
        {
            var svc = new SaveService(_root);
            svc.Save(0, NewState());
            var path = Path.Combine(_root, "saves", "slot0", "save.json");
            var json = JObject.Parse(File.ReadAllText(path));
            json.Remove("ResumeMap"); json.Remove("ResumeX"); json.Remove("ResumeY");           // as an older version wrote it
            File.WriteAllText(path, json.ToString());

            Assert.IsTrue(svc.TryLoad(0, out var loaded, out var error), error);
            Assert.IsNull(loaded.ResumeMap);
            Assert.IsFalse(ResumePosition.TryTake(loaded, MapIds.Farm, out _, out _));
        }

        [Test]
        public void TheSpotIsTakenOnce_AndOnlyByItsOwnMap()
        {
            var state = NewState();
            ResumePosition.Record(state, MapIds.Farm, 4f, 5f);
            Assert.IsFalse(ResumePosition.TryTake(state, MapIds.Beach, out _, out _), "another map does not take it");
            Assert.IsNull(state.ResumeMap, "and it is gone, so a later visit to the farm starts at the door");
            Assert.IsFalse(ResumePosition.TryTake(state, MapIds.Farm, out _, out _));

            ResumePosition.Record(state, MapIds.Farm, 4f, 5f);
            Assert.IsTrue(ResumePosition.TryTake(state, MapIds.Farm, out var x, out var y));
            Assert.AreEqual(4f, x);
            Assert.AreEqual(5f, y);
            Assert.IsFalse(ResumePosition.TryTake(state, MapIds.Farm, out _, out _), "taken once");
        }

        [Test]
        public void ANonsenseSpot_IsNotRecorded()
        {
            var state = NewState();
            ResumePosition.Record(state, MapIds.Farm, float.NaN, 1f);
            Assert.IsNull(state.ResumeMap);
            ResumePosition.Record(state, "", 1f, 1f);
            Assert.IsNull(state.ResumeMap);
        }
    }
}
