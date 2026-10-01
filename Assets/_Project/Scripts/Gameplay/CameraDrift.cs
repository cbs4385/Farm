using UnityEngine;

namespace Farm.Gameplay
{
    // Test helper for the PixelPerfectTest scene: pans the camera back and forth at constant speed
    // so shimmer/judder is easy to see. Speed is in world units/second (16 art pixels per unit).
    public sealed class CameraDrift : MonoBehaviour
    {
        [SerializeField] float _speed = 5f;
        [SerializeField] float _halfRange = 8f;

        Vector3 _origin;

        void Start() => _origin = transform.position;

        void Update()
        {
            var x = Mathf.PingPong(Time.time * _speed, _halfRange * 2f) - _halfRange;
            transform.position = _origin + new Vector3(x, 0f, 0f);
        }
    }
}
