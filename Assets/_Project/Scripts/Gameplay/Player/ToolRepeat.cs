using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Holding the tool button uses the tool again as soon as the last use is over (owner, 2026-10-15: "any tool should continue to be used until the action
    // button is released"). Armed by a press, it is due when the swing time has passed and the button is still down, and it stays due until something was done:
    // so when the aim moves to a cell where the tool has work, that cell is acted on at once. It lets go when the button is, or when the player walks or a menu opens.
    public sealed class ToolRepeat
    {
        public const float SwingSeconds = WalkBob.StrikeSeconds;          // the swing animation: holding never beats clicking at the best timing

        float _left;
        ToolType _tool;
        Vector3Int _cell;

        public bool Armed { get; private set; }

        // The rod and the hammer open their own modes (a cast and a minigame, building) and are not swung at a cell.
        public static bool Repeats(ToolType tool) => tool != ToolType.None && tool != ToolType.Rod && tool != ToolType.Hammer;

        public void Arm(ToolType tool, Vector3Int cell)
        {
            Armed = Repeats(tool);
            _tool = tool; _cell = cell; _left = SwingSeconds;
        }

        public void Cancel() => Armed = false;

        // Call each frame with the state of the player's input. Returns true when the next swing is due now.
        public bool Tick(float deltaTime, bool held, bool blocked, bool walking)
        {
            if (!Armed) return false;
            if (!held || blocked || walking) { Armed = false; return false; }
            _left -= deltaTime;
            return _left <= 0f;
        }

        public ToolType Tool => _tool;
        public Vector3Int Cell => _cell;
    }
}
