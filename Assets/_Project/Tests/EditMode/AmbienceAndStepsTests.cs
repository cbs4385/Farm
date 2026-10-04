using System;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Ambience beds, footsteps and animal calls: behave, loop without a click, and are chosen sensibly.
    public class AmbienceAndStepsTests
    {
        static readonly AmbienceKind[] Beds = { AmbienceKind.Rain, AmbienceKind.Wind, AmbienceKind.Birds, AmbienceKind.Crickets };

        [Test]
        public void EveryBed_IsLongEnough_AudibleBounded_AndLoopsWithoutAClick()
        {
            Directory.CreateDirectory("Builds/sfx");
            foreach (var kind in Beds)
            {
                var d = AmbienceSynth.Make(kind);
                Assert.Greater(d.Length, AmbienceSynth.SampleRate * 2.5f, kind + " is a real loop, not a blip");
                Assert.Less(d.Length, AmbienceSynth.SampleRate * 8f, kind.ToString());
                Assert.IsFalse(d.Any(float.IsNaN), kind.ToString());
                Assert.That(d.Max(Math.Abs), Is.InRange(0.3f, 0.95f), kind + " peak");
                var rms = (float)Math.Sqrt(d.Sum(x => x * x) / d.Length);
                Assert.Greater(rms, 0.03f, kind + " is audible");
                // The jump from the last sample to the first is no bigger than the typical step between samples.
                var jump = Math.Abs(d[0] - d[d.Length - 1]);
                var typical = Enumerable.Range(1, d.Length - 1).Average(i => Math.Abs(d[i] - d[i - 1]));
                Assert.Less(jump, typical * 8f + 0.02f, kind + " loops without a click");
                CollectionAssert.AreEqual(d, AmbienceSynth.Make(kind), kind + " is deterministic");
                File.WriteAllBytes($"Builds/sfx/ambience_{kind.ToString().ToLowerInvariant()}.wav", SfxSynth.ToWav(d));
            }
        }

        [Test]
        public void TheDirector_ChoosesBySkyTimeAndPlace()
        {
            Assert.AreEqual(AmbienceKind.None, AmbienceDirector.Choose(true, WeatherIds.Rain, 12, Season.Spring), "indoors is quiet");
            Assert.AreEqual(AmbienceKind.Rain, AmbienceDirector.Choose(false, WeatherIds.Rain, 12, Season.Spring));
            Assert.AreEqual(AmbienceKind.Rain, AmbienceDirector.Choose(false, WeatherIds.Storm, 3, Season.Fall));
            Assert.AreEqual(AmbienceKind.Wind, AmbienceDirector.Choose(false, WeatherIds.Wind, 12, Season.Fall));
            Assert.AreEqual(AmbienceKind.Wind, AmbienceDirector.Choose(false, WeatherIds.Snow, 12, Season.Winter));
            Assert.AreEqual(AmbienceKind.Birds, AmbienceDirector.Choose(false, WeatherIds.Sunny, 10, Season.Summer));
            Assert.AreEqual(AmbienceKind.Crickets, AmbienceDirector.Choose(false, WeatherIds.Sunny, 22, Season.Summer));
            Assert.AreEqual(AmbienceKind.None, AmbienceDirector.Choose(false, WeatherIds.Sunny, 22, Season.Winter), "no crickets in the snow season");
        }

        [Test]
        public void Footsteps_LandOncePerStride_NeverStandingStill_AndAlternatePitch()
        {
            var f = new Footsteps();
            Assert.IsFalse(f.Walk(0f));
            var steps = 0;
            for (var i = 0; i < 100; i++) if (f.Walk(0.1f)) steps++;      // 10 units walked
            Assert.AreEqual((int)(10f / Footsteps.Stride), steps);
            var a = f.Pitch;
            f.Walk(Footsteps.Stride);
            Assert.AreNotEqual(a, f.Pitch);
            f.Walk(Footsteps.Stride * 0.5f);
            Assert.IsFalse(f.Walk(0f), "stopping resets the stride");
            Assert.IsFalse(f.Walk(Footsteps.Stride * 0.6f), "the half stride before the stop is forgotten");
        }

        [Test]
        public void TheGround_DecidesWhatAStepSoundsLike()
        {
            foreach (var hard in new[] { "tile_path", "tile_cobble", "tile_floor_wood" }) Assert.AreEqual(Sfx.StepHard, FootstepSurface.For(hard), hard);
            Assert.AreEqual(Sfx.StepSand, FootstepSurface.For("tile_sand"));
            foreach (var soft in new[] { "tile_grass", "tile_dirt", "tile_forest", "tile_tilled", null, "something_new" }) Assert.AreEqual(Sfx.Step, FootstepSurface.For(soft), soft);
        }

        [Test]
        public void WeatherOneShots_OnlyOutdoors_StormsRumbleAndWindGusts()
        {
            Assert.AreEqual(Sfx.Thunder, AmbienceDirector.OneShotFor(WeatherIds.Storm, false));
            Assert.AreEqual(Sfx.Gust, AmbienceDirector.OneShotFor(WeatherIds.Wind, false));
            Assert.IsNull(AmbienceDirector.OneShotFor(WeatherIds.Rain, false), "plain rain has no thunder");
            Assert.IsNull(AmbienceDirector.OneShotFor(WeatherIds.Sunny, false));
            Assert.IsNull(AmbienceDirector.OneShotFor(WeatherIds.Storm, true), "indoors the storm is a distant bed, no thunder clap");
        }

        [Test]
        public void TheVillageBell_StrikesAtNoonAndSix()
        {
            Assert.IsTrue(AmbienceDirector.BellHour(12));
            Assert.IsTrue(AmbienceDirector.BellHour(18));
            foreach (var h in new[] { 0, 6, 9, 11, 13, 17, 19, 23 }) Assert.IsFalse(AmbienceDirector.BellHour(h), h.ToString());
        }

        [Test]
        public void EveryFarmAnimal_HasACall_ExceptTheRabbit()
        {
            foreach (var type in new[] { "chicken", "duck", "cow", "goat", "sheep" }) Assert.IsTrue(AnimalCalls.For(type).HasValue, type);
            Assert.IsFalse(AnimalCalls.For("rabbit").HasValue);
            Assert.IsFalse(AnimalCalls.For(null).HasValue);
            foreach (var row in AnimalDefaults.Rows) if (row.Id != "rabbit") Assert.IsTrue(AnimalCalls.For(row.Id).HasValue, row.Id);
        }
    }
}
