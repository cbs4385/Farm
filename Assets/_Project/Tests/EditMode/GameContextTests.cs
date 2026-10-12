using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class GameContextTests
    {
        [Test]
        public void AFreshContext_SaysWhatIsStillMissing_AndANotifierIsNeverNull()
        {
            var context = new GameContext();
            var missing = context.Missing();
            CollectionAssert.Contains(missing, nameof(GameContext.Session));
            CollectionAssert.Contains(missing, nameof(GameContext.Input));
            CollectionAssert.DoesNotContain(missing, nameof(GameContext.Ui), "the UI arrives later, from its own assembly");
            CollectionAssert.DoesNotContain(missing, nameof(GameContext.Notify), "the null notifier stands in until the real one is set");
            Assert.AreSame(NullNotifier.Instance, context.Notify);
        }

        [Test]
        public void TheNullNotifier_DoesNothing_AndStillFinishesAMessage()
        {
            var finished = false;
            NullNotifier.Instance.Toast("anything");
            NullNotifier.Instance.Message("any.key", () => finished = true);
            Assert.IsTrue(finished, "a message that cannot be shown must not leave its caller waiting");
        }

        [Test]
        public void TheNotifier_ToastsThroughTheSession_AndFinishesAMessageWhenThereIsNoUi()
        {
            using (var f = new TestSessionFixture())
            {
                var notifier = new Notifier(f.Session, () => null);
                notifier.Toast("Overnight the villagers tidied up.");
                CollectionAssert.Contains(f.Toasts, "Overnight the villagers tidied up.");
                var closed = false;
                notifier.Message("late_night.warning", () => closed = true);
                Assert.IsTrue(closed, "no UI: the caller is not left waiting");
                var before = f.Toasts.Count;
                notifier.Toast("");
                Assert.AreEqual(before, f.Toasts.Count, "an empty notice says nothing");
            }
        }
    }
}
