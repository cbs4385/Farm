using System;
using System.Text;
using Farm.Core;

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

        // The cloud switches, as one line for the log: which of them is off is the first thing to know when a save does not go up.
        string Flags()
        {
            try { return $"Steam running {_api.IsSteamRunning}, cloud on for the account {_api.IsCloudEnabledForAccount}, for the app {_api.IsCloudEnabledForApp}"; }
            catch (Exception e) { return "the cloud switches could not be read (" + e.Message + ")"; }
        }

        string QuotaText()
        {
            try { return _api.GetQuota(out var total, out var available) ? $"quota {total} bytes, {available} free" : "quota unknown (Steam did not answer)"; }
            catch (Exception e) { return "quota unknown (" + e.Message + ")"; }
        }

        public bool TryRead(string name, out string text)
        {
            text = null;
            try
            {
                if (!_api.FileExists(name)) return false;
                var size = _api.GetFileSize(name);
                if (size < 0) { Log.Warn($"[SteamCloud] '{name}' is in the cloud but its size could not be read ({size})."); return false; }
                var buffer = new byte[size];
                var got = _api.FileRead(name, buffer, size);
                if (got != size) { Log.Warn($"[SteamCloud] '{name}' read {got} of {size} bytes."); return false; }
                text = Utf8.GetString(buffer);
                return true;
            }
            catch (Exception e) { Log.Warn($"[SteamCloud] reading '{name}' failed: {e.Message}"); return false; }
        }

        public bool TryWrite(string name, string text)
        {
            try
            {
                if (!IsAvailable) { LastFailure = CloudFailure.Unavailable; Log.Warn($"[SteamCloud] '{name}' not written: {Flags()}."); return false; }
                var bytes = Utf8.GetBytes(text);
                if (_api.FileWrite(name, bytes, bytes.Length)) { LastFailure = CloudFailure.None; return true; }
                // Steam only says "no": a full quota is the one reason worth telling the player about.
                LastFailure = _api.GetQuota(out _, out var available) && available < bytes.Length ? CloudFailure.QuotaExceeded : CloudFailure.WriteFailed;
                Log.Warn($"[SteamCloud] writing '{name}' ({bytes.Length} bytes) was refused ({LastFailure}): {Flags()}; {QuotaText()}.");
                return false;
            }
            catch (Exception e) { LastFailure = CloudFailure.WriteFailed; Log.Warn($"[SteamCloud] writing '{name}' failed: {e.Message}"); return false; }
        }

        public bool TryDelete(string name)
        {
            try { return !_api.FileExists(name) || _api.FileDelete(name); }
            catch (Exception) { return false; }
        }
    }
}
