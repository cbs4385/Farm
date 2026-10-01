using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Entry point of the optional horror layer. Today it only registers its string table; it adds no hooks, so the
    // base game plays unchanged. To add behaviour, register hooks in Initialize (always after checking
    // context.HorrorLevel), for example:
    //
    //   context.Hooks.AddDayCycleHook(new DreamHook());            // overnight dreams, blight, offerings
    //   context.Hooks.AddWeatherModifier(new FogWeather());        // "fog" / "bloodmoon" weather
    //   context.Hooks.MapLoaded += OnMapLoaded;                    // spawn objects, push atmosphere layers
    //   context.Hooks.AddHudWidget(() => new DreadMeter());        // HUD element
    //   L.AddFilter(DreadTextCorruption.Apply);                    // distort text at high dread
    //   Conditions.Register("cult", (arg, world) => ...);          // new condition atoms for data
    //   context.Session.GetModuleData<MythosSave>(ModuleId);       // private persistent state
    //
    // Extra items/crops go in a ContentPack asset under Resources/Packs; extra maps are scenes named by map id.
    public sealed class MythosModule : IGameModule
    {
        public string Id => MythosIds.ModuleId;

        // English names for the reserved weather ids so they display properly the day a modifier returns them.
        public static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { "weather." + MythosIds.Weather.Fog, "Fog" },
            { "weather." + MythosIds.Weather.BloodMoon, "Blood Moon" },
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() => GameModules.Register(new MythosModule());

        public void Initialize(ModuleContext context)
        {
            L.AddTable(L.DefaultLanguage, Strings);
            if (context.HorrorLevel == 0) return;   // the player turned the layer off: stay inert
        }
    }
}
