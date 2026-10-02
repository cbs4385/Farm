using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The help-wanted board in the village.
    public sealed class HelpWantedBoard : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowBoard();
        }
    }
}
