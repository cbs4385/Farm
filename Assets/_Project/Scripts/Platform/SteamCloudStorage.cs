#if FARM_STEAM
using System;
using System.Text;
using Steamworks;

namespace Farm.Platform
{
    // Steam Remote Storage behind ICloudStorage. Not verified against a live Steam client yet (docs/STATUS.md).
    public sealed class SteamCloudStorage : ICloudStorage
    {
        public bool IsAvailable
        {
            get
            {
                try { return SteamAPI.IsSteamRunning() && SteamRemoteStorage.IsCloudEnabledForAccount() && SteamRemoteStorage.IsCloudEnabledForApp(); }
                catch (Exception) { return false; }
            }
        }

        public bool Exists(string name)
        {
            try { return SteamRemoteStorage.FileExists(name); }
            catch (Exception) { return false; }
        }

        public DateTime ModifiedUtc(string name)
        {
            try
            {
                if (!SteamRemoteStorage.FileExists(name)) return DateTime.MinValue;
                return DateTimeOffset.FromUnixTimeSeconds(SteamRemoteStorage.GetFileTimestamp(name)).UtcDateTime;
            }
            catch (Exception) { return DateTime.MinValue; }
        }

        public bool TryRead(string name, out string text)
        {
            text = null;
            try
            {
                if (!SteamRemoteStorage.FileExists(name)) return false;
                var size = SteamRemoteStorage.GetFileSize(name);
                var buffer = new byte[size];
                var read = SteamRemoteStorage.FileRead(name, buffer, size);
                if (read != size) return false;
                text = new UTF8Encoding(false).GetString(buffer);
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool TryWrite(string name, string text)
        {
            try
            {
                var bytes = new UTF8Encoding(false).GetBytes(text);
                return SteamRemoteStorage.FileWrite(name, bytes, bytes.Length);
            }
            catch (Exception) { return false; }
        }

        public bool TryDelete(string name)
        {
            try { return !SteamRemoteStorage.FileExists(name) || SteamRemoteStorage.FileDelete(name); }
            catch (Exception) { return false; }
        }
    }
}
#endif
