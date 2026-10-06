using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Farm.Mythos;
using NUnit.Framework;

namespace Farm.Tests
{
    // X-005: blight (crops wither, stay, and are cleared with the scythe) and sleepwalking (wake anywhere outdoors), plus the core's
    // generic withered-crop rules.
    public class MythosNightTests
    {
        TestSessionFixture _f;
        GameSession S => _f.Session;
        CropDefinition _crop;

        [SetUp]
        public void SetUp()
        {
            _crop = CropDefinition.Create("parsnip", new[] { 2, 2 }, SeasonMask.All);
            _f = new TestSessionFixture(null, new[] { _crop });
        }

        [TearDown]
        public void TearDown()
        {
            GameSession.HorrorLevelOverride = null;
            _f.Dispose();
            Conditions.ClearCustomForTests();
        }

        void Install(int level)
        {
            GameSession.HorrorLevelOverride = level;
            S.Story = null;
            MythosModule.Install(S, S.Hooks, _f.Bus);
            S.Story = new StoryContent();
        }

        FarmGrid Farm(int crops)
        {
            var grid = S.GetGrid(MapIds.Farm);
            for (var i = 0; i < crops; i++) { grid.Till(i, 0); grid.Plant(i, 0, _crop, Season.Spring); }
            return grid;
        }

        // ---- the core's withered crops ----

        [Test]
        public void AWitheredCrop_DoesNotGrow_CannotBeHarvested_AndCanBeCleared()
        {
            var grid = Farm(1);
            Assert.IsTrue(grid.Wither(0, 0));
            Assert.IsFalse(grid.Wither(0, 0), "already withered");
            grid.Water(0, 0);
            for (var i = 0; i < 6; i++) grid.AdvanceDay(Season.Spring, true, id => _crop);
            grid.TryGetTile(0, 0, out var t);
            Assert.AreEqual(0, t.Crop.Stage, "it never grows");
            t.Crop.Stage = _crop.MatureStage;
            Assert.IsFalse(grid.IsMature(0, 0, id => _crop));
            Assert.IsFalse(grid.TryHarvest(0, 0, id => _crop, out _));
            Assert.IsTrue(grid.IsWithered(0, 0));
            Assert.IsTrue(grid.ClearCrop(0, 0));
            Assert.IsFalse(grid.IsWithered(0, 0));
        }

        [Test]
        public void AWitheredCrop_StillDiesWithTheSeason()
        {
            var grid = S.GetGrid(MapIds.Farm);
            var spring = CropDefinition.Create("p2", new[] { 1 }, SeasonMask.Spring);
            grid.Till(0, 0);
            grid.Plant(0, 0, spring, Season.Spring);
            grid.Wither(0, 0);
            Assert.AreEqual(1, grid.AdvanceDay(Season.Summer, false, id => spring));
            grid.TryGetTile(0, 0, out var t);
            Assert.IsNull(t.Crop);
        }

        // ---- blight ----

        [Test]
        public void Blight_ChanceIsZeroBeforeTheFirstStep_AndGrowsAfter()
        {
            Assert.AreEqual(0f, MythosBlight.Chance(MythosBlight.FirstStep - 1, 1f));
            Assert.Greater(MythosBlight.Chance(MythosBlight.FirstStep, 1f), 0f);
            Assert.Less(MythosBlight.Chance(MythosBlight.FirstStep, 1f), MythosBlight.Chance(20, 1f));
            Assert.AreEqual(MythosBlight.Chance(20, 1f) / 2f, MythosBlight.Chance(20, 0.5f), 1e-6f);
        }

        [Test]
        public void Blight_AtLevel0_NeverWithersAnything()
        {
            Install(0);
            var grid = Farm(20);
            S.SetVar(MythosIds2.Step, 20);
            Assert.AreEqual(0, MythosBlight.Strike(S, 5));
            Assert.IsFalse(grid.Tiles.Any(t => t.Crop.Withered));
        }

        [Test]
        public void Blight_AtTheLastStep_WithersSomeCrops_UpToTheNightlyCap_AndLeavesThemStanding()
        {
            Install(2);
            var grid = Farm(40);
            S.SetVar(MythosIds2.Step, 20);
            var total = 0;
            for (var day = 1; day < 200; day++)
            {
                var n = MythosBlight.Strike(S, day);
                Assert.LessOrEqual(n, MythosBlight.MaxPerNight);
                total += n;
            }
            Assert.Greater(total, 0);
            Assert.AreEqual(total, grid.Tiles.Count(t => t.Crop != null && t.Crop.Withered), "withered crops stay on the soil");
            Assert.AreEqual(40, grid.Tiles.Count(t => t.Crop != null), "none vanished");
        }

