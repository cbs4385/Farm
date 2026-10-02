using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Reacting to a map (ADR 0002 map hook): the layer's mood over the light everywhere, a little extra in the woods.
    public static class MythosMaps
    {
        public static void OnMapLoaded(MapLoadedContext ctx)
        {
            var s = ctx.Session;
            WakefulnessService.ApplyAtmosphere(s);
            if (!ServiceLocator.TryGet<AtmosphereService>(out var atmosphere)) return;
            if (MythosLevel.On(s) && ctx.MapId == MapIds.Woods)
                atmosphere.Stack.Set(MythosIds.Atmosphere.Woods, new Color(0.7f, 0.85f, 0.8f), 0.35f * MythosLevel.Scale(s), 3);
            else atmosphere.Stack.Remove(MythosIds.Atmosphere.Woods);
        }
    }
}
