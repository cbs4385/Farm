using Farm.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Farm.Gameplay
{
    // Drives a global 2D light from the in-game time. Indoor maps use a fixed warm colour.
    public sealed class DayNightLighting : MonoBehaviour, IMapBound
    {
        [SerializeField] Light2D _light;
        [SerializeField] bool _indoor;

        // The colours are tints (their strongest channel is 1): how bright the light is comes from Brightness() below, so night is half as bright as noon
        // whatever its colour (owner, 2026-10-09: "night at about 50% brightness and noon at 100%").
        public const float NightBrightness = 0.1f;
        public const float SceneNightBrightness = 0.5f;          // the "night" mood of a story scene (the world is darker than that now, and a scene must stay readable)
        static readonly Color DawnTint = new Color(0.97f, 0.92f, 1f);
        static readonly Color DayTint = Color.white;
        static readonly Color DuskTint = new Color(1f, 0.88f, 0.78f);
        static readonly Color NightTint = new Color(0.88f, 0.92f, 1f);
        static readonly Color Dawn = Dim(DawnTint, 0.62f);
        static readonly Color Day = Color.white;
        static readonly Color Dusk = Dim(DuskTint, 0.75f);
        static readonly Color Night = Dim(NightTint, SceneNightBrightness);
        static readonly Color Indoor = new Color(1f, 0.95f, 0.85f);

        GameSession _session;
        AtmosphereService _atmosphere;

        public bool IsIndoor => _indoor;

        public void Bind(MapContext map)
        {
            _session = map.Session;
            _atmosphere = map.Atmosphere;
        }

        // The mine (owner, 2026-10-11): tinted blue; (owner, 2026-10-12) a tenth of the light, so that the torch is what lets the player see.
        public const float CaveBrightness = 0.1f;
        public static readonly Color CaveTint = new Color(0.55f, 0.72f, 1f);
        public static readonly Color Cave = Dim(CaveTint, CaveBrightness);
        bool _cave;

        public bool IsCave => _cave;
        public void SetCave() => _cave = true;

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
            _overrideWeight = Mathf.MoveTowards(_overrideWeight, _overrideGoal, _overrideSpeed * Time.deltaTime);

            if (_indoor)
            {
                var room = _cave ? Cave : Indoor;
                var indoor = AsLight(_atmosphere != null ? _atmosphere.Stack.Apply(room) : room);
                _light.color = _overrideWeight > 0f ? Color.Lerp(indoor, _overrideColor, _overrideWeight) : indoor;
                _light.intensity = 1f;
                return;
            }
            if (_session == null) return;
            if (!_session.InGame) return;

            var c = ColorAt(_session.Clock.Now.MinuteOfDay);
            c = _session.Weather.Get(_session.State.Weather).Apply(c);
            // Optional mood layers pushed by modules (fog, dread...) blend over the normal lighting.
            if (_atmosphere != null) c = _atmosphere.Stack.Apply(c);
            c = AsLight(c);
            if (_overrideWeight > 0f) c = Color.Lerp(c, _overrideColor, _overrideWeight);
            _light.color = c;
            _light.intensity = 1f;
        }

        // How bright the outdoor light is at a time of day, from NightBrightness (a tenth, owner 2026-10-12) to 1: it rises through the dawn from 06:00 to 09:00, is full
        // from 09:00 to 17:00, falls through the dusk from 17:00 to 21:30 and stays at the night level until the sky lightens again before 06:00 (the day runs on past
        // midnight, so 24:00 is 24).
        public static float Brightness(int minuteOfDay)
        {
            var h = minuteOfDay / 60f;
            var sun = 0f;
            if (h >= 6f && h < 9f) sun = Smooth((h - 6f) / 3f);
            else if (h >= 9f && h < 17f) sun = 1f;
            else if (h >= 17f && h < 21.5f) sun = Smooth(1f - (h - 17f) / 4.5f);
            else if (h >= 28f) sun = Smooth((h - 28f) / 2f) * 0.5f;               // the sky lightens a little before 06:00 (30:00)
            return Mathf.Lerp(NightBrightness, 1f, sun);
        }

        // The street lamps burn from dusk to dawn: while the daylight is less than about two thirds of its full strength. (pure)
        public const float LampsOnBelow = 0.6f;
        public static bool LampsOn(int minuteOfDay) => Brightness(minuteOfDay) < LampsOnBelow;

        // The game works in linear light, where a light colour of 0.1 shows on screen as about a third of full brightness (owner, 2026-10-12: night was not dim). The brightness
        // numbers of this class mean what the eye sees, so the light is given the colour raised to the screen's power: a tenth looks like a tenth. The tint is kept.
        public const float ScreenPower = 2.2f;

        public static Color AsLight(Color c)
        {
            var top = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            if (top <= 0f) return new Color(0f, 0f, 0f, 1f);
            var scale = Mathf.Pow(top, ScreenPower) / top;
            return new Color(c.r * scale, c.g * scale, c.b * scale, 1f);
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
