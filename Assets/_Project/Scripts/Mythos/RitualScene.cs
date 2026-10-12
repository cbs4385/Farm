using System;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;

namespace Farm.Mythos
{
    // The ritual as a scene the player can watch (2026-10-10). The ritual itself is a system on the clock (RitualDirector); this is what the player sees when they
    // are in the wood at the start of it, at full intensity, from the first summer on: the story event `mythos_ritual_witness` reads the state through the `ritual:`
    // conditions below and moves the ritual along with the effects `ritual:begin`, `ritual:snatch` and `ritual:witness`. At mild there is no such scene (no ritual
    // imagery; the altar shows plain lights) and at off nothing.
    public static class RitualScene
    {
        public const string BeginEffect = "begin", SnatchEffect = "snatch", WitnessEffect = "witness";

        static GameSession _bound;

        // The session the `ritual:` conditions read (set when the module is installed; the game's own session otherwise).
        public static void Bind(GameSession session) => _bound = session;

        static bool Running(GameSession s, out MythosSave save)
        {
            save = null;
            if (!MythosLevel.Full(s) || !RitualDirector.IsRitualDay(s.Clock.Now)) return false;
            save = RitualDirector.Load(s);
            return !save.Sealed && save.Plan.Count > 0 && save.ResolvedSeason != save.PlannedSeason;
        }

        // The ritual has started and its leader is still speaking: the scene begins here.
        public static bool Opening(GameSession s)
        {
            if (!Running(s, out _)) return false;
            var offset = RitualDirector.Offset(s.Clock.Now.MinuteOfDay);
            return offset >= 0 && offset < RitualModel.LeaderMinutes;
        }

        // This season's ritual has not been watched yet.
        public static bool Fresh(GameSession s)
        {
            var save = RitualDirector.Load(s);
            return save.WitnessedSeason != save.PlannedSeason;
        }

        public static int Count(GameSession s) => RitualDirector.Load(s).Plan.Count;

        public static bool Succeeded(GameSession s)
        {
            var save = RitualDirector.Load(s);
            return save.ResolvedSeason == save.PlannedSeason && save.LastSucceeded;
        }

        public static bool Failed(GameSession s)
        {
            var save = RitualDirector.Load(s);
            return save.ResolvedSeason == save.PlannedSeason && !save.LastSucceeded;
        }

        public static bool Snatched(GameSession s) => RitualDirector.Load(s).Plan.Any(p => p.Taken);

        // The scene is the player's: this season's ritual counts as watched, so it never begins twice.
        public static void MarkWitnessed(GameSession s)
        {
            var save = RitualDirector.Load(s);
            save.WitnessedSeason = save.PlannedSeason;
            save.RitualsWitnessed++;
            RitualDirector.Store(s, save);
        }

        // Moves the clock on to the end of the leader's words, when the first offering is laid (a scene begun late takes the rest of the minutes).
        public static void Begin(GameSession s)
        {
            if (!Running(s, out _)) return;
            var offset = RitualDirector.Offset(s.Clock.Now.MinuteOfDay);
            if (offset < RitualModel.LeaderMinutes) s.Clock.AdvanceMinutes(RitualModel.LeaderMinutes - offset);
        }

        // The player steps out and takes the offering lying on the stone: the ritual fails (RitualDirector.TakeFromAltar). Returns whether they did.
        public static bool Snatch(GameSession s)
        {
            if (!Running(s, out _)) return false;
            var slot = RitualDirector.TakeFromAltar(s);
            if (slot != null) s.Toast(L.Get("mythos.altar.taken"));
            return slot != null;
        }

        // `ritual:<what>` in a condition: opening, fresh, succeeded, failed, snatched, count>=N, count==N (and the other comparisons).
        public static bool Condition(string arg)
        {
            var s = _bound;
            if (s == null) return false;
            if (!s.InGame) return false;
            switch (arg)
            {
                case "opening": return Opening(s);
                case "fresh": return Fresh(s);
                case "succeeded": return Succeeded(s);
                case "failed": return Failed(s);
                case "snatched": return Snatched(s);
            }
            if (arg.StartsWith("count", StringComparison.Ordinal))
            {
                var rest = arg.Substring("count".Length);
                foreach (var op in new[] { ">=", "<=", "==", ">", "<" })
                    if (rest.StartsWith(op, StringComparison.Ordinal) && int.TryParse(rest.Substring(op.Length), out var n))
                    {
                        var count = Count(s);
                        return op == ">=" ? count >= n : op == "<=" ? count <= n : op == "==" ? count == n : op == ">" ? count > n : count < n;
                    }
            }
            return false;
        }

        public static bool ValidCondition(string arg)
        {
            switch (arg)
            {
                case "opening": case "fresh": case "succeeded": case "failed": case "snatched": return true;
            }
            if (!arg.StartsWith("count", StringComparison.Ordinal)) return false;
            var rest = arg.Substring("count".Length);
            foreach (var op in new[] { ">=", "<=", "==", ">", "<" })
                if (rest.StartsWith(op, StringComparison.Ordinal) && int.TryParse(rest.Substring(op.Length), out _)) return true;
            return false;
        }
    }
}
