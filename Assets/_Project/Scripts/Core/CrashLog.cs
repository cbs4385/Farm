using System;
using System.IO;
using UnityEngine;

namespace Farm.Core
{
    // T-065: a log file players can send with a bug report. Errors, exceptions and asserts are always written; the previous
    // run's file is kept as farm.prev.log. Lives under the persistent data path (case-correct, no Windows-only APIs).
    public sealed class CrashLog : IDisposable
    {
        public const string FileName = "farm.log";
        public const string PreviousName = "farm.prev.log";
        public const long MaxBytes = 2 * 1024 * 1024;

        static CrashLog _installed;

        readonly string _path;
        readonly object _lock = new object();
        long _bytes;

        public string FilePath => _path;

        public CrashLog(string directory)
        {
            Directory.CreateDirectory(directory);
            _path = Path.Combine(directory, FileName);
            var previous = Path.Combine(directory, PreviousName);
            try
            {
                if (File.Exists(_path))
                {
                    if (File.Exists(previous)) File.Delete(previous);
                    File.Move(_path, previous);
                }
            }
            catch (Exception) { /* a locked old log must not stop the game */ }
            Write($"=== {Application.productName} {Application.version} on {Application.platform} at {DateTime.UtcNow:u} ===");
        }

        public void Write(string line)
        {
            lock (_lock)
            {
                if (_bytes > MaxBytes) return;
                try
                {
                    File.AppendAllText(_path, line + "\n");
                    _bytes += line.Length + 1;
                }
                catch (Exception) { /* never throw out of logging */ }
            }
        }

        public void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;
            Write($"[{type}] {message}" + (type == LogType.Warning || string.IsNullOrEmpty(stackTrace) ? string.Empty : "\n" + stackTrace));
        }

        public static void Install(string directory)
        {
            if (_installed != null) return;
            _installed = new CrashLog(directory);
            Application.logMessageReceivedThreaded += _installed.OnLogMessage;
        }

        public void Dispose()
        {
            if (_installed == this) { Application.logMessageReceivedThreaded -= OnLogMessage; _installed = null; }
        }
    }
}
