using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-101: the staging checker, on a small hand-made map.
    public class EventStagingTests
    {
        // A 10x6 room with a wall across x = 5 from y = 0 to y = 4, leaving a gap at y = 5.
        static WalkGrid Room(bool gap = true) =>
            new WalkGrid(0, 0, 10, 6, (x, y) => !(x == 5 && (y < 5 || !gap)));

        static EventStep Place(string actor, int x, int y) => new EventStep { Type = "place", Actor = actor, X = x, Y = y };
        static EventStep Move(string actor, int x, int y) => new EventStep { Type = "move", Actor = actor, X = x, Y = y };
        static EventDefinition Ev(params EventStep[] steps) => new EventDefinition { Id = "t", Steps = steps.ToList() };

        [Test]
        public void ASoundScene_HasNoProblems() =>
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(Place("tilda", 2, 2), Place("player", 3, 2), Move("tilda", 8, 3)), Room()));

        [Test]
        public void StandingInAWall_IsReported()
        {
            var p = EventStaging.Check(Ev(Place("tilda", 5, 2)), Room());
            Assert.IsTrue(p.Any(m => m.Contains("not free floor")), string.Join("|", p));
        }

        [Test]
        public void OffTheMap_IsReported() =>
            Assert.IsTrue(EventStaging.Check(Ev(Place("tilda", 40, 40)), Room()).Any(m => m.Contains("not free floor")));

        [Test]
        public void WalkingToAWall_OrWithNoPath_IsReported()
        {
            Assert.IsTrue(EventStaging.Check(Ev(Move("tilda", 5, 1)), Room()).Any(m => m.Contains("not free floor")));
            var sealedOff = EventStaging.Check(Ev(Place("tilda", 1, 1), Move("tilda", 8, 1)), Room(gap: false));
            Assert.IsTrue(sealedOff.Any(m => m.Contains("no path")), string.Join("|", sealedOff));
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(Place("tilda", 1, 1), Move("tilda", 8, 1)), Room(gap: true)), "through the gap there is a path");
        }

        [Test]
        public void ThePathIsOnlyChecked_FromAKnownStart() =>
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(Move("tilda", 8, 1)), Room(gap: false)), "a villager's starting cell is not known here");

        [Test]
        public void TwoActors_OnOneCell_AreReported()
        {
            var p = EventStaging.Check(Ev(Place("tilda", 2, 2), Place("player", 2, 2)), Room());
            Assert.IsTrue(p.Any(m => m.Contains("same cell")), string.Join("|", p));
            var afterMoving = EventStaging.Check(Ev(Place("tilda", 2, 2), Move("tilda", 3, 3), Place("player", 2, 2)), Room());
            CollectionAssert.IsEmpty(afterMoving, "she moved away first");
        }

        [Test]
        public void Props_AndTheCamera_AreChecked()
        {
            Assert.IsTrue(EventStaging.Check(Ev(new EventStep { Type = "spawn", Id = "g", Name = "x", X = 5, Y = 0 }), Room()).Any(m => m.Contains("prop")));
            Assert.IsTrue(EventStaging.Check(Ev(new EventStep { Type = "camera", Name = "pan", X = 99, Y = 99 }), Room()).Any(m => m.Contains("off the map")));
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(new EventStep { Type = "camera", Name = "pan", X = 2, Y = 2 }), Room()));
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(new EventStep { Type = "camera", Name = "focus", Actor = "tilda" }), Room()), "an actor needs no cell");
        }

        [Test]
        public void ParallelChildren_AreChecked()
        {
            var group = new EventStep { Type = "parallel", Steps = { Move("tilda", 5, 0) } };
            Assert.IsTrue(EventStaging.Check(Ev(group), Room()).Any(m => m.Contains("step0.0")));
        }

        [Test]
        public void NothingToCheck_IsFine()
        {
            CollectionAssert.IsEmpty(EventStaging.Check(null, Room()));
            CollectionAssert.IsEmpty(EventStaging.Check(Ev(Place("tilda", 1, 1)), null));
        }
    }
}
