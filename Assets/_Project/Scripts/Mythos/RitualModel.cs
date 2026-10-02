using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Mythos
{
    // Who must attend and what must be sacrificed.
    public sealed class RitualParticipant
    {
        public readonly string NpcId;          // the Keeper who must attend
        public readonly string OfferingItemId; // what they lay on the altar (an item id)

        public RitualParticipant(string npcId, string offeringItemId)
        {
            NpcId = npcId;
            OfferingItemId = offeringItemId;
        }
    }

    public enum RitualFailure { None, MissingParticipant, OfferingNotSacrificed }

    public sealed class RitualResult
    {
        public bool Succeeded;
        public RitualFailure Failure;
        public List<string> MissingParticipants = new List<string>();   // Keepers who were not available
        public List<string> UnsacrificedOfferings = new List<string>(); // NPC ids whose offering did not dissolve
    }

    // The ritual rules from the owner's design (GDD section 9, answers G and H). Pure logic; the module does not
    // run rituals yet (task X-004).
    //
    //   - One ritual per season, on the new moon, in Harrow Wood (the current calendar has one new moon per season).
    //   - The number of offerings (and so of participants) depends on the season: spring 2, summer 3, autumn 4, winter 5.
    //   - The offerings are chosen at the start of the season.
    //   - Each participant places their offering on the altar, where it dissolves slowly during the timed ritual.
    //   - The ritual FAILS if any required Keeper is unavailable or any offering is not sacrificed in time.
    //   - A successful ritual lowers the god's wakefulness (WakefulnessModel); a failed one lowers nothing, so the
    //     season's 25% rise stands. (Assumption to confirm: a failure adds no extra spike.)
    public static class RitualModel
    {
        public static int OfferingCount(Season season)
        {
            switch (season)
            {
                case Season.Spring: return 2;
                case Season.Summer: return 3;
                case Season.Fall: return 4;
                default: return 5;
            }
        }

        // Is the plan well formed for the season: right number of participants, no duplicate Keeper or offering id.
        public static bool IsValidPlan(Season season, IReadOnlyList<RitualParticipant> plan)
        {
            if (plan == null || plan.Count != OfferingCount(season)) return false;
            if (plan.Any(p => string.IsNullOrEmpty(p.NpcId) || string.IsNullOrEmpty(p.OfferingItemId))) return false;
            return plan.Select(p => p.NpcId).Distinct().Count() == plan.Count;
        }

        // `available` = Keepers able to attend; `sacrificed` = NPC ids whose offering was fully sacrificed.
        public static RitualResult Resolve(IReadOnlyList<RitualParticipant> plan, ISet<string> available, ISet<string> sacrificed)
        {
            var result = new RitualResult();
            foreach (var p in plan)
            {
                if (!available.Contains(p.NpcId)) result.MissingParticipants.Add(p.NpcId);
                else if (!sacrificed.Contains(p.NpcId)) result.UnsacrificedOfferings.Add(p.NpcId);
            }

            if (result.MissingParticipants.Count > 0) result.Failure = RitualFailure.MissingParticipant;
            else if (result.UnsacrificedOfferings.Count > 0) result.Failure = RitualFailure.OfferingNotSacrificed;
            result.Succeeded = result.Failure == RitualFailure.None;
            return result;
        }

        // The god's wakefulness after the ritual night: lowered only if the ritual succeeded.
        public static int WakefulnessAfter(int wakefulness, RitualResult result, double roll) =>
            result.Succeeded ? WakefulnessModel.AfterSuccessfulRitual(wakefulness, roll) : wakefulness;

        // ---- timeline (GDD section 9, answer M) ----------------------------------------------------------------
        // The ritual is at an altar in a clearing deep in Harrow Wood. It opens with 30 minutes of the leader speaking,
        // then 20 minutes per sacrifice: the offering is laid on the altar at the start of its 20 minutes and is
        // consumed at the end of them. Times are game minutes from the start of the ritual.

        public const int LeaderMinutes = 30;
        public const int MinutesPerSacrifice = 20;

        public enum RitualPhase { NotStarted, LeaderSpeaking, Sacrificing, Finished }

        public static int DurationMinutes(int participants) => LeaderMinutes + MinutesPerSacrifice * participants;

        // When sacrifice `index` (0-based) is laid on the altar and when it is consumed.
        public static int SacrificeStart(int index) => LeaderMinutes + MinutesPerSacrifice * index;
        public static int SacrificeEnd(int index) => LeaderMinutes + MinutesPerSacrifice * (index + 1);

        public static RitualPhase PhaseAt(int offsetMinutes, int participants)
        {
            if (offsetMinutes < 0) return RitualPhase.NotStarted;
            if (offsetMinutes < LeaderMinutes) return RitualPhase.LeaderSpeaking;
            return offsetMinutes < DurationMinutes(participants) ? RitualPhase.Sacrificing : RitualPhase.Finished;
        }

        // Which sacrifice is under way (0-based), or -1 during the leader's speech and outside the ritual.
        public static int CurrentSacrifice(int offsetMinutes, int participants) =>
            PhaseAt(offsetMinutes, participants) == RitualPhase.Sacrificing ? (offsetMinutes - LeaderMinutes) / MinutesPerSacrifice : -1;

        public static bool IsOnAltar(int index, int offsetMinutes) =>
            offsetMinutes >= SacrificeStart(index) && offsetMinutes < SacrificeEnd(index);

        public static bool IsConsumed(int index, int offsetMinutes) => offsetMinutes >= SacrificeEnd(index);

        // The player may take (or use) a marked offering from the moment it is chosen until the end of its own
        // 20 minutes on the altar: before the ritual, or from the altar before it is consumed.
        public static bool CanStillBeTaken(int index, int offsetMinutes) => !IsConsumed(index, offsetMinutes);

        // The latest minute-of-day at which a ritual of this season can start and still end before the day does (06:00).
        public static int LatestStartMinuteOfDay(Season season) =>
            GameDateTime.DayEndMinute - DurationMinutes(OfferingCount(season));

        // Is this night a ritual night: the new moon (days 1-4 of a season). X-004 picks the exact night.
        public static bool IsNewMoon(GameDateTime date) => date.MoonPhase == MoonPhase.New;
    }
}
