using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The farm's mailbox: reads waiting letters one after another.
    public sealed class Mailbox : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var letter = Mail.Next(s);
            if (letter == null) { s.Toast(L.Get("mailbox.empty")); return; }
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowLetter(letter, () => { Mail.Finish(s, letter); });
        }
    }

    // The help-wanted board in the village.
    public sealed class HelpWantedBoard : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowBoard();
        }
    }
}
