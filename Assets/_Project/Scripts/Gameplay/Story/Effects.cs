using System;
using System.Collections.Generic;
using System.Globalization;
using Farm.Core;

namespace Farm.Gameplay
{
    // Effects are small strings in data ("flag:met_tilda", "gold:-50", "give:crop.parsnip,3"): `verb:arg,arg`.
    // Dialogue nodes, choices, events, quests, letters and random events all run them. Modules add their own
    // verbs with Effects.Register; the data validator knows every registered verb and its argument count.
    public delegate void EffectHandler(GameSession session, string[] args);

    public static class Effects
    {
        sealed class Verb
        {
            public EffectHandler Handler; public int MinArgs; public int MaxArgs;
        }

        static readonly Dictionary<string, Verb> Verbs = new Dictionary<string, Verb>();
        static bool _defaultsRegistered;

        public static void Register(string verb, int minArgs, int maxArgs, EffectHandler handler)
        {
            if (string.IsNullOrEmpty(verb)) throw new ArgumentException("verb required", nameof(verb));
            Verbs[verb] = new Verb { Handler = handler ?? throw new ArgumentNullException(nameof(handler)), MinArgs = minArgs, MaxArgs = maxArgs };
        }

        public static bool IsKnown(string verb) { EnsureDefaults(); return Verbs.ContainsKey(verb); }

        public static IEnumerable<string> KnownVerbs { get { EnsureDefaults(); return Verbs.Keys; } }

        public static bool TrySplit(string effect, out string verb, out string[] args)
        {
            verb = null; args = null;
            if (string.IsNullOrWhiteSpace(effect)) return false;
            var colon = effect.IndexOf(':');
            if (colon <= 0) { verb = effect.Trim(); args = new string[0]; return true; }
            verb = effect.Substring(0, colon).Trim();
            var rest = effect.Substring(colon + 1);
            args = rest.Length == 0 ? new string[0] : rest.Split(',');
            for (var i = 0; i < args.Length; i++) args[i] = args[i].Trim();
            return true;
        }

        // For the data validator: a well formed effect with a known verb and a sensible argument count.
        public static bool Validate(string effect, out string error)
        {
            EnsureDefaults();
            error = null;
            if (!TrySplit(effect, out var verb, out var args)) { error = "empty effect"; return false; }
            if (!Verbs.TryGetValue(verb, out var v)) { error = $"unknown effect '{verb}' in '{effect}'"; return false; }
            if (args.Length < v.MinArgs || args.Length > v.MaxArgs)
            {
                error = $"effect '{effect}' needs {v.MinArgs}..{v.MaxArgs} arguments";
                return false;
            }
            return true;
        }

        // Runs one effect. A bad or failing effect is logged and skipped: story data must never break the game.
        public static void Run(GameSession session, string effect)
        {
            EnsureDefaults();
            if (!Validate(effect, out var error)) { Log.Error("Effect rejected: " + error); return; }
            TrySplit(effect, out var verb, out var args);
            try { Verbs[verb].Handler(session, args); }
            catch (Exception e) { Log.Error($"Effect '{effect}' failed: {e}"); }
        }

        public static void RunAll(GameSession session, IEnumerable<string> effects)
        {
            if (effects == null) return;
            foreach (var e in effects) Run(session, e);
        }

        public static int Int(string text, int fallback = 0) =>
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : fallback;

        static void EnsureDefaults()
        {
            if (_defaultsRegistered) return;
            _defaultsRegistered = true;
            Register("flag", 1, 1, (s, a) => s.SetFlag(a[0]));
            Register("unflag", 1, 1, (s, a) => s.SetFlag(a[0], false));
            Register("setvar", 2, 2, (s, a) => s.SetVar(a[0], Int(a[1])));
            Register("addvar", 2, 4, (s, a) => s.AddVar(a[0], Int(a[1]),
                a.Length > 2 ? Int(a[2], int.MinValue) : int.MinValue, a.Length > 3 ? Int(a[3], int.MaxValue) : int.MaxValue));
            Register("gold", 1, 1, (s, a) => s.ChangeGold(Int(a[0])));
            Register("give", 1, 3, (s, a) => { s.GiveItem(a[0], a.Length > 1 ? Int(a[1], 1) : 1, a.Length > 2 ? Int(a[2]) : 0); AudioService.PlayIfAvailable(Sfx.Pickup); });
            Register("parcel", 1, 2, (s, a) => { if (s.Db.TryGetItem(a[0], out _)) Parcels.Send(s, a[0], a.Length > 1 ? Int(a[1], 1) : 1); else Log.Error($"parcel: unknown item '{a[0]}'"); });
            Register("take", 2, 2, (s, a) => s.Backpack.Remove(a[0], Int(a[1], 1)));
            Register("toast", 1, 1, (s, a) => s.Toast(L.Get(a[0])));
            Register("xp", 2, 2, (s, a) => s.AddSkillXp(a[0], Int(a[1])));
            StoryEffects.RegisterAll();
        }

        public static void ResetForTests() { Verbs.Clear(); _defaultsRegistered = false; }
    }
}
