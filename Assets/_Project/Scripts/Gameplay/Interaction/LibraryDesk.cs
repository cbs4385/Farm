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

        public const int ReturnPoints = 15;       // a book handed back is worth a little with Ione, once for each book
        public static string ReturnedFlag(Book book) => "library.returned." + book.Id;

        public string HoverLabel => L.Get("hover.library_desk");

        // A borrowed book the player carries that has not been handed back yet, or null.
        public static Book? NextToReturn(GameSession s)
        {
            foreach (var book in Books)
                if (s.HasFlag(book.Flag) && !s.HasFlag(ReturnedFlag(book)) && s.Backpack.Has(book.ItemId)) return book;
            return null;
        }

        // Takes the book back and thanks the player with friendship. False when there is none to return.
        public static bool TryReturn(GameSession s)
        {
            var back = NextToReturn(s);
            if (back == null) return false;
            var book = back.Value;
            s.Backpack.Remove(book.ItemId, 1);
            s.SetFlag(ReturnedFlag(book));
            NpcInteractions.AddPoints(s, NpcIds.Ione, ReturnPoints);
            s.Toast(L.Get("library.desk.returned", L.Get("item." + book.ItemId + ".name")));
            AudioService.PlayIfAvailable(Sfx.Pickup);
            s.NotifyChanged();
            return true;
        }

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
            if (NextToReturn(s) != null)
            {
                // A book Ione lent can be one the player wants to keep reading (or one a story needs), and the same button that collects a book also hands it back: ask first.
                var unread = NextToReturn(s) is Book due && !BookReading.HasRead(s, due);          // one never opened is asked about with a plainer warning
                if (s.Ui is IUiService ui) ui.ShowConfirm(unread ? "library.desk.confirm_return_unread" : "library.desk.confirm_return", () => TryReturn(s));
                else TryReturn(s);
                return;
            }
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
