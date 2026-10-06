using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The library's front desk, where Ione leaves the book she has set aside for the player (she says so when first spoken to there).
    // It hands the book over once; after that the desk is just a desk.
    public sealed class LibraryDesk : MonoBehaviour, IInteractable
    {
        public const string BookItemId = "prop.book";
        public const string TakenFlag = "library.desk_book_taken";
        const string BusinessId = "library";

        public string HoverLabel => L.Get("hover.library_desk");

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!BusinessHoursRegistry.IsOpen(BusinessId, s.Clock.Now)) { s.Toast(BusinessHoursRegistry.ClosedMessage(BusinessId)); return; }
            if (s.HasFlag(TakenFlag)) { s.Toast(L.Get("library.desk.empty")); return; }
            if (!s.Backpack.CanAdd(BookItemId, 1)) { s.Toast(L.Get("toast.inventory_full")); return; }
            s.Backpack.Add(BookItemId, 1);
            s.SetFlag(TakenFlag);
            s.Toast(L.Get("library.desk.book"));
            s.NotifyChanged();
        }
    }
}
