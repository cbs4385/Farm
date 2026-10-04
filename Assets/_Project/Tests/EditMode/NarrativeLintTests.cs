using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-136: the narrative validator's rules, on small synthetic stories, and the shipped content.
    public class NarrativeLintTests
    {
        static readonly string[] Villagers = { "wren", "hazel", "bram", "tilda", "ione", "marcus", "odalys", "felix", "juno", "piper", "dorian", "elara" };

        // A story with one villager whose talk set holds the given lines.
        static (StoryContent story, Dictionary<string, string> table) Build(string villager, params (string text, int priority, string tag, string rarity)[] lines)
        {
            var dialogues = new List<string>();
            var entries = new List<string>();
            var table = new Dictionary<string, string>();
            for (var i = 0; i < lines.Length; i++)
            {
                var id = $"{villager}.l{i}";
                dialogues.Add($"{{\"id\":\"{id}\",\"start\":\"n0\",\"nodes\":[{{\"id\":\"n0\",\"speaker\":\"{villager}\",\"text\":\"dlg.{id}.0\"}}]}}");
                var tag = lines[i].tag != null ? $",\"tag\":\"{lines[i].tag}\"" : "";
                var rarity = lines[i].rarity != null ? $",\"rarity\":\"{lines[i].rarity}\"" : "";
                entries.Add($"{{\"priority\":{lines[i].priority},\"dialogue\":\"{id}\"{tag}{rarity}}}");
                table[$"dlg.{id}.0"] = lines[i].text;
            }
            var json = $"{{\"dialogues\":[{string.Join(",", dialogues)}],\"sets\":[{{\"id\":\"npc.{villager}.talk\",\"entries\":[{string.Join(",", entries)}]}}]}}";
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(json, "test"));
            return (story, table);
        }

        static List<LintIssue> Lint((StoryContent story, Dictionary<string, string> table) s, NarrativeConfig config = null) =>
            NarrativeLint.Run(s.story, s.table, config, Villagers);

        static (string, int, string, string) Line(string text, int priority = 0, string tag = null, string rarity = null) => (text, priority, tag, rarity);

        static bool Has(List<LintIssue> issues, string rule, LintSeverity? severity = null) =>
            issues.Any(i => i.Rule == rule && (severity == null || i.Severity == severity));

        [TestCase("The neighbour waved.")]
        [TestCase("What a lovely colour.")]
        [TestCase("It is cosy in here.")]
        [TestCase("My favourite book.")]
        [TestCase("Autumn leaves.")]
        [TestCase("I was practising.")]
        [TestCase("Come to the centre of town.")]
        [TestCase("Whilst you wait.")]
        [TestCase("A flavourful stew.")]
        [TestCase("He was travelling.")]
        public void BritishSpellings_AreErrors(string text) =>
            Assert.IsTrue(Has(Lint(Build("wren", Line(text))), "spelling", LintSeverity.Error), text);

        [TestCase("The neighbor waved.")]
        [TestCase("What a lovely color.")]
        [TestCase("It is cozy in here.")]
        [TestCase("My favorite book.")]
        [TestCase("Fall leaves.")]
        [TestCase("I was practicing.")]
        [TestCase("Fresh plaster and a biscuit.")]
        [TestCase("Good timber.")]
        [TestCase("The center of town.")]
        public void AmericanSpellings_Pass(string text) => Assert.IsFalse(Has(Lint(Build("wren", Line(text))), "spelling"), text);

        [Test]
        public void TheSpellingRule_CoversEveryString_NotJustDialogue()
        {
            var s = Build("wren", Line("Fine."));
            s.table["item.x.desc"] = "A grey stone.";
            var issue = Lint(s).Single(i => i.Rule == "spelling");
            Assert.AreEqual("item.x.desc", issue.Where);
        }

        [Test]
        public void TheMythosBand_IsReserved()
        {
            Assert.IsTrue(Has(Lint(Build("wren", Line("Fine.", 9))), "band", LintSeverity.Error), "cozy line in the mythos band");
            Assert.IsFalse(Has(Lint(Build("wren", Line("Fine.", 7))), "band"));
            Assert.IsFalse(Has(Lint(Build("wren", Line("Fine.", 100))), "band"), "first meeting");
            Assert.IsTrue(Has(Lint(Build("wren", Line("Fine.", 50))), "band"), "outside the bands");
        }

        [Test]
        public void TheMythosLayer_MayUseItsOwnBand()
        {
            var story = new StoryContent();
            story.AddJson("{\"dialogues\":[{\"id\":\"mythos.wren.clue1\",\"start\":\"n0\",\"nodes\":[{\"id\":\"n0\",\"speaker\":\"wren\",\"text\":\"k\"}]}],\"sets\":[{\"id\":\"npc.wren.talk\",\"entries\":[{\"priority\":9,\"dialogue\":\"mythos.wren.clue1\"}]}]}", "t");
            var issues = NarrativeLint.Run(story, new Dictionary<string, string> { { "k", "Hello." } }, null, Villagers);
            Assert.IsFalse(Has(issues, "band"));
        }

        [Test]
        public void LongLines_AreWarnings_AndErrorsOnceLocked()
        {
            var long29 = string.Join(" ", Enumerable.Repeat("word", 29)) + ".";
            var s = Build("wren", Line(long29));
            Assert.IsTrue(Has(Lint(s), "length", LintSeverity.Warning));
            var locked = new NarrativeConfig { Locked = { "wren" } };
            Assert.IsTrue(Has(Lint(s, locked), "length", LintSeverity.Error));
            Assert.IsFalse(Has(Lint(Build("wren", Line(string.Join(" ", Enumerable.Repeat("word", 28)) + "."))), "length"), "exactly the budget is fine");
        }

        [Test]
        public void Slang_AndBrands_AreFlagged() =>
            Assert.IsTrue(Has(Lint(Build("wren", Line("lol that is sus"))), "tone"));

        [Test]
        public void VoiceRules_AreChecked()
        {
            var cfg = new NarrativeConfig();
            cfg.Voices["hazel"] = new VoiceRule { AllowExclamation = false, MaxSentenceWords = 5, Banned = { "awesome" } };
            Assert.IsTrue(Has(Lint(Build("hazel", Line("Oh!")), cfg), "voice"), "exclamation");
            Assert.IsTrue(Has(Lint(Build("hazel", Line("This sentence is far too long for her.")), cfg), "voice"), "long sentence");
            Assert.IsTrue(Has(Lint(Build("hazel", Line("That is Awesome.")), cfg), "voice"), "banned word");
            Assert.IsFalse(Has(Lint(Build("hazel", Line("Quiet. Shh.")), cfg), "voice"));
        }

        [Test]
        public void TheTic_NeedsItsShareOfOpeners()
        {
            var cfg = new NarrativeConfig();
            cfg.Voices["bram"] = new VoiceRule { TicOpener = "hm", TicMinShare = 0.5f, AllowedRepeatedOpeners = { "hm" } };
            Assert.IsTrue(Has(Lint(Build("bram", Line("Hm."), Line("Fine."), Line("Iron."), Line("Heat.")), cfg), "voice"));
            Assert.IsFalse(Has(Lint(Build("bram", Line("Hm."), Line("Hm. Fine."), Line("Iron."), Line("Heat.")), cfg), "voice"));
        }

        [Test]
        public void DuplicateLines_AndRepeatedOpeners_AreFlagged()
        {
            Assert.IsTrue(Has(Lint(Build("wren", Line("Same line."), Line("same line."))), "duplicate"));
            var issues = Lint(Build("wren", Line("Well, one."), Line("Well, two."), Line("Well, three."), Line("Well, four.")));
            Assert.IsTrue(Has(issues, "opener"));
            var cfg = new NarrativeConfig();
            cfg.Voices["wren"] = new VoiceRule { AllowedRepeatedOpeners = { "well" } };
            Assert.IsFalse(Has(Lint(Build("wren", Line("Well, one."), Line("Well, two."), Line("Well, three."), Line("Well, four.")), cfg), "opener"));
        }

        [Test]
        public void PlayerTokenOveruse_IsFlagged() =>
            Assert.IsTrue(Has(Lint(Build("wren", Line("Hi [player]."), Line("Bye [player]."), Line("Plain."))), "voice"));

        [Test]
        public void MomentQuotas_AreWarningsForFullVillagers_AndErrorsOnceLocked()
        {
            var bare = Build("wren", Line("Fine."), Line("Also fine."));
            var full = new NarrativeConfig { Full = { "wren" } };
            Assert.IsTrue(Has(Lint(bare, full), "quota", LintSeverity.Warning));
            Assert.IsFalse(Has(Lint(bare), "quota"), "an Enhanced villager has no quota yet");
            var locked = new NarrativeConfig { Full = { "wren" }, Locked = { "wren" } };
            Assert.IsTrue(Has(Lint(bare, locked), "quota", LintSeverity.Error));

            var rich = Build("wren",
                Line("A.", 0, "funny"), Line("B.", 0, "funny"), Line("C.", 0, "funny"),
                Line("D.", 0, "wholesome"), Line("E.", 0, "wholesome"), Line("F.", 0, "surprise"),
                Line("G.", 0, null, "rare"), Line("H.", 0, null, "rare"), Line("I.", 0, null, "rare"), Line("J.", 0, null, "legendary"));
            Assert.IsFalse(Has(Lint(rich, locked), "quota"));
        }

        [Test]
        public void Lines_AreOnlyJudgedForTheirOwnSpeaker()
        {
            var story = new StoryContent();
            story.AddJson("{\"dialogues\":[{\"id\":\"wren.x\",\"start\":\"n0\",\"nodes\":[{\"id\":\"n0\",\"speaker\":\"hazel\",\"text\":\"k\"}]}],\"sets\":[{\"id\":\"npc.wren.talk\",\"entries\":[{\"priority\":0,\"dialogue\":\"wren.x\"}]}]}", "t");
            var cfg = new NarrativeConfig();
            cfg.Voices["wren"] = new VoiceRule { AllowExclamation = false };
            var issues = NarrativeLint.Run(story, new Dictionary<string, string> { { "k", "Hey!" } }, cfg, Villagers);
            Assert.IsFalse(Has(issues, "voice"), "a line spoken by another villager in this dialogue is not Wren's");
        }

        [Test]
        public void TheShippedContent_HasNoNarrativeErrors_AndTheReportBuilds()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var table = Farm.Core.L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var config = NarrativeConfig.Parse(File.ReadAllText("Assets/_Project/Narrative/narrative_rules.json"));
            var issues = NarrativeLint.Run(story, table, config, Villagers);
            var errors = issues.Where(i => i.Severity == LintSeverity.Error).Select(i => i.ToString()).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors.Take(15)));
            var report = NarrativeLint.Report(story, table, config, Villagers, issues);
            StringAssert.Contains("| wren |", report);
            StringAssert.Contains("# Narrative report", report);
        }

        [Test]
        public void TheConfigFile_ParsesAndNamesRealVillagers()
        {
            var config = NarrativeConfig.Parse(File.ReadAllText("Assets/_Project/Narrative/narrative_rules.json"));
            foreach (var v in config.Full.Concat(config.Locked).Concat(config.Voices.Keys)) CollectionAssert.Contains(Villagers, v);
            Assert.AreEqual(6, config.Full.Count);
            Assert.IsFalse(config.VoiceOf("bram").AllowExclamation);
            Assert.AreEqual("hm", config.VoiceOf("bram").TicOpener);
            Assert.IsTrue(config.VoiceOf("piper").AllowExclamation, "defaults apply to unnamed villagers");
        }
    }
}
