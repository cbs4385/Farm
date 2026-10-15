using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Farm.Data;
using Farm.Gameplay;
using Farm.Platform;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // Steam Cloud without Steam: SteamCloudStorage against a fake Remote Storage, the install decision, and what the player is told
    // when the cloud refuses a save. The real Steam client is not involved (docs/QA.md has the manual script).
    public class SteamCloudTests
    {
        sealed class FakeRemoteStorage : IRemoteStorageApi
        {
            public readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();
            public readonly Dictionary<string, long> Stamps = new Dictionary<string, long>();
            public bool Running = true, AccountOn = true, AppOn = true, FailWrites, Throws;
            public long Quota = 1000000, Used;
            public bool IsSteamRunning => Running;
            public bool IsCloudEnabledForAccount => AccountOn;
            public bool IsCloudEnabledForApp => AppOn;
            void Check() { if (Throws) throw new InvalidOperationException("steam"); }
            public bool FileExists(string name) { Check(); return Files.ContainsKey(name); }
            public int GetFileSize(string name) => Files[name].Length;
            public int FileRead(string name, byte[] buffer, int size) { Check(); Array.Copy(Files[name], buffer, Math.Min(size, Files[name].Length)); return Files[name].Length; }
            public bool FileWrite(string name, byte[] data, int size)
            {
                Check();
                if (FailWrites || Used + size > Quota) return false;
                var copy = new byte[size];
                Array.Copy(data, copy, size);
                Files[name] = copy;
                Stamps[name] = 1760000000;
                Used += size;
                return true;
            }
            public bool FileDelete(string name) { Check(); return Files.Remove(name); }
            public long GetFileTimestamp(string name) => Stamps[name];
            public bool GetQuota(out long total, out long available) { total = Quota; available = Quota - Used; return true; }
        }

        string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "farm-steam-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            PlatformServices.Reset();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        static GameState State(int gold)
        {
            var s = GameState.NewGame("Sam", "Meadow", id => 999);
            s.Gold = gold;
            return s;
        }

        [Test]
        public void Storage_RoundTripsTextAndTimestamp_AsUtf8WithoutBom()
        {
            var api = new FakeRemoteStorage();
            var cloud = new SteamCloudStorage(api);
            const string text = "{\"name\":\"Zoë\"}";
            Assert.IsTrue(cloud.TryWrite("saves/slot0.json", text));
            Assert.IsTrue(cloud.Exists("saves/slot0.json"));
            Assert.IsTrue(cloud.TryRead("saves/slot0.json", out var back));
            Assert.AreEqual(text, back);
            Assert.AreEqual((byte)'{', api.Files["saves/slot0.json"][0], "no byte order mark");
            Assert.AreEqual(new DateTime(2025, 10, 9, 8, 53, 20, DateTimeKind.Utc), cloud.ModifiedUtc("saves/slot0.json"));
            Assert.AreEqual(DateTime.MinValue, cloud.ModifiedUtc("saves/none.json"));
            Assert.IsTrue(cloud.TryDelete("saves/slot0.json"));
            Assert.IsFalse(cloud.Exists("saves/slot0.json"));
            Assert.IsTrue(cloud.TryDelete("saves/slot0.json"), "deleting what is gone is fine");
        }

        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public void Storage_IsUnavailable_WhenSteamOrEitherCloudSwitchIsOff(bool running, bool account, bool app)
        {
            var cloud = new SteamCloudStorage(new FakeRemoteStorage { Running = running, AccountOn = account, AppOn = app });
            Assert.IsFalse(cloud.IsAvailable);
            Assert.IsFalse(cloud.TryWrite("saves/slot0.json", "{}"));
            Assert.AreEqual(CloudFailure.Unavailable, cloud.LastFailure);
        }

        [Test]
        public void Storage_NeverThrows_WhenSteamDoes()
        {
            var cloud = new SteamCloudStorage(new FakeRemoteStorage { Throws = true });
            Assert.IsFalse(cloud.Exists("a"));
            Assert.AreEqual(DateTime.MinValue, cloud.ModifiedUtc("a"));
            Assert.IsFalse(cloud.TryRead("a", out _));
            Assert.IsFalse(cloud.TryWrite("a", "{}"));
            Assert.IsFalse(cloud.TryDelete("a"));
        }

        [Test]
        public void Storage_ReportsAFullQuota_AndAPlainFailure_Differently()
        {
            var full = new SteamCloudStorage(new FakeRemoteStorage { Quota = 5 });
            Assert.IsFalse(full.TryWrite("a", "0123456789"));
            Assert.AreEqual(CloudFailure.QuotaExceeded, full.LastFailure);

            var broken = new SteamCloudStorage(new FakeRemoteStorage { FailWrites = true });
            Assert.IsFalse(broken.TryWrite("a", "{}"));
            Assert.AreEqual(CloudFailure.WriteFailed, broken.LastFailure);

            var ok = new SteamCloudStorage(new FakeRemoteStorage());
            Assert.IsTrue(ok.TryWrite("a", "{}"));
            Assert.AreEqual(CloudFailure.None, ok.LastFailure);
        }

        // ---- choosing the Steam platform -------------------------------------------------------------------------------

        [Test]
        public void TheSteamPlatform_IsNotChosen_WhenSteamDoesNotStart()
        {
            PlatformServices.Reset();
            var created = false;
            Assert.IsFalse(SteamPlatformServices.TryInstall(() => false, () => { created = true; return null; }));
            Assert.IsFalse(created);
            Assert.AreEqual("none", PlatformServices.Current.Name);
            Assert.IsNull(PlatformServices.Current.Cloud);
        }

        [Test]
        public void TheSteamPlatform_IsNotChosen_WhenStartingSteamThrows()
        {
            PlatformServices.Reset();
            LogAssert.Expect(LogType.Warning, new Regex("Steam unavailable"));
            Assert.IsFalse(SteamPlatformServices.TryInstall(() => throw new DllNotFoundException("steam_api64"), () => null));
            Assert.AreEqual("none", PlatformServices.Current.Name);
        }

        [Test]
        public void TheSteamPlatform_IsChosen_WhenSteamStarts_AndServesTheCloud()
        {
            var unlocked = new List<string>();
            Assert.IsTrue(SteamPlatformServices.TryInstall(() => true, () => new SteamPlatformServices(new FakeRemoteStorage(), () => true, unlocked.Add)));
            Assert.AreEqual("steam", PlatformServices.Current.Name);
            Assert.IsTrue(PlatformServices.Current.Cloud.IsAvailable);
            PlatformServices.Current.UnlockAchievement("first_harvest");
            CollectionAssert.AreEqual(new[] { "first_harvest" }, unlocked);
        }

        [Test]
        public void TheSteamPlatform_SwallowsAchievementFailures_AndSkipsThemOffline()
        {
            var calls = 0;
            var online = true;
            var steam = new SteamPlatformServices(new FakeRemoteStorage(), () => online, id => { calls++; throw new InvalidOperationException(); });
            Assert.DoesNotThrow(() => steam.UnlockAchievement("x"));
            online = false;
            steam.UnlockAchievement("y");
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void BuildsCarrySteamSupport_UnlessAskedNot()
        {
            CollectionAssert.AreEqual(new[] { "FARM_STEAM" }, Farm.Editor.BuildScript.SteamDefines());
        }

        // ---- what the player is told -----------------------------------------------------------------------------------

        [Test]
        public void AFullCloud_KeepsTheGameLocal_AndTellsThePlayerOnce()
        {
            var api = new FakeRemoteStorage { Quota = 10 };
            var cloud = new SteamCloudStorage(api);
            var svc = new SaveService(_root, null, () => cloud);
            var notices = new List<string>();
            svc.CloudNotice = notices.Add;
            svc.StartSlot(0, SaveLocation.Cloud);
            svc.Save(0, State(1));
            svc.Save(0, State(2));
            CollectionAssert.AreEqual(new[] { "cloud.err.quota" }, notices, "told once, not at every save");
            Assert.IsTrue(svc.TryLoad(0, out var loaded, out _));
            Assert.AreEqual(2, loaded.Gold, "the local copy is what counts");

            api.Quota = 1000000;
            svc.Save(0, State(3));
            Assert.IsTrue(api.Files.ContainsKey(SaveService.CloudName(0)));
            api.Quota = 10; api.Used = 0; api.Files.Clear();
            svc.Save(0, State(4));
            Assert.AreEqual(2, notices.Count, "a new trouble after a success is told again");
        }

        [Test]
        public void WhenTheCloudIsOff_ACloudGameSaysSoOnce_AndStaysLocal()
        {
            var api = new FakeRemoteStorage();
            var cloud = new SteamCloudStorage(api);
            var svc = new SaveService(_root, null, () => cloud);
            var notices = new List<string>();
            svc.CloudNotice = notices.Add;
            svc.StartSlot(0, SaveLocation.Cloud);
            api.Running = false;
            svc.Save(0, State(1));
            svc.Save(0, State(2));
            CollectionAssert.AreEqual(new[] { "cloud.err.offline" }, notices);
            Assert.IsTrue(svc.Exists(0));
        }

        [Test]
        public void Migration_KeepsTheLocalSave_WhenTheCloudRefusesIt()
        {
            new SaveService(_root).Save(0, State(7));
            var svc = new SaveService(_root, null, () => new SteamCloudStorage(new FakeRemoteStorage { Quota = 3 }));
            Assert.AreEqual(0, svc.MigrateLocalToCloud(out var failed));
            Assert.AreEqual(1, failed);
            Assert.AreEqual(SaveLocation.Local, svc.GetLocation(0));
            Assert.IsTrue(svc.TryLoad(0, out var s, out _));
            Assert.AreEqual(7, s.Gold);
        }

        [Test]
        public void TheCloudNoticeKeys_ExistInTheEnglishStrings()
        {
            var text = File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Resources/Localization/en.json"));
            foreach (var key in new[] { "cloud.err.offline", "cloud.err.quota", "cloud.err.write" })
                StringAssert.Contains("\"" + key + "\"", text);
        }
    }
}
