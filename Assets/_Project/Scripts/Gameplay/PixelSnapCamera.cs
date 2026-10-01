using UnityEngine;

namespace Farm.Gameplay
{
    // Snaps the camera to the art pixel grid (1/PPU world units) after all movement/follow logic has run.
    // URP's PixelPerfectCamera snaps sprites but not the camera, so a camera sitting on a sub-pixel position
    // at 2x+ upscale renders half-pixel offsets and shimmers. Keep this on every gameplay camera.
    [DefaultExecutionOrder(1000)]
    public sealed class PixelSnapCamera : MonoBehaviour
    {
        [SerializeField] int _pixelsPerUnit = 16;

        void LateUpdate()
        {
            var p = transform.position;
            p.x = Mathf.Round(p.x * _pixelsPerUnit) / _pixelsPerUnit;
            p.y = Mathf.Round(p.y * _pixelsPerUnit) / _pixelsPerUnit;
            transform.position = p;
        }
    }
}
