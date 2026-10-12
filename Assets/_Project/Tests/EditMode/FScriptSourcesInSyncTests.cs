using System.IO;
using System.Linq;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Compiling the .fscript sources (Farm > Compile Story) must change nothing: the committed story JSON and the English string table are exactly what the sources give.
    // A hand patch to a generated zz_fs_*.json (a condition, a text) that is not in its source is lost the next time anyone compiles; this fails until the source has it too.
    public class FScriptSourcesInSyncTests
    {
        static string Root => Path.Combine(Application.dataPath, "_Project");

        [Test]
        public void EveryFScriptSource_CompilesToTheCommittedJson_AndTheCommittedTexts()
        {
            var table = File.ReadAllText(Path.Combine(Root, "Resources", "Localization", "en.json"));
            var drift = new System.Collections.Generic.List<string>();
            var files = Directory.GetFiles(Path.Combine(Root, "Narrative"), "*.fscript", SearchOption.AllDirectories).OrderBy(f => f, System.StringComparer.Ordinal).ToList();
            Assert.IsNotEmpty(files);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var result = FScript.Compile(File.ReadAllText(file), Path.GetFileName(file));
                Assert.IsTrue(result.Ok, name + ": " + string.Join("; ", result.Errors));

                var committedPath = Path.Combine(Root, "Resources", "Story", "zz_fs_" + name + ".json");
                if (!File.Exists(committedPath)) { drift.Add(name + ": no zz_fs_" + name + ".json (compile the story)"); continue; }
                var compiled = JToken.Parse(FScript.ToStoryJson(result));
                var committed = JToken.Parse(File.ReadAllText(committedPath));
                if (!JToken.DeepEquals(compiled, committed)) drift.Add(name + ": zz_fs_" + name + ".json differs from the source (a hand edit missing from the .fscript, or an uncompiled change)");

                FScript.MergeTexts(table, result.Texts, out var added, out var changed);
                if (added > 0 || changed > 0) drift.Add($"{name}: en.json differs from the source ({added} missing, {changed} different texts)");
            }
            Assert.IsEmpty(drift, "story sources and generated files disagree:\n" + string.Join("\n", drift));
        }
    }
}
