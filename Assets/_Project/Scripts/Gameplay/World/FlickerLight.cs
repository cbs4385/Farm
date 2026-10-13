using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Farm.Gameplay
{
    // A warm point light that flickers (candles, a fire, a lantern's glow): lights the sprites near it, so that a lit place is brighter than the dark round it. Made as a 2D light
    // when it wakes; the colour, radius and strength are set before that or changed afterwards through the properties.
    public sealed class FlickerLight : MonoBehaviour
    {
        [SerializeField] Color _color = new Color(1f, 0.74f, 0.4f);
        [SerializeField] float _radius = 6f, _intensity = 1f, _flicker = 0.12f;

        Light2D _light;
        float _phase;

        public Color Color { get => _color; set { _color = value; if (_light != null) _light.color = value; } }
        public float Radius { get => _radius; set { _radius = value; if (_light != null) { _light.pointLightOuterRadius = value; _light.pointLightInnerRadius = value * 0.12f; } } }
        public float Intensity { get => _intensity; set => _intensity = value; }
        public float Flicker { get => _flicker; set => _flicker = value; }
        public float CurrentIntensity => _light != null ? _light.intensity : 0f;

        // Switched off (a torch that is not held) it gives no light and costs nothing.
        public bool Lit
        {
            get => _light != null && _light.enabled;
            set { if (_light != null) _light.enabled = value; }
        }

        void Awake()
        {
            _phase = Mathf.Abs(transform.position.x * 3.7f + transform.position.y * 1.9f);
            _light = gameObject.AddComponent<Light2D>();
            _light.lightType = Light2D.LightType.Point;
            _light.color = _color;
            _light.intensity = _intensity;
            _light.pointLightOuterRadius = _radius;
            _light.pointLightInnerRadius = _radius * 0.12f;
            _light.falloffIntensity = 0.55f;
            _light.targetSortingLayers = SortingLayer.layers.Select(l => l.id).ToArray();
        }

        // The brightness factor at a time (pure): a steady light when the player has asked for fewer flashes (playtest 2026-10-13: the light in the mine hurt their eyes).
        public static float Factor(float time, float phase, float flicker, bool calm)
        {
            if (calm) return 1f;
            var t = time * 7f + phase;
            var wobble = Mathf.Sin(t) * 0.5f + Mathf.Sin(t * 2.3f + 1.3f) * 0.3f + Mathf.Sin(t * 5.1f) * 0.2f;
            return 1f + wobble * flicker;
        }

        void Update()
        {
            if (_light == null) return;
            var settings = GameContext.Current?.Session?.Settings;
            _light.intensity = _intensity * Factor(Time.time, _phase, _flicker, settings != null && settings.ReduceFlashes);
        }
    }
}
