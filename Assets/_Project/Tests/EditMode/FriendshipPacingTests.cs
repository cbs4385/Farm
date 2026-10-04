using System.Collections.Generic;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-084: how fast hearts come for three kinds of player, so a heart event never arrives too early or never at all.
    // The numbers are the shipped rules (FriendshipModel) plus the slice heart-event rewards; see docs/narrative/PACING.md.
    public class FriendshipPacingTests
    {
        // Heart events: (hearts needed, points given). Existing hearts 2 and 5, then the slice scenes 4, 6, 8 and 10.
        static readonly int[][] Events = { new[] { 2, 40 }, new[] { 4, 50 }, new[] { 5, 60 }, new[] { 6, 60 }, new[] { 8, 70 }, new[] { 10, 80 } };

        // Days until `target` hearts. `giftsPerWeek` gifts of `giftPoints` each, a talk every day if `talkDaily`.
        static int DaysTo(int target, bool talkDaily, int giftsPerWeek, int giftPoints, int eventsEveryDays = 1)
        {
            var points = 0;
            var done = new HashSet<int>();
            for (var day = 1; day <= 1000; day++)
            {
                if (talkDaily) points += FriendshipModel.TalkPoints;
                if (giftsPerWeek > 0 && day % 7 < giftsPerWeek) points += giftPoints;
                if (day % eventsEveryDays == 0)
                    for (var i = 0; i < Events.Length; i++)
                        if (!done.Contains(i) && FriendshipModel.Hearts(points) >= Events[i][0]) { done.Add(i); points += Events[i][1]; }
                points = FriendshipModel.Clamp(points);
                if (FriendshipModel.Hearts(points) >= target) return day;
            }
            return int.MaxValue;
        }

        [Test]
        public void AnEngagedPlayer_ReachesTenHeartsInAboutTwoMonths()
        {
            var days = DaysTo(10, talkDaily: true, giftsPerWeek: 2, giftPoints: FriendshipModel.GiftPoints(GiftTaste.Liked, false));
            Assert.That(days, Is.InRange(40, 90), "engaged: talks daily and gives two liked gifts a week");
        }

        [Test]
        public void ACasualPlayer_StillFinishesWithinTheFirstYear()
        {
            var days = DaysTo(10, talkDaily: true, giftsPerWeek: 0, giftPoints: 0);
            Assert.LessOrEqual(days, 112, "talking every day is enough within a game year (112 days)");
        }

        [Test]
        public void ARarePlayer_NeedsMoreThanAYearButGetsThere()
        {
            var days = DaysTo(10, talkDaily: false, giftsPerWeek: 1, giftPoints: FriendshipModel.GiftPoints(GiftTaste.Liked, false), eventsEveryDays: 7);
            Assert.That(days, Is.InRange(112, 600));
        }

        [Test]
        public void TheFirstHeartEvent_ArrivesWithinTheFirstMonthForAnEngagedPlayer()
        {
            Assert.LessOrEqual(DaysTo(4, true, 2, FriendshipModel.GiftPoints(GiftTaste.Liked, false)), 30);
        }

        [Test]
        public void EachStep_TakesAtLeastAWeek_SoScenesDoNotStackUp()
        {
            var last = 0;
            foreach (var h in new[] { 4, 6, 8, 10 })
            {
                var d = DaysTo(h, true, 2, FriendshipModel.GiftPoints(GiftTaste.Liked, false));
                if (last > 0) Assert.GreaterOrEqual(d - last, 7, $"heart {h} comes at least a week after the one before");
                last = d;
            }
        }
    }
}
