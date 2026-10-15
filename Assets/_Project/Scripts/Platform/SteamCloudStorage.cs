using System;
using System.Text;

namespace Farm.Platform
{
    // Steam Remote Storage behind ICloudStorage, over a small seam (IRemoteStorageApi). Real Steam behavior is not verified yet (docs/STATUS.md).
    public sealed class SteamCloudStorage : ICloudStorage
    {
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        readonly IRemoteStorageApi _api;

        public SteamCloudStorage(IRemoteStorageApi api) { _api = api; }

        public CloudFailure LastFailure { get; private set; }

        public bool IsAvailable
        {
            get
            {
                try { return _api.IsSteamRunning && _api.IsCloudEnabledForAccount && _api.IsCloudEnabledForApp; }
                catch (Exception) { return false; }
            }
        }

        public bool Exists(string name)
        {
            try { return _api.FileExists(name); }
            catch (Exception) { return false; }
        }

        public DateTime ModifiedUtc(string name)
        {
            try
            {
                if (!_api.FileExists(name)) return DateTime.MinValue;
                return DateTimeOffset.FromUnixTimeSeconds(_api.GetFileTimestamp(name)).UtcDateTime;
            }
            catch (Exception) { return DateTime.MinValue; }
        }

        public bool TryRead(string name, out string text)
        {
            text = null;
            try
            {
                if (!_api.FileExists(name)) return false;
                var size = _api.GetFileSize(name);
                if (size < 0) return false;
                var buffer = new byte[size];
                if (_api.FileRead(name, buffer, size) != size) return false;
                text = Utf8.GetString(buffer);
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool TryWrite(string name, string text)
        {
            try
            {
                if (!IsAvailable) { LastFailure = CloudFailure.Unavailable; return false; }
                var bytes = Utf8.GetBytes(text);
                if (_api.FileWrite(name, bytes, bytes.Length)) { LastFailure = CloudFailure.None; return true; }
                // Steam only says "no": a full quota is the one reason worth telling the player about.
                LastFailure = _api.GetQuota(out _, out var available) && available < bytes.Length ? CloudFailure.QuotaExceeded : CloudFailure.WriteFailed;
                return false;
            }
            catch (Exception) { LastFailure = CloudFailure.WriteFailed; return false; }
        }

        public bool TryDelete(string name)
        {
            try { return !_api.FileExists(name) || _api.FileDelete(name); }
            catch (Exception) { return false; }
        }
    }
}
