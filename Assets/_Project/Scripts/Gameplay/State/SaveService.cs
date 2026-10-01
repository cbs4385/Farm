using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Farm.Gameplay
{
    // Upgrades raw save JSON from FromVersion to FromVersion + 1. Never edit a shipped migration; add a new one.
    public interface ISaveMigration
    {
        int FromVersion { get; }
        void Migrate(JObject root);
    }

    public sealed class SlotSummary
    {
        public int Slot;
        public bool Exists;
        public string FarmName;
        public string PlayerName;
        public int Year;
        public int SeasonIndex;
        public int Day;
        public int Gold;
    }

    // Three save slots under <root>/saves/slotN/save.json with atomic writes and a .bak fallback.
    public sealed class SaveService
    {
        public const int SlotCount = 3;
        const string FileName = "save.json";

        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Include,
        };

        readonly string _root;
        readonly List<ISaveMigration> _migrations;

        public SaveService(string rootDirectory, IEnumerable<ISaveMigration> migrations = null)
        {
            _root = Path.Combine(rootDirectory, "saves");
            _migrations = (migrations ?? Enumerable.Empty<ISaveMigration>()).OrderBy(m => m.FromVersion).ToList();
        }

        public string SlotPath(int slot)
        {
            if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(_root, $"slot{slot}", FileName);
        }

        public bool Exists(int slot) => File.Exists(SlotPath(slot)) || File.Exists(SlotPath(slot) + ".bak");

        public DateTime LastWriteUtc(int slot) =>
            File.Exists(SlotPath(slot)) ? File.GetLastWriteTimeUtc(SlotPath(slot)) : DateTime.MinValue;

        public void Save(int slot, GameState state)
        {
            state.SaveVersion = GameState.CurrentVersion;
            AtomicFile.Write(SlotPath(slot), JsonConvert.SerializeObject(state, JsonSettings));
        }

        public bool TryLoad(int slot, out GameState state, out string error)
        {
            state = null;
            error = null;
            var text = AtomicFile.Read(SlotPath(slot), IsParsable, out var usedBackup);
            if (text == null)
            {
                error = "No readable save in this slot.";
                return false;
            }

            try
            {
                var root = JObject.Parse(text);
                var version = root.Value<int?>(nameof(GameState.SaveVersion)) ?? 0;
                if (version > GameState.CurrentVersion)
                {
                    error = $"Save was created by a newer game version ({version}).";
                    return false;
                }

                while (version < GameState.CurrentVersion)
                {
                    var migration = _migrations.FirstOrDefault(m => m.FromVersion == version);
                    if (migration == null)
                    {
                        error = $"No migration from save version {version}.";
                        return false;
                    }
                    migration.Migrate(root);
                    version++;
                    root[nameof(GameState.SaveVersion)] = version;
                }

                state = root.ToObject<GameState>(JsonSerializer.Create(JsonSettings));
                if (usedBackup) Log.Warn($"Save slot {slot} was corrupt; loaded the backup.");
                return state != null;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public SlotSummary Summarize(int slot)
        {
            var summary = new SlotSummary { Slot = slot };
            var text = AtomicFile.Read(SlotPath(slot), IsParsable, out _);
            if (text == null) return summary;
            try
            {
                var root = JObject.Parse(text);
                summary.Exists = true;
                summary.FarmName = root.Value<string>(nameof(GameState.FarmName));
                summary.PlayerName = root.Value<string>(nameof(GameState.PlayerName));
                summary.Year = root.Value<int?>(nameof(GameState.Year)) ?? 1;
                summary.SeasonIndex = root.Value<int?>(nameof(GameState.SeasonIndex)) ?? 0;
                summary.Day = root.Value<int?>(nameof(GameState.Day)) ?? 1;
                summary.Gold = root.Value<int?>(nameof(GameState.Gold)) ?? 0;
            }
            catch (Exception)
            {
                summary.Exists = false;
            }
            return summary;
        }

        public void Delete(int slot)
        {
            var dir = Path.GetDirectoryName(SlotPath(slot));
            if (dir != null && Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        static bool IsParsable(string text)
        {
            try { JObject.Parse(text); return true; }
            catch (Exception) { return false; }
        }
    }
}
