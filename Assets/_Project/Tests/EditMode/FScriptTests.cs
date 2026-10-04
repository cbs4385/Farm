using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-135: the FScript compiler and exporter.
    public class FScriptTests
    {
        const string Sample = @"
# a comment
dialogue tilda.first
n0 tilda: Well, hello there! You must be the new farmer.
  + flag:met.tilda
  next n1
n1 tilda: Seeds, gossip and a backpack.
  ? Nice to meet you, [player]. => r0 | friend:tilda,10 | flag:polite
  ? What do you sell? => r1 if has:crop.parsnip>=1
r0 tilda: The pleasure's mine.
r1 -: She waves at the shelves.\nSeeds, mostly.

set npc.tilda.talk
  100 !flag:met.tilda => tilda.first
  0 season:spring && hearts:tilda>=3 => tilda.sea rarity=rare weight=2.5 cooldown=10 tag=funny cat=seasonal
  1 ~ => tilda.plain
";

        static FScriptResult Ok(string source)
        {
            var r = FScript.Compile(source, "test");
            Assert.IsEmpty(r.Errors, string.Join("\n", r.Errors));
            return r;
        }

        [Test]
        public void ADialogue_CompilesToNodesChoicesEffectsAndGeneratedKeys()
        {
            var r = Ok(Sample);
            var d = r.Dialogues.Single();
            Assert.AreEqual("tilda.first", d.Id);
            Assert.AreEqual("n0", d.Start);
            Assert.AreEqual(4, d.Nodes.Count);
            var n0 = d.Node("n0");
            Assert.AreEqual("tilda", n0.Speaker);
            Assert.AreEqual("dlg.tilda.first.0", n0.Text, "numbered nodes use the number");
            CollectionAssert.AreEqual(new[] { "flag:met.tilda" }, n0.Effects);
            Assert.AreEqual("n1", n0.Next);
            var choices = d.Node("n1").Choices;
            Assert.AreEqual("dlg.tilda.first.c0", choices[0].Text);
            Assert.AreEqual("dlg.tilda.first.c1", choices[1].Text);
            CollectionAssert.AreEqual(new[] { "friend:tilda,10", "flag:polite" }, choices[0].Effects);
            Assert.AreEqual("r0", choices[0].Next);
            Assert.AreEqual("has:crop.parsnip>=1", choices[1].Condition);
            Assert.AreEqual("dlg.tilda.first.r0", d.Node("r0").Text, "other node ids are used as they are");
            Assert.IsNull(d.Node("r1").Speaker, "a dash is narration");
        }

        [Test]
        public void Texts_AreCollected_WithEscapes()
        {
            var r = Ok(Sample);
            Assert.AreEqual("Well, hello there! You must be the new farmer.", r.Texts["dlg.tilda.first.0"]);
            Assert.AreEqual("Nice to meet you, [player].", r.Texts["dlg.tilda.first.c0"]);
            Assert.AreEqual("She waves at the shelves.\nSeeds, mostly.", r.Texts["dlg.tilda.first.r1"]);
        }

        [Test]
        public void ASet_CompilesEntriesWithTheirOptions()
        {
            var set = Ok(Sample).Sets.Single();
            Assert.AreEqual(3, set.Entries.Count);
            Assert.AreEqual(100, set.Entries[0].Priority);
            Assert.AreEqual("!flag:met.tilda", set.Entries[0].Condition);
            var e = set.Entries[1];
            Assert.AreEqual("season:spring && hearts:tilda>=3", e.Condition);
            Assert.AreEqual("rare", e.Rarity);
            Assert.AreEqual(2.5f, e.Weight);
            Assert.AreEqual(10, e.Cooldown);
            Assert.AreEqual("funny", e.Tag);
            Assert.AreEqual("seasonal", e.Category);
            Assert.IsNull(set.Entries[2].Condition, "~ means no condition");
            Assert.AreEqual(-1, set.Entries[2].Cooldown);
            Assert.AreEqual(1f, set.Entries[2].Weight);
        }

        [Test]
        public void AKeyLine_OverridesTheGeneratedKey_ForNodesAndChoices()
        {
            var r = Ok("dialogue a.b\nn0 x: Hello\n  key custom.node\nn1 x: Pick\n  ? Yes => n0\n  key custom.choice\n");
            var d = r.Dialogues.Single();
            Assert.AreEqual("custom.node", d.Node("n0").Text);
            Assert.AreEqual("custom.choice", d.Node("n1").Choices[0].Text);
            Assert.AreEqual("Hello", r.Texts["custom.node"]);
            Assert.AreEqual("Yes", r.Texts["custom.choice"]);
        }

        [Test]
        public void ANodeWithoutText_IsAllowed()
        {
            var d = Ok("dialogue a\nn0 x: Hi\n  next n1\nn1 -:\n  + flag:done\n").Dialogues.Single();
            Assert.IsNull(d.Node("n1").Text);
            CollectionAssert.AreEqual(new[] { "flag:done" }, d.Node("n1").Effects);
        }

        [TestCase("n0 x: Hi\n", "expected")]
        [TestCase("dialogue a\nbogus line\n", "cannot understand")]
        [TestCase("dialogue a\nn0 x: Hi\nn0 x: Again\n", "defined twice")]
        [TestCase("dialogue a\nn0 x: Hi\n  next nowhere\n", "unknown node")]
        [TestCase("dialogue a\nn0 x: Hi\n  ? A => nowhere\n", "unknown node")]
        [TestCase("dialogue a\nn0 x: Hi\n  ? A => n0\n  next n0\n", "both choices and next")]
        [TestCase("dialogue a\nn0 x: Hi\n  ? no arrow\n", "choice looks like")]
        [TestCase("dialogue a\n  + flag:x\n", "needs a node")]
        [TestCase("dialogue a\nn0 x: Hi\ndialogue a\nn0 x: Hi\n", "defined twice")]
        [TestCase("dialogue a\n", "no nodes")]
        [TestCase("set s\n  x => d\n", "not a priority")]
        [TestCase("set s\n  0 nothing\n", "set entry looks like")]
        [TestCase("set s\n  0 ~ => d speed=3\n", "unknown option")]
        [TestCase("set s\n  0 ~ => d cooldown=soon\n", "whole number")]
        public void Mistakes_AreReportedWithTheirLine(string source, string expected)
        {
            var r = FScript.Compile(source, "bad");
            Assert.IsTrue(r.Errors.Any(e => e.Contains(expected)), $"expected '{expected}' in: {string.Join(" / ", r.Errors)}");
            Assert.IsTrue(r.Errors.All(e => e.StartsWith("bad")));
        }

        [Test]
        public void TheErrorMessage_NamesTheLineNumber()
        {
            var r = FScript.Compile("dialogue a\nn0 x: Hi\nwhat is this\n", "f.fscript");
            StringAssert.Contains("f.fscript:3:", r.Errors.Single());
        }

        [Test]
        public void TwoDifferentTextsForOneKey_AreAnError()
        {
            var r = FScript.Compile("dialogue a\nn0 x: One\n  key shared\nn1 x: Two\n  key shared\n", "t");
            Assert.IsTrue(r.Errors.Any(e => e.Contains("two different texts")));
        }

        [Test]
        public void TheJson_LoadsThroughStoryContent_AndKeepsTheVarietyFields()
        {
            var json = FScript.ToStoryJson(Ok(Sample));
            var content = new StoryContent();
            Assert.IsTrue(content.AddJson(json, "compiled"));
            Assert.IsEmpty(content.Errors);
            Assert.IsNotNull(content.Dialogue("tilda.first"));
            var e = content.Set("npc.tilda.talk").Entries[1];
            Assert.AreEqual("rare", e.Rarity);
            Assert.AreEqual(2.5f, e.Weight);
            Assert.AreEqual(10, e.Cooldown);
            Assert.AreEqual("funny", e.Tag);
        }

        [Test]
        public void TheJson_OmitsDefaults_LikeTheHandWrittenFiles()
        {
            var json = FScript.ToStoryJson(Ok("dialogue a\nn0 x: Hi\nset s\n  0 ~ => a\n"));
            StringAssert.DoesNotContain("\"weight\"", json);
            StringAssert.DoesNotContain("\"cooldown\"", json);
            StringAssert.DoesNotContain("\"args\"", json);
            StringAssert.DoesNotContain("\"effects\"", json);
            StringAssert.DoesNotContain("\"condition\"", json);
            StringAssert.Contains("\"priority\": 0", json);
        }

        [Test]
        public void MergeTexts_AddsAndUpdates_AndIsIdempotent()
        {
            var table = "{\n  \"a\": \"one\",\n  \"b\": \"two\"\n}\n";
            var texts = new Dictionary<string, string> { { "b", "TWO" }, { "c", "three" } };
            var merged = FScript.MergeTexts(table, texts, out var added, out var changed);
            Assert.AreEqual(1, added);
            Assert.AreEqual(1, changed);
            var parsed = L.Parse(merged);
            Assert.AreEqual("one", parsed["a"]);
            Assert.AreEqual("TWO", parsed["b"]);
            Assert.AreEqual("three", parsed["c"]);
            Assert.AreEqual(new[] { "a", "b", "c" }, JsonConvert.DeserializeObject<Dictionary<string, string>>(merged).Keys.ToArray(), "order is kept");
            FScript.MergeTexts(merged, texts, out added, out changed);
            Assert.AreEqual(0, added + changed);
        }

        [Test]
        public void EveryShippedDialogueAndSet_RoundTripsThroughFScript()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var dialogues = story.Dialogues.OrderBy(d => d.Id, System.StringComparer.Ordinal).ToList();
            var sets = story.Sets.OrderBy(s => s.Id, System.StringComparer.Ordinal).ToList();
            Assert.Greater(dialogues.Count, 250);

            var script = FScript.Export(dialogues, sets, key => table.TryGetValue(key, out var t) ? t : null);
            var back = FScript.Compile(script, "roundtrip");
            Assert.IsEmpty(back.Errors, string.Join("\n", back.Errors.Take(10)));
            Assert.AreEqual(dialogues.Count, back.Dialogues.Count);
            Assert.AreEqual(sets.Count, back.Sets.Count);

            foreach (var d in dialogues)
            {
                var other = back.Dialogues.Single(x => x.Id == d.Id);
                Assert.AreEqual(Normalised(d), Normalised(other), "dialogue " + d.Id);
                foreach (var key in d.Nodes.Select(n => n.Text).Concat(d.Nodes.SelectMany(n => n.Choices).Select(c => c.Text)).Where(k => !string.IsNullOrEmpty(k)))
                    Assert.AreEqual(table[key], back.Texts[key], key);
            }
            foreach (var s in sets)
                Assert.AreEqual(Normalised(s), Normalised(back.Sets.Single(x => x.Id == s.Id)), "set " + s.Id);
        }

        // The compiler's own JSON omits empty and default values, so a null and an empty speaker compare equal.
        static string Normalised(DialogueGraph d) { var r = new FScriptResult(); r.Dialogues.Add(d); return FScript.ToStoryJson(r); }
        static string Normalised(DialogueSet s) { var r = new FScriptResult(); r.Sets.Add(s); return FScript.ToStoryJson(r); }

        [Test]
        public void TheExporter_RefusesTextItCannotRepresent()
        {
            var d = new DialogueGraph { Id = "x", Start = "n0" };
            d.Nodes.Add(new DialogueNode { Id = "n0", Speaker = "a", Text = "t", Choices = { new DialogueChoice { Text = "c", Next = null } } });
            Assert.Throws<System.FormatException>(() => FScript.Export(new[] { d }, new DialogueSet[0], k => k == "c" ? "left => right" : "hi"));
        }
    }
}
