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

        // The hints in use: the input service's, once there is one (it calls Use). Code with the context reads them from the input service; this is for the language table, which
        // is static, and for labels that have no context. A fresh set stands in until then, and again after each test.
        public static InputHints Current { get; private set; } = new InputHints();

        public static event Action Changed;

        static ControlPrompts() => TestResets.Add(() => Use(new InputHints()));          // a test that used a pad must not leave the next one with the mouse ignored

        public static void Use(InputHints hints)
        {
            if (Current != null) Current.Changed -= Raise;
            Current = hints ?? new InputHints();
            Current.Changed += Raise;
            Raise();
        }

        static void Raise() => Changed?.Invoke();

        public static InputKind Kind => Current.Kind;
        public static void SetKind(InputKind kind) => Current.SetKind(kind);
        public static void NotifyChanged() => Current.NotifyChanged();

        public static void ResetForTests() { Use(new InputHints()); Changed = null; }

        public static string KeyFor(InputKind kind, string action) => (kind == InputKind.Gamepad ? "prompt.pad." : "prompt.kb.") + action;

        // The text with every [[Action]] replaced for the current kind of control. Cheap when there is no token.
        public static string Expand(string text) => Expand(text, Current.Kind, Current.Rebound);

        public static string Expand(string text, InputKind kind) => Expand(text, kind, Current.Rebound);

        public static string Expand(string text, InputKind kind, Func<InputKind, string, string> rebound)
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
                var custom = rebound?.Invoke(kind, name);
                result.Append(custom ?? (L.Has(key) ? L.Get(key) : name));
                i = end + Close.Length;
            }
            return result.ToString();
        }
    }
}
