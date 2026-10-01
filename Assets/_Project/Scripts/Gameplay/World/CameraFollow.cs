using UnityEngine;

namespace Farm.Gameplay
{
    // Follows the player, clamped to the map bounds. Runs before PixelSnapCamera (execution order 1000).
    // If the map is smaller than the view in an axis, the camera centres on the map in that axis.
    [DefaultExecutionOrder(900)]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform _target;
        [SerializeField] float _viewHalfWidth = 15f;    // 480 / 16 / 2
        [SerializeField] float _viewHalfHeight = 8.4375f; // 270 / 16 / 2

        bool _hasBounds;
        Bounds _bounds;

        public void SetTarget(Transform target) => _target = target;

        public void SetBounds(Bounds bounds)
        {
            _bounds = bounds;
            _hasBounds = true;
        }

        public void Snap()
        {
            LateUpdate();
        }

        void LateUpdate()
        {
            if (_target == null) return;
            var p = _target.position + Vector3.up * 0.5f;
            if (_hasBounds)
            {
                p.x = Clamp(p.x, _bounds.min.x, _bounds.max.x, _viewHalfWidth);
                p.y = Clamp(p.y, _bounds.min.y, _bounds.max.y, _viewHalfHeight);
            }
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }

        static float Clamp(float value, float min, float max, float halfView)
        {
            if (max - min <= halfView * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + halfView, max - halfView);
        }
    }
}
