using System;

namespace Farm.Gameplay
{
    public enum AmbienceKind { None = 0, Rain, Wind, Birds, Crickets }

    // Seamless looping ambience beds, synthesised: rain, wind, daytime birds and night crickets. Each loop is rendered a little longer and
    // cross-faded into its own start, so there is no click where it repeats. Deterministic.
    public static class AmbienceSynth
    {
        public const int SampleRate = 22050;

        public static float[] Make(AmbienceKind kind)
        {
            switch (kind)
            {
                case AmbienceKind.Rain: return Loop(3.0f, Rain);
                case AmbienceKind.Wind: return Loop(4.0f, Wind);
                case AmbienceKind.Birds: return Loop(7.0f, Birds);
                case AmbienceKind.Crickets: return Loop(4.0f, Crickets);
                default: return new float[SampleRate / 10];
            }
        }

        // Renders `seconds + crossfade` and blends the extra tail into the head so the end flows into the start.
        static float[] Loop(float seconds, Func<float, float[]> render)
        {
            const float cross = 0.25f;
            var data = render(seconds + cross);
            var len = (int)(seconds * SampleRate);
            var x = (int)(cross * SampleRate);
            var result = new float[len];
            Array.Copy(data, result, len);
            for (var i = 0; i < x; i++)
            {
                var w = i / (float)x;
                result[i] = data[i] * w + data[len + i] * (1f - w);
            }
            var max = 0f;
            foreach (var s in result) max = Math.Max(max, Math.Abs(s));
            var scale = max > 0f ? 0.5f / max : 1f;
            for (var i = 0; i < result.Length; i++) result[i] = Math.Max(-1f, Math.Min(1f, result[i] * scale));
            return result;
        }

        sealed class Noise
        {
            uint _s;
            public Noise(uint seed) { _s = seed == 0 ? 1u : seed; }
            public float Next() { _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5; return (_s / (float)uint.MaxValue) * 2f - 1f; }
        }

        static float[] Rain(float seconds)
        {
            var data = new float[(int)(seconds * SampleRate)];
            var n = new Noise(101);
            float lp = 0, lp2 = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var white = n.Next();
                lp += 0.35f * (white - lp);               // the hiss
                lp2 += 0.04f * (white - lp2);             // the low body
                data[i] = lp * 0.5f + lp2 * 0.8f;
            }
            var d = new Noise(102);
            for (var k = 0; k < (int)(seconds * 14); k++)      // a few drops
            {
                var at = (int)((d.Next() * 0.5f + 0.5f) * (data.Length - 800));
                var hz = 1800f + (d.Next() * 0.5f + 0.5f) * 1800f;
                for (var j = 0; j < 500; j++) data[at + j] += (float)(Math.Sin(2 * Math.PI * hz * j / SampleRate) * Math.Exp(-j / 80.0)) * 0.12f;
            }
            return data;
        }

        static float[] Wind(float seconds)
        {
            var data = new float[(int)(seconds * SampleRate)];
            var n = new Noise(201);
            float lp = 0, lp2 = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (float)SampleRate;
                var gust = 0.55f + 0.45f * (float)(Math.Sin(2 * Math.PI * t / seconds) * Math.Sin(2 * Math.PI * 2 * t / seconds + 1f));
                lp += 0.02f * (n.Next() - lp);
                lp2 += 0.09f * (lp - lp2);
                data[i] = lp2 * gust * 6f;
            }
            return data;
        }

        static float[] Birds(float seconds)
        {
            var data = new float[(int)(seconds * SampleRate)];
            var r = new Noise(301);
            var chirps = new[] { 0.3f, 0.45f, 0.6f, 2.1f, 2.22f, 3.6f, 3.75f, 3.9f, 4.05f, 5.4f, 6.1f };
            foreach (var c in chirps)
            {
                var at = (int)(c * SampleRate);
                var from = 2400f + (r.Next() * 0.5f + 0.5f) * 1400f;
                var to = from + (r.Next() > 0 ? 1 : -1) * 900f;
                var n = (int)(0.09f * SampleRate);
                var phase = 0.0;
                for (var i = 0; i < n && at + i < data.Length; i++)
                {
                    var p = i / (float)n;
                    phase += 2 * Math.PI * (from + (to - from) * p) / SampleRate;
                    data[at + i] += (float)Math.Sin(phase) * (float)Math.Sin(Math.PI * p) * 0.5f;
                }
            }
            return data;
        }

        static float[] Crickets(float seconds)
        {
            var data = new float[(int)(seconds * SampleRate)];
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (float)SampleRate;
                var gate = Math.Sin(2 * Math.PI * 2.5 * t) > 0.2 ? 1f : 0f;                       // chirps in groups
                var pulse = 0.5f + 0.5f * (float)Math.Sin(2 * Math.PI * 38 * t);
                var pulse2 = 0.5f + 0.5f * (float)Math.Sin(2 * Math.PI * 31 * t + 1.3);
                data[i] = (float)Math.Sin(2 * Math.PI * 4300 * t) * pulse * gate * 0.4f + (float)Math.Sin(2 * Math.PI * 3700 * t) * pulse2 * (1f - gate) * 0.15f;
            }
            return data;
        }
    }
}
