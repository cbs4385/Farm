using System;

namespace Farm.Gameplay
{
    // The game's notifier: a notice goes out as a toast request on the event bus (the HUD shows it); a message opens the message window when the UI exists.
    public sealed class Notifier : INotifier
    {
        readonly GameSession _session;
        readonly Func<IUiService> _ui;

        public Notifier(GameSession session, Func<IUiService> ui)
        {
            _session = session;
            _ui = ui;
        }

        public void Toast(string text)
        {
            if (!string.IsNullOrEmpty(text)) _session?.Toast(text);
        }

        public void Message(string messageKey, Action onClose = null)
        {
            var ui = _ui?.Invoke();
            if (ui == null) { onClose?.Invoke(); return; }
            ui.ShowMessage(messageKey, onClose);
        }
    }
}
