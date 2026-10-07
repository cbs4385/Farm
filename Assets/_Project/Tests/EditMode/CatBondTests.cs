using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest 2026-10-06: enough petting makes the village cat adopt the player. +1 per pet, 2 a day, at most 30; a day with no pet costs 1;
    // it adopts at 20 and leaves again at 10.
    public class CatBondTests
    {
        static void PetDays(GameState s, int fromDay, int days)
        {
            for (var d = fromDay; d < fromDay + days; d++)
            {
                if (d > fromDay) CatBond.NewDay(s, d);
                CatBond.Pet(s, d);
                CatBond.Pet(s, d);
            }
        }

        [Test]
        public void EachPetIsOne_AndOnlyTwoCountInADay()
        {
            var s = new GameState();
            Assert.AreEqual(CatBond.PetResult.Counted, CatBond.Pet(s, 5));
            Assert.AreEqual(CatBond.PetResult.Counted, CatBond.Pet(s, 5));
            Assert.AreEqual(CatBond.PetResult.DayLimit, CatBond.Pet(s, 5));
            Assert.AreEqual(2, CatBond.Bond(s));
            CatBond.NewDay(s, 6);
            Assert.AreEqual(CatBond.PetResult.Counted, CatBond.Pet(s, 6));
            Assert.AreEqual(3, CatBond.Bond(s));
        }

        [Test]
        public void TheBond_StopsAtThirty()
        {
            var s = new GameState();
            PetDays(s, 1, 40);
            Assert.AreEqual(CatBond.Max, CatBond.Bond(s));
            Assert.AreEqual(30, CatBond.Max);
        }

        [Test]
        public void ADayWithoutAPet_CostsOne_ButNotBelowZero()
        {
            var s = new GameState();
            CatBond.NewDay(s, 2);
            Assert.AreEqual(0, CatBond.Bond(s), "never below zero");
            CatBond.Pet(s, 3); CatBond.Pet(s, 3);
            CatBond.NewDay(s, 4);                       // petted yesterday: nothing lost
            Assert.AreEqual(2, CatBond.Bond(s));
            CatBond.NewDay(s, 5);                       // not petted on day 4
            Assert.AreEqual(1, CatBond.Bond(s));
            CatBond.NewDay(s, 6);
            Assert.AreEqual(0, CatBond.Bond(s));
        }

        [Test]
        public void TheCatAdoptsAtTwenty_AndLeavesAtTen_NotBefore()
        {
            var s = new GameState();
            PetDays(s, 1, 9);                           // 18
            Assert.IsFalse(CatBond.Adopted(s));
            CatBond.NewDay(s, 10);
            Assert.AreEqual(CatBond.PetResult.Counted, CatBond.Pet(s, 10));      // 19 (the new day's first pet)
            Assert.IsFalse(CatBond.Adopted(s));
            Assert.AreEqual(CatBond.PetResult.Adopted, CatBond.Pet(s, 10));      // 20
            Assert.IsTrue(CatBond.Adopted(s));
            Assert.AreEqual(20, CatBond.Bond(s));

            // Left alone it fades a day at a time: still adopted at 11, gone at 10.
            var day = 11;
            for (; CatBond.Bond(s) > 11; day++) Assert.AreNotEqual(CatBond.DayResult.Left, CatBond.NewDay(s, day));
            Assert.IsTrue(CatBond.Adopted(s), "still home at 11");
            Assert.AreEqual(CatBond.DayResult.Left, CatBond.NewDay(s, day));
            Assert.AreEqual(10, CatBond.Bond(s));
            Assert.IsFalse(CatBond.Adopted(s), "gone at 10");

            // It can be won back.
            PetDays(s, day + 1, 5);
            Assert.IsTrue(CatBond.Adopted(s));
        }
    }
}
