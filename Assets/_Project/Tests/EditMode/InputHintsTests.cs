using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    // The hints for the controls live in an object each run owns (ADR 0006), so one run's pad cannot leak into the next.
    public sealed class InputHintsTests
    {
        [TearDown]
        public void TearDown() => ControlPrompts.ResetForTests();

        [Test]
        public void TwoSetsOfHints_DoNotShareWhatThePlayerIsUsing()
        {
            var a = new InputHints();
            var b = new InputHints();
            a.SetKind(InputKind.Gamepad);
            Assert.AreEqual(InputKind.Gamepad, a.Kind);
            Assert.AreEqual(InputKind.KeyboardMouse, b.Kind);
        }

        [Test]
        public void TheLanguageTableFollowsTheHintsInUse()
        {
            var hints = new InputHints { Rebound = (kind, action) => kind == InputKind.Gamepad ? "PAD-" + action : "KEY-" + action };
            ControlPrompts.Use(hints);
            Assert.AreEqual("go KEY-Use", ControlPrompts.Expand("go [[Use]]"));
            hints.SetKind(InputKind.Gamepad);
            Assert.AreEqual("go PAD-Use", ControlPrompts.Expand("go [[Use]]"));
            Assert.AreEqual(InputKind.Gamepad, ControlPrompts.Kind);
        }

        [Test]
        public void ChangingTheKind_IsAnnouncedToTheLabelsThatListen()
        {
            var hints = new InputHints();
            ControlPrompts.Use(hints);
            var heard = 0;
            ControlPrompts.Changed += () => heard++;
            hints.SetKind(InputKind.Gamepad);
            hints.SetKind(InputKind.Gamepad);            // no change, no notice
            hints.NotifyChanged();
            Assert.AreEqual(2, heard);
        }

        [Test]
        public void AResetBetweenTests_BringsBackFreshHints()
        {
            var hints = new InputHints();
            ControlPrompts.Use(hints);
            hints.SetKind(InputKind.Gamepad);
            TestResets.RunAll();
            Assert.AreEqual(InputKind.KeyboardMouse, ControlPrompts.Kind);
            Assert.AreNotSame(hints, ControlPrompts.Current);
        }
    }
}
