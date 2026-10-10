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
            NoticeGenericPad(control);
            if (control.device is Gamepad || control.device is Joystick) ControlPrompts.SetKind(InputKind.Gamepad);
            else if (control.device is Keyboard || control.device is Mouse) ControlPrompts.SetKind(InputKind.KeyboardMouse);
        }

        void Update()
        {
            SayPendingNotice();
            var joy = Joystick.current;
            if (joy != null && joy.stick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold) ControlPrompts.SetKind(InputKind.Gamepad);
            var pad = Gamepad.current;
            if (pad != null && (pad.leftStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold || pad.rightStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold))
                ControlPrompts.SetKind(InputKind.Gamepad);
            var mouse = Mouse.current;
            if (mouse != null && mouse.delta.ReadValue().sqrMagnitude > MouseThreshold * MouseThreshold) ControlPrompts.SetKind(InputKind.KeyboardMouse);
        }

        // A controller that Windows shows only as a generic joystick (a DirectInput pad, an "EasySMX" in its default mode) has no standard layout: its buttons are
        // guessed. When the player presses one of its buttons, say once what to do if they do not fit (playtest 2026-10-09: a pad that did nothing). Many PCs list odd
        // joystick-class devices (lighting, wheels) that nobody plays with, so merely being plugged in says nothing.
        static bool _noticed;
        static bool _pendingNotice;

        static void NoticeGenericPad(InputControl control)
        {
            if (_noticed || !(control.device is Joystick) || control.device is Gamepad) return;
            _pendingNotice = true;
        }

        // Said once, in a game (not while the game is still starting, when the texts are not there yet).
        static void SayPendingNotice()
        {
            if (!_pendingNotice || !ServiceLocator.TryGet<GameSession>(out var session) || !session.InGame || !L.Has("toast.generic_pad")) return;
            _pendingNotice = false;
            _noticed = true;
            session.Toast(L.Get("toast.generic_pad"));
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
