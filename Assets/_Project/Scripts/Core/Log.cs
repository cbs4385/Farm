using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Farm.Core
{
    // Thin wrapper so logging can be redirected (log file, Steam, etc.) in one place later.
    public static class Log
    {
        public static void Info(string message) => Debug.Log(message);
        public static void Warn(string message) => Debug.LogWarning(message);
        public static void Error(string message) => Debug.LogError(message);

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(string message) => Debug.Log(message);
    }
}
