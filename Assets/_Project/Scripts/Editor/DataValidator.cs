using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // T-040: `Farm/Validate Data` runs the story/NPC/recipe validator over the shipped data and logs every problem.
    public static class DataValidator
    {
        const string TablePath = "Assets/_Project/Resources/Localization/en.json";

        public static System.Collections.Generic.List<string> Run()
        {
            Conditions.ClearCustomForTests();
            StoryConditions.Register();
            BusinessHoursRegistry.RegisterConditionAtom();
            var table = L.Parse(File.ReadAllText(TablePath));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            return StoryValidator.Run(new ValidationInput
            {
                Story = StoryContent.LoadFromResources(), Db = db, Npcs = NpcCatalog.From(db), HasKey = table.ContainsKey,
                RecipeExists = id => RecipeCatalog.From(db).Get(id) != null,
            });
        }

        [MenuItem("Farm/Validate Data")]
        public static void Validate()
        {
            var problems = Run();
            foreach (var p in problems) Debug.LogError("[Validate] " + p);
            Debug.Log($"[Validate] {problems.Count} problem(s).");
        }

        // Headless: exit code 1 when anything is wrong (fails a build pipeline).
        public static void ValidateAndExit()
        {
            var problems = Run();
            foreach (var p in problems) Debug.LogError("[Validate] " + p);
            EditorApplication.Exit(problems.Any() ? 1 : 0);
        }
    }
}
