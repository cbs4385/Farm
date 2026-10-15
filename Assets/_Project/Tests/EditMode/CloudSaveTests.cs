using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Data;
using Farm.Gameplay;
using Farm.Platform;
using NUnit.Framework;

namespace Farm.Tests
{
    // Cloud saves (SaveService.Cloud.cs) against a fake store: where a save lives, the one-time offer to move old saves, offline and
    // failing cloud, and the rule for a slot whose local and cloud copies differ (newest wins, the loser is kept as a backup).
    public class CloudSaveTests
    {
        sealed class FakeCloud : ICloudStorage
        {
            public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
            public readonly Dictionary<string, DateTime> Times = new Dictionary<string, DateTime>();
            public bool Available = true;
            public bool FailWrites;
            public bool FailDeletes;
            public bool IsAvailable => Available;
            public bool Exists(string name) => Available && Files.ContainsKey(name);
            public DateTime ModifiedUtc(string name) => Available && Times.TryGetValue(name, out var t) ? t : DateTime.MinValue;
            public bool TryRead(string name, out string text)
            {
                text = null;
                return Available && Files.TryGetValue(name, out text);
            }
            public bool TryWrite(string name, string text)
            {
                if (!Available || FailWrites) return false;
                Files[name] = text;
                Times[name] = DateTime.UtcNow;
                return true;
            }
            public bool TryDelete(string name)
            {
                if (!Available || FailDeletes) return false;
                Files.Remove(name);
                Times.Remove(name);
                return true;
            }
        }

        string _root;
        FakeCloud _cloud;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-cloud-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _cloud = new FakeCloud();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        SaveService Service(ICloudStorage cloud) => new SaveService(_root, null, () => cloud);

        static int MaxStack(string id) => 999;

        static GameState State(int gold)
        {
            var s = GameState.NewGame("Sam", "Meadow", MaxStack);
            s.Gold = gold;
            return s;
        }

        int GoldOf(SaveService svc, int slot)
        {
            Assert.IsTrue(svc.TryLoad(slot, out var s, out var error), error);
            return s.Gold;
        }

        // ---- the platform and old saves ------------------------------------------------------------------------------------

        [Test]
        public void TheNullPlatformHasNoCloud()
        {
            Assert.IsNull(((IPlatformServices)new NullPlatform()).Cloud);
        }

