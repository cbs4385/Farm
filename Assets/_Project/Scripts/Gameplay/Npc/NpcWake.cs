using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    public readonly struct NpcWoken
    {
        public readonly string NpcId;
        public readonly bool ByPlayer;
        public NpcWoken(string npcId, bool byPlayer) { NpcId = npcId; ByPlayer = byPlayer; }
    }

    // Who has been woken today and until when they stay up (saved as module data, no GameState change).
    [Serializable]
    public sealed class WokenState
    {
        public const string ModuleId = "npc.woken";

        [Serializable]
        public sealed class Entry { public int Day; public int Until; }

        public Dictionary<string, Entry> Awake = new Dictionary<string, Entry>();
    }

    // Waking a villager who is asleep in bed. The player does it by talking to them; a story can do it with the effect `wake:<npc>` (another villager
    // knocking, a cat, a thunderclap...). The sleeper gets up, stands by the bed for WakeMinutes, and then the schedule sends them back to bed. The villager's
    // reaction is a normal reaction (Reactions, trigger `wake:<npc>`, data in story JSON): grumpy, or warm to a friend. Waking someone who is not a friend costs
    // a little friendship; a friend does not mind (they are welcome at any hour, NpcHomes.FriendHearts).
    public static class NpcWake
    {
        public const int WakeMinutes = 90;
        public const int PenaltyPoints = 30;                 // a little more than a first talk of the day earns (FriendshipModel.TalkPoints)

        public static bool IsAsleep(NpcPlacement place) => !place.Walking && place.Facing == NpcSchedule.SleepFacing;

        // Is the villager in bed right now?
        public static bool IsAsleep(GameSession session, NpcDefinition npc) => IsAsleep(Adjust(session, npc.Id, NpcLocator.Where(session, npc), session.Clock.PreciseMinuteOfDay));

        // The schedule's place, except that a villager who was woken and has not yet gone back to bed is up, standing beside the bed. (pure given the session)
        public static NpcPlacement Adjust(GameSession session, string npcId, NpcPlacement place, float minuteOfDay)
        {
            if (!IsAsleep(place)) return place;
            var state = Load(session);
            if (!state.Awake.TryGetValue(npcId, out var entry) || entry.Day != session.Clock.Now.TotalDays || minuteOfDay >= entry.Until) return place;
            return NpcPlacement.Standing(place.Map, NpcHomes.BedX, NpcHomes.BedY - 1, "up");
        }

        static WokenState Load(GameSession session) => session.GetModuleData<WokenState>(Farm.Gameplay.WokenState.ModuleId);

        // The wake reaction a villager has just earned, to be said at once (before any story line a normal talk would pick): the highest-priority pending one
        // whose condition holds (the warm one for a friend). It is marked as said. Null when there is none.
        public static string TakeReaction(GameSession session, string npcId)
        {
            var state = ReactionState.Load(session);
            var today = session.Clock.Now.TotalDays;
            PendingReaction best = null;
            foreach (var p in state.Pending)
            {
                if (p.Npc != npcId || !p.Id.StartsWith("wake.", StringComparison.Ordinal) || p.ExpiresDay < today || p.ConsumedDay >= 0) continue;
                if (!Conditions.TryEvaluate(p.Condition, session.World, out var ok) || !ok) continue;
                if (best == null || p.Priority > best.Priority) best = p;
            }
            if (best == null) return null;
            foreach (var p in state.Pending)                       // the other wake reactions (the grumpy one beside the warm one) are spent too, so they are not said later
                if (p.Npc == npcId && p.Id.StartsWith("wake.", StringComparison.Ordinal) && p.ConsumedDay < 0) p.ConsumedDay = today;
            ReactionState.Store(session, state);
            return best.Dialogue;
        }

        // Wakes the villager if they are asleep. Returns whether they were. `byPlayer`: the player woke them (a story effect does not cost friendship).
        public static bool Wake(GameSession session, string npcId, bool byPlayer)
        {
            if (session == null || !session.InGame || !(session.Npcs.Get(npcId) is NpcDefinition npc)) return false;
            if (!IsAsleep(session, npc)) return false;
            var now = session.Clock.Now;
            var state = Load(session);
            state.Awake[npcId] = new WokenState.Entry { Day = now.TotalDays, Until = (int)session.Clock.PreciseMinuteOfDay + WakeMinutes };
            session.SetModuleData(Farm.Gameplay.WokenState.ModuleId, state);
            if (byPlayer && FriendshipModel.Hearts(NpcInteractions.StateOf(session.State, npcId).Points) < NpcHomes.FriendHearts)
                NpcInteractions.AddPoints(session, npcId, -PenaltyPoints);
            session.Publish(new NpcWoken(npcId, byPlayer));
            Reactions.Fire(session, "wake:" + npcId);
            return true;
        }
    }
}
