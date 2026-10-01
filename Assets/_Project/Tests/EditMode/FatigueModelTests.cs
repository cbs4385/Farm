using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    public class FatigueModelTests
    {
        const int TenPm = 22 * 60;
        const int Midnight = 24 * 60;
        const int SixAm = 30 * 60;

        [Test]
        public void NoFatigueUntilTenPm_ThenItGrowsToFullBySixAm()
        {
            Assert.AreEqual(0f, FatigueModel.Fatigue(6 * 60));
            Assert.AreEqual(0f, FatigueModel.Fatigue(21 * 60 + 59));
            Assert.AreEqual(0f, FatigueModel.Fatigue(TenPm), "exactly 22:00 is still rested");
            Assert.Greater(FatigueModel.Fatigue(TenPm + 10), 0f);
            Assert.AreEqual(0.25f, FatigueModel.Fatigue(Midnight), 0.0001f, "two of the eight hours");
            Assert.AreEqual(0.5f, FatigueModel.Fatigue(26 * 60), 0.0001f, "02:00 is half way");
            Assert.AreEqual(1f, FatigueModel.Fatigue(SixAm), 0.0001f);
            Assert.AreEqual(1f, FatigueModel.Fatigue(SixAm + 120), "never above 1");
        }

        [Test]
        public void FatigueNeverDecreasesAsTheNightGoesOn()
        {
            var previous = 0f;
            for (var minute = 6 * 60; minute <= SixAm; minute += 10)
            {
                var f = FatigueModel.Fatigue(minute);
                Assert.GreaterOrEqual(f, previous, $"minute {minute}");
                previous = f;
            }
        }

        [Test]
        public void TheMeterIsShownOnlyWhileFatigueIsPositive()
        {
            Assert.IsFalse(FatigueModel.MeterVisible(12 * 60));
            Assert.IsFalse(FatigueModel.MeterVisible(TenPm));
            Assert.IsTrue(FatigueModel.MeterVisible(TenPm + 1));
            Assert.IsTrue(FatigueModel.MeterVisible(SixAm));
        }

        [Test]
        public void AFullNight_MeansNoEnergyRecovery_AndHalfEffectiveLuck()
        {
            Assert.AreEqual(1.0f, FatigueModel.EnergyRecoveryPenalty(1f), 0.0001f);
            Assert.AreEqual(0.5f, FatigueModel.LuckPenalty(1f), 0.0001f);
            Assert.AreEqual(0.5f, FatigueModel.LuckEffectiveness(1f), 0.0001f);
            Assert.AreEqual(0f, FatigueModel.EnergyRecoveryPenalty(0f));
            Assert.AreEqual(1f, FatigueModel.LuckEffectiveness(0f));
        }

        [Test]
        public void PenaltiesAreClamped()
        {
            Assert.AreEqual(1f, FatigueModel.EnergyRecoveryPenalty(7f));
            Assert.AreEqual(0f, FatigueModel.EnergyRecoveryPenalty(-2f));
            Assert.AreEqual(0.5f, FatigueModel.LuckPenalty(7f), 0.0001f);
        }

        [Test]
        public void SleepRestoresEnergyTowardsTheTarget_ScaledByFatigue()
        {
            Assert.AreEqual(270, FatigueModel.EnergyAfterSleep(20, 270, 0f), "rested: full recovery");
            Assert.AreEqual(145, FatigueModel.EnergyAfterSleep(20, 270, 0.5f), "half the recovery");
            Assert.AreEqual(20, FatigueModel.EnergyAfterSleep(20, 270, 1f), "an all-nighter recovers nothing");
            Assert.AreEqual(202, FatigueModel.EnergyAfterSleep(10, 202, 0f), "pass-out target is 75%");
            Assert.AreEqual(250, FatigueModel.EnergyAfterSleep(250, 202, 0.3f), "never lowers energy");
        }

        [Test]
        public void GoingToBedAtMidnight_RecoversThreeQuartersOfTheMissingEnergy()
        {
            var fatigue = FatigueModel.Fatigue(Midnight);   // 0.25
            Assert.AreEqual(100 + (int)System.Math.Round(170 * 0.75f), FatigueModel.EnergyAfterSleep(100, 270, fatigue));
        }

        [Test]
        public void TheWarningFiresOnce_WhenTheClockFirstReachesTenPm()
        {
            Assert.IsTrue(FatigueModel.ShouldWarn(TenPm - 10, TenPm, alreadyWarned: false));
            Assert.IsTrue(FatigueModel.ShouldWarn(TenPm - 10, TenPm + 10, alreadyWarned: false), "even if a tick skipped past it");
            Assert.IsFalse(FatigueModel.ShouldWarn(TenPm - 10, TenPm, alreadyWarned: true), "only the first time");
            Assert.IsFalse(FatigueModel.ShouldWarn(TenPm, TenPm + 10, alreadyWarned: false), "already past 22:00");
            Assert.IsFalse(FatigueModel.ShouldWarn(12 * 60, 12 * 60 + 10, alreadyWarned: false));
        }

        [Test]
        public void TheWarnedFlagIsAStableId()
        {
            Assert.AreEqual("tutorial.late_night_warned", FatigueModel.WarnedFlag);
        }
    }
}
