using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-135: `Farm/Compile Story` turns every .fscript under Assets/_Project/Narrative into story JSON
    // (Resources/Story/fs_<name>.json) and merges its texts into the English string table.
    public static class FScriptCompiler
    {
        public const string SourceFolder = "Assets/_Project/Narrative";
        const string OutFolder = "Assets/_Project/Resources/Story";
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";
        public const string OutPrefix = "zz_fs_";      // sorts after the hand-written story files (see StoryContent.LoadFromResources)

        [MenuItem("Farm/Compile Story")]
        public static void CompileMenu() => Debug.Log(Compile() ? "[FScript] compiled." : "[FScript] failed, nothing written.");

        // Headless: exit code 1 on any error.
        public static void CompileAndExit() => EditorApplication.Exit(Compile() ? 0 : 1);

        public static bool Compile()
        {
            if (!Directory.Exists(SourceFolder)) { Debug.Log($"[FScript] no {SourceFolder} folder; nothing to compile."); return true; }
            var files = Directory.GetFiles(SourceFolder, "*.fscript", SearchOption.AllDirectories).OrderBy(f => f, System.StringComparer.Ordinal).ToList();
            var results = new List<(string name, FScriptResult result)>();
            var ok = true;
            var seen = new HashSet<string>();
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var result = FScript.Compile(File.ReadAllText(file), Path.GetFileName(file));
                foreach (var d in result.Dialogues.Where(d => !seen.Add("d:" + d.Id))) result.Errors.Add($"{name}: dialogue '{d.Id}' is also defined in another file");
                foreach (var e in result.Errors) Debug.LogError("[FScript] " + e);
                ok &= result.Ok;
                results.Add((name, result));
            }
            if (!ok) return false;

            var table = File.ReadAllText(TablePath);
            foreach (var (name, result) in results)
            {
                File.WriteAllText(Path.Combine(OutFolder, OutPrefix + name + ".json"), FScript.ToStoryJson(result) + "\n");
                table = FScript.MergeTexts(table, result.Texts, out var added, out var changed);
                Debug.Log($"[FScript] {name}: {result.Dialogues.Count} dialogue(s), {result.Sets.Count} set(s), {added} new text(s), {changed} changed.");
            }
            File.WriteAllText(TablePath, table);
            AssetDatabase.Refresh();
            return true;
        }

        // One-off helper: writes the shipped dialogues as FScript into Builds/narrative_export for inspection (never into Assets).
        [MenuItem("Farm/Export Story As FScript")]
        public static void ExportExisting()
        {
            var story = StoryContent.LoadFromResources();
            var table = Farm.Core.L.Parse(File.ReadAllText(TablePath));
            Directory.CreateDirectory("Builds/narrative_export");
            var text = FScript.Export(story.Dialogues.OrderBy(d => d.Id, System.StringComparer.Ordinal), story.Sets.OrderBy(s => s.Id, System.StringComparer.Ordinal),
                key => table.TryGetValue(key, out var t) ? t : null);
            File.WriteAllText("Builds/narrative_export/shipped.fscript", text);
            Debug.Log("[FScript] exported to Builds/narrative_export/shipped.fscript");
        }
    }
}
