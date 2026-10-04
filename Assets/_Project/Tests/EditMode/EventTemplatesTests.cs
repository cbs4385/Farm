using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-103: the heart-event template library and the expander behind it.
    public class EventTemplatesTests
    {
        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        // ---- the expander ----

        static JObject Template(string json) => JObject.Parse(json);

        const string Small = @"{ ""id"": ""t"", ""params"": [""npc"", ""x""], ""defaults"": { ""when"": ""true"", ""list"": [] },
          ""event"": { ""trigger"": ""manual"", ""condition"": ""${when}"", ""steps"": [
            { ""type"": ""move"", ""actor"": ""${npc}"", ""x"": ""${x}"", ""y"": 2 },
            { ""type"": ""say"", ""speaker"": ""${npc}"", ""text"": ""line.${id}.1"" },
            { ""type"": ""effects"", ""effects"": [""flag:a_${id}"", ""${list}""] } ] } }";

        static bool Expand(string instance, out JObject result, out string error, string template = Small) =>
            EventTemplates.TryExpand(Template(template), JObject.Parse(instance), out result, out error);

        [Test]
        public void Placeholders_AreFilled_AsTextAndAsWholeValues()
        {
            Assert.IsTrue(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"", ""x"": 6 } }", out var e, out var error), error);
            Assert.AreEqual("e1", (string)e["id"]);
            Assert.AreEqual("true", (string)e["condition"], "the default");
            var steps = (JArray)e["steps"];
            Assert.AreEqual("wren", (string)steps[0]["actor"]);
            Assert.AreEqual(JTokenType.Integer, steps[0]["x"].Type, "a whole placeholder keeps its type: a number stays a number");
            Assert.AreEqual(6, (int)steps[0]["x"]);
            Assert.AreEqual("line.e1.1", (string)steps[1]["text"], "{id} inside text");
        }

        [Test]
        public void AListArgument_IsSplicedIntoAList()
        {
            Assert.IsTrue(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"", ""x"": 1, ""list"": [""gold:5"", ""friend:wren,10""] } }", out var e, out var error), error);
            var effects = ((JArray)e["steps"][2]["effects"]).Select(t => (string)t).ToList();
            CollectionAssert.AreEqual(new[] { "flag:a_e1", "gold:5", "friend:wren,10" }, effects);
            Assert.IsTrue(Expand(@"{ ""id"": ""e2"", ""args"": { ""npc"": ""wren"", ""x"": 1 } }", out var none, out error));
            CollectionAssert.AreEqual(new[] { "flag:a_e2" }, ((JArray)none["steps"][2]["effects"]).Select(t => (string)t), "the empty default adds nothing");
        }

        [Test]
        public void Overrides_ReplaceTopLevelFields()
        {
            Assert.IsTrue(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"", ""x"": 1, ""when"": ""hearts:wren>=4"" }, ""event"": { ""priority"": 9, ""titleKey"": ""memory.e1"" } }", out var e, out var error), error);
            Assert.AreEqual(9, (int)e["priority"]);
            Assert.AreEqual("memory.e1", (string)e["titleKey"]);
            Assert.AreEqual("hearts:wren>=4", (string)e["condition"]);
        }

        [Test]
        public void AMissingParameter_IsAnError()
        {
            Assert.IsFalse(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"" } }", out _, out var error));
            StringAssert.Contains("needs x", error);
        }

        [Test]
        public void AnUnknownArgument_IsAnError_BecauseItIsATypo()
        {
            Assert.IsFalse(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"", ""x"": 1, ""nope"": 3 } }", out _, out var error));
            StringAssert.Contains("no parameter 'nope'", error);
        }

        [Test]
        public void AnUnresolvedPlaceholder_IsAnError()
        {
            const string broken = @"{ ""id"": ""t"", ""params"": [""npc""], ""event"": { ""trigger"": ""manual"", ""steps"": [ { ""type"": ""say"", ""text"": ""${npc} ${ghost}"" } ] } }";
            Assert.IsFalse(Expand(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"" } }", out _, out var error, broken));
            StringAssert.Contains("ghost", error);
            const string whole = @"{ ""id"": ""t"", ""params"": [], ""event"": { ""trigger"": ""manual"", ""steps"": [ { ""type"": ""say"", ""text"": ""${ghost}"" } ] } }";
            Assert.IsFalse(Expand(@"{ ""id"": ""e1"" }", out _, out error, whole));
        }

        [Test]
        public void AnInstanceNeedsAnId() =>
            Assert.IsFalse(Expand(@"{ ""args"": { ""npc"": ""wren"", ""x"": 1 } }", out _, out _));

        [Test]
        public void TheTemplate_IsNotChangedByExpanding()
        {
            var template = Template(Small);
            var before = template.ToString();
            EventTemplates.TryExpand(template, JObject.Parse(@"{ ""id"": ""e1"", ""args"": { ""npc"": ""wren"", ""x"": 1 } }"), out _, out _);
            Assert.AreEqual(before, template.ToString());
        }

        // ---- the loader ----

        [Test]
        public void TheLoader_ExpandsInstances_AndReportsBadOnes()
        {
            var story = new StoryContent();
            story.AddJson("{ \"eventTemplates\": [" + Small + "] }", "lib");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            story.AddJson(@"{ ""eventsFromTemplates"": [
                { ""id"": ""good"", ""template"": ""t"", ""args"": { ""npc"": ""wren"", ""x"": 3 } },
                { ""id"": ""missing"", ""template"": ""t"", ""args"": { ""npc"": ""wren"" } },
                { ""id"": ""unknown"", ""template"": ""nope"", ""args"": {} },
                { ""id"": ""good"", ""template"": ""t"", ""args"": { ""npc"": ""bram"", ""x"": 3 } } ] }", "inst");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            Assert.IsNotNull(story.Event("good"));
            Assert.IsNull(story.Event("missing"));
            Assert.IsNull(story.Event("unknown"));
            Assert.AreEqual("wren", story.Event("good").Steps[0].Actor, "the first definition wins; a duplicate is rejected");
            Assert.AreEqual(3, story.Errors.Count, string.Join(" | ", story.Errors));
        }

        [Test]
        public void ATemplate_IsDefinedOnce()
        {
            var story = new StoryContent();
            story.AddJson("{ \"eventTemplates\": [" + Small + "] }", "a");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            story.AddJson("{ \"eventTemplates\": [" + Small + "] }", "b");
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(1, story.Errors.Count);
        }

        // ---- the shipped library ----

        static readonly string[] Library = { "shared_activity", "confession", "heirloom", "shared_meal", "helping_scene", "prank", "performance" };

        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        // Sample arguments for each template, on Wren's saloon scene. Text keys are real keys, so the full validator is happy.
        static JObject Args(string template, string id)
        {
            var args = new JObject
            {
                ["npc"] = "wren", ["map"] = "Saloon", ["px"] = 6, ["py"] = 5,
                ["when"] = "hearts:wren>=4 && !weekday:tue",
                ["reward"] = new JArray("friend:wren,40", "give:artisan.juice,2"),
            };
            string K(string n) => "dlg.wren.chat" + n + ".0";
            switch (template)
            {
                case "shared_activity": args["intro"] = K("1"); args["doing"] = K("2"); args["wrap"] = K("3"); break;
                case "confession": args["intro"] = K("1"); args["talk"] = "wren.first"; args["after"] = K("2"); break;
                case "heirloom": args["nx"] = 6; args["ny"] = 6; args["item"] = "crop.strawberry"; args["intro"] = K("1"); args["give"] = K("2"); args["thanks"] = K("3"); break;
                case "shared_meal": args["nx"] = 6; args["ny"] = 6; args["dish"] = "food.mashed_potato"; args["intro"] = K("1"); args["bite"] = K("2"); args["wrap"] = K("3"); break;
                case "helping_scene": args["need"] = "has:resource.copperbar>=1"; args["ask"] = K("1"); args["success"] = K("2"); args["fallback"] = K("3"); break;
                case "prank": args["setup"] = K("1"); args["prank"] = K("2"); args["talk"] = "wren.first"; args["reaction"] = K("3"); break;
                case "performance": args["intro"] = K("1"); args["cue"] = "open_mic"; args["outro"] = K("2"); break;
            }
            return args;
        }

        static StoryContent StoryWithInstances(string idPrefix = "t_")
        {
            var story = StoryContent.LoadFromResources();
            var instances = new JArray(Library.Select(t => new JObject { ["id"] = idPrefix + t, ["template"] = t, ["args"] = Args(t, idPrefix + t) }));
            story.AddJson(new JObject { ["eventsFromTemplates"] = instances }.ToString(), "instances");
            return story;
        }

        [Test]
        public void TheShippedLibrary_HasTheSevenPatterns_AndLoadsCleanly()
        {
            var story = StoryContent.LoadFromResources();
            Assert.IsEmpty(story.Errors, string.Join("\n", story.Errors));
            CollectionAssert.IsSupersetOf(story.EventTemplateIds, Library);
        }

        [Test]
        public void EveryTemplate_Instantiates_WithNoStepProblems_AndNoLeftoverPlaceholders()
        {
            var story = StoryWithInstances();
            Assert.IsEmpty(story.Errors, string.Join("\n", story.Errors));
            var db = RealDb();
            var npcs = NpcCatalog.From(db).All.Select(n => n.Id).ToList();
            var items = db.AllItems.Select(i => i.Id).ToList();
            foreach (var t in Library)
            {
                var ev = story.Event("t_" + t);
                Assert.IsNotNull(ev, t);
                CollectionAssert.IsEmpty(EventSteps.Problems(ev, npcs.Contains, items.Contains, id => story.Dialogue(id) != null), t);
                Assert.IsFalse(Newtonsoft.Json.JsonConvert.SerializeObject(ev).Contains("${"), t + " has no unresolved placeholder");
                Assert.AreEqual("Saloon", ev.Map);
                Assert.AreEqual("hearts:wren>=4 && !weekday:tue", ev.Condition.Replace("!flag:event.t_helping_scene && ", ""), t);
                CollectionAssert.Contains(Memories.Cast(ev), "wren", t + ": Wren is in the cast");
            }
        }

        [Test]
        public void EveryTemplate_PassesTheFullDataValidator()
        {
            var story = StoryWithInstances();
            var db = RealDb();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = story, Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey, RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            }).Where(p => p.Contains("t_")).ToList();
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void EverySceneEndsByMarkingItselfDone()
        {
            var story = StoryWithInstances();
            foreach (var t in Library)
            {
                var ev = story.Event("t_" + t);
                var flags = ev.Steps.SelectMany(s => s.Effects).Where(e => e == "flag:event.t_" + t).ToList();
                Assert.AreEqual(1, flags.Count, t + " sets its own done flag exactly once");
            }
        }

        [Test]
        public void TheRewards_AreAppended_AfterTheFlag()
        {
            var ev = StoryWithInstances().Event("t_shared_activity");
            var effects = ev.Steps.Where(s => s.Type == "effects").Select(s => s.Effects).ToList();
            CollectionAssert.AreEqual(new[] { "friend:wren,40", "give:artisan.juice,2" }, effects.Last());
        }

        sealed class World : IWorldQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Saloon";
        }

        [Test]
        public void TheHelpingScene_SucceedsOrAsksAgain()
        {
            var ev = StoryWithInstances().Event("t_helping_scene");
            Assert.IsFalse(ev.Once, "it is offered again until the player can help");
            StringAssert.Contains("!flag:event.t_helping_scene", ev.Condition);

            string[] Texts(World w) => EventFlow.PathFrom(ev.Steps, 0, w).Select(i => ev.Steps[i]).Where(s => s.Type == "say").Select(s => s.Text).ToArray();
            var cannot = Texts(new World());
            Assert.Contains("dlg.wren.chat3.0", cannot, "no item: the fallback line");
            Assert.IsFalse(cannot.Contains("dlg.wren.chat2.0"));
            // With the item the condition `has:` needs the game's query, so check the branch through a flag-based condition instead.
            var viaFlag = StoryWithInstances("u_");
            var copy = viaFlag.Event("u_helping_scene");
            copy.Steps.First(s => s.Type == "branch" && s.Target == "ok").Condition = "flag:has_it";
            var helped = new World();
            helped.Flags.Add("has_it");
            var okTexts = EventFlow.PathFrom(copy.Steps, 0, helped).Select(i => copy.Steps[i]).Where(s => s.Type == "say").Select(s => s.Text).ToArray();
            Assert.Contains("dlg.wren.chat2.0", okTexts);
            Assert.IsFalse(okTexts.Contains("dlg.wren.chat3.0"));
            var skipEffects = EventFlow.PathFrom(copy.Steps, 0, helped).Select(i => copy.Steps[i]).Where(s => s.Type == "effects").SelectMany(s => s.Effects).ToList();
            CollectionAssert.Contains(skipEffects, "flag:event.u_helping_scene");
            var failEffects = EventFlow.PathFrom(copy.Steps, 0, new World()).Select(i => copy.Steps[i]).Where(s => s.Type == "effects").SelectMany(s => s.Effects).ToList();
            CollectionAssert.DoesNotContain(failEffects, "flag:event.u_helping_scene", "failing does not use the scene up");
        }

        [Test]
        public void TheDescriptions_ExplainEachPattern()
        {
            var raw = JObject.Parse(File.ReadAllText("Assets/_Project/Resources/Story/event_templates.json"));
            foreach (var t in raw["eventTemplates"])
                Assert.Greater(((string)t["description"]).Length, 40, (string)t["id"]);
        }
    }
}
