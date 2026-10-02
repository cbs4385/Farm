using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Farm.Editor
{
    // T-005: command-line builds. See docs/BUILD.md.
    //   Unity -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildWindows
    //   Unity -batchmode -nographics -projectPath . -executeMethod Farm.Editor.BuildScript.BuildLinux
    // Optional args: -scriptingBackend il2cpp|mono (default mono), -development (includes the developer tools; output defaults to BuildsDev), -buildOutput <dir>
    public static class BuildScript
    {
        const string ExecutableName = "Farm";

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", $"{ExecutableName}.exe");
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux", ExecutableName);

        // Entry point for game-ci/unity-builder, which passes -buildTarget and -customBuildPath.
        public static void BuildFromCI()
        {
            var target = GetArg("-buildTarget") == "StandaloneLinux64" ? BuildTarget.StandaloneLinux64 : BuildTarget.StandaloneWindows64;
            var customPath = GetArg("-customBuildPath");
            if (!string.IsNullOrEmpty(customPath))
            {
                // customBuildPath is the full output file path; Build() composes <root>/<folder>/<version>/<exe>, so use its directory as the root override.
                var dir = Path.GetDirectoryName(customPath);
                var exe = Path.GetFileName(customPath);
                BuildTo(target, dir, exe);
                return;
            }
            if (target == BuildTarget.StandaloneLinux64) BuildLinux(); else BuildWindows();
        }

        [MenuItem("Farm/Build/Windows x64")]
        static void MenuWindows() => BuildWindows();

        [MenuItem("Farm/Build/Linux x64")]
        static void MenuLinux() => BuildLinux();

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
            }

            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

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
