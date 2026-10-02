using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // T-040: checks authored data for mistakes the game could only hit in play. Used by `Farm/Validate Data` and a test.
    // It never touches the scene; the scene-side checks (warps, spawn points, door conditions) live in the editor tool.
    public sealed class ValidationInput
    {
        public StoryContent Story;
        public GameDatabase Db;
        public NpcCatalog Npcs;
        public Func<string, bool> HasKey;            // does the string table contain this key?
        public Func<string, bool> RecipeExists = id => false;
        public IEnumerable<string> MapIdList = MapIds.All.Concat(MapIds.Dungeons);
    }

    public static class StoryValidator
    {
        public static List<string> Run(ValidationInput v)
        {
            var problems = new List<string>();
            void Bad(string where, string message) => problems.Add($"{where}: {message}");

            var maps = new HashSet<string>(v.MapIdList) { MapIds.Woods };
            var items = new HashSet<string>(v.Db.AllItems.Select(i => i.Id));
            var npcIds = new HashSet<string>(v.Npcs.All.Select(n => n.Id));
            var skills = new HashSet<string>(SkillIds.All);

            void Condition(string where, string condition)
            {
                if (!Conditions.Validate(condition, out var error)) Bad(where, error);
            }
            void Key(string where, string key)
            {
                if (string.IsNullOrEmpty(key)) Bad(where, "missing string key");
                else if (!v.HasKey(key)) Bad(where, $"string key '{key}' is not in the string table");
            }
            void Effect(string where, string effect)
            {
                if (!Effects.Validate(effect, out var error)) { Bad(where, error); return; }
                Effects.TrySplit(effect, out var verb, out var a);
                switch (verb)
                {
                    case "give": case "take": if (!items.Contains(a[0])) Bad(where, $"effect '{effect}': unknown item '{a[0]}'"); break;
                    case "friend": if (!npcIds.Contains(a[0])) Bad(where, $"effect '{effect}': unknown NPC '{a[0]}'"); break;
                    case "xp": if (!skills.Contains(a[0])) Bad(where, $"effect '{effect}': unknown skill '{a[0]}'"); break;
                    case "quest.start": case "quest.done": if (v.Story.Quest(a[0]) == null) Bad(where, $"effect '{effect}': unknown quest '{a[0]}'"); break;
                    case "mail": if (v.Story.Letter(a[0]) == null) Bad(where, $"effect '{effect}': unknown letter '{a[0]}'"); break;
                    case "event": if (v.Story.Event(a[0]) == null) Bad(where, $"effect '{effect}': unknown event '{a[0]}'"); break;
                    case "learn": if (!v.RecipeExists(a[0])) Bad(where, $"effect '{effect}': unknown recipe '{a[0]}'"); break;
                    case "toast": Key(where, a[0]); break;
                }
            }
            void Effs(string where, IEnumerable<string> effects) { foreach (var e in effects ?? new string[0]) Effect(where, e); }

            foreach (var error in v.Story.Errors) Bad("story", error);

            // ---- dialogue -------------------------------------------------------------------------------------
            foreach (var d in v.Story.Dialogues)
            {
                var where = "dialogue " + d.Id;
                var ids = new HashSet<string>();
                foreach (var n in d.Nodes)
                    if (!ids.Add(n.Id ?? "")) Bad(where, $"duplicate or empty node id '{n.Id}'");
                if (d.Node(d.Start) == null) Bad(where, $"start node '{d.Start}' does not exist");
                foreach (var n in d.Nodes)
                {
                    var at = $"{where}/{n.Id}";
                    if (!string.IsNullOrEmpty(n.Next) && d.Node(n.Next) == null) Bad(at, $"next '{n.Next}' does not exist");
                    if (!string.IsNullOrEmpty(n.Text)) Key(at, n.Text);
                    if (!string.IsNullOrEmpty(n.Speaker) && !npcIds.Contains(n.Speaker)) Bad(at, $"unknown speaker '{n.Speaker}'");
                    Condition(at, n.Condition);
                    Effs(at, n.Effects);
                    for (var i = 0; i < n.Choices.Count; i++)
                    {
                        var c = n.Choices[i];
                        var cat = $"{at}/choice{i}";
                        Key(cat, c.Text);
                        Condition(cat, c.Condition);
                        Effs(cat, c.Effects);
                        if (!string.IsNullOrEmpty(c.Next) && d.Node(c.Next) == null) Bad(cat, $"next '{c.Next}' does not exist");
                    }
                }
            }
            foreach (var set in v.Story.Sets)
            {
                if (set.Entries.Count == 0) Bad("set " + set.Id, "has no entries");
                foreach (var e in set.Entries)
                {
                    var at = $"set {set.Id}";
                    if (v.Story.Dialogue(e.Dialogue) == null) Bad(at, $"unknown dialogue '{e.Dialogue}'");
                    Condition(at, e.Condition);
                }
            }

            // ---- NPCs -----------------------------------------------------------------------------------------
            foreach (var npc in v.Npcs.All)
            {
                var where = "npc " + npc.Id;
                Key(where, npc.NameKey);
                if (!maps.Contains(npc.HomeMap)) Bad(where, $"home map '{npc.HomeMap}' is not a map");
                if (v.Story.Set(npc.TalkSetId) == null) Bad(where, $"no dialogue set '{npc.TalkSetId}'");
                if (npc.BirthdayDay < 1 || npc.BirthdayDay > GameDateTime.DaysPerSeason) Bad(where, "birthday day is outside the season");
                foreach (var id in npc.Loved.Concat(npc.Liked).Concat(npc.Disliked))
                    if (!items.Contains(id)) Bad(where, $"gift taste names unknown item '{id}'");
                foreach (var cat in npc.LovedCategories.Concat(npc.DislikedCategories))
                    if (!Enum.TryParse<ItemCategory>(cat, out _)) Bad(where, $"gift taste names unknown category '{cat}'");
                if (npc.SpriteFor(UnityEngine.Vector2Int.down) == null) Bad(where, "has no sprites");
                if (npc.Schedule.Count == 0) Bad(where, "has no schedule");
                foreach (var entry in npc.Schedule)
                {
                    var at = $"{where}/schedule {entry.Id}";
                    Condition(at, entry.Condition);
                    if (!NpcSchedule.IsValid(entry, out var scheduleError)) Bad(at, scheduleError);
                    foreach (var stop in entry.Stops)
                        if (!maps.Contains(stop.Map)) Bad(at, $"stop on unknown map '{stop.Map}'");
                }
                if (!npc.Schedule.Any(e => string.IsNullOrWhiteSpace(e.Condition))) Bad(where, "needs a default schedule entry with no condition");
            }

            // ---- quests, letters, events ---------------------------------------------------------------------
            foreach (var q in v.Story.Quests)
            {
                var where = "quest " + q.Id;
                Key(where, q.TitleKey);
                Key(where, q.DescriptionKey);
                Condition(where, q.Available);
                if (!string.IsNullOrEmpty(q.Giver) && !npcIds.Contains(q.Giver)) Bad(where, $"unknown giver '{q.Giver}'");
                if (q.Objectives.Count == 0 && !q.AutoComplete) Bad(where, "has no objectives");
                foreach (var o in q.Objectives)
                {
                    Key(where, o.Text);
                    Condition(where, o.Condition);
                    if (!string.IsNullOrEmpty(o.TakeItem) && !items.Contains(o.TakeItem)) Bad(where, $"takes unknown item '{o.TakeItem}'");
                }
                Effs(where, q.OnStart);
                Effs(where, q.Rewards);
            }
            foreach (var l in v.Story.Letters)
            {
                var where = "letter " + l.Id;
                Key(where, l.SubjectKey);
                Key(where, l.BodyKey);
                Condition(where, l.Condition);
                if (!string.IsNullOrEmpty(l.Sender) && !npcIds.Contains(l.Sender)) Bad(where, $"unknown sender '{l.Sender}'");
                Effs(where, l.Effects);
            }
            foreach (var e in v.Story.Events)
            {
                var where = "event " + e.Id;
                Condition(where, e.Condition);
                if (e.Trigger != "map" && e.Trigger != "dawn" && e.Trigger != "manual") Bad(where, $"unknown trigger '{e.Trigger}'");
                if (e.Trigger == "map" && !maps.Contains(e.Map)) Bad(where, $"unknown map '{e.Map}'");
                if (e.Steps.Count == 0) Bad(where, "has no steps");
                for (var i = 0; i < e.Steps.Count; i++)
                {
                    var step = e.Steps[i];
                    var at = $"{where}/step{i}";
                    if (!EventSteps.Known.Contains(step.Type)) { Bad(at, $"unknown step type '{step.Type}'"); continue; }
                    if (step.Type == "say") Key(at, step.Text);
                    if (step.Type == "dialogue" && v.Story.Dialogue(step.Dialogue) == null) Bad(at, $"unknown dialogue '{step.Dialogue}'");
                    if ((step.Type == "move" || step.Type == "face" || step.Type == "place") && string.IsNullOrEmpty(step.Actor)) Bad(at, "needs an actor");
                    if (!string.IsNullOrEmpty(step.Actor) && step.Actor != "player" && !npcIds.Contains(step.Actor)) Bad(at, $"unknown actor '{step.Actor}'");
                    if (!string.IsNullOrEmpty(step.Speaker) && !npcIds.Contains(step.Speaker)) Bad(at, $"unknown speaker '{step.Speaker}'");
                    Effs(at, step.Effects);
                }
                Effs(where, e.SkipEffects);
            }
            foreach (var r in v.Story.RandomEvents)
            {
                var where = "random event " + r.Id;
                Key(where, r.TextKey);
                Condition(where, r.Condition);
                if (r.Weight <= 0f) Bad(where, "weight must be positive");
                if (r.Mood != "good" && r.Mood != "bad" && r.Mood != "neutral") Bad(where, $"unknown mood '{r.Mood}'");
                Effs(where, r.Effects);
            }
            foreach (var b in v.Story.BoardJobs)
            {
                var where = "board job " + b.Id;
                Condition(where, b.Condition);
                if (b.Items.Count == 0) Bad(where, "has no items");
                foreach (var id in b.Items) if (!items.Contains(id)) Bad(where, $"unknown item '{id}'");
                if (b.MinCount < 1 || b.MaxCount < b.MinCount) Bad(where, "bad count range");
            }
            return problems;
        }
    }

    // The kinds of step an event may contain (see EventDefinition); the runner and the validator share this list.
    public static class EventSteps
    {
        public static readonly HashSet<string> Known = new HashSet<string>
        {
            "say", "dialogue", "move", "face", "wait", "advance", "fadeout", "fadein", "effects", "place",
        };
    }
}
