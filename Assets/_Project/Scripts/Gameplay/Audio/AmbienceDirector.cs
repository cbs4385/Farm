using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Created on every map: chooses the ambience bed from where the player is and what the sky is doing, and hands it to the audio service,
    // which fades between beds. Indoors the world is quiet. Pure choice logic is in Choose so it can be tested.
    public sealed class AmbienceDirector : MonoBehaviour
    {
        const float CheckEvery = 1f;

        GameSession _session;
        DayNightLighting _lighting;
        float _next;

        public static AmbienceKind Last { get; private set; }
        public static void ResetForTests() { Last = AmbienceKind.None; }

        public void Init(GameSession session, DayNightLighting lighting)
        {
            _session = session; _lighting = lighting;
            Apply();
        }

        void Update()
        {
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + CheckEvery;
            Apply();
        }

        void Apply()
        {
            if (_session == null || !_session.InGame) return;
            var indoor = _lighting != null && _lighting.IsIndoor;
            Last = Choose(indoor, _session.State.Weather, _session.Clock.Now.MinuteOfDay / 60, _session.Clock.Now.Season);
            if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.SetAmbience(Last);
        }

        void OnDestroy()
        {
            if (ServiceLocator.TryGet<AudioService>(out var audio)) audio.SetAmbience(AmbienceKind.None);
        }

        // Rain and storms rain; wind and snow blow; a clear day has birds, a clear night (outside winter) has crickets.
        public static AmbienceKind Choose(bool indoor, string weather, int hour, Season season)
        {
            if (indoor) return AmbienceKind.None;
            if (weather == WeatherIds.Rain || weather == WeatherIds.Storm) return AmbienceKind.Rain;
            if (weather == WeatherIds.Wind || weather == WeatherIds.Snow) return AmbienceKind.Wind;
            var day = hour >= 6 && hour < 19;
            if (day) return AmbienceKind.Birds;
            return season == Season.Winter ? AmbienceKind.None : AmbienceKind.Crickets;
        }
    }
}
