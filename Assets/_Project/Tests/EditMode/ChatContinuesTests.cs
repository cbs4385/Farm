using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest report: conversations stopped suddenly and the villager had to be clicked again for every further line. Every chat menu now
    // offers "keep chatting" next to Goodbye, even when there are no topics or social actions left.
    public class ChatContinuesTests
    {
        [Test]
        public void EveryMenu_OffersKeepChatting_BeforeGoodbye_AndTheMinimalMenuHasOnlyThose()
        {
            var minimal = InteractionMenu.Minimal("tilda", key => false);
            var texts = minimal.Node("menu").Choices.Select(c => c.Text).ToList();
            CollectionAssert.AreEqual(new[] { InteractionMenu.MoreKey, InteractionMenu.GoodbyeKey }, texts);
            Assert.IsTrue(minimal.Node("menu").Choices.Last().Default, "Goodbye stays the default, so a habitual Enter ends the chat");

            var again = InteractionMenu.WithMore(minimal, "tilda");
            Assert.AreEqual(2, again.Node("menu").Choices.Count, "never added twice");
            Assert.IsTrue(L.Has(InteractionMenu.MoreKey));
        }

        [Test]
        public void TheKeepChattingEffect_SetsTheVariable_ThatTheMenuReadsWhenItCloses()
        {
            using (var f = new TestSessionFixture())
            {
                InteractionMenu.RegisterEffects();
                Effects.Run(f.Session, "talk.more");
                Assert.AreEqual(1, f.Session.GetVar(InteractionMenu.MoreVar));
            }
        }
    }
}
