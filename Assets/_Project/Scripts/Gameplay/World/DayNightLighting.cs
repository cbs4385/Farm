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

        // The colours are tints (their strongest channel is 1): how bright the light is comes from Brightness() below, so night is half as bright as noon
        // whatever its colour (owner, 2026-10-09: "night at about 50% brightness and noon at 100%").
        public const float NightBrightness = 0.5f;
        static readonly Color DawnTint = new Color(0.97f, 0.92f, 1f);
        static readonly Color DayTint = Color.white;
        static readonly Color DuskTint = new Color(1f, 0.88f, 0.78f);
        static readonly Color NightTint = new Color(0.88f, 0.92f, 1f);
        static readonly Color Dawn = Dim(DawnTint, 0.62f);
        static readonly Color Day = Color.white;
        static readonly Color Dusk = Dim(DuskTint, 0.75f);
        static readonly Color Night = Dim(NightTint, NightBrightness);
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

        // How bright the outdoor light is at a time of day, from NightBrightness (about 50%) to 1 at noon: it rises from 06:00 to noon, falls from noon until
        // 22:00 and stays at the night level until the sky lightens again before 06:00 (the day runs on past midnight, so 24:00 is 24).
        public static float Brightness(int minuteOfDay)
        {
            var h = minuteOfDay / 60f;
            var sun = 0f;
            if (h >= 6f && h < 12f) sun = Smooth((h - 6f) / 6f);
            else if (h >= 12f && h < 22f) sun = Smooth(1f - (h - 12f) / 10f);
            else if (h >= 28f) sun = Smooth((h - 28f) / 2f) * 0.5f;               // the sky lightens a little before 06:00 (30:00)
            return Mathf.Lerp(NightBrightness, 1f, sun);
        }

        static Color Dim(Color c, float f) => new Color(c.r * f, c.g * f, c.b * f, 1f);

        static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

        // The colour of the light: a tint that moves dawn, day, dusk, night, at the brightness above.
        public static Color ColorAt(int minuteOfDay)
        {
            var h = minuteOfDay / 60f;
            Color tint;
            if (h < 9f) tint = Color.Lerp(DawnTint, DayTint, Mathf.InverseLerp(6f, 9f, h));
            else if (h < 17f) tint = DayTint;
            else if (h < 19.5f) tint = Color.Lerp(DayTint, DuskTint, Mathf.InverseLerp(17f, 19.5f, h));
            else if (h < 22f) tint = Color.Lerp(DuskTint, NightTint, Mathf.InverseLerp(19.5f, 22f, h));
            else if (h < 28f) tint = NightTint;
            else tint = Color.Lerp(NightTint, DawnTint, Mathf.InverseLerp(28f, 30f, h));
            var b = Brightness(minuteOfDay);
            return new Color(tint.r * b, tint.g * b, tint.b * b, 1f);
        }
    }
}
