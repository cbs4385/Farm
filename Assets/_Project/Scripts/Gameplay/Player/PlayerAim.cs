using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Where the tool square is: the cell in front of the player, or, while the mouse is steering it, the one around the player that the pointer is over (AimMath).
    public sealed class PlayerAim
    {
        readonly GameSession _session;
        readonly FarmMap _map;
        readonly PlayerController _player;
        readonly InputService _input;
        bool _mouseAim;
        Vector2 _lastMouse;
        Vector3Int _pointerCell;

        public PlayerAim(GameSession session, FarmMap map, PlayerController player, InputService input)
        {
            _session = session; _map = map; _player = player; _input = input;
        }

        public bool MouseAiming => _mouseAim;
        public Vector3Int PointerCell => _pointerCell;

        public Vector3Int Target
        {
            get
            {
                var cell = _map.WorldToCell(_player.CellSamplePoint);
                if (_mouseAim) return AimMath.Target(cell, _pointerCell);
                return cell + new Vector3Int(_player.Facing.x, _player.Facing.y, 0);
            }
        }

        // Pointer speed: the movement of the mouse since the last frame is scaled by the setting, by moving the pointer on by the difference. Over a window it is left alone.
        static Vector2 ScaledPointer(UnityEngine.InputSystem.Mouse mouse, Vector2 position, float sensitivity, InputKind kind)
        {
            if (Mathf.Approximately(sensitivity, 1f) || kind == InputKind.Gamepad) return position;
            var scaled = PointerMath.Scale(position, mouse.delta.ReadValue(), sensitivity, new Vector2(Screen.width, Screen.height));
            if (scaled != position) mouse.WarpCursorPosition(scaled);
            return scaled;
        }

        // The mouse takes over the square when it moves over the world and gives it back as soon as the player walks.
        public void Update(IUiService ui)
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var settings = _session.Settings;
            if (mouse == null || settings == null || !settings.MouseAim || _input.GameplayBlocked || _input.Hints.Kind == InputKind.Gamepad) { _mouseAim = false; return; }          // a pad in use: the mouse is ignored
            var position = mouse.position.ReadValue();
            position = ScaledPointer(mouse, position, settings.MouseSensitivity, _input.Hints.Kind);
            var moved = (position - _lastMouse).sqrMagnitude > 1f;
            _lastMouse = position;
            var walking = _input.Move.ReadValue<Vector2>().sqrMagnitude > 0.04f;
            var overUi = ui != null && ui.PointerOverUi;
            if (walking) _mouseAim = false;
            else if (moved && !overUi) _mouseAim = true;
            if (!_mouseAim) return;
            var cam = Camera.main;
            if (cam == null) { _mouseAim = false; return; }
            var world = cam.ScreenToWorldPoint(new Vector3(position.x, position.y, -cam.transform.position.z));
            _pointerCell = _map.WorldToCell(world);
            var playerCell = _map.WorldToCell(_player.CellSamplePoint);
            var facing = AimMath.FacingToward(playerCell, AimMath.Target(playerCell, _pointerCell), _player.Facing);
            if (facing != _player.Facing) _player.Face(facing);
        }
    }
}
