using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest request (2026-10-05): the player should be told why they have this farm (a distant uncle left it to them), and the village
    // should not meow with no cat in sight.
    public class OpeningStoryTests
    {
        [Test]
        public void TheOpening_ShowsEveryPageInOrder_EachAfterTheLastIsClosed()
        {
            var shown = new List<string>();
            var closers = new List<System.Action>();
            OpeningStory.Run((key, onClose) => { shown.Add(key); closers.Add(onClose); });
            Assert.AreEqual(1, shown.Count, "only the first page at first");
            for (var i = 0; i < OpeningStory.Pages.Length - 1; i++) closers[i]();
            CollectionAssert.AreEqual(OpeningStory.Pages, shown);
            closers.Last()();
            Assert.AreEqual(OpeningStory.Pages.Length, shown.Count, "nothing after the last page");
        }

        [Test]
        public void TheOpeningPages_ExistAndFitTheMessageBox_AndNameTheUncle()
        {
            L.SetLanguage("en");
            foreach (var key in OpeningStory.Pages)
            {
                var text = L.Get(key);
                Assert.AreNotEqual(key, text, key);
                Assert.LessOrEqual(text.Length, 230, key + " fits the message window");
            }
            StringAssert.Contains("great-uncle", L.Get("intro.page1"));
            StringAssert.Contains("Edmund Fenn", L.Get("intro.page1"));
        }

        [Test]
        public void TheUncleLetter_ArrivesOnTheSecondDay_NotTheFirst()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var letter = story.Letter("uncle_letter");
            Assert.IsNotNull(letter);
            bool Due(GameDateTime now)
            {
                var state = new GameState(); state.SetDate(now);
                return Conditions.TryEvaluate(letter.Condition, new StateWorldQuery(state, null), out var ok) && ok;
            }
            Assert.IsFalse(Due(GameDateTime.NewGame));
            Assert.IsTrue(Due(new GameDateTime(1, Season.Spring, 2)));
            Assert.AreNotEqual(letter.BodyKey, L.Get(letter.BodyKey));
        }

        [Test]
        public void TheLetterReadOnTheBus_IsTildasWelcome_AndMentionsTheLateOwner()
        {
            L.SetLanguage("en");
            StringAssert.Contains("Edmund Fenn", L.Get("intro.page3"));
            StringAssert.Contains("Tilda Ashby", L.Get("intro.page3"));
            StringAssert.Contains("100 gold", L.Get("intro.page4"));
            var story = StoryContent.LoadFromResources();
            Assert.IsNull(story.Letter("welcome"), "no copy of it waits in the mailbox");
        }
    }
}