        [Test]
        public void Blight_SparesCropsUnderAScarecrow_AndASeasonAfterASuccessfulRitual()
        {
            Install(2);
            var grid = Farm(40);
            S.SetVar(MythosIds2.Step, 20);
            var scarecrow = S.Placeables.Get(CraftingDefaults.Scarecrow);
            Assert.IsNotNull(scarecrow);
            S.GetObjects(MapIds.Farm).Place(scarecrow, 20, 0, "sc");
            var covered = Enumerable.Range(0, 40).Where(x => Sprinklers.Protected(S.GetObjects(MapIds.Farm).All, S.Placeables, x, 0)).ToList();
            Assert.IsNotEmpty(covered);
            for (var day = 1; day < 300; day++) MythosBlight.Strike(S, day);
            foreach (var x in covered)
            {
                grid.TryGetTile(x, 0, out var t);
                Assert.IsFalse(t.Crop.Withered, "protected x=" + x);
            }

            var save = RitualDirector.Load(S);
            save.LastSucceeded = true;
            save.ResolvedSeason = RitualDirector.SeasonIndex(S.Clock.Now);
            RitualDirector.Store(S, save);
            foreach (var tile in grid.Tiles) tile.Crop.Withered = false;
            for (var day = 1; day < 300; day++) Assert.AreEqual(0, MythosBlight.Strike(S, day));
        }

        // ---- sleepwalking ----

        [Test]
        public void Sleepwalk_ChanceIsZeroEarlyAndAtOff_AndRestedNeedsAWeek()
        {
            Assert.AreEqual(0f, MythosSleepwalk.Chance(0, 0, 1f));
            Assert.AreEqual(0f, MythosSleepwalk.Chance(15, 80, 0f));
            Assert.Greater(MythosSleepwalk.Chance(15, 80, 1f), 0f);
            Assert.LessOrEqual(MythosSleepwalk.Chance(20, 100, 1f), 0.2f);
            Assert.IsTrue(MythosSleepwalk.Rested(0, 3));
            Assert.IsFalse(MythosSleepwalk.Rested(11, 14));         // last walked on day 10
            Assert.IsTrue(MythosSleepwalk.Rested(11, 17));
        }

        [Test]
        public void Sleepwalk_Destination_IsOutdoors_AndTheWoodNeedsItsGate()
        {
            var closed = Enumerable.Range(0, 100).Select(i => MythosSleepwalk.Destination(i / 100f, false)).Distinct().ToList();
            CollectionAssert.DoesNotContain(closed, MapIds.Woods);
            CollectionAssert.Contains(closed, MapIds.Farm);
            var open = Enumerable.Range(0, 100).Select(i => MythosSleepwalk.Destination(i / 100f, true)).Distinct().ToList();
            CollectionAssert.Contains(open, MapIds.Woods);
            foreach (var m in open) CollectionAssert.Contains(MythosSleepwalk.Maps, m);
        }

        [Test]
        public void Sleepwalking_MovesTheWakePlace_AtMostOncePerWeek_AndNeverAtOff()
        {
            foreach (var level in new[] { 0, 2 })
            {
                _f.Dispose();
                SetUp();
                Install(level);
                S.SetVar(MythosIds.Vars.Wakefulness, 900);
                var walked = 0;
                var lastDay = -100;
                for (var i = 0; i < 80; i++)
                {
                    S.SetVar(MythosIds.Vars.Dread, 90);
                    S.EndDay(false);
                    if (S.State.SpawnPoint != MythosSleepwalk.SpawnName) continue;
                    walked++;
                    Assert.GreaterOrEqual(S.Clock.Now.TotalDays - lastDay, MythosSleepwalk.MinDaysApart, "once a week at most");
                    lastDay = S.Clock.Now.TotalDays;
                    CollectionAssert.Contains(MythosSleepwalk.Maps, S.State.CurrentMap);
                }
                if (level == 0) Assert.AreEqual(0, walked);
                else Assert.Greater(walked, 0, "at full intensity it happens");
            }
        }
    }
}
