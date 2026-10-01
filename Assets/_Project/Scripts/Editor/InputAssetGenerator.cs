using System.IO;
using Farm.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Editor
{
    // T-010: builds Resources/FarmInput.inputactions from code so bindings are reviewable and reproducible.
    public static class InputAssetGenerator
    {
        const string OutPath = "Assets/_Project/Resources/FarmInput.inputactions";
        const string Kbm = "Keyboard&Mouse";
        const string Pad = "Gamepad";

        [MenuItem("Farm/Setup/Generate Input Actions")]
        public static void Generate()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "FarmInput";
            AddGameplay(asset);
            AddUi(asset);

            Directory.CreateDirectory(Path.GetDirectoryName(OutPath));
            File.WriteAllText(OutPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(OutPath);
            Debug.Log($"[InputAssetGenerator] Wrote {OutPath}");
        }

        public static void GenerateAndExit()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        static void AddGameplay(InputActionAsset asset)
        {
            var map = asset.AddActionMap(InputNames.GameplayMap);

            var move = map.AddAction(InputNames.Move, InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w", Kbm).With("Down", "<Keyboard>/s", Kbm)
                .With("Left", "<Keyboard>/a", Kbm).With("Right", "<Keyboard>/d", Kbm);
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow", Kbm).With("Down", "<Keyboard>/downArrow", Kbm)
                .With("Left", "<Keyboard>/leftArrow", Kbm).With("Right", "<Keyboard>/rightArrow", Kbm);
            move.AddBinding("<Gamepad>/leftStick", groups: Pad);
            move.AddCompositeBinding("2DVector").With("Up", "<Gamepad>/dpad/up", Pad).With("Down", "<Gamepad>/dpad/down", Pad)
                .With("Left", "<Gamepad>/dpad/left", Pad).With("Right", "<Gamepad>/dpad/right", Pad);

            Button(map, InputNames.UseTool, ("<Mouse>/leftButton", Kbm), ("<Keyboard>/c", Kbm), ("<Gamepad>/buttonWest", Pad));
            Button(map, InputNames.Interact, ("<Keyboard>/e", Kbm), ("<Mouse>/rightButton", Kbm), ("<Gamepad>/buttonSouth", Pad));
            Button(map, InputNames.HotbarNext, ("<Mouse>/scroll/up", Kbm), ("<Keyboard>/period", Kbm), ("<Gamepad>/rightShoulder", Pad));
            Button(map, InputNames.HotbarPrev, ("<Mouse>/scroll/down", Kbm), ("<Keyboard>/comma", Kbm), ("<Gamepad>/leftShoulder", Pad));
            Button(map, InputNames.Inventory, ("<Keyboard>/tab", Kbm), ("<Keyboard>/i", Kbm), ("<Gamepad>/buttonNorth", Pad));
            Button(map, InputNames.Pause, ("<Keyboard>/escape", Kbm), ("<Gamepad>/start", Pad));

            var keys = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "minus", "equals" };
            for (var i = 0; i < keys.Length; i++)
                Button(map, InputNames.HotbarPrefix + (i + 1), ($"<Keyboard>/{keys[i]}", Kbm));
        }

        static void AddUi(InputActionAsset asset)
        {
            var map = asset.AddActionMap(InputNames.UiMap);

            var nav = map.AddAction(InputNames.Navigate, InputActionType.PassThrough, expectedControlLayout: "Vector2");
            nav.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow", Kbm).With("Down", "<Keyboard>/downArrow", Kbm)
                .With("Left", "<Keyboard>/leftArrow", Kbm).With("Right", "<Keyboard>/rightArrow", Kbm);
            nav.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w", Kbm).With("Down", "<Keyboard>/s", Kbm)
                .With("Left", "<Keyboard>/a", Kbm).With("Right", "<Keyboard>/d", Kbm);
            nav.AddBinding("<Gamepad>/leftStick", groups: Pad);
            nav.AddCompositeBinding("2DVector").With("Up", "<Gamepad>/dpad/up", Pad).With("Down", "<Gamepad>/dpad/down", Pad)
                .With("Left", "<Gamepad>/dpad/left", Pad).With("Right", "<Gamepad>/dpad/right", Pad);

            Button(map, InputNames.Submit, ("<Keyboard>/enter", Kbm), ("<Keyboard>/space", Kbm), ("<Gamepad>/buttonSouth", Pad));
            Button(map, InputNames.Cancel, ("<Keyboard>/escape", Kbm), ("<Gamepad>/buttonEast", Pad));
            map.AddAction(InputNames.Point, InputActionType.PassThrough, expectedControlLayout: "Vector2")
                .AddBinding("<Mouse>/position", groups: Kbm);
            map.AddAction(InputNames.Click, InputActionType.PassThrough)
                .AddBinding("<Mouse>/leftButton", groups: Kbm);
            map.AddAction(InputNames.ScrollWheel, InputActionType.PassThrough, expectedControlLayout: "Vector2")
                .AddBinding("<Mouse>/scroll", groups: Kbm);
        }

        static void Button(InputActionMap map, string name, params (string path, string group)[] bindings)
        {
            var action = map.AddAction(name, InputActionType.Button);
            foreach (var (path, group) in bindings) action.AddBinding(path, groups: group);
        }
    }
}
