#if UNITY_EDITOR || DEVELOPMENT_BUILD
// Developer and QA tools (task T-043). This file is compiled only in the Editor and in development builds; a release
// build must not contain it. BuildScript scans release output for the marker names below and fails the build if any
// are found: DebugCommandProcessor, DebugConsoleScreen.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    public sealed class DebugCommandResult
    {
        public bool Ok;
        public string Message;
        public bool ReloadScene;     // the console reloads the current map so the world matches the changed state

        public static DebugCommandResult Success(string message, bool reload = false) =>
            new DebugCommandResult { Ok = true, Message = message, ReloadScene = reload };

        public static DebugCommandResult Fail(string message) => new DebugCommandResult { Ok = false, Message = message };
    }

    // Parses and runs developer commands against the running game. Pure with respect to Unity UI: the console screen
    // only collects text and shows results, so every command is unit-testable.
    public sealed class DebugCommandProcessor
    {
        const int MaxDaysToSkip = 4 * GameDateTime.DaysPerYear;   // safety limit for date jumps

        readonly GameSession _session;
        readonly Func<string, bool> _sceneExists;
        readonly Dictionary<string, (string usage, string help, Func<string[], DebugCommandResult> run)> _commands;

        // `sceneExists` decides whether a map scene can be loaded; tests supply their own, the game asks Unity.
        public DebugCommandProcessor(GameSession session, Func<string, bool> sceneExists = null)
        {
            _session = session;
            _sceneExists = sceneExists ?? (name => Application.CanStreamedLevelBeLoaded(name));
            _commands = new Dictionary<string, (string, string, Func<string[], DebugCommandResult>)>(StringComparer.OrdinalIgnoreCase)
            {
                ["help"] = ("help", "list the commands", _ => Help()),
                ["time"] = ("time <HH:MM>", "set the time of day (00:00-01:59 mean after midnight)", Time),
                ["skip"] = ("skip <minutes>", "advance the clock by game minutes", Skip),
                ["day"] = ("day [N]", "sleep through N days (default 1), running the overnight logic each time", Days),
                ["date"] = ("date <season> [day]", "jump forward to a season and day (spring 1 ... winter 28)", Date),
                ["xp"] = ("xp <skill> <amount>", "add skill XP (farming, foraging, mining, fishing, combat)", AddXp),
                ["weather"] = ("weather <id>", "set today's weather (for example sunny, rain)", Weather),
                ["flag"] = ("flag <id> [on|off]", "set or clear a story flag", Flag),
                ["var"] = ("var <name> <value|+N|-N>", "set or change a story variable", Var),
                ["gold"] = ("gold <amount|+N|-N>", "set or change gold", Gold),
                ["energy"] = ("energy <amount|full>", "set energy", Energy),
                ["give"] = ("give <itemId> [count]", "add items to the backpack", Give),
                ["hold"] = ("hold <itemId>", "select an item on the hotbar (adding one if needed), to see it carried", Hold),
                ["tp"] = ("tp <MapId> [spawn]", "go to a map (Farm, FarmHouse...)", Teleport),
                ["animal"] = ("animal <chicken|duck|rabbit|cow|goat|sheep>", "add an animal to the coop or barn you are in (tp Coop or tp Barn first)", AddAnimal),
                ["floor"] = ("floor <1-40>", "go to a mine floor", MineFloor),
                ["sleep"] = ("sleep", "start the real sleep flow (fade, summary, wake in bed)", Sleep),
                ["save"] = ("save", "save the game to the active slot", Save),
                ["pseudoloc"] = ("pseudoloc [on|off]", "show every string pseudo-localised (accented, 35% longer, bracketed) to find layout problems", PseudoLocCommand),
                ["state"] = ("state", "print the date, weather, gold, energy and map", PrintState),
            };
            // Narrative tools (T-137): hearts, mood, the talk pool and why, scenes, reactions, topics, social actions, coverage.
            foreach (var (name, usage, help, run) in NarrativeDebug.Commands(session)) _commands[name] = (usage, help, run);
        }

        static readonly Func<string, string, string> PseudoFilter = (key, text) => PseudoLoc.Apply(text);
        static bool _pseudo;

        DebugCommandResult PseudoLocCommand(string[] a)
        {
            var on = a.Length == 0 ? !_pseudo : a[0].ToLowerInvariant() == "on";
            if (on == _pseudo) return DebugCommandResult.Success($"Pseudo-localisation is already {(on ? "on" : "off")}.");
            _pseudo = on;
            if (on) L.AddFilter(PseudoFilter); else L.RemoveFilter(PseudoFilter);
            return DebugCommandResult.Success($"Pseudo-localisation {(on ? "on" : "off")}. Open screens again to see it.");
        }

        public IReadOnlyCollection<string> CommandNames => _commands.Keys;

        public DebugCommandResult Execute(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return DebugCommandResult.Success(string.Empty);
            var parts = line.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (!_commands.TryGetValue(parts[0], out var command))
                return DebugCommandResult.Fail($"Unknown command '{parts[0]}'. Type help.");
            if (!_session.InGame && parts[0].ToLowerInvariant() != "help")
                return DebugCommandResult.Fail("No game is running.");
            try { return command.run(parts.Skip(1).ToArray()); }
            catch (Exception e) { return DebugCommandResult.Fail($"{parts[0]} failed: {e.Message}"); }
        }

        // ---- commands ------------------------------------------------------------------------------------------

        DebugCommandResult Help()
        {
            var sb = new StringBuilder();
            foreach (var kv in _commands.OrderBy(k => k.Key))
                sb.AppendLine($"{kv.Value.usage}  -  {kv.Value.help}");
            return DebugCommandResult.Success(sb.ToString().TrimEnd());
        }

        DebugCommandResult Time(string[] a)
        {
            if (a.Length != 1 || !TryParseClock(a[0], out var minute)) return DebugCommandResult.Fail("Usage: time <HH:MM>, for example time 22:30");
            _session.Clock.SetTime(_session.Clock.Now.WithMinuteOfDay(minute));
            _session.NotifyChanged();
            return DebugCommandResult.Success($"Time is now {_session.Clock.Now.ClockString()}.");
        }

        public static bool TryParseClock(string text, out int minuteOfDay)
        {
            minuteOfDay = 0;
            var p = text.Split(':');
            if (p.Length != 2 || !int.TryParse(p[0], out var h) || !int.TryParse(p[1], out var m)) return false;
            if (h < 0 || h > 30 || m < 0 || m > 59) return false;
            if (h < GameDateTime.DayStartMinute / 60) h += 24;     // 00:00-05:59 are the small hours after midnight
            minuteOfDay = Math.Max(GameDateTime.DayStartMinute, Math.Min(GameDateTime.DayEndMinute, h * 60 + m));
            return true;
        }

        DebugCommandResult Skip(string[] a)
        {
            if (a.Length != 1 || !int.TryParse(a[0], out var minutes) || minutes <= 0) return DebugCommandResult.Fail("Usage: skip <minutes>");
            _session.Clock.AdvanceMinutes(minutes);
            _session.NotifyChanged();
            return DebugCommandResult.Success($"Time is now {_session.Clock.Now.ClockString()}.");
        }

        DebugCommandResult Days(string[] a)
        {
            var count = 1;
            if (a.Length > 0 && (!int.TryParse(a[0], out count) || count < 1 || count > MaxDaysToSkip))
                return DebugCommandResult.Fail($"Usage: day [1-{MaxDaysToSkip}]");
            for (var i = 0; i < count; i++) _session.EndDay(false);
            return DebugCommandResult.Success($"Now {_session.Clock.Now}.", reload: true);
        }

        DebugCommandResult Date(string[] a)
        {
            if (a.Length < 1 || !Enum.TryParse<Season>(a[0], true, out var season)) return DebugCommandResult.Fail("Usage: date <spring|summer|fall|winter> [day 1-28]");
            var day = 1;
            if (a.Length > 1 && (!int.TryParse(a[1], out day) || day < 1 || day > GameDateTime.DaysPerSeason)) return DebugCommandResult.Fail("Day must be 1-28.");

            var steps = 0;
            while (!(_session.Clock.Now.Season == season && _session.Clock.Now.Day == day))
            {
                if (steps++ >= MaxDaysToSkip) return DebugCommandResult.Fail("Could not reach that date.");
                _session.EndDay(false);
            }
            return DebugCommandResult.Success($"Now {_session.Clock.Now}. Skipped {steps} day(s).", reload: steps > 0);
        }

        DebugCommandResult AddXp(string[] a)
        {
            if (a.Length != 2 || !int.TryParse(a[1], out var amount) || amount < 1) return DebugCommandResult.Fail("Usage: xp <skill> <amount>");
            var skill = a[0].ToLowerInvariant();
            if (!SkillModel.IsKnown(skill)) return DebugCommandResult.Fail($"Unknown skill '{skill}'. Known: {string.Join(", ", SkillIds.All)}");
            _session.AddSkillXp(skill, amount);
            return DebugCommandResult.Success($"{skill} is now level {_session.GetSkillLevel(skill)} ({_session.GetSkillXp(skill)} XP).");
        }

        DebugCommandResult Weather(string[] a)
        {
            if (a.Length != 1) return DebugCommandResult.Fail("Usage: weather <id>");
            var id = a[0].ToLowerInvariant();
            if (!_session.Weather.Contains(id))
                return DebugCommandResult.Fail($"Unknown weather '{id}'. Known: {string.Join(", ", _session.Weather.Ids)}");
            _session.State.Weather = id;
            if (_session.Weather.Get(id).WateringCrops) foreach (var g in _session.Grids.Values) g.WaterAll();
            _session.NotifyChanged();
            return DebugCommandResult.Success($"Weather is now {_session.State.Weather}.", reload: true);
        }

        DebugCommandResult Flag(string[] a)
        {
            if (a.Length < 1 || a.Length > 2) return DebugCommandResult.Fail("Usage: flag <id> [on|off]");
            var on = true;
            if (a.Length == 2)
            {
                if (a[1].Equals("on", StringComparison.OrdinalIgnoreCase)) on = true;
                else if (a[1].Equals("off", StringComparison.OrdinalIgnoreCase)) on = false;
                else return DebugCommandResult.Fail("Use on or off.");
            }
            _session.SetFlag(a[0], on);
            return DebugCommandResult.Success($"Flag {a[0]} is {(_session.HasFlag(a[0]) ? "on" : "off")}.");
        }

        DebugCommandResult Var(string[] a)
        {
            if (a.Length != 2) return DebugCommandResult.Fail("Usage: var <name> <value|+N|-N>");
            if (!TryParseAmount(a[1], _session.GetVar(a[0]), out var value)) return DebugCommandResult.Fail("The value must be a whole number, optionally with + or -.");
            _session.SetVar(a[0], value);
            return DebugCommandResult.Success($"{a[0]} = {_session.GetVar(a[0])}.");
        }

        DebugCommandResult Gold(string[] a)
        {
            if (a.Length != 1 || !TryParseAmount(a[0], _session.State.Gold, out var value) || value < 0) return DebugCommandResult.Fail("Usage: gold <amount|+N|-N> (not below 0)");
            _session.AddGold(value - _session.State.Gold);
            return DebugCommandResult.Success($"Gold is now {_session.State.Gold}.");
        }

        DebugCommandResult Energy(string[] a)
        {
            if (a.Length != 1) return DebugCommandResult.Fail("Usage: energy <amount|full>");
            if (a[0].Equals("full", StringComparison.OrdinalIgnoreCase)) _session.SetEnergy(_session.State.MaxEnergy);
            else if (TryParseAmount(a[0], _session.State.Energy, out var value)) _session.SetEnergy(value);
            else return DebugCommandResult.Fail("Usage: energy <amount|full>");
            return DebugCommandResult.Success($"Energy is now {_session.State.Energy}/{_session.State.MaxEnergy}.");
        }

        DebugCommandResult Give(string[] a)
        {
            if (a.Length < 1 || a.Length > 2) return DebugCommandResult.Fail("Usage: give <itemId> [count]");
            var count = 1;
            if (a.Length == 2 && (!int.TryParse(a[1], out count) || count < 1)) return DebugCommandResult.Fail("The count must be 1 or more.");
            if (!_session.Db.TryGetItem(a[0], out var item)) return DebugCommandResult.Fail($"Unknown item '{a[0]}'.");
            var left = _session.Backpack.Add(item.Id, count);
            return left == 0
                ? DebugCommandResult.Success($"Gave {count} x {item.Id}.")
                : DebugCommandResult.Success($"Gave {count - left} x {item.Id}; the backpack is full ({left} did not fit).");
        }

        DebugCommandResult Hold(string[] a)
        {
            if (a.Length != 1) return DebugCommandResult.Fail("Usage: hold <itemId>");
            if (!_session.Db.TryGetItem(a[0], out var item)) return DebugCommandResult.Fail($"Unknown item '{a[0]}'.");
            if (!_session.Backpack.Has(item.Id) && _session.Backpack.Add(item.Id, 1) > 0) return DebugCommandResult.Fail("The backpack is full.");
            for (var i = 0; i < InputNames.HotbarSlots; i++)
            {
                var stack = _session.Backpack.Get(i);
                if (stack == null || stack.ItemId != item.Id) continue;
                _session.State.SelectedHotbar = i;
                return DebugCommandResult.Success($"Holding {item.Id}.");
            }
            return DebugCommandResult.Fail($"{item.Id} is in the backpack but not on the hotbar.");
        }

        DebugCommandResult Teleport(string[] a)
        {
            if (a.Length < 1 || a.Length > 2) return DebugCommandResult.Fail("Usage: tp <MapId> [spawn]");
            if (!_sceneExists(a[0])) return DebugCommandResult.Fail($"No map scene named '{a[0]}'.");
            _session.State.CurrentMap = a[0];
            _session.State.SpawnPoint = a.Length == 2 ? a[1] : "default";
            return DebugCommandResult.Success($"Going to {a[0]} ({_session.State.SpawnPoint}).", reload: true);
        }

        DebugCommandResult AddAnimal(string[] a)
        {
            if (a.Length != 1) return DebugCommandResult.Fail("Usage: animal <type>");
            var row = AnimalDefaults.Find(a[0]);
            if (row == null) return DebugCommandResult.Fail($"No animal called '{a[0]}'.");
            var building = row.Value.Building;
            if (_session.State.CurrentMap != building) return DebugCommandResult.Fail($"A {a[0]} lives in the {building}: tp {building} first.");
            if (!AnimalRules.HasRoom(_session.State, building)) return DebugCommandResult.Fail($"The {building} is full.");
            var count = AnimalRules.In(_session.State, building).Count;
            var animal = AnimalRules.Add(_session.State, row.Value.Id, Guid.NewGuid().ToString("N").Substring(0, 8), $"{row.Value.Name} {count + 1}");
            if (animal == null) return DebugCommandResult.Fail("Could not add it.");
            AnimalManager.Current?.Spawn(animal, 4 + count % 5 * 2, 4 + count / 5 * 2);
            return DebugCommandResult.Success($"Added a {row.Value.Name} to the {building}.");
        }

        DebugCommandResult MineFloor(string[] a)
        {
            if (a.Length != 1 || !int.TryParse(a[0], out var floor) || floor < 1 || floor > 40) return DebugCommandResult.Fail("Usage: floor <1-40>");
            if (!_sceneExists(MapIds.Mine)) return DebugCommandResult.Fail("There is no mine scene.");
            _session.State.Mine.Floor = floor;
            _session.State.CurrentMap = MapIds.Mine;
            _session.State.SpawnPoint = "default";
            return DebugCommandResult.Success($"Going to mine floor {floor}.", reload: true);
        }

        DebugCommandResult Sleep(string[] a)
        {
            _session.StartSleep(false);
            return DebugCommandResult.Success("Going to sleep.");
        }

        DebugCommandResult Save(string[] a) =>
            _session.Save() ? DebugCommandResult.Success("Saved.") : DebugCommandResult.Fail("Not saved (a dev game has no slot).");

        DebugCommandResult PrintState(string[] a)
        {
            var s = _session.State;
            return DebugCommandResult.Success($"{_session.Clock.Now} | weather {s.Weather} | gold {s.Gold} | energy {s.Energy}/{s.MaxEnergy} | map {s.CurrentMap} | luck {_session.Luck:0.00}");
        }

        // "250" sets, "+25" adds, "-25" subtracts.
        static bool TryParseAmount(string text, int current, out int value)
        {
            value = 0;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return false;
            value = text.StartsWith("+") || text.StartsWith("-") ? current + n : n;
            return true;
        }
    }
}
#endif
