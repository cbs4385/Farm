using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // Encodes the owner's ritual rules (GDD section 9, G and H).
    public class RitualModelTests
    {
        static List<RitualParticipant> Plan(int count) =>
            Enumerable.Range(1, count).Select(i => new RitualParticipant($"npc{i}", $"item.offering{i}")).ToList();

        static HashSet<string> Ids(int count) => new HashSet<string>(Enumerable.Range(1, count).Select(i => $"npc{i}"));

        [Test]
        public void OfferingsPerSeason_AreTwoThreeFourFive()
        {
            Assert.AreEqual(2, RitualModel.OfferingCount(Season.Spring));
            Assert.AreEqual(3, RitualModel.OfferingCount(Season.Summer));
            Assert.AreEqual(4, RitualModel.OfferingCount(Season.Fall));
            Assert.AreEqual(5, RitualModel.OfferingCount(Season.Winter));
        }

        [Test]
        public void APlanMustHaveTheRightNumberOfDistinctParticipants()
        {
            Assert.IsTrue(RitualModel.IsValidPlan(Season.Spring, Plan(2)));
            Assert.IsTrue(RitualModel.IsValidPlan(Season.Winter, Plan(5)));
            Assert.IsFalse(RitualModel.IsValidPlan(Season.Spring, Plan(3)));
            Assert.IsFalse(RitualModel.IsValidPlan(Season.Winter, Plan(4)));
            Assert.IsFalse(RitualModel.IsValidPlan(Season.Spring, null));

            var duplicate = new List<RitualParticipant> { new RitualParticipant("a", "x"), new RitualParticipant("a", "y") };
            Assert.IsFalse(RitualModel.IsValidPlan(Season.Spring, duplicate), "one Keeper cannot fill two places");
            var blank = new List<RitualParticipant> { new RitualParticipant("a", ""), new RitualParticipant("b", "y") };
            Assert.IsFalse(RitualModel.IsValidPlan(Season.Spring, blank));
        }

        [Test]
        public void EveryOfferingSacrificedByAnAvailableKeeper_Succeeds()
        {
            var plan = Plan(4);
            var result = RitualModel.Resolve(plan, Ids(4), Ids(4));
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(RitualFailure.None, result.Failure);
        }

        [Test]
        public void AMissingKeeper_FailsTheRitual()
        {
            var plan = Plan(3);
            var available = Ids(3);
            available.Remove("npc2");   // e.g. ill, away, or kept from the wood by the player
            var result = RitualModel.Resolve(plan, available, Ids(3));
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(RitualFailure.MissingParticipant, result.Failure);
            CollectionAssert.AreEqual(new[] { "npc2" }, result.MissingParticipants);
        }

        [Test]
        public void AnOfferingThatIsNotFullySacrificed_FailsTheRitual()
        {
            var plan = Plan(2);
            var sacrificed = Ids(2);
            sacrificed.Remove("npc1");   // e.g. taken from the altar before it dissolved
            var result = RitualModel.Resolve(plan, Ids(2), sacrificed);
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(RitualFailure.OfferingNotSacrificed, result.Failure);
            CollectionAssert.AreEqual(new[] { "npc1" }, result.UnsacrificedOfferings);
        }

        [Test]
        public void AMissingKeeper_IsReportedAsMissing_NotAsAnUnsacrificedOffering()
        {
            var result = RitualModel.Resolve(Plan(2), new HashSet<string> { "npc1" }, new HashSet<string> { "npc1" });
            Assert.AreEqual(RitualFailure.MissingParticipant, result.Failure);
            Assert.IsEmpty(result.UnsacrificedOfferings);
        }

        [Test]
        public void SuccessLowersWakefulness_FailureLeavesItAlone()
        {
            var plan = Plan(2);
            var ok = RitualModel.Resolve(plan, Ids(2), Ids(2));
            var bad = RitualModel.Resolve(plan, Ids(1), Ids(2));
            Assert.AreEqual(700, RitualModel.WakefulnessAfter(1000, ok, 0.0));
            Assert.AreEqual(1000, RitualModel.WakefulnessAfter(1000, bad, 0.0));
        }

        [Test]
        public void ASeasonOfSuccessfulRituals_TrendsDownward_AndAMissedOneDoesNot()
        {
            // The calendar gives one ritual per season. Midpoint reduction is 35%; the season adds 25%.
            var w = 400;
            for (var season = 0; season < 4; season++)
            {
                w = Rise(w);
                w = WakefulnessModel.AfterSuccessfulRitual(w, 0.5);
            }
            Assert.Less(w, 400, "all rituals succeeding lowers wakefulness over a year");

            var missed = 400;
            for (var season = 0; season < 4; season++) missed = Rise(missed);
            Assert.AreEqual(WakefulnessModel.Max, missed, "no successful ritual for a year wakes the god");
        }

        static int Rise(int w)
        {
            for (var day = 1; day <= GameDateTime.DaysPerSeason; day++) w = WakefulnessModel.AfterDay(w, day);
            return w;
        }

        [Test]
        public void NewMoonNights_AreDaysOneToFourOfEachSeason()
        {
            Assert.IsTrue(RitualModel.IsNewMoon(new GameDateTime(1, Season.Spring, 1)));
            Assert.IsTrue(RitualModel.IsNewMoon(new GameDateTime(1, Season.Winter, 4)));
            Assert.IsFalse(RitualModel.IsNewMoon(new GameDateTime(1, Season.Fall, 5)));
            Assert.IsFalse(RitualModel.IsNewMoon(new GameDateTime(1, Season.Fall, 16)));
        }
    }
}
