using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    public enum Sfx { Click, Hoe, Water, Plant, Harvest, Coin, Error }

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

            _clips = new[]
            {
                Tone("click", 880f, 0.05f), Tone("hoe", 160f, 0.12f), Tone("water", 520f, 0.18f, 0.5f),
                Tone("plant", 330f, 0.10f), Tone("harvest", 660f, 0.14f), Tone("coin", 1200f, 0.12f),
                Tone("error", 130f, 0.15f),
            };
        }

        public void ApplySettings(SettingsData s)
        {
            AudioListener.volume = s.MasterVolume;
            _sfxVolume = s.SfxVolume;
            _musicVolume = s.MusicVolume;
            _ambienceVolume = s.AmbienceVolume;
            if (_music != null) _music.volume = _musicVolume;
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
