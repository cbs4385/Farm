using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Draws the weather of the day in outdoor maps (rain, snow, wind, lightning), entirely from the
    // WeatherDefinition: a pool of pixel-sized sprites that wrap around the view. No weather ids appear here.
    public sealed class WeatherEffects : MonoBehaviour
    {
        const int MaxParticles = 220;
        const float PixelsPerUnit = 16f;
        const int SortingOrder = 10000;

        struct Particle
        {
            public Vector2 View;     // position within the view, 0..1
            public float Speed;      // per-particle speed multiplier
            public float Phase;
        }

        SpriteRenderer[] _renderers;
        Particle[] _particles;
        Sprite _pixel;
        SpriteRenderer _flash;
        Camera _camera;
        GameSession _session;
        WeatherDefinition _shown;
        int _active;
        float _nextFlash;
        float _flashLevel;

        public int ActiveParticles => _active;
        public WeatherDefinition Shown => _shown;

        void Awake()
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            var rng = new System.Random(12345);
            _renderers = new SpriteRenderer[MaxParticles];
            _particles = new Particle[MaxParticles];
            for (var i = 0; i < MaxParticles; i++)
            {
                var go = new GameObject("p" + i);
                go.transform.SetParent(transform, false);
                var r = go.AddComponent<SpriteRenderer>();
                r.sprite = _pixel;
                r.sortingOrder = SortingOrder;
                r.enabled = false;
                _renderers[i] = r;
                _particles[i] = new Particle
                {
                    View = new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()),
                    Speed = 0.7f + (float)rng.NextDouble() * 0.6f,
                    Phase = (float)rng.NextDouble() * 6.28f,
                };
            }

            var flashObject = new GameObject("Flash");
            flashObject.transform.SetParent(transform, false);
            _flash = flashObject.AddComponent<SpriteRenderer>();
            _flash.sprite = _pixel;
            _flash.sortingOrder = SortingOrder + 1;
            _flash.color = new Color(1f, 1f, 1f, 0f);
        }

        void OnDestroy()
        {
            if (_pixel != null) Destroy(_pixel.texture);
            if (_pixel != null) Destroy(_pixel);
        }

        void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;
            if (_session == null && !Farm.Core.ServiceLocator.TryGet(out _session)) return;
            if (!_session.InGame) return;

            var def = _session.Weather.Get(_session.State.Weather);
            if (!ReferenceEquals(def, _shown) && (_shown == null || def.Id != _shown.Id)) Show(def);

            var viewHeight = _camera.orthographicSize * 2f;
            var viewSize = new Vector2(viewHeight * _camera.aspect, viewHeight);
            var center = (Vector2)_camera.transform.position;
            var dt = Time.deltaTime;
            var t = Time.time;

            for (var i = 0; i < _active; i++)
            {
                var p = _particles[i];
                var velocity = VelocityOf(def, p, t);
                p.View += new Vector2(velocity.x / viewSize.x, velocity.y / viewSize.y) * dt;
                p.View.x -= Mathf.Floor(p.View.x);
                p.View.y -= Mathf.Floor(p.View.y);
                _particles[i] = p;

                var world = center + new Vector2((p.View.x - 0.5f) * viewSize.x, (p.View.y - 0.5f) * viewSize.y);
                var r = _renderers[i];
                r.transform.position = new Vector3(Snap(world.x), Snap(world.y), 0f);
            }

            UpdateLightning(def, center, viewSize, dt);
        }

        void Show(WeatherDefinition def)
        {
            _shown = def;
            _active = def.Particles == WeatherParticles.None ? 0 : Mathf.RoundToInt(MaxParticles * def.ParticleDensity);
            for (var i = 0; i < MaxParticles; i++)
            {
                var r = _renderers[i];
                r.enabled = i < _active;
                if (i >= _active) continue;
                switch (def.Particles)
                {
                    case WeatherParticles.Rain:
                        r.color = new Color(0.75f, 0.85f, 1f, 0.7f);
                        r.transform.localScale = new Vector3(1f / PixelsPerUnit, 4f / PixelsPerUnit, 1f);
                        break;
                    case WeatherParticles.Snow:
                        r.color = new Color(1f, 1f, 1f, 0.9f);
                        r.transform.localScale = new Vector3(2f / PixelsPerUnit, 2f / PixelsPerUnit, 1f);
                        break;
                    default:
                        r.color = new Color(0.85f, 0.80f, 0.55f, 0.75f);
                        r.transform.localScale = new Vector3(3f / PixelsPerUnit, 1f / PixelsPerUnit, 1f);
                        break;
                }
            }
            _nextFlash = Time.time + Random.Range(4f, 9f);
        }

        // World units per second.
        static Vector2 VelocityOf(WeatherDefinition def, Particle p, float t)
        {
            switch (def.Particles)
            {
                case WeatherParticles.Rain:
                    return new Vector2(def.WindSlant * 7f, -22f * p.Speed);
                case WeatherParticles.Snow:
                    return new Vector2(def.WindSlant * 2f + Mathf.Sin(t * 1.3f + p.Phase) * 0.9f, -2.4f * p.Speed);
                default:
                    return new Vector2(def.WindSlant * 13f * p.Speed, Mathf.Sin(t * 2.2f + p.Phase) * 1.4f);
            }
        }

        void UpdateLightning(WeatherDefinition def, Vector2 center, Vector2 viewSize, float dt)
        {
            if (def.Lightning && Time.time >= _nextFlash)
            {
                _flashLevel = 0.55f;
                _nextFlash = Time.time + Random.Range(6f, 14f);
            }
            _flashLevel = Mathf.MoveTowards(_flashLevel, 0f, dt * 2.2f);
            _flash.color = new Color(1f, 1f, 1f, _flashLevel);
            _flash.transform.position = new Vector3(center.x, center.y, 0f);
            _flash.transform.localScale = new Vector3(viewSize.x, viewSize.y, 1f);
        }

        static float Snap(float v) => Mathf.Round(v * PixelsPerUnit) / PixelsPerUnit;
    }
}
