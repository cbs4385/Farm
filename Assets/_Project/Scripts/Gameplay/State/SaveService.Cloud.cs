using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Farm.Gameplay
{
    // Cloud saves (docs/RELEASE.md, "Cloud saves"). The local file under saves/slotN/ is always written and is the working copy;
    // a slot whose location is Cloud is also pushed to the store's cloud storage as saves/slotN.json after every save.
    // Without cloud (no Steam, cloud switched off, offline) everything silently stays local and catches up at the next sync.
    //
    // Conflict rule (SyncSlot): when the local and the cloud copy of a slot differ, the NEWER one wins (a copy that is not valid
    // JSON always loses; a tie goes to the cloud). The loser is kept as saves/slotN/conflict-<time>.json (the last three are kept),
    // so nothing is lost for good.
    public sealed partial class SaveService
    {
        const string LocationFile = "location.json";
        const string PrefsFile = "cloud.json";
        const string ConflictPrefix = "conflict-";
        const int ConflictsKept = 3;

        sealed class LocationData { public string Location; }

        sealed class CloudPrefs
        {
            public bool MigrationAnswered;
            public List<int> PendingDeletes = new List<int>();
            public List<int> PendingUploads = new List<int>();     // slots whose newest local save did not reach the cloud: the local copy wins at the next sync
        }

        // Told the key of a short sentence (Resources/Localization/en.json) when a cloud game could not be kept in the cloud; GameServices turns
        // it into a toast. Said once per trouble: it is armed again by the next successful cloud write.
        public Action<string> CloudNotice;
        bool _noticeGiven;

        void NoticeCloudProblem(string key)
        {
            if (_noticeGiven) return;
            _noticeGiven = true;
            try { CloudNotice?.Invoke(key); }
            catch (Exception e) { Log.Warn($"Cloud notice failed: {e.Message}"); }
        }

        public static string CloudName(int slot) => $"saves/slot{slot}.json";

        ICloudStorage Cloud
        {
            get
            {
                try
                {
                    var cloud = _cloud?.Invoke();
                    return cloud != null && cloud.IsAvailable ? cloud : null;
                }
                catch (Exception e) { Log.Warn($"Cloud storage check failed: {e.Message}"); return null; }
            }
        }

        // Whether the player can choose cloud saves at all right now.
        public bool CloudAvailable => Cloud != null;

        // ---- location ----------------------------------------------------------------------------------------------------

        string LocationPath(int slot) => Path.Combine(Path.GetDirectoryName(SlotPath(slot)) ?? ".", LocationFile);

        public SaveLocation GetLocation(int slot)
        {
            try
            {
                var path = LocationPath(slot);
                if (!File.Exists(path)) return SaveLocation.Local;
                var data = JsonConvert.DeserializeObject<LocationData>(File.ReadAllText(path));
                return data != null && string.Equals(data.Location, "cloud", StringComparison.OrdinalIgnoreCase) ? SaveLocation.Cloud : SaveLocation.Local;
            }
            catch (Exception) { return SaveLocation.Local; }
        }

        void WriteLocation(int slot, SaveLocation location)
        {
            try
            {
                AtomicFile.Write(LocationPath(slot), JsonConvert.SerializeObject(new LocationData { Location = location == SaveLocation.Cloud ? "cloud" : "local" }));
            }
            catch (Exception e) { Log.Warn($"Could not record the location of slot {slot}: {e.Message}"); }
        }

        // Prepares a slot for a NEW game: removes what was there (also from the cloud) and records where the new game will live.
        // Cloud is honoured only while cloud storage is available; otherwise the slot silently becomes local.
        public SaveLocation StartSlot(int slot, SaveLocation wanted)
        {
            Delete(slot);
            var location = wanted == SaveLocation.Cloud && CloudAvailable ? SaveLocation.Cloud : SaveLocation.Local;
            WriteLocation(slot, location);
            return location;
        }

        // ---- push, delete -------------------------------------------------------------------------------------------------

        void PushToCloud(int slot, string json)
        {
            if (GetLocation(slot) != SaveLocation.Cloud) return;
            var cloud = Cloud;
            if (cloud == null)                                                // offline: the next sync pushes the newer local copy
            {
                SetPendingUpload(slot, true);
                NoticeCloudProblem("cloud.err.offline");
                return;
            }
            if (cloud.TryWrite(CloudName(slot), json)) { ForgetPendingDelete(slot); SetPendingUpload(slot, false); _noticeGiven = false; }
            else
            {
                SetPendingUpload(slot, true);
                NoticeCloudProblem(cloud.LastFailure == CloudFailure.QuotaExceeded ? "cloud.err.quota" : "cloud.err.write");
                Log.Warn($"Save slot {slot} was kept locally; the cloud copy could not be written.");
            }
        }

        void DeleteFromCloud(int slot)
        {
            SetPendingUpload(slot, false);
            var cloud = Cloud;
            var wasCloud = GetLocation(slot) == SaveLocation.Cloud;
            if (cloud != null)
            {
                if (!cloud.TryDelete(CloudName(slot))) AddPendingDelete(slot);
            }
            else if (wasCloud) AddPendingDelete(slot);                   // cannot reach the cloud now: delete it as soon as it can be
        }

        // ---- sync --------------------------------------------------------------------------------------------------------

        public void SyncAll()
        {
            for (var i = 0; i < SlotCount; i++) SyncSlot(i);
        }

        public void SyncSlot(int slot)
        {
            var cloud = Cloud;
            if (cloud == null) return;
            try
            {
                var name = CloudName(slot);
                var prefs = LoadPrefs();
                if (prefs.PendingDeletes.Contains(slot))
                {
                    if (cloud.TryDelete(name)) ForgetPendingDelete(slot);
                    return;
                }

                var path = SlotPath(slot);
                var localHas = File.Exists(path);
                var cloudHas = cloud.Exists(name);
                var location = GetLocation(slot);
                if (location != SaveLocation.Cloud && !cloudHas) return;          // a purely local slot
                if (!localHas && !cloudHas) return;

                if (localHas && !cloudHas)
                {
                    if (cloud.TryWrite(name, File.ReadAllText(path)) && location != SaveLocation.Cloud) WriteLocation(slot, SaveLocation.Cloud);
                    return;
                }

                if (prefs.PendingUploads.Contains(slot))
                {
                    // The newest save never reached the cloud (timestamps cannot be trusted to show that): the local copy is the newest.
                    if (cloud.TryWrite(name, File.ReadAllText(path))) SetPendingUpload(slot, false);
                    return;
                }

                if (!cloud.TryRead(name, out var cloudText)) return;
                var cloudTime = cloud.ModifiedUtc(name);

                if (!localHas)
                {
                    AtomicFile.Write(path, cloudText);
                    if (cloudTime != DateTime.MinValue) File.SetLastWriteTimeUtc(path, cloudTime);
                    WriteLocation(slot, SaveLocation.Cloud);
                    return;
                }

                var localText = File.ReadAllText(path);
                if (location != SaveLocation.Cloud) WriteLocation(slot, SaveLocation.Cloud);
                if (localText == cloudText) return;

                var localTime = File.GetLastWriteTimeUtc(path);
                var cloudWins = ChooseCloud(localText, cloudText, localTime, cloudTime);
                KeepConflictBackup(slot, cloudWins ? localText : cloudText);
                if (cloudWins)
                {
                    AtomicFile.Write(path, cloudText);
                    if (cloudTime != DateTime.MinValue) File.SetLastWriteTimeUtc(path, cloudTime);
                }
                else cloud.TryWrite(name, localText);
                Log.Warn($"Save slot {slot}: the local and the cloud copy differed; the {(cloudWins ? "cloud" : "local")} copy was newer and was kept (the other one is saved as a conflict backup).");
            }
            catch (Exception e) { Log.Warn($"Cloud sync of slot {slot} failed: {e.Message}"); }
        }

        // The conflict rule, in one place for the tests.
        public static bool ChooseCloud(string localText, string cloudText, DateTime localUtc, DateTime cloudUtc)
        {
            var localValid = IsParsableQuiet(localText);
            var cloudValid = IsParsableQuiet(cloudText);
            if (localValid != cloudValid) return cloudValid;
            return cloudUtc >= localUtc;
        }

        static bool IsParsableQuiet(string text)
        {
            try { JObject.Parse(text); return true; }
            catch (Exception) { return false; }
        }

        void KeepConflictBackup(int slot, string text)
        {
            var dir = Path.GetDirectoryName(SlotPath(slot));
            if (dir == null) return;
            var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            AtomicFile.Write(Path.Combine(dir, $"{ConflictPrefix}{stamp}.json"), text);
            foreach (var old in Directory.GetFiles(dir, ConflictPrefix + "*.json").OrderByDescending(f => f, StringComparer.Ordinal).Skip(ConflictsKept))
                File.Delete(old);
        }

        public IReadOnlyList<string> ConflictBackups(int slot)
        {
            var dir = Path.GetDirectoryName(SlotPath(slot));
            return dir != null && Directory.Exists(dir)
                ? Directory.GetFiles(dir, ConflictPrefix + "*.json").OrderBy(f => f, StringComparer.Ordinal).ToList()
                : new List<string>();
        }

        // ---- moving saves of an earlier version to the cloud (offered once) --------------------------------------------------

        // True when cloud saves are available, the player has never answered, and there are local saves to move. With nothing to
        // move there is nothing to ask, so the question counts as answered (new games choose their own location).
        public bool ShouldOfferMigration()
        {
            if (!CloudAvailable) return false;
            SyncAll();
            var prefs = LoadPrefs();
            if (prefs.MigrationAnswered) return false;
            if (LocalSaveSlots().Count > 0) return true;
            prefs.MigrationAnswered = true;
            SavePrefs(prefs);
            return false;
        }

        public void DeclineMigration()
        {
            var prefs = LoadPrefs();
            prefs.MigrationAnswered = true;
            SavePrefs(prefs);
        }

        // Moves every local save to the cloud. A slot that could not be uploaded stays local. The question is answered either way
        // and is not asked again. Returns how many moved; `failed` counts the ones that stayed local.
        public int MigrateLocalToCloud(out int failed)
        {
            failed = 0;
            var moved = 0;
            var cloud = Cloud;
            foreach (var slot in LocalSaveSlots())
            {
                var text = AtomicFile.Read(SlotPath(slot), IsParsableQuiet, out _);
                // The slot only counts as moved once the cloud hands the same text back; the local file is never touched here.
                if (cloud != null && text != null && cloud.TryWrite(CloudName(slot), text) && cloud.TryRead(CloudName(slot), out var back) && back == text)
                {
                    WriteLocation(slot, SaveLocation.Cloud);
                    ForgetPendingDelete(slot);
                    moved++;
                }
                else failed++;
            }
            DeclineMigration();
            return moved;
        }

        List<int> LocalSaveSlots()
        {
            var slots = new List<int>();
            for (var i = 0; i < SlotCount; i++)
                if (Exists(i) && GetLocation(i) == SaveLocation.Local) slots.Add(i);
            return slots;
        }

        // ---- preferences (the answer to the migration offer, deletes the cloud has not yet accepted) ------------------------

        string PrefsPath => Path.Combine(_dataRoot, PrefsFile);

        CloudPrefs LoadPrefs()
        {
            try
            {
                if (File.Exists(PrefsPath)) return JsonConvert.DeserializeObject<CloudPrefs>(File.ReadAllText(PrefsPath)) ?? new CloudPrefs();
            }
            catch (Exception e) { Log.Warn($"cloud.json unreadable: {e.Message}"); }
            return new CloudPrefs();
        }

        void SavePrefs(CloudPrefs prefs)
        {
            try { AtomicFile.Write(PrefsPath, JsonConvert.SerializeObject(prefs)); }
            catch (Exception e) { Log.Warn($"cloud.json could not be written: {e.Message}"); }
        }

        void SetPendingUpload(int slot, bool pending)
        {
            var prefs = LoadPrefs();
            if (pending ? prefs.PendingUploads.Contains(slot) : !prefs.PendingUploads.Contains(slot)) return;
            if (pending) prefs.PendingUploads.Add(slot); else prefs.PendingUploads.Remove(slot);
            SavePrefs(prefs);
        }

        void AddPendingDelete(int slot)
        {
            var prefs = LoadPrefs();
            if (prefs.PendingDeletes.Contains(slot)) return;
            prefs.PendingDeletes.Add(slot);
            SavePrefs(prefs);
        }

        void ForgetPendingDelete(int slot)
        {
            var prefs = LoadPrefs();
            if (prefs.PendingDeletes.Remove(slot)) SavePrefs(prefs);
        }
    }
}
