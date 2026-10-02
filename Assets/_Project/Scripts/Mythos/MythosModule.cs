using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The optional horror layer ("mythos"): a secretive cult, a sleeping elder god, dread. It lives entirely in this assembly
    // and in data, and plugs in through the generic hooks of ADR 0002. It reads the intensity setting live (Options), so
    // at 0 every hook, atom and effect is inert and the game is the plain farming sim.
    public sealed class MythosModule : IGameModule
    {
        public const string ResourceFolder = "Mythos";

        public string Id => MythosIds.ModuleId;

        // English names for the reserved ids, plus the layer's own strings (added with the story data).
        public static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { "weather." + MythosIds.Weather.Fog, "Fog" },
            { "weather." + MythosIds.Weather.BloodMoon, "Blood Moon" },
            { "mythos.god.name", "Nharoth" },
            { "mythos.cult.name", "Keepers of the Covenant" },
            { "mythos.woods.name", "Harrow Wood" },
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() => GameModules.Register(new MythosModule());

        public void Initialize(ModuleContext context)
        {
            L.AddTable(L.DefaultLanguage, Strings);
            if (context.Session == null) return;
            Install(context.Session, context.Hooks, context.Bus);
        }

        // Registers everything. Safe to call with a fresh session in tests. Every piece checks the intensity itself.
        public static void Install(GameSession session, GameHooks hooks, EventBus bus)
        {
            RegisterConditions();
            MythosEffects.Register();
            MythosContent.Load(session.Story);

            hooks.AddDayCycleHook(new MythosDayHook(session));
            hooks.AddLuckModifier(new DreadLuckModifier(session));
            hooks.AddWeatherWeightModifier(new DreadWeatherWeights(session));
            hooks.AddWeatherModifier(new BloodMoonWeather(session));
            hooks.AddEventWeightModifier(new DreadEventWeights(session));
            hooks.AddFriendshipDecayModifier(new DreadFriendshipDecay(session));
            hooks.AddScheduleSource(new MythosScheduleSource());
            hooks.AddJournalPage(new MythosJournalPage(session));
            hooks.MapLoaded += ctx => MythosMaps.OnMapLoaded(ctx);
            if (bus != null) bus.Subscribe<MinuteChanged>(e => OnMinute(session, e));
            if (bus != null) bus.Subscribe<WakefulnessStepChanged>(e => MythosEnding.OnStep(session, e));
        }

        static void OnMinute(GameSession s, MinuteChanged e)
        {
            if (!s.InGame || !MythosLevel.On(s)) return;
            RitualDirector.Progress(s, e.Now.MinuteOfDay);
        }

        // `horror:<n>` (the intensity is at least n), `keeper:<npc>` (a Keeper, while the cult still keeps its watch).
        public static void RegisterConditions()
        {
            Conditions.Register("horror", (arg, w) => int.TryParse(arg, out var n) && GameSession.CurrentHorrorLevel >= n);
            Conditions.Register("keeper", (arg, w) => GameSession.CurrentHorrorLevel >= 1 && MythosCast.IsKeeper(arg));
        }
    }
}
