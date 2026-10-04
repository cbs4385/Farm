using System;

namespace Farm.Gameplay
{
    // Original, procedurally synthesised sound effects (no recorded or sampled audio). Each effect is a small recipe of notes, glides
    // and filtered noise, rendered to mono floats in [-1, 1]. Deterministic: the same effect always gives the same samples.
    public static class SfxSynth
    {
        public const int SampleRate = 22050;

        // The effects this synth makes (the first ten Sfx values are the simple tones in AudioService).
        public static bool Makes(Sfx sfx) => (int)sfx >= (int)Sfx.Shutter;

        public static float[] Make(Sfx sfx)
        {
            switch (sfx)
            {
                case Sfx.Shutter: return Shutter();
                case Sfx.PageTurn: return PageTurn();
                case Sfx.Letter: return Bells(0.55f, 0.16f, 1047f, 1319f);
                case Sfx.QuestDone: return Bells(0.9f, 0.13f, 523f, 659f, 784f, 1047f);
                case Sfx.LevelUp: return LevelUp();
                case Sfx.Door: return Door();
                case Sfx.Pickup: return Glide(520f, 1040f, 0.09f, 0.5f);
                case Sfx.Heart: return Heart();
                case Sfx.Gift: return Bells(0.6f, 0.1f, 880f, 1175f, 1760f);
                case Sfx.Cast: return Whoosh(0.3f, 2000f, 400f, 0.45f, 41);
                case Sfx.Splash: return Splash();
                case Sfx.Bite: return Bites();
                case Sfx.SwordSwing: return Whoosh(0.13f, 1500f, 6500f, 0.55f, 77);
                case Sfx.Hit: return Hit();
                case Sfx.ChestOpen: return ChestOpen();
                case Sfx.Rooster: return Rooster();
                case Sfx.Sleep: return Bells(1.0f, 0.3f, 659f, 523f, 392f);
                case Sfx.Lantern: return Lantern();
                case Sfx.Step: return Step();
                case Sfx.Cluck: return Cluck();
                case Sfx.Moo: return Moo();
                case Sfx.Baa: return Baa();
                case Sfx.Quack: return Quack();
                case Sfx.Hover: return Glide(1500f, 1700f, 0.025f, 0.4f);
                default: return new float[SampleRate / 20];
            }
        }

        // ---- building blocks ----------------------------------------------------------------------------------------

        static float[] Buffer(float seconds) => new float[Math.Max(1, (int)(SampleRate * seconds))];

        // A deterministic white-noise source (xorshift), so effects are repeatable.
        sealed class Noise
        {
            uint _s;
            public Noise(uint seed) { _s = seed == 0 ? 1u : seed; }
            public float Next()
            {
                _s ^= _s << 13; _s ^= _s >> 17; _s ^= _s << 5;
                return (_s / (float)uint.MaxValue) * 2f - 1f;
            }
        }

        // Attack and exponential decay, in seconds.
        static float Env(float t, float attack, float decay)
        {
            var a = attack <= 0f ? 1f : Math.Min(1f, t / attack);
            return a * (float)Math.Exp(-t / Math.Max(0.001f, decay));
        }

        // A bell-like note: a sine with a soft second partial that fades faster.
        static void AddBell(float[] data, float start, float freq, float decay, float gain)
        {
            var begin = (int)(start * SampleRate);
            for (var i = Math.Max(0, begin); i < data.Length; i++)
            {
                var t = (i - begin) / (float)SampleRate;
                var e = Env(t, 0.004f, decay);
                if (t > 0.01f && e < 0.001f) break;      // not during the attack, when the envelope is still near zero
                var w = Math.Sin(2 * Math.PI * freq * t) + 0.35 * Math.Sin(2 * Math.PI * freq * 2.01 * t) * Math.Exp(-t / (decay * 0.5));
                data[i] += (float)w * e * gain;
            }
        }

