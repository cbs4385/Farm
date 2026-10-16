using UnityEngine;

namespace Farm.Gameplay
{
    public static class PointerMath
    {
        // The pointer after the mouse moved by `delta` pixels, when the movement should count `sensitivity` times: the extra (or the part taken back) is added to where the system put it, kept on the screen. (pure)
        public static Vector2 Scale(Vector2 position, Vector2 delta, float sensitivity, Vector2 screen)
        {
            var extra = delta * (sensitivity - 1f);
            var scaled = position + extra;
            return new Vector2(Mathf.Clamp(scaled.x, 0f, Mathf.Max(0f, screen.x - 1f)), Mathf.Clamp(scaled.y, 0f, Mathf.Max(0f, screen.y - 1f)));
        }
    }
}
