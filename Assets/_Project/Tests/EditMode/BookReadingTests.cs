using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Owner, 2026-10-16: books can be read (it takes time, at the fastest clock speed), unread ones are asked about before they are given away, and a read book can be talked over
    // with each villager.
    public class BookReadingTests
    {
        static readonly Dictionary<string, string> En = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
        static LibraryDesk.Book Slim => LibraryDesk.Books[0];

        [Test]
        public void TheClock_RunsFaster_WhileReading_AndNeverSlowerThanNormal()
        {
            var clock = new GameClock(new GameDateTime(1, Season.Spring, 3, 8 * 60), new EventBus());
            clock.Tick(GameClock.DefaultSecondsPerStep);
            var normal = clock.Now.MinuteOfDay;
            clock.SpeedMultiplier = 10f;
            clock.Tick(GameClock.DefaultSecondsPerStep);
            Assert.AreEqual(normal + 10 * GameClock.MinutesPerStep, clock.Now.MinuteOfDay, "ten steps in the time of one");
            clock.SpeedMultiplier = 0.2f;
            var before = clock.Now.MinuteOfDay;
            clock.Tick(GameClock.DefaultSecondsPerStep);
            Assert.AreEqual(before + GameClock.MinutesPerStep, clock.Now.MinuteOfDay, "a speed below 1 is not a slow-down");
        }

        [Test]
        public void ReadingABook_TakesItsMinutes_ThenMarksItRead_AndPutsTheSpeedBack()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                s.Clock.SetTime(new GameDateTime(1, Season.Spring, 3, 9 * 60));
                using (var reading = new BookReading.Session(s, f.Bus, Slim))
                {
                    Assert.AreEqual(BookReading.FastSpeed, s.Clock.SpeedMultiplier, "the fastest speed while reading");
                    Assert.IsFalse(BookReading.HasRead(s, Slim));
                    var steps = 0;
                    while (reading.Active && steps < 100) { s.Clock.AdvanceMinutes(GameClock.MinutesPerStep); steps++; }
                    Assert.AreEqual(BookReading.MinutesFor(Slim) / GameClock.MinutesPerStep, steps);
                    Assert.IsTrue(reading.Finished);
                }
                Assert.IsTrue(BookReading.HasRead(s, Slim));
                Assert.AreEqual(1f, s.Clock.SpeedMultiplier, "normal speed again");
                Assert.AreEqual(1, s.GetVar(BookReading.ReadStat));
            }
        }

        [Test]
        public void StoppingEarly_KeepsTheBookUnread_AndTheSpeedNormal()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                var reading = new BookReading.Session(s, f.Bus, LibraryDesk.Books[4]);
                s.Clock.AdvanceMinutes(GameClock.MinutesPerStep);
                reading.Cancel();
                Assert.IsTrue(reading.Cancelled);
                Assert.IsFalse(BookReading.HasRead(s, LibraryDesk.Books[4]));
                Assert.AreEqual(1f, s.Clock.SpeedMultiplier);
                s.Clock.AdvanceMinutes(600);
                Assert.IsFalse(BookReading.HasRead(s, LibraryDesk.Books[4]), "a stopped reading counts nothing more");
            }
        }

        [Test]
        public void EveryBook_HasATime_AndALineFromEveryVillager()
        {
            foreach (var book in LibraryDesk.Books)
            {
                Assert.Greater(BookReading.MinutesFor(book), 0);
                Assert.IsTrue(En.ContainsKey("dlg.book.generic." + book.Id), book.Id);
                foreach (var npc in NpcIds.All) Assert.IsTrue(En.ContainsKey("dlg.book." + npc + "." + book.Id), npc + " on " + book.Id);
            }
        }

        [Test]
        public void TheBookMenu_ListsOnlyBooksRead_AndTalkingItOverPaysOnce()
        {
            using (var f = new TestSessionFixture())
            {
                var s = f.Session;
                Interactions_Register();
                Assert.IsEmpty(BookReading.ToDiscuss(s, NpcIds.Bram));
                s.SetFlag(BookReading.ReadFlag(Slim));
                Assert.AreEqual(1, BookReading.ToDiscuss(s, NpcIds.Bram).Count);

                var graph = InteractionMenu.Minimal(NpcIds.Bram, L.Has);
                InteractionMenu.WithBooks(graph, NpcIds.Bram, BookReading.ToDiscuss(s, NpcIds.Bram), En.ContainsKey);
                var menu = graph.Nodes.Find(n => n.Id == "menu");
                Assert.IsTrue(menu.Choices.Any(c => c.Text == InteractionMenu.BookKey));
                Assert.IsTrue(menu.Choices.Last().Default, "Goodbye stays last");
                var books = graph.Nodes.Find(n => n.Id == "books");
                Assert.AreEqual(2, books.Choices.Count, "the one book and Back");
                Assert.AreEqual("dlg.book.bram.slim", graph.Nodes.Find(n => n.Id == books.Choices[0].Next).Text);

                var before = NpcInteractions.StateOf(s.State, NpcIds.Bram).Points;
                Effects.Run(s, books.Choices[0].Effects[0]);
                var after = NpcInteractions.StateOf(s.State, NpcIds.Bram).Points;
                Assert.AreEqual(before + BookReading.DiscussPoints, after);
                Effects.Run(s, books.Choices[0].Effects[0]);
                Assert.AreEqual(after, NpcInteractions.StateOf(s.State, NpcIds.Bram).Points, "a book is talked over once with each villager");
                Assert.IsEmpty(BookReading.ToDiscuss(s, NpcIds.Bram));
                Assert.AreEqual(1, BookReading.ToDiscuss(s, NpcIds.Ione).Count, "another villager still has it to hear");
            }
        }

        static void Interactions_Register() => InteractionMenu.RegisterEffects();

        [Test]
        public void ABookThatIsNotInTheList_IsNotReadable() => Assert.IsFalse(BookReading.IsBook("crop.parsnip"));

        [Test]
        public void ThePageTurn_NarrowsAndOpensAgain()
        {
            Assert.AreEqual(1f, ReadingBook.PageWidth(0.1f));
            Assert.Less(ReadingBook.PageWidth(ReadingBook.PageSeconds * 0.925f), 0.2f);
            Assert.AreEqual(1f, ReadingBook.PageWidth(ReadingBook.PageSeconds * 1.1f));
        }
    }
}