        // A burst of noise through a one-pole low-pass whose cutoff slides, scaled by an envelope.
        static void AddNoiseSweep(float[] data, float start, float seconds, float fromHz, float toHz, float gain, uint seed, float attack = 0.004f)
        {
            var noise = new Noise(seed);
            var begin = (int)(start * SampleRate);
            var count = (int)(seconds * SampleRate);
            var y = 0f;
            for (var k = 0; k < count && begin + k < data.Length; k++)
            {
                var p = k / (float)count;
                var hz = fromHz + (toHz - fromHz) * p;
                var a = (float)(1 - Math.Exp(-2 * Math.PI * hz / SampleRate));
                y += a * (noise.Next() - y);
                var t = k / (float)SampleRate;
                var e = Math.Min(1f, t / attack) * (1f - p) * (1f - p);
                data[begin + k] += y * e * gain;
            }
        }

        static void AddTone(float[] data, float start, float seconds, float fromHz, float toHz, float gain, bool square = false)
        {
            var begin = (int)(start * SampleRate);
            var count = (int)(seconds * SampleRate);
            var phase = 0.0;
            for (var k = 0; k < count && begin + k < data.Length; k++)
            {
                var p = k / (float)count;
                phase += 2 * Math.PI * (fromHz + (toHz - fromHz) * p) / SampleRate;
                var w = square ? (Math.Sin(phase) >= 0 ? 0.6 : -0.6) : Math.Sin(phase);
                var e = Math.Min(1f, k / (SampleRate * 0.003f)) * (1f - p);
                data[begin + k] += (float)w * e * gain;
            }
        }

        // Scales to a safe peak and fades the last few milliseconds, so nothing clicks or clips.
        static float[] Finish(float[] data, float peak = 0.6f)
        {
            var max = 0f;
            for (var i = 0; i < data.Length; i++) max = Math.Max(max, Math.Abs(data[i]));
            var scale = max > 0f ? peak / max : 1f;
            var fade = Math.Min(data.Length, (int)(SampleRate * 0.012f));
            for (var i = 0; i < data.Length; i++)
            {
                var tail = data.Length - i;
                var f = tail < fade ? tail / (float)fade : 1f;
                data[i] = Math.Max(-1f, Math.Min(1f, data[i] * scale * f));
            }
            return data;
        }

        // ---- the effects ----------------------------------------------------------------------------------------------

        static float[] Bells(float seconds, float gap, params float[] notes)
        {
            var data = Buffer(seconds);
            for (var i = 0; i < notes.Length; i++) AddBell(data, i * gap, notes[i], 0.16f, 0.7f / (1 + i * 0.1f));
            return Finish(data);
        }

        static float[] Glide(float from, float to, float seconds, float gain)
        {
            var data = Buffer(seconds);
            AddTone(data, 0f, seconds, from, to, gain);
            return Finish(data);
        }

        static float[] Shutter()
        {
            var data = Buffer(0.22f);
            AddNoiseSweep(data, 0f, 0.025f, 9000f, 6000f, 1.0f, 11, 0.001f);
            AddTone(data, 0.005f, 0.06f, 180f, 110f, 0.5f);
            AddNoiseSweep(data, 0.085f, 0.03f, 7000f, 4000f, 0.8f, 12, 0.001f);
            AddTone(data, 0.09f, 0.08f, 150f, 90f, 0.4f);
            return Finish(data);
        }

        static float[] PageTurn()
        {
            var data = Buffer(0.22f);
            AddNoiseSweep(data, 0f, 0.2f, 1400f, 5200f, 0.7f, 21, 0.03f);
            return Finish(data, 0.4f);
        }

        static float[] LevelUp()
        {
            var data = Buffer(1.1f);
            var notes = new[] { 392f, 494f, 587f, 784f, 988f };
            for (var i = 0; i < notes.Length; i++) AddBell(data, i * 0.09f, notes[i], 0.22f, 0.7f);
            AddBell(data, 0.5f, 1568f, 0.3f, 0.35f);
            AddBell(data, 0.56f, 1976f, 0.3f, 0.25f);
            return Finish(data);
        }

        static float[] Door()
        {
            var data = Buffer(0.4f);
            AddTone(data, 0f, 0.3f, 150f, 105f, 0.45f, square: true);
            AddNoiseSweep(data, 0.28f, 0.1f, 900f, 300f, 0.9f, 31, 0.002f);
            AddTone(data, 0.28f, 0.1f, 90f, 55f, 0.6f);
            return Finish(data, 0.55f);
        }

