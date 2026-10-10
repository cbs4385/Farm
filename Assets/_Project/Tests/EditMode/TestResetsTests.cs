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
        public void UiAccess_does_nothing_when_there_is_no_UI()
        {
            ServiceLocator.Clear();
            var ran = false;
            Assert.IsFalse(UiAccess.Run(ui => ran = true));
            Assert.IsFalse(ran);
            Assert.IsFalse(UiAccess.AnyModalOpen);
        }
    }
}
