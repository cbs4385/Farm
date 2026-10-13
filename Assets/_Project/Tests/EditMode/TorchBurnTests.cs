using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Torches burn down (owner, 2026-10-12): an hour of game time, lit by themselves in the dark and out by themselves in daylight and indoors.
    public class TorchBurnTests
    {
        [Test]
        public void ATorchIsNeeded_InTheMine_AndOutdoorsFromDuskToDawn_NotInDaylightNorIndoors()
        {
            Assert.IsTrue(TorchBurn.Needed(MapIds.Mine, 12 * 60), "the mine is dark at noon");
            Assert.IsTrue(TorchBurn.Needed(MapIds.Farm, 23 * 60), "night");
            Assert.IsTrue(TorchBurn.Needed(MapIds.Village, 20 * 60), "dusk");
            Assert.IsTrue(TorchBurn.Needed(MapIds.Forest, 6 * 60), "the day starts in the dawn dark");
            Assert.IsFalse(TorchBurn.Needed(MapIds.Farm, 12 * 60), "daylight");
            Assert.IsFalse(TorchBurn.Needed(MapIds.Village, 10 * 60), "morning");
            Assert.IsFalse(TorchBurn.Needed(MapIds.FarmHouse, 23 * 60), "inside a building: it is lit inside");
            Assert.IsFalse(TorchBurn.Needed(MapIds.Carpenter, 2 * 60 + 24 * 60), "indoors at night too");
        }

        [Test]
        public void ATorchLightsItself_BurnsAnHour_AndTheNextOneTakesOver()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                s.State.CurrentMap = MapIds.Mine;
                s.Backpack.Remove(ItemIds.Torch, s.Backpack.Count(ItemIds.Torch));          // a new game starts with one
                s.Backpack.Add(ItemIds.Torch, 2);
                Assert.IsTrue(TorchBurn.Refresh(s, out var lighted, out var none));
                Assert.IsTrue(lighted);
                Assert.IsFalse(none);
                Assert.AreEqual(TorchBurn.Minutes, TorchBurn.Left(s));
                Assert.AreEqual(2, s.Backpack.Count(ItemIds.Torch), "the burning torch stays in its slot");
                Assert.IsTrue(TorchBurn.IsBurning(s));
                Assert.AreEqual(1f, TorchBurn.Fraction(s), 0.001f);

                for (var i = 0; i < TorchBurn.Minutes - 1; i++) Assert.IsFalse(TorchBurn.OnMinute(s), "still burning at minute " + i);
                Assert.AreEqual(1, TorchBurn.Left(s));
                Assert.AreEqual(2, s.Backpack.Count(ItemIds.Torch), "still there at minute 59");
                Assert.IsTrue(TorchBurn.OnMinute(s), "the hour is up");
                Assert.AreEqual(1, s.Backpack.Count(ItemIds.Torch), "and used up");
                Assert.AreEqual(0, TorchBurn.Left(s));
                Assert.IsFalse(TorchBurn.IsBurning(s));

                Assert.IsTrue(TorchBurn.Refresh(s, out lighted, out none), "the second torch takes over");
                Assert.IsTrue(lighted);
                Assert.AreEqual(1, s.Backpack.Count(ItemIds.Torch), "the second one is alight and still in its slot");
                for (var i = 0; i < TorchBurn.Minutes; i++) TorchBurn.OnMinute(s);
                Assert.AreEqual(0, s.Backpack.Count(ItemIds.Torch), "and gone when its hour is over");
                Assert.IsFalse(TorchBurn.Refresh(s, out _, out none), "nothing left to light");
                Assert.IsTrue(none);
            }
        }

        [Test]
        public void WithNoTorchLeft_ItStaysDark_AndSaysSo()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                s.State.CurrentMap = MapIds.Mine;
                s.Backpack.Remove(ItemIds.Torch, s.Backpack.Count(ItemIds.Torch));
                Assert.IsFalse(TorchBurn.Refresh(s, out var lighted, out var none));
                Assert.IsFalse(lighted);
                Assert.IsTrue(none);
            }
        }

        [Test]
        public void AnAlightTorch_GoesOutIndoors_AndIsNotUsedUpMeanwhile()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                s.State.CurrentMap = MapIds.Mine;
                s.Backpack.Remove(ItemIds.Torch, s.Backpack.Count(ItemIds.Torch));
                s.Backpack.Add(ItemIds.Torch, 1);
                TorchBurn.Refresh(s, out _, out _);
                for (var i = 0; i < 10; i++) TorchBurn.OnMinute(s);
                Assert.AreEqual(50, TorchBurn.Left(s));

                s.State.CurrentMap = MapIds.FarmHouse;
                Assert.IsFalse(TorchBurn.IsBurning(s), "out indoors");
                for (var i = 0; i < 30; i++) TorchBurn.OnMinute(s);
                Assert.AreEqual(50, TorchBurn.Left(s), "no minutes are used up indoors");
                Assert.IsFalse(TorchBurn.Refresh(s, out var lighted, out _));
                Assert.IsFalse(lighted, "and a new torch is not lit indoors");

                s.State.CurrentMap = MapIds.Mine;
                Assert.IsTrue(TorchBurn.IsBurning(s), "it burns again in the dark, with what was left");
                Assert.AreEqual(50, TorchBurn.Left(s));
            }
        }

        [Test]
        public void TheMinutesLeft_AreKeptInTheStoryVariables_SoASaveHasThem()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                s.State.CurrentMap = MapIds.Mine;
                s.Backpack.Add(ItemIds.Torch, 1);
                TorchBurn.Refresh(s, out _, out _);
                Assert.IsTrue(s.State.Vars.TryGetValue(TorchBurn.LeftVar, out var v));
                Assert.AreEqual(TorchBurn.Minutes, v);
            }
        }
    }
}
