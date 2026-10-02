namespace Farm.Mythos
{
    // Reserved identifiers for the planned cosmic-horror layer: a secretive cult in the village serving an Elder God
    // that dwells in the neighbouring woods. Nothing here is used by the base game. The names exist so scenes, data
    // and saves can refer to them consistently; they can be renamed freely until content ships.
    // See docs/adr/0002-mythos-extension-points.md and docs/01-GameDesign.md section 3.12.
    public static class MythosIds
    {
        public const string ModuleId = "mythos";

        // Story variables (GameSession.GetVar / SetVar, and "var:<name>>=n" in conditions)
        public static class Vars
        {
            // The Elder God's wakefulness in permille (0..1000); 1000 = it wakes. See WakefulnessModel.
            public const string Wakefulness = "mythos.wakefulness";
            public const string Dread = "dread";               // the player's creeping unease (0..100)
            public const string Lore = "lore";                 // forbidden knowledge gathered
            public const string CultStanding = "cult.standing"; // how the cult regards the player
            public const string Offerings = "cult.offerings";
        }

        // Story flags (GameSession.SetFlag / HasFlag, and "flag:<name>" in conditions)
        public static class Flags
        {
            public const string CultKnown = "mythos.cult_known";
            public const string CultRevealed = "mythos.cult_revealed";
            public const string WoodsOpen = Farm.Gameplay.MapIds.WoodsOpenFlag;   // the Forest gate (core)
            public const string Initiated = "mythos.initiated";
        }

        // Weather ids a weather modifier may return (each needs a "weather.<id>" string)
        public static class Weather
        {
            public const string Fog = "fog";
            public const string BloodMoon = "bloodmoon";
        }

        // Atmosphere layer ids (AtmosphereStack.Set)
        public static class Atmosphere
        {
            public const string Dread = "mythos.dread";
            public const string Woods = "mythos.woods";
        }

        // Maps the layer is expected to add (scenes named after the map id)
        public static class Maps
        {
            public const string Woods = Farm.Gameplay.MapIds.Woods;                  // scene shipped with the layer
            public const string CultHall = "CultHall";
        }
    }
}
