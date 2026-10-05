using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A farm animal in its coop or barn: wanders about; Interact pets it, or collects its product when ready.
    public sealed class AnimalActor : MonoBehaviour, IInteractable
    {
        public string HoverLabel => _state != null ? _state.Name : null;

        const float Speed = 1.2f;
        AnimalState _state;
        AnimalManager _manager;
        SpriteRenderer _renderer;
        Vector3 _target;
        float _wait;
        Vector2Int _facing = Vector2Int.down;
        float _eatLeft;
        float _idleLeft;
        float _nextIdle = 3f;
        int _idle;
        bool _moving;
        bool _hasArt;

        public AnimalState State => _state;
        public string Type => _state != null ? _state.Type : null;
        public bool IsEating => _eatLeft > 0f;
        public int IdleAction => _idleLeft > 0f ? _idle : AnimalSprites.NoIdle;
        public bool IsAsleep => _manager != null && _manager.Night;
        public string ShownPicture { get; private set; }

        // Fed: it faces the front and munches for a few seconds.
        public void Eat(float seconds)
        {
            _eatLeft = seconds;
            _facing = Vector2Int.down;
        }

        // The animal's call (the species decides which; a rabbit is silent).
        public void Call()
        {
            var sfx = AnimalCalls.For(Type);
            if (sfx.HasValue) AudioService.PlayIfAvailable(sfx.Value, 0.7f, 0.95f + (Mathf.Abs(NpcInteractions.StableHash(_state.Id)) % 10) * 0.01f);
        }

        public Vector3Int Cell => _manager.Map.WorldToCell(transform.position);

        public void Setup(AnimalState state, AnimalManager manager)
        {
            _state = state;
            _manager = manager;
            name = "Animal_" + state.Id;
            var row = AnimalDefaults.Row(state.Type);
            _renderer = GetComponent<SpriteRenderer>();
            var bob = TryGetComponent<WalkBob>(out var existing) ? existing : gameObject.AddComponent<WalkBob>();
            bob.Breathes = true;
            bob.StepsLegs = false;                      // its walking pictures are its own
            // The animal's own picture (the art made for its item); a coloured square only if there is none.
            _hasArt = AnimalSprites.Has(state.Type);
            var icon = ServiceLocator.TryGet<GameSession>(out var session) && session.Db.TryGetItem(row.ItemId, out var item) ? item.Icon : null;
            _renderer.sprite = _hasArt ? AnimalSprites.Get(state.Type, "down0") : icon != null ? icon : RuntimeSprites.Square(row.Color, 16, character: true);
            _renderer.sortingOrder = 7;
            _target = transform.position;
        }

        void Update()
        {
            _renderer.color = _state.ProductReady ? new Color(1f, 1f, 0.7f) : Color.white;
            var running = _manager.Running;
            if (running && _eatLeft > 0f) _eatLeft = Mathf.Max(0f, _eatLeft - Time.deltaTime);
            _moving = false;
            if (_idleLeft > 0f && running) _idleLeft -= Time.deltaTime;
            if (running && Vector3.Distance(transform.position, _target) > 0.02f)       // a step in progress is finished first
            {
                transform.position = Vector3.MoveTowards(transform.position, _target, Speed * Time.deltaTime);
                _moving = true;
            }
            else if (running && !IsAsleep && _eatLeft <= 0f) Wander();
            ShowPicture();
        }

        void Wander()
        {
            _nextIdle -= Time.deltaTime;
            if (_nextIdle <= 0f && _idleLeft <= 0f)                       // now and then it grazes, flaps or fidgets where it stands
            {
                _idle = Random.value < 0.5f ? AnimalSprites.Graze : AnimalSprites.Fidget;
                _idleLeft = 1.4f + Random.value * 1.4f;
                _nextIdle = 4f + Random.value * 6f;
                _facing = Vector2Int.down;
            }
            if (_idleLeft > 0f) return;
            _wait -= Time.deltaTime;
            if (_wait > 0f) return;
            _wait = 0.8f + Random.value * 2.5f;
            var dir = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right }[Random.Range(0, 4)];
            _facing = new Vector2Int(dir.x, dir.y);
            var next = Cell + dir;
            if (_manager.Free(next, this)) _target = _manager.Map.CellCenter(next);
        }

        // The picture for what it is doing: walking the way it faces, standing, munching, or asleep.
        void ShowPicture()
        {
            if (!_hasArt) return;
            var (key, flip) = AnimalSprites.Pick(_facing, _moving, IsEating, IsAsleep, Time.time, IdleAction);
            ShownPicture = key;
            _renderer.sprite = AnimalSprites.Get(_state.Type, key);
            _renderer.flipX = flip;
        }

        public void Interact(PlayerActions player)
        {
            Call();
            var s = player.Session;
            if (AnimalRules.Collect(_state, s.Backpack, out var product))
            {
                s.Toast(L.Get("animal.collected", s.Db.TryGetItem(product, out var item) ? L.Get(item.NameKey) : product));
                s.NotifyChanged();
                return;
            }
            if (_state.ProductReady) { s.Toast(L.Get("toast.inventory_full")); return; }
            var petted = AnimalRules.Pet(_state);
            s.Toast(petted ? L.Get("animal.petted", _state.Name) : L.Get("animal.happy", _state.Name));
            if (petted) ActionPuff.Hearts(transform.position + Vector3.up * 0.7f, GetComponent<WalkBob>());
        }
    }
}
