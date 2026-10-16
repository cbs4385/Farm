using UnityEngine;

namespace Farm.Gameplay
{
    // One leaf of an interior doorway (the doors between the farmhouse's rooms; playtest 2026-10-16: "the interior doors do not open or close when the player moves through
    // them"). A doorway two cells wide has two leaves, like a pair of doors: when the farmer or a villager is within a cell or so, each leaf swings back toward its own end of the
    // doorway, and when they are gone it swings shut. A leaf has no collider (the doorway is always open to walk through); it only draws the door.
    public sealed class DoorSwing : MonoBehaviour
    {
        public const float OpenScale = 0.2f;          // how much of the cell the leaf covers once swung back
        public const float Reach = 3f;                // the square round the leaf (in cells) in which someone makes it open
        const float Interval = 0.1f;

        [SerializeField] Vector2 _hinge = Vector2.left;          // from the leaf's cell towards the end of the doorway it folds to (a unit step along the wall)

        static readonly Collider2D[] Buffer = new Collider2D[24];
        Vector3 _home, _homeWorld, _size;
        float _openness, _target, _next;
        bool _ready;

        public float Openness => _openness;

        public void Configure(Vector2 hinge) => _hinge = hinge;

        // How far open a leaf is after `dt` seconds of heading for `target` (0 shut, 1 open), at a speed that opens it in about a fifth of a second. (pure)
        public static float Approach(float openness, float target, float dt) => Mathf.MoveTowards(openness, target, dt * 5f);

        // The scale and shift of a leaf at an openness: it shrinks along the wall and slides to its end. (pure)
        public static (float scale, float shift) Pose(float openness) => (Mathf.Lerp(1f, OpenScale, openness), openness * (1f - OpenScale) * 0.5f);

        void Update()
        {
            if (!_ready) { _home = transform.localPosition; _homeWorld = transform.position; _size = transform.localScale; _ready = true; }
            if (Time.time >= _next)
            {
                _next = Time.time + Interval;
                _target = Occupied() ? 1f : 0f;
            }
            var before = _openness;
            _openness = Approach(_openness, _target, Time.deltaTime);
            if (!Mathf.Approximately(before, _openness)) Apply();
        }

        void Apply()
        {
            var (scale, shift) = Pose(_openness);
            var alongX = Mathf.Abs(_hinge.x) > Mathf.Abs(_hinge.y);
            transform.localScale = new Vector3(alongX ? _size.x * scale : _size.x, alongX ? _size.y : _size.y * scale, _size.z);
            transform.localPosition = _home + new Vector3(_hinge.x * shift, _hinge.y * shift, 0f);
        }

        // Is the farmer or a villager standing close to this leaf?
        bool Occupied()
        {
            var filter = new ContactFilter2D { useTriggers = false };
            var count = Physics2D.OverlapBox((Vector2)_homeWorld, new Vector2(Reach, Reach), 0f, filter, Buffer);
            for (var i = 0; i < count; i++)
            {
                var c = Buffer[i];
                if (c != null && (c.GetComponentInParent<PlayerController>() != null || c.GetComponentInParent<NpcActor>() != null)) return true;
            }
            return false;
        }
    }
}
