using System.Collections.Generic;
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

        static readonly Dictionary<Sprite, Sprite> RaisedOf = new Dictionary<Sprite, Sprite>();
        static readonly HashSet<Sprite> AreRaised = new HashSet<Sprite>();

        SpriteRenderer _renderer;
        Vector3 _last;
        float _clock;
        bool _movingNow;

        public bool IsRaised { get; private set; }

        // Is the picture up at this moment of a walk? (pure, so it can be tested)
        public static bool UpAt(float walkedSeconds) => walkedSeconds >= 0f && (int)(walkedSeconds / StepSeconds) % 2 == 1;

        // The same picture one pixel higher: the sprite is re-made over the same texture with its pivot one pixel lower.
        public static Sprite Raised(Sprite sprite)
        {
            if (sprite == null) return null;
            if (AreRaised.Contains(sprite)) return sprite;
            if (RaisedOf.TryGetValue(sprite, out var cached) && cached != null) return cached;
            var rect = sprite.textureRect;
            var pivot = new Vector2(sprite.pivot.x / rect.width, (sprite.pivot.y - 1f) / rect.height);
            var raised = Sprite.Create(sprite.texture, rect, pivot, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            raised.name = sprite.name + "_up";
            RaisedOf[sprite] = raised;
            AreRaised.Add(raised);
            return raised;
        }

        // The sprite that was set by the game, whether or not we have swapped in the raised copy of it.
        static Sprite BaseOf(Sprite shown)
        {
            if (shown == null || !AreRaised.Contains(shown)) return shown;
            foreach (var pair in RaisedOf) if (pair.Value == shown) return pair.Key;
            return shown;
        }

        void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _last = transform.position;
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

            var shown = _renderer.sprite;
            var original = BaseOf(shown);
            var wantRaised = _movingNow && UpAt(_clock);
            IsRaised = wantRaised;
            var want = wantRaised ? Raised(original) : original;
            if (want != shown) _renderer.sprite = want;
        }
    }
}
