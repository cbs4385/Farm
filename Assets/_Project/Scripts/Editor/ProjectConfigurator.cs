using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Farm.Editor
{
    // T-002: applies the project-wide settings from docs/02-TechnicalDesign.md. Idempotent.
    public static class ProjectConfigurator
    {
        public const string CompanyName = "Farm Studio";
        public const string ProductName = "Farm";
        public const string Version = "0.0.1";

        [MenuItem("Farm/Setup/Apply Project Settings")]
        public static void Apply()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);

            // Standalone (Windows/Linux): Mono for fast iteration; flipped to IL2CPP by BuildScript for release builds.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, true);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64, true);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ProjectConfigurator] Applied settings (version {Version}).");
        }

        // For -executeMethod runs.
        public static void ApplyAndExit()
        {
            Apply();
            EditorApplication.Exit(0);
        }
    }
}
