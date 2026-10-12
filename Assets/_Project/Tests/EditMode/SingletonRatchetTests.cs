using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The parts of a loaded map (villagers, animals, placed objects, enemies, the mine, the event director) are fields of MapContext, set by MapSceneController
    // (docs/adr/0006-retire-the-service-locator.md). A new `static ... Current { get; private set; }` is a new singleton: take the thing in Init(...) instead.
    public class SingletonRatchetTests
    {
        // What may still be static: the run's context itself, the current control hints (the language table is static), and a diagnostic label that is not a service.
        static readonly string[] Allowed = { "Core/Localization/ControlPrompts.cs", "Gameplay/GameContext.cs", "Gameplay/World/HoverInspector.cs" };

        [Test]
        public void NoNewCurrentInstanceSingletons()
        {
            var scripts = Path.Combine(Application.dataPath, "_Project", "Scripts");
            var found = new List<string>();
            foreach (var file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            {
                var rel = file.Substring(scripts.Length + 1).Replace(System.IO.Path.DirectorySeparatorChar, '/');
                if (rel.StartsWith("Editor/")) continue;
                if (Regex.IsMatch(File.ReadAllText(file), @"public static \w+ (Current|Instance) \{ get; private set; \}") && !Allowed.Contains(rel)) found.Add(rel);
            }
            Assert.IsEmpty(found, "these files hold a static current instance: put it in MapContext or GameContext instead (ADR 0006)");
        }
    }
}
