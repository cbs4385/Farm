using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    public class ItemMarkTests
    {
        static int Max(string id) => 10;

        [Test]
        public void MarkedStacks_DoNotMergeWithUnmarkedOnes()
        {
            var inv = new Inventory(4, Max);
            inv.Add("crop.parsnip", 3);
            inv.Add("crop.parsnip", 2, 0, "sacrifice");
            Assert.AreEqual(3, inv.Get(0).Count);
            Assert.IsNull(inv.Get(0).Mark);
            Assert.AreEqual(2, inv.Get(1).Count);
            Assert.AreEqual("sacrifice", inv.Get(1).Mark);

            inv.Add("crop.parsnip", 4, 0, "sacrifice");
            Assert.AreEqual(6, inv.Get(1).Count, "identically marked stacks merge");
            Assert.AreEqual(3, inv.Get(0).Count);
        }

        [Test]
        public void MoveMergesOnlyIdenticallyMarkedStacks_AndMarksSurviveCloneAndRemove()
        {
            var inv = new Inventory(4, Max);
            inv.Add("crop.parsnip", 3);
            inv.Add("crop.parsnip", 2, 0, "m");
            inv.Move(0, 1);                       // different marks: swap, not merge
            Assert.AreEqual("m", inv.Get(0).Mark);
            Assert.IsNull(inv.Get(1).Mark);

            var removed = inv.RemoveFromSlot(0, 1);
            Assert.AreEqual("m", removed.Mark);
            Assert.AreEqual("m", removed.Clone().Mark);
            Assert.AreEqual("m", inv.Get(0).Mark);
        }

        [Test]
        public void MarksSurviveSaveDataRoundTrip_AndEmptyMarkIsNull()
        {
            var inv = new Inventory(3, Max);
            inv.Add("crop.kale", 2, 1, "tagged");
            inv.Add("crop.kale", 1, 1, "");
            var copy = Inventory.FromData(inv.ToData(), Max);
            Assert.AreEqual("tagged", copy.Get(0).Mark);
            Assert.IsNull(copy.Get(1).Mark);
            Assert.IsTrue(copy.CanAdd("crop.kale", 5, 1, "tagged"));
        }
    }

    public class WorldObjectSourceTests
    {
        sealed class FakeSource : IWorldObjectSource
        {
            public string Kind { get; }
            public readonly HashSet<string> Ids;
            public FakeSource(string kind, params string[] ids) { Kind = kind; Ids = new HashSet<string>(ids); }
            public IEnumerable<WorldObjectRef> Enumerate() => Ids.Select(i => new WorldObjectRef(Kind, i, "Farm", "x"));
            public bool Exists(WorldObjectRef r) => Ids.Contains(r.Id);
            public bool TryConsume(WorldObjectRef r) => Ids.Remove(r.Id);
        }

        sealed class Broken : IWorldObjectSource
        {
            public string Kind => WorldObjectKinds.Animal;
            public IEnumerable<WorldObjectRef> Enumerate() => throw new InvalidOperationException("x");
            public bool Exists(WorldObjectRef r) => throw new InvalidOperationException("x");
            public bool TryConsume(WorldObjectRef r) => throw new InvalidOperationException("x");
        }

        [Test]
        public void SourcesAreEnumeratedByKind_AndAllKindsWhenNoneGiven()
        {
            var hooks = new GameHooks();
            hooks.AddWorldObjectSource(new FakeSource(WorldObjectKinds.Animal, "hen1", "cow1"));
            hooks.AddWorldObjectSource(new FakeSource(WorldObjectKinds.CraftedItem, "chest1:3"));

            Assert.AreEqual(3, hooks.EnumerateWorldObjects().Count);
            Assert.AreEqual(2, hooks.EnumerateWorldObjects(WorldObjectKinds.Animal).Count);
            Assert.AreEqual(3, hooks.EnumerateWorldObjects(WorldObjectKinds.Animal, WorldObjectKinds.CraftedItem).Count);
            Assert.AreEqual(0, hooks.EnumerateWorldObjects(WorldObjectKinds.PlantProduct).Count);
        }

        [Test]
        public void ExistsAndConsume_RouteToTheOwningSource()
        {
            var hooks = new GameHooks();
            var animals = new FakeSource(WorldObjectKinds.Animal, "hen1");
            hooks.AddWorldObjectSource(animals);
            var hen = new WorldObjectRef(WorldObjectKinds.Animal, "hen1");
            var other = new WorldObjectRef(WorldObjectKinds.CraftedItem, "hen1");

            Assert.IsTrue(hooks.WorldObjectExists(hen));
            Assert.IsFalse(hooks.WorldObjectExists(other), "same id, different kind");
            Assert.IsTrue(hooks.ConsumeWorldObject(hen));
            Assert.IsFalse(hooks.WorldObjectExists(hen));
            Assert.IsFalse(hooks.ConsumeWorldObject(hen), "already gone");
        }

        [Test]
        public void ReferencesCompareByKindAndId()
        {
            var a = new WorldObjectRef("animal", "1", "Farm", "hen");
            var b = new WorldObjectRef("animal", "1", "Barn", "other");
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, new WorldObjectRef("plant", "1"));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ABrokenSource_IsLoggedAndSkipped()
        {
            var hooks = new GameHooks();
            hooks.AddWorldObjectSource(new Broken());
            hooks.AddWorldObjectSource(new FakeSource(WorldObjectKinds.CraftedItem, "jar1"));
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex("World object source"));
            Assert.AreEqual(1, hooks.EnumerateWorldObjects().Count);
        }
    }

    public class RitualTimelineTests
    {
        [Test]
        public void DurationIsThirtyMinutesPlusTwentyPerSacrifice()
        {
            Assert.AreEqual(70, RitualModel.DurationMinutes(RitualModel.OfferingCount(Season.Spring)));
            Assert.AreEqual(90, RitualModel.DurationMinutes(RitualModel.OfferingCount(Season.Summer)));
            Assert.AreEqual(110, RitualModel.DurationMinutes(RitualModel.OfferingCount(Season.Fall)));
            Assert.AreEqual(130, RitualModel.DurationMinutes(RitualModel.OfferingCount(Season.Winter)));
        }

        [Test]
        public void EachSacrificeHasItsOwnTwentyMinuteWindowAfterTheLeadersSpeech()
        {
            Assert.AreEqual(30, RitualModel.SacrificeStart(0));
            Assert.AreEqual(50, RitualModel.SacrificeEnd(0));
            Assert.AreEqual(50, RitualModel.SacrificeStart(1));
            Assert.AreEqual(130, RitualModel.SacrificeEnd(4));
        }

        [Test]
        public void Phases_FollowTheTimeline()
        {
            Assert.AreEqual(RitualModel.RitualPhase.NotStarted, RitualModel.PhaseAt(-1, 2));
            Assert.AreEqual(RitualModel.RitualPhase.LeaderSpeaking, RitualModel.PhaseAt(0, 2));
            Assert.AreEqual(RitualModel.RitualPhase.LeaderSpeaking, RitualModel.PhaseAt(29, 2));
            Assert.AreEqual(RitualModel.RitualPhase.Sacrificing, RitualModel.PhaseAt(30, 2));
            Assert.AreEqual(RitualModel.RitualPhase.Sacrificing, RitualModel.PhaseAt(69, 2));
            Assert.AreEqual(RitualModel.RitualPhase.Finished, RitualModel.PhaseAt(70, 2));
        }

        [Test]
        public void CurrentSacrifice_IsMinusOneOutsideTheSacrifices()
        {
            Assert.AreEqual(-1, RitualModel.CurrentSacrifice(10, 3));
            Assert.AreEqual(0, RitualModel.CurrentSacrifice(30, 3));
            Assert.AreEqual(0, RitualModel.CurrentSacrifice(49, 3));
            Assert.AreEqual(1, RitualModel.CurrentSacrifice(50, 3));
            Assert.AreEqual(2, RitualModel.CurrentSacrifice(89, 3));
            Assert.AreEqual(-1, RitualModel.CurrentSacrifice(90, 3));
        }

        [Test]
        public void AnOfferingCanBeTakenUntilItIsConsumed_NotAfter()
        {
            // Before the ritual, during the speech, and while it lies on the altar: yes. At the end of its 20 minutes: no.
            Assert.IsTrue(RitualModel.CanStillBeTaken(1, -600));
            Assert.IsTrue(RitualModel.CanStillBeTaken(1, 10));
            Assert.IsTrue(RitualModel.CanStillBeTaken(1, 50));       // just laid on the altar
            Assert.IsTrue(RitualModel.CanStillBeTaken(1, 69));       // one minute before consumption
            Assert.IsFalse(RitualModel.CanStillBeTaken(1, 70));      // consumed
            Assert.IsTrue(RitualModel.IsOnAltar(1, 60));
            Assert.IsFalse(RitualModel.IsOnAltar(1, 40));
            Assert.IsTrue(RitualModel.IsConsumed(0, 50));
        }

        [Test]
        public void EverySeasonsRitualCanFinishBeforeTheDayEnds()
        {
            foreach (Season s in Enum.GetValues(typeof(Season)))
            {
                var latest = RitualModel.LatestStartMinuteOfDay(s);
                Assert.Greater(latest, 20 * 60, $"{s}: leaves a reasonable evening to start in");
                Assert.AreEqual(GameDateTime.DayEndMinute, latest + RitualModel.DurationMinutes(RitualModel.OfferingCount(s)));
            }
            Assert.AreEqual(27 * 60 + 50, RitualModel.LatestStartMinuteOfDay(Season.Winter), "03:50, with the day ending at 06:00");
        }
    }
}
