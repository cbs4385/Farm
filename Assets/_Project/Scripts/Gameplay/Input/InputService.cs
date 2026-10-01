using Farm.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Gameplay
{
    // Owns a private instance of the input actions asset (so rebinding never edits the shared asset),
    // and switches between gameplay and UI control modes. Rebinds persist through SettingsData.
    public sealed class InputService
    {
        readonly InputActionAsset _asset;

        public InputService(InputActionAsset sourceAsset)
        {
            _asset = Object.Instantiate(sourceAsset);
            Gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            Ui = _asset.FindActionMap(InputNames.UiMap, true);
            Ui.Enable();   // UI navigation is always available
        }

        public InputActionAsset Asset => _asset;
        public InputActionMap Gameplay { get; }
        public InputActionMap Ui { get; }

        public InputAction Move => Gameplay[InputNames.Move];
        public InputAction UseTool => Gameplay[InputNames.UseTool];
        public InputAction Interact => Gameplay[InputNames.Interact];
        public InputAction HotbarNext => Gameplay[InputNames.HotbarNext];
        public InputAction HotbarPrev => Gameplay[InputNames.HotbarPrev];
        public InputAction Inventory => Gameplay[InputNames.Inventory];
        public InputAction Pause => Gameplay[InputNames.Pause];

        // Ref-counted so overlapping menus do not re-enable gameplay input prematurely.
        int _gameplayBlockers;

        public void BlockGameplay()
        {
            if (_gameplayBlockers++ == 0) Gameplay.Disable();
        }

        public void UnblockGameplay()
        {
            if (_gameplayBlockers == 0) return;
            if (--_gameplayBlockers == 0) Gameplay.Enable();
        }

        public bool GameplayBlocked => _gameplayBlockers > 0;

        public void EnableGameplay()
        {
            if (_gameplayBlockers == 0) Gameplay.Enable();
        }

        public InputAction Hotbar(int index) => Gameplay[InputNames.HotbarPrefix + (index + 1)];

        // Binding overrides <-> settings string.
        public void ApplyOverrides(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            _asset.LoadBindingOverridesFromJson(json);
        }

        public string ExportOverrides() => _asset.SaveBindingOverridesAsJson();

        public void ResetBindings() => _asset.RemoveAllBindingOverrides();

        // Human-readable binding for prompts and the rebind screen (first non-gamepad binding of the action).
        public string DisplayBinding(string actionName, string group = "Keyboard&Mouse")
        {
            var action = Gameplay[actionName];
            var index = action.GetBindingIndex(group);
            return index >= 0 ? action.GetBindingDisplayString(index) : action.GetBindingDisplayString();
        }
    }
}
