#if FARM_STEAM
using System;
using Steamworks;
using UnityEngine;

namespace Farm.Platform
{
    // Steamworks.NET glue. Compiled only with the scripting define FARM_STEAM (BuildScript sets it for player builds that go to Steam;
    // the Editor and the tests never see it). Any failure leaves the game on the null platform, so it still runs without Steam.
    public static class SteamPlatform
    {
        public const uint AppId = 5408390;

        // When true, a game started outside Steam is relaunched through Steam (SteamAPI.RestartAppIfNecessary). Off on purpose: the
        // same files are also handed out as plain downloads that must run without Steam (CLAUDE.md), and the relaunch would quit them.
        const bool RequireSteamLaunch = false;

        public static bool TryInstall() => SteamPlatformServices.TryInstall(Init, Create);

        static bool Init()
        {
            if (RequireSteamLaunch && SteamAPI.RestartAppIfNecessary(new AppId_t(AppId))) { Application.Quit(); return false; }
            // Outside Steam the library reads steam_appid.txt next to the executable (development only: it is never shipped).
            if (!SteamAPI.Init()) { Debug.Log("Steam is not running (or the app is not owned): saves stay on this computer."); return false; }
            SteamPump.Create();
            return true;
        }

        static IPlatformServices Create() =>
            new SteamPlatformServices(new SteamworksRemoteStorage(), SteamAPI.IsSteamRunning, id =>
            {
                SteamUserStats.SetAchievement(id);
                SteamUserStats.StoreStats();
            });

        // Players start Steam before the first scene; the Editor and the tests never do.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Startup()
        {
#if !UNITY_EDITOR
            TryInstall();
#endif
        }
    }

    sealed class SteamworksRemoteStorage : IRemoteStorageApi
    {
        public bool IsSteamRunning => SteamAPI.IsSteamRunning();
        public bool IsCloudEnabledForAccount => SteamRemoteStorage.IsCloudEnabledForAccount();
        public bool IsCloudEnabledForApp => SteamRemoteStorage.IsCloudEnabledForApp();
        public bool FileExists(string name) => SteamRemoteStorage.FileExists(name);
        public int GetFileSize(string name) => SteamRemoteStorage.GetFileSize(name);
        public int FileRead(string name, byte[] buffer, int size) => SteamRemoteStorage.FileRead(name, buffer, size);
        public bool FileWrite(string name, byte[] data, int size) => SteamRemoteStorage.FileWrite(name, data, size);
        public bool FileDelete(string name) => SteamRemoteStorage.FileDelete(name);
        public long GetFileTimestamp(string name) => SteamRemoteStorage.GetFileTimestamp(name);

        public bool GetQuota(out long totalBytes, out long availableBytes)
        {
            if (SteamRemoteStorage.GetQuota(out var total, out var available)) { totalBytes = (long)total; availableBytes = (long)available; return true; }
            totalBytes = availableBytes = 0;
            return false;
        }
    }
}
#endif
