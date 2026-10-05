using System.Collections.Generic;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The black cat that ambles about the village by day. It walks short hops between nearby cells, rests between them, meows now and then
    // while it is on screen (so a meow always has a cat to go with it), and can be petted.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VillageCat : MonoBehaviour, IInteractable
    {
        const float Speed = 1.4f;
        const float FrameTime = 0.2f;
        const int FirstHour = 7, LastHour = 21;

        public string HoverLabel => L.Get("cat.name");
        public Vector3Int Cell { get; private set; }
        public bool IsWalking => _route != null;
        public bool IsShown => _renderer != null && _renderer.enabled;

        FarmMap _map;
        GameSession _session;
        NpcManager _npcs;
        SpriteRenderer _renderer;
        System.Random _rng;
        List<(int x, int y)> _route;
        int _step;
        float _rest = 2f;
        float _nextMeow = 20f;
        float _frameClock;
        bool _placed;
        string _facing = CatSprites.Down;

        public void Init(FarmMap map, GameSession session, NpcManager npcs, int seed)
        {
            _map = map; _session = session; _npcs = npcs;
            _rng = new System.Random(seed);
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = 8;
            _renderer.enabled = false;
            name = "VillageCat";
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.8f, 0.8f);
        }

        // Where the cat sits at first: a walkable cell near the middle of the village.
        void Place(WalkGrid grid)
        {
            var b = _map.WorldBounds;
            var cx = Mathf.RoundToInt(b.center.x); var cy = Mathf.RoundToInt(b.center.y);
            for (var i = 0; i < 200; i++)
            {
                var x = cx + _rng.Next(-10, 11); var y = cy + _rng.Next(-8, 9);
                if (!grid.IsWalkable(x, y)) continue;
                Cell = new Vector3Int(x, y, 0);
                transform.position = _map.CellCenter(Cell);
                _placed = true;
                return;
            }
        }

        static bool Daytime(int hour) => hour >= FirstHour && hour < LastHour;

        void Update()
        {
            if (_session == null || !_session.InGame || _session.Clock.IsPaused) return;
            var grid = _npcs != null ? _npcs.Grid() : null;
            if (grid == null) return;
            if (!_placed) Place(grid);
            if (!_placed) return;

            _renderer.enabled = Daytime(_session.Clock.Now.MinuteOfDay / 60);
            if (!_renderer.enabled) { _route = null; return; }

            if (_route != null) Walk();
            else
            {
                _rest -= Time.deltaTime;
                if (_rest <= 0f)
                {
                    _route = CatWander.PickRoute(grid, Cell.x, Cell.y, _rng);
                    _step = 1;
                    _rest = 2f + (float)_rng.NextDouble() * 5f;
                }
            }
            Meow();
            Animate();
        }

        void Walk()
        {
            var target = _map.CellCenter(new Vector3Int(_route[_step].x, _route[_step].y, 0));
            var here = transform.position;
            var delta = target - here;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) _facing = delta.x < 0 ? CatSprites.Left : CatSprites.Right;
            else if (delta.sqrMagnitude > 0.0001f) _facing = delta.y < 0 ? CatSprites.Down : CatSprites.Up;
            transform.position = Vector3.MoveTowards(here, target, Speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude > 0.0001f) return;
            Cell = new Vector3Int(_route[_step].x, _route[_step].y, 0);
            if (++_step >= _route.Count) _route = null;
        }

        void Animate()
        {
            _frameClock += Time.deltaTime;
            var frames = CatSprites.Frames(_facing);
            _renderer.sprite = frames[IsWalking ? (int)(_frameClock / FrameTime) % 2 : 0];
        }

        // A meow only while the cat is in view and it is not raining, so a sound always has a cat to go with it.
        void Meow()
        {
            _nextMeow -= Time.deltaTime;
            if (_nextMeow > 0f) return;
            _nextMeow = Random.Range(40f, 90f);
            if (_session.State.Weather == WeatherIds.Rain || !OnScreen()) return;
            AudioService.PlayIfAvailable(Sfx.Meow, 0.35f, Random.Range(0.9f, 1.15f));
        }

        bool OnScreen()
        {
            var cam = Camera.main;
            if (cam == null) return false;
            var v = cam.WorldToViewportPoint(transform.position);
            return v.z > 0f && v.x > 0.02f && v.x < 0.98f && v.y > 0.02f && v.y < 0.98f;
        }

        public void Interact(PlayerActions player)
        {
            _route = null;
            _rest = 4f;
            AudioService.PlayIfAvailable(Sfx.Meow, 0.5f, Random.Range(0.95f, 1.1f));
            _session.Toast(L.Get("cat.pet." + _rng.Next(3)));
        }
    }
}
