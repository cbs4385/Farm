using System.Collections.Generic;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Walking animation without extra art (playtest: "there are no animations"): while the thing it is on is moving, the picture hops one
    // pixel up and down in step. Works for anything that moves and shows a SpriteRenderer (the farmer, villagers, farm animals): it watches
    // the position, so it needs nothing from the code that does the moving, and the pixel-snapped hop keeps the pixel art crisp.
    public sealed class WalkBob : MonoBehaviour
    {
        public const float StepSeconds = 0.14f;
        const float MovingSpeed = 0.2f;            // world units per second that count as walking

        public const float LungeSeconds = 0.16f;
        public const float StrikeSeconds = 0.40f;             // a tool swing of four frames
        const int LungePixels = 2;

        static readonly Dictionary<(Sprite, int, int), Sprite> ShiftedOf = new Dictionary<(Sprite, int, int), Sprite>();
        static readonly Dictionary<Sprite, Sprite> BaseOfShifted = new Dictionary<Sprite, Sprite>();

        SpriteRenderer _renderer;
        Vector3 _last;
        float _clock;
        bool _movingNow;
        float _lungeLeft;
        float _strikeLeft;
        ToolType _strikeTool;
        Vector2Int _strikeTarget;
        SpriteRenderer _shadow;
        float _breathPhase;
        Vector2Int _lungeDir;

        public const float BreathSeconds = 1.8f;

        // Standing still, the picture rises a pixel for the second half of every breath. Off for the farmer, on for villagers and animals.
        public bool Breathes;

        // Walking, alternate a lifted left foot and a lifted right foot with the body's own hop (StepFrames). Off for things that have walking
        // pictures of their own (the cat, the farm animals).
        public bool StepsLegs = true;

        public bool NoShadow;                                    // a villager in bed has none

        public bool IsRaised { get; private set; }
        public int StepPhase { get; private set; }               // 0 left foot, 1 passing, 2 right foot, 3 passing (while walking)
        public bool IsLunging => _lungeLeft > 0f;
        public bool IsStriking => _strikeLeft > 0f;
        public ToolType StrikeTool => _strikeTool;
        public int StrikeFrame => _strikeLeft <= 0f ? -1 : Mathf.Min(CharacterRig.StrikeFrames - 1, (int)((1f - _strikeLeft / StrikeSeconds) * CharacterRig.StrikeFrames));

        // Swings a tool: the character's own four-frame swing with the tool in the hand (farmer and villagers; for the rest a lunge is all there is).
        public void Strike(ToolType tool) => Strike(tool, Vector2Int.zero);

        // `target`: the tile the swing is aimed at, in cells from the feet (x right, y up); the tool lands on it. Zero means the tile in front.
        public void Strike(ToolType tool, Vector2Int target)
        {
            _strikeTool = tool;
            _strikeTarget = target;
            _strikeLeft = StrikeSeconds;
        }

        // Is the picture up at this moment of a walk? (pure, so it can be tested)
        public static bool UpAt(float walkedSeconds) => walkedSeconds >= 0f && (int)(walkedSeconds / StepSeconds) % 2 == 1;

        // Is the picture up at this moment of a slow breath? (pure) `phase` keeps neighbours out of step.
        public static bool BreathUp(float time, float phase) => Mathf.Repeat(time + phase, BreathSeconds) >= BreathSeconds * 0.5f;

        // How many pixels a swing has pushed the picture forward this far (0..1) through it: out fast, back slower. (pure)
        public static int LungeOffset(float progress) => progress <= 0f || progress >= 1f ? 0 : progress < 0.4f ? LungePixels : 1;

        // The same picture one pixel higher: the sprite is re-made over the same texture with its pivot one pixel lower.
        public static Sprite Raised(Sprite sprite) => Shifted(sprite, 0, 1);

        // The same picture moved by whole pixels (re-made over the same texture with the pivot moved the other way).
        public static Sprite Shifted(Sprite sprite, int dx, int dy)
        {
            if (sprite == null) return null;
            if (BaseOfShifted.ContainsKey(sprite)) return sprite;
            if (dx == 0 && dy == 0) return sprite;
            var key = (sprite, dx, dy);
            if (ShiftedOf.TryGetValue(key, out var cached) && cached != null) return cached;
            var rect = sprite.textureRect;
            var pivot = new Vector2((sprite.pivot.x - dx) / rect.width, (sprite.pivot.y - dy) / rect.height);
            var shifted = Sprite.Create(sprite.texture, rect, pivot, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            shifted.name = sprite.name + "_up";
            ShiftedOf[key] = shifted;
            BaseOfShifted[shifted] = sprite;
            return shifted;
        }

        // The sprite that was set by the game, whether or not we have swapped in a shifted copy of it.
        static Sprite BaseOf(Sprite shown)
        {
            if (shown == null) return null;
            if (CharacterFrames.TryGetBase(shown, out var character)) return character;
            if (BaseOfShifted.TryGetValue(shown, out var original)) return original;
            return StepFrames.TryGetOriginal(shown, out var stepped) ? stepped : shown;
        }

        // A tool swing or a strike: the picture lunges a couple of pixels the way the farmer faces, then settles back.
        public void Lunge(Vector2Int direction)
        {
            _lungeDir = direction;
            _lungeLeft = LungeSeconds;
        }

        void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _last = transform.position;
            _breathPhase = Mathf.Abs(transform.position.x * 7.3f + transform.position.y * 3.1f) % BreathSeconds;
        }

        void LateUpdate()
        {
            if (_renderer == null) return;
            var dt = Time.deltaTime;
            var now = transform.position;
            var speed = dt > 0f ? (now - _last).magnitude / dt : 0f;
            _last = now;
            var moving = speed > MovingSpeed && speed < 12f;      // a teleport is not a walk
            if (moving) { _clock += dt; _movingNow = true; }
            else { _clock = 0f; _movingNow = false; }

            if (_shadow == null && _renderer.sprite != null) _shadow = ActorShadow.Add(gameObject, _renderer);          // once there is a picture to follow
            if (_shadow != null) { _shadow.enabled = !NoShadow && _renderer.enabled && _renderer.sprite != null; _shadow.sortingOrder = _renderer.sortingOrder - 1; }
            var shown = _renderer.sprite;
            var original = BaseOf(shown);
            if (_strikeLeft > 0f) _strikeLeft = Mathf.Max(0f, _strikeLeft - dt);

            // A character picture (the farmer, a villager): walks and swings with its own frames, feet on the ground; a pose only gets the look.
            if (CharacterFrames.IsCharacter(original))
            {
                var wantFrame = original;
                if (CharacterFrames.Animates(original))
                {
                    var rigStep = _movingNow ? (int)(_clock / StepSeconds) % 4 : 0;
                    StepPhase = rigStep;
                    var breath = !_movingNow && Breathes && BreathUp(Time.time, _breathPhase);
                    IsRaised = _movingNow ? rigStep % 2 == 1 : breath;
                    wantFrame = _strikeLeft > 0f ? CharacterFrames.Strike(original, _strikeTool, StrikeFrame, _strikeTarget)
                        : _movingNow ? CharacterFrames.Walk(original, rigStep) : CharacterFrames.Idle(original, breath);
                }
                else wantFrame = CharacterFrames.Looked(original);
                if (_lungeLeft > 0f) _lungeLeft = Mathf.Max(0f, _lungeLeft - dt);
                if (wantFrame != shown) _renderer.sprite = wantFrame;
                return;
            }
            var wantRaised = _movingNow && UpAt(_clock);
            if (!_movingNow && Breathes && _lungeLeft <= 0f) wantRaised = BreathUp(Time.time, _breathPhase);
            IsRaised = wantRaised;
            int dx = 0, dy = wantRaised ? 1 : 0;
            if (_lungeLeft > 0f)
            {
                _lungeLeft = Mathf.Max(0f, _lungeLeft - dt);
                var push = LungeOffset(1f - _lungeLeft / LungeSeconds);
                dx += _lungeDir.x * push; dy += _lungeDir.y * push;
            }
            var step = _movingNow ? (int)(_clock / StepSeconds) % 4 : 0;
            StepPhase = step;
            var legs = _movingNow && StepsLegs && _lungeLeft <= 0f && !wantRaised ? StepFrames.For(original) : null;
            var want = legs != null ? legs[step / 2] : Shifted(original, dx, dy);
            if (want != shown) _renderer.sprite = want;
        }
    }
}
