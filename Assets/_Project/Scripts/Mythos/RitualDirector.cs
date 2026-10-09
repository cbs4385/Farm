using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;

namespace Farm.Mythos
{
    public readonly struct WakefulnessStepChanged
    {
        public readonly int Step, Wakefulness;
        public WakefulnessStepChanged(int step, int wakefulness) { Step = step; Wakefulness = wakefulness; }
    }

    public readonly struct RitualResolved
    {
        public readonly bool Succeeded; public readonly RitualFailure Failure;
        public RitualResolved(bool succeeded, RitualFailure failure) { Succeeded = succeeded; Failure = failure; }
    }

    // The ritual as a system (X-004), built on RitualModel. Each season's offerings are chosen at its start; on the night of the
    // fourth day the Keepers walk to the altar (their schedules), and from 22:00 the ritual runs on the clock: the leader speaks,
    // then each offering is laid on the altar and, twenty minutes later, consumed. The player may take an offering from the
    // altar (from the first summer on) or make a Keeper unavailable; either makes the ritual fail, and a failure lowers
    // nothing. A success lowers wakefulness by 30-40%. It all runs whether or not the player is watching.
    public static class RitualDirector
    {
        public const string ModuleId = "mythos";
        public const int StartMinute = 22 * 60;
        public const int RitualDay = 4;

        static readonly string[] VillageOfferings = { "food.salad", "artisan.jam", "crop.pumpkin", "product.wool", "product.egg", "artisan.pickles" };

        public static int SeasonIndex(GameDateTime d) => (d.Year - 1) * 4 + (int)d.Season;
        public static bool IsRitualDay(GameDateTime d) => d.Day == RitualDay;

        public static MythosSave Load(GameSession s) => s.GetModuleData<MythosSave>(ModuleId);
        public static void Store(GameSession s, MythosSave save) => s.SetModuleData(ModuleId, save);

        // The ritual in progress, as an offset in game minutes from its start.
        public static int Offset(int minuteOfDay) => minuteOfDay - StartMinute;

        // ---- planning ---------------------------------------------------------------------------------------------------

        public static void PlanSeason(GameSession s, MythosSave save)
        {
            var now = s.Clock.Now;
            save.PlannedSeason = SeasonIndex(now);
            save.Plan = new List<OfferingSlot>();
            var count = RitualModel.OfferingCount(now.Season);
            var keepers = MythosCast.Keepers;
            var candidates = new List<WorldObjectRef>();
            if (MythosLevel.Of(s) >= 1)
            {
                var kinds = MythosLevel.Full(s) ? new[] { WorldObjectKinds.Animal, WorldObjectKinds.PlantProduct, WorldObjectKinds.CraftedItem }
                                                : new[] { WorldObjectKinds.PlantProduct, WorldObjectKinds.CraftedItem };
                candidates = s.Hooks.EnumerateWorldObjects(kinds).Where(r => !save.Plan.Any(p => p.RefId == r.Id)).ToList();
            }
            for (var i = 0; i < count; i++)
            {
                var npc = keepers[(save.PlannedSeason + i) % keepers.Length];
                var slot = new OfferingSlot { NpcId = npc };
                // The first ritual of the first spring has a fixed first offering: the plant the opening quest asks the player to gather. Handing it over takes it
                // from the Keepers (see Progress), so a player who helps Elara makes that ritual fail.
                if (i == 0 && save.PlannedSeason == 0)
                {
                    slot.ItemId = MythosIds.Intro.Item; slot.RefKind = MythosIds.Intro.Kind; slot.RefId = MythosIds.Intro.Quest;
                    save.Plan.Add(slot);
                    continue;
                }
                // A modest chance that it is something of the player's; otherwise the village's own offering.
                var roll = WeatherRoller.Unit(save.PlannedSeason * 31 + i * 7 + 1, s.State.WorldSeed ^ 0x2F6E2B1);
                if (candidates.Count > 0 && roll < 0.25f)
                {
                    var pick = candidates[(int)(WeatherRoller.Unit(save.PlannedSeason * 17 + i * 5 + 2, s.State.WorldSeed) * candidates.Count) % candidates.Count];
                    candidates.Remove(pick);
                    slot.RefKind = pick.Kind; slot.RefId = pick.Id; slot.ItemId = pick.ItemId;
                }
                else slot.ItemId = VillageOfferings[(int)(WeatherRoller.Unit(save.PlannedSeason * 13 + i * 3 + 4, s.State.WorldSeed) * VillageOfferings.Length) % VillageOfferings.Length];
                save.Plan.Add(slot);
            }
        }

