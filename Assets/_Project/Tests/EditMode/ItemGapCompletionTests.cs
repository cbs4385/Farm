using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The last items of docs/ItemGapAudit.md: Elara's socks, Ione's very dull book, and the shelves and the lantern that the dialogue mentions.
    public class ItemGapCompletionTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void TheDullBook_IsAnItem_WithAName()
        {
            Assert.IsTrue(RealDb().TryGetItem("prop.dullbook", out var item));
            Assert.IsNotNull(item.Icon);
            Assert.IsTrue(L.Has("item.prop.dullbook.name"));
        }

        [Test]
        public void AGreatTease_IsRemembered_AndMakesIoneSetADullBookAside()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems))
            {
                var s = f.Session;
                InteractionMenu.RegisterEffects();
                foreach (var book in LibraryDesk.Books.Where(b => b.Id != "dullbook")) s.SetFlag(book.Flag);          // the others were collected
                Assert.IsNull(LibraryDesk.NextWaiting(s), "nothing waits yet");
                Effects.Run(s, "social.done:ione,tease,flop");
                Assert.IsNull(LibraryDesk.NextWaiting(s), "a flop earns nothing");
                Effects.Run(s, "social.done:ione,tease,great");
                Assert.IsTrue(s.HasFlag("social.ione.tease.great"));
                Assert.AreEqual("dullbook", LibraryDesk.NextWaiting(s)?.Id);
            }
        }

        [Test]
        public void ElarasSock_ComesWithTheWoolQuest_AndAFriendLetter()
        {
            var story = StoryContent.LoadFromResources();
            var quest = story.Quests.First(q => q.Id == "elara_wool");
            CollectionAssert.Contains(quest.Rewards, "give:prop.sock");
            var letter = story.Letters.First(l => l.Id == "elara_sock");
            Assert.AreEqual("hearts:elara>=4", letter.Condition);
            CollectionAssert.Contains(letter.Effects, "give:prop.sock");
            Assert.IsTrue(L.Has(letter.SubjectKey));
            Assert.IsTrue(L.Has(letter.BodyKey));
        }

        [TestCase("curio.backshelf")]
        [TestCase("curio.hazelshelf")]
        [TestCase("curio.lantern")]
        public void TheShelvesAndTheLantern_HaveTheirLines(string key) => Assert.IsTrue(L.Has(key), key);
    }
}
