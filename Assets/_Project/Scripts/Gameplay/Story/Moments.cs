using System;
using System.Collections.Generic;
using System.Linq;

namespace Farm.Gameplay
{
    public enum MomentKind { Line, Node, Scene }

    public sealed class Moment
    {
        public MomentKind Kind;
        public string Id;            // dialogue id (a set entry or node) or event id
        public string Tag;           // funny, wholesome, surprise, mystery
        public string Villager;      // the villager it belongs to, or empty
        public string Rarity;        // set entries only
    }

    // Every moment the writers have marked as worth remembering or clipping (T-140): set entries (a talk line), dialogue nodes
    // and whole scenes, found in one place so quotas, reports and the stream-appeal targets count the same things.
    public static class MomentCatalog
    {
        public static List<Moment> Build(StoryContent story, IEnumerable<string> villagers)
        {
            var list = new List<Moment>();
            var names = villagers.ToList();

            foreach (var set in story.Sets)
            {
                var owner = names.FirstOrDefault(v => set.Id == $"npc.{v}.talk") ?? string.Empty;
                foreach (var e in set.Entries.Where(e => !string.IsNullOrEmpty(e.Tag)))
                    list.Add(new Moment { Kind = MomentKind.Line, Id = e.Dialogue, Tag = e.Tag, Villager = owner, Rarity = e.Rarity ?? string.Empty });
            }
            foreach (var d in story.Dialogues)
                foreach (var n in d.Nodes.Where(n => !string.IsNullOrEmpty(n.Tag)))
                    list.Add(new Moment { Kind = MomentKind.Node, Id = d.Id + "/" + n.Id, Tag = n.Tag, Villager = names.Contains(n.Speaker ?? string.Empty) ? n.Speaker : OwnerFromId(d.Id, names) });
            foreach (var ev in story.Events.Where(e => !string.IsNullOrEmpty(e.Tag)))
                list.Add(new Moment { Kind = MomentKind.Scene, Id = ev.Id, Tag = ev.Tag, Villager = SceneOwner(ev, names) });
            return list;
        }

        static string OwnerFromId(string dialogueId, List<string> names) =>
            names.FirstOrDefault(v => dialogueId.StartsWith(v + ".", StringComparison.Ordinal) || dialogueId.StartsWith($"social.{v}.", StringComparison.Ordinal)) ?? string.Empty;

        static string SceneOwner(EventDefinition ev, List<string> names)
        {
            var group = Memories.GroupOf(ev);
            return names.Contains(group) ? group : string.Empty;
        }

        public static int Count(IEnumerable<Moment> moments, string villager, string tag) =>
            moments.Count(m => m.Villager == villager && m.Tag == tag);

        public static int Scenes(IEnumerable<Moment> moments) => moments.Count(m => m.Kind == MomentKind.Scene);
    }

    // A measurable version of the "clip-worthiness" checklist for set pieces (docs/narrative/STYLE.md section 8): a scene that is
    // meant to be shared should be short enough to clip, have at least one beat the eye can see or the ear can hear, end on a
    // spoken line (the payoff) and be a titled memory. Pure; durations are estimates.
    public static class MomentChecklist
    {
        public const float MaxSceneSeconds = 150f;
        const float WordsPerSecond = 3.3f, SecondsPerLine = 0.6f, WalkCellsPerSecond = 3.5f;
        static readonly HashSet<string> Beats = new HashSet<string> { "emote", "anim", "camera", "lighting", "sfx", "music", "spawn", "letterbox", "expression" };

        // Rough run time: lines are read at a steady pace, walks and waits take their time.
        public static float EstimateSeconds(EventDefinition ev, Func<string, string> textOf)
        {
            float seconds = 0f;
            Add(ev.Steps);
            return seconds;

            void Add(IEnumerable<EventStep> steps)
            {
                foreach (var s in steps)
                {
                    switch (s.Type)
                    {
                        case "say": seconds += Reading(textOf(s.Text)); break;
                        case "wait": seconds += s.Seconds; break;
                        case "anim": case "emote": case "camera": case "lighting": case "letterbox": if (!s.Async) seconds += s.Seconds > 0 ? s.Seconds : 0.8f; break;
                        case "move": seconds += 3f; break;
                        case "fadeout": case "fadein": seconds += s.Seconds > 0 ? s.Seconds : 0.4f; break;
                        case "dialogue": seconds += 8f; break;           // a conversation graph: a typical length
                        case "parallel": seconds += s.Steps.Count == 0 ? 0f : s.Steps.Max(c => Math.Max(c.Seconds, c.Type == "move" ? 3f : 0.8f)); break;
                    }
                }
            }
        }

        static float Reading(string text) =>
            string.IsNullOrEmpty(text) ? SecondsPerLine : SecondsPerLine + RichText.Plain(text).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length / WordsPerSecond;

        public static int PresentationBeats(EventDefinition ev)
        {
            var n = 0;
            void Walk(IEnumerable<EventStep> steps)
            {
                foreach (var s in steps) { if (Beats.Contains(s.Type)) n++; Walk(s.Steps); }
            }
            Walk(ev.Steps);
            return n;
        }

        // The last thing the scene does that a player sees is a line.
        public static bool EndsOnALine(EventDefinition ev)
        {
            for (var i = ev.Steps.Count - 1; i >= 0; i--)
            {
                var t = ev.Steps[i].Type;
                if (t == "say" || t == "dialogue") return true;
                if (t == "effects" || t == "label" || t == "despawn" || t == "lighting" || t == "camera" || t == "letterbox" || t == "branch" || t == "waitFor") continue;
                return false;
            }
            return false;
        }

        // Problems with a tagged scene against the checklist (empty when it passes).
        public static List<string> Problems(EventDefinition ev, Func<string, string> textOf)
        {
            var problems = new List<string>();
            if (string.IsNullOrEmpty(ev.Tag)) return problems;
            if (!Memories.IsMemory(ev)) problems.Add("a tagged scene needs a title, so it can be replayed and clipped");
            var seconds = EstimateSeconds(ev, textOf);
            if (seconds > MaxSceneSeconds) problems.Add($"about {seconds:0} seconds long; a clip-worthy scene stays under {MaxSceneSeconds:0}");
            if (PresentationBeats(ev) == 0) problems.Add("no visible or audible beat (an emote, gesture, camera move, light, sound or prop)");
            if (!EndsOnALine(ev)) problems.Add("it should end on a spoken line (the payoff)");
            return problems;
        }
    }
}
