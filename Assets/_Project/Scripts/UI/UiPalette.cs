using Farm.Core;
using UnityEngine;

namespace Farm.UI
{
    // Colours of the status bars. The colour-blind palette moves them to blue, orange and yellow, which stay apart for the
    // common kinds of red-green colour blindness (the bars also differ in position and label).
    public static class UiPalette
    {
        public static bool ColorblindOn =>
            ServiceLocator.TryGet<SettingsStore>(out var s) && s.Current.ColorblindPalette;

        public static Color Energy => ColorblindOn ? new Color(0.30f, 0.60f, 0.95f) : new Color(0.45f, 0.80f, 0.30f);
        public static Color Low => ColorblindOn ? new Color(0.95f, 0.60f, 0.10f) : UiKit.Danger;
        public static Color Health => ColorblindOn ? new Color(0.95f, 0.85f, 0.20f) : new Color(0.85f, 0.25f, 0.30f);
        public static Color Fatigue => ColorblindOn ? new Color(0.85f, 0.45f, 0.75f) : new Color(0.55f, 0.45f, 0.85f);
    }
}
