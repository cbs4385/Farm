using System.Collections.Generic;
using Farm.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Gameplay
{
    // Plays the music: asks MusicChoice what belongs here and now, and fades from one track to the next (they loop). A scene can ask for a track
    // by name with the `music` step (a MusicCue); "stop" gives control back. The loudness follows the music volume of the options. Tracks are
    // files under Resources/Music named after their cue; a cue with no file is silent (and logged once).
    public sealed class MusicDirector : MonoBehaviour
    {
        public const string ResourceFolder = "Music";
        public const float FadeSeconds = 2.5f;
        public const float TrackGain = 0.55f;                   // the tracks are mastered loud: sit them under the effects
        const float CheckEvery = 0.5f;

        readonly AudioSource[] _source = new AudioSource[2];
        readonly float[] _level = new float[2];
        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        readonly HashSet<string> _missing = new HashSet<string>();
        int _active;
        float _next;
        string _asked;                                          // a scene's own request (MusicCue), or null

        public static MusicDirector Current { get; private set; }
        public string Wanted { get; private set; }              // the cue that should be playing now (null: silence)
        public string Playing { get; private set; }             // the cue on the active source

        void Awake()
        {
            Current = this;
            for (var i = 0; i < 2; i++)
            {
                _source[i] = gameObject.AddComponent<AudioSource>();
                _source[i].loop = true;
                _source[i].playOnAwake = false;
                _source[i].spatialBlend = 0f;
            }
        }

        void Start()
        {
            if (ServiceLocator.TryGet<EventBus>(out var bus)) bus.Subscribe<MusicCue>(OnCue);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        // A scene asks for a track by name; a name with no file (a track that has not been made yet) is ignored, so the usual music carries on.
        void OnCue(MusicCue cue) => _asked = cue.IsStop || Clip(cue.Name) == null ? null : cue.Name;

        void Update()
        {
            if (Time.unscaledTime >= _next)
            {
                _next = Time.unscaledTime + CheckEvery;
                Wanted = _asked ?? Choose();
                if (Wanted != Playing) Switch(Wanted);
            }
            Fade();
        }

        string Choose()
        {
            var inGame = ServiceLocator.TryGet<GameSession>(out var session) && session.InGame;
            var now = inGame ? session.Clock.Now : default;
            var rain = inGame && session.Weather.Get(session.State.Weather).WateringCrops;
            var festival = inGame && session.Story != null && StoryCalendar.DaysUntilFestival(session.Story.Events, now) == 0;
            return MusicChoice.For(SceneManager.GetActiveScene().name, inGame, now.Season, inGame ? now.MinuteOfDay / 60 : 12, rain, festival);
        }

        // Starts `cue` on the idle source (it fades in while the other fades out); null fades everything out.
        void Switch(string cue)
        {
            Playing = cue;
            if (cue == null) { _active = 1 - _active; return; }
            var clip = Clip(cue);
            if (clip == null) { Playing = null; _active = 1 - _active; return; }
            _active = 1 - _active;
            var s = _source[_active];
            s.clip = clip;
            _level[_active] = 0f;
            s.Play();
        }

        void Fade()
        {
            var step = Time.unscaledDeltaTime / FadeSeconds;
            var volume = (ServiceLocator.TryGet<AudioService>(out var audio) ? audio.MusicVolume : 0.7f) * TrackGain;
            for (var i = 0; i < 2; i++)
            {
                var on = i == _active && Playing != null;
                _level[i] = Mathf.MoveTowards(_level[i], on ? 1f : 0f, step);
                _source[i].volume = _level[i] * volume;
                if (!on && _level[i] <= 0f && _source[i].isPlaying) _source[i].Stop();
            }
        }

        AudioClip Clip(string cue)
        {
            if (_clips.TryGetValue(cue, out var clip)) return clip;
            clip = Resources.Load<AudioClip>(ResourceFolder + "/" + cue);
            if (clip == null && _missing.Add(cue)) Log.Warn($"No music file for the cue '{cue}'.");
            _clips[cue] = clip;
            return clip;
        }
    }
}
