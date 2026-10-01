using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public sealed class Bed : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            if (!ServiceLocator.TryGet<IUiService>(out var ui)) { player.Session.StartSleep(false); return; }
            ui.ShowConfirm("confirm.sleep", () => player.Session.StartSleep(false));
        }
    }
}
