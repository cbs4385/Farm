using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-102 in the real game: the Memories tab, and a replay that changes nothing in the save and returns the player.
    public class MemoriesFlowTests : InputTestFixture
    {
        string _dataRoot;
        Keyboard _keyboard;
        GameSession _session;
        int _eventFinished;

        public override void Setup()
        {
            base.Setup();
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-memtests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _eventFinished = 0;
        }

        public override void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            base.TearDown();
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        IEnumerator Begin()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            ServiceLocator.Get<SettingsStore>().Current.DialogueSpeed = DialoguePacing.InstantSpeed;
            ServiceLocator.Get<EventBus>().Subscribe<EventFinished>(_ => _eventFinished++);
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        static IUiService Ui => ServiceLocator.Get<IUiService>();
        static PlayerController Player => UnityEngine.Object.FindAnyObjectByType<PlayerController>();

        IEnumerator Tap(Key key)
        {
            Press(_keyboard[key]);
            yield return null;
            Release(_keyboard[key]);
            yield return null;
        }

        static IEnumerator WaitForScene(string name, float seconds = 20f)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < end) yield return null;
        }

        [UnityTest]
        public IEnumerator TheMemoriesTab_ListsTheSeenScenesOfTheChosenGroup_AndCountsTheRest()
        {
            yield return Begin();
            _session.State.EventsSeen.Add("wren_heart2");
            _session.State.EventsSeen.Add("festival_spring");
            Ui.ShowGameMenu(MenuTabs.Memories);
            for (var i = 0; i < 4; i++) yield return null;

            Button Find(string id) => UnityEngine.Object.FindObjectsByType<Button>().FirstOrDefault(b => b.name == id && b.gameObject.activeInHierarchy);
            Assert.IsNotNull(Find("Group_festival"), "the festivals are a group");
            Assert.IsNotNull(Find("Group_wren"), "and so is each villager with scenes");
            Assert.IsNotNull(Find("Group_other"));
            var seenFestival = Find("Memory_festival_spring");
            Assert.IsNotNull(seenFestival, "the first group with a seen scene is open: the festival");
            Assert.IsTrue(seenFestival.interactable, "and it can be replayed");
            Assert.IsNull(Find("Memory_wren_heart2"), "Wren's scenes are in her own group");

            Find("Group_wren").onClick.Invoke();
            yield return null;
            var seen = Find("Memory_wren_heart2");
            Assert.IsNotNull(seen, "her seen scene is listed");
            Assert.IsTrue(seen.interactable);
            StringAssert.Contains("The New Recipe", seen.GetComponentInChildren<TMPro.TextMeshProUGUI>().text);
            Assert.IsNull(Find("Memory_wren_heart5"), "a scene not yet seen is not listed, only counted");
            var texts = UnityEngine.Object.FindObjectsByType<TMPro.TextMeshProUGUI>().Where(t => t.gameObject.activeInHierarchy).Select(t => t.text).ToList();
            Assert.IsTrue(texts.Any(t => t.Contains("more to be seen")), "the rest are counted");
            Assert.IsFalse(texts.Any(t => t.Contains("???")), "no wall of question marks");
        }

        [UnityTest]
        public IEnumerator AReplay_ChangesNothingInTheSave_AndReturnsThePlayer()
        {
            yield return Begin();
            var s = _session;
            s.State.EventsSeen.Add("wren_heart2");
            Player.transform.position = new Vector3(20.5f, 10.5f, 0f);
            yield return null;
            var home = Player.transform.position;
            var gold = s.State.Gold;
            var minute = s.Clock.Now.MinuteOfDay;
            var points = NpcInteractions.StateOf(s.State, "wren").Points;
            var backpack = s.Backpack.Count("artisan.juice");
            var seen = s.State.EventsSeen.Count;

            Assert.IsTrue(Memories.Start(s, "wren_heart2"));
            Assert.IsFalse(Memories.Start(s, "wren_heart2"), "one replay at a time");
            yield return WaitForScene(MapIds.Saloon);
            Assert.AreEqual(MapIds.Saloon, SceneManager.GetActiveScene().name, "the scene plays on its own map");

            // Walk through the scene's lines.
            var end = Time.realtimeSinceStartup + 45f;
            while (s.MemoryId != null && Time.realtimeSinceStartup < end)
            {
                if (Ui.AnyModalOpen) yield return Tap(Key.Enter); else yield return null;
            }
            Assert.IsNull(s.MemoryId, "the replay ended");
            yield return WaitForScene(MapIds.Farm);
            for (var i = 0; i < 20; i++) yield return null;

            Assert.AreEqual(MapIds.Farm, SceneManager.GetActiveScene().name, "back on the farm");
            Assert.Less(Vector3.Distance(Player.transform.position, home), 0.05f, "standing where they were");
            Assert.AreEqual(points, NpcInteractions.StateOf(s.State, "wren").Points, "the +40 friendship was not given again");
            Assert.AreEqual(backpack, s.Backpack.Count("artisan.juice"), "nor the gift");
            Assert.IsFalse(s.HasFlag("event.wren_heart2") && !s.State.Flags.Contains("event.wren_heart2"));
            Assert.AreEqual(gold, s.State.Gold);
            Assert.AreEqual(minute, s.Clock.Now.MinuteOfDay, "the clock did not move");
            Assert.AreEqual(seen, s.State.EventsSeen.Count, "nothing new was marked seen");
            Assert.AreEqual(0, _eventFinished, "no EventFinished, so reactions and quests never hear of a replay");
            Assert.AreEqual(MapIds.Farm, s.State.CurrentMap);
        }

        [UnityTest]
        public IEnumerator AnUnseenScene_CannotBeReplayed()
        {
            yield return Begin();
            Assert.IsFalse(Memories.CanStart(_session, "wren_heart5"));
            Assert.IsFalse(Memories.Start(_session, "wren_heart5"));
            Assert.IsFalse(Memories.CanStart(_session, "no_such_event"));
            Assert.IsNull(_session.MemoryId);
        }
    }
}
