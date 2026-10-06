using System;
using Farm.Core;
using Farm.Gameplay;

namespace Farm.Mythos
{
    // The layer's sound (X-008): quiet beds laid over the weather and time ambience through the generic ambience-layer hook of AudioService.
    // The sleeping god is felt as a low drone that grows with the wakefulness step, sparse dissonant tones from step 10, and in the wood a
    // heartbeat that follows dread. Mild plays everything at half volume; off plays nothing. Everything is synthesised and deterministic.
    public static class MythosSoundscape
    {
        public const string DroneId = "mythos.drone";
        public const string TonesId = "mythos.tones";
        public const string HeartId = "mythos.heart";

        const int Rate = AudioService.LayerSampleRate;

        public readonly struct Plan
        {
            public readonly float Drone, Tones, Heart;
            public Plan(float drone, float tones, float heart) { Drone = drone; Tones = tones; Heart = heart; }
            public bool Silent => Drone <= 0f && Tones <= 0f && Heart <= 0f;
        }

        // What should be heard. step 0..20, dread 0..100, level 0..2. (pure)
        public static Plan PlanFor(int step, int dread, int level, bool inWoods)
        {
            var scale = level <= 0 ? 0f : level == 1 ? 0.5f : 1f;
            if (scale <= 0f) return new Plan(0f, 0f, 0f);
            var drone = step < 5 ? 0f : step < 10 ? 0.15f : step < 15 ? 0.3f : step < 20 ? 0.45f : 0.6f;
            var tones = step < 10 ? 0f : step < 15 ? 0.2f : 0.35f;
            var heart = inWoods && dread >= 40 ? 0.2f + 0.4f * (Math.Min(100, dread) - 40) / 60f : 0f;
            return new Plan(drone * scale, tones * scale, heart * scale);
        }

        public static void Apply(GameSession s)
        {
            if (s == null || !s.InGame) return;
            var step = s.GetVar(MythosIds2.Step);
            var plan = PlanFor(step, s.GetVar(MythosIds.Vars.Dread), MythosLevel.Of(s), s.State.CurrentMap == MapIds.Woods);
            AudioService.SetLayerIfAvailable(DroneId, Drone, plan.Drone);
            AudioService.SetLayerIfAvailable(TonesId, Tones, plan.Tones);
            AudioService.SetLayerIfAvailable(HeartId, Heart, plan.Heart);
        }

        // ---- the loops: each seamless (whole cycles, windowed events, or modulo wrap) -----------------------------------------------------

        // 6 s of two close low tones beating against each other, swelling slowly. Every frequency is a whole number of cycles per loop.
        public static float[] Drone()
        {
            const float seconds = 6f;
            var data = new float[(int)(seconds * Rate)];
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (float)Rate;
                var swell = 0.65f + 0.35f * (float)Math.Sin(2 * Math.PI * t / seconds);
                var v = Math.Sin(2 * Math.PI * 54 * t) + 0.8 * Math.Sin(2 * Math.PI * 57 * t) + 0.5 * Math.Sin(2 * Math.PI * 81 * t + 0.7)
                      + 0.25 * Math.Sin(2 * Math.PI * 108 * t + 1.9);
                data[i] = (float)v * swell;
            }
            return Normalise(data, 0.6f);
        }

        // 14 s, mostly silence: three slow, fading pairs of tones a tritone apart.
        public static float[] Tones()
        {
            const float seconds = 14f;
            var data = new float[(int)(seconds * Rate)];
            var events = new[] { (at: 1.5f, a: 311f, b: 440f), (at: 6.0f, a: 233f, b: 330f), (at: 10.0f, a: 392f, b: 554f) };
            foreach (var e in events)
            {
                var start = (int)(e.at * Rate);
                var n = (int)(3.2f * Rate);
                for (var i = 0; i < n && start + i < data.Length; i++)
                {
                    var p = i / (float)n;
                    var window = (float)Math.Pow(Math.Sin(Math.PI * p), 2);
                    var t = i / (float)Rate;
                    data[start + i] += (float)(Math.Sin(2 * Math.PI * e.a * t) + 0.8 * Math.Sin(2 * Math.PI * e.b * t)) * window;
                }
            }
            return Normalise(data, 0.5f);
        }

        // 4 s, five slow beats of a double thump (low and soft), wrapping around the end of the loop.
        public static float[] Heart()
        {
            const float seconds = 4f;
            var data = new float[(int)(seconds * Rate)];
            for (var beat = 0; beat < 5; beat++)
            {
                AddThump(data, beat * 0.8f, 1f);
                AddThump(data, beat * 0.8f + 0.26f, 0.6f);
            }
            return Normalise(data, 0.7f);
        }

        static void AddThump(float[] data, float at, float gain)
        {
            var start = (int)(at * Rate);
            var n = (int)(0.3f * Rate);
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)Rate;
                var v = (float)(Math.Sin(2 * Math.PI * 52 * t) * Math.Exp(-t / 0.07) * gain);
                data[(start + i) % data.Length] += v;
            }
        }

        static float[] Normalise(float[] d, float peak)
        {
            var max = 0f;
            foreach (var x in d) max = Math.Max(max, Math.Abs(x));
            var k = max > 0f ? peak / max : 1f;
            for (var i = 0; i < d.Length; i++) d[i] *= k;
            return d;
        }
    }
}
