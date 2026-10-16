using System;
using Farm.Core;

namespace Farm.Gameplay
{
    // Reading a borrowed book (owner, 2026-10-16). It takes game time: the book's own number of minutes goes by while the farmer sits with it, at the fastest clock speed, and
    // stops when the farmer moves or presses a button. A finished book is remembered by a flag ("book.read.<id>"), which a villager's conversation, a gift and the library desk read.
    // The model is plain C#; PlayerController plays the pose and ReadingBook draws the book.
    public static class BookReading
    {
        public const float FastSpeed = 12f;                     // the clock runs this many times faster while reading (a ten-minute step takes under a second)
        public const string ReadPrefix = "book.read.";
        public const string DiscussedPrefix = "book.talked.";   // "book.talked.<npc>.<book>"
        public const string ReadStat = "stat.books_read";
        public const int DiscussPoints = 10;

        public static int MinutesFor(LibraryDesk.Book book)
        {
            switch (book.Id)
            {
                case "slim": return 30;
                case "almanac": return 60;
                case "fieldguide": return 60;
                case "dullbook": return 90;
                case "longbook": return 120;
                default: return 60;
            }
        }

        public static LibraryDesk.Book? ByItem(string itemId)
        {
            foreach (var b in LibraryDesk.Books) if (b.ItemId == itemId) return b;
            return null;
        }

        public static bool IsBook(string itemId) => ByItem(itemId) != null;
        public static string ReadFlag(LibraryDesk.Book book) => ReadPrefix + book.Id;
        public static string DiscussedFlag(string npc, LibraryDesk.Book book) => DiscussedPrefix + npc + "." + book.Id;
        public static bool HasRead(GameSession s, LibraryDesk.Book book) => s.HasFlag(ReadFlag(book));
        public static bool Discussed(GameSession s, string npc, LibraryDesk.Book book) => s.HasFlag(DiscussedFlag(npc, book));

        // The books this player has read and not yet talked over with this villager.
        public static System.Collections.Generic.List<LibraryDesk.Book> ToDiscuss(GameSession s, string npc)
        {
            var list = new System.Collections.Generic.List<LibraryDesk.Book>();
            foreach (var b in LibraryDesk.Books)
                if (HasRead(s, b) && !Discussed(s, npc, b)) list.Add(b);
            return list;
        }

        // The farmer's reading of one book: counts the minutes down as the clock passes them.
        public sealed class Session : IDisposable
        {
            readonly GameSession _game;
            readonly EventBus _bus;
            public LibraryDesk.Book Book { get; }
            public int MinutesLeft { get; private set; }
            public bool Finished { get; private set; }
            public bool Cancelled { get; private set; }
            public bool Active => !Finished && !Cancelled;
            public event Action<Session> Ended;

            public Session(GameSession game, EventBus bus, LibraryDesk.Book book)
            {
                _game = game; _bus = bus; Book = book;
                MinutesLeft = MinutesFor(book);
                _bus?.Subscribe<MinuteChanged>(OnMinute);
                _game.Clock.SpeedMultiplier = FastSpeed;
            }

            // Whatever the clock does between two events, the minutes that went by are counted by the event's own step.
            void OnMinute(MinuteChanged e)
            {
                if (!Active) return;
                MinutesLeft -= GameClock.MinutesPerStep;
                if (e.Now.IsDayOver) { Cancel(); return; }
                if (MinutesLeft <= 0) Finish();
            }

            void Finish()
            {
                Finished = true;
                _game.SetFlag(ReadFlag(Book));
                _game.AddVar(ReadStat, 1);
                Release();
                _game.Toast(L.Get("book.finished", L.Get("item." + Book.ItemId + ".name")));
                Ended?.Invoke(this);
            }

            public void Cancel()
            {
                if (!Active) return;
                Cancelled = true;
                Release();
                _game.Toast(L.Get("book.stopped", L.Get("item." + Book.ItemId + ".name")));
                Ended?.Invoke(this);
            }

            void Release()
            {
                _bus?.Unsubscribe<MinuteChanged>(OnMinute);
                _game.Clock.SpeedMultiplier = 1f;
            }

            public void Dispose()
            {
                if (Active) { Cancelled = true; Release(); }
            }
        }

        // Can the farmer start reading here? (not in the middle of a menu, not when the day is over)
        public static bool CanStart(GameSession s) => s != null && s.InGame && !s.Clock.IsPaused && !s.Clock.Now.IsDayOver;
    }
}
