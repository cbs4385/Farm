using System;
using System.IO;
using System.Text;

namespace Farm.Core
{
    // Crash-safe text file writes: write to <path>.tmp, then swap into place keeping the previous file as <path>.bak.
    public static class AtomicFile
    {
        const string TempSuffix = ".tmp";
        const string BackupSuffix = ".bak";

        public static void Write(string path, string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var temp = path + TempSuffix;
            var backup = path + BackupSuffix;
            File.WriteAllText(temp, contents, new UTF8Encoding(false));

            if (File.Exists(path))
            {
                // File.Replace is atomic on NTFS/POSIX and keeps the old file as the backup.
                File.Replace(temp, path, backup);
            }
            else
            {
                File.Move(temp, path);
            }
        }

        // Reads the main file; if it is missing or rejected by `isValid`, falls back to the backup.
        // Returns null when neither is usable. `usedBackup` tells the caller a recovery happened.
        public static string Read(string path, Func<string, bool> isValid, out bool usedBackup)
        {
            usedBackup = false;
            var main = TryRead(path, isValid);
            if (main != null) return main;

            var backup = TryRead(path + BackupSuffix, isValid);
            if (backup != null) usedBackup = true;
            return backup;
        }

        static string TryRead(string path, Func<string, bool> isValid)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var text = File.ReadAllText(path, Encoding.UTF8);
                return isValid == null || isValid(text) ? text : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
