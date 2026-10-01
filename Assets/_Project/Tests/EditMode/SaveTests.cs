using System;
using System.IO;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Farm.Tests
{
    public class SaveTests
    {
        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static int MaxStack(string id) => id.StartsWith("tool.") ? 1 : 999;

        static GameState MakeState()
        {
            var s = GameState.NewGame("Sam", "Meadow", MaxStack);
            s.Gold = 1234;
            s.SetDate(new GameDateTime(2, Season.Fall, 14, 900));
            s.GetMap(MapIds.Farm).Tiles.Add(new FarmTile
            {
                X = 3, Y = -4, Watered = true,
                Crop = new CropInstance { CropId = "parsnip", Stage = 2, DaysInStage = 1 },
            });
            s.ShippingBin.Add(new ItemStack("crop.parsnip", 3, 1));
            s.Flags.Add("tutorial.done");
            s.SkillXp["farming"] = 55;
            return s;
        }

        [Test]
        public void SaveLoad_RoundTripsEverything()
        {
            var svc = new SaveService(_root);
            svc.Save(1, MakeState());

            Assert.IsTrue(svc.TryLoad(1, out var loaded, out var error), error);

            Assert.AreEqual("Sam", loaded.PlayerName);
            Assert.AreEqual("Meadow", loaded.FarmName);
            Assert.AreEqual(1234, loaded.Gold);
            Assert.AreEqual(new GameDateTime(2, Season.Fall, 14, 900), loaded.GetDate());
            var tile = loaded.GetMap(MapIds.Farm).Tiles[0];
            Assert.AreEqual(3, tile.X);
            Assert.AreEqual(-4, tile.Y);
            Assert.IsTrue(tile.Watered);
            Assert.AreEqual(2, tile.Crop.Stage);
            Assert.AreEqual(3, loaded.ShippingBin[0].Count);
            Assert.AreEqual(1, loaded.ShippingBin[0].Quality);
            Assert.IsTrue(loaded.Flags.Contains("tutorial.done"));
            Assert.AreEqual(55, loaded.SkillXp["farming"]);
            var pack = Inventory.FromData(loaded.Backpack, MaxStack);
            Assert.AreEqual(15, pack.Count("seed.parsnip"));
            Assert.AreEqual(1, pack.Count(ItemIds.Hoe));
        }

        [Test]
        public void Summarize_ReportsSlotInfo_AndEmptySlots()
        {
            var svc = new SaveService(_root);
            Assert.IsFalse(svc.Summarize(0).Exists);
            svc.Save(2, MakeState());
            var sum = svc.Summarize(2);
            Assert.IsTrue(sum.Exists);
            Assert.AreEqual("Meadow", sum.FarmName);
            Assert.AreEqual(1234, sum.Gold);
            Assert.AreEqual(14, sum.Day);
        }

        [Test]
        public void CorruptMainFile_FallsBackToBackup()
        {
            var svc = new SaveService(_root);
            var first = MakeState();
            first.Gold = 111;
            svc.Save(0, first);
            var second = MakeState();
            second.Gold = 222;
            svc.Save(0, second);              // first becomes save.json.bak

            File.WriteAllText(svc.SlotPath(0), "{ not json");

            Assert.IsTrue(svc.TryLoad(0, out var loaded, out var error), error);
            Assert.AreEqual(111, loaded.Gold);
        }

        [Test]
        public void MissingSlot_FailsCleanly()
        {
            var svc = new SaveService(_root);
            Assert.IsFalse(svc.TryLoad(0, out var state, out var error));
            Assert.IsNull(state);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void NewerSaveVersion_IsRejected()
        {
            var svc = new SaveService(_root);
            svc.Save(0, MakeState());
            var json = JObject.Parse(File.ReadAllText(svc.SlotPath(0)));
            json["SaveVersion"] = GameState.CurrentVersion + 5;
            File.WriteAllText(svc.SlotPath(0), json.ToString());

            Assert.IsFalse(svc.TryLoad(0, out _, out var error));
            StringAssert.Contains("newer", error);
        }

        class RenameGoldMigration : ISaveMigration
        {
            public int FromVersion => 0;
            public void Migrate(JObject root)
            {
                root["Gold"] = root["Money"];
                root.Remove("Money");
            }
        }

        [Test]
        public void OldVersion_IsMigrated()
        {
            var svc = new SaveService(_root, new ISaveMigration[] { new RenameGoldMigration() });
            var json = JObject.FromObject(MakeState());
            json["SaveVersion"] = 0;
            json["Money"] = 777;
            json.Remove("Gold");
            Directory.CreateDirectory(Path.GetDirectoryName(svc.SlotPath(1)));
            File.WriteAllText(svc.SlotPath(1), json.ToString());

            Assert.IsTrue(svc.TryLoad(1, out var loaded, out var error), error);
            Assert.AreEqual(777, loaded.Gold);
            Assert.AreEqual(GameState.CurrentVersion, loaded.SaveVersion);
        }

        [Test]
        public void OldVersion_WithoutMigration_FailsCleanly()
        {
            var svc = new SaveService(_root);
            var json = JObject.FromObject(MakeState());
            json["SaveVersion"] = 0;
            Directory.CreateDirectory(Path.GetDirectoryName(svc.SlotPath(1)));
            File.WriteAllText(svc.SlotPath(1), json.ToString());
            Assert.IsFalse(svc.TryLoad(1, out _, out _));
        }

        [Test]
        public void Delete_RemovesSlot()
        {
            var svc = new SaveService(_root);
            svc.Save(1, MakeState());
            svc.Delete(1);
            Assert.IsFalse(svc.Exists(1));
        }

        [Test]
        public void Settings_RoundTrip_AndClamp()
        {
            var store = new SettingsStore(_root);
            store.Load();
            store.Current.MasterVolume = 5f;
            store.Current.Language = "";
            store.Current.BindingOverridesJson = "{\"x\":1}";
            store.Save();

            var again = new SettingsStore(_root);
            var loaded = again.Load();
            Assert.AreEqual(1f, loaded.MasterVolume);
            Assert.AreEqual("en", loaded.Language);
            Assert.AreEqual("{\"x\":1}", loaded.BindingOverridesJson);
        }

        [Test]
        public void AtomicFile_KeepsBackupOfPreviousVersion()
        {
            var path = Path.Combine(_root, "a.txt");
            AtomicFile.Write(path, "one");
            AtomicFile.Write(path, "two");
            Assert.AreEqual("two", File.ReadAllText(path));
            Assert.AreEqual("one", File.ReadAllText(path + ".bak"));
        }
    }
}
