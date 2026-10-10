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
        float _nextShot = 15f;
        int _lastHour = -1;
        bool _village;

        public static AmbienceKind Last { get; private set; }
        static AmbienceDirector() => TestResets.Add(ResetForTests);
        public static void ResetForTests() { Last = AmbienceKind.None; }

        public void Init(GameSession session, DayNightLighting lighting)
        {
            _session = session; _lighting = lighting;
            _village = session != null && session.State.CurrentMap == MapIds.Village;
            _lastHour = session != null && session.InGame ? session.Clock.Now.MinuteOfDay / 60 : -1;
            Apply();
        }

        void Update()
        {
            OneShots();
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + CheckEvery;
            Apply();
        }

        // Thunder in storms, a gust in the wind, the bell at noon and at six in the evening.
        void OneShots()
        {
            if (_session == null || !_session.InGame || _session.Clock.IsPaused) return;
            var now = Time.time;
            var shot = OneShotFor(_session.State.Weather, _lighting != null && _lighting.IsIndoor);
            if (shot.HasValue && now >= _nextShot)
            {
                _nextShot = now + UnityEngine.Random.Range(14f, 38f);
                AudioService.PlayIfAvailable(shot.Value, 0.8f, UnityEngine.Random.Range(0.9f, 1.1f));
            }
            var hour = _session.Clock.Now.MinuteOfDay / 60;
            if (hour != _lastHour)
            {
                if (_village && BellHour(hour)) StartCoroutine(Strikes(hour == 12 ? 3 : 2));
                _lastHour = hour;
            }
        }

        System.Collections.IEnumerator Strikes(int count)
        {
            for (var i = 0; i < count; i++)
            {
                AudioService.PlayIfAvailable(Sfx.Bell, 0.55f);
                yield return new WaitForSeconds(1.3f);
            }
        }

        public static bool BellHour(int hour) => hour == 12 || hour == 18;

        // The one-shot that goes with the weather outdoors: a storm rumbles, the wind gusts.
        public static Sfx? OneShotFor(string weather, bool indoor) =>
            indoor ? (Sfx?)null : weather == WeatherIds.Storm ? Sfx.Thunder : weather == WeatherIds.Wind ? Sfx.Gust : (Sfx?)null;

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
