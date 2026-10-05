using UnityEngine;

namespace Farm.Gameplay
{
    // Aiming the tool square with the mouse (playtest feedback, 2026-10-04: steering it by facing alone was fiddly in small spaces). The
    // square sits on one of the nine cells in and around the player's own: the one the pointer is over (so the cell under the avatar can be
    // tilled too), or the nearest one in its direction when the pointer is further away. Pure, so it can be tested.
    public static class AimMath
    {
        // The target cell for a player standing in `playerCell` and a pointer over `pointerCell`: the pointer's cell, kept within one cell of the player.
        public static Vector3Int Target(Vector3Int playerCell, Vector3Int pointerCell)
        {
            var dx = Mathf.Clamp(pointerCell.x - playerCell.x, -1, 1);
            var dy = Mathf.Clamp(pointerCell.y - playerCell.y, -1, 1);
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
