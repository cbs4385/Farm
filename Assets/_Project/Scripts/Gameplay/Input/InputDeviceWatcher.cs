using Farm.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Farm.Gameplay
{
    // Tells the hints (ControlPrompts) what the player is playing with: the pad as soon as one of its buttons or sticks is touched, the keyboard and mouse
    // as soon as a key is pressed or the mouse moves. Unplugging the pad while it is the one in use stops the game with the pause menu, so nothing happens
    // behind the player's back (a pad that went to sleep or lost its battery).
    public sealed class InputDeviceWatcher : MonoBehaviour
    {
        const float StickThreshold = 0.5f, MouseThreshold = 3f;
        System.IDisposable _buttons;

        void OnEnable()
        {
            _buttons = InputSystem.onAnyButtonPress.Call(OnButton);
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDisable()
        {
            _buttons?.Dispose();
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        static void OnButton(InputControl control)
        {
            if (control.device is Gamepad) ControlPrompts.SetKind(InputKind.Gamepad);
            else if (control.device is Keyboard || control.device is Mouse) ControlPrompts.SetKind(InputKind.KeyboardMouse);
        }

        void Update()
        {
            var pad = Gamepad.current;
            if (pad != null && (pad.leftStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold || pad.rightStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold))
                ControlPrompts.SetKind(InputKind.Gamepad);
            var mouse = Mouse.current;
            if (mouse != null && mouse.delta.ReadValue().sqrMagnitude > MouseThreshold * MouseThreshold) ControlPrompts.SetKind(InputKind.KeyboardMouse);
        }

        static void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is Gamepad) || (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected)) return;
            if (Gamepad.all.Count > 0) return;                                   // another pad is still there
            var wasPlaying = ControlPrompts.Kind == InputKind.Gamepad;
            ControlPrompts.SetKind(InputKind.KeyboardMouse);
            if (wasPlaying && ServiceLocator.TryGet<GameSession>(out var session) && session.InGame && ServiceLocator.TryGet<IUiService>(out var ui) && !ui.AnyModalOpen)
                ui.ShowPause();
        }
    }
}
