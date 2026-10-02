using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Farm.Editor
{
    // T-043: developer tools must never ship. After a non-development build, BuildScript scans the output for the
    // names of the debug types; finding one fails the build. Names are searched as UTF-8 and UTF-16 because managed
    // assemblies, IL2CPP metadata and native binaries store identifiers differently.
    public static class ReleaseGuard
    {
        public static readonly string[] DebugMarkers = { "DebugCommandProcessor", "DebugConsoleScreen" };

        static readonly string[] ScannedExtensions = { ".dll", ".dat", ".so", ".exe", ".dylib" };

        // Returns "<relative path>: <marker>" for every marker found in a scanned file.
        public static List<string> Scan(string directory, IEnumerable<string> markers = null)
        {
            var found = new List<string>();
            var patterns = new List<(string name, byte[] bytes)>();
            foreach (var marker in markers ?? DebugMarkers)
            {
                patterns.Add((marker, Encoding.UTF8.GetBytes(marker)));
                patterns.Add((marker, Encoding.Unicode.GetBytes(marker)));
            }

            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (Array.IndexOf(ScannedExtensions, ext) < 0) continue;

                var data = File.ReadAllBytes(file);
                foreach (var (name, bytes) in patterns)
                {
                    if (new ReadOnlySpan<byte>(data).IndexOf(bytes) < 0) continue;
                    var entry = $"{Path.GetRelativePath(directory, file)}: {name}";
                    if (!found.Contains(entry)) found.Add(entry);
                }
            }
            return found;
        }
    }
}