        // ---- running ----------------------------------------------------------------------------------------------------

        // Advances the ritual to `minuteOfDay` of a ritual day. Safe to call repeatedly; does nothing off the ritual night.
        public static void Progress(GameSession s, int minuteOfDay)
        {
            if (!MythosLevel.On(s) || !IsRitualDay(s.Clock.Now)) return;
            var save = Load(s);
            if (save.Sealed || save.Plan.Count == 0 || save.ResolvedSeason == save.PlannedSeason) return;
            var offset = Offset(minuteOfDay);
            var n = save.Plan.Count;
            if (offset < 0) return;

            for (var i = 0; i < n; i++)
            {
                var slot = save.Plan[i];
                if (offset >= RitualModel.SacrificeStart(i) && !slot.Placed)
                {
                    slot.Placed = true;
                    if (slot.RefKind == MythosIds.Intro.Kind) slot.Missing = IntroDelivered(s);           // the player gave the plants to Elara
                    else if (!string.IsNullOrEmpty(slot.RefId))
                    {
                        var reference = new WorldObjectRef(slot.RefKind, slot.RefId, string.Empty, slot.ItemId);
                        if (!s.Hooks.ConsumeWorldObject(reference)) slot.Missing = true;       // the player used or took it first
                    }
                }
                if (offset >= RitualModel.SacrificeEnd(i) && slot.Placed && !slot.Consumed)
                    slot.Consumed = !slot.Taken && !slot.Missing && MythosCast.Available(s, slot.NpcId);
            }

            if (offset >= RitualModel.DurationMinutes(n)) Resolve(s, save);
            Store(s, save);
        }

        static void Resolve(GameSession s, MythosSave save)
        {
            var plan = save.Plan.Select(p => new RitualParticipant(p.NpcId, p.ItemId)).ToList();
            var available = new HashSet<string>(MythosCast.Keepers.Where(k => MythosCast.Available(s, k)));
            var sacrificed = new HashSet<string>(save.Plan.Where(p => p.Consumed).Select(p => p.NpcId));
            var result = RitualModel.Resolve(plan, available, sacrificed);
            save.ResolvedSeason = save.PlannedSeason;
            save.LastSucceeded = result.Succeeded;
            if (result.Succeeded)
            {
                save.Successes++;
                var roll = WeatherRoller.Unit(save.PlannedSeason * 11 + 5, s.State.WorldSeed ^ 0x1D872B4);
                s.SetVar(MythosIds.Vars.Wakefulness, RitualModel.WakefulnessAfter(s.GetVar(MythosIds.Vars.Wakefulness), result, roll));
                WakefulnessService.SyncStep(s, save);
            }
            else save.Failures++;
            s.Publish(new RitualResolved(result.Succeeded, result.Failure));
        }

        // Has the player handed the opening quest's plants over?
        public static bool IntroDelivered(GameSession s) =>
            s.State.Quests.TryGetValue(MythosIds.Intro.Quest, out var q) && q != null && q.Status == QuestStatus.Done;

        // The slot whose offering is lying on the altar right now (not yet consumed, not taken).
        public static OfferingSlot OnAltar(MythosSave save, int minuteOfDay)
        {
            var offset = Offset(minuteOfDay);
            for (var i = 0; i < save.Plan.Count; i++)
                if (RitualModel.IsOnAltar(i, offset) && !save.Plan[i].Taken && !save.Plan[i].Missing) return save.Plan[i];
            return null;
        }

        // The player takes the offering from the altar (disrupting the ritual). Gives the item back. Returns the slot taken.
        public static OfferingSlot TakeFromAltar(GameSession s)
        {
            if (!MythosLevel.On(s) || !s.HasFlag(MythosIds2.Interference) || !IsRitualDay(s.Clock.Now)) return null;
            var save = Load(s);
            if (save.Sealed || save.ResolvedSeason == save.PlannedSeason) return null;
            var slot = OnAltar(save, s.Clock.Now.MinuteOfDay);
            if (slot == null) return null;
            if (!s.Backpack.CanAdd(slot.ItemId, 1)) return null;
            slot.Taken = true;
            s.Backpack.Add(slot.ItemId, 1);
            s.AddVar(MythosIds.Vars.Lore, 1);
            s.AddVar(MythosIds.Vars.CultStanding, -2);
            Store(s, save);
            return slot;
        }

        // Marked things the player could still find: the plan's real objects (for the journal).
        public static List<OfferingSlot> Marked(MythosSave save) => save.Plan.Where(p => !string.IsNullOrEmpty(p.RefId) && !p.Consumed && !p.Taken).ToList();
    }
}
