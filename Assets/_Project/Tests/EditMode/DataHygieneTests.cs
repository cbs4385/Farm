using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Technical-debt review 2026-10-09: mistakes that only show up in play are caught by looking at the sources and data directly.
    public class DataHygieneTests
    {
        static string ProjectPath(string relative) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        [Test]
        public void The_string_table_is_strict_json_with_no_raw_line_breaks_in_strings()
        {
            var s = File.ReadAllText(ProjectPath("Assets/_Project/Resources/Localization/en.json"));
            var inString = false; var escaped = false; var line = 1; var bad = new List<int>();
            foreach (var c in s)
            {
                if (c == '\n') { if (inString) bad.Add(line); line++; }
                if (inString) { if (escaped) escaped = false; else if (c == '\\') escaped = true; else if (c == '"') inString = false; }
                else if (c == '"') inString = true;
            }
            Assert.IsEmpty(bad, "raw line breaks inside strings (write \\n) at lines " + string.Join(", ", bad));
            Assert.DoesNotThrow(() => Newtonsoft.Json.Linq.JObject.Parse(s));
        }

        [Test]
        public void Every_sprite_name_written_in_the_code_has_a_picture()
        {
            var art = new HashSet<string>(Directory.GetFiles(ProjectPath("Assets/_Project"), "*.png", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension));
            var name = new Regex("\"((?:obj|prop)_[a-z0-9_]+)\"");
            var missing = new SortedSet<string>();
            foreach (var file in Directory.GetFiles(ProjectPath("Assets/_Project/Scripts"), "*.cs", SearchOption.AllDirectories))
                foreach (Match m in name.Matches(File.ReadAllText(file)))
                    if (!m.Groups[1].Value.EndsWith("_") && !art.Contains(m.Groups[1].Value)) missing.Add($"{m.Groups[1].Value} ({Path.GetFileName(file)})");
            Assert.IsEmpty(missing, "sprite names with no picture: " + string.Join(", ", missing));
        }
    }
}
