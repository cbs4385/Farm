using System.IO;
using Farm.Core;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-139: `Farm/Localization/Export CSV` writes the whole string table with translator context to Builds/localization/en.csv.
    public static class LocalizationTools
    {
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";
        const string OutPath = "Builds/localization/en.csv";

        public static int Export()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText(TablePath));
            var rows = LocalizationExport.Build(table, story);
            Directory.CreateDirectory(Path.GetDirectoryName(OutPath));
            File.WriteAllText(OutPath, LocalizationExport.ToCsv(rows), new System.Text.UTF8Encoding(true));
            return rows.Count;
        }

        [MenuItem("Farm/Localization/Export CSV")]
        public static void ExportMenu() => Debug.Log($"[Localization] {Export()} string(s) written to {OutPath}");

        public static void ExportAndExit()
        {
            Export();
            EditorApplication.Exit(0);
        }
    }
}
