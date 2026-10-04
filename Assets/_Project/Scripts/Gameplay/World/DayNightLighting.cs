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
        AtmosphereService _atmosphere;

        public bool IsIndoor => _indoor;

        // A scene's mood (T-100): blends a colour over the normal lighting, and back.
        Color _overrideColor = Color.white;
        float _overrideWeight, _overrideGoal, _overrideSpeed;

        public float OverrideWeight => _overrideWeight;

        public void SetOverride(Color color, float seconds)
        {
            _overrideColor = color;
            _overrideGoal = 1f;
            _overrideSpeed = seconds <= 0.01f ? 1000f : 1f / seconds;
            if (seconds <= 0.01f) _overrideWeight = 1f;
        }

        public void ClearOverride(float seconds)
        {
            _overrideGoal = 0f;
            _overrideSpeed = seconds <= 0.01f ? 1000f : 1f / seconds;
            if (seconds <= 0.01f) _overrideWeight = 0f;
        }

        public static Color PresetColor(string preset)
        {
            switch (preset)
            {
                case "dawn": return Dawn;
                case "dusk": return Dusk;
                case "night": return Night;
                case "warm": return new Color(1f, 0.85f, 0.65f);
                case "dim": return new Color(0.62f, 0.62f, 0.72f);
                default: return Day;
            }
        }

        public void Configure(Light2D light, bool indoor)
        {
            _light = light;
            _indoor = indoor;
        }

        void Update()
        {
            if (_light == null) return;
            if (_atmosphere == null) ServiceLocator.TryGet(out _atmosphere);
            _overrideWeight = Mathf.MoveTowards(_overrideWeight, _overrideGoal, _overrideSpeed * Time.deltaTime);

            if (_indoor)
            {
                var indoor = _atmosphere != null ? _atmosphere.Stack.Apply(Indoor) : Indoor;
                _light.color = _overrideWeight > 0f ? Color.Lerp(indoor, _overrideColor, _overrideWeight) : indoor;
                _light.intensity = 1f;
                return;
            }
            if (_session == null && !ServiceLocator.TryGet(out _session)) return;
            if (!_session.InGame) return;

            var c = ColorAt(_session.Clock.Now.MinuteOfDay);
            c = _session.Weather.Get(_session.State.Weather).Apply(c);
            // Optional mood layers pushed by modules (fog, dread...) blend over the normal lighting.
            if (_atmosphere != null) c = _atmosphere.Stack.Apply(c);
            if (_overrideWeight > 0f) c = Color.Lerp(c, _overrideColor, _overrideWeight);
            _light.color = c;
            _light.intensity = 1f;
        }

        // Piecewise gradient: 6:00 dawn, 9:00 day, 17:00 day, 19:30 dusk, 22:00 night, 28:00 night, 30:00 dawn.
        public static Color ColorAt(int minuteOfDay)
        {
            var h = minuteOfDay / 60f;
            if (h < 9f) return Color.Lerp(Dawn, Day, Mathf.InverseLerp(6f, 9f, h));
            if (h < 17f) return Day;
            if (h < 19.5f) return Color.Lerp(Day, Dusk, Mathf.InverseLerp(17f, 19.5f, h));
            if (h < 22f) return Color.Lerp(Dusk, Night, Mathf.InverseLerp(19.5f, 22f, h));
            if (h < 28f) return Night;
            return Color.Lerp(Night, Dawn, Mathf.InverseLerp(28f, 30f, h));   // the sky lightens before 06:00
        }
    }
}
