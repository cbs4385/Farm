using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-094 and T-095: inline markup, tokens, and the optional presentation fields of dialogue nodes.
    public class RichTextTests
    {
        sealed class World : IWorldQuery
        {
            public readonly HashSet<string> Flags = new HashSet<string>();
            public bool HasFlag(string flag) => Flags.Contains(flag);
            public int GetVar(string name) => 0;
            public GameDateTime Now => GameDateTime.NewGame;
            public string Weather => "sunny";
            public string MapId => "Village";
        }

        [SetUp]
        public void SetUp() => Conditions.ClearCustomForTests();

        static RichLine P(string raw, World w = null, int seed = 1) => RichText.Process(raw, w ?? new World(), seed);

        [Test]
        public void PlainText_PassesThrough()
        {
            var l = P("Hello there, friend.");
            Assert.AreEqual("Hello there, friend.", l.Text);
            Assert.IsEmpty(l.Cues);
            Assert.AreEqual(string.Empty, P(null).Text);
            Assert.AreEqual(string.Empty, P("").Text);
        }

        [Test]
        public void ThePause_IsAnchoredToTheVisibleCharacter()
        {
            var l = P("Well...{pause=0.6} maybe.");
            Assert.AreEqual("Well... maybe.", l.Text);
            var cue = l.Cues.Single();
            Assert.AreEqual(7, cue.Index, "after 'Well...'");
            Assert.AreEqual(0.6f, cue.Pause, 0.0001f);
            Assert.AreEqual(0f, cue.Speed);
        }

        [Test]
        public void TheSpeed_AcceptsWordsAndNumbers()
        {
            var l = P("{speed=slow}Slowly{speed=fast} fast{speed=1.5} x{speed=normal}");
            CollectionAssert.AreEqual(new[] { 0.5f, 2f, 1.5f, 1f }, l.Cues.Select(c => c.Speed));
            Assert.AreEqual("Slowly fast x", l.Text);
        }

        [Test]
        public void TextMeshProTags_AreKept_ButNotCounted()
        {
            var l = P("<b>Bold</b>{pause=1} done");
            Assert.AreEqual("<b>Bold</b> done", l.Text);
            Assert.AreEqual(4, l.Cues.Single().Index, "the tags are not visible characters");
        }

        [Test]
        public void AVariation_ChoosesOneOption_TheSameWithTheSameSeed()
        {
            var seen = new HashSet<string>();
            for (var seed = 0; seed < 200; seed++)
            {
                var a = P("{Hello|Hi|Hey}, friend.", null, seed).Text;
                Assert.AreEqual(a, P("{Hello|Hi|Hey}, friend.", null, seed).Text);
                Assert.IsTrue(a == "Hello, friend." || a == "Hi, friend." || a == "Hey, friend.", a);
                seen.Add(a);
            }
            Assert.AreEqual(3, seen.Count, "every option is used");
        }

        [Test]
        public void AVariation_CountsItsChosenTextInCueIndexes()
        {
            var l = P("{Hello|Hello}{pause=1}!", null, 5);
            Assert.AreEqual(5, l.Cues.Single().Index);
        }

        [Test]
        public void Conditionals_PickTheBranch()
        {
            var w = new World();
            const string raw = "{if flag:rich}Welcome back, big spender.{else}Welcome.{/if} Come in.";
            Assert.AreEqual("Welcome. Come in.", P(raw, w).Text);
            w.Flags.Add("rich");
            Assert.AreEqual("Welcome back, big spender. Come in.", P(raw, w).Text);
        }

        [Test]
        public void Conditionals_CanOmitTheElse_AndNest()
        {
            var w = new World();
            w.Flags.Add("a");
            Assert.AreEqual("x", P("x{if flag:b}y{/if}", w).Text);
            Assert.AreEqual("x y", P("x {if flag:a}y{/if}", w).Text);
            const string nested = "{if flag:a}A{if flag:b}B{else}b{/if}{else}no{/if}";
            Assert.AreEqual("Ab", P(nested, w).Text);
            w.Flags.Add("b");
            Assert.AreEqual("AB", P(nested, w).Text);
            w.Flags.Clear();
            Assert.AreEqual("no", P(nested, w).Text);
        }

        [Test]
        public void CuesInAnInactiveBranch_Vanish()
        {
            var l = P("{if flag:no}{pause=2}Hidden{/if}Shown");
            Assert.AreEqual("Shown", l.Text);
            Assert.IsEmpty(l.Cues);
        }

        [Test]
        public void AnInvalidCondition_IsFalse_NotACrash() =>
            Assert.AreEqual("else", P("{if bogus:atom}then{else}else{/if}").Text);

        [Test]
        public void UnknownBraces_AreLeftAlone() =>
            Assert.AreEqual("a {nothing} b", P("a {nothing} b").Text);

        [Test]
        public void Validate_AcceptsGoodMarkup()
        {
            Assert.IsEmpty(RichText.Validate("Fine. {pause=0.5} {speed=slow}ok{speed=normal} {a|b} {if flag:x}y{else}z{/if}"));
            Assert.IsEmpty(RichText.Validate("Plain text, no markup."));
        }

        [TestCase("{pause=9}", "pause")]
        [TestCase("{pause=abc}", "pause")]
        [TestCase("{speed=turbo}", "speed")]
        [TestCase("{speed=9}", "speed")]
        [TestCase("{a||b}", "empty option")]
        [TestCase("{if flag:x}never closed", "never closed")]
        [TestCase("{/if}", "no {if}")]
        [TestCase("{else}", "outside")]
        [TestCase("{if flag:x}a{else}b{else}c{/if}", "two {else}")]
        [TestCase("{what}", "not a known markup")]
        [TestCase("open { brace", "never closed")]
        [TestCase("stray } brace", "no '{'")]
        [TestCase("{if nonsense:1}a{/if}", "unknown condition")]
        public void Validate_FindsMistakes(string raw, string expected)
        {
            Conditions.ClearCustomForTests();
            var problems = RichText.Validate(raw);
            Assert.IsTrue(problems.Any(p => p.Contains(expected)), $"'{raw}' -> {string.Join(" | ", problems)}");
        }

        [Test]
        public void Plain_ShowsWhatTheReaderSees()
        {
            Assert.AreEqual("Hi there friend.", RichText.Plain("{Hi|Hello} there{pause=1} <b>friend</b>."));
            Assert.AreEqual("Welcome.", RichText.Plain("{if flag:x}Welcome.{else}Go away, you.{/if}"));
        }

        // ---- tokens ----

        [Test]
        public void Tokens_AreReplaced_AndUnknownOnesKept()
        {
            string Lookup(string k) => k == "player" ? "Sam" : k == "npc:wren" ? "Wren" : null;
            Assert.AreEqual("Hi Sam, meet Wren. [mystery] [npc:nobody]", StoryTokens.Apply("Hi [player], meet [npc:wren]. [mystery] [npc:nobody]", Lookup));
            Assert.AreEqual("no tokens", StoryTokens.Apply("no tokens", Lookup));
            Assert.IsNull(StoryTokens.Apply(null, Lookup));
        }

        [TestCase("Wren Calloway", "Wren")]
        [TestCase("Dr. Odalys Penn", "Dr. Penn")]
        [TestCase("Bram Hollis", "Bram")]
        [TestCase("Cher", "Cher")]
        [TestCase("", "")]
        public void ShortNames(string full, string expected) => Assert.AreEqual(expected, StoryTokens.ShortName(full));

        // ---- the runner carries the new fields ----

        const string Json = @"{ ""dialogues"": [ { ""id"": ""t.x"", ""start"": ""n0"", ""nodes"": [
          { ""id"": ""n0"", ""speaker"": ""wren"", ""text"": ""k0"", ""expression"": ""happy"", ""emote"": ""heart"", ""sfx"": ""clink"", ""voice"": ""wren_b"", ""camera"": ""wide"", ""tag"": ""funny"", ""next"": ""n1"" },
          { ""id"": ""n1"", ""speaker"": ""wren"", ""text"": ""k1"", ""choices"": [ { ""text"": ""c0"", ""tone"": ""kind"", ""next"": ""n0"" }, { ""text"": ""c1"" } ] } ] } ] }";

        [Test]
        public void TheRunner_ProcessesMarkup_AndCarriesThePresentationFields()
        {
            var story = new StoryContent();
            Assert.IsTrue(story.AddJson(Json, "t"));
            var table = new Dictionary<string, string> { { "k0", "Well{pause=0.5}, hello." }, { "k1", "{Pick|Choose} one." }, { "c0", "Sure" }, { "c1", "No" } };
            var runner = new DialogueRunner(story.Dialogue("t.x"), new World(), (k, a) => table[k], _ => { });
            var line = runner.Current;
            Assert.AreEqual("Well, hello.", line.Text);
            Assert.AreEqual(1, line.Cues.Count);
            Assert.AreEqual("happy", line.Expression);
            Assert.AreEqual("heart", line.Emote);
            Assert.AreEqual("clink", line.Sfx);
            Assert.AreEqual("wren_b", line.Voice);
            Assert.AreEqual("wide", line.Camera);
            Assert.AreEqual("funny", line.Tag);
            runner.Advance();
            Assert.IsTrue(runner.Current.Text == "Pick one." || runner.Current.Text == "Choose one.");
            Assert.AreEqual("kind", runner.Current.Options[0].Tone);
            Assert.AreEqual(string.Empty, runner.Current.Options[1].Tone);
            Assert.AreEqual(string.Empty, runner.Current.Expression, "unset fields are empty, not null");
        }

        [Test]
        public void ExistingContent_StillRunsUnchanged()
        {
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(System.IO.File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
            var runner = new DialogueRunner(story.Dialogue("wren.first"), new World(), (k, a) => table[k], _ => { });
            Assert.AreEqual(table["dlg.wren.first.0"], runner.Current.Text);
            Assert.IsEmpty(runner.Current.Cues);
        }

        // ---- the validator and FScript ----

        [Test]
        public void FScript_CompilesAndExportsThePresentationDirectives()
        {
            const string src = "dialogue a.b\nn0 wren: Hi {pause=0.3}there.\n  expr happy\n  emote note\n  sfx clink\n  voice wren_b\n  camera wide\n  tag funny\n  next n1\nn1 wren: Pick.\n  ? Sure => -\n  tone playful\n";
            var r = FScript.Compile(src, "t");
            Assert.IsEmpty(r.Errors, string.Join("\n", r.Errors));
            var d = r.Dialogues.Single();
            Assert.AreEqual("happy", d.Node("n0").Expression);
            Assert.AreEqual("note", d.Node("n0").Emote);
            Assert.AreEqual("clink", d.Node("n0").Sfx);
            Assert.AreEqual("wren_b", d.Node("n0").Voice);
            Assert.AreEqual("wide", d.Node("n0").Camera);
            Assert.AreEqual("funny", d.Node("n0").Tag);
            Assert.AreEqual("playful", d.Node("n1").Choices[0].Tone);
            var again = FScript.Compile(FScript.Export(r.Dialogues, new DialogueSet[0], k => r.Texts[k]), "round");
            Assert.IsEmpty(again.Errors);
            var json = FScript.ToStoryJson(r);
            StringAssert.Contains("\"expression\": \"happy\"", json);
            StringAssert.Contains("\"tone\": \"playful\"", json);
        }

        [Test]
        public void FScript_RejectsMisplacedDirectives()
        {
            Assert.IsTrue(FScript.Compile("dialogue a\nexpr happy\n", "t").Errors.Any(e => e.Contains("needs a node")));
            Assert.IsTrue(FScript.Compile("dialogue a\nn0 x: Hi\n  tone kind\n", "t").Errors.Any(e => e.Contains("needs a choice")));
        }

        [Test]
        public void TheVocabulary_IsEnforced()
        {
            Assert.IsTrue(DialogueVocabulary.Has(DialogueVocabulary.Expressions, "happy"));
            Assert.IsTrue(DialogueVocabulary.Has(DialogueVocabulary.Expressions, null), "optional");
            Assert.IsFalse(DialogueVocabulary.Has(DialogueVocabulary.Expressions, "furious"));
            Assert.IsFalse(DialogueVocabulary.Has(DialogueVocabulary.Tones, "rude"));
            Assert.IsFalse(DialogueVocabulary.Has(DialogueVocabulary.Emotes, "skull"));
            Assert.IsFalse(DialogueVocabulary.Has(DialogueVocabulary.Cameras, "above"));
            Assert.AreEqual(6, DialogueVocabulary.Expressions.Length);
        }

        [Test]
        public void TheNarrativeLint_FlagsBrokenMarkup_ButCountsReaderWords()
        {
            var story = new StoryContent();
            story.AddJson("{\"dialogues\":[{\"id\":\"wren.x\",\"start\":\"n0\",\"nodes\":[{\"id\":\"n0\",\"speaker\":\"wren\",\"text\":\"k\"}]}],\"sets\":[{\"id\":\"npc.wren.talk\",\"entries\":[{\"priority\":0,\"dialogue\":\"wren.x\"}]}]}", "t");
            var broken = NarrativeLint.Run(story, new Dictionary<string, string> { { "k", "Oops {pause=99} text" } }, null, new[] { "wren" });
            Assert.IsTrue(broken.Any(i => i.Rule == "markup" && i.Severity == LintSeverity.Error));
            var words = string.Join(" ", Enumerable.Repeat("word", 28));
            var padded = NarrativeLint.Run(story, new Dictionary<string, string> { { "k", "{pause=1}" + words + "{pause=1}" } }, null, new[] { "wren" });
            Assert.IsFalse(padded.Any(i => i.Rule == "length"), "markup is not words");
        }
    }
}
