using System;

namespace Farm.Gameplay
{
    // Fire and forget: tells the player something (a notice at the top of the screen, or a message window) without the caller holding the UI or the session. Gameplay
    // code that used to reach for a static UI lookup just to say something takes this instead (docs/adr/0006-retire-the-service-locator.md).
    // It does nothing when there is no UI (tests, a bare scene), and never throws.
    public interface INotifier
    {
        void Toast(string text);
        void Message(string messageKey, Action onClose = null);
    }

    public sealed class NullNotifier : INotifier
    {
        public static readonly NullNotifier Instance = new NullNotifier();
        public void Toast(string text) { }
        public void Message(string messageKey, Action onClose = null) => onClose?.Invoke();
    }
}
