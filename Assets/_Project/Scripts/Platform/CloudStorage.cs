using System;

namespace Farm.Platform
{
    // A store's cloud file storage (Steam Remote Storage). Names are relative paths with forward slashes. Every call is
    // failure-tolerant: false/MinValue means "did not work", never an exception, so the game falls back to the local copy.
    public interface ICloudStorage
    {
        // False when the store's cloud is switched off or unreachable for this player; the game then behaves as without cloud.
        bool IsAvailable { get; }
        bool Exists(string name);
        DateTime ModifiedUtc(string name);               // DateTime.MinValue when missing
        bool TryRead(string name, out string text);
        bool TryWrite(string name, string text);
        bool TryDelete(string name);
    }
}
