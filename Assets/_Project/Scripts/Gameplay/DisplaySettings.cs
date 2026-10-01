using System;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    public static class DisplaySettings
    {
        // Command-line display flags (used by QA capture runs) win over saved settings.
        public static void ApplyAtStartup(SettingsData s)
        {
            QualitySettings.vSyncCount = s.VSync ? 1 : 0;
            if (Application.isBatchMode || HasCommandLineResolution()) return;
            Apply(s);
        }

        public static void Apply(SettingsData s)
        {
            QualitySettings.vSyncCount = s.VSync ? 1 : 0;
            Screen.SetResolution(s.ResolutionWidth, s.ResolutionHeight,
                s.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        static bool HasCommandLineResolution()
        {
            foreach (var a in Environment.GetCommandLineArgs())
                if (a == "-screen-width" || a == "-screen-height" || a == "-screen-fullscreen") return true;
            return false;
        }
    }
}
