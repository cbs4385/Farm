namespace Farm.Platform
{
    // The few Steam Remote Storage calls the game uses, so SteamCloudStorage can be tested without Steam (the real
    // implementation wraps Steamworks.NET and exists only in builds with FARM_STEAM). Names are relative paths with forward slashes.
    public interface IRemoteStorageApi
    {
        bool IsSteamRunning { get; }
        bool IsCloudEnabledForAccount { get; }
        bool IsCloudEnabledForApp { get; }
        bool FileExists(string name);
        int GetFileSize(string name);
        int FileRead(string name, byte[] buffer, int size);
        bool FileWrite(string name, byte[] data, int size);
        bool FileDelete(string name);
        long GetFileTimestamp(string name);                  // Unix seconds
        bool GetQuota(out long totalBytes, out long availableBytes);
    }
}
