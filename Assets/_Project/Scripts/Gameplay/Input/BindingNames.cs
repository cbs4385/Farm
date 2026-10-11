using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Farm.Gameplay
{
    // What a binding is called on screen, and how a pressed control becomes a binding. Pads are named like an Xbox pad (A, B, X, Y, LB, RB, LT, RT, Menu, View),
    // the names the hints use; a generic joystick's buttons are "Button 3" and so on.
    public static class BindingNames
    {
        public const string Gamepad = "<Gamepad>/", Joystick = "<Joystick>/", Keyboard = "<Keyboard>/";

        // The first binding of the action on that kind of device ("<Gamepad>/", "<Keyboard>/"...), or -1. Parts of a composite do not count.
        public static int IndexOf(InputAction action, string devicePrefix)
        {
            for (var i = 0; i < action.bindings.Count; i++)
            {
                var b = action.bindings[i];
                if (b.isComposite || b.isPartOfComposite) continue;
                if (b.path != null && b.path.StartsWith(devicePrefix, System.StringComparison.Ordinal)) return i;
            }
            return -1;
        }

        // True when the player has rebound that binding.
        public static bool IsOverridden(InputAction action, int index) => index >= 0 && !string.IsNullOrEmpty(action.bindings[index].overridePath);

        public static string Describe(InputAction action, int index)
        {
            var path = action.bindings[index].effectivePath ?? "";
            return path.StartsWith(Gamepad, System.StringComparison.Ordinal) ? PadName(path.Substring(Gamepad.Length)) : action.GetBindingDisplayString(index);
        }

        public static string PadName(string control)
        {
            switch (control)
            {
                case "buttonSouth": return "A";
                case "buttonEast": return "B";
                case "buttonWest": return "X";
                case "buttonNorth": return "Y";
                case "leftShoulder": return "LB";
                case "rightShoulder": return "RB";
                case "leftTrigger": return "LT";
                case "rightTrigger": return "RT";
                case "start": return "Menu";
                case "select": return "View";
                case "leftStickPress": return "L-stick click";
                case "rightStickPress": return "R-stick click";
                case "dpad/up": return "D-pad up";
                case "dpad/down": return "D-pad down";
                case "dpad/left": return "D-pad left";
                case "dpad/right": return "D-pad right";
                default: return control;
            }
        }

        // The device-independent path of a pressed control ("<Gamepad>/rightTrigger"), or null when it is not on a pad.
        public static string PadPathOf(InputControl control)
        {
            if (control == null || control is AxisControl && control.device is Pointer) return null;
            var relative = control.path.Substring(control.device.path.Length + 1);
            if (control.device is Gamepad) return Gamepad + relative;
            if (control.device is Joystick) return Joystick + relative;
            return null;
        }

        // Makes the action answer to the pressed control: the pad binding (or the joystick binding, for a generic pad) takes it. False when the control is not on a pad or
        // the action has no binding for that kind of pad.
        public static bool RebindPad(InputAction action, InputControl control)
        {
            var path = PadPathOf(control);
            if (path == null) return false;
            var index = IndexOf(action, path.StartsWith(Gamepad, System.StringComparison.Ordinal) ? Gamepad : Joystick);
            if (index < 0) return false;
            action.ApplyBindingOverride(index, path);
            return true;
        }
    }
}
