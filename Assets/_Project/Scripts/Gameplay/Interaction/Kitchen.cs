using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The kitchen in the farmhouse: opens the cooking list (recipes of the kitchen station).
    public sealed class Kitchen : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.kitchen");

        public void Interact(PlayerActions player)
        {
            UiAccess.Run(ui => ui.ShowCrafting(Stations.Kitchen));
        }
    }
}
