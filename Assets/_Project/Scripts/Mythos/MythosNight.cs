using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Night events of a restless god (X-005): blight in the fields and sleepwalking. Both are day-cycle effects that scale with the intensity
    // setting (mild at half strength, off never) and never touch what a scarecrow or a quieted god protects.
    public static class MythosBlight
    {
        public const int FirstStep = 8;                    // the god is restless enough
        public const float MaxChance = 0.06f;              // per mature-or-growing crop per night at the last step, full intensity
        public const int MaxPerNight = 6;

        // Chance one unprotected crop withers tonight. (pure)
        public static float Chance(int step, float scale) =>
            step < FirstStep ? 0f : Mathf.Clamp01((step - FirstStep + 1) / (float)(WakefulnessModel.StepCount - FirstStep + 1)) * MaxChance * scale;

        // A ritual that worked this season keeps the fields safe until the next one.
        public static bool SeasonQuieted(GameSession s, MythosSave save, GameDateTime now) =>
            save.Sealed || save.LastSucceeded && save.ResolvedSeason == RitualDirector.SeasonIndex(now);

        // Withers crops on the farm's grids. Returns how many died. Runs before the night's growth.
        public static int Strike(GameSession s, int today)
        {
            var scale = MythosLevel.Scale(s);
            if (scale <= 0f) return 0;
            var chance = Chance(s.GetVar(MythosIds2.Step), scale);
            if (chance <= 0f) return 0;
            if (SeasonQuieted(s, RitualDirector.Load(s), s.Clock.Now)) return 0;
            var withered = 0;
            foreach (var pair in s.Grids.OrderBy(p => p.Key, System.StringComparer.Ordinal))
            {
                if (pair.Key == MapIds.Greenhouse) continue;                   // indoors, kept dry and warm
                var objects = s.GetObjects(pair.Key).All;
                foreach (var tile in pair.Value.Tiles.OrderBy(t => t.X).ThenBy(t => t.Y))
                {
                    if (withered >= MaxPerNight) return withered;
                    var crop = tile.Crop;
                    if (crop == null || crop.Withered || crop.CropId.StartsWith("mutant_") || !s.Db.TryGetCrop(crop.CropId, out var def) || def.IsTree) continue;
                    if (!string.IsNullOrEmpty(def.GrowCondition)) continue;     // the horror crops thrive on it
                    if (Sprinklers.Protected(objects, s.Placeables, tile.X, tile.Y)) continue;      // a scarecrow keeps the ward
                    if (WeatherRoller.Unit(today * 131 + tile.X * 11 + tile.Y * 17, s.State.WorldSeed ^ 0x5EED) >= chance) continue;
                    if (pair.Value.Wither(tile.X, tile.Y)) withered++;
                }
            }
            return withered;
        }
    }

    public static class MythosSleepwalk
    {
        public const string SpawnName = "sleepwalk";
        public const string LastDayVar = "mythos.sleepwalk.day";     // stored as day + 1 so that 0 means never
        public const int MinDaysApart = 7;
        public const int EnergyLoss = 15;

        // Where a sleepwalker can end up: anywhere outdoors, the wood too once it is open.
        public static readonly string[] Maps = { MapIds.Farm, MapIds.Village, MapIds.Forest, MapIds.Beach, MapIds.Woods };

        public static float Chance(int step, int dread, float scale) =>
            scale <= 0f || step < 3 && dread < 25 ? 0f : Mathf.Min(0.2f, 0.03f + step * 0.006f + dread / 100f * 0.08f) * scale;

        public static bool Rested(int lastDayPlusOne, int today) => lastDayPlusOne == 0 || today - (lastDayPlusOne - 1) >= MinDaysApart;

        // The map the sleepwalker wakes on, chosen from those allowed (the wood needs its gate open). (pure)
        public static string Destination(float roll, bool woodsOpen)
        {
            var options = Maps.Where(m => m != MapIds.Woods || woodsOpen).ToArray();
            return options[System.Math.Min(options.Length - 1, (int)(roll * options.Length))];
        }

        // At dawn: may move where the player wakes. Returns true when they sleepwalked.
        public static bool Check(GameSession s, DayCycleContext context)
        {
            var scale = MythosLevel.Scale(s);
            if (scale <= 0f || context.PassedOut) return false;
            var today = context.Clock.Now.TotalDays;
            if (!Rested(s.GetVar(LastDayVar), today)) return false;
            var chance = Chance(s.GetVar(MythosIds2.Step), s.GetVar(MythosIds.Vars.Dread), scale);
            if (chance <= 0f || WeatherRoller.Unit(today * 37 + 5, s.State.WorldSeed ^ 0x51EE9) >= chance) return false;

            var map = Destination(WeatherRoller.Unit(today * 41 + 9, s.State.WorldSeed ^ 0x6A11), s.HasFlag(MapIds.WoodsOpenFlag));
            context.WakeMap = map;
            context.WakeSpawn = SpawnName;
            context.State.Energy = System.Math.Max(1, context.State.Energy - EnergyLoss);
            s.SetVar(LastDayVar, today + 1);
            s.AddVar(MythosIds.Vars.Lore, 1, 0, 100);
            var variant = (int)(WeatherRoller.Unit(today * 43 + 1, s.State.WorldSeed ^ 0x7777) * 3) % 3;
            context.Note("mythos.sleepwalk." + variant);
            return true;
        }
    }
}
