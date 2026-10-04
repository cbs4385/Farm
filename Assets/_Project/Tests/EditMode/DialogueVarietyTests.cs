using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Dialogue reachability and variety (docs/NPC_DIALOGUE_ANALYSIS.md): every authored talk line can be heard, no tier
    // leaves a player with a couple of lines, time-bound lines only play at their time, quest offers are never hidden by
    // friendship chatter, and invitations have an answer.
    public class DialogueVarietyTests
    {
        static readonly string[] Npcs = { "tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara" };
        static readonly string[] Seasons = { "spring", "summer", "fall", "winter" };

        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public readonly Dictionary<string, int> Hearts_ = new Dictionary<string, int>();
            public readonly Dictionary<string, string> Quests = new Dictionary<string, string>();
            public GameDateTime Time = new GameDateTime(1, Season.Spring, 10, 12 * 60);
            public string WeatherId = "sunny";

            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => Time;
            public string Weather => WeatherId;
            public string MapName = "Village";
            public string MapId => MapName;
            public int Hearts(string npcId) => Hearts_.TryGetValue(npcId, out var h) ? h : 0;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => Quests.TryGetValue(questId, out var q) ? q : "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        World Met(string npc, int hearts, Season season = Season.Spring, string weather = "sunny", int hour = 12, int day = 3)
        {
            var w = new World { Time = new GameDateTime(1, season, day, hour * 60), WeatherId = weather };
            w.Flags.Add("met." + npc);
            w.Hearts_[npc] = hearts;
            w.Flags.Add("hazel.book");              // one-shot scenes are spent, so only the ordinary pool is measured
            w.Quests["bram_stone"] = "done";        // an open offer deliberately outranks every chat line
            foreach (var q in new[] { "wren_stew", "wren_cider", "wren_full_house", "hazel_shelf", "hazel_pressed", "hazel_notes", "bram_copper", "bram_hooks", "tilda_display", "tilda_cauliflower", "tilda_pie_week", "juno_ore", "juno_charcoal", "juno_gold", "piper_strings", "piper_flowers", "piper_juice", "marcus_planks", "odalys_elderflower", "felix_bait", "dorian_basket", "elara_wool", "ione_pearl" }) w.Quests[q] = "done";
            return w;
        }

        HashSet<string> Heard(string npc, World w)
        {
            var set = _story.Set($"npc.{npc}.talk");
            var heard = new HashSet<string>();
            for (var seed = 0; seed < 400; seed++) heard.Add(set.Pick(w, seed));
            return heard;
        }

        // Mood, farm, festival and birthday lines need a world state this sweep does not build; MoodModelTests and the slice tests cover them.
        static bool StateDependent(string c) => c.Contains("mood:") || c.Contains("farm:") || c.Contains("festival.in") || c.Contains("birthday.in") || c.Contains("farmname:") || c.Contains("playername:") || c.Contains("storyline:") || c.Contains("storydone.") || c.Contains("notes.began") || c.Contains("partners.") || c.Contains("upgrade:") || c.Contains("merchant:") || c.Contains("shipped:") || c.Contains("gold:") || c.Contains("hall:") || c.Contains("upgraded:") || c.Contains("farm.coop") || c.Contains("farm.barn") || c.Contains("farm.greenhouse") || c.Contains("hall.restored");

        [Test]
        public void EveryTalkEntry_CanBeHeardInSomeState()
        {
            foreach (var npc in Npcs)
            {
                var set = _story.Set($"npc.{npc}.talk");
                var heard = new HashSet<string>();
                foreach (var hearts in new[] { 0, 3, 6, 10 })
                    foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter })
                        foreach (var weather in new[] { "sunny", "rain" })
                            foreach (var hour in new[] { 8, 12, 17, 21 })
                                foreach (var id in Heard(npc, Met(npc, hearts, season, weather, hour))) heard.Add(id);
                // Narrower sweeps for the other axes, so the whole test stays quick.
                foreach (var day in new[] { 1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 15, 16 })
                    heard.UnionWith(Heard(npc, Met(npc, 6, Season.Spring, "sunny", 12, day)));
                // The days around each festival (T-123) have their own lines.
                foreach (var (season, first) in new[] { (Season.Spring, 12), (Season.Summer, 10), (Season.Fall, 15), (Season.Winter, 24) })
                    for (var day = first; day < first + 3; day++)
                        heard.UnionWith(Heard(npc, Met(npc, 6, season, "sunny", 12, day)));
                foreach (var weather in new[] { "storm", "snow", "wind" })
                    heard.UnionWith(Heard(npc, Met(npc, 6, Season.Spring, weather, 12)));
                foreach (var map in new[] { "Saloon", "Library", "Blacksmith", "GeneralStore", "Carpenter", "Clinic", "Forest", "Beach", "Farm" })
                {
                    var there = Met(npc, 6);
                    there.MapName = map;
                    heard.UnionWith(Heard(npc, there));
                }
                var unmet = new World();
                heard.UnionWith(Heard(npc, unmet));
                // Quest entries have their own tests; every other entry must be reachable.
                foreach (var e in set.Entries.Where(e => e.Condition == null || !e.Condition.Contains("quest:") && !e.Condition.Contains("flag:invite.") && !e.Condition.Contains("flag:hazel.book") && !StateDependent(e.Condition)))
                    Assert.IsTrue(heard.Contains(e.Dialogue), $"{npc}: '{e.Dialogue}' ({e.Condition}) can never be heard");
            }
        }

        [Test]
        public void NoTierLeavesAPlayerWithAHandfulOfLines()
        {
            foreach (var npc in Npcs)
            {
                foreach (var season in new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter })
                {
                    Assert.GreaterOrEqual(Heard(npc, Met(npc, 0, season)).Count, 4, $"{npc} at 0 hearts in {season}");
                    Assert.GreaterOrEqual(Heard(npc, Met(npc, 4, season)).Count, 7, $"{npc} at 4 hearts in {season}");
                    Assert.GreaterOrEqual(Heard(npc, Met(npc, 8, season)).Count, 9, $"{npc} at 8 hearts in {season}");
                    Assert.GreaterOrEqual(Heard(npc, Met(npc, 8, season, "rain")).Count, 2, $"{npc} on a rainy day in {season}");
                }
            }
        }

        [Test]
        public void RainLines_StillPlayForCloseFriends()
        {
            foreach (var npc in Npcs)
                Assert.IsTrue(Heard(npc, Met(npc, 9, Season.Spring, "rain")).All(id => id.Contains(".rain")), npc);
        }

        [Test]
        public void TimeBoundLines_OnlyPlayAtTheirTime()
        {
            foreach (var season in new[] { Season.Summer, Season.Fall, Season.Winter })
                CollectionAssert.DoesNotContain(Heard("tilda", Met("tilda", 0, season)), "tilda.chat1", "the planting rush is spring talk");
            CollectionAssert.Contains(Heard("tilda", Met("tilda", 0, Season.Spring)), "tilda.chat1");
            foreach (var season in new[] { Season.Spring, Season.Summer, Season.Winter })
                CollectionAssert.DoesNotContain(Heard("ione", Met("ione", 0, season)), "ione.chat1", "the winter forecast is fall talk");
            CollectionAssert.DoesNotContain(Heard("wren", Met("wren", 0, Season.Spring, "sunny", 12)), "wren.chat1", "'after dark' at noon");
            CollectionAssert.Contains(Heard("wren", Met("wren", 0, Season.Spring, "sunny", 20)), "wren.chat1");
            CollectionAssert.DoesNotContain(Heard("bram", Met("bram", 0, Season.Spring, "sunny", 9)), "bram.chat3", "'early evening' at nine");
        }

        [Test]
        public void SeasonalLines_PlayInTheirSeasonOnly()
        {
            foreach (var npc in Npcs)
                foreach (var season in Seasons)
                    foreach (var other in Seasons.Where(s => s != season))
                        CollectionAssert.DoesNotContain(Heard(npc, Met(npc, 0, (Season)System.Array.IndexOf(Seasons, other))), $"{npc}.sea_{season}");
        }

        [Test]
        public void BramQuestOffer_IsNeverHiddenByFriendship()
        {
            foreach (var hearts in new[] { 1, 3, 5, 6, 8, 10 })
            {
                var w = Met("bram", hearts);
                w.Quests["bram_stone"] = "new";
                Assert.AreEqual(new[] { "bram.ask" }, Heard("bram", w).ToArray(), $"at {hearts} hearts");
            }
            var active = Met("bram", 8);
            active.Quests["bram_stone"] = "active";
            Assert.AreEqual(new[] { "bram.remind" }, Heard("bram", active).ToArray());
        }

        [Test]
        public void Invitations_AreAChoice_ThenGiveWayToAFollowUp()
        {
            foreach (var npc in new[] { "odalys", "piper", "elara", "marcus", "felix", "dorian" })
            {
                var d = _story.Dialogue($"{npc}.friend2");
                Assert.IsNotNull(d, npc);
                Assert.AreEqual(2, d.Nodes.First(n => n.Choices.Count > 0).Choices.Count, $"{npc} needs an accept and a decline");
                Assert.IsTrue(d.Nodes.SelectMany(n => n.Choices).Any(c => c.Effects.Contains($"flag:invite.{npc}")), $"{npc}: accepting must be remembered");

                var fresh = Met(npc, 4);
                CollectionAssert.Contains(Heard(npc, fresh), $"{npc}.friend2");
                CollectionAssert.DoesNotContain(Heard(npc, fresh), $"{npc}.friend2b");
                var accepted = Met(npc, 4);
                accepted.Flags.Add($"invite.{npc}");
                CollectionAssert.DoesNotContain(Heard(npc, accepted), $"{npc}.friend2");
                CollectionAssert.Contains(Heard(npc, accepted), $"{npc}.friend2b");
            }
        }

        [Test]
        public void HazelsFavouriteBook_IsAnsweredOnceAtFourHearts()
        {
            var w = Met("hazel", 3);
            w.Flags.Remove("hazel.book");
            CollectionAssert.DoesNotContain(Heard("hazel", w), "hazel.book");
            w.Hearts_["hazel"] = 4;
            Assert.AreEqual(new[] { "hazel.book" }, Heard("hazel", w).ToArray());
            w.Flags.Add("hazel.book");
            CollectionAssert.DoesNotContain(Heard("hazel", w), "hazel.book");
        }

        static Dictionary<string, string> Table() =>
            L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));

        [Test]
        public void WrensGossip_ActuallyGossips()
        {
            var table = Table();
            var runner = new DialogueRunner(_story.Dialogue("wren.first"), new World(), (key, args) => table[key], effect => { });
            var said = new List<string>();
            while (!runner.Finished)
            {
                said.Add(runner.Current.Text);
                if (runner.Current.HasOptions) runner.Choose(1); else runner.Advance();
            }
            StringAssert.Contains("jam", said.Last(), "asking for gossip must produce some");
        }

        [Test]
        public void NeutralGiftReactions_AreDistinctPerVillager()
        {
            var table = Table();
            var lines = Npcs.Select(n => table[$"dlg.npc.{n}.gift.neutral.0"]).ToList();
            Assert.AreEqual(lines.Count, lines.Distinct().Count());
        }
    }
}