        static float[] Heart()
        {
            var data = Buffer(0.6f);
            AddTone(data, 0f, 0.12f, 230f, 140f, 0.8f);
            AddTone(data, 0.16f, 0.14f, 250f, 150f, 0.7f);
            AddBell(data, 0.28f, 784f, 0.18f, 0.5f);
            return Finish(data);
        }

        static float[] Whoosh(float seconds, float fromHz, float toHz, float gain, uint seed)
        {
            var data = Buffer(seconds);
            AddNoiseSweep(data, 0f, seconds, fromHz, toHz, gain, seed, 0.02f);
            if (fromHz > toHz) AddTone(data, seconds * 0.8f, 0.05f, 700f, 1000f, 0.25f);      // a cast ends with a small "plip" on the water
            return Finish(data, 0.45f);
        }

        static float[] Splash()
        {
            var data = Buffer(0.45f);
            AddNoiseSweep(data, 0f, 0.28f, 5000f, 700f, 1.0f, 51, 0.002f);
            AddTone(data, 0.05f, 0.07f, 500f, 900f, 0.3f);
            AddTone(data, 0.14f, 0.06f, 420f, 800f, 0.22f);
            AddTone(data, 0.22f, 0.05f, 600f, 1100f, 0.16f);
            return Finish(data, 0.55f);
        }

        static float[] Bites()
        {
            var data = Buffer(0.3f);
            AddTone(data, 0f, 0.05f, 950f, 900f, 0.6f);
            AddTone(data, 0.1f, 0.05f, 950f, 900f, 0.6f);
            AddTone(data, 0.2f, 0.07f, 1150f, 1100f, 0.7f);
            return Finish(data, 0.55f);
        }

        static float[] Hit()
        {
            var data = Buffer(0.2f);
            AddTone(data, 0f, 0.14f, 140f, 55f, 0.9f);
            AddNoiseSweep(data, 0f, 0.05f, 4000f, 800f, 0.8f, 61, 0.001f);
            return Finish(data, 0.7f);
        }

        static float[] ChestOpen()
        {
            var data = Buffer(0.6f);
            AddTone(data, 0f, 0.25f, 200f, 330f, 0.4f, square: true);
            AddBell(data, 0.22f, 784f, 0.2f, 0.5f);
            AddBell(data, 0.3f, 1175f, 0.22f, 0.4f);
            return Finish(data, 0.55f);
        }

        // A cheerful, plainly synthetic "cock-a-doodle": four rising and falling slides. Not a recording of anything.
        static float[] Rooster()
        {
            var data = Buffer(0.9f);
            AddTone(data, 0.0f, 0.12f, 420f, 640f, 0.5f, square: true);
            AddTone(data, 0.14f, 0.12f, 520f, 780f, 0.5f, square: true);
            AddTone(data, 0.28f, 0.14f, 600f, 880f, 0.5f, square: true);
            AddTone(data, 0.46f, 0.38f, 900f, 520f, 0.55f, square: true);
            return Finish(data, 0.45f);
        }

