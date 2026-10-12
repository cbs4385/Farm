using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public sealed class Bed : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.bed");

        public void Interact(PlayerActions player)
        {
            var ui = player.Session.Ui;
            if (ui == null) { player.Session.StartSleep(false); return; }
            ui.ShowConfirm("confirm.sleep", () => player.Session.StartSleep(false));
        }
    }
}
