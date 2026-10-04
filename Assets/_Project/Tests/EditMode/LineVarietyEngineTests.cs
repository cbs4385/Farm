using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-090: the line variety engine (cooldowns, tiers, never-heard bias, rarity, same-day stability, saved memory).
    public class LineVarietyEngineTests
    {
        sealed class World : IWorldQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public string WeatherId = "sunny";
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string Weather => WeatherId;
            public string MapId => "Village";
        }

        static DialogueSetEntry E(string id, int priority = 0, string condition = null, string rarity = null, int cooldown = -1, float weight = 1f) =>
            new DialogueSetEntry { Dialogue = id, Priority = priority, Condition = condition, Rarity = rarity, Cooldown = cooldown, Weight = weight };

        static DialogueSet Set(params DialogueSetEntry[] entries) => new DialogueSet { Id = "s", Entries = entries.ToList() };

        static string Day(DialogueSet set, World w, LineMemory m, int day) =>
            set.PickVaried(w, day * 7919 + 13, m, "s", day);

        static IEnumerable<DialogueSetEntry> Lines(int n) => Enumerable.Range(0, n).Select(i => E("line" + i));

        [Test]
        public void Unit_IsDeterministic_AndInRange()
        {
            for (var seed = -50; seed < 5000; seed++)
            {
                var v = DialogueSet.Unit(seed);
                Assert.GreaterOrEqual(v, 0.0);
                Assert.Less(v, 1.0);
                Assert.AreEqual(v, DialogueSet.Unit(seed));
            }
            Assert.AreNotEqual(DialogueSet.Unit(1), DialogueSet.Unit(2));
        }

        [Test]
        public void WithoutMemory_BehavesLikeThePlainPick()
        {
            var set = Set(Lines(6).ToArray());
            var w = new World();
            for (var seed = 0; seed < 50; seed++) Assert.AreEqual(set.Pick(w, seed), set.PickVaried(w, seed, null, "s", 3));
        }

        [Test]
        public void TalkingTwiceTheSameDay_SaysTheSameThing()
        {
            var set = Set(Lines(8).ToArray());
            var w = new World();
            var m = new LineMemory();
            for (var day = 0; day < 40; day++)
            {
                var first = Day(set, w, m, day);
                Assert.AreEqual(first, Day(set, w, m, day), $"day {day}");
                Assert.AreEqual(first, Day(set, w, m, day), $"day {day}");
            }
        }

        [Test]
        public void ALargePool_NeverRepeatsWithinTheCooldown()
        {
            var set = Set(Lines(20).ToArray());
            var w = new World();
            var m = new LineMemory();
            var lastSeen = new Dictionary<string, int>();
            for (var day = 0; day < 200; day++)
            {
                var id = Day(set, w, m, day);
                if (lastSeen.TryGetValue(id, out var before))
                    Assert.GreaterOrEqual(day - before, DialogueSet.DefaultCooldownDays, $"{id} repeated after {day - before} days");
                lastSeen[id] = day;
            }
        }

        [Test]
        public void ASmallPool_RotatesThroughEveryLine_BeforeAnyRepeat()
        {
            var set = Set(Lines(5).ToArray());
            var w = new World();
            var m = new LineMemory();
            var lastSeen = new Dictionary<string, int>();
            for (var day = 0; day < 60; day++)
            {
                var id = Day(set, w, m, day);
                if (lastSeen.TryGetValue(id, out var before)) Assert.AreEqual(5, day - before, $"{id} on day {day}: the least recent line goes first");
                lastSeen[id] = day;
            }
        }

        [Test]
        public void EveryLine_IsHeardWithinAPoolsLength()
        {
            var set = Set(Lines(10).ToArray());
            var m = new LineMemory();
            var heard = new HashSet<string>();
            for (var day = 0; day < 10; day++) heard.Add(Day(set, new World(), m, day));
            Assert.AreEqual(10, heard.Count);
        }

        [Test]
        public void ARainTierOnCooldown_FallsThroughToTheNextTier()
        {
            var set = Set(E("rain1", 1, "weather:rain"), E("rain2", 1, "weather:rain"), E("chat1"), E("chat2"), E("chat3"), E("chat4"));
            var w = new World { WeatherId = "rain" };
            var m = new LineMemory();
            var said = Enumerable.Range(0, 6).Select(d => Day(set, w, m, d)).ToList();
            CollectionAssert.AreEquivalent(new[] { "rain1", "rain2" }, said.Take(2), "the higher tier goes first");
            Assert.IsTrue(said.Skip(2).All(s => s.StartsWith("chat")), "then fresh lines of the lower tier: " + string.Join(",", said));
        }

        [Test]
        public void StoryBeats_NeverGoOnCooldown()
        {
            var set = Set(E("offer", 4, "flag:open"), E("chat1"), E("chat2"));
            var w = new World();
            w.Flags.Add("open");
            var m = new LineMemory();
            for (var day = 0; day < 20; day++) Assert.AreEqual("offer", Day(set, w, m, day), $"day {day}");
        }

        [Test]
        public void AFirstMeeting_IsNotRepeated_OnceTheFlagIsSet()
        {
            var set = Set(E("first", 100, "!flag:met"), E("chat1"), E("chat2"), E("chat3"));
            var w = new World();
            var m = new LineMemory();
            Assert.AreEqual("first", Day(set, w, m, 0));
            w.Flags.Add("met");
            Assert.AreNotEqual("first", Day(set, w, m, 0), "talking again the same day moves on");
        }

        [Test]
        public void ANewHigherTierLine_InterruptsTheSameDayRepeat()
        {
            var set = Set(E("scene", 6, "flag:go"), E("chat1"), E("chat2"), E("chat3"));
            var w = new World();
            var m = new LineMemory();
            var chat = Day(set, w, m, 5);
            Assert.AreEqual(chat, Day(set, w, m, 5));
            w.Flags.Add("go");
            Assert.AreEqual("scene", Day(set, w, m, 5));
        }

        [Test]
        public void RarityTiers_AreChosenRarely_ButNotNever()
        {
            var set = Set(E("common", 0, null, null, 0), E("legend", 0, null, LineRarity.Legendary, 0), E("rare", 0, null, LineRarity.Rare, 0));
            var w = new World();
            var m = new LineMemory();
            var counts = new Dictionary<string, int> { { "common", 0 }, { "legend", 0 }, { "rare", 0 } };
            const int days = 6000;
            for (var day = 0; day < days; day++)
            {
                // A fresh memory each day isolates the weights from the never-heard boost.
                counts[set.PickVaried(w, day * 7919 + 13, new LineMemory(), "s", day)]++;
            }
            Assert.Greater(counts["legend"], 0, "legendary lines can appear");
            Assert.Less(counts["legend"], days / 40, "but rarely");
            Assert.Greater(counts["rare"], counts["legend"]);
            Assert.Greater(counts["common"], counts["rare"] * 4);
            Assert.NotNull(m);
        }

        [Test]
        public void TheWeightMultiplier_ShiftsTheOdds()
        {
            var set = Set(E("a", 0, null, null, 0, 1f), E("b", 0, null, null, 0, 4f));
            var w = new World();
            var b = 0;
            for (var day = 0; day < 4000; day++) if (set.PickVaried(w, day * 7919 + 13, new LineMemory(), "s", day) == "b") b++;
            Assert.Greater(b, 4000 * 0.7);
            Assert.Less(b, 4000 * 0.9);
        }

        [Test]
        public void TheMemory_SurvivesTheSave_AndIsCapped()
        {
            var m = new LineMemory();
            m.Record("s", "x", 4, 0);
            m.Record("s", "x", 9, 0);
            var back = JsonConvert.DeserializeObject<LineMemory>(JsonConvert.SerializeObject(m));
            Assert.AreEqual(9, back.LastHeardDay("s", "x"));
            Assert.AreEqual(2, back.TimesHeard("s", "x"));
            Assert.AreEqual(-1, back.LastHeardDay("s", "never"));
            for (var i = 0; i < LineMemory.MaxPerScope + 40; i++) m.Record("s", "l" + i, i, 0);
            Assert.LessOrEqual(m.Heard["s"].Count, LineMemory.MaxPerScope);
            Assert.IsTrue(m.WasHeard("s", "l" + (LineMemory.MaxPerScope + 39)), "the newest lines are kept");
        }

        [Test]
        public void TheSameSeedAndMemory_GiveTheSameAnswer()
        {
            var set = Set(Lines(12).ToArray());
            var a = new LineMemory();
            var b = new LineMemory();
            for (var day = 0; day < 60; day++) Assert.AreEqual(Day(set, new World(), a, day), Day(set, new World(), b, day));
        }
    }
}
