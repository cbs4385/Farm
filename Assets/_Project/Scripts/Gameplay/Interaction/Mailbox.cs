using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The farm's mailbox: reads waiting letters one after another.
    public sealed class Mailbox : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.mailbox");

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var handed = Parcels.Claim(s);
            if (handed.Count > 0)
            {
                s.Toast(L.Get("mailbox.parcel", string.Join(", ", handed.Select(id => L.Get("item." + id + ".name")))));
                AudioService.PlayIfAvailable(Sfx.Pickup);
                s.NotifyChanged();
            }
            var letter = Mail.Next(s);
            if (letter == null)
            {
                if (handed.Count == 0) s.Toast(L.Get(Parcels.Count(s) > 0 ? "mailbox.parcel_full" : "mailbox.empty"));
                return;
            }
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowLetter(letter, () => { Mail.Finish(s, letter); });
        }
    }
}
