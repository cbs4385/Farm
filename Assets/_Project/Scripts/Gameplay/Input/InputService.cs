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

        public InputHints Hints { get; } = new InputHints();

        public InputService(InputActionAsset sourceAsset)
        {
            _asset = Object.Instantiate(sourceAsset);
            Gameplay = _asset.FindActionMap(InputNames.GameplayMap, true);
            Ui = _asset.FindActionMap(InputNames.UiMap, true);
            Ui.Enable();   // UI navigation is always available
            Hints.Rebound = ReboundPrompt;
            ControlPrompts.Use(Hints);
        }

        public InputActionAsset Asset => _asset;
        public InputActionMap Gameplay { get; }
        public InputActionMap Ui { get; }

        public InputAction Move => Gameplay[InputNames.Move];
        public InputAction UseTool => Gameplay[InputNames.UseTool];
        public InputAction Interact => Gameplay[InputNames.Interact];
        public InputAction Rotate => Gameplay[InputNames.Rotate];
        public InputAction HotbarNext => Gameplay[InputNames.HotbarNext];
        public InputAction HotbarPrev => Gameplay[InputNames.HotbarPrev];
        public InputAction RowNext => Gameplay[InputNames.RowNext];
        public InputAction RowPrev => Gameplay[InputNames.RowPrev];
        public InputAction Rearrange => Gameplay[InputNames.Rearrange];
        public InputAction Inventory => Gameplay[InputNames.Inventory];
        public InputAction Pause => Gameplay[InputNames.Pause];
        public InputAction Menu => Gameplay[InputNames.Menu];
        public InputAction Journal => Gameplay[InputNames.Journal];
        public InputAction Run => Gameplay[InputNames.Run];
        public InputAction Crafting => Gameplay[InputNames.Crafting];

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

        // Puts every pad and joystick button back where it was, leaving the keyboard keys as the player set them.
        public void ResetPadBindings()
        {
            foreach (var action in _asset)
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var path = action.bindings[i].path ?? "";
                    if (path.StartsWith(BindingNames.Gamepad, System.StringComparison.Ordinal) || path.StartsWith(BindingNames.Joystick, System.StringComparison.Ordinal))
                        action.RemoveBindingOverride(i);
                }
        }

        // The hint text for a control the player has moved ("click (or press V)", "press RT"); null while it is on its default, which has curated wording.
        string ReboundPrompt(InputKind kind, string token)
        {
            var cap = token.EndsWith("Cap", System.StringComparison.Ordinal);
            var again = token.EndsWith("Again", System.StringComparison.Ordinal);
            var name = cap ? token.Substring(0, token.Length - 3) : again ? token.Substring(0, token.Length - 5) : token;
            var action = Gameplay.FindAction(name) ?? Ui.FindAction(name);
            if (action == null) return null;
            var index = BindingNames.IndexOf(action, kind == InputKind.Gamepad ? BindingNames.Gamepad : BindingNames.Keyboard);
            if (!BindingNames.IsOverridden(action, index)) return null;
            var key = BindingNames.Describe(action, index);
            string text;
            if (name == InputNames.UseTool)
            {
                if (kind == InputKind.Gamepad) text = L.Get(again ? "prompt.rebound.use_again.pad" : "prompt.rebound.use.pad", key);
                else if (again) return null;
                else text = L.Get("prompt.rebound.use.kb", key);
                return cap && text.Length > 0 ? char.ToUpperInvariant(text[0]) + text.Substring(1) : text;
            }
            return key;
        }

        // Human-readable binding for prompts and the rebind screen (first non-gamepad binding of the action).
        public string DisplayBinding(string actionName, string group = "Keyboard&Mouse")
        {
            var action = Gameplay[actionName];
            var index = action.GetBindingIndex(group);
            return index >= 0 ? action.GetBindingDisplayString(index) : action.GetBindingDisplayString();
        }
    }
}
