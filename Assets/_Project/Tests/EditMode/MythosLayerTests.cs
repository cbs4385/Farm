using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // The horror layer end to end on a real session: inert at level 0, active at 1 and 2, the ritual's rules, and the promise
    // that a player who ignores it is never destroyed by it.
    public class MythosLayerTests
    {
        TestSessionFixture _f;
        GameSession S => _f.Session;

        [SetUp]
        public void SetUp()
        {
            _f = new TestSessionFixture();
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        void Install(int level)
        {
            GameSession.HorrorLevelOverride = level;
            S.Story = null;   // the story files need the full item database; these tests cover the layer's code
            MythosModule.Install(S, S.Hooks, _f.Bus);
            S.Story = new StoryContent();
        }

        // Runs `days` whole days; on ritual nights the clock walks through the ritual so the minute subscribers fire.
        void Simulate(int days, bool walkRituals = true)
        {
            for (var i = 0; i < days; i++)
            {
                var now = S.Clock.Now;
                if (walkRituals && RitualDirector.IsRitualDay(now))
                {
                    S.Clock.SetTime(new GameDateTime(now.Year, now.Season, now.Day, RitualDirector.StartMinute - 10));
                    for (var m = 0; m < 20 && !S.Clock.Now.IsDayOver; m++) S.Clock.AdvanceMinutes(10);
                }
                S.EndDay(false);
            }
        }

        [Test]
        public void Level0_ChangesNothing()
        {
            Install(0);
            Simulate(60);
            Assert.AreEqual(0, S.GetVar(MythosIds.Vars.Wakefulness));
            Assert.AreEqual(0, S.GetVar(MythosIds.Vars.Dread));
            Assert.IsFalse(S.HasFlag(MapIds.WoodsOpenFlag));
            Assert.AreEqual(0f, MythosLevel.Scale(S));
        }

        [TestCase(1)]
        [TestCase(2)]
        public void ActiveLevels_RaiseWakefulnessAndOpenTheWoods(int level)
        {
            Install(level);
            Simulate(40);
            Assert.Greater(S.GetVar(MythosIds.Vars.Wakefulness), 0);
            Assert.IsTrue(S.HasFlag(MapIds.WoodsOpenFlag), "from the first summer");
            Assert.AreEqual(WakefulnessModel.Step(S.GetVar(MythosIds.Vars.Wakefulness)), S.GetVar(MythosIds2.Step));
        }

        [Test]
        public void ThePathOpensQuest_StartsWhenTheWoodsOpen_AndEndsWhenThePlayerWalksIn()
        {
            var text = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath, "_Project", "Resources", "Mythos", "mythos_story.json"));
            var quest = Newtonsoft.Json.Linq.JObject.Parse(text)["quests"].First(q => (string)q["id"] == "mythos_woods");
            Assert.IsTrue((bool)quest["autoStart"]);
            Assert.AreEqual("horror:1 && flag:woods.open", (string)quest["available"]);
            Assert.AreEqual("flag:mythos.woods_entered", (string)quest["objectives"][0]["condition"]);
            L.SetLanguage("en");
            foreach (var key in new[] { "quest.mythos_woods.title", "quest.mythos_woods.desc", "quest.mythos_woods.o0", "mythos.woods_open" })
                Assert.AreNotEqual(key, L.Get(key), key);

            Install(1);
            MythosMaps.NoteWoodsEntered(S, MapIds.Forest);
            Assert.IsFalse(S.HasFlag(MythosIds.Flags.WoodsEntered), "the forest is not the wood");
            MythosMaps.NoteWoodsEntered(S, MapIds.Woods);
            Assert.IsTrue(S.HasFlag(MythosIds.Flags.WoodsEntered));
        }

        [Test]
        public void AtLevel0_WalkingIntoTheWoodsSetsNothing()
        {
            Install(0);
            MythosMaps.NoteWoodsEntered(S, MapIds.Woods);
            Assert.IsFalse(S.HasFlag(MythosIds.Flags.WoodsEntered));
        }

        [Test]
        public void ARitualThatRunsToTheEnd_LowersWakefulness()
        {
            Install(2);
            S.SetVar(MythosIds.Vars.Wakefulness, 500);
            Simulate(3);
            var before = S.GetVar(MythosIds.Vars.Wakefulness);
            Simulate(1);
            Assert.AreEqual(1, RitualDirector.Load(S).Successes);
            Assert.Less(S.GetVar(MythosIds.Vars.Wakefulness), before);
        }

        [Test]
        public void TakingFromTheAltar_NeedsTheInterferenceFlag()
        {
            Install(2);
            var now = S.Clock.Now;
            S.Clock.SetTime(new GameDateTime(now.Year, now.Season, RitualDirector.RitualDay, RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5));
            var save = RitualDirector.Load(S);
            RitualDirector.PlanSeason(S, save);
            RitualDirector.Store(S, save);
            Assert.IsNull(RitualDirector.TakeFromAltar(S));
        }

        [Test]
        public void AnIgnoringPlayer_NeverWakesTheGod_InSixYears()
        {
            Install(2);
            var max = 0;
            for (var d = 0; d < 6 * 4 * 28; d++)
            {
                Simulate(1);
                max = System.Math.Max(max, S.GetVar(MythosIds.Vars.Wakefulness));
            }
            Assert.Less(max, WakefulnessModel.Max, "rituals out-pace the rising god");
        }

        [Test]
        public void ARitualThatCannotBeHeld_LetsTheGodWake()
        {
            Install(2);
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;   // the awakening event is story data, absent here
            foreach (var keeper in MythosCast.Keepers) S.SetFlag(MythosIds2.Away(keeper));   // nobody can hold the ritual
            for (var d = 0; d < 2 * 4 * 28 && S.GetVar(MythosIds.Vars.Wakefulness) < WakefulnessModel.Max; d++) Simulate(1, walkRituals: false);
            Assert.AreEqual(WakefulnessModel.Max, S.GetVar(MythosIds.Vars.Wakefulness));
        }

        [Test]
        public void StepAnnouncements_FireOncePerStep()
        {
            Install(1);
            var steps = new List<int>();
            _f.Bus.Subscribe<WakefulnessStepChanged>(e => steps.Add(e.Step));
            S.SetVar(MythosIds.Vars.Wakefulness, 120);
            var save = RitualDirector.Load(S);
            WakefulnessService.SyncStep(S, save);
            WakefulnessService.SyncStep(S, save);
            Assert.AreEqual(new[] { 2 }, steps.ToArray());
        }

        [Test]
        public void DreadModifiers_ScaleWithTheLevel()
        {
            Install(2);
            S.SetVar(MythosIds.Vars.Dread, 80);
            var luck2 = S.Hooks.ComputeLuck(S.State);
            GameSession.HorrorLevelOverride = 1;
            var luck1 = S.Hooks.ComputeLuck(S.State);
            GameSession.HorrorLevelOverride = 0;
            var luck0 = S.Hooks.ComputeLuck(S.State);
            Assert.Less(luck2, luck1);
            Assert.Less(luck1, luck0);
            Assert.AreEqual(0f, MythosMutation.Chance(80, MythosLevel.Scale(S)), 1e-6f);
        }

        [Test]
        public void ScheduleSource_GivesKeepersARitualNight_AndOnlyKeepers()
        {
            var source = new MythosScheduleSource();
            var npcs = NpcDefaults.CreateAll().ToList();
            foreach (var npc in npcs)
            {
                var entries = source.EntriesFor(npc).ToList();
                if (MythosCast.IsKeeper(npc.Id))
                {
                    Assert.IsTrue(entries.Any(e => e.Id == "ritual_night"), npc.Id);
                    Assert.IsTrue(entries.Any(e => e.Id == "unavailable"), npc.Id);
                }
                else Assert.AreEqual(0, entries.Count, npc.Id);
            }
        }

        [Test]
        public void TurningTheLayerOff_KeepsItsState()
        {
            Install(2);
            Simulate(30);
            var wake = S.GetVar(MythosIds.Vars.Wakefulness);
            GameSession.HorrorLevelOverride = 0;
            Assert.AreEqual(wake, S.GetVar(MythosIds.Vars.Wakefulness));
        }

        [Test]
        public void AHookThatThrows_DoesNotStopTheDay()
        {
            Install(2);
            S.Hooks.AddDayCycleHook(new ThrowingHook());
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("ThrowingHook.OnDawn failed"));
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() => Simulate(3));
            Assert.Greater(S.GetVar(MythosIds.Vars.Wakefulness), 0);
        }

        sealed class ThrowingHook : DayCycleHook
        {
            public override int Order => 50;
            public override void OnDawn(DayCycleContext context) => throw new System.InvalidOperationException("boom");
        }
    }
}
