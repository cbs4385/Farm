using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Ambient barks (T-125): a villager near the player says one short line in a speech bubble. The lines are ordinary one-node
    // dialogues in the set npc.<villager>.bark, picked with the same variety rules as talk lines. Pure rules live here; the
    // world part is BarkDirector.
    public static class Barks
    {
        public const float Range = 6f;                    // world units (cells) from the player
        public const float SecondsBetweenAny = 20f;       // at most one bubble this often, on the whole map
        public const float SecondsPerVillager = 90f;      // and the same villager waits this long
        public const float ShowSeconds = 4.5f;
        public const float ChancePerCheck = 0.35f;        // rolled twice a second once the gaps allow it

        public static string SetId(string npcId) => $"npc.{npcId}.bark";

        public static bool InRange(float dx, float dy) => dx * dx + dy * dy <= Range * Range;

        // Real-time gaps, kept only for this map visit (not saved).
        public sealed class Cooldowns
        {
            float _lastAny = float.NegativeInfinity;
            readonly Dictionary<string, float> _last = new Dictionary<string, float>();

            public bool Ready(string npcId, float now) =>
                now - _lastAny >= SecondsBetweenAny && (!_last.TryGetValue(npcId, out var t) || now - t >= SecondsPerVillager);

            public void Mark(string npcId, float now) { _lastAny = now; _last[npcId] = now; }
        }

        // The dialogue id a villager would bark now, or null (no set, or nothing fits). Records it in the line memory.
        public static string Pick(GameSession session, string npcId, int counter)
        {
            var set = session.Story?.Set(SetId(npcId));
            if (set == null || set.Entries.Count == 0) return null;
            var today = session.Clock.Now.TotalDays;
            var memory = LineMemory.Load(session);
            var id = set.PickVaried(session.World, today * 7919 + NpcInteractions.StableHash(npcId) * 31 + counter, memory, set.Id, today);
            if (id != null) LineMemory.Store(session, memory);
            return id;
        }

        // The words of a bark as shown: the first node's text, names filled in and inline markup resolved.
        public static string TextOf(GameSession session, string dialogueId)
        {
            var graph = session.Story?.Dialogue(dialogueId);
            var node = graph?.Nodes != null && graph.Nodes.Count > 0 ? graph.Nodes[0] : null;
            if (node == null || string.IsNullOrEmpty(node.Text)) return null;
            return RichText.Process(session.StoryText(node.Text, Array.Empty<object>()), session.World, session.Clock.Now.TotalDays).Text;
        }
    }
}
