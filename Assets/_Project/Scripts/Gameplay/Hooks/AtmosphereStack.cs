using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Named visual "mood" layers blended over the day/night lighting. Modules push a layer (id, tint, strength,
    // priority) and remove it later; the lighting asks the stack to colour the base light. Pure logic.
    public sealed class AtmosphereStack
    {
        struct Layer
        {
            public Color Tint;
            public float Strength;
            public int Priority;
        }

        readonly Dictionary<string, Layer> _layers = new Dictionary<string, Layer>();

        public int Count => _layers.Count;

        public void Set(string id, Color tint, float strength, int priority = 0) =>
            _layers[id] = new Layer { Tint = tint, Strength = Mathf.Clamp01(strength), Priority = priority };

        public bool Remove(string id) => _layers.Remove(id);

        public void Clear() => _layers.Clear();

        // Layers apply from lowest to highest priority; each blends the colour towards (colour x tint).
        public Color Apply(Color baseColor)
        {
            if (_layers.Count == 0) return baseColor;
            var ordered = new List<Layer>(_layers.Values);
            ordered.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            var c = baseColor;
            foreach (var l in ordered)
                c = Color.Lerp(c, new Color(c.r * l.Tint.r, c.g * l.Tint.g, c.b * l.Tint.b, c.a), l.Strength);
            return c;
        }
    }
}
