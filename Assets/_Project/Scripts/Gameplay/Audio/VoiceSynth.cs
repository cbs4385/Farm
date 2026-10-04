using System;

namespace Farm.Gameplay
{
    public enum VoiceWave { Sine, Triangle, Square, Saw }

    // How a villager sounds when they talk (T-132): a short blip, played every few characters as their line types out, in a pitch
    // and tone that is theirs. No recorded voice: this is the Animal Crossing and Stardew way, with original sounds.
    public readonly struct VoiceProfile
    {
        public readonly float Pitch;          // Hz
        public readonly VoiceWave Wave;
        public readonly float Milliseconds;   // length of one blip
        public readonly int CharsPerBlip;     // one blip per this many revealed characters
        public readonly float Wobble;         // 0..1: how much each blip drifts in pitch
        public VoiceProfile(float pitch, VoiceWave wave, float milliseconds, int charsPerBlip, float wobble)
        { Pitch = pitch; Wave = wave; Milliseconds = milliseconds; CharsPerBlip = charsPerBlip; Wobble = wobble; }
    }

    public static class VoiceProfiles
    {
        // The slice villagers are hand-tuned; everyone else gets a stable profile from their id, so no two sound the same by accident.
        public static VoiceProfile For(string voiceId)
        {
            switch (voiceId)
            {
                case "wren": return new VoiceProfile(262f, VoiceWave.Triangle, 70f, 2, 0.35f);     // warm, mid, lively
                case "hazel": return new VoiceProfile(392f, VoiceWave.Sine, 55f, 3, 0.10f);        // soft, high, hushed
                case "bram": return new VoiceProfile(110f, VoiceWave.Square, 90f, 4, 0.05f);       // low, flat, sparing
                case "tilda": return new VoiceProfile(330f, VoiceWave.Sine, 75f, 3, 0.08f);        // warm, even, a shopkeeper's bell
                case "juno": return new VoiceProfile(294f, VoiceWave.Saw, 45f, 2, 0.30f);          // quick, bright, tapping
                case "piper": return new VoiceProfile(440f, VoiceWave.Triangle, 60f, 2, 0.45f);    // musical, sliding
                case "marcus": return new VoiceProfile(196f, VoiceWave.Triangle, 85f, 3, 0.06f);   // low, steady, a plane on pine
                case "odalys": return new VoiceProfile(370f, VoiceWave.Sine, 50f, 4, 0.04f);       // clear, formal, exact
                case "felix": return new VoiceProfile(147f, VoiceWave.Saw, 95f, 4, 0.12f);         // slow, salty, low
                case "dorian": return new VoiceProfile(554f, VoiceWave.Sine, 38f, 5, 0.05f);       // hushed, high, sparing
                case "elara": return new VoiceProfile(494f, VoiceWave.Triangle, 52f, 2, 0.33f);    // bouncy, bright
                case "ione": return new VoiceProfile(311f, VoiceWave.Sine, 72f, 3, 0.07f);         // gentle, even
            }
            var h = (uint)Math.Abs(NpcInteractions.StableHash(voiceId ?? "voice"));
            var wave = (VoiceWave)(h % 4);
            return new VoiceProfile(130f + (h >> 3) % 260, wave, 55f + (h >> 7) % 40, 2 + (int)((h >> 11) % 3), 0.1f + ((h >> 13) % 30) / 100f);
        }
    }

    public static class VoiceSynth
    {
        public const int SampleRate = 22050;

