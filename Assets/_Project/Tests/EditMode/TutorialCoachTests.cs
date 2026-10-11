using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // Playtest 2026-10-10: a player wanted to be told what to do as they did it, one thing at a time, without leafing through a tutorial.
    public class TutorialCoachTests
    {
        TestSessionFixture Fixture()
        {
            L.SetTable(L.Parse(File.ReadAllText("Assets/_Project/Resources/Localization/en.json")));
            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            return new TestSessionFixture(db.AllItems, db.AllCrops);
        }

        [Test]
        public void TheCoach_FollowsWhatThePlayerHasDone_OneStepAtATime_AndEndsAfterTheFirstShipment()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                Assert.AreEqual("till", TutorialCoach.StepOf(s), "a new game starts with tilling");
                s.AddVar(QuestLog.Stats.Tilled, 3);
                Assert.AreEqual("plant", TutorialCoach.StepOf(s));
                s.AddVar(QuestLog.Stats.Planted, 3);
                Assert.AreEqual("water", TutorialCoach.StepOf(s));
                s.AddVar(QuestLog.Stats.Watered, 3);
                Assert.AreEqual("wait", TutorialCoach.StepOf(s), "nothing is ripe yet: sleep and wait");
                s.AddVar(QuestLog.Stats.Harvested, 1);
                Assert.AreEqual("ship", TutorialCoach.StepOf(s));
                s.AddVar(QuestLog.Stats.Shipped, 1);
                Assert.IsNull(TutorialCoach.StepOf(s), "done");
                Assert.AreEqual(string.Empty, TutorialCoach.Text(s));
            }
        }

        [Test]
        public void AfterTheFirstNight_TheTipRests_WithANotice_AndComesBackWhenACropIsRipe_UnlessSkipped()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.AddVar(QuestLog.Stats.Tilled, 3); s.AddVar(QuestLog.Stats.Planted, 3); s.AddVar(QuestLog.Stats.Watered, 3);
                Assert.AreEqual("wait", TutorialCoach.StepOf(s));

                TutorialCoach.OnNewDay(s);                                   // the player slept
                Assert.IsNull(TutorialCoach.StepOf(s), "tip 4 is gone after sleeping");
                Assert.IsTrue(f.Toasts.Exists(t => t.Contains("tips are resting")), "with a word that it will come back");
                var notices = f.Toasts.Count;
                TutorialCoach.OnNewDay(s);
                Assert.AreEqual(notices, f.Toasts.Count, "said once");

                var grid = s.GetGrid(MapIds.Farm);
                grid.Till(5, 5);
                grid.TryGetTile(5, 5, out var tile);
                s.Db.TryGetCrop("parsnip", out var parsnip);
                tile.Crop = new CropInstance { CropId = "parsnip", Stage = parsnip.MatureStage };
                Assert.AreEqual("harvest", TutorialCoach.StepOf(s), "it is back when a crop is ripe");

                TutorialCoach.Skip(s);
                Assert.IsNull(TutorialCoach.StepOf(s), "unless the tips were skipped");
            }
        }

        [Test]
        public void ANewDay_BeforeTheCropsAreWatered_DoesNotRestTheTips()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.AddVar(QuestLog.Stats.Tilled, 3);
                TutorialCoach.OnNewDay(s);
                Assert.AreEqual("plant", TutorialCoach.StepOf(s));
                Assert.IsEmpty(f.Toasts.Where(t => t.Contains("resting")).ToList());
            }
        }

        [Test]
        public void ARipeCrop_MakesTheStepSayHarvest()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                s.AddVar(QuestLog.Stats.Tilled, 3); s.AddVar(QuestLog.Stats.Planted, 3); s.AddVar(QuestLog.Stats.Watered, 3);
                var grid = s.GetGrid(MapIds.Farm);
                grid.Till(5, 5);
                Assert.IsTrue(grid.TryGetTile(5, 5, out var tile));
                s.Db.TryGetCrop("parsnip", out var parsnip);
                tile.Crop = new CropInstance { CropId = "parsnip", Stage = 0 };
                Assert.AreEqual("wait", TutorialCoach.StepOf(s));
                tile.Crop.Stage = parsnip.MatureStage;
                Assert.AreEqual("harvest", TutorialCoach.StepOf(s));
                tile.Crop.Withered = true;
                Assert.AreEqual("wait", TutorialCoach.StepOf(s), "a withered crop is not a tip to harvest");
            }
        }

        [Test]
        public void Skipping_StopsTheTipsForGood_AndTheTextNamesTheButtonsOfWhatIsInHand()
        {
            using (var f = Fixture())
            {
                var s = f.Session;
                ControlPrompts.SetKind(InputKind.KeyboardMouse);
                StringAssert.Contains("Tip 1 of 5", TutorialCoach.Text(s));
                StringAssert.Contains("hoe", TutorialCoach.Text(s));
                ControlPrompts.SetKind(InputKind.Gamepad);
                StringAssert.Contains("press X", TutorialCoach.Text(s), "a pad player is told the pad's button");
                ControlPrompts.SetKind(InputKind.KeyboardMouse);

                TutorialCoach.Skip(s);
                Assert.IsNull(TutorialCoach.StepOf(s));
                Assert.AreEqual(string.Empty, TutorialCoach.Text(s));
            }
        }

        [Test]
        public void TheOptionSwitchesTheTipsOff_ForANewGameToo()
        {
            using (var f = Fixture())
            {
                var settings = new SettingsData { TutorialTips = false };
                using (new SettingsOverride(settings)) Assert.IsNull(TutorialCoach.StepOf(f.Session));
                Assert.IsTrue(new SettingsData().TutorialTips, "on by default");
            }
        }

        // A settings store for the test, put back afterwards.
        sealed class SettingsOverride : System.IDisposable
        {
            readonly string _root = Path.Combine(Path.GetTempPath(), "farm-tips-" + System.Guid.NewGuid().ToString("N"));
            public SettingsOverride(SettingsData data)
            {
                Directory.CreateDirectory(_root);
                var store = new SettingsStore(_root);
                store.Load();
                store.Current.TutorialTips = data.TutorialTips;
                ServiceLocator.Register(store);
            }
            public void Dispose()
            {
                ServiceLocator.Clear();
                if (Directory.Exists(_root)) Directory.Delete(_root, true);
            }
        }
    }
}
