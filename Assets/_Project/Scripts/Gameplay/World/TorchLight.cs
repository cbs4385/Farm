using System.Linq;
using Farm.Core;
using Farm.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Farm.Gameplay
{
    // A torch in the picked slot of the item bar lights a bubble round the player (owner, 2026-10-11: five tiles, a glow that drifts between orange, red and yellow).
    // The 2D light is made on the player when it wakes and switched on only while the torch is the picked item.
    public sealed class TorchLight : MonoBehaviour
    {
        public const float Radius = 5f;
        public const float Intensity = 1.1f;
        public static readonly Color Orange = new Color(1f, 0.55f, 0.15f);
        public static readonly Color Red = new Color(0.95f, 0.22f, 0.1f);
        public static readonly Color Yellow = new Color(1f, 0.85f, 0.3f);

        Light2D _light;
        GameSession _session;
        float _phase;

        public bool IsLit => _light != null && _light.enabled;
        public Color CurrentColor => _light != null ? _light.color : Color.black;

        void Awake()
        {
            var go = new GameObject("TorchLight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            _light = go.AddComponent<Light2D>();
            _light.lightType = Light2D.LightType.Point;
            _light.pointLightOuterRadius = Radius;
            _light.pointLightInnerRadius = Radius * 0.1f;
            _light.falloffIntensity = 0.5f;
            _light.targetSortingLayers = SortingLayer.layers.Select(l => l.id).ToArray();
            _light.enabled = false;
        }

        // Which colour the glow has at a moment: a slow drift around orange, red and yellow, never leaving them.
        public static Color GlowAt(float t)
        {
            var a = Mathf.Sin(t * 1.7f) * 0.5f + 0.5f;                 // orange <-> red
            var b = Mathf.Sin(t * 2.9f + 1.1f) * 0.5f + 0.5f;          // and towards yellow
            return Color.Lerp(Color.Lerp(Orange, Red, a), Yellow, b * 0.7f);
        }

        public static bool Holding(GameSession s)
        {
            if (s == null || !s.InGame) return false;
            var slot = s.State.SelectedHotbar;
            if (slot < 0 || slot >= s.Backpack.Capacity) return false;
            return s.Backpack.Get(slot)?.ItemId == ItemIds.Torch;
        }

        void Update()
        {
            if (_session == null && !ServiceLocator.TryGet(out _session)) return;
            var on = Holding(_session);
            if (_light.enabled != on) _light.enabled = on;
            if (!on) return;
            var t = Time.time + _phase;
            _light.color = GlowAt(t);
            var wobble = Mathf.Sin(t * 9f) * 0.5f + Mathf.Sin(t * 14.3f + 1.3f) * 0.3f + Mathf.Sin(t * 23f) * 0.2f;
            _light.intensity = Intensity * (1f + wobble * 0.12f);
        }
    }
}
