using System;
using System.Collections.Generic;
using Farm.Core;

namespace Farm.Gameplay
{
    // ---- data (loaded from JSON, see ADR 0003) -----------------------------------------------------------------

    [Serializable]
    public sealed class DialogueChoice
    {
        public string Text;                 // string key
        public string Condition;            // hidden while it does not hold (dread can remove favourable options)
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
    }

    // ---- running a conversation ----------------------------------------------------------------------------------

    public sealed class DialogueOption
    {
        public readonly int Index;       // index into the node's Choices
        public readonly string Text;
        public DialogueOption(int index, string text) { Index = index; Text = text; }
    }

    public sealed class DialogueLine
    {
        public string Speaker;
        public string Text;
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

                var line = new DialogueLine
                {
                    Speaker = node.Speaker ?? string.Empty,
                    Text = string.IsNullOrEmpty(node.Text) ? string.Empty : _text(node.Text, node.Args.ToArray()),
                };
                for (var i = 0; i < node.Choices.Count; i++)
                {
                    var c = node.Choices[i];
                    if (!Conditions.TryEvaluate(c.Condition, _world, out var visible) || !visible) continue;
                    line.Options.Add(new DialogueOption(i, _text(c.Text, new object[0])));
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
