using UnityEngine;

namespace Farm.Gameplay
{
    // One monster on the screen: a sprite driven by an EnemyBrain. Positions are in cells (the mine's cell size is 1).
    public sealed class EnemyActor : MonoBehaviour
    {
        public const float FlashSeconds = 0.12f;
        public const float BarSeconds = 3f;               // how long the health bar stays up after a hit
        const float BarWidth = 0.8f;

        EnemyBrain _brain;
        EnemyManager _manager;
        SpriteRenderer _renderer;
        Sprite[] _art;
        float _flash;
        float _frameOffset;
        float _barLeft;
        Transform _bar;
        Transform _barFill;

        public EnemyBrain Brain => _brain;
        public bool IsDead => _brain.Mode == EnemyMode.Dead;
        public bool HealthBarShown => _bar != null && _bar.gameObject.activeSelf;

        public void Setup(EnemyRow row, int seed, EnemyManager manager)
        {
            _brain = new EnemyBrain(row, seed);
            _manager = manager;
            name = "Enemy_" + row.Id;
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.color = Color.white;
            _art = EnemySprites.Frames(row.Id);
            _renderer.sprite = _art != null ? _art[0] : RuntimeSprites.Square(row.Color, 16, character: true);
            _renderer.sortingOrder = 8;
            _frameOffset = (seed & 7) * 0.11f;
            // The boss picture is drawn 20 pixels wide and shown at twice the size (2.5 cells).
            transform.localScale = row.Boss ? (_art != null ? new Vector3(2f, 2f, 1f) : new Vector3(2.5f, 2.5f, 1f)) : Vector3.one;
            BuildBar();
        }

        // A small health bar over the monster's head, shown for a few seconds after it is hit.
        void BuildBar()
        {
            if (_bar != null) Destroy(_bar.gameObject);
            var scale = transform.localScale.x;
            var height = (_art != null ? _art[0].rect.height / 16f : 1f) * 0.5f + 0.1f / scale;      // in the picture's own (scaled) space
            _bar = new GameObject("HealthBar").transform;
            _bar.SetParent(transform, false);
            _bar.localPosition = new Vector3(0f, height, 0f);
            var back = Piece("Back", new Color(0.1f, 0.05f, 0.05f, 0.9f), BarWidth + 0.1f, 0.2f, -BarWidth / 2f - 0.05f, 20);
            back.SetParent(_bar, false);
            _barFill = Piece("Fill", new Color(0.85f, 0.2f, 0.2f, 1f), BarWidth, 0.1f, -BarWidth / 2f, 21);
            _barFill.SetParent(_bar, false);
            _bar.localScale = Vector3.one / scale;           // the boss is drawn big; its bar is not
            _bar.gameObject.SetActive(false);
        }

        static Transform Piece(string name, Color color, float width, float height, float left, int order)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EnemySprites.Pixel();
            sr.color = color;
            sr.sortingOrder = order;
            go.transform.localPosition = new Vector3(left, 0f, 0f);
            go.transform.localScale = new Vector3(width, height, 1f);
            return go.transform;
        }

        public bool Hurt(int damage, Vector2 knock)
        {
            var killed = _brain.Hurt(damage);
            _flash = FlashSeconds;
            ActionPuff.Burst(transform.position, new Color(1f, 0.95f, 0.7f), killed ? 5 : 2, Fx.HitStar);
            AudioService.PlayIfAvailable(Sfx.Hit, 0.5f, Random.Range(1.1f, 1.4f));
            if (!killed)
            {
                Move(knock);
                _barLeft = BarSeconds;
                ShowBar();
            }
            return killed;
        }

        void ShowBar()
        {
            if (_bar == null) return;
            _bar.gameObject.SetActive(true);
            var ratio = Mathf.Clamp01(_brain.Health / (float)_brain.Row.Hp);
            _barFill.localScale = new Vector3(BarWidth * ratio, 0.1f, 1f);
        }

        void Update()
        {
            if (_brain == null || IsDead || !_manager.Active) return;
            var dt = Time.deltaTime;
            var step = _brain.Tick(dt, transform.position, _manager.PlayerPosition);
            if (step.Move != Vector2.zero) Move(step.Move * dt);
            if (step.Strike) _manager.StrikePlayer(_brain.Row.Damage);

            if (_flash > 0f) _flash -= dt;
            if (_art != null) _renderer.sprite = _flash > 0f ? _art[2] : _art[EnemySprites.FrameAt(Time.time, _frameOffset)];
            else _renderer.color = _flash > 0f ? Color.red : Color.white;

            if (_barLeft > 0f)
            {
                _barLeft -= dt;
                if (_barLeft <= 0f) _bar.gameObject.SetActive(false);
            }
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
