using System;
using System.IO;

namespace Farm.Core
{
    // The game was renamed from "Farm" to "Wetherell Farm Saga" (2026-10-07). Unity keeps a game's saves and settings in a folder named after the
    // product (<company>/<product> under the platform's data folder), so after the rename the old saves would sit in a folder the game no longer
    // looks in. On the first start the new folder has no saves, the old one is found next to it and its saves and settings are copied across. The
    // old folder is left as it was (nothing is moved or deleted), and a game that already has saves in the new folder is never touched.
    public static class DataMigration
    {
        public const string OldProductFolder = "Farm";

        // What is carried over: the saves, the settings (and their backup) and the photos. Logs and bug reports stay where they were.
        static readonly string[] Folders = { "saves", "Photos" };
        static readonly string[] Files = { "settings.json", "settings.json.bak" };

        // `root` is the game's data folder (Application.persistentDataPath). Returns how many files were copied.
        public static int Run(string root, string oldFolderName = OldProductFolder)
        {
            if (string.IsNullOrEmpty(root)) return 0;
            var trimmed = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parent = Path.GetDirectoryName(trimmed);
            if (string.IsNullOrEmpty(parent)) return 0;
            var old = Path.Combine(parent, oldFolderName);
            if (!Directory.Exists(old) || SamePath(old, trimmed)) return 0;
            if (HasUserData(trimmed)) return 0;

            var copied = 0;
            Directory.CreateDirectory(trimmed);
            foreach (var folder in Folders)
            {
                var from = Path.Combine(old, folder);
                if (Directory.Exists(from)) copied += CopyFolder(from, Path.Combine(trimmed, folder));
            }
            foreach (var file in Files)
            {
                var from = Path.Combine(old, file);
                if (!File.Exists(from)) continue;
                File.Copy(from, Path.Combine(trimmed, file), false);
                copied++;
            }
            return copied;
        }

        // Does the new folder already hold saves or settings of its own?
        static bool HasUserData(string root)
        {
            var saves = Path.Combine(root, "saves");
            if (Directory.Exists(saves) && Directory.GetFileSystemEntries(saves).Length > 0) return true;
            return File.Exists(Path.Combine(root, "settings.json"));
        }

        static int CopyFolder(string from, string to)
        {
            Directory.CreateDirectory(to);
            var count = 0;
            foreach (var file in Directory.GetFiles(from))
            {
                File.Copy(file, Path.Combine(to, Path.GetFileName(file)), false);
                count++;
            }
            foreach (var dir in Directory.GetDirectories(from))
                count += CopyFolder(dir, Path.Combine(to, Path.GetFileName(dir)));
            return count;
        }

        static bool SamePath(string a, string b) =>
            string.Equals(Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }
}
