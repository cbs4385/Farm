using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The storyline payoff scenes in the real game: each of the nine plays start to finish on the village crossing, taking every choice with
    // real key presses, and ends cleanly (end event, bars gone), sets its choice flag and its done flag, and pays its friendship.
    public class StorylineScenesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;

        public override void Setup()
        {
            base.Setup();
            PlayModeFixture.Uncapped();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-storyscenes-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static IUiService Ui => ServiceLocator.Get<IUiService>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        IEnumerator StartGame()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var settings = ServiceLocator.Get<SettingsStore>().Current;
            settings.DialogueSpeed = DialoguePacing.InstantSpeed;
            settings.ChatMenu = false;
        }

        // The `flag:choice.*` effects of the scene's dialogue, in the order the choices are listed.
        List<string> ChoiceFlags(string eventId)
        {
            var ev = _session.Story.Event(eventId);
            var graph = _session.Story.Dialogue(ev.Steps.First(s => s.Type == "dialogue").Dialogue);
            return graph.Nodes.SelectMany(n => n.Choices ?? new List<DialogueChoice>())
                .Select(c => (c.Effects ?? new List<string>()).First(e => e.StartsWith("flag:choice.")).Substring("flag:".Length)).ToList();
        }

        IEnumerator Play(string eventId, int choice)
        {
            var op = SceneManager.LoadSceneAsync(MapIds.Village);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
            var loader = ServiceLocator.Get<SceneLoader>();
            var settle = Time.realtimeSinceStartup + 10f;
            while (loader.IsLoading && Time.realtimeSinceStartup < settle) yield return null;

            _session.State.EventsSeen.Remove(eventId);
            var finished = false;
            ServiceLocator.Get<EventBus>().Subscribe<EventFinished>(e => { if (e.EventId == eventId) finished = true; });
            Assert.IsTrue(EventRunner.Trigger(_session, eventId), eventId);
            var end = Time.realtimeSinceStartup + 90f;
            while (!finished && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen)
                {
                    yield return Tap(Key.Digit1 + choice - 1);
                    yield return Tap(Key.Enter);
                }
                else yield return null;
            }
            Assert.IsTrue(finished, eventId + " ran to its end");
            for (var i = 0; i < 30; i++) yield return null;
            Assert.IsFalse(((UiService)Ui).LetterboxOn, eventId + ": the bars are gone");
            Assert.IsEmpty(_session.PendingEvents, eventId);
        }

        IEnumerator PlayAll(string idPrefix)
        {
            yield return StartGame();
            foreach (var ev in _session.Story.Events.Where(e => e.Id.StartsWith(idPrefix)).OrderBy(e => e.Id, StringComparer.Ordinal).ToList())
            {
                var done = ev.Steps.SelectMany(s => s.Effects ?? new List<string>()).First(e => e.StartsWith("flag:storydone.")).Substring("flag:".Length);
                var friend = ev.Steps.SelectMany(s => s.Effects ?? new List<string>()).Where(e => e.StartsWith("friend:")).Select(e => e.Split(':')[1].Split(',')).ToList();
                var flags = ChoiceFlags(ev.Id);
                Assert.GreaterOrEqual(flags.Count, 3, ev.Id);
                for (var choice = 1; choice <= flags.Count; choice++)
                {
                    var before = friend.ToDictionary(f => f[0], f => NpcInteractions.StateOf(_session.State, f[0]).Points);
                    yield return Play(ev.Id, choice);
                    Assert.IsTrue(_session.HasFlag(flags[choice - 1]), $"{ev.Id}/{choice}: sets {flags[choice - 1]}");
                    Assert.IsTrue(_session.HasFlag(done), $"{ev.Id}/{choice}: sets {done}");
                    foreach (var f in friend)
                        Assert.GreaterOrEqual(NpcInteractions.StateOf(_session.State, f[0]).Points - before[f[0]], int.Parse(f[1]), $"{ev.Id}/{choice}: friendship with {f[0]}");
                    _session.SetFlag(flags[choice - 1], false);
                    _session.SetFlag(done, false);
                }
            }
        }

        [UnityTest, Timeout(600000)] public IEnumerator PieFeud_PlaysWithEveryChoice() { yield return PlayAll("story_pie"); }
        [UnityTest, Timeout(600000)] public IEnumerator AnonymousNotes_PlaysWithEveryChoiceAndEveryWriter() { yield return PlayAll("story_notes"); }
        [UnityTest, Timeout(600000)] public IEnumerator LostUmbrella_PlaysWithEveryChoice() { yield return PlayAll("story_umbrella"); }
        [UnityTest, Timeout(600000)] public IEnumerator RivalScarecrows_PlaysWithEveryChoice() { yield return PlayAll("story_scarecrows"); }
        [UnityTest, Timeout(600000)] public IEnumerator MysteryWhistler_PlaysWithEveryChoiceAndEveryWhistler() { yield return PlayAll("story_whistler"); }
        [UnityTest, Timeout(600000)] public IEnumerator CompetingBand_PlaysWithEveryChoice() { yield return PlayAll("story_band"); }
        [UnityTest, Timeout(600000)] public IEnumerator MissingPumpkin_PlaysWithEveryChoiceAndEveryCulprit() { yield return PlayAll("story_pumpkin"); }
        [UnityTest, Timeout(600000)] public IEnumerator CatWithFiveNames_PlaysWithEveryChoice() { yield return PlayAll("story_cat"); }

        [UnityTest]
        public IEnumerator TheScarecrowContest_PaysAScarecrow()
        {
            yield return StartGame();
            var before = ((IGameQuery)_session.World).ItemCount("machine.scarecrow");
            yield return Play("story_scarecrows", 1);
            Assert.AreEqual(before + 1, ((IGameQuery)_session.World).ItemCount("machine.scarecrow"), "the runner-up scarecrow is given");
        }

        [UnityTest]
        public IEnumerator ANewGame_DrawsFourStorylines_AndAVariantForTheVariantOnes()
        {
            yield return StartGame();
            var drawn = _session.State.Flags.Where(f => f.StartsWith("storyline.") && f.IndexOf('.', "storyline.".Length) < 0).ToList();
            Assert.AreEqual(Storylines.PerGame, drawn.Count, string.Join(",", drawn));
            foreach (var flag in drawn)
            {
                var def = _session.Story.Storylines.First(d => "storyline." + d.Id == flag);
                var variants = _session.State.Flags.Where(f => f.StartsWith(flag + ".")).ToList();
                Assert.AreEqual(def.Variants != null && def.Variants.Count > 0 ? 1 : 0, variants.Count, flag);
            }
        }

        [UnityTest]
        public IEnumerator TheScenes_StartOnTheirOwn_WhenTheirConditionsHold()
        {
            yield return StartGame();
            // Fall Saturday morning, rainy, nothing else going on: the scarecrow contest and the umbrella are both ready.
            _session.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 10 * 60));
            _session.SetFlag("storyline.rival_scarecrows");
            _session.SetFlag("storyline.lost_umbrella");
            var due = EventRunner.FindTriggered(_session, MapIds.Village, false).Select(e => e.Id).ToList();
            CollectionAssert.Contains(due, "story_scarecrows");
        }
    }
}
