using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-097 and T-141: conversation topics, social actions and the chat menu that offers them.
    public class InteractionsTests
    {
        sealed class World : IWorldQuery, IGameQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public int Hearts_ = 5;
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Village";
            public int Hearts(string npcId) => Hearts_;
            public int ItemCount(string itemId) => 0;
            public string QuestState(string questId) => "new";
            public bool KnowsRecipe(string recipeId) => false;
        }

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        static TopicDefinition Topic(string id, string npc = "wren", int priority = 2, bool once = false, int cooldown = 1, string condition = null) =>
            new TopicDefinition { Id = id, Npc = npc, LabelKey = "topic." + id, Dialogue = id + ".d", Priority = priority, Once = once, CooldownDays = cooldown, Condition = condition };

        // ---- topics ----

        [Test]
        public void Topics_NeedTheirCondition_AndTheRightVillager()
        {
            var topics = new[] { Topic("a", condition: "hearts:wren>=9"), Topic("b"), Topic("c", npc: "bram") };
            var list = Topics.Available(topics, new World(), new InteractionState(), "wren", 10);
            CollectionAssert.AreEqual(new[] { "b" }, list.Select(t => t.Id));
        }

        [Test]
        public void AtMostTwoTopics_HighestPriorityFirst()
        {
            var topics = new[] { Topic("low", priority: 1), Topic("high", priority: 5), Topic("mid", priority: 3) };
            var list = Topics.Available(topics, new World(), new InteractionState(), "wren", 10);
            CollectionAssert.AreEqual(new[] { "high", "mid" }, list.Select(t => t.Id));
        }

        [Test]
        public void ATopic_IsAskedOncePerDay_ByDefault_AndAgainAfterItsCooldown()
        {
            var topics = new[] { Topic("a", cooldown: 3) };
            var state = new InteractionState();
            state.TopicLastAsked["a"] = 10;
            Assert.IsEmpty(Topics.Available(topics, new World(), state, "wren", 10));
            Assert.IsEmpty(Topics.Available(topics, new World(), state, "wren", 12));
            Assert.AreEqual(1, Topics.Available(topics, new World(), state, "wren", 13).Count);
        }

        [Test]
        public void AOnceTopic_NeverReturns()
        {
            var state = new InteractionState();
            state.TopicTimes["a"] = 1;
            state.TopicLastAsked["a"] = 1;
            Assert.IsEmpty(Topics.Available(new[] { Topic("a", once: true) }, new World(), state, "wren", 500));
            Assert.AreEqual(1, Topics.Available(new[] { Topic("a", once: false) }, new World(), state, "wren", 500).Count);
        }

        [Test]
        public void TheLeastAskedTopic_IsPreferredAmongEquals()
        {
            var topics = new[] { Topic("a"), Topic("b"), Topic("c") };
            var state = new InteractionState();
            state.TopicTimes["a"] = 4; state.TopicTimes["b"] = 2;
            var list = Topics.Available(topics, new World(), state, "wren", 10);
            CollectionAssert.AreEqual(new[] { "c", "b" }, list.Select(t => t.Id));
        }

        // ---- social actions ----

        [Test]
        public void Odds_FollowTheVillagersTaste()
        {
            var loves = new SocialProfile { Npc = "x", Loves = { "joke" } };
            var dislikes = new SocialProfile { Npc = "x", Dislikes = { "joke" } };
            int Count(SocialProfile p, SocialOutcome o) => Enumerable.Range(0, 2000).Count(i => SocialActions.Roll(p, "joke", i * 7919 + 3) == o);
            Assert.AreEqual(0, Count(loves, SocialOutcome.Flop), "a loved action never flops");
            Assert.Greater(Count(loves, SocialOutcome.Great), 800);
            Assert.AreEqual(0, Count(dislikes, SocialOutcome.Great), "a disliked action is never great");
            Assert.Greater(Count(dislikes, SocialOutcome.Flop), 800);
            Assert.AreEqual(2, SocialActions.Affinity(null, "joke"), "no profile is neutral");
        }

        [Test]
        public void TheRoll_IsDeterministic()
        {
            var p = new SocialProfile { Npc = "x", Likes = { "tease" } };
            for (var seed = 0; seed < 100; seed++)
                Assert.AreEqual(SocialActions.Roll(p, "tease", seed), SocialActions.Roll(p, "tease", seed));
            Assert.AreEqual(SocialActions.Seed("wren", "joke", 5, 0), SocialActions.Seed("wren", "joke", 5, 0));
            Assert.AreNotEqual(SocialActions.Seed("wren", "joke", 5, 0), SocialActions.Seed("wren", "joke", 5, 1), "the second try is a different roll");
        }

        [Test]
        public void Points_HalveOnTheSecondAction_AndStopAfterThat()
        {
            Assert.AreEqual(14, SocialActions.PointsFor(SocialOutcome.Great, 0));
            Assert.AreEqual(7, SocialActions.PointsFor(SocialOutcome.Great, 1));
            Assert.AreEqual(0, SocialActions.PointsFor(SocialOutcome.Great, 2));
            Assert.AreEqual(0, SocialActions.PointsFor(SocialOutcome.Flop, 0), "a flop is funny, never a loss");
            foreach (var p in SocialActions.Points) Assert.GreaterOrEqual(p, 0);
        }

        [Test]
        public void ReactionLookup_PrefersTheVillagersOwnLines_ThenTheGenericOnes()
        {
            var story = new StoryContent();
            story.AddJson(@"{ ""dialogues"": [
              { ""id"": ""social.joke.great"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""*"", ""text"": ""k"" } ] },
              { ""id"": ""social.wren.joke.great"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""k"" } ] } ] }", "t");
            Assert.AreEqual("social.wren.joke.great", SocialActions.ReactionDialogue(story, "wren", "joke", SocialOutcome.Great));
            Assert.AreEqual("social.joke.great", SocialActions.ReactionDialogue(story, "bram", "joke", SocialOutcome.Great));
            Assert.IsNull(SocialActions.ReactionDialogue(story, "bram", "tease", SocialOutcome.Great));
        }

        // ---- the menu, with the shipped story ----

        static StoryContent Story() => StoryContent.LoadFromResources();

        static DialogueGraph Menu(StoryContent story, string npc, InteractionState state = null, int day = 10) =>
            InteractionMenu.Build(story, npc, new World(), state ?? new InteractionState(), day, key => key == "menu.prompt." + npc);

        [Test]
        public void TheMenu_OffersTopics_ThenTheSocialSubmenu_ThenGoodbye()
        {
            var g = Menu(Story(), "wren");
            Assert.IsNotNull(g);
            var menu = g.Node("menu");
            Assert.AreEqual("menu.prompt.wren", menu.Text, "Wren has her own prompt");
            var texts = menu.Choices.Select(c => c.Text).ToList();
            Assert.AreEqual(InteractionMenu.GoodbyeKey, texts.Last());
            Assert.IsTrue(menu.Choices.Last().Default, "Goodbye starts highlighted, so a habitual Enter just ends the chat");
            Assert.AreEqual(1, menu.Choices.Count(c => c.Default));
            CollectionAssert.Contains(texts, InteractionMenu.SocialKey);
            Assert.LessOrEqual(texts.Count(t => t.StartsWith("topic.")), Topics.MaxOffered);
            Assert.GreaterOrEqual(texts.Count(t => t.StartsWith("topic.")), 1);
            var social = g.Node("social");
            CollectionAssert.AreEqual(new[] { "social.joke", "social.compliment", "social.advice", "social.tease", InteractionMenu.BackKey }, social.Choices.Select(c => c.Text));
            Assert.AreEqual("menu", social.Choices.Last().Next, "Back returns to the menu");
            Assert.IsTrue(social.Choices.Last().Default);
        }

        [Test]
        public void TheMenu_UsesTheGenericPrompt_WhenAVillagerHasNone()
        {
            var g = Menu(Story(), "wren");
            var generic = InteractionMenu.Build(Story(), "tilda", new World(), new InteractionState(), 10, key => false);
            Assert.AreEqual("menu.prompt", generic.Node("menu").Text);
            Assert.AreEqual("menu.prompt.wren", g.Node("menu").Text);
        }

        [Test]
        public void EveryVillager_GetsAMenu_FromTheGenericSocialLines()
        {
            var story = Story();
            foreach (var npc in new[] { "tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara" })
            {
                var g = Menu(story, npc);
                Assert.IsNotNull(g, npc);
                Assert.AreEqual(5, g.Node("social").Choices.Count, npc + ": four actions and Back");
            }
        }

        [Test]
        public void TheCopiedReactions_SpeakAsTheVillager()
        {
            var g = Menu(Story(), "elara");
            foreach (var node in g.Nodes.Where(n => n.Id.StartsWith("social:")))
                Assert.AreEqual("elara", node.Speaker, node.Id + ": '*' became the villager");
        }

        [Test]
        public void EachSocialChoice_RunsTheDoneEffect_WithTheOutcomeItShows()
        {
            var story = Story();
            var g = Menu(story, "wren");
            foreach (var c in g.Node("social").Choices.Where(c => c.Text.StartsWith("social.") && c.Text != "social.menu"))
            {
                var effect = c.Effects.Single();
                StringAssert.StartsWith("social.done:wren,", effect);
                var outcome = effect.Split(',').Last();
                var firstNode = g.Node(c.Next);
                Assert.IsNotNull(firstNode, c.Text);
                var dialogue = c.Next.Substring(c.Next.IndexOf(':') + 1).Split('/')[0];
                Assert.IsTrue(story.Dialogue($"social.wren.{dialogue}.{outcome}") != null || story.Dialogue($"social.{dialogue}.{outcome}") != null);
            }
        }

        [Test]
        public void AfterTwoActions_TheSocialOptionsAreGone_ButTopicsRemain()
        {
            var state = new InteractionState();
            state.RecordSocial("wren", 10);
            Assert.IsNotNull(Menu(Story(), "wren", state).Node("social"), "one action left");
            state.RecordSocial("wren", 10);
            var g = Menu(Story(), "wren", state);
            Assert.IsNull(g.Node("social"));
            CollectionAssert.DoesNotContain(g.Node("menu").Choices.Select(c => c.Text), InteractionMenu.SocialKey);
            Assert.IsTrue(g.Node("menu").Choices.Any(c => c.Text.StartsWith("topic.")));
            Assert.IsNotNull(Menu(Story(), "wren", state, day: 11).Node("social"), "a new day starts fresh");
        }

        [Test]
        public void WithNothingToOffer_ThereIsNoMenu()
        {
            var empty = new StoryContent();
            Assert.IsNull(InteractionMenu.Build(empty, "wren", new World(), new InteractionState(), 10, null));
        }

        [Test]
        public void CopiedNodes_KeepTheirChoicesAndEffects()
        {
            var story = new StoryContent();
            story.AddJson(@"{ ""dialogues"": [ { ""id"": ""t.d"", ""start"": ""a"", ""nodes"": [
                { ""id"": ""a"", ""speaker"": ""wren"", ""text"": ""k1"", ""effects"": [ ""flag:seen"" ], ""choices"": [ { ""text"": ""c1"", ""next"": ""b"", ""effects"": [ ""flag:picked"" ] } ] },
                { ""id"": ""b"", ""speaker"": ""wren"", ""text"": ""k2"" } ] } ],
              ""topics"": [ { ""id"": ""t"", ""npc"": ""wren"", ""labelKey"": ""topic.t"", ""dialogue"": ""t.d"" } ] }", "t");
            var g = InteractionMenu.Build(story, "wren", new World(), new InteractionState(), 1, null);
            var a = g.Node("topic:t/a");
            CollectionAssert.AreEqual(new[] { "flag:seen" }, a.Effects);
            Assert.AreEqual("topic:t/b", a.Choices[0].Next);
            CollectionAssert.AreEqual(new[] { "flag:picked" }, a.Choices[0].Effects);
            Assert.IsNull(g.Node("topic:t/b").Next, "the end of the topic ends the conversation");
            Assert.AreEqual("topic:t/a", g.Node("menu").Choices[0].Next);
        }

        // ---- effects, in a session ----

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        [Test]
        public void TheDoneEffects_RecordTopics_AndAwardSocialPoints()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null))
            {
                var s = f.Session;
                s.Story = Story();
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                var today = s.Clock.Now.TotalDays;

                Effects.Run(s, "topic.done:wren.gossip");
                var state = InteractionState.Load(s);
                Assert.AreEqual(today, state.TopicLastAsked["wren.gossip"]);
                Assert.AreEqual(1, state.TopicTimes["wren.gossip"]);

                Effects.Run(s, "social.done:wren,joke,great");
                Assert.AreEqual(14, s.State.Npcs["wren"].Points, "the first action is worth full points");
                Effects.Run(s, "social.done:wren,joke,great");
                Assert.AreEqual(14 + 7, s.State.Npcs["wren"].Points, "the second is worth half");
                Effects.Run(s, "social.done:wren,joke,great");
                Assert.AreEqual(14 + 7, s.State.Npcs["wren"].Points, "the third is worth nothing");
                Assert.AreEqual(3, InteractionState.Load(s).SocialToday("wren", today));
                Assert.AreEqual(today, s.State.Npcs["wren"].LastContactDay);
                Assert.AreEqual(3, s.GetVar("stat.social"));

                Effects.Run(s, "social.done:bram,tease,flop");
                Assert.AreEqual(0, s.State.Npcs["bram"].Points, "a flop costs nothing");
            }
        }

        [Test]
        public void AWholeSocialConversation_RunsThroughTheDialogueRunner()
        {
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, null))
            {
                var s = f.Session;
                s.Story = Story();
                s.Clock.SetTime(new GameDateTime(1, Season.Fall, 6, 12 * 60));
                var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
                var graph = InteractionMenu.Build(s.Story, "bram", s.World, InteractionState.Load(s), s.Clock.Now.TotalDays, table.ContainsKey);
                var runner = new DialogueRunner(graph, s.World, (k, a) => table.TryGetValue(k, out var t) ? t : k, e => Effects.Run(s, e));
                Assert.AreEqual("Hm?", runner.Current.Text, "Bram's own prompt");
                var socialIndex = runner.Current.Options.FindIndex(o => o.Text == table[InteractionMenu.SocialKey]);
                Assert.GreaterOrEqual(socialIndex, 0);
                runner.Choose(socialIndex);
                var adviceIndex = runner.Current.Options.FindIndex(o => o.Text == table["social.advice"]);
                runner.Choose(adviceIndex);
                Assert.IsFalse(runner.Finished);
                StringAssert.DoesNotContain("social:", runner.Current.Text);
                Assert.AreEqual("bram", runner.Current.Speaker);
                runner.Advance();
                Assert.IsTrue(runner.Finished, "the reaction ends the conversation");
                Assert.AreEqual(1, InteractionState.Load(s).SocialToday("bram", s.Clock.Now.TotalDays));
                Assert.Greater(s.State.Npcs["bram"].Points, 0, "Bram loves being asked for advice");
            }
        }

        // ---- FScript and validation ----

        [Test]
        public void FScript_CompilesTopicsAndSocialProfiles()
        {
            const string src = "topic a.topic\n  npc wren\n  label Ask about it\n  plays a.d\n  if hearts:wren>=2\n  priority 4\n  cooldown 3\n  once\n" +
                               "social wren\n  loves joke\n  likes advice, tease\n  dislikes compliment\n" +
                               "dialogue a.d\nn0 wren: Hello.\n";
            var r = FScript.Compile(src, "t");
            Assert.IsEmpty(r.Errors, string.Join("\n", r.Errors));
            var t = r.Topics.Single();
            Assert.AreEqual("a.topic", t.Id); Assert.AreEqual("wren", t.Npc); Assert.AreEqual("topic.a.topic", t.LabelKey);
            Assert.AreEqual("a.d", t.Dialogue); Assert.AreEqual("hearts:wren>=2", t.Condition);
            Assert.AreEqual(4, t.Priority); Assert.AreEqual(3, t.CooldownDays); Assert.IsTrue(t.Once);
            Assert.AreEqual("Ask about it", r.Texts["topic.a.topic"]);
            var p = r.Socials.Single();
            CollectionAssert.AreEqual(new[] { "joke" }, p.Loves);
            CollectionAssert.AreEqual(new[] { "advice", "tease" }, p.Likes);
            CollectionAssert.AreEqual(new[] { "compliment" }, p.Dislikes);
            var json = FScript.ToStoryJson(r);
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(json, "c"));
            Assert.IsEmpty(story.Errors);
            Assert.AreEqual(1, story.Topics.Count());
            Assert.AreEqual("wren", story.Socials.Single().Npc);
        }

        [TestCase("topic t\n  bogus 1\n", "unknown topic line")]
        [TestCase("topic t\n  priority high\n", "not a whole number")]
        [TestCase("topic t\ntopic t\n", "defined twice")]
        [TestCase("social wren\n  adores joke\n", "unknown social line")]
        [TestCase("social wren\nsocial wren\n", "defined twice")]
        [TestCase("topic\n", "needs an id")]
        public void FScript_ReportsBadTopicAndSocialLines(string src, string expected) =>
            Assert.IsTrue(FScript.Compile(src, "t").Errors.Any(e => e.Contains(expected)), expected);

        [Test]
        public void TheValidator_ChecksTopicsAndSocials()
        {
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var db = RealDb();
            var story = StoryContent.LoadFromResources();
            story.AddJson(@"{ ""topics"": [ { ""id"": ""bad.t"", ""npc"": ""nobody"", ""labelKey"": ""no.such.key"", ""dialogue"": ""missing"", ""cooldownDays"": 0, ""condition"": ""wat:1"" } ],
              ""socials"": [ { ""npc"": ""ghost"", ""loves"": [ ""flirt"", ""joke"" ], ""likes"": [ ""joke"" ] } ] }", "t");
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = story, Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey, RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            }).Where(p => p.StartsWith("topic bad.t") || p.StartsWith("social ghost")).ToList();
            foreach (var needle in new[] { "unknown villager", "unknown dialogue", "cooldownDays", "unknown action", "listed twice" })
                Assert.IsTrue(problems.Any(p => p.Contains(needle)), $"{needle}: {string.Join(" | ", problems)}");
        }

        [Test]
        public void TheShippedSocialContent_IsComplete()
        {
            var story = Story();
            foreach (var action in SocialActions.All)
                foreach (var outcome in new[] { "flop", "meh", "good", "great" })
                    Assert.IsNotNull(story.Dialogue($"social.{action}.{outcome}"), $"generic {action}.{outcome}");
            foreach (var npc in new[] { "wren", "hazel", "bram" })
            {
                Assert.IsTrue(story.Socials.Any(p => p.Npc == npc), npc + " has a profile");
                Assert.GreaterOrEqual(story.Topics.Count(t => t.Npc == npc), 2, npc + " has topics");
                foreach (var action in SocialActions.All)
                    foreach (var outcome in new[] { "flop", "good", "great" })
                        Assert.IsNotNull(story.Dialogue($"social.{npc}.{action}.{outcome}"), $"{npc} {action}.{outcome}");
            }
        }

        [Test]
        public void TheChatMenuSetting_DefaultsOn()
        {
            Assert.IsTrue(new SettingsData().ChatMenu);
            Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsData>("{\"MasterVolume\":0.5}").ChatMenu, "old settings files get the default");
        }

        // Playtest 2026-10-06: a topic or a social reply ended the conversation. Now its last line asks for the menu again.
        [Test]
        public void TheLastLineOfEveryTopicAndReaction_AsksForTheMenuAgain()
        {
            var g = Menu(Story(), "wren");
            var ends = g.Nodes.Where(n => (n.Id.StartsWith("topic:") || n.Id.StartsWith("social:")) && string.IsNullOrEmpty(n.Next) && n.Choices.Count == 0).ToList();
            Assert.IsNotEmpty(ends);
            foreach (var n in ends) CollectionAssert.Contains(n.Effects, "talk.again", n.Id);
        }
    }
}
