using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Running (1.75 times the walking speed, for energy), the stick and mouse sensitivity settings, and the run action in the key list.
    public class RunAndSensitivityTests
    {
        [Test]
        public void Running_IsOneAndThreeQuartersTimesTheWalk()
        {
            var run = new RunModel();
            Assert.AreEqual(1.75f, RunModel.SpeedFactor);
            Assert.AreEqual(1.75f, run.Step(true, true, 100, 0.016f, false, out _));
            Assert.AreEqual(1f, run.Step(false, true, 100, 0.016f, false, out _), "not held");
            Assert.AreEqual(1f, run.Step(true, false, 100, 0.016f, false, out _), "standing still");
            Assert.AreEqual(1f, run.Step(true, true, 0, 0.016f, false, out _), "no energy: walks");
        }

        [Test]
        public void Running_CostsOneEnergyForEveryThreeSeconds_AndNothingWhenWalkingOrStanding()
        {
            var run = new RunModel();
            var total = 0;
            for (var i = 0; i < 600; i++) { run.Step(true, true, 100, 0.1f, false, out var cost); total += cost; }          // a minute
            Assert.AreEqual(20, total);
            total = 0;
            for (var i = 0; i < 600; i++) { run.Step(false, true, 100, 0.1f, false, out var cost); total += cost; }
            for (var i = 0; i < 600; i++) { run.Step(true, false, 100, 0.1f, false, out var cost); total += cost; }
            Assert.AreEqual(0, total);
        }

        [Test]
        public void Running_CostsHalfAsMuch_WithRelaxedEnergy()
        {
            var run = new RunModel();
            var total = 0;
            for (var i = 0; i < 600; i++) { run.Step(true, true, 100, 0.1f, true, out var cost); total += cost; }
            Assert.AreEqual(10, total);
        }

        [Test]
        public void TheStickSensitivity_ScalesATilt_AndNeverPastFull()
        {
            Assert.AreEqual(new Vector2(0.5f, 0f), RunModel.ApplyStickSensitivity(new Vector2(0.5f, 0f), 1f));
            Assert.AreEqual(1f, RunModel.ApplyStickSensitivity(new Vector2(0.5f, 0f), 2f).x, 0.001f);
            Assert.AreEqual(0.25f, RunModel.ApplyStickSensitivity(new Vector2(0.5f, 0f), 0.5f).x, 0.001f);
            Assert.LessOrEqual(RunModel.ApplyStickSensitivity(new Vector2(1f, 1f), 2f).magnitude, 1.0001f);
            Assert.AreEqual(Vector2.zero, RunModel.ApplyStickSensitivity(Vector2.zero, 2f));
        }

        [Test]
        public void TheMouseSensitivity_MovesThePointerByTheExtraAndStaysOnTheScreen()
        {
            var screen = new Vector2(1280f, 720f);
            Assert.AreEqual(new Vector2(110f, 50f), PointerMath.Scale(new Vector2(110f, 50f), new Vector2(10f, 0f), 1f, screen));
            Assert.AreEqual(new Vector2(130f, 50f), PointerMath.Scale(new Vector2(110f, 50f), new Vector2(10f, 0f), 3f, screen));
            Assert.AreEqual(new Vector2(105f, 50f), PointerMath.Scale(new Vector2(110f, 50f), new Vector2(10f, 0f), 0.5f, screen));
            Assert.AreEqual(1279f, PointerMath.Scale(new Vector2(1275f, 50f), new Vector2(20f, 0f), 2.5f, screen).x);
        }

        [Test]
        public void TheSensitivities_DefaultToOne_AreClamped_AndAnOldFileGetsTheDefault()
        {
            var s = new SettingsData();
            Assert.AreEqual(1f, s.MouseSensitivity);
            Assert.AreEqual(1f, s.ControllerSensitivity);
            s.MouseSensitivity = 9f; s.ControllerSensitivity = -3f;
            s.Clamp();
            Assert.AreEqual(SettingsData.MaxMouseSensitivity, s.MouseSensitivity);
            Assert.AreEqual(1f, s.ControllerSensitivity, "a value below zero is a missing one");
            var old = JsonUtility.FromJson<SettingsData>("{\"MasterVolume\":0.5}");
            old.MouseSensitivity = 0f; old.Clamp();
            Assert.AreEqual(1f, old.MouseSensitivity);
        }

        [Test]
        public void TheRunAction_IsHeldOnAKeyAndAPadButton_AndCanBeRebound()
        {
            var asset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(InputNames.ResourcePath);
            var run = asset.FindActionMap(InputNames.GameplayMap, true)[InputNames.Run];
            Assert.IsNotNull(run);
            CollectionAssert.Contains(InputNames.Rebindable, InputNames.Run);
            CollectionAssert.Contains(InputNames.PadRebindable, InputNames.Run);
            Assert.GreaterOrEqual(BindingNames.IndexOf(run, BindingNames.Gamepad), 0);
            Assert.GreaterOrEqual(BindingNames.IndexOf(run, BindingNames.Keyboard), 0);
        }
    }
}
