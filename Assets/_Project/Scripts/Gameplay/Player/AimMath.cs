using UnityEngine;

namespace Farm.Gameplay
{
    // Aiming the tool square with the mouse (playtest feedback, 2026-10-04: steering it by facing alone was fiddly in small spaces). The
    // square sits on one of the eight cells around the player: the one the pointer is over, or the nearest one in its direction when the
    // pointer is further away. Pure, so it can be tested.
    public static class AimMath
    {
        // The target cell for a player standing in `playerCell`, a pointer over `pointerCell` and a facing direction.
        // Over the player's own cell there is no direction to aim in, so the facing decides.
        public static Vector3Int Target(Vector3Int playerCell, Vector3Int pointerCell, Vector2Int facing)
        {
            var dx = Mathf.Clamp(pointerCell.x - playerCell.x, -1, 1);
            var dy = Mathf.Clamp(pointerCell.y - playerCell.y, -1, 1);
            if (dx == 0 && dy == 0) return playerCell + new Vector3Int(facing.x, facing.y, 0);
            return playerCell + new Vector3Int(dx, dy, 0);
        }

        // The four-way facing that goes with a target, so the sprite looks at what the square is on (the longer axis wins; a tie keeps `current`).
        public static Vector2Int FacingToward(Vector3Int playerCell, Vector3Int target, Vector2Int current)
        {
            var dx = target.x - playerCell.x;
            var dy = target.y - playerCell.y;
            if (Mathf.Abs(dx) > Mathf.Abs(dy)) return dx > 0 ? Vector2Int.right : Vector2Int.left;
            if (Mathf.Abs(dy) > Mathf.Abs(dx)) return dy > 0 ? Vector2Int.up : Vector2Int.down;
            return current;
        }
    }
}
