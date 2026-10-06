using System.Collections.Generic;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The black cat that ambles about the village by day. It walks short hops between nearby cells, rests between them, meows now and then
    // while it is on screen (so a meow always has a cat to go with it), and can be petted.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VillageCat : MonoBehaviour, IInteractable, ICellOccupant
    {
        const float Speed = 1.4f;
        const float FrameTime = 0.2f;
        const int FirstHour = 7, LastHour = 21;
        public const float PurrSeconds = 1.6f;
        public const float HopEvery = 0.4f;                   // while it purrs the cat hops this often, and hearts rise with every second hop

        // How many hops fall between two moments of a purr (pure).
        public static int HopsBetween(float from, float to) => Mathf.FloorToInt(to / HopEvery) - Mathf.FloorToInt(from / HopEvery);

        public string HoverLabel => L.Get("cat.name");
        public Vector3Int Cell { get; private set; }
        public bool IsPlaced => _placed && _renderer != null && _renderer.enabled;
        public Vector3Int? Claim => _route != null && _step < _route.Count ? new Vector3Int(_route[_step].x, _route[_step].y, 0) : (Vector3Int?)null;

        void OnEnable() => CellOccupants.Add(this);
        void OnDisable() => CellOccupants.Remove(this);
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
        WalkBob _bob;
        float _purr;                                          // seconds of purring left after being petted
        int _hops;

        public bool IsPurring => _purr > 0f;

        public void Init(FarmMap map, GameSession session, NpcManager npcs, int seed)
        {
            _map = map; _session = session; _npcs = npcs;
            _rng = new System.Random(seed);
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = 8;
            _renderer.enabled = false;
            name = "VillageCat";
            _bob = gameObject.AddComponent<WalkBob>();
            _bob.Breathes = true;
            _bob.StepsLegs = false;
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.8f, 0.8f);
            ActorBody.Add(gameObject);
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

            if (CellOccupants.SharingWith(this) > 0 && _purr <= 0f) StepAside(grid);
            if (_route != null) Walk();
            else
            {
                _rest -= Time.deltaTime;
                if (_rest <= 0f)
                {
                    _route = CatWander.PickRoute(grid, Cell.x, Cell.y, _rng, occupied: (x, y) => CellOccupants.IsTaken(new Vector3Int(x, y, 0), this));      // around anyone in the way
                    _step = 1;
                    _rest = 2f + (float)_rng.NextDouble() * 5f;
                }
            }
            Meow();
            Purr();
            Animate();
        }

        // After a pet the cat bounces on the spot and hearts rise, for a moment.
        void Purr()
        {
            if (_purr <= 0f) return;
            var before = PurrSeconds - _purr;
            _purr = Mathf.Max(0f, _purr - Time.deltaTime);
            var hops = HopsBetween(before, PurrSeconds - _purr);
            for (var i = 0; i < hops; i++)
            {
                _bob.Lunge(Vector2Int.up);
                if (++_hops % 2 == 0) ActionPuff.Hearts(transform.position + Vector3.up * 0.6f);
            }
        }

        // A villager walked onto the cat's cell: it hops to the nearest free one.
        void StepAside(WalkGrid grid)
        {
            if (!CellOccupants.TryFindFree(Cell, c => grid.IsWalkable(c.x, c.y), this, out var free)) return;
            _route = new List<(int x, int y)> { (Cell.x, Cell.y), (free.x, free.y) };
            _step = 1;
        }

        void Walk()
        {
            var next = new Vector3Int(_route[_step].x, _route[_step].y, 0);
            if (next != Cell && CellOccupants.IsTaken(next, this))            // somebody is in the way: wait and try another time
            {
                _route = null;
                _rest = 1f;
                return;
            }
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
            _purr = PurrSeconds;
            _hops = 0;
            if (player != null)                                   // it turns to face whoever is petting it
            {
                var toPlayer = player.transform.position - transform.position;
                _facing = Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y) ? (toPlayer.x < 0 ? CatSprites.Left : CatSprites.Right) : (toPlayer.y < 0 ? CatSprites.Down : CatSprites.Up);
            }
            ActionPuff.Hearts(transform.position + Vector3.up * 0.6f, _bob);
            AudioService.PlayIfAvailable(Sfx.Meow, 0.5f, Random.Range(0.95f, 1.1f));
            _session.Toast(L.Get("cat.pet." + _rng.Next(3)));
        }
    }
}
