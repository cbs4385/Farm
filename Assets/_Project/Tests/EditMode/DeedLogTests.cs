using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The deed log and the `recent:` condition, and the sale reactions that fire on a big day.
    public class DeedLogTests
    {
        [Test]
        public void ADeed_IsRecentForSevenDays_ThenFadesAndFutureDaysAreIgnored()
        {
            var state = new DeedState();
            state.Last[DeedLog.Gift] = 100;
            Assert.IsTrue(DeedLog.IsRecent(state, DeedLog.Gift, 100));
            Assert.IsTrue(DeedLog.IsRecent(state, DeedLog.Gift, 106));
            Assert.IsFalse(DeedLog.IsRecent(state, DeedLog.Gift, 107));
            Assert.IsFalse(DeedLog.IsRecent(state, DeedLog.Gift, 99), "a deed from the future is not recent");
            Assert.IsFalse(DeedLog.IsRecent(state, DeedLog.Quest, 100), "a different deed");
            Assert.IsFalse(DeedLog.IsRecent(null, DeedLog.Gift, 100));
        }

        [Test]
        public void TheCondition_ReadsThroughTheWorldQuery()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var state = new GameState();
            var recent = false;
            var q = new StateWorldQuery(state, null) { DeedLookup = k => recent && k == "gift" };
            bool Eval(string c) => Conditions.TryEvaluate(c, q, out var r) && r;
            Assert.IsFalse(Eval("recent:gift"));
            recent = true;
            Assert.IsTrue(Eval("recent:gift"));
            Assert.IsFalse(Eval("recent:quest"));
        }

        [Test]
        public void TheSaleReactions_ListenForABigSale_AndCoverTheShopkeepers()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            foreach (var npc in new[] { "tilda", "wren", "felix", "marcus" })
            {
                var r = System.Linq.Enumerable.FirstOrDefault(story.Reactions, x => x.Id == "sale." + npc);
                Assert.IsNotNull(r, npc);
                Assert.AreEqual("random:big_sale", r.On);
                Assert.IsTrue(Reactions.IsKnownTrigger(r.On));
                Assert.IsNotNull(story.Dialogue(r.Dialogue), npc);
            }
        }
    }
}
