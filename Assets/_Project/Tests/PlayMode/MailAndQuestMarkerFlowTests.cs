using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Playtest 2026-10-10: nothing showed that there was mail, and nothing showed who a carried quest item was for.
    public class MailAndQuestMarkerFlowTests
    {
        string _root;
        GameSession _s;

        [SetUp]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-mailmark-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            GameServices.DataRootOverride = _root;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        IEnumerator Enter(string map)
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _s = ServiceLocator.Get<GameSession>();
            _s.BeginNewGame("Tester", "Test Farm", 0);
            _s.SetFlag(FatigueModel.WarnedFlag);
            _s.State.CurrentMap = map;
            var op = SceneManager.LoadSceneAsync(map);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 20; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator TheMailbox_ShowsAnEnvelope_WhileItHoldsALetter_AndNotOtherwise()
        {
            yield return Enter(MapIds.Farm);
            var indicator = UnityEngine.Object.FindAnyObjectByType<MailIndicator>();
            Assert.IsNotNull(indicator, "the farm's mailbox has one");
            _s.State.Mailbox.Clear();
            yield return new WaitForSecondsRealtime(0.8f);          // the indicator looks a few times a second
            Assert.IsFalse(indicator.Showing, "an empty box shows nothing");

            _s.State.Mailbox.Add("uncle_letter");
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.IsTrue(indicator.Showing, "a letter is waiting");

            _s.State.Mailbox.Clear();
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.IsFalse(indicator.Showing, "and gone once it is taken");
        }

        [UnityTest]
        public IEnumerator AVillagerWaitingForACarriedQuestItem_IsMarked_AndTheMapNamesTheirPlace()
        {
            yield return Enter(MapIds.Village);
            var quest = _s.Story.Quests.First(q => q.Id == "bram_stone");
            Assert.IsTrue(QuestLog.Start(_s, quest));
            Assert.IsFalse(QuestTargets.Wants(_s, "bram"), "nothing to hand over yet");

            _s.Backpack.Add("resource.stone", 10);
            Assert.IsTrue(QuestTargets.Wants(_s, "bram"), "ten stone in the pack: Bram is waiting");

            var actor = NpcManager.Current.Take("bram", new Vector3Int(30, 20, 0));
            yield return new WaitForSecondsRealtime(0.8f);
            var marker = actor.GetComponent<QuestMarker>();
            Assert.IsNotNull(marker);
            Assert.IsTrue(marker.Showing, "a mark over Bram");

            var spot = new WorldMapLayout.Spot { Map = NpcLocator.MapOf(_s, _s.Npcs.Get("bram")) };
            StringAssert.Contains(L.Get(_s.Npcs.Get("bram").NameKey), MapPage.HoverText(_s, spot));
            StringAssert.Contains(L.Get("map.wants_delivery", L.Get(_s.Npcs.Get("bram").NameKey)), MapPage.HoverText(_s, spot));

            _s.Backpack.Remove("resource.stone", 10);
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.IsFalse(marker.Showing, "the mark goes when the item is gone");
        }
    }
}
