using UnityEngine;

namespace Farm.Gameplay
{
    // One monster on the screen: a sprite driven by an EnemyBrain. Positions are in cells (the mine's cell size is 1).
    public sealed class EnemyActor : MonoBehaviour
    {
        EnemyBrain _brain;
        EnemyManager _manager;
        SpriteRenderer _renderer;
        float _flash;

        public EnemyBrain Brain => _brain;
        public bool IsDead => _brain.Mode == EnemyMode.Dead;

        public void Setup(EnemyRow row, int seed, EnemyManager manager)
        {
            _brain = new EnemyBrain(row, seed);
            _manager = manager;
            name = "Enemy_" + row.Id;
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = RuntimeSprites.Square(row.Color, row.Boss ? 16 : 16, character: true);
            _renderer.sortingOrder = 8;
            transform.localScale = row.Boss ? new Vector3(2.5f, 2.5f, 1f) : Vector3.one;
        }

        public bool Hurt(int damage, Vector2 knock)
        {
            var killed = _brain.Hurt(damage);
            _flash = 0.15f;
            if (!killed) Move(knock);
            return killed;
        }

        void Update()
        {
            if (_brain == null || IsDead || !_manager.Active) return;
            var dt = Time.deltaTime;
            var step = _brain.Tick(dt, transform.position, _manager.PlayerPosition);
            if (step.Move != Vector2.zero) Move(step.Move * dt);
            if (step.Strike) _manager.StrikePlayer(_brain.Row.Damage);
            if (_flash > 0f)
            {
                _flash -= dt;
                _renderer.color = Color.Lerp(Color.white, Color.red, _flash > 0f ? 1f : 0f);
            }
            else _renderer.color = Color.white;
        }

        // Moves, stopping at walls (each axis on its own so enemies slide along them).
        void Move(Vector2 delta)
        {
            var p = (Vector2)transform.position;
            var tryX = p + new Vector2(delta.x, 0f);
            if (_manager.Walkable(tryX)) p = tryX;
            var tryY = p + new Vector2(0f, delta.y);
            if (_manager.Walkable(tryY)) p = tryY;
            transform.position = new Vector3(p.x, p.y, 0f);
        }
    }
}
