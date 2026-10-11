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
            ControlPrompts.Changed += OnKindChanged;
            ApplyKind();
        }

        void OnDisable()
        {
            _buttons?.Dispose();
            InputSystem.onDeviceChange -= OnDeviceChange;
            ControlPrompts.Changed -= OnKindChanged;
            Cursor.visible = true;
            SetPointerActions(true);
        }

        // The mouse pointer is hidden and ignored while a pad is in use (playtest 2026-10-11: "while using the controller the mouse icon still shows, and its hover effects still
        // fire") and comes back when the mouse moves or a key is pressed. The player is told which one is in use, at the start of a game and whenever it changes.
        public static bool CursorShownFor(InputKind kind) => kind != InputKind.Gamepad;
        public const float ToastGap = 4f;
        static float _lastToast = -100f;
        static GameState _announcedFor;

        static void OnKindChanged()
        {
            ApplyKind();
            if (Time.unscaledTime - _lastToast < ToastGap) return;          // a pad's drifting stick and a jogged mouse must not make a flicker of notices
            Announce();
        }

        static void ApplyKind()
        {
            Cursor.visible = CursorShownFor(ControlPrompts.Kind);
            SetPointerActions(CursorShownFor(ControlPrompts.Kind));
        }

        // The menus' pointer: while it is off, nothing under the mouse is hovered or clicked.
        static void SetPointerActions(bool on)
        {
            if (!ServiceLocator.TryGet<InputService>(out var input)) return;
            foreach (var name in new[] { InputNames.Point, InputNames.Click, InputNames.RightClick, InputNames.ScrollWheel })
            {
                var action = input.Ui.FindAction(name);
                if (action == null) continue;
                if (on) action.Enable(); else action.Disable();
            }
        }

        public static string AnnouncementKey(InputKind kind) => kind == InputKind.Gamepad ? "input.using_pad" : "input.using_kb";

        static void Announce()
        {
            if (!ServiceLocator.TryGet<GameSession>(out var session) || !session.InGame) return;
            _lastToast = Time.unscaledTime;
            session.Toast(L.Get(AnnouncementKey(ControlPrompts.Kind)));
        }

        // Called when a map starts: once for each game that is begun or loaded, says what the game is being played with.
        public static void AnnounceAtStart(GameSession session)
        {
            if (session == null || !session.InGame || ReferenceEquals(_announcedFor, session.State)) return;
            _announcedFor = session.State;
            ApplyKind();
            Announce();
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
