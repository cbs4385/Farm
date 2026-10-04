using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-096: the pure parts of the dialogue box: typewriter timing with cues, auto-advance, the backlog, settings.
    public class DialoguePacingTests
    {
        static TextCue Pause(int index, float seconds) => new TextCue(index, seconds, 0f);
        static TextCue Speed(int index, float speed) => new TextCue(index, 0f, speed);

        [Test]
        public void TheTypewriter_RevealsAtTheBaseSpeed()
        {
            var t = new Typewriter(100, null, 50f);
            Assert.AreEqual(0, t.Visible);
            t.Advance(0.5f);
            Assert.AreEqual(25, t.Visible);
            t.Advance(1.0f);
            Assert.AreEqual(75, t.Visible);
            Assert.IsFalse(t.Done);
            t.Advance(10f);
            Assert.AreEqual(100, t.Visible);
            Assert.IsTrue(t.Done);
        }

        [Test]
        public void SmallFrames_AddUpTheSameAsOneBigFrame()
        {
            var a = new Typewriter(90, new[] { Pause(20, 0.3f), Speed(40, 2f) }, 30f);
            var b = new Typewriter(90, new[] { Pause(20, 0.3f), Speed(40, 2f) }, 30f);
            for (var i = 0; i < 100; i++) a.Advance(0.01f);
            b.Advance(1f);
            Assert.AreEqual(b.Visible, a.Visible, 1, "the frame rate does not change what is shown");
        }

        [Test]
        public void APause_HoldsTheRevealAtItsCharacter()
        {
            var t = new Typewriter(100, new[] { Pause(10, 1.0f) }, 10f);   // 10 chars per second
            t.Advance(1.0f);                                              // exactly reaches character 10
            Assert.AreEqual(10, t.Visible);
            t.Advance(0.5f);
            Assert.AreEqual(10, t.Visible, "paused");
            Assert.IsTrue(t.Pausing);
            t.Advance(0.5f);
            Assert.IsFalse(t.Pausing);
            t.Advance(0.5f);
            Assert.AreEqual(15, t.Visible, "typing again after the pause");
        }

        [Test]
        public void APauseAtTheStart_DelaysTheFirstCharacter()
        {
            var t = new Typewriter(10, new[] { Pause(0, 0.4f) }, 10f);
            t.Advance(0.3f);
            Assert.AreEqual(0, t.Visible);
            t.Advance(0.2f);                                              // 0.1 s into the typing
            Assert.AreEqual(1, t.Visible);
        }

        [Test]
        public void ASpeedCue_ChangesThePaceFromItsCharacter()
        {
            var t = new Typewriter(40, new[] { Speed(10, 0.5f) }, 10f);   // 10 cps, then 5 cps from character 10
            t.Advance(1.0f);
            Assert.AreEqual(10, t.Visible);
            t.Advance(1.0f);
            Assert.AreEqual(15, t.Visible);
        }

        [Test]
        public void Cues_AreHonouredInOrder_EvenIfGivenOutOfOrder()
        {
            var t = new Typewriter(50, new[] { Pause(20, 1f), Pause(10, 1f) }, 10f);
            t.Advance(1.0f);
            Assert.AreEqual(10, t.Visible);
            Assert.IsTrue(t.Pausing, "the earlier cue fires first");
        }

        [Test]
        public void Finish_ShowsEverything_AndDropsPendingCues()
        {
            var t = new Typewriter(50, new[] { Pause(10, 5f) }, 10f);
            t.Advance(1.0f);
            t.Finish();
            Assert.IsTrue(t.Done);
            Assert.AreEqual(50, t.Visible);
            Assert.IsFalse(t.Pausing);
        }

        [Test]
        public void InstantSpeed_ShowsTheWholeLineAtOnce()
        {
            var t = new Typewriter(80, new[] { Pause(10, 2f) }, DialoguePacing.CpsFor(DialoguePacing.InstantSpeed));
            Assert.IsTrue(t.Done);
            Assert.AreEqual(80, t.Visible);
        }

        [Test]
        public void AnEmptyLine_IsDoneAtOnce()
        {
            Assert.IsTrue(new Typewriter(0, null, 90f).Done);
        }

        [Test]
        public void CueBeyondTheEnd_IsHarmless()
        {
            var t = new Typewriter(10, new[] { Pause(99, 1f) }, 10f);
            t.Advance(5f);
            Assert.IsTrue(t.Done);
        }

        [Test]
        public void TheSpeedSetting_MapsToCharactersPerSecond()
        {
            Assert.AreEqual(45f, DialoguePacing.CpsFor(0));
            Assert.AreEqual(90f, DialoguePacing.CpsFor(1));
            Assert.AreEqual(180f, DialoguePacing.CpsFor(2));
            Assert.IsTrue(float.IsPositiveInfinity(DialoguePacing.CpsFor(3)));
            Assert.AreEqual(45f, DialoguePacing.CpsFor(-5), "out of range is clamped");
            Assert.IsTrue(float.IsPositiveInfinity(DialoguePacing.CpsFor(99)));
        }

        [Test]
        public void AutoAdvance_WaitsLongerForLongerLines()
        {
            Assert.AreEqual(1.5f, DialoguePacing.AutoAdvanceSeconds(0), 0.0001f);
            Assert.Greater(DialoguePacing.AutoAdvanceSeconds(100), DialoguePacing.AutoAdvanceSeconds(10));
            Assert.AreEqual(DialoguePacing.AutoAdvanceSeconds(200), DialoguePacing.AutoAdvanceSeconds(5000), "capped");
        }

        [Test]
        public void TheBacklog_KeepsTheLastLines_AndSkipsEmptyAndRepeated()
        {
            var log = new DialogueBacklog();
            log.Add("Wren", "Hello.");
            log.Add("Wren", "Hello.");
            log.Add("Wren", "   ");
            log.Add("", null);
            Assert.AreEqual(1, log.Entries.Count);
            for (var i = 0; i < DialogueBacklog.Capacity + 10; i++) log.Add("Wren", "Line " + i);
            Assert.AreEqual(DialogueBacklog.Capacity, log.Entries.Count);
            Assert.AreEqual("Line " + (DialogueBacklog.Capacity + 9), log.Entries.Last().Text, "newest last");
            log.Clear();
            Assert.IsEmpty(log.Entries);
        }

        [Test]
        public void TheNewSettings_AreClamped_AndDefaultToNormal()
        {
            var s = new SettingsData();
            Assert.AreEqual(DialoguePacing.DefaultSpeed, s.DialogueSpeed);
            Assert.IsFalse(s.AutoAdvance);
            s.DialogueSpeed = 17;
            s.Clamp();
            Assert.AreEqual(3, s.DialogueSpeed);
            s.DialogueSpeed = -2;
            s.Clamp();
            Assert.AreEqual(0, s.DialogueSpeed);
        }

        [Test]
        public void OldSettingsFiles_LoadWithDefaults()
        {
            var s = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsData>("{\"MasterVolume\":0.5}");
            Assert.AreEqual(1, s.DialogueSpeed);
            Assert.IsFalse(s.AutoAdvance);
        }

        [Test]
        public void APortraitExpression_FallsBackToTheNeutralPortrait()
        {
            var npc = NpcDefinition.Create("test", Season.Spring, 1, MapIds.Village, 5, 5);
            Assert.IsNull(npc.PortraitFor("happy"), "no art at all: nothing, not a crash");
            Assert.IsNull(npc.PortraitFor(null));
            Assert.IsNull(npc.PortraitFor("not_an_expression"));
            CollectionAssert.AreEqual(NpcDefinition.ExpressionNames, DialogueVocabulary.Expressions, "one list of expressions");
        }
    }
}
