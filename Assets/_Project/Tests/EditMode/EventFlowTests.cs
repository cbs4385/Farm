using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-100: the control flow of scenes (conditions, labels, branches), step validation and the pure parts of the new steps.
    public class EventFlowTests
    {
        sealed class World : IWorldQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Village";
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        static EventStep S(string type, string label = null, string target = null, string condition = null, string effect = null)
        {
            var s = new EventStep { Type = type, Label = label, Target = target, Condition = condition };
            if (effect != null) s.Effects.Add(effect);
            return s;
        }

        // ---- flow ----

        [Test]
        public void AStepCondition_SkipsTheStep_WhileFalse()
        {
            var w = new World();
            var step = S("effects", condition: "flag:ready");
            Assert.IsFalse(EventFlow.ShouldRun(step, w));
            w.Flags.Add("ready");
            Assert.IsTrue(EventFlow.ShouldRun(step, w));
            Assert.IsTrue(EventFlow.ShouldRun(S("effects"), w), "no condition: always");
            Assert.IsTrue(EventFlow.ShouldRun(S("branch", condition: "flag:never"), w), "a branch uses its condition to jump, not to skip itself");
        }

        [Test]
        public void ABrokenCondition_NeverRunsTheStep() =>
            Assert.IsFalse(EventFlow.ShouldRun(S("effects", condition: "bogus:atom"), new World()));

        [Test]
        public void ABranch_JumpsToItsLabel_WhenTheConditionHolds()
        {
            var steps = new List<EventStep> { S("branch", target: "end", condition: "flag:go"), S("effects", effect: "flag:a"), S("label", label: "end"), S("effects", effect: "flag:b") };
            var w = new World();
            Assert.AreEqual(1, EventFlow.Next(steps, 0, w), "false: carry on");
            w.Flags.Add("go");
            Assert.AreEqual(2, EventFlow.Next(steps, 0, w), "true: jump to the label");
            Assert.AreEqual(2, EventFlow.Next(steps, 1, w), "an ordinary step just moves on");
        }

        [Test]
        public void AnUnconditionalBranch_AlwaysJumps_AndAMissingLabelFallsThrough()
        {
            var steps = new List<EventStep> { S("branch", target: "x"), S("label", label: "x") };
            Assert.AreEqual(1, EventFlow.Next(steps, 0, new World()));
            var broken = new List<EventStep> { S("branch", target: "nowhere"), S("effects") };
            Assert.AreEqual(1, EventFlow.Next(broken, 0, new World()), "a bad target does not crash a scene");
        }

        [Test]
        public void ThePath_FollowsBranches_AndSkipsFalseSteps()
        {
            var steps = new List<EventStep>
            {
                S("branch", target: "b", condition: "flag:take_b"),                  // 0
                S("effects", effect: "flag:a"),                                        // 1
                S("branch", target: "end"),                                            // 2
                S("label", label: "b"),                                                // 3
                S("effects", effect: "flag:b"),                                        // 4
                S("label", label: "end"),                                              // 5
                S("effects", condition: "flag:never", effect: "flag:never_runs"),      // 6
                S("effects", effect: "flag:done"),                                     // 7
            };
            var w = new World();
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 5, 7 }, EventFlow.PathFrom(steps, 0, w));
            w.Flags.Add("take_b");
            CollectionAssert.AreEqual(new[] { 0, 3, 4, 5, 7 }, EventFlow.PathFrom(steps, 0, w));
            CollectionAssert.AreEqual(new[] { 5, 7 }, EventFlow.PathFrom(steps, 5, w), "from the middle of a scene");
        }

        [Test]
        public void ALoopingScene_IsStoppedBySafetyCap()
        {
            var steps = new List<EventStep> { S("label", label: "top"), S("branch", target: "top") };
            var path = EventFlow.PathFrom(steps, 0, new World());
            Assert.LessOrEqual(path.Count, EventFlow.MaxSteps);
            Assert.Greater(path.Count, 10);
        }

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void ASkippedScene_RunsOnlyTheEffectsOfTheBranchItWasOn()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null))
            {
                var ev = new EventDefinition
                {
                    Id = "t", Trigger = "manual",
                    Steps =
                    {
                        S("branch", target: "b", condition: "flag:take_b"),
                        S("effects", effect: "flag:a"), S("branch", target: "end"),
                        S("label", label: "b"), S("effects", effect: "flag:b"),
                        S("label", label: "end"), S("effects", effect: "flag:done"),
                    },
                    SkipEffects = { "flag:skipped" },
                };
                f.Session.SetFlag("take_b");
                EventRunner.RunSkipped(f.Session, ev, 0);
                Assert.IsTrue(f.Session.HasFlag("b"));
                Assert.IsFalse(f.Session.HasFlag("a"), "the other branch is not run");
                Assert.IsTrue(f.Session.HasFlag("done"));
                Assert.IsTrue(f.Session.HasFlag("skipped"));
            }
        }

        // ---- validation ----

        static List<string> Check(params EventStep[] steps)
        {
            var ev = new EventDefinition { Id = "t", Steps = steps.ToList() };
            return EventSteps.Problems(ev, id => id == "tilda" || id == "wren", id => id == "crop.strawberry", id => id == "wren.first");
        }

        static EventStep Step(string type, string actor = null, string name = null, string id = null)
        {
            var s = new EventStep { Type = type, Actor = actor, Name = name, Id = id };
            return s;
        }

        [Test]
        public void GoodSteps_HaveNoProblems()
        {
            var problems = Check(
                Step("emote", "tilda", "heart"), Step("expression", "wren", "happy"), Step("anim", "player", "hop"),
                Step("camera", null, "shake"), Step("camera", "tilda", "focus"), Step("sfx", null, "coin"), Step("music", null, "stop"),
                Step("lighting", null, "night"), Step("letterbox", null, "on"), Step("spawn", null, "crop.strawberry", "gift"), Step("despawn", null, null, "gift"),
                new EventStep { Type = "waitFor" }, new EventStep { Type = "dialogue", Dialogue = "wren.first" },
                new EventStep { Type = "say", Speaker = "tilda", Text = "k", Expression = "happy", Emote = "note" },
                new EventStep { Type = "move", Actor = "tilda", Async = true });
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [TestCase("emote", "tilda", "skull", "unknown emote")]
        [TestCase("emote", null, "heart", "needs an actor")]
        [TestCase("expression", "tilda", "furious", "unknown expression")]
        [TestCase("anim", "tilda", "backflip", "unknown animation")]
        [TestCase("camera", null, "zoom", "unknown camera mode")]
        [TestCase("sfx", null, "boom", "unknown sound")]
        [TestCase("music", null, null, "cue name")]
        [TestCase("lighting", null, "purple", "unknown lighting preset")]
        [TestCase("letterbox", null, "maybe", "unknown letterbox setting")]
        [TestCase("spawn", null, "not.an.item", "not an item")]
        [TestCase("emote", "ghost", "heart", "unknown actor")]
        [TestCase("dance", null, null, "unknown step type")]
        public void BadSteps_AreReported(string type, string actor, string name, string expected)
        {
            var problems = Check(Step(type, actor, name, type == "spawn" ? "p" : null));
            Assert.IsTrue(problems.Any(p => p.Contains(expected)), $"{expected}: {string.Join(" | ", problems)}");
        }

        [Test]
        public void Labels_AndBranches_AreChecked()
        {
            Assert.IsTrue(Check(S("label")).Any(p => p.Contains("needs a name")));
            Assert.IsTrue(Check(S("label", label: "a"), S("label", label: "a")).Any(p => p.Contains("defined twice")));
            Assert.IsTrue(Check(S("branch", target: "nowhere")).Any(p => p.Contains("unknown label")));
            Assert.IsTrue(Check(S("branch")).Any(p => p.Contains("needs a target")));
            Assert.IsTrue(Check(S("label", label: "top"), S("branch", target: "top")).Any(p => p.Contains("needs a condition")), "an unconditional branch back is an endless loop");
            CollectionAssert.IsEmpty(Check(S("label", label: "top"), S("branch", target: "top", condition: "flag:again")), "a conditional loop is fine");
            CollectionAssert.IsEmpty(Check(S("branch", target: "end"), S("label", label: "end")), "a forward jump is fine");
        }

        [Test]
        public void ParallelGroups_AreChecked()
        {
            Assert.IsTrue(Check(new EventStep { Type = "parallel" }).Any(p => p.Contains("needs steps")));
            var withSay = new EventStep { Type = "parallel", Steps = { new EventStep { Type = "say", Speaker = "tilda", Text = "k" } } };
            Assert.IsTrue(Check(withSay).Any(p => p.Contains("cannot run inside a parallel group")));
            var nested = new EventStep { Type = "parallel", Steps = { new EventStep { Type = "parallel", Steps = { Step("wait") } } } };
            Assert.IsTrue(Check(nested).Any(p => p.Contains("cannot")));
            var async = new EventStep { Type = "parallel", Steps = { new EventStep { Type = "move", Actor = "tilda", Async = true } } };
            Assert.IsTrue(Check(async).Any(p => p.Contains("already in the background")));
            var good = new EventStep { Type = "parallel", Steps = { new EventStep { Type = "move", Actor = "tilda" }, new EventStep { Type = "wait", Seconds = 1 } } };
            CollectionAssert.IsEmpty(Check(good));
        }

        [Test]
        public void Async_IsOnlyForBackgroundSteps()
        {
            Assert.IsTrue(Check(new EventStep { Type = "say", Speaker = "tilda", Text = "k", Async = true }).Any(p => p.Contains("cannot be async")));
            Assert.IsTrue(Check(new EventStep { Type = "dialogue", Dialogue = "wren.first", Async = true }).Any(p => p.Contains("cannot be async")));
        }

        [Test]
        public void Props_MustBeSpawnedBeforeTheyAreRemoved()
        {
            Assert.IsTrue(Check(Step("despawn", null, null, "gift")).Any(p => p.Contains("no prop 'gift' was spawned")));
            Assert.IsTrue(Check(Step("spawn", null, "crop.strawberry")).Any(p => p.Contains("needs an id")));
            CollectionAssert.IsEmpty(Check(Step("spawn", null, "crop.strawberry", "g"), Step("despawn", null, null, "g")));
        }

        [Test]
        public void TheNewStepFields_LoadFromStoryJson()
        {
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(@"{ ""events"": [ { ""id"": ""x"", ""trigger"": ""manual"", ""steps"": [
              { ""type"": ""camera"", ""name"": ""shake"", ""value"": 0.3, ""seconds"": 0.5 },
              { ""type"": ""label"", ""label"": ""top"" },
              { ""type"": ""branch"", ""target"": ""top"", ""condition"": ""flag:again"" },
              { ""type"": ""move"", ""actor"": ""tilda"", ""x"": 3, ""y"": 4, ""async"": true },
              { ""type"": ""parallel"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 }, { ""type"": ""emote"", ""actor"": ""tilda"", ""name"": ""note"" } ] } ] } ] }", "t"));
            Assert.IsEmpty(story.Errors);
            var steps = story.Event("x").Steps;
            Assert.AreEqual(0.3f, steps[0].Value, 0.0001f);
            Assert.AreEqual("top", steps[2].Target);
            Assert.IsTrue(steps[3].Async);
            Assert.AreEqual(2, steps[4].Steps.Count);
            Assert.AreEqual("note", steps[4].Steps[1].Name);
        }

        [Test]
        public void EveryShippedEvent_PassesTheNewStepChecks()
        {
            var story = StoryContent.LoadFromResources();
            var db = RealDb();
            var npcs = NpcCatalog.From(db).All.Select(n => n.Id).ToList();
            var items = db.AllItems.Select(i => i.Id).ToList();
            foreach (var ev in story.Events)
            {
                var problems = EventSteps.Problems(ev, npcs.Contains, items.Contains, id => story.Dialogue(id) != null);
                CollectionAssert.IsEmpty(problems, ev.Id + ": " + string.Join(" | ", problems));
            }
        }

        // ---- the pure bits of the stage ----

        [Test]
        public void Gestures_StartAndEndAtRest_AndActuallyMove()
        {
            foreach (var name in EventSteps.Anims)
            {
                Assert.AreEqual(Vector3.zero, EventStage.GestureOffset(name, 0f), name + " starts at rest");
                if (name == "look") continue;       // turns the actor, does not move it
                Assert.Less(EventStage.GestureOffset(name, 1f).magnitude, 0.05f, name + " ends near rest");
                var moved = Enumerable.Range(1, 99).Max(i => EventStage.GestureOffset(name, i / 100f).magnitude);
                Assert.Greater(moved, 0.03f, name + " moves");
                Assert.Less(moved, 0.4f, name + " stays small");
            }
        }

        [Test]
        public void LightingPresets_AreColours_AndResetIsNotOne()
        {
            Assert.AreEqual(Color.white, DayNightLighting.PresetColor("day"));
            Assert.AreNotEqual(Color.white, DayNightLighting.PresetColor("night"));
            foreach (var preset in EventSteps.LightingPresets.Where(p => p != "reset")) Assert.IsNotNull(DayNightLighting.PresetColor(preset));
        }

        [Test]
        public void MusicCue_KnowsStop()
        {
            Assert.IsTrue(new MusicCue("stop").IsStop);
            Assert.IsFalse(new MusicCue("festival").IsStop);
        }

        [Test]
        public void EmoteGlyphs_ExistForEveryEmote()
        {
            foreach (var emote in DialogueVocabulary.Emotes) Assert.IsNotNull(DialogueVocabulary.EmoteGlyph(emote), emote);
            Assert.IsNull(DialogueVocabulary.EmoteGlyph("skull"));
        }
    }
}
