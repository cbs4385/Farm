using System;
using System.IO;
using System.Linq;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The synthesised sound effects are original and well-behaved: audible, never clipping, no clicks at the ends, deterministic and
    // different from each other. The test also writes every effect as a WAV into Builds/sfx so a person can listen to them.
    public class SfxSynthTests
    {
        static readonly Sfx[] Synthesised = ((Sfx[])Enum.GetValues(typeof(Sfx))).Where(SfxSynth.Makes).ToArray();

        [Test]
        public void EveryNewEffect_IsAudible_Bounded_AndEndsQuietly()
        {
            Assert.AreEqual(18, Synthesised.Length);
            foreach (var sfx in Synthesised)
            {
                var d = SfxSynth.Make(sfx);
                Assert.Greater(d.Length, SfxSynth.SampleRate * 0.05f, sfx + " is not a blip too short to hear");
                Assert.Less(d.Length, SfxSynth.SampleRate * 1.3f, sfx + " is not a long jingle");
                var peak = d.Max(Math.Abs);
                Assert.That(peak, Is.InRange(0.3f, 0.95f), sfx + " peak");
                Assert.IsFalse(d.Any(float.IsNaN), sfx.ToString());
                var rms = (float)Math.Sqrt(d.Sum(x => x * x) / d.Length);
                Assert.Greater(rms, 0.02f, sfx + " is loud enough to notice");
                var tail = d.Skip(d.Length - 40).Max(Math.Abs);
                Assert.Less(tail, 0.05f, sfx + " ends without a click");
                Assert.Less(Math.Abs(d.Average()), 0.05f, sfx + " has no DC offset");
            }
        }

        [Test]
        public void TheEffects_AreDeterministic_AndDistinct()
        {
            foreach (var sfx in Synthesised) CollectionAssert.AreEqual(SfxSynth.Make(sfx), SfxSynth.Make(sfx), sfx.ToString());
            for (var i = 0; i < Synthesised.Length; i++)
                for (var j = i + 1; j < Synthesised.Length; j++)
                {
                    var a = SfxSynth.Make(Synthesised[i]); var b = SfxSynth.Make(Synthesised[j]);
                    var n = Math.Min(a.Length, b.Length);
                    double dot = 0, na = 0, nb = 0;
                    for (var k = 0; k < n; k++) { dot += a[k] * b[k]; na += a[k] * a[k]; nb += b[k] * b[k]; }
                    var corr = Math.Abs(dot / Math.Sqrt(na * nb + 1e-9));
                    Assert.Less(corr, 0.6, $"{Synthesised[i]} and {Synthesised[j]} sound different");
                }
        }

        [Test]
        public void EverySfxValue_HasAClip()
        {
            var clips = AudioService.BuildClips();
            Assert.AreEqual(Enum.GetValues(typeof(Sfx)).Length, clips.Length);
            for (var i = 0; i < clips.Length; i++) Assert.IsNotNull(clips[i], ((Sfx)i).ToString());
            foreach (var c in clips) UnityEngine.Object.DestroyImmediate(c);
        }

        [Test]
        public void ExportsWavFilesForReview()
        {
            Directory.CreateDirectory("Builds/sfx");
            foreach (var sfx in Synthesised)
            {
                var bytes = SfxSynth.ToWav(SfxSynth.Make(sfx));
                File.WriteAllBytes($"Builds/sfx/{sfx.ToString().ToLowerInvariant()}.wav", bytes);
                Assert.AreEqual((char)'R', (char)bytes[0]);
            }
        }
    }
}
