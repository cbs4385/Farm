using System;

namespace Farm.Gameplay
{
    // Where the player stood when a game was saved. The save carries it (GameState.ResumeMap/X/Y); the map that loads takes it once and puts the player there,
    // so a loaded game continues on the spot instead of at the map's door. Any other way into a map (a door, a bed, a warp) uses its named spawn point.
    public static class ResumePosition
    {
        public static void Record(GameState state, string mapId, float x, float y)
        {
            if (state == null || string.IsNullOrEmpty(mapId) || float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y)) { Clear(state); return; }
            state.ResumeMap = mapId;
            state.ResumeX = x;
            state.ResumeY = y;
        }

        public static void Clear(GameState state)
        {
            if (state == null) return;
            state.ResumeMap = null;
            state.ResumeX = 0f;
            state.ResumeY = 0f;
        }

        // The saved spot for this map, once: it is cleared whether or not it was for this map, so it can never put the player somewhere later.
        public static bool TryTake(GameState state, string mapId, out float x, out float y)
        {
            x = y = 0f;
            if (state == null || string.IsNullOrEmpty(state.ResumeMap)) return false;
            var matches = string.Equals(state.ResumeMap, mapId, StringComparison.Ordinal);
            if (matches) { x = state.ResumeX; y = state.ResumeY; }
            Clear(state);
            return matches;
        }
    }
}
