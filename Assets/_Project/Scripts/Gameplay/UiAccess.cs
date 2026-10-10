using System;
using Farm.Core;

namespace Farm.Gameplay
{
    // How gameplay code asks the UI for something (open a shop, hide the hover label): the UI may be absent (tests, a bare scene), and then nothing happens. One place for
    // the "is there a UI" check, instead of the same lookup written out at each call.
    public static class UiAccess
    {
        public static IUiService Current => ServiceLocator.TryGet<IUiService>(out var ui) ? ui : null;

        public static bool AnyModalOpen => Current?.AnyModalOpen ?? false;
        public static bool PointerOverUi => Current?.PointerOverUi ?? false;

        // Runs `show` with the UI when there is one. Returns whether there was.
        public static bool Run(Action<IUiService> show)
        {
            var ui = Current;
            if (ui == null) return false;
            show(ui);
            return true;
        }
    }
}
