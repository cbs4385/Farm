using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-124: shop and service state is readable by story conditions through IGameQuery.
    public class ShopServiceAtomsTests
    {
        static GameState NewState() => new GameState { Gold = 100 };

        static StateWorldQuery Query(GameState state)
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            return new StateWorldQuery(state, null);
        }

        static bool Eval(string cond, StateWorldQuery q) => Conditions.TryEvaluate(cond, q, out var r) && r;

        [Test]
        public void UpgradeState_FollowsThePendingList()
        {
            var state = NewState();
            var q = Query(state);
            Assert.IsTrue(Eval("upgrade:none", q));
            var today = q.Now.TotalDays;
            state.PendingUpgrades.Add(new PendingUpgrade { ToolItemId = "tool.hoe", Tier = 1, ReadyDay = today + 2 });
            Assert.IsTrue(Eval("upgrade:waiting", q));
            Assert.IsFalse(Eval("upgrade:ready", q));
            state.PendingUpgrades[0].ReadyDay = today;
            Assert.IsTrue(Eval("upgrade:ready", q));
        }

        [Test]
        public void HallAndUpgrades_AreReadable()
        {
            var state = NewState();
            var q = Query(state);
            Assert.IsTrue(Eval("hall:==0", q));
            Assert.IsFalse(Eval("upgraded:backpack.24", q));
            state.UpgradesDone.Add("backpack.24");
            Assert.IsTrue(Eval("upgraded:backpack.24", q));
            foreach (var room in new[] { "hall_pantry", "hall_crafts", "hall_fishtank" }) state.Quests[room] = new QuestProgress { Status = "done" };
            Assert.IsTrue(Eval("hall:>=3", q));
            Assert.IsFalse(Eval("hall:>=4", q));
        }

        [Test]
        public void GoldAndShipped_Compare()
        {
            var state = NewState();
            var q = Query(state);
            Assert.IsTrue(Eval("gold:<150", q));
            Assert.IsFalse(Eval("gold:>=5000", q));
            state.Gold = 6000;
            Assert.IsTrue(Eval("gold:>=5000", q));
            state.ShippedTotals["crop.parsnip"] = 12;
            state.ShippedTotals["crop.potato"] = 10;
            Assert.IsTrue(Eval("shipped:>=20", q));
            Assert.IsFalse(Eval("shipped:>=100", q));
        }
    }
}
