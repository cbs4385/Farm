namespace Farm.Platform
{
    // What the game needs from a store or launcher. The game runs without Steam: the default is NullPlatform, and the
    // Steam implementation (compiled only with FARM_STEAM and the Steamworks.NET package) replaces it at startup.
    public interface IPlatformServices
    {
        string Name { get; }
        bool IsOnline { get; }
        // Unlocks an achievement by its stable id. Safe to call repeatedly.
        void UnlockAchievement(string id);
    }

    public sealed class NullPlatform : IPlatformServices
    {
        public string Name => "none";
        public bool IsOnline => false;
        public void UnlockAchievement(string id) { }
    }

    public static class PlatformServices
    {
        static IPlatformServices _current = new NullPlatform();
        public static IPlatformServices Current => _current;
        public static void Use(IPlatformServices services) => _current = services ?? new NullPlatform();
        public static void Reset() => _current = new NullPlatform();
    }
}
