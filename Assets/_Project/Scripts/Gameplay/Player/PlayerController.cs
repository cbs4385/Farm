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
        readonly Footsteps _steps = new Footsteps();
        Vector3 _lastPosition;

        public Vector2Int Facing { get; private set; } = Vector2Int.down;

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
        }

        void LateUpdate()
        {
            var moved = (transform.position - _lastPosition).magnitude;
            _lastPosition = transform.position;
            // A teleport or a map change is not a walk.
            if (moved > 1f) { _steps.Walk(0f); return; }
            if (_steps.Walk(_move.sqrMagnitude > 0.01f ? moved : 0f)) AudioService.PlayIfAvailable(Sfx.Step, 0.35f, _steps.Pitch);
        }

        void Update()
        {
            _move = Vector2.zero;
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
