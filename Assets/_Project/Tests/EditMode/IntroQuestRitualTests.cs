using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // Elara's herb garden, the opening quest, and the horror layer's first ritual: with the horror on, the first offering of the first spring's ritual is the plant
    // she asks for, so a player who hands the plants over takes it from the Keepers and the ritual fails. Without the horror it is only a cozy errand.
    public class IntroQuestRitualTests
    {
        TestSessionFixture _f;
        GameSession S => _f.Session;

        [SetUp]
        public void SetUp() => _f = new TestSessionFixture();

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        void Install(int level)
        {
            GameSession.HorrorLevelOverride = level;
            S.Story = null;
            MythosModule.Install(S, S.Hooks, _f.Bus);
            S.Story = new StoryContent();
        }

        // Runs whole days; on a ritual night the clock walks through the ritual so the minute subscribers fire.
        void Simulate(int days)
        {
            for (var i = 0; i < days; i++)
            {
                var now = S.Clock.Now;
                if (RitualDirector.IsRitualDay(now))
                {
                    S.Clock.SetTime(new GameDateTime(now.Year, now.Season, now.Day, RitualDirector.StartMinute - 10));
                    for (var m = 0; m < 20 && !S.Clock.Now.IsDayOver; m++) S.Clock.AdvanceMinutes(10);
                }
                S.EndDay(false);
            }
        }

        static void Deliver(GameSession s) => s.State.Quests[MythosIds.Intro.Quest] = new QuestProgress { Status = QuestStatus.Done };

        [TestCase(1)]
        [TestCase(2)]
        public void WithTheHorrorOn_TheFirstRitualsFirstOfferingIsThePlantTheQuestAsksFor(int level)
        {
            Install(level);
            Simulate(1);
            var plan = RitualDirector.Load(S).Plan;
            Assert.AreEqual(2, plan.Count, "two offerings in spring");
            Assert.AreEqual(MythosIds.Intro.Item, plan[0].ItemId);
            Assert.AreEqual(MythosIds.Intro.Kind, plan[0].RefKind);
            Assert.AreEqual(MythosIds.Intro.Quest, plan[0].RefId);
            Assert.IsTrue(MythosCast.IsKeeper(plan[0].NpcId));
            Assert.AreNotEqual(MythosIds.Intro.Kind, plan[1].RefKind, "the second is chosen as ever");
            Assert.AreEqual("forage.wildgarlic", MythosIds.Intro.Item);
        }

        [Test]
        public void WithTheHorrorOff_ThereIsNoRitualAndNoPlan()
        {
            Install(0);
            Simulate(5);
            Assert.AreEqual(0, RitualDirector.Load(S).Plan.Count);
            Assert.AreEqual(0, RitualDirector.Load(S).Failures);
        }

        [Test]
        public void IfThePlayerNeverDeliversThePlants_TheFirstRitualGoesAheadAsBefore()
        {
            Install(2);
            S.SetVar(MythosIds.Vars.Wakefulness, 500);
            Simulate(4);                                                   // spring 1 to 4: the ritual night is the fourth
            var save = RitualDirector.Load(S);
            Assert.AreEqual(1, save.Successes, "nothing took the offering");
            Assert.AreEqual(0, save.Failures);
        }

        [TestCase(1)]
        [TestCase(2)]
        public void IfThePlayerDeliversThePlantsBeforeTheRitual_ItFails_AndNothingLowersTheGod(int level)
        {
            Install(level);
            S.SetVar(MythosIds.Vars.Wakefulness, 500);
            Simulate(3);
            Deliver(S);                                                    // Elara has the plants
            var before = S.GetVar(MythosIds.Vars.Wakefulness);
            Simulate(1);                                                   // the ritual night
            var save = RitualDirector.Load(S);
            Assert.AreEqual(0, save.Successes);
            Assert.AreEqual(1, save.Failures, "the ritual failed");
            Assert.IsTrue(save.Plan[0].Missing, "the fixed offering was gone");
            Assert.GreaterOrEqual(S.GetVar(MythosIds.Vars.Wakefulness), before, "a failed ritual lowers nothing");
        }

        [Test]
        public void DeliveringThePlantsAfterTheRitual_ChangesNothing()
        {
            Install(2);
            S.SetVar(MythosIds.Vars.Wakefulness, 500);
            Simulate(4);
            Assert.AreEqual(1, RitualDirector.Load(S).Successes);
            Deliver(S);
            Simulate(1);
            Assert.AreEqual(1, RitualDirector.Load(S).Successes, "the ritual had already been held");
            Assert.AreEqual(0, RitualDirector.Load(S).Failures);
        }

        [Test]
        public void OnlyTheFirstSpringHasTheFixedOffering()
        {
            Install(2);
            Simulate(28);                                                  // into summer
            Simulate(1);
            var save = RitualDirector.Load(S);
            Assert.AreEqual(3, save.Plan.Count, "three offerings in summer");
            Assert.IsFalse(save.Plan.Any(p => p.RefKind == MythosIds.Intro.Kind), "no fixed offering after the first spring");
        }

        [Test]
        public void TheFixedOffering_IsListedAsMarkedUntilItIsTakenOrSacrificed()
        {
            Install(2);
            Simulate(1);
            var marked = RitualDirector.Marked(RitualDirector.Load(S));
            Assert.IsTrue(marked.Any(m => m.RefKind == MythosIds.Intro.Kind), "the journal can list it");
        }
    }
}
