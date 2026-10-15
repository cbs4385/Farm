using System;

namespace Farm.Platform
{
    // The Steam implementation of IPlatformServices, free of any Steamworks.NET type so it compiles and is tested without the package
    // (SteamPlatform, under FARM_STEAM, supplies the real calls).
    public sealed class SteamPlatformServices : IPlatformServices
    {
        readonly Func<bool> _online;
        readonly Action<string> _unlock;

        public SteamPlatformServices(IRemoteStorageApi storage, Func<bool> online, Action<string> unlock)
        {
            Cloud = new SteamCloudStorage(storage);
            _online = online;
            _unlock = unlock;
        }

        public string Name => "steam";
        public bool IsOnline { get { try { return _online(); } catch (Exception) { return false; } } }
        public ICloudStorage Cloud { get; }

        public void UnlockAchievement(string id)
        {
            try { if (IsOnline) _unlock(id); }
            catch (Exception) { }
        }

        // Replaces the null platform only when Steam really started. Any failure (no Steam, no library, Steam refusing) leaves the game
        // on the platform it had, which is how a build without Steam runs.
        public static bool TryInstall(Func<bool> init, Func<IPlatformServices> create)
        {
            try
            {
                if (!init()) return false;
                PlatformServices.Use(create());
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Steam unavailable: " + e.Message);
                return false;
            }
        }
    }
}
