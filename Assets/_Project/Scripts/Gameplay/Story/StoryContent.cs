using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Farm.Gameplay
{
    // Every piece of authored story data: dialogues and dialogue sets, quests, letters, events, random events and
    // help-wanted job templates. Content is JSON under Resources/Story/*.json (ADR 0003); modules add more at
    // runtime with AddJson. A duplicate id is rejected and logged (replacing shipped content would break saves).
    public sealed class StoryContent
    {
        public const string ResourceFolder = "Story";

        readonly Dictionary<string, DialogueGraph> _dialogues = new Dictionary<string, DialogueGraph>();
        readonly Dictionary<string, DialogueSet> _sets = new Dictionary<string, DialogueSet>();
        readonly Dictionary<string, QuestDefinition> _quests = new Dictionary<string, QuestDefinition>();
        readonly Dictionary<string, LetterDefinition> _letters = new Dictionary<string, LetterDefinition>();
        readonly Dictionary<string, EventDefinition> _events = new Dictionary<string, EventDefinition>();
        readonly Dictionary<string, RandomEventDefinition> _randomEvents = new Dictionary<string, RandomEventDefinition>();
        readonly Dictionary<string, BoardJobTemplate> _jobs = new Dictionary<string, BoardJobTemplate>();
        readonly Dictionary<string, ReactionDefinition> _reactions = new Dictionary<string, ReactionDefinition>();
        readonly Dictionary<string, TopicDefinition> _topics = new Dictionary<string, TopicDefinition>();
        readonly Dictionary<string, SocialProfile> _socials = new Dictionary<string, SocialProfile>();
        readonly Dictionary<string, JObject> _eventTemplates = new Dictionary<string, JObject>();
        readonly Dictionary<string, StorylineDefinition> _storylines = new Dictionary<string, StorylineDefinition>();
        readonly List<string> _sources = new List<string>();

        // Problems found while loading (bad JSON, duplicate ids). The data validator reports them too.
        public List<string> Errors { get; } = new List<string>();

        public IEnumerable<DialogueGraph> Dialogues => _dialogues.Values;
        public IEnumerable<DialogueSet> Sets => _sets.Values;
        public IEnumerable<QuestDefinition> Quests => _quests.Values;
        public IEnumerable<LetterDefinition> Letters => _letters.Values;
        public IEnumerable<EventDefinition> Events => _events.Values;
        public IEnumerable<RandomEventDefinition> RandomEvents => _randomEvents.Values;
        public IEnumerable<BoardJobTemplate> BoardJobs => _jobs.Values;
        public IEnumerable<ReactionDefinition> Reactions => _reactions.Values;
        public IEnumerable<TopicDefinition> Topics => _topics.Values;
        public IEnumerable<SocialProfile> Socials => _socials.Values;
        public IEnumerable<string> EventTemplateIds => _eventTemplates.Keys;
        public IEnumerable<StorylineDefinition> Storylines => _storylines.Values;
        public ReactionDefinition Reaction(string id) => id != null && _reactions.TryGetValue(id, out var r) ? r : null;
        public IReadOnlyList<string> Sources => _sources;

        public DialogueGraph Dialogue(string id) => id != null && _dialogues.TryGetValue(id, out var d) ? d : null;
        public DialogueSet Set(string id) => id != null && _sets.TryGetValue(id, out var s) ? s : null;
        public QuestDefinition Quest(string id) => id != null && _quests.TryGetValue(id, out var q) ? q : null;
        public LetterDefinition Letter(string id) => id != null && _letters.TryGetValue(id, out var l) ? l : null;
        public EventDefinition Event(string id) => id != null && _events.TryGetValue(id, out var e) ? e : null;

        public static StoryContent LoadFromResources()
        {
            var content = new StoryContent();
            // A fixed order, so files that add to others (compiled FScript, template instances) always load after what they use.
            foreach (var asset in Resources.LoadAll<TextAsset>(ResourceFolder).OrderBy(a => a.name, StringComparer.Ordinal))
                content.AddJson(asset.text, asset.name);
            return content;
        }

        // Adds the content of one JSON file. Returns false when the file could not be read at all.
        public bool AddJson(string json, string source)
        {
            JObject root;
            try { root = JObject.Parse(json); }
            catch (Exception e)
            {
                Fail($"Story file '{source}' is not valid JSON: {e.Message}");
                return false;
            }
            _sources.Add(source);
            Merge(root, "dialogues", source, (DialogueGraph d) => d.Id, _dialogues);
            Merge(root, "sets", source, (DialogueSet s) => s.Id, _sets);
            Merge(root, "quests", source, (QuestDefinition q) => q.Id, _quests);
            Merge(root, "letters", source, (LetterDefinition l) => l.Id, _letters);
            AddEventTemplates(root, source);
            Merge(root, "events", source, (EventDefinition e) => e.Id, _events);
            AddEventsFromTemplates(root, source);
            Merge(root, "randomEvents", source, (RandomEventDefinition r) => r.Id, _randomEvents);
            Merge(root, "boardJobs", source, (BoardJobTemplate b) => b.Id, _jobs);
            Merge(root, "reactions", source, (ReactionDefinition r) => r.Id, _reactions);
            Merge(root, "topics", source, (TopicDefinition t) => t.Id, _topics);
            Merge(root, "socials", source, (SocialProfile p) => p.Npc, _socials);
            Merge(root, "storylines", source, (StorylineDefinition s) => s.Id, _storylines);
            AddSetEntries(root, source);
            return true;
        }

        // "eventTemplates" and "eventsFromTemplates": see EventTemplates. Templates may come from any earlier file.
        void AddEventTemplates(JObject root, string source)
        {
            if (!root.TryGetValue("eventTemplates", StringComparison.OrdinalIgnoreCase, out var token) || !(token is JArray array)) return;
            foreach (var item in array.OfType<JObject>())
            {
                var id = (string)item["id"];
                if (string.IsNullOrEmpty(id)) { Fail($"{source}: an event template has no id"); continue; }
                if (_eventTemplates.ContainsKey(id)) { Fail($"{source}: 'eventTemplates' redefines '{id}'; skipped"); continue; }
                _eventTemplates[id] = item;
            }
        }

        void AddEventsFromTemplates(JObject root, string source)
        {
            if (!root.TryGetValue("eventsFromTemplates", StringComparison.OrdinalIgnoreCase, out var token) || !(token is JArray array)) return;
            foreach (var item in array.OfType<JObject>())
            {
                var templateId = (string)item["template"];
                if (string.IsNullOrEmpty(templateId) || !_eventTemplates.TryGetValue(templateId, out var template))
                { Fail($"{source}: event '{(string)item["id"]}' uses unknown template '{templateId}'"); continue; }
                if (!EventTemplates.TryExpand(template, item, out var expanded, out var error)) { Fail($"{source}: {error}"); continue; }
                EventDefinition value;
                try { value = expanded.ToObject<EventDefinition>(); }
                catch (Exception e) { Fail($"{source}: event '{(string)item["id"]}' from '{templateId}' is not a valid event: {e.Message}"); continue; }
                if (_events.ContainsKey(value.Id)) { Fail($"{source}: 'eventsFromTemplates' redefines '{value.Id}'; skipped"); continue; }
                _events[value.Id] = value;
            }
        }

        // "setEntries": [{ "set": "npc.tilda.talk", "entries": [...] }] adds entries to a dialogue set from another file (an optional
        // layer's lines for an existing villager).
        void AddSetEntries(JObject root, string source)
        {
            if (!root.TryGetValue("setEntries", StringComparison.OrdinalIgnoreCase, out var token) || !(token is JArray array)) return;
            foreach (var item in array)
            {
                var id = (string)item["set"];
                if (!_sets.TryGetValue(id ?? string.Empty, out var set)) { Fail($"{source}: setEntries for unknown set '{id}'"); continue; }
                var entries = item["entries"]?.ToObject<List<DialogueSetEntry>>();
                if (entries != null) set.Entries.AddRange(entries);
            }
        }

        void Merge<T>(JObject root, string property, string source, Func<T, string> idOf, Dictionary<string, T> into)
        {
            if (!root.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out var token) || !(token is JArray array)) return;
            foreach (var item in array)
            {
                T value;
                try { value = item.ToObject<T>(); }
                catch (Exception e) { Fail($"{source}: bad entry in '{property}': {e.Message}"); continue; }
                var id = value == null ? null : idOf(value);
                if (string.IsNullOrEmpty(id)) { Fail($"{source}: an entry in '{property}' has no id"); continue; }
                if (into.ContainsKey(id)) { Fail($"{source}: '{property}' redefines '{id}'; skipped"); continue; }
                into[id] = value;
            }
        }

        void Fail(string message)
        {
            Errors.Add(message);
            Log.Error("[Story] " + message);
        }
    }
}
