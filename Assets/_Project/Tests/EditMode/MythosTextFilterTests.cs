using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // X-008: text distortion at high dread. Full intensity only; never blocks reading.
    public class MythosTextFilterTests
    {
        const string Line = "The harvest looks wonderful this season, [player], and the {count} apples are ready for everyone.";

        TestSessionFixture _f;

        [SetUp]
        public void SetUp() => _f = new TestSessionFixture();

        [TearDown]
        public void TearDown()
        {
            MythosTextFilter.UninstallForTests();
            GameSession.HorrorLevelOverride = null;
            L.SetTable(new Dictionary<string, string>());
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        void Setup(int level, int dread)
        {
            GameSession.HorrorLevelOverride = level;
            _f.Session.SetVar(MythosIds.Vars.Dread, dread);
            L.SetTable(new Dictionary<string, string> { { "dlg.test.0", Line }, { "ui.close", Line }, { "dlg.test.c0", Line }, { "dlg.test.clue1", Line } });
            MythosTextFilter.Install(_f.Session);
        }

        [Test]
        public void Distort_IsDeterministic() =>
            Assert.AreEqual(MythosTextFilter.Distort("dlg.a.0", Line, 100, 12), MythosTextFilter.Distort("dlg.a.0", Line, 100, 12));

        [Test]
        public void Distort_BelowThreshold_ChangesNothing() =>
            Assert.AreEqual(Line, MythosTextFilter.Distort("dlg.a.0", Line, MythosTextFilter.Threshold - 1, 12));

        [Test]
        public void Distort_AtFullDread_ChangesSomeWords_ButKeepsTokensAndPunctuation()
        {
            var changed = false;
            for (var day = 0; day < 40; day++)
            {
                var d = MythosTextFilter.Distort("dlg.a.0", Line, 100, day);
                StringAssert.Contains("[player]", d);
                StringAssert.Contains("{count}", d);
                Assert.IsTrue(d.EndsWith("."));
                if (d != Line) changed = true;
            }
            Assert.IsTrue(changed, "at dread 100 some day must show a stumbling word");
        }

        [Test]
        public void Distort_NeverTouchesCapitalisedWords_OrShortOnes()
        {
            for (var day = 0; day < 60; day++)
            {
                var d = MythosTextFilter.Distort("dlg.a.0", "Tilda and Wren sat by the old fire.", 100, day);
                StringAssert.StartsWith("Tilda and Wren sat by", d);
            }
        }

        [Test]
        public void Distort_StrengthGrowsWithDread()
        {
            int Changes(int dread) => Enumerable.Range(0, 300).Count(day => MythosTextFilter.Distort("dlg.a.0", Line, dread, day) != Line);
            Assert.Less(Changes(55), Changes(100));
        }

        [TestCase("dlg.tilda.chat1.0", true)]
        [TestCase("event.wren_heart2.1", true)]
        [TestCase("letter.juno_clasp.body", true)]
        [TestCase("ui.close", false)]
        [TestCase("hover.seat", false)]
        [TestCase("dlg.tilda.ask.c0", false)]
        [TestCase("dlg.wren.clue1", false)]
        [TestCase("mythos.dream.full", false)]
        public void Eligible_OnlyStorySpeech(string key, bool expected) => Assert.AreEqual(expected, MythosTextFilter.Eligible(key));

        [Test]
        public void TheFilter_OffAndMild_NeverDistort()
        {
            foreach (var level in new[] { 0, 1 })
            {
                Setup(level, 100);
                for (var day = 0; day < 40; day++)
                {
                    _f.Session.Clock.SetTime(new GameDateTime(1, Season.Spring, 1 + day % 28, 12 * 60));
                    Assert.AreEqual(Line, L.Get("dlg.test.0"), $"level {level}");
                }
            }
        }

        [Test]
        public void TheFilter_AtFull_DistortsStorySpeechOnly()
        {
            Setup(2, 100);
            var seen = false;
            for (var day = 0; day < 28; day++)
            {
                _f.Session.Clock.SetTime(new GameDateTime(1, Season.Spring, 1 + day, 12 * 60));
                if (L.Get("dlg.test.0") != Line) seen = true;
                Assert.AreEqual(Line, L.Get("ui.close"));
                Assert.AreEqual(Line, L.Get("dlg.test.c0"));
                Assert.AreEqual(Line, L.Get("dlg.test.clue1"));
            }
            Assert.IsTrue(seen);
        }

        [Test]
        public void TheFilter_BelowThresholdAtFull_DoesNothing()
        {
            Setup(2, MythosTextFilter.Threshold - 1);
            Assert.AreEqual(Line, L.Get("dlg.test.0"));
        }
    }
}
