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
    // T-140: the moment framework: one catalog of tagged moments, quotas that count them all, and the clip-worthiness
    // checklist for set-piece scenes.
    public class MomentsTests
    {
        static readonly string[] Villagers = { "wren", "hazel", "bram", "tilda", "ione", "marcus", "odalys", "felix", "juno", "piper", "dorian", "elara" };

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
        }

        const string Json = @"{
          ""dialogues"": [
            { ""id"": ""wren.chat1"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""k"", ""tag"": ""funny"" } ] },
            { ""id"": ""social.hazel.joke.great"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""hazel"", ""text"": ""k"", ""tag"": ""wholesome"" } ] },
            { ""id"": ""plain"", ""start"": ""n0"", ""nodes"": [ { ""id"": ""n0"", ""speaker"": ""bram"", ""text"": ""k"" } ] } ],
          ""sets"": [ { ""id"": ""npc.wren.talk"", ""entries"": [ { ""priority"": 0, ""dialogue"": ""wren.chat1"", ""tag"": ""funny"", ""rarity"": ""rare"" }, { ""priority"": 0, ""dialogue"": ""plain"" } ] } ],
          ""events"": [ { ""id"": ""wren_heart2"", ""trigger"": ""manual"", ""tag"": ""surprise"", ""titleKey"": ""t"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 } ] },
                        { ""id"": ""festival_x"", ""trigger"": ""manual"", ""tag"": ""wholesome"", ""calendar"": ""c"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 } ] },
                        { ""id"": ""untagged"", ""trigger"": ""manual"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 } ] } ] }";

        static StoryContent Story()
        {
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(Json, "t"));
            return story;
        }

        [Test]
        public void TheCatalog_FindsEveryTaggedMoment_AndWhoOwnsIt()
        {
            var moments = MomentCatalog.Build(Story(), Villagers);
            Assert.AreEqual(5, moments.Count, string.Join(", ", moments.Select(m => m.Kind + ":" + m.Id)));
            Assert.AreEqual(1, moments.Count(m => m.Kind == MomentKind.Line));
            Assert.AreEqual(2, moments.Count(m => m.Kind == MomentKind.Node));
            Assert.AreEqual(2, moments.Count(m => m.Kind == MomentKind.Scene));
            Assert.AreEqual("rare", moments.Single(m => m.Kind == MomentKind.Line).Rarity);
            Assert.AreEqual("wren", moments.Single(m => m.Id == "wren_heart2").Villager, "a heart event belongs to its villager");
            Assert.AreEqual(string.Empty, moments.Single(m => m.Id == "festival_x").Villager, "a festival belongs to nobody");
            Assert.AreEqual("hazel", moments.Single(m => m.Id == "social.hazel.joke.great/n0").Villager);
            Assert.AreEqual(2, MomentCatalog.Scenes(moments));
        }

        [Test]
        public void Counts_AreByVillagerAndTag()
        {
            var moments = MomentCatalog.Build(Story(), Villagers);
            Assert.AreEqual(2, MomentCatalog.Count(moments, "wren", "funny"), "the talk line and the node");
            Assert.AreEqual(1, MomentCatalog.Count(moments, "wren", "surprise"), "the scene");
            Assert.AreEqual(0, MomentCatalog.Count(moments, "bram", "funny"));
        }

        [Test]
        public void TheQuotas_CountNodesAndScenes_NotOnlyTalkLines()
        {
            var totals = NarrativeLint.TagCounts(Story(), "wren", Villagers);
            Assert.AreEqual(2, totals.Funny);
            Assert.AreEqual(1, totals.Surprise);
            Assert.AreEqual(1, totals.Rare, "rarity still counts talk lines");
            var hazel = NarrativeLint.TagCounts(Story(), "hazel", Villagers);
            Assert.AreEqual(1, hazel.Wholesome, "a tagged social reaction counts for Hazel");
        }

        // ---- the checklist ----

        static EventStep Say(string text = "k") => new EventStep { Type = "say", Speaker = "wren", Text = text };
        static string Text(string key) => key == "long" ? string.Join(" ", Enumerable.Repeat("word", 33)) : "A short line.";

        static EventDefinition Scene(params EventStep[] steps) =>
            new EventDefinition { Id = "wren_heart9", Tag = "funny", TitleKey = "memory.x", Steps = steps.ToList() };

        [Test]
        public void ASceneThatPassesTheChecklist_HasNoProblems()
        {
            var scene = Scene(new EventStep { Type = "emote", Actor = "wren", Name = "note", Seconds = 1 }, Say());
            CollectionAssert.IsEmpty(MomentChecklist.Problems(scene, Text));
        }

        [Test]
        public void Problems_AreFoundOneByOne()
        {
            var untitled = Scene(new EventStep { Type = "emote", Actor = "wren", Name = "note" }, Say());
            untitled.TitleKey = null;
            Assert.IsTrue(MomentChecklist.Problems(untitled, Text).Any(p => p.Contains("title")));
            Assert.IsTrue(MomentChecklist.Problems(Scene(Say(), Say()), Text).Any(p => p.Contains("no visible or audible beat")));
            Assert.IsTrue(MomentChecklist.Problems(Scene(new EventStep { Type = "emote", Actor = "wren", Name = "note" }, Say(), new EventStep { Type = "effects" }, new EventStep { Type = "move", Actor = "wren" }), Text)
                .Any(p => p.Contains("end on a spoken line")));
            var slow = Scene(new EventStep { Type = "emote", Actor = "wren", Name = "note" }, new EventStep { Type = "wait", Seconds = 200 }, Say());
            Assert.IsTrue(MomentChecklist.Problems(slow, Text).Any(p => p.Contains("seconds long")));
        }

        [Test]
        public void AnUntaggedScene_IsNotChecked() =>
            CollectionAssert.IsEmpty(MomentChecklist.Problems(new EventDefinition { Id = "x", Steps = { Say(), Say() } }, Text));

        [Test]
        public void TheDurationEstimate_AddsReadingWalkingAndWaiting()
        {
            var scene = Scene(Say("long"), new EventStep { Type = "wait", Seconds = 2 }, new EventStep { Type = "move", Actor = "wren" });
            var seconds = MomentChecklist.EstimateSeconds(scene, Text);
            Assert.Greater(seconds, 2f + 3f + 5f, "33 words take about ten seconds");
            Assert.Less(seconds, 20f);
            Assert.Less(MomentChecklist.EstimateSeconds(Scene(Say("short")), Text), MomentChecklist.EstimateSeconds(Scene(Say("long")), Text));
            var async = Scene(new EventStep { Type = "anim", Actor = "wren", Name = "hop", Seconds = 5, Async = true });
            Assert.AreEqual(0f, MomentChecklist.EstimateSeconds(async, Text), "a background step adds no time");
        }

        [Test]
        public void Beats_AndEndings_AreCounted()
        {
            var group = new EventStep { Type = "parallel", Steps = { new EventStep { Type = "anim" }, new EventStep { Type = "emote" } } };
            Assert.AreEqual(2, MomentChecklist.PresentationBeats(Scene(group)));
            Assert.IsTrue(MomentChecklist.EndsOnALine(Scene(Say(), new EventStep { Type = "effects" }, new EventStep { Type = "lighting" })), "housekeeping after the last line does not count");
            Assert.IsTrue(MomentChecklist.EndsOnALine(Scene(new EventStep { Type = "dialogue", Dialogue = "x" })));
            Assert.IsFalse(MomentChecklist.EndsOnALine(Scene(Say(), new EventStep { Type = "wait" })));
            Assert.IsFalse(MomentChecklist.EndsOnALine(Scene()));
        }

        [Test]
        public void EveryTemplate_ProducesAScene_ThatPassesTheChecklist()
        {
            var story = StoryContent.LoadFromResources();
            var table = new Dictionary<string, string>();
            string K(string n) => "dlg.wren.chat" + n + ".0";
            var instances = new Newtonsoft.Json.Linq.JArray();
            foreach (var t in new[] { "shared_activity", "confession", "heirloom", "shared_meal", "prank", "performance" })
            {
                var args = new Newtonsoft.Json.Linq.JObject { ["npc"] = "wren", ["map"] = "Saloon", ["px"] = 6, ["py"] = 5 };
                foreach (var key in new[] { "intro", "doing", "wrap", "give", "thanks", "bite", "setup", "prank", "reaction", "outro", "after" }) args[key] = K("1");
                args["nx"] = 6; args["ny"] = 6; args["item"] = "crop.strawberry"; args["dish"] = "food.mashed_potato"; args["talk"] = "wren.first"; args["cue"] = "c";
                var template = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("Assets/_Project/Resources/Story/event_templates.json"))["eventTemplates"].First(x => (string)x["id"] == t);
                var declared = ((Newtonsoft.Json.Linq.JArray)template["params"]).Select(x => (string)x).ToList();
                var trimmed = new Newtonsoft.Json.Linq.JObject();
                foreach (var p in args.Properties()) if (declared.Contains(p.Name)) trimmed[p.Name] = p.Value;
                instances.Add(new Newtonsoft.Json.Linq.JObject { ["id"] = "tt_" + t, ["template"] = t, ["args"] = trimmed, ["event"] = new Newtonsoft.Json.Linq.JObject { ["tag"] = "funny", ["titleKey"] = "memory.wren_heart2" } });
            }
            story.AddJson(new Newtonsoft.Json.Linq.JObject { ["eventsFromTemplates"] = instances }.ToString(), "i");
            var en = Farm.Core.L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            foreach (var id in instances.Select(i => (string)i["id"]))
            {
                var problems = MomentChecklist.Problems(story.Event(id), k => en.TryGetValue(k, out var v) ? v : "x");
                CollectionAssert.IsEmpty(problems, id + ": " + string.Join(" | ", problems));
            }
        }

        // ---- the shipped content ----

        [Test]
        public void EveryShippedScene_IsTagged_SoTheSetPieceTargetIsMet()
        {
            var story = StoryContent.LoadFromResources();
            var moments = MomentCatalog.Build(story, Villagers);
            Assert.GreaterOrEqual(MomentCatalog.Scenes(moments), 25, "the stream-appeal target of tagged set pieces");
            foreach (var ev in Memories.All(story)) Assert.IsFalse(string.IsNullOrEmpty(ev.Tag), ev.Id + " is tagged");
            foreach (var tag in DialogueVocabulary.MomentTags.Where(t => t != "mystery")) Assert.Greater(moments.Count(m => m.Tag == tag), 0, tag);
        }

        [Test]
        public void TheReport_ListsMoments_AndTheChecklistWarnings()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var issues = NarrativeLint.Run(story, table, null, Villagers);
            var report = NarrativeLint.Report(story, table, null, Villagers, issues);
            StringAssert.Contains("## Moments", report);
            StringAssert.Contains("tagged moment(s)", report);
            Assert.IsFalse(issues.Any(i => i.Severity == LintSeverity.Error), "the checklist only warns");
            Assert.IsTrue(issues.Any(i => i.Rule == "moment"), "the older scenes have no emotes, gestures or sounds yet, and the checklist says so");
        }

        [Test]
        public void ALockedVillagersScene_MustPassTheChecklist()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var config = new NarrativeConfig { Locked = { "wren" } };
            var issues = NarrativeLint.Run(story, table, config, Villagers);
            Assert.IsTrue(issues.Any(i => i.Rule == "moment" && i.Villager == "wren" && i.Severity == LintSeverity.Error));
        }

        [Test]
        public void TheValidator_RejectsAnUnknownSceneTag()
        {
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var story = StoryContent.LoadFromResources();
            story.AddJson(@"{ ""events"": [ { ""id"": ""bad_tag"", ""trigger"": ""manual"", ""tag"": ""hilarious"", ""steps"": [ { ""type"": ""wait"", ""seconds"": 1 } ] } ] }", "t");
            var problems = StoryValidator.Run(new ValidationInput
            {
                Story = story, Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey, RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            });
            Assert.IsTrue(problems.Any(p => p.Contains("bad_tag") && p.Contains("hilarious")));
        }
    }
}
