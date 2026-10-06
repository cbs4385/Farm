using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Farm.Core
{
    // The player's "Report a bug": a subject and a description, packed with the game's logs and some system details into one zip file, and an
    // email to the developers opened with the subject and description filled in. A game cannot attach a file to a mail it hands to the player's
    // mail program, and must never carry mail credentials, so the zip is left in a folder and the mail says which file to attach.
    // Nothing is sent without the player pressing send in their own mail program. Paths and the account name are removed from what is packed.
    public static class BugReport
    {
        public const string Address = "gamestrubios@gmail.com";
        public const string SubjectPrefix = "[Farm bug] ";
        public const int MinSubject = 3, MaxSubject = 120, MinBody = 5, MaxBody = 4000;
        public const long MaxLogBytes = 1024 * 1024;           // the end of each log: that is where the trouble is
        public const int MaxUrlLength = 1900;                  // mail programs and the shell refuse much longer links
        public const int KeepReports = 10;
        public const string FolderName = "BugReports";

        // How the mail program is opened; the tests replace it so that nothing opens.
        public static Action<string> OpenUrl = url => Application.OpenURL(url);

        public sealed class Request
        {
            public string Subject, Body;
            public DateTime UtcNow = DateTime.UtcNow;
            public string OutputDirectory;                     // where the zip goes
            public string LogDirectory;                        // farm.log and farm.prev.log
            public string ConsoleLogPath;                      // Unity's own Player.log
            public string SystemInfo;                          // computer, graphics, screen
            public string GameInfo;                            // version, language, settings, where in the game the player is
            public string SaveJson;                            // the current save, only when the player agreed
        }

        public sealed class Result
        {
            public bool Ok;
            public string ZipPath, Folder, MailtoUrl, Error;
        }

        // null when the text can be sent, else the string key that says what is missing.
        public static string Validate(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(subject) || subject.Trim().Length < MinSubject) return "bug.need_subject";
            if (string.IsNullOrWhiteSpace(body) || body.Trim().Length < MinBody) return "bug.need_body";
            return null;
        }

        // Takes the account name and the home folder out of text that is sent to somebody else. (pure)
        public static string Redact(string text, string homeFolder, string accountName, string machineName = null)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = Replace(text, homeFolder, "<home>");
            if (!string.IsNullOrEmpty(homeFolder)) text = Replace(text, homeFolder.Replace('\\', '/'), "<home>");
            text = Replace(text, accountName, "<user>");
            text = Replace(text, machineName, "<machine>");
            return text;
        }

        static string Replace(string text, string what, string with)
        {
            if (string.IsNullOrEmpty(what) || what.Length < 3) return text;       // a one or two letter name would mangle the text
            return text.Replace(what, with, StringComparison.OrdinalIgnoreCase);
        }

        // The `mailto:` link: addressed, with the subject and the start of the description, and the name of the file to attach. Never longer than
        // MaxUrlLength once escaped, whatever the player typed. (pure)
        public static string BuildMailto(string subject, string body, string zipPath, string gameInfo)
        {
            var tail = $"\n\n---\n{gameInfo}\nThe full report, with the game's logs, is this file: {zipPath}\nPlease attach it to this email before sending.";
            var cleanSubject = SubjectPrefix + (subject ?? string.Empty).Trim();
            var text = (body ?? string.Empty).Trim();
            var url = Compose(cleanSubject, text, tail);
            while (url.Length > MaxUrlLength && text.Length > 0)
            {
                text = text.Substring(0, Math.Max(0, text.Length - Math.Max(40, (url.Length - MaxUrlLength) / 2)));
                url = Compose(cleanSubject, text.TrimEnd() + (text.Length > 0 ? " [...]" : string.Empty), tail);
            }
            return url.Length > MaxUrlLength ? Compose(cleanSubject, string.Empty, $"\n\nPlease attach: {zipPath}") : url;
        }

        static string Compose(string subject, string body, string tail) =>
            $"mailto:{Address}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body + tail)}";

        // Packs the report. The zip holds report.txt, the logs (only their last megabyte, with the account name and home folder removed) and, when the
        // player agreed, the save. Older reports beyond the newest KeepReports are deleted.
        public static Result Create(Request r)
        {
            var result = new Result();
            try
            {
                var problem = Validate(r.Subject, r.Body);
                if (problem != null) { result.Error = problem; return result; }
                Directory.CreateDirectory(r.OutputDirectory);
                var zipPath = Path.Combine(r.OutputDirectory, $"bug-{r.UtcNow:yyyyMMdd-HHmmss}.zip");
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var account = Environment.UserName;
                var machine = Environment.MachineName;
                string Clean(string s) => Redact(s, home, account, machine);

                using (var stream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    AddText(zip, "report.txt", Clean(ReportText(r)));
                    if (!string.IsNullOrEmpty(r.LogDirectory))
                    {
                        AddLog(zip, "logs/farm.log", Path.Combine(r.LogDirectory, CrashLog.FileName), Clean);
                        AddLog(zip, "logs/farm.prev.log", Path.Combine(r.LogDirectory, CrashLog.PreviousName), Clean);
                    }
                    if (!string.IsNullOrEmpty(r.ConsoleLogPath)) AddLog(zip, "logs/Player.log", r.ConsoleLogPath, Clean);
                    if (!string.IsNullOrEmpty(r.SaveJson)) AddText(zip, "save.json", r.SaveJson);
                }

                Prune(r.OutputDirectory);
                result.ZipPath = zipPath;
                result.Folder = r.OutputDirectory;
                result.MailtoUrl = BuildMailto(r.Subject, Clean(r.Body), Clean(zipPath), Clean(r.GameInfo));
                result.Ok = true;
            }
            catch (Exception e)
            {
                result.Ok = false;
                result.Error = "bug.failed";
                Log.Warn("Bug report failed: " + e.Message);
            }
            return result;
        }

        // Creates the report and opens the mail program. The result says what was done.
        public static Result Send(Request r)
        {
            var result = Create(r);
            if (!result.Ok) return result;
            try { OpenUrl(result.MailtoUrl); }
            catch (Exception e) { Log.Warn("Could not open the mail program: " + e.Message); }
            return result;
        }

        static string ReportText(Request r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Subject: " + (r.Subject ?? string.Empty).Trim());
            sb.AppendLine("Sent (UTC): " + r.UtcNow.ToString("u"));
            sb.AppendLine();
            sb.AppendLine((r.Body ?? string.Empty).Trim());
            sb.AppendLine();
            sb.AppendLine("=== Game ===");
            sb.AppendLine(r.GameInfo ?? string.Empty);
            sb.AppendLine();
            sb.AppendLine("=== System ===");
            sb.AppendLine(r.SystemInfo ?? string.Empty);
            return sb.ToString();
        }

        static void AddText(ZipArchive zip, string name, string text)
        {
            var entry = zip.CreateEntry(name, System.IO.Compression.CompressionLevel.Optimal);
            using (var w = new StreamWriter(entry.Open(), new UTF8Encoding(false))) w.Write(text ?? string.Empty);
        }

        static void AddLog(ZipArchive zip, string name, string path, Func<string, string> clean)
        {
            var text = ReadTail(path);
            if (text == null) return;
            AddText(zip, name, clean(text));
        }

        // The last MaxLogBytes of a file that may be open in another program (Unity holds its own log open); null when there is none.
        public static string ReadTail(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                using (var f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    var skip = Math.Max(0, f.Length - MaxLogBytes);
                    f.Seek(skip, SeekOrigin.Begin);
                    using (var reader = new StreamReader(f, Encoding.UTF8)) return (skip > 0 ? "[... earlier lines left out ...]\n" : string.Empty) + reader.ReadToEnd();
                }
            }
            catch (Exception) { return null; }
        }

        static void Prune(string directory)
        {
            try
            {
                var old = new DirectoryInfo(directory).GetFiles("bug-*.zip").OrderByDescending(f => f.Name, StringComparer.Ordinal).Skip(KeepReports).ToList();
                foreach (var f in old) f.Delete();
            }
            catch (Exception) { /* tidying up must never fail the report */ }
        }

        public static string DefaultFolder(string dataRoot) => Path.Combine(dataRoot, FolderName);

        // Where the player can find the file: the folder as a link the shell opens.
        public static string FolderUrl(string folder) => "file:///" + folder.Replace('\\', '/').TrimStart('/');

        public static List<string> Contents(string zipPath)
        {
            using (var zip = ZipFile.OpenRead(zipPath)) return zip.Entries.Select(e => e.FullName).ToList();
        }
    }
}
