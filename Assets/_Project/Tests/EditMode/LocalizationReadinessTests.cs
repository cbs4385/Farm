using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // T-139: localisation readiness. Pseudo-localisation keeps every token and markup group intact; the CSV export has one row per key
    // with context, budgets and tokens, and round-trips through the CSV reader.
    public class LocalizationReadinessTests
    {
        static readonly Regex Tokens = new Regex(@"\[[a-z:_]+\]|\{\d+\}|\{[^{}]*\}|<[^<>]+>");

        Dictionary<string, string> _table;
        StoryContent _story;

        [OneTimeSetUp]
        public void Load()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            _story = StoryContent.LoadFromResources();
            _table = L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json"));
        }

        static List<string> TokensOf(string text) => Tokens.Matches(text).Cast<Match>().Select(m => m.Value).ToList();

        [Test]
        public void PseudoLoc_AccentsPadsAndBrackets_ButKeepsTokensAndMarkupAsTheyWere()
        {
            var raw = "Hello, [player]! {pause=0.4}Welcome to {0}. <b>Sit</b> {if flag:x}down{else}up{/if}.";
            var pseudo = PseudoLoc.Apply(raw);
            Assert.IsTrue(PseudoLoc.IsPseudo(pseudo));
            CollectionAssert.AreEqual(TokensOf(raw), TokensOf(pseudo));
            StringAssert.Contains("Hélló", pseudo);
            Assert.AreEqual(pseudo, PseudoLoc.Apply(raw), "deterministic");
            Assert.AreEqual(string.Empty, PseudoLoc.Apply(string.Empty));
        }

        [Test]
        public void PseudoLoc_MakesLongTextAboutAThirdLonger()
        {
            var raw = "This is a fairly ordinary sentence of the kind a villager might say to the player on a quiet afternoon.";
            Assert.GreaterOrEqual(PseudoLoc.Apply(raw).Length, (int)(raw.Length * 1.3f));
        }

        [Test]
        public void EveryString_KeepsItsTokensAndPassesTheMarkupCheck_WhenPseudoLocalised()
        {
            foreach (var kv in _table)
            {
                var pseudo = PseudoLoc.Apply(kv.Value);
                CollectionAssert.AreEqual(TokensOf(kv.Value), TokensOf(pseudo), kv.Key);
                if (!RichText.Validate(kv.Value).Any()) CollectionAssert.IsEmpty(RichText.Validate(pseudo), kv.Key + ": " + pseudo);
            }
        }

        [Test]
        public void TheLint_FlagsGenderedAddressOfThePlayer_ButNotOrdinaryWords()
        {
            Assert.IsTrue(NarrativeLint.IsGenderedAddress("Welcome back, sir."));
            Assert.IsTrue(NarrativeLint.IsGenderedAddress("Aren't you a handsome one."));
            Assert.IsTrue(NarrativeLint.IsGenderedAddress("Evening, Ma'am!"));
            Assert.IsFalse(NarrativeLint.IsGenderedAddress("Welcome back, friend. Missed you."));
            Assert.IsFalse(NarrativeLint.IsGenderedAddress("The missus of the house said something about misery."));
        }

        [Test]
        public void TheShippedLines_NeverAddressThePlayerByGender()
        {
            foreach (var kv in _table.Where(k => k.Key.StartsWith("dlg.", System.StringComparison.Ordinal) || k.Key.StartsWith("event.", System.StringComparison.Ordinal)))
                Assert.IsFalse(NarrativeLint.IsGenderedAddress(kv.Value), kv.Key + ": " + kv.Value);
        }

        [Test]
        public void TheExport_HasOneRowPerKey_WithContext()
        {
            var rows = LocalizationExport.Build(_table, _story);
            Assert.AreEqual(_table.Count, rows.Count);
            Assert.IsTrue(rows.All(r => !string.IsNullOrEmpty(r.Context)), "every key has context");
            Assert.AreEqual(rows.Count, rows.Select(r => r.Key).Distinct().Count());
        }

        [Test]
        public void TheExport_KnowsWhoSpeaks_AndTheBudgets()
        {
            var rows = LocalizationExport.Build(_table, _story).ToDictionary(r => r.Key);
            var wren = rows["dlg.wren.spring.1.0"];
            Assert.AreEqual("wren", wren.Speaker);
            StringAssert.Contains("spoken by wren", wren.Context);
            Assert.AreEqual(NarrativeLint.TalkWords, wren.MaxWords);
            var bark = rows["dlg.bram.bark.1.0"];
            Assert.AreEqual(NarrativeLint.BarkWords, bark.MaxWords);
            StringAssert.Contains("speech bubble", bark.Context);
            Assert.AreEqual(22, rows["memory.wren_heart4"].MaxChars);
            var choice = rows.Values.First(r => r.Context.StartsWith("A choice the player can pick"));
            Assert.AreEqual(NarrativeLint.ChoiceWords, choice.MaxWords);
            Assert.IsTrue(rows.Values.Any(r => r.Tokens.Contains("[player]")));
        }

        [Test]
        public void TheCsv_RoundTrips_CommasQuotesAndLineBreaks()
        {
            var rows = new List<LocalizationRow>
            {
                new LocalizationRow { Key = "a", Text = "plain", Context = "x" },
                new LocalizationRow { Key = "b", Text = "with, comma and \"quotes\"", Context = "y, z" },
                new LocalizationRow { Key = "c", Text = "line one\nline two", Context = "" },
            };
            var back = LocalizationExport.ReadKeyText(LocalizationExport.ToCsv(rows));
            Assert.AreEqual("plain", back["a"]);
            Assert.AreEqual("with, comma and \"quotes\"", back["b"]);
            Assert.AreEqual("line one\nline two", back["c"]);
        }

        [Test]
        public void TheWholeTable_RoundTripsThroughTheCsv()
        {
            var back = LocalizationExport.ReadKeyText(LocalizationExport.ToCsv(LocalizationExport.Build(_table, _story)));
            Assert.AreEqual(_table.Count, back.Count);
            foreach (var kv in _table) Assert.AreEqual(kv.Value.Replace("\r\n", "\n"), back[kv.Key].Replace("\r\n", "\n"), kv.Key);
        }
    }
}
