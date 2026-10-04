using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-108: the "missed you" greeting and the return bonus.
    public class WarmthTests
    {
        static readonly string[] All = { "tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara" };

        [Test]
        public void AReturningVillager_IsMissed_OnlyAfterAWeek_AndOnlyIfTheyKnowThePlayer()
        {
            Assert.IsFalse(WarmthModel.Missed(true, WarmthModel.MissedAfterDays - 1, 5), "six days is not a week");
            Assert.IsTrue(WarmthModel.Missed(true, WarmthModel.MissedAfterDays, 5));
            Assert.IsFalse(WarmthModel.Missed(false, 30, 5), "never met");
            Assert.IsFalse(WarmthModel.Missed(true, -1, 5), "no earlier contact");
            Assert.IsFalse(WarmthModel.Missed(true, 30, WarmthModel.MinimumHearts - 1), "barely acquainted");
        }

        [Test]
        public void TheBonus_IsSmall_ButMoreThanADaysTalk_AndMoreThanTheLossFromAWeek()
        {
            Assert.Greater(WarmthModel.ReturnBonusPoints, 0);
            Assert.Less(WarmthModel.ReturnBonusPoints, FriendshipModel.PointsPerHeart / 10);
            var weekOfDecay = 4 * (int)System.Math.Round((double)FriendshipModel.BaseDecayPerDay);      // from day 3 to day 7
            Assert.GreaterOrEqual(WarmthModel.ReturnBonusPoints, weekOfDecay);
        }

        [Test]
        public void EveryVillager_HasAMissedYouLine_InTheirOwnVoice()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var en = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var npc in All)
            {
                var d = story.Dialogue(WarmthModel.DialogueId(npc));
                Assert.IsNotNull(d, npc);
                Assert.AreEqual(npc, d.Nodes[0].Speaker, npc);
                var text = en[d.Nodes[0].Text];
                Assert.LessOrEqual(text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length, 28, npc + ": " + text);
                if (npc == "hazel" || npc == "bram") Assert.IsFalse(text.Contains("!"), npc + " uses no exclamation marks");
            }
        }
    }
}
