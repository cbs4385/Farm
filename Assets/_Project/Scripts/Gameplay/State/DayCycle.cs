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
    }

    public static class WeatherIds
    {
        public const string Sunny = "sunny";
        public const string Rain = "rain";
    }

    // Everything that happens between one morning and the next. Pure logic so it can be tested without a scene.
    public static class DayCycle
    {
        public const string BedSpawn = "bed";
        static readonly float[] QualityMultiplier = { 1f, 1.25f, 1.5f, 2f };

        // Deterministic weather: same date always gives the same weather. M1 only has sunny/rain; T-030 extends it.
        public static string RollWeather(GameDateTime date)
        {
            if (date.TotalDays < 2 || date.Season == Season.Winter) return WeatherIds.Sunny;
            var h = (uint)(date.TotalDays * 2654435761u);
            return (h >> 16) % 100 < 20 ? WeatherIds.Rain : WeatherIds.Sunny;
        }

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
            GameHooks hooks = null)
        {
            var summary = new DaySummary { PassedOut = passedOut };
            var context = new DayCycleContext
            {
                State = state, Clock = clock, Grids = grids, Items = itemLookup, Crops = cropLookup,
                Summary = summary, PassedOut = passedOut, World = new StateWorldQuery(state, clock),
                WakeMap = MapIds.FarmHouse, WakeSpawn = BedSpawn,
            };

            // 1. Shipping bin pays out.
            foreach (var stack in state.ShippingBin)
            {
                var item = itemLookup(stack.ItemId);
                if (item == null) continue;
                summary.Earnings += SellValue(item, stack.Quality, stack.Count);
                summary.Shipped.Add(stack.Clone());
            }
            state.ShippingBin.Clear();
            state.Gold += summary.Earnings;

            // 1b. Night hooks: dreams, blight, offerings...
            hooks?.RunNightFalls(context);

            // 2. Crops grow (or die in the new season) using today's weather.
            var rainedToday = state.Weather == WeatherIds.Rain;
            var newSeason = clock.Now.StartOfNextDay().Season;
            foreach (var grid in grids.Values)
                summary.CropsDied += grid.AdvanceDay(newSeason, rainedToday, cropLookup);

            // 3. Calendar moves to 06:00 tomorrow (publishes DayEnded / SeasonChanged / DayStarted).
            clock.StartNextDay(passedOut);
            summary.NewDate = clock.Now;

            // 4. Tomorrow's weather.
            state.Weather = RollWeather(clock.Now);
            if (hooks != null) state.Weather = hooks.ApplyWeather(clock.Now, state.Weather, state);
            summary.NewWeather = state.Weather;
            if (state.Weather == WeatherIds.Rain)
                foreach (var grid in grids.Values) grid.WaterAll();

            // 5. Rest.
            if (passedOut)
            {
                summary.PassOutGoldLoss = Math.Min(state.Gold / 20, 500);
                state.Gold -= summary.PassOutGoldLoss;
                state.Energy = (int)(state.MaxEnergy * 0.75f);
            }
            else
            {
                state.Energy = state.MaxEnergy;
            }
            state.Health = state.MaxHealth;

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
