using UnityEngine;

namespace Farm.Gameplay
{
    // A street lamp of the village: its lantern glows from dusk to dawn (owner, 2026-10-12: the lamps light between dusk and dawn). The light is a FlickerLight, made when the
    // lamp wakes, switched on and off by the time of day.
    public sealed class StreetLamp : MonoBehaviour, IMapBound
    {
        public const float Radius = 9f;          // doubled from 4.5 tiles on the owner's word, 2026-10-16
        public const float Intensity = 0.9f;
        public static readonly Color Glow = new Color(1f, 0.78f, 0.42f);

        FlickerLight _flame;
        GameSession _session;
        float _next;

        public bool IsLit => _flame != null && _flame.Lit;

        void Awake()
        {
            var go = new GameObject("LampLight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            _flame = go.AddComponent<FlickerLight>();
            _flame.Color = Glow;
            _flame.Radius = Radius;
            _flame.Intensity = Intensity;
            _flame.Flicker = 0.05f;
            _flame.Lit = false;
        }

        public void Bind(MapContext map) => _session = map.Session;

        void Update()
        {
            if (_flame == null || _session == null || Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 0.5f;
            var on = _session.InGame && DayNightLighting.LampsOn(_session.Clock.Now.MinuteOfDay);
            if (_flame.Lit != on) _flame.Lit = on;
        }
    }
}
