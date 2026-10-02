using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // T-022: every user-facing string goes through L.Get and exists in en.json.
    public class LocalizationTests
    {
        const string ScriptsDir = "Assets/_Project/Scripts";
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";

        static Dictionary<string, string> LoadTable() => L.Parse(File.ReadAllText(TablePath));

        static IEnumerable<string> SourceFiles(string sub = "") =>
            Directory.GetFiles(Path.Combine(ScriptsDir, sub), "*.cs", SearchOption.AllDirectories);

        [Test]
        public void EveryLiteralKeyUsedInCode_ExistsInEnglishTable()
        {
            var table = LoadTable();
            // Complete literal keys only; dynamic keys ("weather." + id) are covered by DynamicKeys_AreAllDefined.
            var keyPattern = new Regex("(?:L\\.Get|ShowConfirm)\\(\\s*\"([^\"]+)\"\\s*[,)]");
            var missing = new List<string>();
            foreach (var file in SourceFiles())
                foreach (Match m in keyPattern.Matches(File.ReadAllText(file)))
                    if (!table.ContainsKey(m.Groups[1].Value)) missing.Add($"{Path.GetFileName(file)}: {m.Groups[1].Value}");
            CollectionAssert.IsEmpty(missing, "keys missing from en.json");
        }

        [Test]
        public void DynamicKeys_AreAllDefined()
        {
            var table = LoadTable();
            var expected = new List<string>();
            expected.AddRange(new[] { "spring", "summer", "fall", "winter" }.Select(s => "season." + s));
            expected.AddRange(new[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" }.Select(d => "day." + d));
            expected.AddRange(WeatherCatalog.BuiltIn.Ids.Select(w => "weather." + w));
            expected.AddRange(InputNames.Rebindable.Select(a => "action." + a));
            expected.AddRange(new[] { "general", "blacksmith", "carpenter", "fish", "clinic", "library", "saloon", "merchant" }.Select(b => "business." + b));
            expected.AddRange(new[] { "skills", "social", "calendar", "map", "journal", "crafting" }.Select(t => "menu.tab." + t));
            expected.AddRange(MapIds.All.Concat(new[] { MapIds.Woods }).Select(m => "map." + m));
            expected.AddRange(SkillIds.All.Select(sk => "skill." + sk));
            expected.AddRange(NpcDefaults.CreateAll().Select(n => n.NameKey));
            expected.Add("shop.general.title");
            expected.Add("language.en");
            CollectionAssert.IsEmpty(expected.Where(k => !table.ContainsKey(k)).ToList());
        }

        [Test]
        public void EveryItem_HasNameAndDescription()
        {
            var table = LoadTable();
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            Assert.IsNotNull(db);
            var missing = new List<string>();
            foreach (var item in db.Items)
            {
                if (!table.ContainsKey(item.NameKey)) missing.Add(item.NameKey);
                if (!table.ContainsKey(item.DescriptionKey)) missing.Add(item.DescriptionKey);
            }
            CollectionAssert.IsEmpty(missing);
        }

        [Test]
        public void UiCode_HasNoHardcodedUserText()
        {
            // Labels and buttons must take their text from L.Get(...) or a variable, never a literal.
            var literalText = new Regex("(?:Label|MakeButton|MakeToggle)\\([^,()]+,\\s*\"([^\"]+)\"");
            var offenders = new List<string>();
            foreach (var file in SourceFiles("UI"))
                foreach (Match m in literalText.Matches(File.ReadAllText(file)))
                    offenders.Add($"{Path.GetFileName(file)}: \"{m.Groups[1].Value}\"");
            CollectionAssert.IsEmpty(offenders, "hardcoded UI text (use L.Get)");
        }

        [Test]
        public void L_Get_FormatsArguments_AndFallsBackToKey()
        {
            L.SetTable(new Dictionary<string, string> { { "a", "{0} of {1}" }, { "bad", "{9}" } });
            Assert.AreEqual("3 of 4", L.Get("a", 3, 4));
            Assert.AreEqual("{9}", L.Get("bad", 1));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new Regex("Missing localization key"));
            Assert.AreEqual("nope", L.Get("nope"));
            L.SetLanguage("en");
        }
    }
}
