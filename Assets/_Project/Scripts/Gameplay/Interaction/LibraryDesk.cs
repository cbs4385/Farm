using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The library's front desk, where Ione leaves the books she has set aside for the player (she says so in many places: on the first talk,
    // as a friend, after the pearl, in the birthday letter). Each book is handed over once, when its condition holds; the desk keeps the
    // library's opening hours.
    public sealed class LibraryDesk : MonoBehaviour, IInteractable
    {
        public int Reach => 1;
        public const string BookItemId = "prop.book";
        public const string TakenFlag = "library.desk_book_taken";          // the first book (kept for the dialogue conditions that read it)
        const string BusinessId = "library";

        public readonly struct Book
        {
            public readonly string Id, ItemId, Condition;
            public Book(string id, string itemId, string condition) { Id = id; ItemId = itemId; Condition = condition; }
            public string Flag => Id == "slim" ? TakenFlag : "library.desk." + Id;
        }

        // In the order they are handed over.
        public static readonly Book[] Books =
        {
            new Book("slim", BookItemId, null),
            new Book("almanac", "prop.almanac", "hearts:ione>=3"),
            new Book("fieldguide", "prop.fieldguide", "var:stat.foraged>=10"),
            new Book("longbook", "prop.longbook", "season:winter && hearts:ione>=2"),
            new Book("dullbook", "prop.dullbook", "flag:social.ione.tease.great"),
        };

        public string HoverLabel => L.Get("hover.library_desk");

        // The next book waiting for this player, or null. (pure apart from reading the game state)
        public static Book? NextWaiting(GameSession s)
        {
            foreach (var book in Books)
            {
                if (s.HasFlag(book.Flag)) continue;
                if (book.Condition == null || Conditions.Evaluate(book.Condition, s.World)) return book;
            }
            return null;
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!BusinessHoursRegistry.IsOpen(BusinessId, s.Clock.Now)) { s.Toast(BusinessHoursRegistry.ClosedMessage(BusinessId)); return; }
            var next = NextWaiting(s);
            if (next == null) { s.Toast(L.Get("library.desk.empty")); return; }
            var book = next.Value;
            if (!s.Backpack.CanAdd(book.ItemId, 1)) { s.Toast(L.Get("toast.inventory_full")); return; }
            s.Backpack.Add(book.ItemId, 1);
            s.SetFlag(book.Flag);
            s.Toast(L.Get("library.desk.book", L.Get("item." + book.ItemId + ".name")));
            AudioService.PlayIfAvailable(Sfx.Pickup);
            s.NotifyChanged();
        }
    }
}
