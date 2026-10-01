using System.IO;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // TextMeshPro needs its essential resources (default font, shaders) imported once per project.
    public static class TmpSetup
    {
        const string PackagePath = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        [MenuItem("Farm/Setup/Import TMP Essentials")]
        public static void Import()
        {
            if (Directory.Exists("Assets/TextMesh Pro/Resources"))
            {
                Debug.Log("[TmpSetup] TMP essentials already present.");
                return;
            }
            AssetDatabase.ImportPackage(PackagePath, false);
            AssetDatabase.Refresh();
            Debug.Log("[TmpSetup] Imported TMP essentials.");
        }

        // Package import is asynchronous in batch mode: exit only once it reports completion.
        public static void ImportAndExit()
        {
            if (Directory.Exists("Assets/TextMesh Pro/Resources"))
            {
                EditorApplication.Exit(0);
                return;
            }
            AssetDatabase.importPackageCompleted += _ => EditorApplication.Exit(0);
            AssetDatabase.importPackageFailed += (_, error) =>
            {
                Debug.LogError($"[TmpSetup] Import failed: {error}");
                EditorApplication.Exit(1);
            };
            AssetDatabase.ImportPackage(PackagePath, false);
        }
    }
}
