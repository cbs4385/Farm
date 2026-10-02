#if FARM_STEAM
using System;
using Steamworks;

namespace Farm.Platform
{
    // Steamworks.NET implementation. Compiled only when the scripting define FARM_STEAM is set and the package is installed
    // (see docs/RELEASE.md). Any failure falls back to the null platform so the game still runs without Steam.
    public sealed class SteamPlatform : IPlatformServices
    {
        public string Name => "steam";
        public bool IsOnline => SteamAPI.IsSteamRunning();

        public static void TryInstall()
        {
            try
            {
                if (SteamAPI.Init()) PlatformServices.Use(new SteamPlatform());
            }
            catch (Exception e) { UnityEngine.Debug.LogWarning("Steam unavailable: " + e.Message); }
        }

        public void UnlockAchievement(string id)
        {
            if (!SteamAPI.IsSteamRunning()) return;
            SteamUserStats.SetAchievement(id);
            SteamUserStats.StoreStats();
        }
    }
}
#endif
