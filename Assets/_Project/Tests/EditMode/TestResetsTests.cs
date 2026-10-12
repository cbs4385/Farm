using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class TestResetsTests
    {
        [Test]
        public void A_registered_reset_runs_with_Bootstrapper_ResetForTests_and_only_once_per_run()
        {
            var runs = 0;
            void Reset() => runs++;
            TestResets.Add(Reset);
            TestResets.Add(Reset);            // the same reset twice is still one
            Bootstrapper.ResetForTests();
            Assert.AreEqual(1, runs);
        }

        [Test]
        public void ASessionWithNoUi_HasNone_AndTheContextSaysSoWithoutFailing()
        {
            var context = new GameContext();
            Assert.IsNull(context.Ui);
            Assert.IsNotNull(context.Notify, "toasts and messages are swallowed when there is no UI");
            context.Notify.Toast("nobody is there");
        }
    }
}
