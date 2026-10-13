using Farm.Core;

namespace Farm.Gameplay
{
    // The first time the player walks into a building that has a shop counter, one line says what to do there (playtest 2026-10-13). It is one of the tips for the first days:
    // it is not shown when they are switched off, and never again after it has been shown once.
    public static class ShopHint
    {
        public const string Flag = "tutorial.shop_hint";

        public static bool Wanted(GameSession s, string mapId, bool hasCounter)
        {
            if (s == null || !s.InGame || !hasCounter || s.HasFlag(Flag) || !MapIds.IsInterior(mapId)) return false;
            return s.Settings == null || s.Settings.TutorialTips;
        }

        public static void ShowIfWanted(GameSession s, string mapId, bool hasCounter)
        {
            if (!Wanted(s, mapId, hasCounter)) return;
            s.SetFlag(Flag);
            s.Toast(L.Get("tutorial.shop_hint"));
        }
    }
}