        // The samples of one blip. `variant` shifts the pitch a little so a line does not drone; it is deterministic.
        public static float[] Blip(VoiceProfile p, int variant)
        {
            var count = (int)(SampleRate * p.Milliseconds / 1000f);
            var data = new float[count];
            var drift = 1f + p.Wobble * 0.12f * (((variant * 37) % 7) - 3) / 3f;
            var freq = p.Pitch * drift;
            var phase = 0.0;
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)count;
                var envelope = Math.Min(1f, t * 14f) * (1f - t) * (1f - t);        // a soft attack and a quick fade
                phase += freq / SampleRate;
                data[i] = Wave(p.Wave, phase) * envelope * 0.35f;
            }
            return data;
        }

        static float Wave(VoiceWave wave, double phase)
        {
            var x = (float)(phase - Math.Floor(phase));
            switch (wave)
            {
                case VoiceWave.Triangle: return 4f * Math.Abs(x - 0.5f) - 1f;
                case VoiceWave.Square: return x < 0.5f ? 0.7f : -0.7f;
                case VoiceWave.Saw: return 2f * x - 1f;
                default: return (float)Math.Sin(2 * Math.PI * x);
            }
        }

        // A villager's signature sound: a few hundred milliseconds that says who just spoke (or arrived).
        public static float[] Signature(string npcId)
        {
            switch (npcId)
            {
                case "wren": return Pings(new[] { 1760f, 2349f }, new[] { 0f, 0.09f }, 0.55f, 5f);              // a glass clink
                case "bram": return Pings(new[] { 988f, 1480f, 2250f }, new[] { 0f, 0f, 0f }, 0.8f, 3.5f);       // an anvil ring
                case "hazel": return Rustle();                                                                   // a page turning
                case "tilda": return Pings(new[] { 1568f, 2093f }, new[] { 0f, 0.14f }, 0.7f, 4.5f);             // the shop bell, two notes
                case "juno": return Pings(new[] { 1245f, 1245f, 1245f }, new[] { 0f, 0.1f, 0.2f }, 0.55f, 16f);  // three quick hammer taps
                case "piper": return Slide(520f, 1170f, 0.45f);                                                  // a rising fiddle slide
                case "marcus": return Pings(new[] { 820f, 820f }, new[] { 0f, 0.18f }, 0.4f, 20f);               // two pencil taps
                case "odalys": return Pings(new[] { 262f, 524f }, new[] { 0f, 0.12f }, 0.35f, 22f);              // a soft stethoscope tap
                case "felix": return Slide(740f, 210f, 0.3f);                                                    // a bobber plop
                case "dorian": return Snap();                                                                    // a twig snap
                case "elara": return Pings(new[] { 2470f, 3135f }, new[] { 0f, 0.13f }, 0.3f, 45f);             // two needle clicks
                case "ione": return Pings(new[] { 2093f, 3136f }, new[] { 0f, 0f }, 0.9f, 3.2f);                // a small desk bell
            }
            var h = (uint)Math.Abs(NpcInteractions.StableHash(npcId ?? "npc"));
            var root = 330f + h % 330;
            return Pings(new[] { root, root * 1.25f, root * 1.5f }, new[] { 0f, 0.08f, 0.16f }, 0.6f, 5f);
        }

        // Decaying sine partials, each starting at its own offset (seconds).
        static float[] Pings(float[] freqs, float[] starts, float seconds, float decay)
        {
            var count = (int)(SampleRate * seconds);
            var data = new float[count];
            for (var k = 0; k < freqs.Length; k++)
            {
                var from = (int)(starts[k] * SampleRate);
                for (var i = from; i < count; i++)
                {
                    var t = (i - from) / (float)SampleRate;
                    var amp = (float)Math.Exp(-decay * t) * (0.5f / freqs.Length + 0.1f);
                    data[i] += (float)Math.Sin(2 * Math.PI * freqs[k] * t) * amp;
                }
            }
            for (var i = 0; i < count; i++) data[i] = Math.Max(-1f, Math.Min(1f, data[i]));
            return data;
        }

        // A pitch that glides from `from` to `to` Hz with a little vibrato at the top, like a fiddle slide.
        static float[] Slide(float from, float to, float seconds)
        {
            var count = (int)(SampleRate * seconds);
            var data = new float[count];
            var phase = 0.0;
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)count;
                var freq = from + (to - from) * t * t + (t > 0.7f ? 8f * (float)Math.Sin(i * 0.05) : 0f);
                phase += freq / SampleRate;
                var env = Math.Min(1f, t * 12f) * (1f - t * 0.6f) * (t > 0.9f ? (1f - t) * 10f : 1f);
                var saw = 2f * (float)(phase - Math.Floor(phase)) - 1f;
                data[i] = (saw * 0.5f + (float)Math.Sin(2 * Math.PI * phase) * 0.5f) * env * 0.35f;
            }
            return data;
        }

        // A short dry crack: a burst of noise that dies away almost at once.
        static float[] Snap()
        {
            var count = (int)(SampleRate * 0.14f);
            var data = new float[count];
            uint state = 88172645u;
            for (var i = 0; i < count; i++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                var noise = (state / (float)uint.MaxValue) * 2f - 1f;
                data[i] = noise * (float)Math.Exp(-38f * i / SampleRate) * 0.8f;
            }
            return data;
        }

        // Soft noise with a slow swell, like a page turning (a deterministic generator, so the clip is always the same).
        static float[] Rustle()
        {
            var count = (int)(SampleRate * 0.35f);
            var data = new float[count];
            uint state = 2463534242u;
            var last = 0f;
            for (var i = 0; i < count; i++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                var noise = (state / (float)uint.MaxValue) * 2f - 1f;
                last = last * 0.7f + noise * 0.3f;                                // a little low-pass: papery, not harsh
                var t = i / (float)count;
                data[i] = last * (float)Math.Sin(Math.PI * t) * 0.6f;
            }
            return data;
        }

        // True when a line that went from `before` to `now` visible characters should blip.
        public static bool ShouldBlip(int before, int now, int charsPerBlip) =>
            now > before && charsPerBlip > 0 && (now / charsPerBlip) > (before / charsPerBlip);
    }
}
