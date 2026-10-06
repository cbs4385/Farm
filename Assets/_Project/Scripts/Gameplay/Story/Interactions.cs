using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // ---- data ---------------------------------------------------------------------------------------------------

    // An optional subject a villager will talk about when asked (T-097). Story JSON: `topics`.
    [Serializable]
    public sealed class TopicDefinition
    {
        public string Id;
        public string Npc;
        public string LabelKey;             // the menu entry, for example "Ask about the latest gossip"
        public string Dialogue;             // played when chosen
        public string Condition;
        public int Priority = 2;            // higher topics are offered first (at most two a visit)
        public bool Once;                   // only ever asked once
        public int CooldownDays = 1;        // days before it can be asked again (1 = once a day)
    }

    // How a villager takes the social actions (T-141). Story JSON: `socials`.
    [Serializable]
    public sealed class SocialProfile
    {
        public string Npc;
        public List<string> Loves = new List<string>();
        public List<string> Likes = new List<string>();
        public List<string> Dislikes = new List<string>();
    }

    public enum SocialOutcome { Flop, Meh, Good, Great }

    // What has been asked and done (module data; nothing is added to GameState).
    [Serializable]
    public sealed class InteractionState
    {
        public const string ModuleId = "interactions";
        public Dictionary<string, int> TopicLastAsked = new Dictionary<string, int>();
        public Dictionary<string, int> TopicTimes = new Dictionary<string, int>();
        public Dictionary<string, int> SocialDay = new Dictionary<string, int>();      // villager -> day of the last action
        public Dictionary<string, int> SocialCount = new Dictionary<string, int>();    // villager -> actions that day
        public Dictionary<string, int> GiftDay = new Dictionary<string, int>();        // "villager|item" -> the last day that item was given

        public static InteractionState Load(GameSession s) => s.GetModuleData<InteractionState>(ModuleId);
        public static void Store(GameSession s, InteractionState i) => s.SetModuleData(ModuleId, i);

        public int SocialToday(string npc, int today) =>
            SocialDay.TryGetValue(npc, out var d) && d == today && SocialCount.TryGetValue(npc, out var n) ? n : 0;

        public void RecordSocial(string npc, int today)
        {
            SocialCount[npc] = SocialToday(npc, today) + 1;
            SocialDay[npc] = today;
        }
    }

    // ---- social actions ---------------------------------------------------------------------------------------------

    public static class SocialActions
    {
        public const string Joke = "joke", Compliment = "compliment", Advice = "advice", Tease = "tease";
        public static readonly string[] All = { Joke, Compliment, Advice, Tease };
        public const int MaxPerDay = 2;
        public static readonly int[] Points = { 0, 3, 8, 14 };      // Flop, Meh, Good, Great

        public static bool IsAction(string action) => Array.IndexOf(All, action) >= 0;

        // The chances of Flop, Meh, Good, Great (percent) for a villager who loves, likes, is neutral to, or dislikes an action.
        static readonly int[][] Odds =
        {
            new[] { 0, 10, 40, 50 },     // loves
            new[] { 5, 25, 50, 20 },     // likes
            new[] { 15, 45, 35, 5 },     // neutral
            new[] { 50, 40, 10, 0 },     // dislikes
        };

        public static int Affinity(SocialProfile profile, string action)
        {
            if (profile == null) return 2;
            if (profile.Loves.Contains(action)) return 0;
            if (profile.Likes.Contains(action)) return 1;
            if (profile.Dislikes.Contains(action)) return 3;
            return 2;
        }

        // Deterministic: the same villager, action, day and number of earlier actions always give the same outcome.
        public static SocialOutcome Roll(SocialProfile profile, string action, int seed)
        {
            var odds = Odds[Affinity(profile, action)];
            var roll = DialogueSet.Unit(seed) * 100.0;
            for (var outcome = 0; outcome < odds.Length; outcome++)
            {
                roll -= odds[outcome];
                if (roll < 0) return (SocialOutcome)outcome;
            }
            return SocialOutcome.Meh;
        }

        public static int Seed(string npc, string action, int today, int doneToday) =>
            NpcInteractions.StableHash(npc + "/" + action) + today * 7919 + doneToday * 104729;

        // The second action of a day is worth half; a flop is funny but never costs friendship.
        public static int PointsFor(SocialOutcome outcome, int doneToday) =>
            doneToday <= 0 ? Points[(int)outcome] : doneToday == 1 ? Points[(int)outcome] / 2 : 0;

        public static string OutcomeName(SocialOutcome o) => o.ToString().ToLowerInvariant();

        // The reaction dialogue for a villager, falling back to the generic one; null when neither exists.
        public static string ReactionDialogue(StoryContent story, string npc, string action, SocialOutcome outcome)
        {
            var specific = $"social.{npc}.{action}.{OutcomeName(outcome)}";
            if (story.Dialogue(specific) != null) return specific;
            var generic = $"social.{action}.{OutcomeName(outcome)}";
            return story.Dialogue(generic) != null ? generic : null;
        }
    }

    // ---- topics ------------------------------------------------------------------------------------------------------

    public static class Topics
    {
        public const int MaxOffered = 2;

        public static List<TopicDefinition> Available(IEnumerable<TopicDefinition> topics, IWorldQuery world, InteractionState state, string npc, int today)
        {
            var list = new List<TopicDefinition>();
            if (topics == null) return list;
            foreach (var t in topics)
            {
                if (t == null || t.Npc != npc || string.IsNullOrEmpty(t.Dialogue)) continue;
                if (!Conditions.TryEvaluate(t.Condition, world, out var ok) || !ok) continue;
                if (state.TopicTimes.TryGetValue(t.Id, out var times) && times > 0 && t.Once) continue;
                if (state.TopicLastAsked.TryGetValue(t.Id, out var last) && today - last < Math.Max(1, t.CooldownDays)) continue;
                list.Add(t);
            }
            // Highest priority first; among equals, the one asked least; then a stable shuffle by day.
            return list
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => state.TopicTimes.TryGetValue(t.Id, out var n) ? n : 0)
                .ThenBy(t => DialogueSet.Unit(NpcInteractions.StableHash(t.Id) + today * 31))
                .Take(MaxOffered)
                .ToList();
        }
    }

    // ---- the menu after a chat ------------------------------------------------------------------------------------

    // Builds the "anything else?" conversation: the villager's topics, a "do something together" submenu with the social
    // actions, and "goodbye". It is an ordinary dialogue graph, made on the fly by copying the topic and reaction
    // dialogues into it, so the dialogue box needs nothing new.
    public static class InteractionMenu
    {
        public const string PromptKey = "menu.prompt", SocialPromptKey = "menu.social_prompt", SocialKey = "menu.social",
            GoodbyeKey = "menu.goodbye", BackKey = "menu.back", MoreKey = "menu.more";

        public const string MoreVar = "chat.more";         // set by the "keep chatting" choice; read when the menu closes

        public static string SocialKeyFor(string action) => "social." + action;

        // Null when there is nothing to offer. `hasKey` tells whether a text key exists (for per-villager prompts).
        public static DialogueGraph Build(StoryContent story, string npc, IWorldQuery world, InteractionState state, int today, Func<string, bool> hasKey)
        {
            var topics = Topics.Available(story.Topics, world, state, npc, today);
            var profile = story.Socials.FirstOrDefault(p => p.Npc == npc);
            var doneToday = state.SocialToday(npc, today);
            var actions = new List<(string action, SocialOutcome outcome, string dialogue)>();
            if (doneToday < SocialActions.MaxPerDay)
                foreach (var action in SocialActions.All)
                {
                    var outcome = SocialActions.Roll(profile, action, SocialActions.Seed(npc, action, today, doneToday));
                    var dialogue = SocialActions.ReactionDialogue(story, npc, action, outcome);
                    if (dialogue != null) actions.Add((action, outcome, dialogue));
                }
            if (topics.Count == 0 && actions.Count == 0) return null;

            var graph = new DialogueGraph { Id = "menu." + npc, Start = "menu" };
            var promptKey = hasKey != null && hasKey(PromptKey + "." + npc) ? PromptKey + "." + npc : PromptKey;
            var menu = new DialogueNode { Id = "menu", Speaker = npc, Text = promptKey };
            graph.Nodes.Add(menu);

            foreach (var topic in topics)
            {
                var source = story.Dialogue(topic.Dialogue);
                if (source == null) continue;
                var prefix = "topic:" + topic.Id + "/";
                CopyInto(graph, source, prefix, npc);
                menu.Choices.Add(new DialogueChoice
                {
                    Text = topic.LabelKey ?? "topic." + topic.Id,
                    Effects = { "topic.done:" + topic.Id },
                    Next = prefix + source.Start,
                });
            }

            if (actions.Count > 0)
            {
                menu.Choices.Add(new DialogueChoice { Text = SocialKey, Next = "social" });
                var social = new DialogueNode { Id = "social", Speaker = npc, Text = SocialPromptKey };
                graph.Nodes.Add(social);
                foreach (var (action, outcome, dialogue) in actions)
                {
                    var prefix = "social:" + action + "/";
                    var source = story.Dialogue(dialogue);
                    CopyInto(graph, source, prefix, npc);
                    social.Choices.Add(new DialogueChoice
                    {
                        Text = SocialKeyFor(action),
                        Effects = { $"social.done:{npc},{action},{SocialActions.OutcomeName(outcome)}" },
                        Next = prefix + source.Start,
                    });
                }
                social.Choices.Add(new DialogueChoice { Text = BackKey, Next = "menu", Default = true });
            }

            menu.Choices.Add(new DialogueChoice { Text = GoodbyeKey, Default = true });
            return graph;
        }

        // A conversation never just stops: every menu offers "keep chatting" (another line from the same villager) next to Goodbye.
        public static DialogueGraph WithMore(DialogueGraph graph, string npc)
        {
            var menu = graph.Nodes.Find(n => n.Id == "menu");
            if (menu == null || menu.Choices.Exists(c => c.Text == MoreKey)) return graph;
            var at = menu.Choices.FindIndex(c => c.Default);
            var more = new DialogueChoice { Text = MoreKey, Effects = { "talk.more" } };
            if (at < 0) menu.Choices.Add(more); else menu.Choices.Insert(at, more);
            return graph;
        }

        // What is offered when there are no topics or social actions left today: just "keep chatting" and Goodbye.
        public static DialogueGraph Minimal(string npc, Func<string, bool> hasKey)
        {
            var graph = new DialogueGraph { Id = "menu." + npc, Start = "menu" };
            var promptKey = hasKey != null && hasKey(PromptKey + "." + npc) ? PromptKey + "." + npc : PromptKey;
            var menu = new DialogueNode { Id = "menu", Speaker = npc, Text = promptKey };
            menu.Choices.Add(new DialogueChoice { Text = GoodbyeKey, Default = true });
            graph.Nodes.Add(menu);
            return WithMore(graph, npc);
        }

        // Copies a dialogue's nodes into `into` with prefixed ids. A speaker of "*" means the villager the menu is for.
        static void CopyInto(DialogueGraph into, DialogueGraph source, string prefix, string npc)
        {
            foreach (var n in source.Nodes)
            {
                var copy = new DialogueNode
                {
                    Id = prefix + n.Id, Condition = n.Condition, Speaker = n.Speaker == "*" ? npc : n.Speaker, Text = n.Text,
                    Expression = n.Expression, Emote = n.Emote, Sfx = n.Sfx, Voice = n.Voice, Camera = n.Camera, Tag = n.Tag,
                    Next = string.IsNullOrEmpty(n.Next) ? null : prefix + n.Next,
                };
                copy.Args.AddRange(n.Args);
                copy.Effects.AddRange(n.Effects);
                foreach (var c in n.Choices)
                    copy.Choices.Add(new DialogueChoice
                    {
                        Text = c.Text, Condition = c.Condition, Tone = c.Tone, Default = c.Default,
                        Next = string.IsNullOrEmpty(c.Next) ? null : prefix + c.Next, Effects = new List<string>(c.Effects),
                    });
                into.Nodes.Add(copy);
            }
        }

        // ---- effects and session wiring ----

        public static void RegisterEffects()
        {
            Effects.Register("topic.done", 1, 1, (s, a) =>
            {
                var state = InteractionState.Load(s);
                var today = s.Clock.Now.TotalDays;
                state.TopicLastAsked[a[0]] = today;
                state.TopicTimes[a[0]] = (state.TopicTimes.TryGetValue(a[0], out var n) ? n : 0) + 1;
                InteractionState.Store(s, state);
            });
            Effects.Register("talk.more", 0, 0, (s, a) => s.SetVar(MoreVar, 1));
            Effects.Register("social.done", 3, 3, (s, a) =>
            {
                var state = InteractionState.Load(s);
                var today = s.Clock.Now.TotalDays;
                var done = state.SocialToday(a[0], today);
                if (!Enum.TryParse<SocialOutcome>(a[2], true, out var outcome)) outcome = SocialOutcome.Meh;
                state.RecordSocial(a[0], today);
                InteractionState.Store(s, state);
                var points = SocialActions.PointsFor(outcome, done);
                NpcInteractions.StateOf(s.State, a[0]).LastContactDay = today;
                if (points > 0) NpcInteractions.AddPoints(s, a[0], points);
                s.AddVar("stat.social", 1);
            });
        }

        // After a normal chat: offer the menu. Not after story beats (first meetings, quest offers, scenes).
        public static void Offer(GameSession session, NpcDefinition npc, int pickedPriority)
        {
            if (pickedPriority >= DialogueSet.StoryBandPriority) return;
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            if (settings != null && !settings.ChatMenu) return;
            var state = InteractionState.Load(session);
            var graph = Build(session.Story, npc.Id, session.World, state, session.Clock.Now.TotalDays, L.Has) ?? Minimal(npc.Id, L.Has);
            session.SetVar(MoreVar, 0);
            session.BeginDialogueGraph(WithMore(graph, npc.Id), () =>
            {
                if (session.GetVar(MoreVar) == 0) return;
                session.SetVar(MoreVar, 0);
                NpcInteractions.Talk(session, npc);               // another line, then the menu again
            });
        }
    }
}
