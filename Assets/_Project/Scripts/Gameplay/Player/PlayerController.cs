using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Top-down movement with four-way facing. transform.position is the player's feet (sprite pivot at bottom).
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] float _speed = 4.5f;
        [SerializeField] SpriteRenderer _renderer;
        [SerializeField] Sprite _down;
        [SerializeField] Sprite _up;
        [SerializeField] Sprite _left;
        [SerializeField] Sprite _right;

        Rigidbody2D _rb;
        Vector2 _move;
        SitSpot _seat;
        Vector3 _standFrom;
        int _normalOrder;
        float _restCarry;
        int _restMinute;
        readonly Footsteps _steps = new Footsteps();
        Vector3 _lastPosition;

        public Vector2Int Facing { get; private set; } = Vector2Int.down;

        public bool Seated => _seat != null;
        public SitSpot Seat => _seat;

        static int NowMinute() => ServiceLocator.TryGet<GameSession>(out var s) && s.InGame ? s.Clock.Now.TotalDays * 1440 + s.Clock.Now.MinuteOfDay : 0;

        // Sits the player down on a seat: they stop where the seat is, facing the way it does, and rest until they move.
        public bool Sit(SitSpot spot)
        {
            if (_seat != null || spot == null) return false;
            _seat = spot;
            spot.Occupant = this;
            _standFrom = transform.position;
            Stop();
            Teleport(spot.SeatPosition);
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;          // the seat is a solid thing: the player is on it, not against it
            if (_rb != null) _rb.simulated = false;
            Face(spot.Facing);
            if (_renderer != null) { _normalOrder = _renderer.sortingOrder; _renderer.sortingOrder = 12; }          // over a bench, which is drawn over the player
            _restMinute = NowMinute();
            _restCarry = 0f;
            return true;
        }

        // Stands up where the player was before sitting.
        public void StandUp()
        {
            if (_seat == null) return;
            _seat.Occupant = null;
            _seat = null;
            if (_rb != null) _rb.simulated = true;
            foreach (var c in GetComponents<Collider2D>()) c.enabled = true;
            Teleport(_standFrom);
            if (_renderer != null) _renderer.sortingOrder = _normalOrder;
        }

        // Each game minute spent sitting brings some energy back.
        void Rest()
        {
            var now = NowMinute();
            var minutes = now - _restMinute;
            if (minutes <= 0) return;
            _restMinute = now;
            var gain = SeatRest.Gain(minutes, ref _restCarry);
            if (gain > 0 && ServiceLocator.TryGet<GameSession>(out var session)) session.RestoreEnergy(gain);
        }

        // Cell the player stands in (sampled slightly above the feet so edges do not flicker).
        public Vector3 CellSamplePoint => transform.position + Vector3.up * 0.25f;

        public void Configure(SpriteRenderer renderer, Sprite down, Sprite up, Sprite left, Sprite right)
        {
            _renderer = renderer;
            _down = down; _up = up; _left = left; _right = right;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (!TryGetComponent<WalkBob>(out _)) gameObject.AddComponent<WalkBob>();
        }

        FarmMap _map;

        // The step sound for the ground under the player's feet.
        Sfx StepSound()
        {
            if (_map == null) _map = FindAnyObjectByType<FarmMap>();
            if (_map == null || _map.Ground == null) return Sfx.Step;
            var tile = _map.Ground.GetTile(_map.WorldToCell(transform.position));
            return FootstepSurface.For(tile != null ? tile.name : null);
        }

        void LateUpdate()
        {
            var moved = (transform.position - _lastPosition).magnitude;
            _lastPosition = transform.position;
            // A teleport or a map change is not a walk.
            if (moved > 1f) { _steps.Walk(0f); return; }
            if (_steps.Walk(_move.sqrMagnitude > 0.01f ? moved : 0f)) AudioService.PlayIfAvailable(StepSound(), 0.35f, _steps.Pitch);
        }

        void Update()
        {
            _move = Vector2.zero;
            if (_seat != null)
            {
                Rest();
                // Any push on the stick or the keys gets the player up (not while a menu is open: the keys are the menu's).
                if (ServiceLocator.TryGet<InputService>(out var seatedInput) && !seatedInput.GameplayBlocked && seatedInput.Move.ReadValue<Vector2>().sqrMagnitude > 0.25f) StandUp();
                return;
            }
            if (ServiceLocator.TryGet<InputService>(out var input))
            {
                var raw = input.Move.ReadValue<Vector2>();
                _move = Vector2.ClampMagnitude(raw, 1f);
            }

            if (_move.sqrMagnitude > 0.01f)
            {
                // Dominant axis decides facing; ties keep the current facing to avoid flicker on diagonals.
                if (Mathf.Abs(_move.x) > Mathf.Abs(_move.y) + 0.01f) Facing = _move.x > 0 ? Vector2Int.right : Vector2Int.left;
                else if (Mathf.Abs(_move.y) > Mathf.Abs(_move.x) + 0.01f) Facing = _move.y > 0 ? Vector2Int.up : Vector2Int.down;
                UpdateSprite();
            }
        }

        void FixedUpdate()
        {
            _rb.linearVelocity = _move * _speed;
        }

        // Dresses the farmer in a chosen look (the layered avatar sprites replace the plain placeholder ones).
        public void ApplyAvatar(AvatarData look)
        {
            var set = AvatarSprites.For(look);
            _down = set.Down; _up = set.Up; _left = set.Left; _right = set.Right;
            UpdateSprite();
        }

        public void Face(Vector2Int dir)
        {
            Facing = dir;
            UpdateSprite();
        }

        // Moves the player to a position without physics (cutscenes).
        public void Teleport(Vector3 position)
        {
            transform.position = position;
            if (_rb != null) { _rb.position = position; _rb.linearVelocity = Vector2.zero; }
        }

        public void Stop()
        {
            _move = Vector2.zero;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }

        void UpdateSprite()
        {
            if (_renderer == null) return;
            if (Facing == Vector2Int.up) _renderer.sprite = _up;
            else if (Facing == Vector2Int.left) _renderer.sprite = _left;
            else if (Facing == Vector2Int.right) _renderer.sprite = _right;
            else _renderer.sprite = _down;
        }
    }
}
