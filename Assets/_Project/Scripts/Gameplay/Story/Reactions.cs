using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // A villager's reaction to something the player did or something that happened (T-091). Data in story JSON:
    //   { "id": "wren.harvest_hello", "on": "flag:first_harvest", "npcs": "wren,tilda", "condition": "hearts:wren>=1",
    //     "ttlDays": 3, "priority": 3, "cooldownDays": 28, "once": false, "dialogue": "wren.react_harvest" }
    // `on` is one of: flag:<flag> (set), quest.start:<id>, quest.done:<id>, skill.up:<skill>, event:<id> (a scene ended),
    // season:<spring|summer|fall|winter>, gift:<npc> (the player gave that villager a gift), wake:<npc> (that villager was woken from their bed, by the player or a story effect), random:<id>, or `day`
    // (checked every morning: use it with a condition such as festival.in:==1).
    [Serializable]
    public sealed class ReactionDefinition
    {
        public string Id;
        public string On;
        public string Npcs;                 // comma-separated villager ids
        public string Condition;
        public int TtlDays = 3;             // how long the reaction waits for the player to talk to the villager
        public int Priority = 3;            // dialogue band 3-4: above chat, below quest offers and story scenes
        public int CooldownDays = 28;       // it cannot be queued again for the same villager within this many days
        public bool Once;                   // never again after the first time
        public string Dialogue;
    }

    [Serializable]
    public sealed class PendingReaction
    {
        public string Id, Npc, Dialogue, Condition;
        public int Priority;
        public int QueuedDay, ExpiresDay;
        public int ConsumedDay = -1;
    }

    // The reaction queue, saved as module data (no GameState change).
    [Serializable]
    public sealed class ReactionState
    {
        public const string ModuleId = "dialogue.reactions";
        public List<PendingReaction> Pending = new List<PendingReaction>();
        public Dictionary<string, int> Fired = new Dictionary<string, int>();      // "<reaction>|<npc>" -> last day queued

        public static ReactionState Load(GameSession s) => s.GetModuleData<ReactionState>(ModuleId);
        public static void Store(GameSession s, ReactionState r) => s.SetModuleData(ModuleId, r);
    }

    public static class Reactions
    {
        static readonly string[] TriggerPrefixes = { "flag:", "quest.start:", "quest.done:", "skill.up:", "event:", "season:", "gift:", "random:", "wake:" };

        public static bool IsKnownTrigger(string on) =>
            on == "day" || (on != null && TriggerPrefixes.Any(p => on.StartsWith(p, StringComparison.Ordinal) && on.Length > p.Length));

        // Queues the reactions that match `on`. Returns how many were added.
        public static int Trigger(ReactionState state, IEnumerable<ReactionDefinition> definitions, IWorldQuery world, string on, int today)
        {
            if (state == null || definitions == null) return 0;
            var added = 0;
            foreach (var def in definitions)
            {
                if (def == null || def.On != on || string.IsNullOrEmpty(def.Dialogue) || string.IsNullOrEmpty(def.Npcs)) continue;
                if (!Conditions.TryEvaluate(def.Condition, world, out var ok) || !ok) continue;
                foreach (var npc in def.Npcs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
                {
                    var key = def.Id + "|" + npc;
                    if (state.Pending.Any(p => p.Id == def.Id && p.Npc == npc)) continue;
                    if (state.Fired.TryGetValue(key, out var last) && (def.Once || today - last < def.CooldownDays)) continue;
                    state.Fired[key] = today;
                    state.Pending.Add(new PendingReaction
                    {
                        Id = def.Id, Npc = npc, Dialogue = def.Dialogue, Condition = def.Condition, Priority = def.Priority,
                        QueuedDay = today, ExpiresDay = today + Math.Max(1, def.TtlDays) - 1,
                    });
                    added++;
                }
            }
            return added;
        }

        // The pending reactions a villager can say today, as dialogue set entries for DialogueSet.PickVaried. A reaction
        // already said today stays available so talking twice gives the same line.
        public static List<DialogueSetEntry> EntriesFor(ReactionState state, string npc, int today)
        {
            var list = new List<DialogueSetEntry>();
            if (state == null) return list;
            foreach (var p in state.Pending)
            {
                if (p.Npc != npc || p.ExpiresDay < today || (p.ConsumedDay >= 0 && p.ConsumedDay != today)) continue;
                list.Add(new DialogueSetEntry { Dialogue = p.Dialogue, Priority = p.Priority, Condition = p.Condition, Cooldown = 0 });
            }
            return list;
        }

        // Called after a villager picked `dialogue`: if it was a reaction it is spent (it lingers until tomorrow, for repeats).
        public static void MarkConsumed(ReactionState state, string npc, string dialogue, int today)
        {
            if (state == null) return;
            foreach (var p in state.Pending)
                if (p.Npc == npc && p.Dialogue == dialogue && p.ConsumedDay < 0) p.ConsumedDay = today;
        }

        // Morning housekeeping: expired reactions and ones spent on an earlier day are dropped.
        public static int Prune(ReactionState state, int today) =>
            state == null ? 0 : state.Pending.RemoveAll(p => p.ExpiresDay < today || (p.ConsumedDay >= 0 && p.ConsumedDay < today));

        // ---- session wiring ----------------------------------------------------------------------------------------

        public static void Fire(GameSession session, string on)
        {
            if (session == null || !session.InGame || session.Story == null) return;
            var defs = session.Story.Reactions;
            if (!defs.Any(d => d.On == on)) return;
            var state = ReactionState.Load(session);
            if (Trigger(state, defs, session.World, on, session.Clock.Now.TotalDays) > 0) ReactionState.Store(session, state);
        }

        public static void NewDay(GameSession session)
        {
            if (session == null || !session.InGame) return;
            var state = ReactionState.Load(session);
            var changed = Prune(state, session.Clock.Now.TotalDays) > 0;
            if (session.Story != null && session.Story.Reactions.Any(d => d.On == "day"))
                changed |= Trigger(state, session.Story.Reactions, session.World, "day", session.Clock.Now.TotalDays) > 0;
            if (changed) ReactionState.Store(session, state);
        }
    }
}
