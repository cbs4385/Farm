using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public sealed class DaySummary
    {
        public List<ItemStack> Shipped = new List<ItemStack>();
        public int Earnings;
        public int GoldAfter;
        public int PassOutGoldLoss;
        public bool PassedOut;
        public int CropsDied;
        public List<SummaryNote> Notes = new List<SummaryNote>();   // extra lines added by day-cycle hooks
        public GameDateTime NewDate;
        public string NewWeather;
        public float FatigueAtSleep;      // 0..1, how tired the player was when they fell asleep
    }

    public static class WeatherIds
    {
        public const string Sunny = WeatherDefaults.Sunny;
        public const string Rain = WeatherDefaults.Rain;
        public const string Storm = WeatherDefaults.Storm;
        public const string Snow = WeatherDefaults.Snow;
        public const string Wind = WeatherDefaults.Wind;
    }

    // Everything that happens between one morning and the next. Pure logic so it can be tested without a scene.
    public static class DayCycle
    {
        public const string BedSpawn = "bed";
        static readonly float[] QualityMultiplier = { 1f, 1.25f, 1.5f, 2f };

        // The base roll for a date from the built-in weather table (seed 0, no modules). The game itself rolls
        // through WeatherRoller with the save's seed, the loaded weather data and the module hooks.
        public static string RollWeather(GameDateTime date) => WeatherRoller.Roll(date, WeatherCatalog.BuiltIn, 0);

        public static int SellValue(ItemDefinition item, int quality, int count)
        {
            var mult = QualityMultiplier[Math.Max(0, Math.Min(quality, QualityMultiplier.Length - 1))];
            return (int)(item.SellPrice * mult) * count;
        }

        public static DaySummary EndDay(
            GameState state,
            GameClock clock,
            IDictionary<string, FarmGrid> grids,
            Func<string, ItemDefinition> itemLookup,
            Func<string, CropDefinition> cropLookup,
            bool passedOut,
            GameHooks hooks = null,
            WeatherCatalog weather = null,
            PlaceableCatalog placeables = null)
        {
            weather = weather ?? WeatherCatalog.BuiltIn;
            var summary = new DaySummary { PassedOut = passedOut };
            var context = new DayCycleContext
            {
                State = state, Clock = clock, Grids = grids, Items = itemLookup, Crops = cropLookup,
                Summary = summary, PassedOut = passedOut, World = new StateWorldQuery(state, clock),
                WakeMap = MapIds.FarmHouse, WakeSpawn = BedSpawn,
            };

            var sleepMinute = clock.Now.MinuteOfDay;
            summary.FatigueAtSleep = new FatigueState(state.FatigueCarried).RecoveryRating(sleepMinute);

            // 1. Shipping bin pays out.
            foreach (var stack in state.ShippingBin)
            {
                var item = itemLookup(stack.ItemId);
                if (item == null) continue;
                var value = (int)(SellValue(item, stack.Quality, stack.Count) * Professions.SellMultiplier(state, item));
                summary.Earnings += value;
                state.ShippedTotals[stack.ItemId] = (state.ShippedTotals.TryGetValue(stack.ItemId, out var shipped) ? shipped : 0) + stack.Count;
                summary.Shipped.Add(stack.Clone());
            }
            state.ShippingBin.Clear();
            state.Gold += summary.Earnings;
            state.TotalEarned += summary.Earnings;

            // 1b. Night hooks: dreams, blight, offerings...
            hooks?.RunNightFalls(context);

            // 1c. Sprinklers water their tiles.
            placeables = placeables ?? PlaceableCatalog.BuiltIn;
            foreach (var kv in grids)
                if (state.Maps.TryGetValue(kv.Key, out var mapState)) Sprinklers.WaterAll(mapState.Objects, placeables, kv.Value);

            // 2. Crops grow (or die in the new season) using today's weather. The greenhouse stays dry in the rain.
            var rainedToday = weather.Get(state.Weather).WateringCrops;
            var newSeason = clock.Now.StartOfNextDay().Season;
            foreach (var kv in grids)
                summary.CropsDied += kv.Value.AdvanceDay(newSeason, rainedToday && kv.Key != MapIds.Greenhouse, cropLookup, context.World);

            // 3. Calendar moves to 06:00 tomorrow (publishes DayEnded / SeasonChanged / DayStarted).
            clock.StartNextDay(passedOut);
            summary.NewDate = clock.Now;
            NpcInteractions.NewDay(state, clock.Now, hooks);
            AnimalRules.NewDay(state);
            if (CatBond.NewDay(state, clock.Now.TotalDays) == CatBond.DayResult.Left) summary.Notes.Add(new SummaryNote("summary.cat_left", System.Array.Empty<object>()));

            // 4. The new day's weather is the forecast made yesterday (rolled now if there is none, e.g. after a date
            // jump), which modules may still override. Then the next day is forecast.
            var rolled = !string.IsNullOrEmpty(state.ForecastWeather)
                ? state.ForecastWeather
                : WeatherRoller.Roll(clock.Now, weather, state.WorldSeed, hooks, state);
            state.Weather = hooks != null ? hooks.ApplyWeather(clock.Now, rolled, state) : rolled;
            state.ForecastWeather = WeatherRoller.Roll(clock.Now.StartOfNextDay(), weather, state.WorldSeed, hooks, state);
            summary.NewWeather = state.Weather;
            var rainsToday = weather.Get(state.Weather).WateringCrops;
            foreach (var kv in grids)
            {
                kv.Value.RainsToday = rainsToday && kv.Key != MapIds.Greenhouse;
                if (kv.Value.RainsToday) kv.Value.WaterAll();
            }

            // 5. Rest.
            // Sleeping restores energy (full in bed, 75% after collapsing), less the more tired the player was when they
            // fell asleep: nothing after a whole night awake. A bed sleep clears fatigue; a collapse carries it over.
            var fatigue = new FatigueState(state.FatigueCarried);
            if (passedOut)
            {
                summary.PassOutGoldLoss = Math.Min(state.Gold / 20, 500);
                state.Gold -= summary.PassOutGoldLoss;
            }
            var energyTarget = passedOut ? (int)(state.MaxEnergy * 0.75f) : state.MaxEnergy;
            state.Energy = fatigue.EnergyAfterSleep(state.Energy, energyTarget, sleepMinute);
            fatigue.AfterSleep(!passedOut, sleepMinute);
            state.FatigueCarried = fatigue.Carried;
            state.Health = state.MaxHealth;
            state.Mine.Floor = 0;

            foreach (var ready in state.PendingUpgrades)
                if (ready.ReadyDay == clock.Now.TotalDays && itemLookup(ready.ToolItemId) is ItemDefinition tool)
                    summary.Notes.Add(new SummaryNote("summary.upgrade_ready",
                        new object[] { L.Get("upgrade.tool", L.Get(ToolModel.TierKey(ready.Tier)), L.Get(tool.NameKey)) }));

            if (passedOut) summary.Notes.Add(new SummaryNote("summary.collapsed", new object[0]));
            else if (summary.FatigueAtSleep >= 0.05f) summary.Notes.Add(new SummaryNote("summary.late_night", new object[0]));

            // 5b. Dawn hooks may add summary notes or change where the player wakes up.
            hooks?.RunDawn(context);

            // 6. Wake up (in bed unless a hook moved the player).
            state.CurrentMap = context.WakeMap;
            state.SpawnPoint = context.WakeSpawn;
            state.SetDate(clock.Now);

            summary.GoldAfter = state.Gold;
            return summary;
        }
    }
}
