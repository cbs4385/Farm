using System;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // New effects are only ever appended: the order is the index into the clip table.
    public enum Sfx
    {
        Click, Hoe, Water, Plant, Harvest, Coin, Error, Clink, Rustle, Anvil,
        Shutter, PageTurn, Letter, QuestDone, LevelUp, Door, Pickup, Heart, Gift, Cast, Splash, Bite, SwordSwing, Hit, ChestOpen, Rooster, Sleep, Lantern,
        Step, Cluck, Moo, Baa, Quack, Hover,
        StepHard, StepSand, Thunder, Gust, Meow, Bell, Knock,
    }

    // M1 audio: logical buses (master/music/sfx/ambience) implemented as volume multipliers, plus procedurally
    // generated placeholder blips so actions have feedback before real audio exists. A real AudioMixer with
    // groups and snapshots replaces the buses when final audio arrives (T-061).
    public sealed class AudioService : MonoBehaviour
    {
        const int SfxVoices = 6;

        AudioSource[] _voices;
        AudioSource _music;
        AudioClip[] _clips;
        int _next;
        float _sfxVolume = 0.8f;
        float _musicVolume = 0.7f;
        float _ambienceVolume = 0.7f;

        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;

        void Awake()
        {
            _voices = new AudioSource[SfxVoices];
            for (var i = 0; i < SfxVoices; i++)
            {
                _voices[i] = gameObject.AddComponent<AudioSource>();
                _voices[i].playOnAwake = false;
                _voices[i].spatialBlend = 0f;
            }
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;

            _clips = BuildClips();
        }

        // One clip for every Sfx value, in enum order: the first ten are simple tones, the rest come from SfxSynth.
        public static AudioClip[] BuildClips()
        {
            var clips = new AudioClip[Enum.GetValues(typeof(Sfx)).Length];
            clips[(int)Sfx.Click] = Tone("click", 880f, 0.05f);
            clips[(int)Sfx.Hoe] = Tone("hoe", 160f, 0.12f);
            clips[(int)Sfx.Water] = Tone("water", 520f, 0.18f, 0.5f);
            clips[(int)Sfx.Plant] = Tone("plant", 330f, 0.10f);
            clips[(int)Sfx.Harvest] = Tone("harvest", 660f, 0.14f);
            clips[(int)Sfx.Coin] = Tone("coin", 1200f, 0.12f);
            clips[(int)Sfx.Error] = Tone("error", 130f, 0.15f);
            clips[(int)Sfx.Clink] = Tone("clink", 1760f, 0.3f);
            clips[(int)Sfx.Rustle] = Tone("rustle", 600f, 0.2f);
            clips[(int)Sfx.Anvil] = Tone("anvil", 988f, 0.4f);
            foreach (Sfx sfx in Enum.GetValues(typeof(Sfx)))
            {
                if (!SfxSynth.Makes(sfx)) continue;
                var samples = SfxSynth.Make(sfx);
                var clip = AudioClip.Create(sfx.ToString().ToLowerInvariant(), samples.Length, 1, SfxSynth.SampleRate, false);
                clip.SetData(samples, 0);
                clips[(int)sfx] = clip;
            }
            return clips;
        }

        public void ApplySettings(SettingsData s)
        {
            AudioListener.volume = s.MasterVolume;
            _sfxVolume = s.SfxVolume;
            _musicVolume = s.MusicVolume;
            _ambienceVolume = s.AmbienceVolume;
            if (_music != null) _music.volume = _musicVolume;
        }

        // Voices and signature sounds (T-132), made on first use and kept.
        readonly System.Collections.Generic.Dictionary<string, AudioClip> _voiceClips = new System.Collections.Generic.Dictionary<string, AudioClip>();
        public string LastVoice { get; private set; }          // for tests: the voice that blipped last
        public int VoiceBlipCount { get; private set; }
        public string LastSignature { get; private set; }

        AudioClip ClipFrom(string key, float[] samples)
        {
            if (_voiceClips.TryGetValue(key, out var clip)) return clip;
            clip = AudioClip.Create(key, samples.Length, 1, VoiceSynth.SampleRate, false);
            clip.SetData(samples, 0);
            _voiceClips[key] = clip;
            return clip;
        }

        void PlayClip(AudioClip clip, float volume)
        {
            if (_voices == null || clip == null) return;
            var voice = _voices[_next];
            _next = (_next + 1) % _voices.Length;
            voice.clip = clip;
            voice.volume = volume;
            voice.Play();
        }

        // One talking blip for a villager; `variant` varies the pitch a little. Cheap after the first call per voice.
        public void PlayVoice(string voiceId, int variant)
        {
            LastVoice = voiceId;
            VoiceBlipCount++;
            var profile = VoiceProfiles.For(voiceId);
            var bucket = ((variant % 7) + 7) % 7;
            PlayClip(ClipFrom($"voice.{voiceId}.{bucket}", VoiceSynth.Blip(profile, bucket)), _sfxVolume * 0.5f);
        }

        public void PlaySignature(string npcId)
        {
            LastSignature = npcId;
            PlayClip(ClipFrom("signature." + npcId, VoiceSynth.Signature(npcId)), _sfxVolume * 0.8f);
        }

        public void Play(Sfx sfx)
        {
            if (_voices == null) return;
            var voice = _voices[_next];
            _next = (_next + 1) % _voices.Length;
            voice.clip = _clips[(int)sfx];
            voice.volume = _sfxVolume;
            voice.Play();
        }

        public static void PlayIfAvailable(Sfx sfx)
        {
            if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.Play(sfx);
        }

        public static void PlayIfAvailable(Sfx sfx, float volumeScale, float pitch = 1f)
        {
            if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.PlayPitched(sfx, volumeScale, pitch);
        }

        // A quieter and/or higher or lower version of an effect (footsteps, hover ticks, animal calls).
        public void PlayPitched(Sfx sfx, float volumeScale, float pitch)
        {
            if (_voices == null) return;
            var voice = _voices[_next];
            _next = (_next + 1) % _voices.Length;
            voice.clip = _clips[(int)sfx];
            voice.volume = _sfxVolume * volumeScale;
            voice.pitch = pitch;
            voice.Play();
        }

        // ---- ambience: one looping bed at a time, faded in and out ----------------------------------------------------------------------
        AudioSource _ambience;
        readonly System.Collections.Generic.Dictionary<AmbienceKind, AudioClip> _ambienceClips = new System.Collections.Generic.Dictionary<AmbienceKind, AudioClip>();
        AmbienceKind _wanted, _playing;
        float _ambienceLevel;
        public AmbienceKind AmbienceWanted => _wanted;
        const float AmbienceGain = 0.35f;
        const float FadeSeconds = 1.5f;

        public void SetAmbience(AmbienceKind kind) => _wanted = kind;

        AudioClip AmbienceClip(AmbienceKind kind)
        {
            if (_ambienceClips.TryGetValue(kind, out var clip)) return clip;
            var samples = AmbienceSynth.Make(kind);
            clip = AudioClip.Create("ambience." + kind, samples.Length, 1, AmbienceSynth.SampleRate, false);
            clip.SetData(samples, 0);
            _ambienceClips[kind] = clip;
            return clip;
        }

        void Update()
        {
            if (_ambience == null)
            {
                _ambience = gameObject.AddComponent<AudioSource>();
                _ambience.loop = true;
                _ambience.playOnAwake = false;
                _ambience.spatialBlend = 0f;
            }
            var step = Time.unscaledDeltaTime / FadeSeconds;
            if (_playing != _wanted)
            {
                _ambienceLevel = Mathf.MoveTowards(_ambienceLevel, 0f, step);          // fade the old bed out first
                if (_ambienceLevel <= 0f)
                {
                    _playing = _wanted;
                    if (_playing == AmbienceKind.None) _ambience.Stop();
                    else { _ambience.clip = AmbienceClip(_playing); _ambience.Play(); }
                }
            }
            else if (_playing != AmbienceKind.None) _ambienceLevel = Mathf.MoveTowards(_ambienceLevel, 1f, step);
            _ambience.volume = _ambienceLevel * _ambienceVolume * AmbienceGain;
            UpdateLayers(step);
        }

        public AmbienceKind AmbiencePlaying => _playing;

        // ---- ambience layers: extra looping beds that modules lay over the main bed (ADR 0002), each fading to its own volume --------------
        sealed class Layer
        {
            public string Id;
            public Func<float[]> Render;
            public float Wanted, Level;
            public AudioSource Source;
        }

        readonly System.Collections.Generic.List<Layer> _layers = new System.Collections.Generic.List<Layer>();

        public const int LayerSampleRate = AmbienceSynth.SampleRate;

        // A looping bed laid over the main ambience. `render` makes the seamless loop (called once, on first use); volume 0 fades it out.
        public void SetAmbienceLayer(string id, Func<float[]> render, float volume)
        {
            if (string.IsNullOrEmpty(id)) return;
            Layer layer = null;
            foreach (var l in _layers) if (l.Id == id) { layer = l; break; }
            if (layer == null)
            {
                if (volume <= 0f) return;
                layer = new Layer { Id = id, Render = render };
                _layers.Add(layer);
            }
            layer.Wanted = Mathf.Clamp01(volume);
        }

        public float LayerWanted(string id)
        {
            foreach (var l in _layers) if (l.Id == id) return l.Wanted;
            return 0f;
        }

        public static void SetLayerIfAvailable(string id, Func<float[]> render, float volume)
        {
            if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.SetAmbienceLayer(id, render, volume);
        }

        void UpdateLayers(float step)
        {
            for (var i = 0; i < _layers.Count; i++)
            {
                var l = _layers[i];
                l.Level = Mathf.MoveTowards(l.Level, l.Wanted, step);
                if (l.Source == null)
                {
                    if (l.Wanted <= 0f) continue;
                    var samples = l.Render();
                    var clip = AudioClip.Create("layer." + l.Id, samples.Length, 1, LayerSampleRate, false);
                    clip.SetData(samples, 0);
                    l.Source = gameObject.AddComponent<AudioSource>();
                    l.Source.loop = true;
                    l.Source.playOnAwake = false;
                    l.Source.spatialBlend = 0f;
                    l.Source.clip = clip;
                }
                if (l.Level > 0f && !l.Source.isPlaying) l.Source.Play();
                else if (l.Level <= 0f && l.Source.isPlaying) l.Source.Stop();
                l.Source.volume = l.Level * _ambienceVolume * AmbienceGain;
            }
        }

        static AudioClip Tone(string name, float frequency, float seconds, float vibrato = 0f)
        {
            const int rate = 44100;
            var count = (int)(rate * seconds);
            var data = new float[count];
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)rate;
                var envelope = 1f - i / (float)count;
                var f = frequency * (1f + vibrato * 0.05f * Mathf.Sin(t * 40f));
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * envelope * 0.35f;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