        static float[] Lantern()
        {
            var data = Buffer(1.1f);
            var n = (int)(SampleRate * 1.0f);
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)SampleRate;
                var e = (float)(Math.Sin(Math.PI * Math.Min(1f, t / 1.0f)));
                data[i] += (float)(Math.Sin(2 * Math.PI * 392 * t) * 0.5 + Math.Sin(2 * Math.PI * 587 * t) * 0.4) * e;
            }
            AddBell(data, 0.62f, 1568f, 0.25f, 0.35f);
            return Finish(data, 0.45f);
        }

        // A soft footstep: a low thud and a short, dull rustle of noise.
        static float[] Step()
        {
            var data = Buffer(0.09f);
            AddTone(data, 0f, 0.07f, 120f, 70f, 0.8f);
            AddNoiseSweep(data, 0f, 0.05f, 1400f, 500f, 0.7f, 91, 0.002f);
            return Finish(data, 0.5f);
        }

        // Cartoon animal calls. They are plain synthetic voices (glides, buzz and vibrato), not imitations of any recording.
        static float[] Cluck()
        {
            var data = Buffer(0.4f);
            AddTone(data, 0.00f, 0.07f, 900f, 620f, 0.5f, square: true);
            AddTone(data, 0.10f, 0.07f, 860f, 600f, 0.5f, square: true);
            AddTone(data, 0.20f, 0.10f, 760f, 480f, 0.55f, square: true);
            return Finish(data, 0.4f);
        }

        static float[] Moo()
        {
            var data = Buffer(0.9f);
            var phase = 0.0;
            var n = (int)(SampleRate * 0.8f);
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)SampleRate;
                var p = i / (float)n;
                var hz = (150f - 40f * p) * (1f + 0.03f * Math.Sin(2 * Math.PI * 5 * t));
                phase += 2 * Math.PI * hz / SampleRate;
                var saw = (float)((phase / Math.PI) % 2.0 - 1.0);
                var env = Math.Min(1f, t / 0.08f) * (float)Math.Sin(Math.PI * Math.Min(1f, p));
                data[i] = saw * env * 0.5f + (float)Math.Sin(phase * 2) * env * 0.2f;
            }
            return Finish(data, 0.5f);
        }

        static float[] Baa()
        {
            var data = Buffer(0.55f);
            var phase = 0.0;
            var n = (int)(SampleRate * 0.5f);
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)SampleRate;
                var p = i / (float)n;
                phase += 2 * Math.PI * (330f - 40f * p) / SampleRate;
                var buzz = (float)(0.6 + 0.4 * Math.Sin(2 * Math.PI * 32 * t));          // the wobble that makes it a bleat
                var saw = (float)((phase / Math.PI) % 2.0 - 1.0);
                var env = Math.Min(1f, t / 0.03f) * (1f - p * p);
                data[i] = saw * buzz * env * 0.6f;
            }
            return Finish(data, 0.4f);
        }

        static float[] Quack()
        {
            var data = Buffer(0.4f);
            for (var k = 0; k < 2; k++)
            {
                var phase = 0.0;
                var n = (int)(SampleRate * 0.12f);
                var begin = (int)(k * 0.17f * SampleRate);
                for (var i = 0; i < n && begin + i < data.Length; i++)
                {
                    var p = i / (float)n;
                    phase += 2 * Math.PI * (520f - 170f * p) / SampleRate;
                    var saw = (float)((phase / Math.PI) % 2.0 - 1.0);
                    var env = Math.Min(1f, i / (SampleRate * 0.005f)) * (1f - p);
                    data[begin + i] += saw * env * 0.6f;
                }
            }
            return Finish(data, 0.4f);
        }

        // ---- export for review ----------------------------------------------------------------------------------------------------------------------------

        // 16-bit mono WAV bytes, so a person can listen to every effect (the editor test writes them to Builds/sfx).
        public static byte[] ToWav(float[] samples)
        {
            var bytes = new byte[44 + samples.Length * 2];
            void Text(int at, string s) { for (var i = 0; i < s.Length; i++) bytes[at + i] = (byte)s[i]; }
            void Int(int at, int v) { bytes[at] = (byte)v; bytes[at + 1] = (byte)(v >> 8); bytes[at + 2] = (byte)(v >> 16); bytes[at + 3] = (byte)(v >> 24); }
            Text(0, "RIFF"); Int(4, 36 + samples.Length * 2); Text(8, "WAVE"); Text(12, "fmt "); Int(16, 16);
            bytes[20] = 1; bytes[22] = 1; Int(24, SampleRate); Int(28, SampleRate * 2); bytes[32] = 2; bytes[34] = 16;
            Text(36, "data"); Int(40, samples.Length * 2);
            for (var i = 0; i < samples.Length; i++)
            {
                var v = (short)Math.Round(Math.Max(-1f, Math.Min(1f, samples[i])) * 32767f);
                bytes[44 + i * 2] = (byte)v; bytes[45 + i * 2] = (byte)(v >> 8);
            }
            return bytes;
        }
    }
}
