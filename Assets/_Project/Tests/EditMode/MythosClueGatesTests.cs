using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The Keepers and Hazel confide their clues to a friend, never to a useful stranger (NPC design review 2026-10-16, 4.1):
    // clue 1 needs 2 hearts, clue 2 needs 4, clue 3 needs 6, and Odalys invites a friend of 6 hearts into the circle.
    public class MythosClueGatesTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public readonly Dictionary<string, int> Hearts_ = new Dictionary<string, int>();
            public readonly Dictionary<string, int> Vars = new Dictionary<string, int>();

            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => Vars.TryGetValue(name, out var v) ? v : 0;
            public GameDateTime Now => new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => Hearts_.TryGetValue(npcId, out var h) ? h : 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        static readonly string[] Confidants = MythosCast.Keepers.Concat(new[] { "hazel" }).ToArray();

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            GameSession.HorrorLevelOverride = 2;
            MythosModule.RegisterConditions();
            _story = StoryContent.LoadFromResources();
            _story.AddJson(File.ReadAllText(Path.Combine(Application.dataPath, "_Project", "Resources", "Mythos", "mythos_story.json")), "mythos_story");
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            Conditions.ClearCustomForTests();
        }

        // A player who has read everything in the woods but never befriended the villager.
        static World Stranger(string npc, int lore)
        {
            var w = new World();
            w.Flags.Add("met." + npc);
            w.Vars["lore"] = lore;
            return w;
        }

        static IEnumerable<string> Offered(DialogueSet set, World w)
        {
            for (var seed = 0; seed < 50; seed++) yield return set.Pick(w, seed);
        }

        [Test]
        public void AKeeperAtZeroHearts_WithLoreFive_IsNotOfferedAClue()
        {
            foreach (var npc in Confidants)
            {
                var set = _story.Set($"npc.{npc}.talk");
                Assert.IsNotNull(set, npc);
                var clues = Offered(set, Stranger(npc, 5)).Where(d => d != null && d.StartsWith($"mythos.{npc}.clue")).Distinct().ToList();
                Assert.IsEmpty(clues, $"{npc} tells a stranger: {string.Join(", ", clues)}");
            }
        }

        [Test]
        public void AFriendOfTwoHearts_HearsTheFirstClue_AndNoMore()
        {
            foreach (var npc in Confidants)
            {
                var w = Stranger(npc, 5);
                w.Hearts_[npc] = 2;
                var offered = Offered(_story.Set($"npc.{npc}.talk"), w).Distinct().ToList();
                CollectionAssert.Contains(offered, $"mythos.{npc}.clue1", npc);
                Assert.IsFalse(offered.Any(d => d == $"mythos.{npc}.clue2" || d == $"mythos.{npc}.clue3"), $"{npc}: {string.Join(", ", offered)}");
            }
        }

        [Test]
        public void EveryClueEntry_NamesItsHeartsGate()
        {
            foreach (var npc in Confidants)
                for (var clue = 1; clue <= 3; clue++)
                {
                    var entry = _story.Set($"npc.{npc}.talk").Entries.FirstOrDefault(e => e.Dialogue == $"mythos.{npc}.clue{clue}");
                    Assert.IsNotNull(entry, $"{npc} clue {clue}");
                    StringAssert.Contains($"hearts:{npc}>={clue * 2}", entry.Condition, $"{npc} clue {clue}");
                }
        }

        [Test]
        public void OdalysInvitesAFriend_NotAUsefulStranger()
        {
            var entry = _story.Set("npc.odalys.talk").Entries.First(e => e.Dialogue == "mythos.odalys.join");
            StringAssert.Contains("hearts:odalys>=6", entry.Condition);
            var w = Stranger("odalys", 5);
            w.Vars["cult.standing"] = 20;
            w.Flags.Add("clue.odalys.1"); w.Flags.Add("clue.odalys.2"); w.Flags.Add("clue.odalys.3");
            CollectionAssert.DoesNotContain(Offered(_story.Set("npc.odalys.talk"), w).ToList(), "mythos.odalys.join", "standing alone does not earn the invitation");
            w.Hearts_["odalys"] = 6;
            CollectionAssert.Contains(Offered(_story.Set("npc.odalys.talk"), w).ToList(), "mythos.odalys.join");
        }
    }
}
