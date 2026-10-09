using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;

namespace Farm.Tests
{
    // Playtest chat 2026-10-09: the hover label names a planted crop and says when it will be ready.
    public class CropInfoTests
    {
        static CropDefinition Parsnip() => CropDefinition.Create("parsnip", new[] { 1, 2, 1, 3 }, SeasonMask.Spring);
        static CropDefinition Beans() => CropDefinition.Create("beans", new[] { 1, 1, 1, 2 }, SeasonMask.Spring, regrowDays: 3);

        [Test]
        public void DaysToHarvest_AddsTheStagesLeft_AndTakesOffDaysAlreadySpent()
        {
            var def = Parsnip();
            Assert.AreEqual(7, CropInfo.DaysToHarvest(new CropInstance { CropId = "parsnip" }, def));
            Assert.AreEqual(6, CropInfo.DaysToHarvest(new CropInstance { CropId = "parsnip", Stage = 1 }, def));
            Assert.AreEqual(5, CropInfo.DaysToHarvest(new CropInstance { CropId = "parsnip", Stage = 1, DaysInStage = 1 }, def));
            Assert.AreEqual(0, CropInfo.DaysToHarvest(new CropInstance { CropId = "parsnip", Stage = 4 }, def));
        }

        [Test]
        public void ARegrowingCrop_CountsTheRegrowDays_InItsLastStage()
        {
            var def = Beans();
            Assert.AreEqual(3, CropInfo.DaysToHarvest(new CropInstance { CropId = "beans", Stage = 3, Regrowing = true }, def));
            Assert.AreEqual(2, CropInfo.DaysToHarvest(new CropInstance { CropId = "beans", Stage = 3, Regrowing = true, DaysInStage = 1 }, def));
        }

        [Test]
        public void SpeedGro_TakesADayOffEveryStageOfTwoOrMoreDays()
        {
            Assert.AreEqual(5, CropInfo.DaysToHarvest(new CropInstance { CropId = "parsnip" }, Parsnip(), speedGro: true));   // 1 + 1 + 1 + 2
        }

        [Test]
        public void TheLabel_SaysSoil_Crop_Ready_AndWithered()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var f = new TestSessionFixture(UnityEngine.Resources.Load<GameDatabase>(GameDatabase.ResourcePath).AllItems);
            try
            {
                var db = UnityEngine.Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
                var bare = new FarmTile { X = 1, Y = 1, Watered = false };
                StringAssert.Contains("needs water", CropInfo.Label(bare, db));
                bare.Watered = true;
                StringAssert.Contains("watered", CropInfo.Label(bare, db));

                Assert.IsTrue(db.TryGetCrop("parsnip", out var parsnip), "the parsnip crop exists");
                var planted = new FarmTile { X = 1, Y = 1, Watered = false, Crop = new CropInstance { CropId = parsnip.Id } };
                var text = CropInfo.Label(planted, db);
                StringAssert.Contains("Parsnip", text);
                StringAssert.Contains("days", text);
                StringAssert.Contains("needs water", text);

                planted.Crop.Stage = parsnip.MatureStage;
                StringAssert.Contains("ready to harvest", CropInfo.Label(planted, db));
                planted.Crop.Withered = true;
                StringAssert.Contains("withered", CropInfo.Label(planted, db));
            }
            finally { f.Dispose(); }
        }
    }
}