        [Test]
        public void ASaveWithoutLocationFile_IsLocal_AndLoadsUnchanged()
        {
            var plain = new SaveService(_root);
            plain.Save(0, State(321));
            Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(plain.SlotPath(0)), "location.json")));

            var svc = Service(_cloud);
            Assert.AreEqual(SaveLocation.Local, svc.Summarize(0).Location);
            Assert.AreEqual(321, GoldOf(svc, 0));
            Assert.IsEmpty(_cloud.Files, "a local save is never uploaded unless the player moves it");
        }

        // ---- choosing where a new game lives -------------------------------------------------------------------------------

        [Test]
        public void ANewCloudGame_IsPushedOnEverySave_AndLabelledCloud()
        {
            var svc = Service(_cloud);
            Assert.AreEqual(SaveLocation.Cloud, svc.StartSlot(1, SaveLocation.Cloud));
            svc.Save(1, State(10));
            svc.Save(1, State(20));

            Assert.IsTrue(_cloud.Files.ContainsKey(SaveService.CloudName(1)));
            Assert.AreEqual(SaveLocation.Cloud, svc.Summarize(1).Location);
            Assert.AreEqual(20, GoldOf(Service(_cloud), 1));
        }

        [Test]
        public void ANewLocalGame_StaysOffTheCloud()
        {
            var svc = Service(_cloud);
            Assert.AreEqual(SaveLocation.Local, svc.StartSlot(0, SaveLocation.Local));
            svc.Save(0, State(5));
            Assert.IsEmpty(_cloud.Files);
            Assert.AreEqual(SaveLocation.Local, svc.Summarize(0).Location);
        }

        [Test]
        public void WithoutACloud_TheCloudChoiceFallsBackToLocalSilently()
        {
            foreach (var svc in new[] { new SaveService(_root), Service(null), Service(new FakeCloud { Available = false }) })
            {
                Assert.IsFalse(svc.CloudAvailable);
                Assert.AreEqual(SaveLocation.Local, svc.StartSlot(2, SaveLocation.Cloud));
                svc.Save(2, State(7));
                Assert.AreEqual(SaveLocation.Local, svc.Summarize(2).Location);
                Assert.AreEqual(7, GoldOf(svc, 2));
            }
        }

        [Test]
        public void ACloudProviderThatThrows_IsTreatedAsNoCloud()
        {
            var svc = new SaveService(_root, null, () => throw new InvalidOperationException("steam exploded"));
            Assert.IsFalse(svc.CloudAvailable);
            Assert.AreEqual(SaveLocation.Local, svc.StartSlot(0, SaveLocation.Cloud));
            svc.Save(0, State(1));
            Assert.AreEqual(1, GoldOf(svc, 0));
        }

        // ---- offline and failing cloud -------------------------------------------------------------------------------------

        [Test]
        public void AFailingCloudWrite_KeepsTheLocalSave_AndTheNextSyncCatchesUp()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(100));
            _cloud.FailWrites = true;
            svc.Save(0, State(200));                                     // does not reach the cloud
            Assert.AreEqual(200, GoldOf(svc, 0));
            Assert.AreNotEqual(_cloud.Files[SaveService.CloudName(0)], File.ReadAllText(svc.SlotPath(0)));

            _cloud.FailWrites = false;
            File.SetLastWriteTimeUtc(svc.SlotPath(0), DateTime.UtcNow.AddMinutes(5));      // the local copy is the newer one
            svc.SyncSlot(0);
            Assert.AreEqual(File.ReadAllText(svc.SlotPath(0)), _cloud.Files[SaveService.CloudName(0)]);
        }

        [Test]
        public void WhileOffline_ACloudSlotStillPlaysAndShowsItsLabel()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            _cloud.Available = false;
            svc.Save(0, State(2));
            Assert.AreEqual(SaveLocation.Cloud, svc.Summarize(0).Location);
            Assert.IsTrue(svc.Summarize(0).Exists);
            Assert.AreEqual(2, GoldOf(svc, 0));
        }

        [Test]
        public void ACloudSaveFromAnotherComputer_IsPulledDown()
        {
            var other = Service(_cloud);
            other.StartSlot(0, SaveLocation.Cloud);
            other.Save(0, State(777));

            var freshRoot = Path.Combine(Path.GetTempPath(), "farm-cloud-" + Guid.NewGuid().ToString("N"));
            try
            {
                var here = new SaveService(freshRoot, null, () => _cloud);
                Assert.IsFalse(here.Summarize(0).Exists);
                here.SyncAll();
                var sum = here.Summarize(0);
                Assert.IsTrue(sum.Exists);
                Assert.AreEqual(SaveLocation.Cloud, sum.Location);
                Assert.AreEqual(777, GoldOf(here, 0));
            }
            finally { if (Directory.Exists(freshRoot)) Directory.Delete(freshRoot, true); }
        }

        // ---- conflicts ---------------------------------------------------------------------------------------------------

        void MakeDiffering(SaveService svc, int localGold, int cloudGold, DateTime localTime, DateTime cloudTime)
        {
            svc.Save(0, State(localGold));
            var cloudCopy = new SaveService(Path.Combine(_root, "other"), null, null);
            cloudCopy.Save(0, State(cloudGold));
            _cloud.Files[SaveService.CloudName(0)] = File.ReadAllText(cloudCopy.SlotPath(0));
            _cloud.Times[SaveService.CloudName(0)] = cloudTime;
            File.SetLastWriteTimeUtc(svc.SlotPath(0), localTime);
        }

        [Test]
        public void WhenTheCopiesDiffer_TheNewerCloudCopyWins_AndTheLocalOneIsKept()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            var now = DateTime.UtcNow;
            MakeDiffering(svc, 100, 900, now.AddHours(-2), now);
            svc.SyncSlot(0);

            Assert.AreEqual(900, GoldOf(svc, 0));
            var backups = svc.ConflictBackups(0);
            Assert.AreEqual(1, backups.Count);
            StringAssert.Contains("100", File.ReadAllText(backups[0]));
        }

        [Test]
        public void WhenTheCopiesDiffer_TheNewerLocalCopyWins_AndTheCloudOneIsKept()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            var now = DateTime.UtcNow;
            MakeDiffering(svc, 100, 900, now, now.AddHours(-2));
            svc.SyncSlot(0);

            Assert.AreEqual(100, GoldOf(svc, 0));
            StringAssert.Contains("\"Gold\":100", _cloud.Files[SaveService.CloudName(0)]);
            var backups = svc.ConflictBackups(0);
            Assert.AreEqual(1, backups.Count);
            StringAssert.Contains("\"Gold\":900", File.ReadAllText(backups[0]));
        }

        [Test]
        public void ACorruptCopyAlwaysLoses_AndATieGoesToTheCloud()
        {
            var t = DateTime.UtcNow;
            Assert.IsTrue(SaveService.ChooseCloud("{ broken", "{}", t, t.AddDays(-9)));
            Assert.IsFalse(SaveService.ChooseCloud("{}", "{ broken", t.AddDays(-9), t));
            Assert.IsTrue(SaveService.ChooseCloud("{\"a\":1}", "{\"a\":2}", t, t));
            Assert.IsFalse(SaveService.ChooseCloud("{\"a\":1}", "{\"a\":2}", t, t.AddSeconds(-1)));
        }

        [Test]
        public void IdenticalCopies_MakeNoBackup()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            svc.SyncSlot(0);
            svc.SyncSlot(0);
            Assert.IsEmpty(svc.ConflictBackups(0));
        }

        [Test]
        public void OnlyTheLastThreeConflictBackupsAreKept()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            for (var i = 0; i < 5; i++)
            {
                var now = DateTime.UtcNow;
                MakeDiffering(svc, 100 + i, 900 + i, now.AddHours(-2), now);
                svc.SyncSlot(0);
                System.Threading.Thread.Sleep(5);
            }
            Assert.AreEqual(3, svc.ConflictBackups(0).Count);
        }

        // ---- deleting ----------------------------------------------------------------------------------------------------

        [Test]
        public void DeletingACloudSave_RemovesTheCloudCopyToo()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            svc.Delete(0);
            Assert.IsEmpty(_cloud.Files);
            Assert.IsFalse(svc.Summarize(0).Exists);
        }

        [Test]
        public void ADeleteTheCloudRefuses_IsRetriedLater_AndTheSaveDoesNotComeBack()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            _cloud.FailDeletes = true;
            svc.Delete(0);
            Assert.IsTrue(_cloud.Files.ContainsKey(SaveService.CloudName(0)));

            _cloud.FailDeletes = false;
            svc.SyncAll();
            Assert.IsEmpty(_cloud.Files);
            Assert.IsFalse(svc.Summarize(0).Exists, "the deleted game must not be pulled back from the cloud");
        }

        [Test]
        public void StartingANewLocalGameOverACloudOne_RemovesTheCloudCopy()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            svc.StartSlot(0, SaveLocation.Local);
            svc.Save(0, State(2));
            Assert.IsEmpty(_cloud.Files);
            svc.SyncAll();
            Assert.AreEqual(2, GoldOf(svc, 0));
            Assert.AreEqual(SaveLocation.Local, svc.Summarize(0).Location);
        }

        // ---- the one-time offer to move old saves --------------------------------------------------------------------------

        [Test]
        public void TheOffer_IsMadeOnce_WhenLocalSavesExist()
        {
            new SaveService(_root).Save(0, State(11));
            new SaveService(_root).Save(2, State(33));
            var svc = Service(_cloud);

            Assert.IsTrue(svc.ShouldOfferMigration());
            Assert.IsTrue(svc.ShouldOfferMigration(), "asking again before answering does not change the answer");

            Assert.AreEqual(2, svc.MigrateLocalToCloud(out var failed));
            Assert.AreEqual(0, failed);
            Assert.IsFalse(svc.ShouldOfferMigration());
            Assert.IsFalse(Service(_cloud).ShouldOfferMigration(), "the answer is remembered across runs");
            Assert.AreEqual(SaveLocation.Cloud, svc.Summarize(0).Location);
            Assert.AreEqual(SaveLocation.Cloud, svc.Summarize(2).Location);
            Assert.AreEqual(11, GoldOf(Service(_cloud), 0));
            Assert.IsTrue(_cloud.Files.ContainsKey(SaveService.CloudName(2)));
        }

        [Test]
        public void DecliningTheOffer_KeepsSavesLocal_AndNeverAsksAgain()
        {
            new SaveService(_root).Save(0, State(11));
            var svc = Service(_cloud);
            Assert.IsTrue(svc.ShouldOfferMigration());
            svc.DeclineMigration();

            Assert.IsFalse(svc.ShouldOfferMigration());
            Assert.IsFalse(Service(_cloud).ShouldOfferMigration());
            Assert.AreEqual(SaveLocation.Local, svc.Summarize(0).Location);
            Assert.IsEmpty(_cloud.Files);
        }

        [Test]
        public void TheOffer_IsNotMadeWithoutCloud_AndIsNotUsedUp_ByAnOfflineStart()
        {
            new SaveService(_root).Save(0, State(11));
            Assert.IsFalse(new SaveService(_root).ShouldOfferMigration());
            Assert.IsFalse(Service(new FakeCloud { Available = false }).ShouldOfferMigration());
            Assert.IsTrue(Service(_cloud).ShouldOfferMigration(), "when Steam is available later the offer is still open");
        }

        [Test]
        public void TheOffer_IsNotMadeToAPlayerWithNoSaves_AndNewLocalGamesNeverTriggerIt()
        {
            var svc = Service(_cloud);
            Assert.IsFalse(svc.ShouldOfferMigration());
            svc.StartSlot(0, SaveLocation.Local);                        // the player chose local on purpose
            svc.Save(0, State(1));
            Assert.IsFalse(svc.ShouldOfferMigration());
        }

        [Test]
        public void AFailedMove_LeavesThatSaveLocal_ReportsIt_AndIsNotRepeated()
        {
            new SaveService(_root).Save(0, State(11));
            var svc = Service(_cloud);
            Assert.IsTrue(svc.ShouldOfferMigration());
            _cloud.FailWrites = true;

            Assert.AreEqual(0, svc.MigrateLocalToCloud(out var failed));
            Assert.AreEqual(1, failed);
            Assert.AreEqual(SaveLocation.Local, svc.Summarize(0).Location);
            Assert.AreEqual(11, GoldOf(svc, 0));
            Assert.IsFalse(svc.ShouldOfferMigration());
        }

        [Test]
        public void SavesAlreadyInTheCloud_AreNotOfferedAgain()
        {
            var svc = Service(_cloud);
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            Assert.IsFalse(svc.ShouldOfferMigration());
        }

        // ---- the QA folder cloud (development builds) -----------------------------------------------------------------------

        [Test]
        public void TwoComputersSharingAFolderCloud_SeeEachOthersGames()
        {
            var shared = Path.Combine(_root, "shared-cloud");
            var a = new SaveService(Path.Combine(_root, "pc-a"), null, () => new FolderCloudStorage(shared));
            var b = new SaveService(Path.Combine(_root, "pc-b"), null, () => new FolderCloudStorage(shared));

            a.StartSlot(1, SaveLocation.Cloud);
            a.Save(1, State(500));
            b.SyncAll();
            Assert.AreEqual(500, GoldOf(b, 1));
            Assert.AreEqual(SaveLocation.Cloud, b.Summarize(1).Location);

            b.Save(1, State(600));
            File.SetLastWriteTimeUtc(Path.Combine(shared, "saves", "slot1.json"), DateTime.UtcNow.AddMinutes(1));
            Assert.AreEqual(600, GoldOf(a, 1), "loading syncs first, so the newer cloud copy is the one played");

        }

        [Test]
        public void TheFolderCloud_ReadsWritesAndDeletes()
        {
            var cloud = new FolderCloudStorage(Path.Combine(_root, "folder"));
            Assert.IsFalse(cloud.Exists("saves/slot0.json"));
            Assert.IsTrue(cloud.TryWrite("saves/slot0.json", "{\"a\":1}"));
            Assert.IsTrue(cloud.TryRead("saves/slot0.json", out var text));
            Assert.AreEqual("{\"a\":1}", text);
            Assert.Greater(cloud.ModifiedUtc("saves/slot0.json"), DateTime.MinValue);
            Assert.IsTrue(cloud.TryDelete("saves/slot0.json"));
            Assert.IsTrue(cloud.TryDelete("saves/slot0.json"), "deleting what is not there is not a failure");
            Assert.IsFalse(cloud.TryRead("saves/slot0.json", out _));
        }
    }
}
