using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The ritual scenes (2026-10-10): the staged whispers, the robes, the altar's night, and the witnessed ritual with its choice, for every number of offerings and every way it can end.
    public class RitualScenesTests
    {
        TestSessionFixture _f;
        GameSession S => _f.Session;
        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            _f = new TestSessionFixture();
            GameSession.HorrorLevelOverride = 2;
            S.Story = null;
            MythosModule.Install(S, S.Hooks, _f.Bus);
            _story = new StoryContent();
            var text = File.ReadAllText(Path.Combine(Application.dataPath, "_Project", "Resources", "Mythos", "mythos_story.json"));
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;          // the mythos file adds entries to the villagers' sets, which this story does not have
            _story.AddJson(text, "mythos_story");
            S.Story = _story;
            L.SetLanguage("en");
            S.SetFlag(MapIds.WoodsOpenFlag);
            S.SetFlag(MythosIds2.Interference);
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            RitualScene.Bind(null);
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        // A ritual of `count` offerings at `minute` of its night, in the state `resolved` says.
        void Ritual(int count, int minute, bool? succeeded = null, bool taken = false)
        {
            var now = S.Clock.Now;
            S.Clock.SetTime(new GameDateTime(now.Year, now.Season, RitualDirector.RitualDay, minute));
            var save = new MythosSave { PlannedSeason = RitualDirector.SeasonIndex(S.Clock.Now) };
            for (var i = 0; i < count; i++) save.Plan.Add(new OfferingSlot { NpcId = MythosCast.Keepers[i % 5], ItemId = "food.salad", Taken = taken && i == count - 1 });
            if (succeeded.HasValue) { save.ResolvedSeason = save.PlannedSeason; save.LastSucceeded = succeeded.Value; }
            RitualDirector.Store(S, save);
        }

        [Test]
        public void TheWitnessedRitual_HasNoProblems_AndEveryPictureExists()
        {
            var ev = _story.Event("mythos_ritual_witness");
            Assert.IsNotNull(ev);
            var problems = EventSteps.Problems(ev, id => S.Db.AllNpcs.Any(n => n.Id == id) || MythosCast.IsKeeper(id) || id == "player", id => true, id => _story.Dialogue(id) != null);
            Assert.IsEmpty(problems, string.Join("\n", problems));
            foreach (var step in ev.Steps.Where(x => x.Type == "illustration"))
            {
                Assert.IsNotNull(Resources.Load<Texture2D>(step.Name), step.Name);
                Assert.IsTrue(L.Has(step.Text), step.Text);
            }
        }

        [Test]
        public void EveryRitualConditionInTheStory_IsOneTheLayerKnows()
        {
            foreach (var ev in _story.Events)
            {
                var conditions = new List<string> { ev.Condition };
                conditions.AddRange(ev.Steps.Select(s => s.Condition));
                foreach (var condition in conditions.Where(c => !string.IsNullOrEmpty(c)))
                    foreach (var token in condition.Split(new[] { "&&", "||", "(", ")", " " }, System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('!')).Where(x => x.StartsWith("ritual:")))
                        Assert.IsTrue(RitualScene.ValidCondition(token.Substring("ritual:".Length)), $"{ev.Id}: {token}");
            }
        }

        // The steps a scene takes through a ritual in a given state.
        List<EventStep> StepsTaken(EventDefinition ev) => EventFlow.PathFrom(ev.Steps, 0, S.World).Select(i => ev.Steps[i]).ToList();

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void WhenNoOneSteps_In_TheScenePlaysEveryOfferingThenTheSuccess_AndTheClockMovesToTheEnd(int count)
        {
            Ritual(count, RitualDirector.StartMinute + RitualModel.LeaderMinutes + 20 * count, succeeded: true);
            var ev = _story.Event("mythos_ritual_witness");
            var path = StepsTaken(ev);
            Assert.AreEqual(count, path.Count(s => s.Type == "say" && s.Text != null && s.Text.StartsWith("event.ritual.p")), "one beat for each offering");
            Assert.AreEqual(20 * count, path.Where(s => s.Type == "advance").Sum(s => s.Minutes), "each offering takes its twenty minutes");
            Assert.AreEqual(1, path.Count(s => s.Type == "dialogue" && s.Dialogue == "mythos.ritual.choice"), "one choice, at the last offering");
            Assert.IsTrue(path.Any(s => s.Text == "event.ritual.w0"), "the ending of a success");
            Assert.IsFalse(path.Any(s => s.Text == "event.ritual.s0" || s.Text == "event.ritual.x0"));
            Assert.AreEqual(1, path.Count(s => s.Type == "illustration" && s.Name == "Ritual/ritual_dissolve"));
            Assert.AreEqual("ritual:witness", path.Last(s => s.Type == "effects").Effects[0]);
        }

        [Test]
        public void WhenARitualFailsWithoutAnyoneStepping_In_TheSceneSaysOneLightWasMissing()
        {
            Ritual(3, RitualDirector.StartMinute + 120, succeeded: false);
            var path = StepsTaken(_story.Event("mythos_ritual_witness"));
            Assert.IsTrue(path.Any(s => s.Text == "event.ritual.x0"));
            Assert.IsTrue(path.Any(s => s.Type == "illustration" && s.Name == "Ritual/ritual_stir"));
            Assert.IsFalse(path.Any(s => s.Text == "event.ritual.w0"));
        }

        [TestCase(2)]
        [TestCase(5)]
        public void WhenThePlayerSnatchesTheOffering_TheSceneTakesTheOtherRoad_AndStillSpendsEveryTwentyMinutes(int count)
        {
            Ritual(count, RitualDirector.StartMinute + 30 + 20 * (count - 1) + 5, succeeded: null, taken: true);
            var path = StepsTaken(_story.Event("mythos_ritual_witness"));
            Assert.IsTrue(path.Any(s => s.Text == "event.ritual.s0"), "the player steps out");
            Assert.IsTrue(path.Any(s => s.Text == "event.ritual.f0"), "and the ritual fails");
            Assert.IsFalse(path.Any(s => s.Text == "event.ritual.w0"));
            Assert.AreEqual(20 * count, path.Where(s => s.Type == "advance").Sum(s => s.Minutes));
        }

        [Test]
        public void TheScene_BeginsOnlyAtFullIntensity_OnARitualNight_DuringTheLeadersWords_Once()
        {
            Ritual(3, RitualDirector.StartMinute + 5);
            Assert.IsTrue(RitualScene.Opening(S));
            Assert.IsTrue(RitualScene.Fresh(S));
            RitualScene.MarkWitnessed(S);
            Assert.IsFalse(RitualScene.Fresh(S), "watched: never twice in a season");
            Assert.AreEqual(1, RitualDirector.Load(S).RitualsWitnessed);

            Ritual(3, RitualDirector.StartMinute + RitualModel.LeaderMinutes + 1);
            Assert.IsFalse(RitualScene.Opening(S), "after the leader has finished it is too late for the opening");
            Ritual(3, RitualDirector.StartMinute - 10);
            Assert.IsFalse(RitualScene.Opening(S), "before it has started");

            GameSession.HorrorLevelOverride = 1;
            Ritual(3, RitualDirector.StartMinute + 5);
            Assert.IsFalse(RitualScene.Opening(S), "at mild there is no ritual imagery");
            GameSession.HorrorLevelOverride = 0;
            Assert.IsFalse(RitualScene.Opening(S));
        }

        [Test]
        public void Begin_MovesTheClockToTheEndOfTheLeadersWords_AndSnatchTakesTheOfferingFromTheStone()
        {
            Ritual(3, RitualDirector.StartMinute + 12);
            RitualScene.Begin(S);
            Assert.AreEqual(RitualDirector.StartMinute + RitualModel.LeaderMinutes, S.Clock.Now.MinuteOfDay);
            RitualDirector.Progress(S, S.Clock.Now.MinuteOfDay);
            S.Backpack.Add("food.salad", 0);
            Assert.IsTrue(RitualScene.Snatch(S) || !S.Backpack.CanAdd("food.salad", 1), "the offering is taken (or the pack is full)");
        }

        [Test]
        public void TheKeepers_WearRobesOnARitualNight_AtFullIntensity_WhileTheyAreFree()
        {
            Ritual(3, RitualDirector.StartMinute - 30);
            var source = new MythosOutfitSource(S);
            foreach (var keeper in MythosCast.Keepers) Assert.AreEqual("robe", source.OutfitFor(keeper), keeper);
            Assert.IsNull(source.OutfitFor("hazel"), "Hazel is not a Keeper");
            Assert.IsNull(source.OutfitFor("bram"));

            S.SetFlag(MythosIds2.Sick("tilda"));
            Assert.IsNull(source.OutfitFor("tilda"), "a sick Keeper stays in bed");

            Ritual(3, 600);
            Assert.IsNull(source.OutfitFor("marcus"), "in the morning of a ritual day");
            Ritual(3, 1500);
            Assert.IsNull(source.OutfitFor("marcus"), "long after the ritual");

            GameSession.HorrorLevelOverride = 1;
            Ritual(3, RitualDirector.StartMinute - 30);
            Assert.IsNull(source.OutfitFor("marcus"), "no robes at mild");

            GameSession.HorrorLevelOverride = 2;
            var now = S.Clock.Now;
            S.Clock.SetTime(new GameDateTime(now.Year, now.Season, 2, RitualDirector.StartMinute));
            Assert.IsNull(source.OutfitFor("marcus"), "an ordinary night");
        }

        [Test]
        public void TheRobedHours_RunFromTheEveningToAnHourAfterTheRitual()
        {
            Assert.IsFalse(MythosOutfitSource.RobedAt(MythosOutfitSource.LeaveMinute - 1, 3));
            Assert.IsTrue(MythosOutfitSource.RobedAt(MythosOutfitSource.LeaveMinute, 3));
            Assert.IsTrue(MythosOutfitSource.RobedAt(RitualDirector.StartMinute + RitualModel.DurationMinutes(3), 3));
            Assert.IsFalse(MythosOutfitSource.RobedAt(RitualDirector.StartMinute + RitualModel.DurationMinutes(3) + 61, 3));
        }

        [Test]
        public void EveryKeeper_HasARobeInAllFourDirections()
        {
            foreach (var keeper in MythosCast.Keepers)
                foreach (var facing in new[] { Vector2Int.down, Vector2Int.up, Vector2Int.left, Vector2Int.right })
                {
                    var sprite = NpcOutfits.SpriteFor("robe", keeper, facing);
                    Assert.IsNotNull(sprite, NpcOutfits.SpriteName("robe", keeper, facing));
                    Assert.AreEqual(16, (int)sprite.rect.width);
                    Assert.AreEqual(32, (int)sprite.rect.height);
                    Assert.AreEqual(0f, sprite.pivot.y, 0.01f, "the pivot is at the feet");
                    Assert.IsTrue(CharacterFrames.Animates(sprite), "the rig can walk and swing it: " + sprite.name);
                }
            Assert.IsNull(NpcOutfits.SpriteFor("robe", "bram", Vector2Int.down), "no robe for someone who is not a Keeper");
            Assert.IsNull(NpcOutfits.SpriteFor(null, "tilda", Vector2Int.down));
        }

        [Test]
        public void TheAltar_BurnsItsCandlesOnlyOnARitualNight_AtFullIntensity()
        {
            var participants = 3;
            Assert.IsTrue(AltarObject.NightOn(true, true, RitualDirector.StartMinute + 10, participants));
            Assert.IsFalse(AltarObject.NightOn(false, true, RitualDirector.StartMinute + 10, participants), "not at mild");
            Assert.IsFalse(AltarObject.NightOn(true, false, RitualDirector.StartMinute + 10, participants), "not on an ordinary night");
            Assert.IsFalse(AltarObject.NightOn(true, true, 900, participants), "not in the afternoon");
            Assert.IsFalse(AltarObject.NightOn(true, true, RitualDirector.StartMinute + 400, participants), "not long after");
        }

        [Test]
        public void EveryStringTheScenesUse_IsInTheTable()
        {
            foreach (var ev in _story.Events.Where(e => e.Id.StartsWith("mythos_")))
                foreach (var step in ev.Steps.Where(s => s.Type == "say"))
                    Assert.IsTrue(L.Has(step.Text), $"{ev.Id}: {step.Text}");
        }

        [Test]
        public void TheKeepersClues_AreInTheirOwnVoices_NoLineIsSharedBetweenTwoKeepers()
        {
            var seen = new Dictionary<string, string>();
            foreach (var keeper in MythosCast.Keepers)
                for (var clue = 1; clue <= 3; clue++)
                {
                    var dialogue = _story.Dialogue($"mythos.{keeper}.clue{clue}");
                    Assert.IsNotNull(dialogue, $"{keeper} clue {clue}");
                    foreach (var node in dialogue.Nodes)
                    {
                        var text = L.Get(node.Text);
                        if (seen.TryGetValue(text, out var other)) Assert.Fail($"{keeper} and {other} say the same line: {text}");
                        seen[text] = keeper;
                        foreach (var choice in node.Choices)
                        {
                            var c = L.Get(choice.Text);
                            if (seen.TryGetValue(c, out var o2)) Assert.Fail($"{keeper} and {o2} offer the same answer: {c}");
                            seen[c] = keeper;
                        }
                    }
                }
        }
    }
}
