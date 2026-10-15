using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Holding the tool button swings the axe or pickaxe again as soon as the last swing is over. Armed by a swing from a press, it
    // gives the next one when the swing time has passed and the button is still down, and lets go the moment anything changes.
    public sealed class ToolRepeat
    {
        public const float SwingSeconds = WalkBob.StrikeSeconds;          // the swing animation: holding never beats clicking at the best timing

        float _left;
        ToolType _tool;
        Vector3Int _cell;

        public bool Armed { get; private set; }

        public static bool Repeats(ToolType tool) => tool == ToolType.Axe || tool == ToolType.Pickaxe;

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
            if (_left > 0f) return false;
            _left = SwingSeconds;
            return true;
        }

        public ToolType Tool => _tool;
        public Vector3Int Cell => _cell;
    }
}
