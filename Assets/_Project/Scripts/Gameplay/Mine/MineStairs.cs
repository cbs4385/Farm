using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The ladder down, the way up, and the elevator on the first floor. Use them with the Interact button.
    public sealed class MineStairs : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.stairs");

        public enum StairKind { Down, Up, Elevator }

        public StairKind Kind;

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            switch (Kind)
            {
                case StairKind.Down:
                    MineTravel.GoToFloor(s, s.State.Mine.Floor + 1);
                    break;
                case StairKind.Up:
                    MineTravel.GoToFloor(s, s.State.Mine.Floor - 1, fromBelow: true);
                    break;
                case StairKind.Elevator:
                    UiAccess.Run(ui => ui.ShowElevator());
                    break;
            }
        }
    }

    public static class MineTravel
    {
        // Moves between floors by reloading the Mine scene (floor 0 leaves the mine for the forest).
        public const string FromBelowSpawn = "fromBelow";

        // Coming up from the floor below one arrives beside that floor's ladder down, not beside its way up.
        public static void GoToFloor(GameSession s, int floor, bool fromBelow = false)
        {
            if (floor <= 0)
            {
                s.State.Mine.Floor = 0;
                MapTravel.GoTo(MapIds.Forest, "fromMine");
                return;
            }
            floor = Mathf.Min(floor, MineGenerator.Floors);
            s.State.Mine.Floor = floor;
            if (floor > s.State.Mine.Deepest)
            {
                s.State.Mine.Deepest = floor;
                if (floor % 5 == 0) s.Toast(Farm.Core.L.Get("mine.elevator_unlocked", floor));
            }
            s.Toast(Farm.Core.L.Get("mine.floor", floor));
            MapTravel.GoTo(MapIds.Mine, fromBelow ? FromBelowSpawn : "default");
        }
    }
}
