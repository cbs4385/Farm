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
    // T-040: the shipped story data passes the validator, and the validator catches each kind of mistake.
    public class DataValidationTests
    {
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";

        static ValidationInput Real()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            BusinessHoursRegistry.RegisterConditionAtom();
            var table = L.Parse(File.ReadAllText(TablePath));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            Assert.IsNotNull(db);
            return new ValidationInput
            {
                Story = StoryContent.LoadFromResources(),
                Db = db,
                Npcs = NpcCatalog.From(db),
                HasKey = table.ContainsKey,
                RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            };
        }

        [Test]
        public void ShippedData_HasNoProblems()
        {
            var problems = StoryValidator.Run(Real());
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void MythosData_HasNoProblems_AndItsStringsExist()
        {
            var input = Real();
            Farm.Mythos.MythosModule.RegisterConditions();
            Farm.Mythos.MythosEffects.Register();
            foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) input.Db.Merge(pack);
            Farm.Mythos.MythosContent.Load(input.Story);
            Assert.IsNotNull(input.Story.Dialogue("mythos.altar"));
            var problems = StoryValidator.Run(input);
            input.Db.ClearMergedPacks();
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void ShippedStory_IsLoadedFromTheResourcesFolder()
        {
            var story = StoryContent.LoadFromResources();
            Assert.Greater(story.Dialogues.Count(), 30);
            foreach (var id in NpcIds.All) Assert.IsNotNull(story.Set($"npc.{id}.talk"), id);
        }

        sealed class Harness
        {
            public readonly StoryContent Story = new StoryContent();
            public readonly HashSet<string> Keys = new HashSet<string> { "k" };
            public ValidationInput Input;

            public Harness(string json)
            {
                Conditions.ClearCustomForTests();
                StoryConditions.Register();
                var items = new[] { ItemDefinition.Create("crop.parsnip", ItemCategory.Crop) };
                var db = GameDatabase.Create(items, new CropDefinition[0]);
                db.SetNpcs(new NpcDefinition[0]);
                Story.AddJson(json, "test");
                Input = new ValidationInput { Story = Story, Db = db, Npcs = new NpcCatalog(NpcDefaults.CreateAll()), HasKey = Keys.Contains };
            }

            public List<string> Run() => StoryValidator.Run(Input).Where(p => p.StartsWith("dialogue") || p.StartsWith("set") || p.StartsWith("quest") || p.StartsWith("letter") || p.StartsWith("event") || p.StartsWith("random") || p.StartsWith("board")).ToList();
        }

        [Test]
        public void Dialogue_Mistakes_AreReported()
        {
            var h = new Harness(@"{ ""dialogues"": [ { ""id"": ""bad"", ""start"": ""gone"", ""nodes"": [
                { ""id"": ""a"", ""speaker"": ""nobody"", ""text"": ""missing.key"", ""condition"": ""flag:"", ""next"": ""nowhere"", ""effects"": [""explode:1"", ""give:no.item,1"", ""friend:nobody,5""] },
                { ""id"": ""a"", ""text"": ""k"" } ] } ] }");
            var problems = string.Join("\n", h.Run());
            StringAssert.Contains("start node 'gone'", problems);
            StringAssert.Contains("duplicate or empty node id 'a'", problems);
            StringAssert.Contains("unknown speaker 'nobody'", problems);
            StringAssert.Contains("string key 'missing.key'", problems);
            StringAssert.Contains("next 'nowhere'", problems);
            StringAssert.Contains("unknown effect 'explode'", problems);
            StringAssert.Contains("unknown item 'no.item'", problems);
            StringAssert.Contains("unknown NPC 'nobody'", problems);
            StringAssert.Contains("Condition", problems);
        }

        [Test]
        public void Set_Mistakes_AreReported()
        {
            var h = new Harness(@"{ ""sets"": [ { ""id"": ""s"", ""entries"": [ { ""dialogue"": ""ghost"", ""condition"": ""moon:purple"" } ] }, { ""id"": ""empty"", ""entries"": [] } ] }");
            var problems = string.Join("\n", h.Run());
            StringAssert.Contains("unknown dialogue 'ghost'", problems);
            StringAssert.Contains("unknown moon phase", problems);
            StringAssert.Contains("has no entries", problems);
        }

        [Test]
        public void ACleanDialogue_Passes()
        {
            var h = new Harness(@"{ ""dialogues"": [ { ""id"": ""ok"", ""start"": ""a"", ""nodes"": [
                { ""id"": ""a"", ""speaker"": ""tilda"", ""text"": ""k"", ""effects"": [""flag:x"", ""give:crop.parsnip,2"", ""friend:tilda,5""],
                  ""choices"": [ { ""text"": ""k"", ""condition"": ""var:dread<3"", ""next"": ""b"" } ] },
                { ""id"": ""b"", ""text"": ""k"" } ] } ] }");
            CollectionAssert.IsEmpty(h.Run());
        }

        [Test]
        public void AllEffectVerbs_AreKnownToTheValidator()
        {
            foreach (var verb in new[] { "flag", "unflag", "setvar", "addvar", "gold", "give", "take", "toast", "xp", "friend" })
                Assert.IsTrue(Effects.IsKnown(verb), verb);
        }
    }
}
