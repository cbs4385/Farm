using System;
using System.IO;
using System.Linq;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // X-008: the horror layer's sound beds (drone by wakefulness step, tones from step 10, a heartbeat in the wood), played through the
    // generic ambience-layer hook.
    public class MythosSoundscapeTests
    {
        [Test]
        public void Off_IsSilent_AtAnyStepAndDread()
        {
            foreach (var step in new[] { 0, 7, 12, 19, 20 })
                Assert.IsTrue(MythosSoundscape.PlanFor(step, 100, 0, true).Silent, "step " + step);
        }

        [Test]
        public void TheDrone_GrowsWithTheStep_AndTheEarlyGameIsQuiet()
        {
            Assert.AreEqual(0f, MythosSoundscape.PlanFor(4, 0, 2, false).Drone);
            var last = 0f;
            foreach (var step in new[] { 5, 10, 15, 20 })
            {
                var d = MythosSoundscape.PlanFor(step, 0, 2, false).Drone;
                Assert.Greater(d, last, "step " + step);
                last = d;
            }
        }

        [Test]
        public void Tones_StartAtStepTen()
        {
            Assert.AreEqual(0f, MythosSoundscape.PlanFor(9, 0, 2, false).Tones);
            Assert.Greater(MythosSoundscape.PlanFor(10, 0, 2, false).Tones, 0f);
        }

        [Test]
        public void Mild_IsHalfOfFull()
        {
            var full = MythosSoundscape.PlanFor(16, 80, 2, true);
            var mild = MythosSoundscape.PlanFor(16, 80, 1, true);
            Assert.AreEqual(full.Drone / 2f, mild.Drone, 1e-5f);
            Assert.AreEqual(full.Tones / 2f, mild.Tones, 1e-5f);
            Assert.AreEqual(full.Heart / 2f, mild.Heart, 1e-5f);
        }

        [Test]
        public void TheHeartbeat_IsOnlyInTheWood_WhenDreadIsHigh_AndGrowsWithIt()
        {
            Assert.AreEqual(0f, MythosSoundscape.PlanFor(0, 90, 2, false).Heart, "not in the wood");
            Assert.AreEqual(0f, MythosSoundscape.PlanFor(0, 39, 2, true).Heart, "dread too low");
            Assert.Less(MythosSoundscape.PlanFor(0, 45, 2, true).Heart, MythosSoundscape.PlanFor(0, 100, 2, true).Heart);
        }

        [Test]
        public void EveryLoop_IsAudible_Bounded_Deterministic_AndLoopsWithoutAClick()
        {
            foreach (var (name, make) in new (string, Func<float[]>)[] { ("drone", MythosSoundscape.Drone), ("tones", MythosSoundscape.Tones), ("heart", MythosSoundscape.Heart) })
            {
                var d = make();
                Assert.Greater(d.Length, AudioService.LayerSampleRate * 3f, name);
                Assert.IsFalse(d.Any(float.IsNaN), name);
                Assert.That(d.Max(Math.Abs), Is.InRange(0.3f, 0.95f), name + " peak");
                var rms = (float)Math.Sqrt(d.Sum(x => x * x) / d.Length);
                Assert.Greater(rms, 0.01f, name + " is audible");
                var jump = Math.Abs(d[0] - d[d.Length - 1]);
                var typical = Enumerable.Range(1, d.Length - 1).Average(i => Math.Abs(d[i] - d[i - 1]));
                Assert.Less(jump, typical * 8f + 0.02f, name + " loops without a click");
                CollectionAssert.AreEqual(d, make(), name + " is deterministic");
                Directory.CreateDirectory("Builds/sfx");
                File.WriteAllBytes($"Builds/sfx/mythos_{name}.wav", SfxSynth.ToWav(d));       // to listen to by hand
            }
        }

        [Test]
        public void Tones_AreMostlySilence()
        {
            var d = MythosSoundscape.Tones();
            var quiet = d.Count(x => Math.Abs(x) < 0.01f) / (float)d.Length;
            Assert.Greater(quiet, 0.4f);
        }

        [Test]
        public void TheAudioService_FadesLayersToTheirVolume_AndSilencesThem()
        {
            var go = new GameObject("audio-test");
            try
            {
                var audio = go.AddComponent<AudioService>();
                audio.SetAmbienceLayer("test.layer", () => new float[AudioService.LayerSampleRate], 0.4f);
                Assert.AreEqual(0.4f, audio.LayerWanted("test.layer"), 1e-5f);
                audio.SetAmbienceLayer("test.layer", null, 0f);
                Assert.AreEqual(0f, audio.LayerWanted("test.layer"));
                audio.SetAmbienceLayer("never.used", null, 0f);       // silencing a layer that never played makes nothing
                Assert.AreEqual(0f, audio.LayerWanted("never.used"));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
