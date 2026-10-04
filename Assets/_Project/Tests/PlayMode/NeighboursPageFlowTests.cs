using System;
using System.Collections;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Farm.Tests
{
    // T-106 and T-144 in the real game: selecting a villager on the Neighbours page shows what the player has found out, and the Gossip
    // Book counts rare lines and solved stories.
    public class NeighboursPageFlowTests
    {
        string _dataRoot;
        GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _dataRoot = Path.Combine(Path.GetTempPath(), "farm-neighbours-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dataRoot);
            GameServices.DataRootOverride = _dataRoot;
            Bootstrapper.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (Directory.Exists(_dataRoot)) Directory.Delete(_dataRoot, true);
        }

        static string AllText(MenuPage page) => string.Join("\n", page.Root.GetComponentsInChildren<TextMeshProUGUI>(true).Select(t => t.text));

        IEnumerator Start()
        {
            Bootstrapper.InitializeServices();
            yield return null;
            _session = ServiceLocator.Get<GameSession>();
            _session.BeginNewGame("Tester", "Test Farm", 0);
            _session.SetFlag(FatigueModel.WarnedFlag);
            _session.State.GetMap(MapIds.Farm).ClutterSeeded = true;
            var op = SceneManager.LoadSceneAsync(MapIds.Farm);
            while (!op.isDone) yield return null;
            for (var i = 0; i < 12; i++) yield return null;
        }

        [UnityTest]
        public IEnumerator SelectingAVillager_ShowsTheirStage_TheTastesFoundOut_AndAnOpenFavor()
        {
            yield return Start();
            _session.State.Npcs["wren"] = new NpcState { Met = true, Points = 6 * FriendshipModel.PointsPerHeart };
            var inter = InteractionState.Load(_session);
            inter.GiftDay["wren|artisan.wine"] = 1;
            inter.TopicTimes["wren.gossip"] = 1;
            InteractionState.Store(_session, inter);
            _session.State.Quests["wren_stew"] = new QuestProgress { Status = "active" };

            var ui = (UiService)ServiceLocator.Get<IUiService>();
            ui.ShowGameMenu(MenuTabs.Social);
            yield return null;
            var page = ui.GameMenu.Current;
            Assert.AreEqual(MenuTabs.Social, page.Id);
            page.Root.GetComponentsInChildren<Button>(true).First(b => b.name == "Neighbor_wren").onClick.Invoke();
            var text = AllText(page);
            StringAssert.Contains("Wren Calloway", text);
            StringAssert.Contains("Close friend (6 of 10 hearts)", text);
            StringAssert.Contains("Loves: Wine", text);
            StringAssert.Contains("Told you:", text);
            StringAssert.Contains("Asked a favor: Stew for the Regulars", text);
            StringAssert.DoesNotContain("Dislikes", text, "nothing disliked has been found out");
        }

        [UnityTest]
        public IEnumerator AnUnmetVillager_StaysAMystery_EvenWhenSelected()
        {
            yield return Start();
            var ui = (UiService)ServiceLocator.Get<IUiService>();
            ui.ShowGameMenu(MenuTabs.Social);
            yield return null;
            var page = ui.GameMenu.Current;
            page.Root.GetComponentsInChildren<Button>(true).First(b => b.name == "Neighbor_tilda").onClick.Invoke();
            var text = AllText(page);
            StringAssert.DoesNotContain("Tilda", text);
            StringAssert.Contains("haven't met this neighbor", text);
        }

        [UnityTest]
        public IEnumerator TheGossipBook_CountsRareLinesFound_AndSolvedStories()
        {
            yield return Start();
            _session.State.Npcs["wren"] = new NpcState { Met = true };
            var set = _session.Story.Set("npc.wren.talk");
            var memory = LineMemory.Load(_session);
            memory.Record(set.Id, set.Entries.First(GossipBook.IsRare).Dialogue, 3, 0);
            LineMemory.Store(_session, memory);
            _session.SetFlag("storyline.pie_feud");
            _session.SetFlag("storydone.pie_feud");

            var ui = (UiService)ServiceLocator.Get<IUiService>();
            ui.ShowGameMenu(MenuTabs.Gossip);
            yield return null;
            var text = AllText(ui.GameMenu.Current);
            StringAssert.Contains("Rare lines found: 1 of", text);
            StringAssert.Contains("Solved: The Pie Feud", text);
            StringAssert.Contains("Wren", text);
        }
    }
}
