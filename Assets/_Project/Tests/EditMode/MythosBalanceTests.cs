using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // X-010 balance: bots that play the horror layer for years through the real session, hooks and ritual director, to check what the design
    // promises: a player who ignores the layer is never destroyed by it, one who fights the Keepers has a long window to find the true fix,
    // and wakefulness can be brought back down by someone who stops. The numbers are also written to Builds/balance/mythos.md
    // (copy them to docs/balance/mythos.md when they change).
    public class MythosBalanceTests
    {
        static GameDatabase RealDb() => Resources.Load<GameDatabase>(GameDatabase.ResourcePath);

        sealed class Run
        {
            public string Name;
            public int Level, Seed;
            public int Days;
            public int AwakenedOnDay = -1;                        // day number of the run (1-based) the god woke, or -1
            public int Successes, Failures, Disrupted;
            public int MaxWakefulness;
            public int SealedOnDay = -1;
            public int WakefulnessAtSeal;
            public int WoodsOpenDay = -1;
            public readonly List<int> SeasonEnds = new List<int>();   // wakefulness at the end of each season
            public int FinalWakefulness;
        }

        [TearDown]
        public void TearDown() => GameSession.HorrorLevelOverride = null;

        // sabotage(ritualCount): does the bot take the first offering from the altar at ritual number `ritualCount` (1-based, counting from the first
        // one the player may interfere with)? sealAfterWoodsDays: the bot completes the true fix this many days after the woods open.
        static Run Play(string name, int level, int years, Func<int, bool> sabotage = null, int? sealAfterWoodsDays = null, int seed = 1, Action<GameSession, int> daily = null)
        {
            var run = new Run { Name = name, Level = level, Seed = seed };
            GameSession.HorrorLevelOverride = level;
            using (var f = new TestSessionFixture(RealDb().AllItems, RealDb().AllCrops, NpcDefaults.CreateAll()))
            {
                var s = f.Session;
                s.State.WorldSeed = seed * 7919 + 11;                                // the same world every time for a given seed
                s.Story = StoryContent.LoadFromResources();
                foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder)) s.Db.Merge(pack);
                MythosModule.Install(s, s.Hooks, f.Bus);
                f.Bus.Subscribe<RitualResolved>(r => { if (r.Succeeded) run.Successes++; else run.Failures++; });
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;       // the awakening scene is story data that needs the whole UI
                try
                {
                    var interfered = 0;
                    for (var d = 1; d <= years * 4 * 28; d++)
                    {
                        run.Days = d;
                        daily?.Invoke(s, d);
                        var now = s.Clock.Now;
                        if (RitualDirector.IsRitualDay(now))
                        {
                            s.Clock.SetTime(new GameDateTime(now.Year, now.Season, now.Day, RitualDirector.StartMinute - 10));
                            var acted = false;
                            for (var m = 0; m < 30 && !s.Clock.Now.IsDayOver; m++)
                            {
                                s.Clock.AdvanceMinutes(10);
                                if (!acted && sabotage != null && s.HasFlag(MythosIds2.Interference) && s.Clock.Now.MinuteOfDay >= RitualDirector.StartMinute + RitualModel.LeaderMinutes + 5)
                                {
                                    acted = true;
                                    interfered++;
                                    if (!sabotage(interfered)) continue;
                                    var taken = RitualDirector.TakeFromAltar(s);
                                    if (taken == null) continue;
                                    run.Disrupted++;
                                    s.Backpack.Remove(taken.ItemId, 1);                 // a player eats, sells or uses what was taken
                                }
                            }
                        }
                        s.EndDay(false);

                        var wake = s.GetVar(MythosIds.Vars.Wakefulness);
                        run.MaxWakefulness = Math.Max(run.MaxWakefulness, wake);
                        if (run.WoodsOpenDay < 0 && s.HasFlag(MapIds.WoodsOpenFlag)) run.WoodsOpenDay = d;
                        if (sealAfterWoodsDays.HasValue && run.SealedOnDay < 0 && run.WoodsOpenDay >= 0 && d >= run.WoodsOpenDay + sealAfterWoodsDays.Value)
                        {
                            run.WakefulnessAtSeal = wake;
                            MythosEnding.Seal(s);
                            run.SealedOnDay = d;
                        }
                        if (s.Clock.Now.Day == 1) run.SeasonEnds.Add(wake);
                        if (wake >= WakefulnessModel.Max && run.AwakenedOnDay < 0) { run.AwakenedOnDay = d; break; }
                    }
                    run.FinalWakefulness = s.GetVar(MythosIds.Vars.Wakefulness);
                }
                finally { UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false; }
            }
            return run;
        }

        // Wakefulness is reset to nothing by every successful ritual (30-40% off a god that has risen 25% at most), so the dice change little: two worlds are enough.
        static readonly int[] Seeds = { 1, 2 };

        static string Row(Run r) =>
            $"| {r.Name} | {(r.Level == 2 ? "full" : "mild")} | {r.Seed} | {r.Days} | {r.Successes} / {r.Failures} / {r.Disrupted} | {r.MaxWakefulness / 10f:0.#}% | " +
            $"{(r.AwakenedOnDay < 0 ? "never" : "day " + r.AwakenedOnDay)} | {(r.SealedOnDay < 0 ? "-" : "day " + r.SealedOnDay + " at " + r.WakefulnessAtSeal / 10f + "%")} | {r.FinalWakefulness / 10f:0.#}% |";

        static readonly List<Run> Runs = new List<Run>();

        // Plays the same bot in a few different worlds (the seed changes the offerings and the dice) and keeps every run for the report.
        static List<Run> PlayAll(string name, int level, int years, Func<int, bool> sabotage = null, int? sealAfterWoodsDays = null, Action<GameSession, int> daily = null)
        {
            var list = Seeds.Select(seed => Play(name, level, years, sabotage, sealAfterWoodsDays, seed, daily)).ToList();
            Runs.AddRange(list);
            return list;
        }

        [TestCase(1)]
        [TestCase(2)]
        public void TheIgnorer_NeverWakesTheGod_AndWakefulnessStaysLow(int level)
        {
            foreach (var r in PlayAll("ignorer, 6 years", level, 6))
            {
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}: the god never wakes");
                Assert.LessOrEqual(r.MaxWakefulness, 400, $"seed {r.Seed}: the sawtooth stays under 40% (25% a season, 30-40% off at each ritual)");
                Assert.AreEqual(0, r.Failures, $"seed {r.Seed}: every ritual of an ignored world succeeds");
            }
        }

        [TestCase(2)]
        public void TheSaboteur_WhoDisruptsEveryRitual_WakesTheGodButNotForAYear(int level)
        {
            var firstSabotageDay = 28 + RitualDirector.RitualDay;               // the first summer ritual
            foreach (var r in PlayAll("saboteur, every ritual", level, 5, n => true))
            {
                Assert.Greater(r.AwakenedOnDay, 0, $"seed {r.Seed}: someone who always disrupts the Keepers does wake the god");
                Assert.GreaterOrEqual(r.AwakenedOnDay - firstSabotageDay, 28 * 5 / 2, $"seed {r.Seed}: about three seasons (at least ten weeks) of warning after the first disruption");
                Assert.LessOrEqual(r.AwakenedOnDay, 28 * 4 * 3, $"seed {r.Seed}: but not so long that nothing is at stake");
            }
        }

        [TestCase(2)]
        public void TheSaboteur_WhoSealsTheGod_BeforeItWakes_NeverSeesItAwake(int level)
        {
            foreach (var r in PlayAll("saboteur who seals 14 days after the woods open", level, 5, n => true, sealAfterWoodsDays: 14))
            {
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}");
                Assert.Greater(r.SealedOnDay, 0, $"seed {r.Seed}");
                Assert.AreEqual(0, r.FinalWakefulness, $"seed {r.Seed}: sealed, the god sleeps for good");
                Assert.Less(r.WakefulnessAtSeal, 600, $"seed {r.Seed}: still comfortably asleep when the fix was found");
            }
            foreach (var r in PlayAll("saboteur who seals 60 days after the woods open", level, 5, n => true, sealAfterWoodsDays: 60))
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}: even a slow investigator is in time");
            // The window is real: someone who disrupts every ritual and takes 90 days to find the fix is too late. (If the design numbers change, this
            // line is the one that says how the window moved.)
            foreach (var r in PlayAll("saboteur who seals 90 days after the woods open", level, 5, n => true, sealAfterWoodsDays: 90))
                Assert.Greater(r.AwakenedOnDay, 0, $"seed {r.Seed}: ninety days is too slow");
        }

        [TestCase(2)]
        public void ARepentantSaboteur_WhoStopsAfterTwoRituals_BringsTheGodBackDown(int level)
        {
            foreach (var r in PlayAll("saboteur for 2 rituals, then stops", level, 5, n => n <= 2))
            {
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}: stopping in time avoids the awakening");
                Assert.LessOrEqual(r.SeasonEnds.Last(), 400, $"seed {r.Seed}: after the next rituals the god is back below 40%");
                Assert.AreEqual(2, r.Disrupted, $"seed {r.Seed}");
            }
        }

        // An ordinary farmer who never reads the journal: a greenhouse full of crops that are harvested and shipped as soon as they are ready (every
        // `harvestEvery` days). Whatever the Keepers marked among those crops is gone before the ritual, and the bot never meant to stop anything.
        static Action<GameSession, int> Farmer(int harvestEvery) => (s, day) =>
        {
            var grid = s.GetGrid(MapIds.Greenhouse);
            s.Db.TryGetCrop("parsnip", out var parsnip);
            for (var i = 0; i < 40; i++)
            {
                int x = i % 8, y = i / 8;
                grid.Till(x, y);
                if (!grid.TryGetTile(x, y, out var tile)) continue;
                if (tile.Crop == null) grid.Plant(x, y, parsnip, s.Clock.Now.Season);
                grid.Water(x, y);
                if (day % harvestEvery == 0 && grid.TryHarvest(x, y, id => parsnip, out var got)) s.State.ShippingBin.Add(new ItemStack(got.ItemId, 1));
            }
        };

        [TestCase(2)]
        public void AFarmerWhoShipsEverythingEachDay_IsMeasured_ForAccidentalFailures(int level)
        {
            // FINDING (docs/balance/mythos.md): an unaware farmer fails 15-30% of the rituals without ever touching the altar, because the Keepers mark
            // some of the player's own crops, items and animals and the ritual fails when a marked thing is gone. Nobody woke the god in these worlds,
            // but the peaks reach 50-70%. The owner decided to keep the rule as designed (2026-10-06), so the test pins the outcome.
            foreach (var r in PlayAll("farmer: harvests and ships every day", level, 5, daily: Farmer(1)))
            {
                Assert.GreaterOrEqual(r.Successes + r.Failures, 1, $"seed {r.Seed}: rituals ran");
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}: an unaware farmer does not end the world");
            }
            foreach (var r in PlayAll("farmer: harvests and ships every 3 days", level, 5, daily: Farmer(3)))
                Assert.AreEqual(-1, r.AwakenedOnDay, $"seed {r.Seed}: an unaware farmer does not end the world");
        }

        [Test]
        public void MildAndFull_FollowTheSameRules_OnlyThePresentationDiffers()
        {
            foreach (var seed in Seeds.Take(2))
            {
                var mild = Play("saboteur, every ritual (mild)", 1, 5, n => true, seed: seed);
                var full = Play("saboteur, every ritual (full)", 2, 5, n => true, seed: seed);
                Runs.Add(mild);
                Runs.Add(full);
                Assert.AreEqual(mild.Disrupted > 0, full.Disrupted > 0);
                Assert.AreEqual(-1 == mild.AwakenedOnDay, -1 == full.AwakenedOnDay, $"seed {seed}: both wake the god or neither does");
            }
        }

        [OneTimeTearDown]
        public void WriteTheReport()
        {
            if (Runs.Count == 0) return;
            var sb = new StringBuilder();
            sb.AppendLine("# Mythos balance (X-010)");
            sb.AppendLine();
            sb.AppendLine("Generated by `MythosBalanceTests` (bots through the real session, hooks and ritual director). Columns: rituals that succeeded / failed / disrupted by the bot; the highest wakefulness; when the god woke; when the bot sealed it.");
            sb.AppendLine();
            sb.AppendLine("| Bot | Intensity | Seed | Days | Rituals | Peak | Awake | Sealed | End |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
            foreach (var r in Runs) sb.AppendLine(Row(r));
            Directory.CreateDirectory("Builds/balance");
            File.WriteAllText("Builds/balance/mythos.md", sb.ToString());
            Runs.Clear();
        }
    }
}
