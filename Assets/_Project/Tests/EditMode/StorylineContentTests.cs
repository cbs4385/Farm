using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // The five village storylines (T-143): variants, talk lines, scenes and the done flags that tie them together.
    public class StorylineContentTests
    {
        static readonly string[] Ids = { "pie_feud", "anonymous_notes", "lost_umbrella", "rival_scarecrows", "mystery_whistler", "competing_band", "missing_pumpkin", "five_names_cat" };

        StoryContent _story;

        [SetUp]
        public void SetUp()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
        }

        IEnumerable<DialogueSetEntry> AllTalk() => _story.Sets.SelectMany(s => s.Entries);

        [Test]
        public void AllEightStorylines_AreDefined_AndEachHasTalkLinesAndABeatAfterTheScene()
        {
            var defs = _story.Storylines.ToList();
            foreach (var id in Ids)
            {
                Assert.IsTrue(defs.Any(d => d.Id == id), id);
                var lines = AllTalk().Where(e => e.Condition != null && e.Condition.Contains("storyline:" + id)).ToList();
                Assert.GreaterOrEqual(lines.Count, 4, id + ": talk lines");
                Assert.IsTrue(AllTalk().Any(e => e.Condition != null && e.Condition.Contains("flag:storydone." + id)), id + ": a line after the payoff");
            }
        }

        [Test]
        public void EachStoryline_HasAPayoffScene_ThatSetsItsDoneFlag()
        {
            foreach (var id in Ids)
            {
                var scenes = _story.Events.Where(e => e.Steps.Any(s => s.Type == "effects" && s.Effects != null && s.Effects.Contains("flag:storydone." + id))).ToList();
                Assert.IsNotEmpty(scenes, id);
                foreach (var e in scenes)
                {
                    StringAssert.Contains("storyline:" + id, e.Condition, e.Id);
                    Assert.IsTrue(Memories.IsMemory(e), e.Id + " is a memory");
                    Assert.IsFalse(string.IsNullOrEmpty(e.Tag), e.Id + " is tagged");
                    Assert.IsTrue(e.Steps.Any(s => s.Type == "place"), e.Id + " puts its villager on stage");
                }
            }
        }

        [Test]
        public void TheVariantStorylines_HaveOneSceneForEachVariant()
        {
            foreach (var def in _story.Storylines.Where(d => d.Variants != null && d.Variants.Count > 0))
                foreach (var v in def.Variants)
                    Assert.IsTrue(_story.Events.Any(e => e.Condition != null && e.Condition.Contains($"storyline:{def.Id}.{v}")), $"{def.Id}.{v}");
        }

        [Test]
        public void PickVariant_IsDeterministic_AndUsesEveryVariantAcrossSeeds()
        {
            var def = new StorylineDefinition { Id = "x", Variants = new List<string> { "a", "b", "c" } };
            Assert.AreEqual(Storylines.PickVariant(def, 77), Storylines.PickVariant(def, 77));
            var seen = new HashSet<string>(Enumerable.Range(1, 60).Select(s => Storylines.PickVariant(def, s * 977)));
            CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, seen);
            Assert.IsNull(Storylines.PickVariant(new StorylineDefinition { Id = "y" }, 5));
            Assert.IsNull(Storylines.PickVariant(null, 5));
        }

        [Test]
        public void ShippedVariantStorylines_ListWhoWritesAndWhoWhistles()
        {
            CollectionAssert.AreEquivalent(new[] { "dorian", "juno", "tilda" }, _story.Storylines.First(d => d.Id == "anonymous_notes").Variants);
            CollectionAssert.AreEquivalent(new[] { "bram", "marcus", "felix" }, _story.Storylines.First(d => d.Id == "mystery_whistler").Variants);
            CollectionAssert.AreEquivalent(new[] { "dorian", "marcus", "felix" }, _story.Storylines.First(d => d.Id == "missing_pumpkin").Variants);
        }

        [Test]
        public void StorySceneCells_AreTheVillageCrossing_AndHaveTheirChoiceDialogues()
        {
            foreach (var e in _story.Events.Where(e => e.Id.StartsWith("story_")))
            {
                Assert.AreEqual("Village", e.Map, e.Id);
                var dialogue = e.Steps.First(s => s.Type == "dialogue").Dialogue;
                Assert.IsNotNull(_story.Dialogue(dialogue), e.Id);
                Assert.GreaterOrEqual(_story.Dialogue(dialogue).Nodes.SelectMany(n => n.Choices ?? new List<DialogueChoice>()).Count(), 3, e.Id + ": a real choice");
            }
        }

        [Test]
        public void TheFirstNote_IsALetter_ThatStartsTheNotes()
        {
            var letter = _story.Letters.First(l => l.Id == "notes_first");
            StringAssert.Contains("storyline:anonymous_notes", letter.Condition);
            CollectionAssert.Contains(letter.Effects, "flag:notes.began");
        }

        // Playtest 2026-10-09: "I received a letter complimenting her on her corn. She was not growing any corn." The note comes once any three crops are planted,
        // so it must not name one.
        [Test]
        public void TheFirstNote_NamesNoCrop_BecauseAnyThreeCropsStartIt()
        {
            var table = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var letter = _story.Letters.First(l => l.Id == "notes_first");
            Assert.IsTrue(letter.Condition.Contains("farm:crops>=3"), "the condition counts crops of any kind");
            var body = table[letter.BodyKey].ToLowerInvariant();
            foreach (var crop in Farm.Data.CropDefaults.Rows)
                Assert.IsFalse(body.Contains(crop.Id.Replace("_", " ")), "the note names " + crop.Id);
        }
    }
}
