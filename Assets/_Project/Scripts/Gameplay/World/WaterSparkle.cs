using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // Small glints that come and go on open water (a pond, the sea), so the water does not sit dead still. A handful of one-pixel white
    // sparks are placed on water cells once, and each one twinkles on its own beat. Nothing is made or destroyed while the game runs.
    public sealed class WaterSparkle : MonoBehaviour
    {
        public const int MaxGlints = 18;

        struct Glint
        {
            public SpriteRenderer Renderer;
            public float Phase, Speed;
        }

        Glint[] _glints = new Glint[0];

        public int Count => _glints.Length;

        // How bright a glint is at a moment: mostly dark, with a short bright flash each beat (0..1). (pure)
        public static float Twinkle(float time, float phase, float speed)
        {
            var wave = Mathf.Max(0f, Mathf.Sin(time * speed + phase));
            return wave * wave * wave * wave * wave * wave;
        }

        public void Init(FarmMap map)
        {
            var water = new List<Vector3Int>();
            var bounds = map.Ground.cellBounds;
            foreach (var cell in bounds.allPositionsWithin)
                if (map.IsWater(cell)) water.Add(cell);
            if (water.Count == 0) { Destroy(gameObject); return; }

            var rng = new System.Random(map.MapId.GetHashCode() ^ 0x5eed);
            var count = Mathf.Min(MaxGlints, 4 + water.Count / 6);
            _glints = new Glint[count];
            for (var i = 0; i < count; i++)
            {
                var cell = water[rng.Next(water.Count)];
                var go = new GameObject("Glint" + i);
                go.transform.SetParent(transform, false);
                var center = map.CellCenter(cell);
                go.transform.position = new Vector3(center.x + (rng.Next(-6, 7)) / 16f, center.y + (rng.Next(-6, 7)) / 16f, 0f);
                go.transform.localScale = new Vector3(1f / 16f, 1f / 16f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = EnemySprites.Pixel();
                sr.sortingOrder = 5;
                sr.color = new Color(1f, 1f, 1f, 0f);
                _glints[i] = new Glint { Renderer = sr, Phase = (float)rng.NextDouble() * 6.28f, Speed = 1.2f + (float)rng.NextDouble() * 1.4f };
            }
        }

        void Update()
        {
            var t = Time.time;
            for (var i = 0; i < _glints.Length; i++)
            {
                var g = _glints[i];
                g.Renderer.color = new Color(1f, 1f, 1f, Twinkle(t, g.Phase, g.Speed) * 0.9f);
            }
        }
    }
}
