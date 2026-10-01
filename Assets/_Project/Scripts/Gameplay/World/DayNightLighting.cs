using Farm.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Farm.Gameplay
{
    // Drives a global 2D light from the in-game time. Indoor maps use a fixed warm colour.
    public sealed class DayNightLighting : MonoBehaviour
    {
        [SerializeField] Light2D _light;
        [SerializeField] bool _indoor;

        static readonly Color Dawn = new Color(0.85f, 0.80f, 0.95f);
        static readonly Color Day = Color.white;
        static readonly Color Dusk = new Color(1f, 0.72f, 0.55f);
        static readonly Color Night = new Color(0.30f, 0.34f, 0.60f);
        static readonly Color Indoor = new Color(1f, 0.95f, 0.85f);

        GameSession _session;

        public void Configure(Light2D light, bool indoor)
        {
            _light = light;
            _indoor = indoor;
        }

        void Update()
        {
            if (_light == null) return;
            if (_indoor) { _light.color = Indoor; _light.intensity = 1f; return; }
            if (_session == null && !ServiceLocator.TryGet(out _session)) return;
            if (!_session.InGame) return;

            var c = ColorAt(_session.Clock.Now.MinuteOfDay);
            if (_session.State.Weather == WeatherIds.Rain) c = Color.Lerp(c, new Color(0.55f, 0.60f, 0.70f), 0.45f);
            _light.color = c;
            _light.intensity = 1f;
        }

        // Piecewise gradient: 6:00 dawn, 9:00 day, 17:00 day, 19:30 dusk, 22:00 night, 26:00 night.
        public static Color ColorAt(int minuteOfDay)
        {
            var h = minuteOfDay / 60f;
            if (h < 9f) return Color.Lerp(Dawn, Day, Mathf.InverseLerp(6f, 9f, h));
            if (h < 17f) return Day;
            if (h < 19.5f) return Color.Lerp(Day, Dusk, Mathf.InverseLerp(17f, 19.5f, h));
            if (h < 22f) return Color.Lerp(Dusk, Night, Mathf.InverseLerp(19.5f, 22f, h));
            return Night;
        }
    }
}
