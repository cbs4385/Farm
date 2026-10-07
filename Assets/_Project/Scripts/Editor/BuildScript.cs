using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Farm.Editor
{
    // T-005: command-line builds. See docs/BUILD.md.
    //   Unity -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildWindows
    //   Unity -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildLinux
    //   Unity -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildMac
    // Optional args: -scriptingBackend il2cpp|mono (default mono), -development (includes the developer tools; output defaults to BuildsDev), -buildOutput <dir>,
    // -macArchitecture x64|arm64|universal (macOS only; default universal on a Mac and x64 elsewhere, see MacArchitecture)
    public static class BuildScript
    {
        const string ExecutableName = "Farm";

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", $"{ExecutableName}.exe");
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux", ExecutableName);
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "Mac", $"{ExecutableName}.app");

        // Entry point for game-ci/unity-builder, which passes -buildTarget and -customBuildPath.
        public static void BuildFromCI()
        {
            var named = GetArg("-buildTarget");
            var target = named == "StandaloneLinux64" ? BuildTarget.StandaloneLinux64 : named == "StandaloneOSX" ? BuildTarget.StandaloneOSX : BuildTarget.StandaloneWindows64;
            var customPath = GetArg("-customBuildPath");
            if (!string.IsNullOrEmpty(customPath))
            {
                // customBuildPath is the full output file path; Build() composes <root>/<folder>/<version>/<exe>, so use its directory as the root override.
                var dir = Path.GetDirectoryName(customPath);
                var exe = Path.GetFileName(customPath);
                BuildTo(target, dir, exe);
                return;
            }
            if (target == BuildTarget.StandaloneLinux64) BuildLinux(); else if (target == BuildTarget.StandaloneOSX) BuildMac(); else BuildWindows();
        }

        [MenuItem("Farm/Build/Windows x64")]
        static void MenuWindows() => BuildWindows();

        [MenuItem("Farm/Build/Linux x64")]
        static void MenuLinux() => BuildLinux();

        [MenuItem("Farm/Build/macOS")]
        static void MenuMac() => BuildMac();

        static void Build(BuildTarget target, string folder, string exe)
        {
            // Development builds (which include the developer tools) go to their own folder by default.
            var outputRoot = GetArg("-buildOutput") ?? (HasFlag("-development") ? "BuildsDev" : "Builds");
            BuildTo(target, Path.Combine(outputRoot, folder, PlayerSettings.bundleVersion), exe);
        }

        static void BuildTo(BuildTarget target, string outDir, string exe)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
                Fail("No enabled scenes in Build Settings. Run 'Farm/Setup/Create M0 Scenes'.");

            Directory.CreateDirectory(outDir);

            var group = BuildPipeline.GetBuildTargetGroup(target);
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            var previousBackend = PlayerSettings.GetScriptingBackend(namedTarget);
            var backend = string.Equals(GetArg("-scriptingBackend"), "il2cpp", StringComparison.OrdinalIgnoreCase)
                ? ScriptingImplementation.IL2CPP
                : ScriptingImplementation.Mono2x;
            PlayerSettings.SetScriptingBackend(namedTarget, backend);

            var options = BuildOptions.None;
            if (HasFlag("-development")) options |= BuildOptions.Development;

            var previousMacArchitecture = target == BuildTarget.StandaloneOSX ? SetMacArchitecture(MacArchitecture(GetArg("-macArchitecture"))) : null;
            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = Path.Combine(outDir, exe),
                    target = target,
                    options = options,
                });

                var summary = report.summary;
                Debug.Log($"[BuildScript] {target}: {summary.result}, {summary.totalErrors} errors, " +
                          $"{summary.totalSize / (1024 * 1024)} MB, backend {backend}, output {outDir}");
                if (summary.result != BuildResult.Succeeded) Fail($"Build failed: {summary.result}");

                // Release builds must not contain the developer tools (T-043).
                if ((options & BuildOptions.Development) == 0)
                {
                    var leaks = ReleaseGuard.Scan(outDir);
                    if (leaks.Count > 0) Fail("Developer tools found in a release build: " + string.Join("; ", leaks));
                    Debug.Log("[BuildScript] Release guard: no developer tools in the output.");
                }
            }
            finally
            {
                PlayerSettings.SetScriptingBackend(namedTarget, previousBackend);
                if (previousMacArchitecture != null) RestoreMacArchitecture(previousMacArchitecture);
            }

            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        // The macOS architecture: a universal app (Intel and Apple silicon) when built on a Mac, where Unity can sign it; Intel only elsewhere, because
        // Apple silicon refuses an unsigned arm64 app, while an unsigned Intel app still runs there through Rosetta 2. -macArchitecture overrides.
        public static string MacArchitecture(string requested)
        {
            switch ((requested ?? "").ToLowerInvariant())
            {
                case "x64": case "intel": return "x64";
                case "arm64": return "ARM64";
                case "universal": case "x64arm64": return "x64ARM64";
            }
            return Application.platform == RuntimePlatform.OSXEditor ? "x64ARM64" : "x64";
        }

        const string MacSettingsType = "UnityEditor.OSXStandalone.UserBuildSettings, UnityEditor.OSXStandalone.Extensions";

        // Sets the architecture through reflection (the type only exists where Mac build support is installed, so CI machines without it still compile this).
        // Returns the previous value to restore, or null when it could not be set.
        static object SetMacArchitecture(string name)
        {
            var property = Type.GetType(MacSettingsType)?.GetProperty("architecture", BindingFlags.Public | BindingFlags.Static);
            if (property == null) { Debug.LogWarning("[BuildScript] Mac build support is not installed: building with the default architecture."); return null; }
            var previous = property.GetValue(null);
            property.SetValue(null, Enum.Parse(property.PropertyType, name));
            Debug.Log($"[BuildScript] macOS architecture: {name}");
            return previous;
        }

        static void RestoreMacArchitecture(object previous) =>
            Type.GetType(MacSettingsType)?.GetProperty("architecture", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, previous);

        static void Fail(string message)
        {
            Debug.LogError($"[BuildScript] {message}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            throw new BuildFailedException(message);
        }

        static bool HasFlag(string name) => Environment.GetCommandLineArgs().Contains(name);

        static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
