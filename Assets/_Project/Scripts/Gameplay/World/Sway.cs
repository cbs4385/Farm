using UnityEngine;

namespace Farm.Gameplay
{
    // The wind that plants lean in: a slow wave that travels across the map, so neighbours lean one after another instead of together.
    // Everything is whole pixels (-1, 0 or +1 at the top of a one-tile plant) to keep the pixel art crisp.
    public static class Sway
    {
        public const float Period = 1.6f;            // seconds for one full lean and back
        const float Threshold = 0.45f;               // below this the plant stands upright for a moment

        // How hard the wind blows in a weather: a breeze on a clear day, more with a slanting rain or wind, most in a thunderstorm. (pure)
        public static float StrengthFor(float windSlant, bool lightning) => Mathf.Clamp(0.8f + 1.2f * Mathf.Abs(windSlant) + (lightning ? 0.2f : 0f), 0.8f, 1.5f);

        // The wind in today's weather (a breeze when there is no game running).
        public static float Current()
        {
            if (!Farm.Core.ServiceLocator.TryGet<GameSession>(out var session) || !session.InGame) return 1f;
            var weather = session.Weather.Get(session.State.Weather);
            return weather == null ? 1f : StrengthFor(weather.WindSlant, weather.Lightning);
        }

        // How far the top of a plant at this cell leans, in pixels: -1, 0 or 1. `strength` 1 is a breeze; 0 is dead calm. (pure)
        public static int LeanPixels(float time, int x, int y, float strength = 1f)
        {
            if (strength <= 0f) return 0;
            var wave = Mathf.Sin(time * (2f * Mathf.PI / Period) + x * 0.55f + y * 0.35f) * Mathf.Min(strength, 1.5f);
            return wave > Threshold ? 1 : wave < -Threshold ? -1 : 0;
        }

        // A tile matrix that leans the top of the tile sideways by `pixels` while the bottom stays put (a shear about the tile's base).
        public static Matrix4x4 Shear(int pixels, float pixelsPerUnit = 16f)
        {
            var k = pixels / pixelsPerUnit;
            var m = Matrix4x4.identity;
            m.m01 = k;                // x grows with height
            m.m03 = k * 0.5f;         // tiles turn about their centre: shift so the base (half a tile down) does not move
            return m;
        }

        // The angle a scene object (a tree) tilts to, in degrees, for the same lean: a few degrees about its base.
        public static float AngleFor(int leanPixels) => -leanPixels * 2.5f;
    }
}
