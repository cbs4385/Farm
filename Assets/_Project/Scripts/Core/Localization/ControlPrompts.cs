using System;
using System.Text;

namespace Farm.Core
{
    public enum InputKind { KeyboardMouse, Gamepad }

    // The names of the controls in on-screen hints, for whatever the player is using right now. A string may hold [[Action]] tokens ("[[Interact]] to
    // talk"); L.Get swaps each for the entry `prompt.kb.<Action>` (the keyboard and mouse) or `prompt.pad.<Action>` (an Xbox style pad: A, B, X, Y, LB,
    // RB, View, Menu) in the language table. A token with no entry is left as its bare name. Which one is used follows the last control the player touched
    // (InputDeviceWatcher sets it), so a hint is right for a pad player and a keyboard player alike.
    public static class ControlPrompts
    {
        public const string Open = "[[", Close = "]]";

        public static InputKind Kind { get; private set; } = InputKind.KeyboardMouse;
        public static event Action Changed;

        public static void SetKind(InputKind kind)
        {
            if (kind == Kind) return;
            Kind = kind;
            Changed?.Invoke();
        }

        public static void ResetForTests() { Kind = InputKind.KeyboardMouse; Changed = null; }

        public static string KeyFor(InputKind kind, string action) => (kind == InputKind.Gamepad ? "prompt.pad." : "prompt.kb.") + action;

        // The text with every [[Action]] replaced for the current kind of control. Cheap when there is no token.
        public static string Expand(string text) => Expand(text, Kind);

        public static string Expand(string text, InputKind kind)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(Open, StringComparison.Ordinal) < 0) return text;
            var result = new StringBuilder(text.Length + 16);
            var i = 0;
            while (i < text.Length)
            {
                var start = text.IndexOf(Open, i, StringComparison.Ordinal);
                if (start < 0) { result.Append(text, i, text.Length - i); break; }
                var end = text.IndexOf(Close, start + Open.Length, StringComparison.Ordinal);
                if (end < 0) { result.Append(text, i, text.Length - i); break; }
                result.Append(text, i, start - i);
                var name = text.Substring(start + Open.Length, end - start - Open.Length);
                var key = KeyFor(kind, name);
                result.Append(L.Has(key) ? L.Get(key) : name);
                i = end + Close.Length;
            }
            return result.ToString();
        }
    }
}
