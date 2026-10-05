using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The help-wanted board in the village.
    public sealed class HelpWantedBoard : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.help_board");

        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowBoard();
        }
    }
}
