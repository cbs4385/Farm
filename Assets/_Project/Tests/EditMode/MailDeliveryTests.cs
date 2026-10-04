using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Regression (found by the full PlayMode suite, 2026-10-04): the twelve birthday thank-you letters had no condition, so every new game
    // opened with them in the mailbox. A letter that is only ever sent by a `mail:` effect must say so with the condition `false`.
    public class MailDeliveryTests
    {
        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        [Test]
        public void ANewGame_HasOnlyTheWelcomeLetterDue()
        {
            var world = new StateWorldQuery(new GameState(), null);
            var due = _story.Letters.Where(l => Conditions.TryEvaluate(l.Condition, world, out var ok) && ok).Select(l => l.Id).ToList();
            CollectionAssert.AreEqual(new[] { "welcome" }, due);
        }

        [Test]
        public void EveryLetterSentByAnEffect_IsNeverDeliveredByTheCalendar()
        {
            var sent = _story.Events.SelectMany(e => e.Steps).Where(s => s.Effects != null).SelectMany(s => s.Effects)
                .Concat(_story.Events.SelectMany(e => e.SkipEffects ?? new System.Collections.Generic.List<string>()))
                .Where(e => e != null && e.StartsWith("mail:")).Select(e => e.Substring(5)).ToHashSet();
            foreach (var id in sent)
            {
                var letter = _story.Letter(id);
                Assert.IsNotNull(letter, id);
                if (id.EndsWith("_bday_thanks")) Assert.AreEqual("false", letter.Condition, id);
            }
            Assert.GreaterOrEqual(sent.Count, 12);
        }
    }
}
