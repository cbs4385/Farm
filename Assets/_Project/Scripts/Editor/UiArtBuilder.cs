using System.IO;
using System.Linq;
using Farm.Data;
using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // Collects the HUD, effect and UI sprites (hud_*, fx_*, ui_* except portraits) from Art/Placeholders into Resources/UiArt.asset,
    // so that code can ask for them by name.
    public static class UiArtBuilder
    {
        const string SpriteDir = "Assets/_Project/Art/Placeholders";
        const string AssetPath = "Assets/_Project/Resources/UiArt.asset";

        public static bool Wanted(string fileName) =>
            (fileName.StartsWith("hud_") || fileName.StartsWith("fx_") || fileName.StartsWith("ui_")) && !fileName.StartsWith("ui_portrait");

        [MenuItem("Farm/Setup/Build UI Art")]
        public static void Build()
        {
            var entries = Directory.GetFiles(SpriteDir, "*.png")
                .Select(p => p.Replace('\\', '/'))
                .Where(p => Wanted(Path.GetFileName(p)))
                .OrderBy(p => p, System.StringComparer.Ordinal)
                .Select(p => new UiArt.Entry { Name = Path.GetFileNameWithoutExtension(p), Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p) })
                .Where(e => e.Sprite != null)
                .ToList();
            var asset = AssetDatabase.LoadAssetAtPath<UiArt>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UiArt>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            asset.Set(entries);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UiArtBuilder] {entries.Count} sprites");
        }

        public static void BuildAndExit()
        {
            Build();
            EditorApplication.Exit(0);
        }
    }
}
