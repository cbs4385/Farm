using UnityEngine;

namespace Farm.Gameplay
{
    // Flips a world object between a few pictures on a timer (a flag in the wind, a weather vane). Frames are set by the map builder.
    public sealed class FrameAnimator : MonoBehaviour
    {
        [SerializeField] Sprite[] _frames;
        [SerializeField] float _seconds = 0.6f;
        [SerializeField] float _offset;
        SpriteRenderer _renderer;

        public void Configure(Sprite[] frames, float seconds, float offset)
        {
            _frames = frames; _seconds = seconds; _offset = offset;
        }

        public static int FrameAt(float time, float seconds, float offset, int count) =>
            count <= 0 || seconds <= 0f ? 0 : (int)Mathf.Floor((time + offset) / seconds) % count;

        void Awake() => _renderer = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (_renderer == null || _frames == null || _frames.Length == 0) return;
            var frame = _frames[FrameAt(Time.time, _seconds, _offset, _frames.Length)];
            if (frame != null && _renderer.sprite != frame) _renderer.sprite = frame;
        }
    }
}
