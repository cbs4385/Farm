using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Top-down movement with four-way facing. transform.position is the player's feet (sprite pivot at bottom).
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour, IMapBound
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
        readonly RunModel _run = new RunModel();
        float _runFactor = 1f;
        public bool Running => _runFactor > 1f;

        public Vector2Int Facing { get; private set; } = Vector2Int.down;

        // Dancing at a festival: the farmer stays where they are and dances (WalkBob's dance frames) for a few seconds, turning to the four sides, until they have finished or
        // the stick is pushed. `onDone` runs only when the dance was danced to the end.
        float _danceLeft;
        System.Action _danceDone;
        public bool Dancing => _danceLeft > 0f;

        public bool StartDance(float seconds, System.Action onDone = null)
        {
            if (_seat != null || Dancing || seconds <= 0f) return false;
            Stop();
            _danceLeft = seconds;
            _danceDone = onDone;
            if (TryGetComponent<WalkBob>(out var bob)) bob.Dancing = true;
            return true;
        }

        void EndDance(bool completed)
        {
            _danceLeft = 0f;
            if (TryGetComponent<WalkBob>(out var bob)) bob.Dancing = false;
            var done = _danceDone;
            _danceDone = null;
            if (completed) done?.Invoke();
        }

        // Reading a borrowed book (BookReading): the farmer stays where they are, facing the screen, with the book held up, until the book is done or they move or press a button.
        BookReading.Session _reading;
        ReadingBook _readingBook;
        int _readingFrame;
        public bool Reading => _reading != null && _reading.Active;
        public BookReading.Session ReadingSession => _reading;

        public bool StartReading(LibraryDesk.Book book, Sprite icon)
        {
            if (_session == null || _seat != null || Dancing || Reading || !BookReading.CanStart(_session)) return false;
            Stop();
            Face(Vector2Int.down);
            _readingFrame = Time.frameCount;
            _reading = new BookReading.Session(_session, _session.Bus, book);
            _reading.Ended += OnReadingEnded;
            _readingBook = ReadingBook.Create(transform, icon, _renderer != null ? _renderer.sortingOrder + 1 : 13);
            return true;
        }

        public void StopReading() => _reading?.Cancel();

        void OnReadingEnded(BookReading.Session ended)
        {
            ended.Ended -= OnReadingEnded;
            if (_readingBook != null) Destroy(_readingBook.gameObject);
            _readingBook = null;
        }

        void OnDestroy()
        {
            if (_reading != null) { _reading.Ended -= OnReadingEnded; _reading.Dispose(); }          // the clock never stays fast after the player is gone
        }

        public bool Seated => _seat != null;
        public SitSpot Seat => _seat;

        GameSession _session;
        InputService _input;

        public void Bind(MapContext map)
        {
            _session = map.Session;
            _input = map.Input;
        }

        int NowMinute() => _session != null && _session.InGame ? _session.Clock.Now.TotalDays * 1440 + _session.Clock.Now.MinuteOfDay : 0;

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
            if (gain > 0 && _session != null) _session.RestoreEnergy(gain);
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
            if (_danceLeft > 0f)
            {
                _danceLeft -= Time.deltaTime;
                var stopped = _input != null && !_input.GameplayBlocked && _input.Move.ReadValue<Vector2>().sqrMagnitude > 0.25f;
                if (stopped) EndDance(false);
                else if (_danceLeft <= 0f) EndDance(true);
                else
                {
                    var turn = (int)(Time.time / 1.1f) % 4;
                    var side = turn == 0 || turn == 2 ? Vector2Int.down : turn == 1 ? Vector2Int.left : Vector2Int.right;
                    if (side != Facing) Face(side);
                    return;
                }
            }
            if (Reading)
            {
                var pushed = _input != null && !_input.GameplayBlocked && (_input.Move.ReadValue<Vector2>().sqrMagnitude > 0.25f
                    || (Time.frameCount != _readingFrame && (_input.UseTool.WasPressedThisFrame() || _input.Interact.WasPressedThisFrame())));
                if (pushed || (_input != null && _input.GameplayBlocked)) _reading.Cancel();
                else return;
            }
            if (_seat != null)
            {
                Rest();
                // Any push on the stick or the keys gets the player up (not while a menu is open: the keys are the menu's).
                if (_input != null && !_input.GameplayBlocked && _input.Move.ReadValue<Vector2>().sqrMagnitude > 0.25f) StandUp();
                return;
            }
            var input = _input;
            if (input != null)
            {
                var raw = input.Move.ReadValue<Vector2>();
                var settings = _session != null ? _session.Settings : null;
                if (settings != null && input.Hints.Kind == InputKind.Gamepad) raw = RunModel.ApplyStickSensitivity(raw, settings.ControllerSensitivity);
                _move = Vector2.ClampMagnitude(raw, 1f);
            }

            var moving = _move.sqrMagnitude > 0.01f;
            _runFactor = 1f;
            if (input != null && _session != null && _session.InGame)
            {
                _runFactor = _run.Step(input.Run.IsPressed(), moving, _session.State.Energy, Time.deltaTime, _session.Settings != null && _session.Settings.RelaxedEnergy, out var cost);
                if (cost > 0) _session.TrySpendEnergy(cost);
            }
            else _run.Reset();

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
            _rb.linearVelocity = _move * (_speed * _runFactor);
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
            _runFactor = 1f;
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
