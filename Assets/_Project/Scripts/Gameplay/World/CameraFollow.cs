using UnityEngine;

namespace Farm.Gameplay
{
    // Follows the player, clamped to the map bounds. Runs before PixelSnapCamera (execution order 1000).
    // If the map is smaller than the view in an axis, the camera centres on the map in that axis.
    [DefaultExecutionOrder(900)]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform _target;
        Camera _camera;
        bool _hasBounds;
        Bounds _bounds;

        // Scene control (T-100): look at an actor or a point instead of the player, and shake.
        Transform _focusTransform;
        Vector3 _focusPoint;
        float _focusWeight, _focusGoal, _focusSpeed;
        float _shakeStrength, _shakeLeft, _shakeTotal;

        public bool IsFocused => _focusGoal > 0f;
        public float FocusWeight => _focusWeight;

        public void FocusOn(Transform target, float seconds)
        {
            _focusTransform = target;
            BeginFocus(seconds);
        }

        public void FocusOn(Vector3 point, float seconds)
        {
            _focusTransform = null;
            _focusPoint = point;
            BeginFocus(seconds);
        }

        void BeginFocus(float seconds)
        {
            _focusGoal = 1f;
            _focusSpeed = seconds <= 0.01f ? 1000f : 1f / seconds;
        }

        public void ReleaseFocus(float seconds)
        {
            _focusGoal = 0f;
            _focusSpeed = seconds <= 0.01f ? 1000f : 1f / seconds;
            if (seconds <= 0.01f) _focusWeight = 0f;
        }

        public void Shake(float strength, float seconds)
        {
            _shakeStrength = strength;
            _shakeTotal = _shakeLeft = Mathf.Max(0.01f, seconds);
        }

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
            _focusWeight = Mathf.MoveTowards(_focusWeight, _focusGoal, _focusSpeed * Time.deltaTime);
            if (_focusWeight > 0f)
            {
                var focus = (_focusTransform != null ? _focusTransform.position : _focusPoint) + Vector3.up * 0.5f;
                p = Vector3.Lerp(p, focus, Mathf.SmoothStep(0f, 1f, _focusWeight));
            }
            if (_hasBounds)
            {
                // The pixel-perfect camera changes the ortho size to fit the window, so read the real view size.
                if (_camera == null) _camera = GetComponent<Camera>();
                var halfHeight = _camera.orthographicSize;
                var halfWidth = halfHeight * _camera.aspect;
                p.x = Clamp(p.x, _bounds.min.x, _bounds.max.x, halfWidth);
                p.y = Clamp(p.y, _bounds.min.y, _bounds.max.y, halfHeight);
            }
            if (_shakeLeft > 0f)
            {
                _shakeLeft -= Time.deltaTime;
                var strength = _shakeStrength * Mathf.Clamp01(_shakeLeft / _shakeTotal);
                p.x += Mathf.Sin(Time.time * 61f) * strength;
                p.y += Mathf.Cos(Time.time * 47f) * strength;
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
