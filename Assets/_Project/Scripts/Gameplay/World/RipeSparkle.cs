using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // A small twinkle over every crop and fruit tree that is ready to harvest, so a field shows which plants to pick without pointing at each one (playtest 2026-10-14:
    // "all the parsnips look the same"). The map view says where the ripe plants are a few times a second; the stars are pooled.
    public sealed class RipeSparkle : MonoBehaviour, IMapBound
    {
        const float Interval = 0.3f;
        const float Period = 1.6f;

        static Sprite _star;

        readonly List<Vector3> _spots = new List<Vector3>();
        readonly List<SpriteRenderer> _stars = new List<SpriteRenderer>();
        FarmMapView _view;
        GameSession _session;
        float _next;

        public int Count => _spots.Count;

        public void Bind(MapContext map) => _session = map.Session;

        void Awake() => _view = GetComponent<FarmMapView>();

        // 0 at the start and end of a twinkle, 1 at its peak; each plant is at its own moment so the field does not blink as one. (pure)
        public static float Twinkle(float time, float phase)
        {
            var t = Mathf.Repeat(time / Period + phase, 1f);
            return t < 0.5f ? t * 2f : (1f - t) * 2f;
        }

        // Where on the plant's cell its star sits: upper right, then upper left, so neighbours do not line up. (pure)
        public static Vector2 Offset(int x, int y) => new Vector2(((x * 7 + y * 3) & 1) == 0 ? 0.2f : -0.2f, 0.25f + ((x + y) & 1) * 0.15f);

        void Update()
        {
            if (_view == null) return;
            if (Time.time >= _next)
            {
                _next = Time.time + Interval;
                _view.CollectRipe(_spots);
                while (_stars.Count < _spots.Count) _stars.Add(MakeStar());
            }
            var calm = _session != null && _session.Settings != null && _session.Settings.ReduceFlashes;          // a steady mark instead of a twinkle
            for (var i = 0; i < _stars.Count; i++)
            {
                var star = _stars[i];
                if (i >= _spots.Count) { star.enabled = false; continue; }
                var spot = _spots[i];
                var level = calm ? 0.8f : Twinkle(Time.time, Mathf.Abs(spot.x * 0.37f + spot.y * 0.61f));
                star.transform.position = spot;
                star.transform.localScale = Vector3.one * (0.6f + 0.5f * level);
                star.color = new Color(1f, 0.95f, 0.6f, 0.25f + 0.75f * level);
                star.enabled = true;
            }
        }

        SpriteRenderer MakeStar()
        {
            var go = new GameObject("RipeStar");
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Star;
            renderer.sortingOrder = 6;
            return renderer;
        }

        static Sprite Star
        {
            get
            {
                if (_star != null) return _star;
                const int size = 7;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "ripe_star" };
                var c = size / 2;
                for (var y = 0; y < size; y++)
                    for (var x = 0; x < size; x++)
                    {
                        var dx = Mathf.Abs(x - c);
                        var dy = Mathf.Abs(y - c);
                        var on = (dx == 0 && dy <= 3) || (dy == 0 && dx <= 3) || (dx == 1 && dy == 1);          // a four-pointed star
                        texture.SetPixel(x, y, on ? Color.white : Color.clear);
                    }
                texture.Apply();
                _star = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
                return _star;
            }
        }
    }
}
