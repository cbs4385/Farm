using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-10: "I received 50 gold without selling or shipping anything": the quest's reward arrived without a word. The notice names it now.
    public class QuestRewardTextTests
    {
        [Test]
        public void ACompletedQuest_SaysWhatItPaid()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            using (var f = new TestSessionFixture(db.AllItems, db.AllCrops))
            {
                var s = f.Session;
                s.Story = StoryContent.LoadFromResources();
                var def = s.Story.Quest("tut_farm");
                Assert.AreEqual("50 gold", QuestLog.RewardSummary(s, def));
                if (!QuestLog.IsActive(s.State, def.Id)) Assert.IsTrue(QuestLog.Start(s, def));
                var before = s.State.Gold;
                s.AddVar(QuestLog.Stats.Tilled, 3); s.AddVar(QuestLog.Stats.Planted, 3); s.AddVar(QuestLog.Stats.Watered, 3);
                QuestLog.Tick(s);          // a tutorial quest completes by itself once its objectives hold (it may already have)
                Assert.AreEqual(QuestStatus.Done, QuestLog.StatusOf(s.State, def.Id));
                Assert.AreEqual(before + 50, s.State.Gold);
                Assert.IsTrue(f.Toasts.Exists(t => t.Contains("First Furrows") && t.Contains("50 gold")), "the notice says where the gold came from");
            }
        }

        [Test]
        public void ARewardOfItemsAndGold_ListsBoth()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            using (var f = new TestSessionFixture(db.AllItems, db.AllCrops))
            {
                f.Session.Story = StoryContent.LoadFromResources();
                var harvest = f.Session.Story.Quest("tut_harvest");
                var text = QuestLog.RewardSummary(f.Session, harvest);
                StringAssert.StartsWith("100 gold", text);
                StringAssert.Contains("3 ", text, "and the three fertilizers");
            }
        }

        [Test]
        public void TheNumberOfCoins_GrowsWithTheAmount_ButStaysABurstNotAFlood()
        {
            Assert.AreEqual(CoinRain.CountFor(5), CoinRain.CountFor(-5));
            Assert.Less(CoinRain.CountFor(10), CoinRain.CountFor(200));
            Assert.AreEqual(CoinRain.CountFor(100000), 28);
            Assert.GreaterOrEqual(CoinRain.CountFor(1), 4);
        }
    }
}
