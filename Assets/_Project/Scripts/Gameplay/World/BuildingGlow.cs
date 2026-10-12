using UnityEngine;

namespace Farm.Gameplay
{
    // A warm glow on a building's front from dusk to dawn, as if its windows were lit (owner, 2026-10-12: the village's buildings and the player's house glow). The light is a
    // FlickerLight, made when the building wakes, switched on and off by the time of day like the street lamps.
    public sealed class BuildingGlow : MonoBehaviour, IMapBound
    {
        public const float Radius = 4.2f;
        public const float Intensity = 0.7f;
        public static readonly Color Glow = new Color(1f, 0.8f, 0.48f);

        FlickerLight _flame;
        GameSession _session;
        float _next;

        public bool IsLit => _flame != null && _flame.Lit;

        void Awake()
        {
            var go = new GameObject("WindowGlow");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            _flame = go.AddComponent<FlickerLight>();
            _flame.Color = Glow;
            _flame.Radius = Radius;
            _flame.Intensity = Intensity;
            _flame.Flicker = 0.04f;
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
