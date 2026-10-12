using System;

namespace Farm.Core
{
    // What the player is playing with right now (the pad or the keyboard and mouse), and the text for a control they have rebound. Owned by the input service, so each run
    // has its own and nothing is left over from the last one (docs/adr/0006-retire-the-service-locator.md). ControlPrompts is the pure expansion and the one place the
    // localization table (a static by design) finds the current hints.
    public sealed class InputHints
    {
        public InputKind Kind { get; private set; } = InputKind.KeyboardMouse;
        public event Action Changed;

        // The text for a control the player has rebound (null for one still on its default, which has curated text in the language table).
        public Func<InputKind, string, string> Rebound;

        public void SetKind(InputKind kind)
        {
            if (kind == Kind) return;
            Kind = kind;
            Changed?.Invoke();
        }

        // The player changed a binding or a setting that alters what the hints say.
        public void NotifyChanged() => Changed?.Invoke();

        public string Expand(string text) => ControlPrompts.Expand(text, Kind, Rebound);
    }
}
