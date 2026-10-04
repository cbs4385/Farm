using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // ---- data (loaded from JSON, see ADR 0003) -----------------------------------------------------------------

    [Serializable]
    public sealed class DialogueChoice
    {
        public string Text;                 // string key
        public string Condition;            // hidden while it does not hold (dread can remove favourable options)
        public string Tone;                 // optional: kind, honest, playful, shy, curt (shown as a small icon; never a hidden penalty)
        public bool Default;                // optional: the choice that starts highlighted, so a habitual Enter picks it (the chat menu uses Goodbye)
        public List<string> Effects = new List<string>();
        public string Next;                 // node id; empty ends the conversation
    }

    [Serializable]
    public sealed class DialogueNode
    {
        public string Id;
        public string Condition;            // a node whose condition fails is skipped to Next
        public string Speaker;              // npc id; empty = narration
        public string Text;                 // string key; more lines follow via Next
        public string Expression;           // optional: neutral, happy, sad, surprised, embarrassed, thinking (portrait variant)
        public string Emote;                // optional: heart, note, sweat, exclaim, question, ellipsis, sparkle, zzz (bubble over the speaker)
        public string Sfx;                  // optional sound id played when the line appears
        public string Voice;                // optional voice-blip set id (default: the speaker's)
        public string Camera;               // optional: speaker, player, wide (used by scenes)
        public string Tag;                  // optional moment tag: funny, wholesome, surprise, mystery
        public List<object> Args = new List<object>();
        public List<string> Effects = new List<string>();   // run when the node is shown
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
        public string Next;
    }

    [Serializable]
    public sealed class DialogueGraph
    {
        public string Id;
        public string Start;
        public List<DialogueNode> Nodes = new List<DialogueNode>();

        Dictionary<string, DialogueNode> _byId;

        public DialogueNode Node(string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, DialogueNode>();
                foreach (var n in Nodes) if (n != null && !string.IsNullOrEmpty(n.Id)) _byId[n.Id] = n;
            }
            return !string.IsNullOrEmpty(id) && _byId.TryGetValue(id, out var node) ? node : null;
        }
    }

    [Serializable]
    public sealed class DialogueSetEntry
    {
        public string Condition;
        public int Priority;
        public string Dialogue;
        public string Rarity;               // common (default), uncommon, rare, legendary: see LineRarity
        public float Weight = 1f;           // multiplies the rarity weight
        public int Cooldown = -1;           // days before the line may repeat; -1 = the band default (DialogueSet.DefaultCooldownDays)
        public string Tag;                  // moment tag for quotas and the validator: funny, wholesome, surprise, mystery
        public string Category;             // free text for reports: greeting, idle, seasonal, tier, bark ...
    }

    // Which dialogue to play: the highest-priority entries whose condition holds form a pool; one is picked from `seed`.
    [Serializable]
    public sealed class DialogueSet
    {
        public string Id;
        public List<DialogueSetEntry> Entries = new List<DialogueSetEntry>();

        public string Pick(IWorldQuery world, int seed)
        {
            var best = int.MinValue;
            var pool = new List<DialogueSetEntry>();
            foreach (var e in Entries)
            {
                if (e == null || string.IsNullOrEmpty(e.Dialogue)) continue;
                if (!Conditions.TryEvaluate(e.Condition, world, out var ok) || !ok) continue;
                if (e.Priority > best) { best = e.Priority; pool.Clear(); }
                if (e.Priority == best) pool.Add(e);
            }
            if (pool.Count == 0) return null;
            return pool[(int)((uint)seed % (uint)pool.Count)].Dialogue;
        }

        // ---- variety (T-090) -----------------------------------------------------------------------------------

        // Priorities from this value up are story beats (quest offers, one-shot scenes, layers): they never go on cooldown.
        public const int StoryBandPriority = 4;
        public const int DefaultCooldownDays = 14;
        const float NeverHeardBoost = 5f;

        // Rarity is a chance per visit, not a weight in the pool: a villager with a hundred ordinary lines would otherwise almost never say a
        // rare one. On each visit there is a 1.5% chance of a legendary line and a 5% chance of a rare one, if one is eligible and fresh (and
        // not a story beat); lines never said are preferred. A player who talks to a villager daily hears each of their rare lines in a few
        // months, and a legendary one now and then.
        public const double LegendaryChance = 0.015, RareChance = 0.05;

        public static int CooldownOf(DialogueSetEntry e) =>
            e.Cooldown >= 0 ? e.Cooldown : e.Priority >= StoryBandPriority ? 0 : DefaultCooldownDays;

        static float WeightOf(DialogueSetEntry e) => Math.Max(0.0001f, e.Weight) * LineRarity.Weight(e.Rarity);

        // Deterministic value in [0, 1) from a seed (splitmix-style mixing, so neighbouring days are unrelated).
        public static double Unit(int seed)
        {
            unchecked
            {
                var x = (ulong)(uint)seed + 0x9E3779B97F4A7C15UL;
                x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
                x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
                x ^= x >> 31;
                return (x >> 11) / (double)(1UL << 53);
            }
        }

        // Chooses what a villager says, and remembers it. Rules, in order:
        //  1. Talking again on the same day repeats the same line, unless a higher tier now has a fresh line.
        //  2. Tiers (priorities) are tried from the highest. A line on cooldown (said within its cooldown days) is skipped.
        //  3. The first tier with any fresh line decides; the choice is weighted by rarity and favours lines never said.
        //  4. If every line of every tier is on cooldown, the least recently said line of the top tier is used.
        // `scope` is normally the set id. The result is deterministic for the same seed and memory.
        public string PickVaried(IWorldQuery world, int seed, LineMemory memory, string scope, int today, IEnumerable<DialogueSetEntry> extra = null)
        {
            if (memory == null) return Pick(world, seed);
            scope = scope ?? Id ?? string.Empty;

            var eligible = new List<DialogueSetEntry>();
            foreach (var e in extra == null ? Entries : Entries.Concat(extra))
            {
                if (e == null || string.IsNullOrEmpty(e.Dialogue)) continue;
                if (!Conditions.TryEvaluate(e.Condition, world, out var ok) || !ok) continue;
                eligible.Add(e);
            }
            if (eligible.Count == 0) return null;

            bool Fresh(DialogueSetEntry e)
            {
                var last = memory.LastHeardDay(scope, e.Dialogue);
                var cooldown = CooldownOf(e);
                return last < 0 || cooldown == 0 || today - last >= cooldown;
            }

            var previous = memory.LastOf(scope);
            if (previous != null && previous.Day == today)
            {
                var again = eligible.FirstOrDefault(e => e.Dialogue == previous.Dialogue);
                if (again != null && !eligible.Any(e => e.Priority > previous.Priority && Fresh(e)))
                    return again.Dialogue;
            }

            var chosen = (DialogueSetEntry)null;
            var rarity = Unit(unchecked(seed * 31 + 17));
            // A queued reaction, a story window or a scene (priority 3 and up) keeps its claim on the visit: rarity only decorates ordinary chat.
            var urgent = eligible.Any(e => e.Priority >= 3 && Fresh(e));
            if (!urgent && rarity < LegendaryChance + RareChance)
            {
                var wanted = rarity < LegendaryChance ? LineRarity.Legendary : LineRarity.Rare;
                var specials = eligible.Where(e => e.Rarity == wanted && e.Priority < StoryBandPriority && Fresh(e)).ToList();
                if (specials.Count > 0) chosen = WeightedPick(specials, unchecked(seed * 7 + 3), memory, scope);
            }
            if (chosen == null)
            foreach (var tier in eligible.Select(e => e.Priority).Distinct().OrderByDescending(p => p))
            {
                var fresh = eligible.Where(e => e.Priority == tier && Fresh(e)).ToList();
                if (fresh.Count == 0) continue;
                chosen = WeightedPick(fresh, seed, memory, scope);
                break;
            }
            if (chosen == null)
            {
                var top = eligible.Max(e => e.Priority);
                var tierEntries = eligible.Where(e => e.Priority == top).ToList();
                var oldest = tierEntries.Min(e => memory.LastHeardDay(scope, e.Dialogue));
                chosen = WeightedPick(tierEntries.Where(e => memory.LastHeardDay(scope, e.Dialogue) == oldest).ToList(), seed, memory, scope);
            }
            memory.Record(scope, chosen.Dialogue, today, chosen.Priority);
            return chosen.Dialogue;
        }

        static DialogueSetEntry WeightedPick(List<DialogueSetEntry> candidates, int seed, LineMemory memory, string scope)
        {
            var weights = new float[candidates.Count];
            var total = 0f;
            for (var i = 0; i < candidates.Count; i++)
            {
                weights[i] = WeightOf(candidates[i]) * (memory.WasHeard(scope, candidates[i].Dialogue) ? 1f : NeverHeardBoost);
                total += weights[i];
            }
            var roll = Unit(seed) * total;
            for (var i = 0; i < candidates.Count; i++)
            {
                roll -= weights[i];
                if (roll < 0) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }
    }

    // ---- running a conversation ----------------------------------------------------------------------------------

    public sealed class DialogueOption
    {
        public readonly int Index;       // index into the node's Choices
        public readonly string Text;
        public readonly string Tone;
        public readonly bool IsDefault;
        public DialogueOption(int index, string text, string tone = null, bool isDefault = false) { Index = index; Text = text; Tone = tone ?? string.Empty; IsDefault = isDefault; }
    }

    public sealed class DialogueLine
    {
        public string Speaker;
        public string Text;
        public List<TextCue> Cues = new List<TextCue>();       // pauses and speed changes inside Text
        public string Expression = string.Empty, Emote = string.Empty, Sfx = string.Empty, Voice = string.Empty, Camera = string.Empty, Tag = string.Empty;
        public List<DialogueOption> Options = new List<DialogueOption>();
        public bool HasOptions => Options.Count > 0;
    }

    // Pure state machine over a DialogueGraph. The UI calls Advance (no options) or Choose (options) until Finished.
    public sealed class DialogueRunner
    {
        const int MaxSkippedNodes = 200;

        readonly DialogueGraph _graph;
        readonly IWorldQuery _world;
        readonly Func<string, object[], string> _text;
        readonly Action<string> _effect;
        DialogueNode _node;

        public DialogueRunner(DialogueGraph graph, IWorldQuery world, Func<string, object[], string> text, Action<string> runEffect)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _world = world;
            _text = text ?? ((k, a) => k);
            _effect = runEffect ?? (_ => { });
            Enter(graph.Start);
        }

        public bool Finished { get; private set; }
        public DialogueLine Current { get; private set; }
        public string DialogueId => _graph.Id;

        // Continue after a line without options.
        public void Advance()
        {
            if (Finished || Current == null || Current.HasOptions) return;
            Enter(_node.Next);
        }

        public void Choose(int optionIndex)
        {
            if (Finished || Current == null || optionIndex < 0 || optionIndex >= Current.Options.Count) return;
            var choice = _node.Choices[Current.Options[optionIndex].Index];
            foreach (var e in choice.Effects) _effect(e);
            Enter(choice.Next);
        }

        void Enter(string nodeId)
        {
            for (var guard = 0; guard < MaxSkippedNodes; guard++)
            {
                var node = _graph.Node(nodeId);
                if (node == null) { Finish(); return; }
                if (!Conditions.TryEvaluate(node.Condition, _world, out var ok) || !ok) { nodeId = node.Next; continue; }

                _node = node;
                foreach (var e in node.Effects) _effect(e);

                var seed = NpcInteractions.StableHash(_graph.Id + "/" + node.Id) + (_world != null ? _world.Now.TotalDays * 7919 : 0);
                var rich = RichText.Process(string.IsNullOrEmpty(node.Text) ? string.Empty : _text(node.Text, node.Args.ToArray()), _world, seed);
                var line = new DialogueLine
                {
                    Speaker = node.Speaker ?? string.Empty,
                    Text = rich.Text,
                    Expression = node.Expression ?? string.Empty, Emote = node.Emote ?? string.Empty, Sfx = node.Sfx ?? string.Empty,
                    Voice = node.Voice ?? string.Empty, Camera = node.Camera ?? string.Empty, Tag = node.Tag ?? string.Empty,
                };
                line.Cues.AddRange(rich.Cues);
                for (var i = 0; i < node.Choices.Count; i++)
                {
                    var c = node.Choices[i];
                    if (!Conditions.TryEvaluate(c.Condition, _world, out var visible) || !visible) continue;
                    line.Options.Add(new DialogueOption(i, RichText.Process(_text(c.Text, new object[0]), _world, seed + i).Text, c.Tone, c.Default));
                }
                // A node with neither text nor options is just a place to run effects: move on.
                if (string.IsNullOrEmpty(node.Text) && !line.HasOptions) { nodeId = node.Next; continue; }
                Current = line;
                return;
            }
            Finish();
        }

        void Finish()
        {
            Finished = true;
            Current = null;
        }
    }
}
