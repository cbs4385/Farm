using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-136: `Farm/Narrative Report` runs the narrative validator over the shipped story and writes Builds/narrative_report.md.
    public static class NarrativeTools
    {
        public const string RulesPath = "Assets/_Project/Narrative/narrative_rules.json";
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";
        public static readonly string[] Villagers = { "tilda", "bram", "ione", "marcus", "odalys", "wren", "felix", "juno", "hazel", "piper", "dorian", "elara" };

        public static List<LintIssue> Run(out string report)
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            BusinessHoursRegistry.RegisterConditionAtom();
            var story = StoryContent.LoadFromResources();
            var table = L.Parse(File.ReadAllText(TablePath));
            var config = File.Exists(RulesPath) ? NarrativeConfig.Parse(File.ReadAllText(RulesPath)) : new NarrativeConfig();
            var issues = NarrativeLint.Run(story, table, config, Villagers);
            report = NarrativeLint.Report(story, table, config, Villagers, issues);
            return issues;
        }

        [MenuItem("Farm/Narrative Report")]
        public static void ReportMenu()
        {
            var issues = Run(out var report);
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/narrative_report.md", report);
            Debug.Log($"[Narrative] {issues.Count(i => i.Severity == LintSeverity.Error)} error(s), {issues.Count(i => i.Severity == LintSeverity.Warning)} warning(s); report in Builds/narrative_report.md");
        }

        // Headless: exit code 1 when there are errors.
        public static void ReportAndExit()
        {
            var issues = Run(out var report);
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/narrative_report.md", report);
            EditorApplication.Exit(issues.Any(i => i.Severity == LintSeverity.Error) ? 1 : 0);
        }
    }
}
