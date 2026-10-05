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

        public AnimalState State => _state;
        public string Type => _state != null ? _state.Type : null;

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
            (TryGetComponent<WalkBob>(out var bob) ? bob : gameObject.AddComponent<WalkBob>()).Breathes = true;
            _renderer.sprite = RuntimeSprites.Square(row.Color, 16, character: true);
            _renderer.sortingOrder = 7;
            _target = transform.position;
        }

        void Update()
        {
            _renderer.color = _state.ProductReady ? new Color(1f, 1f, 0.7f) : Color.white;
            if (!_manager.Running) return;
            if (Vector3.Distance(transform.position, _target) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _target, Speed * Time.deltaTime);
                return;
            }
            _wait -= Time.deltaTime;
            if (_wait > 0f) return;
            _wait = 0.8f + Random.value * 2.5f;
            var dir = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right }[Random.Range(0, 4)];
            var next = Cell + dir;
            if (_manager.Free(next, this)) _target = _manager.Map.CellCenter(next);
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
