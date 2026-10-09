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
        public static readonly string[] MomentTags = DialogueVocabulary.MomentTags;
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
                    case "friend": case "wake": case "met": if (!npcIds.Contains(a[0])) Bad(where, $"effect '{effect}': unknown NPC '{a[0]}'"); break;
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
                    if (!string.IsNullOrEmpty(n.Speaker) && n.Speaker != "*" && !npcIds.Contains(n.Speaker)) Bad(at, $"unknown speaker '{n.Speaker}'");
                    if (n.Speaker == "*" && !d.Id.StartsWith("social.", StringComparison.Ordinal) && !d.Id.Contains(".topic.")) Bad(at, "the speaker '*' (the villager being talked to) is only for social and topic dialogues");
                    Condition(at, n.Condition);
                    Effs(at, n.Effects);
                    if (!DialogueVocabulary.Has(DialogueVocabulary.Expressions, n.Expression)) Bad(at, $"unknown expression '{n.Expression}'");
                    if (!DialogueVocabulary.Has(DialogueVocabulary.Emotes, n.Emote)) Bad(at, $"unknown emote '{n.Emote}'");
                    if (!DialogueVocabulary.Has(DialogueVocabulary.Cameras, n.Camera)) Bad(at, $"unknown camera '{n.Camera}'");
                    if (!DialogueVocabulary.Has(DialogueVocabulary.MomentTags, n.Tag)) Bad(at, $"unknown moment tag '{n.Tag}'");
                    if (!string.IsNullOrEmpty(n.Sfx) && n.Sfx != "signature" && !Enum.TryParse<Sfx>(n.Sfx, true, out _)) Bad(at, $"unknown sound '{n.Sfx}' (use signature or one of {string.Join(", ", Enum.GetNames(typeof(Sfx)))})");
                    for (var i = 0; i < n.Choices.Count; i++)
                    {
                        var c = n.Choices[i];
                        var cat = $"{at}/choice{i}";
                        Key(cat, c.Text);
                        if (!DialogueVocabulary.Has(DialogueVocabulary.Tones, c.Tone)) Bad(cat, $"unknown tone '{c.Tone}'");
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
                    if (!LineRarity.IsKnown(e.Rarity)) Bad(at, $"'{e.Dialogue}': unknown rarity '{e.Rarity}'");
                    if (e.Weight <= 0f) Bad(at, $"'{e.Dialogue}': weight must be above 0");
                    if (e.Cooldown < -1) Bad(at, $"'{e.Dialogue}': cooldown must be -1 (default) or more");
                    if (!string.IsNullOrEmpty(e.Tag) && Array.IndexOf(MomentTags, e.Tag) < 0) Bad(at, $"'{e.Dialogue}': unknown moment tag '{e.Tag}'");
                }
            }

            foreach (var r in v.Story.Reactions)
            {
                var at = "reaction " + r.Id;
                if (v.Story.Dialogue(r.Dialogue) == null) Bad(at, $"unknown dialogue '{r.Dialogue}'");
                Condition(at, r.Condition);
                if (string.IsNullOrWhiteSpace(r.Npcs)) Bad(at, "needs at least one villager in 'npcs'");
                else
                    foreach (var n in r.Npcs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                        if (!npcIds.Contains(n)) Bad(at, $"unknown villager '{n}'");
                if (!Reactions.IsKnownTrigger(r.On)) Bad(at, $"unknown trigger '{r.On}'");
                if (r.TtlDays < 1) Bad(at, "ttlDays must be at least 1");
                if (r.CooldownDays < 0) Bad(at, "cooldownDays must not be negative");
                if (r.Priority < 0 || r.Priority > 7) Bad(at, "priority must be 0-7 (8-10 is the mythos band)");
            }

            foreach (var sl in v.Story.Storylines)
            {
                if (string.IsNullOrEmpty(sl.Id) || sl.Id.Any(c => !(char.IsLower(c) || char.IsDigit(c) || c == '_'))) Bad("storyline " + sl.Id, "ids use lower case letters, digits and underscores");
                if (sl.Weight <= 0f) Bad("storyline " + sl.Id, "weight must be above 0");
                if (!string.IsNullOrEmpty(sl.TitleKey)) Key("storyline " + sl.Id, sl.TitleKey);
            }
            foreach (var t in v.Story.Topics)
            {
                var at = "topic " + t.Id;
                if (!npcIds.Contains(t.Npc ?? "")) Bad(at, $"unknown villager '{t.Npc}'");
                if (v.Story.Dialogue(t.Dialogue) == null) Bad(at, $"unknown dialogue '{t.Dialogue}'");
                Key(at, t.LabelKey ?? "topic." + t.Id);
                Condition(at, t.Condition);
                if (t.CooldownDays < 1) Bad(at, "cooldownDays must be at least 1");
            }
            foreach (var p in v.Story.Socials)
            {
                var at = "social " + p.Npc;
                if (!npcIds.Contains(p.Npc ?? "")) Bad(at, $"unknown villager '{p.Npc}'");
                foreach (var action in p.Loves.Concat(p.Likes).Concat(p.Dislikes))
                    if (!SocialActions.IsAction(action)) Bad(at, $"unknown action '{action}'");
                if (p.Loves.Concat(p.Likes).Concat(p.Dislikes).GroupBy(x => x).Any(g => g.Count() > 1)) Bad(at, "an action is listed twice");
            }
            if (v.Story.Topics.Any() || v.Story.Socials.Any())
                foreach (var key in new[] { InteractionMenu.PromptKey, InteractionMenu.SocialPromptKey, InteractionMenu.SocialKey, InteractionMenu.GoodbyeKey, InteractionMenu.BackKey })
                    Key("chat menu", key);
            if (v.Story.Socials.Any() || v.Story.Dialogues.Any(d => d.Id.StartsWith("social.", StringComparison.Ordinal)))
                foreach (var action in SocialActions.All)
                {
                    Key("social menu", InteractionMenu.SocialKeyFor(action));
                    foreach (SocialOutcome outcome in Enum.GetValues(typeof(SocialOutcome)))
                        if (v.Story.Dialogue($"social.{action}.{SocialActions.OutcomeName(outcome)}") == null)
                            Bad("social menu", $"missing the generic reaction social.{action}.{SocialActions.OutcomeName(outcome)}");
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
                    if (!string.IsNullOrEmpty(o.GiveItem) && (!items.Contains(o.GiveItem) || o.GiveCount <= 0)) Bad(where, $"gives '{o.GiveItem}' x{o.GiveCount}: unknown item or no count");
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
                if (!string.IsNullOrEmpty(e.TitleKey)) Key(where, e.TitleKey);
                if (!DialogueVocabulary.Has(DialogueVocabulary.MomentTags, e.Tag)) Bad(where, $"unknown moment tag '{e.Tag}'");
                for (var i = 0; i < e.Steps.Count; i++)
                {
                    var step = e.Steps[i];
                    var at = $"{where}/step{i}";
                    if (!EventSteps.Known.Contains(step.Type ?? string.Empty)) continue;       // reported by EventSteps.Problems below
                    if (step.Type == "say") Key(at, step.Text);
                    if (!string.IsNullOrEmpty(step.Speaker) && !npcIds.Contains(step.Speaker)) Bad(at, $"unknown speaker '{step.Speaker}'");
                    Condition(at, step.Condition);
                    Effs(at, step.Effects);
                }
                foreach (var problem in EventSteps.Problems(e, id => npcIds.Contains(id), id => items.Contains(id), id => v.Story.Dialogue(id) != null))
                    Bad(where, problem);
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
}
