using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The service locator is being retired (docs/adr/0006-retire-the-service-locator.md). Until it is gone this test only lets the number of uses fall: a file may not use
    // `ServiceLocator.` more than tools/servicelocator-baseline.txt says, and a file the baseline does not list may not start. New code takes what it needs in `Init(...)`
    // (a GameContext or a MapContext). When a count falls, lower the baseline (python tools/servicelocator_baseline.py) so that the gain cannot be lost.
    public class ServiceLocatorRatchetTests
    {
        static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        static Dictionary<string, int> Baseline()
        {
            var result = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines(Path.Combine(ProjectRoot, "tools", "servicelocator-baseline.txt")))
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;
                var cut = line.LastIndexOf(' ');
                result[line.Substring(0, cut)] = int.Parse(line.Substring(cut + 1));
            }
            return result;
        }

        static Dictionary<string, int> Current()
        {
            var scripts = Path.Combine(Application.dataPath, "_Project", "Scripts");
            var result = new Dictionary<string, int>();
            foreach (var file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            {
                var rel = file.Substring(scripts.Length + 1).Replace('\\', '/');
                if (rel.StartsWith("Editor/") || rel == "Core/ServiceLocator.cs") continue;
                var n = Regex.Matches(File.ReadAllText(file), @"ServiceLocator\.").Count;
                if (n > 0) result[rel] = n;
            }
            return result;
        }

        [Test]
        public void NoFileUsesTheServiceLocatorMoreThanItDid_AndNoNewFileStarts()
        {
            var baseline = Baseline();
            var current = Current();
            var more = current.Where(kv => !baseline.TryGetValue(kv.Key, out var allowed) || kv.Value > allowed)
                .Select(kv => $"{kv.Key}: {kv.Value} (allowed {(baseline.TryGetValue(kv.Key, out var a) ? a : 0)})").ToList();
            Assert.IsEmpty(more, "these files use ServiceLocator more than the baseline allows: take the service in Init(...) instead (ADR 0006):\n" + string.Join("\n", more));
        }

        [Test]
        public void TheBaselineIsTight_SoAGainIsKept()
        {
            var baseline = Baseline();
            var current = Current();
            var slack = baseline.Where(kv => !current.TryGetValue(kv.Key, out var now) || now < kv.Value)
                .Select(kv => $"{kv.Key}: baseline {kv.Value}, now {(current.TryGetValue(kv.Key, out var n) ? n : 0)}").ToList();
            Assert.IsEmpty(slack, "the service locator is used less than the baseline says: run python tools/servicelocator_baseline.py and commit the lower numbers:\n" + string.Join("\n", slack));
        }
    }
}
