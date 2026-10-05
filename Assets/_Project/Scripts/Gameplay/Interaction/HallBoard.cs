using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The bundle board of the Community Hall: six rooms to restore by bringing things (see HallRooms).
    public sealed class HallBoard : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.hall_board");

        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowHall();
        }
    }
}
