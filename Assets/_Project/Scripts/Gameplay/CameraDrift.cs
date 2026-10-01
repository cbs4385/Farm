using UnityEngine;

namespace Farm.Gameplay
{
    // Test helper for the PixelPerfectTest scene: slowly pans the camera so shimmer/jitter is easy to see.
    public sealed class CameraDrift : MonoBehaviour
    {
        [SerializeField] float _speed = 1.7f;
        [SerializeField] float _radius = 6f;

        Vector3 _origin;

        void Start() => _origin = transform.position;

        void Update()
        {
            var t = Time.time * _speed;
            transform.position = _origin + new Vector3(Mathf.Cos(t) * _radius, Mathf.Sin(t * 0.7f) * _radius * 0.5f, 0f);
        }
    }
}
