using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Farm.Gameplay
{
    // Entries added to a dialogue set that another file defines (a villager's talk set gets new lines from a writer's file).
    public sealed class SetAddition
    {
        public string Set;
        public readonly List<DialogueSetEntry> Entries = new List<DialogueSetEntry>();
    }

    public sealed class FScriptResult
    {
        public readonly List<DialogueGraph> Dialogues = new List<DialogueGraph>();
        public readonly List<DialogueSet> Sets = new List<DialogueSet>();
        public readonly List<TopicDefinition> Topics = new List<TopicDefinition>();
        public readonly List<SocialProfile> Socials = new List<SocialProfile>();
        public readonly List<SetAddition> SetAdditions = new List<SetAddition>();
        public readonly List<ReactionDefinition> Reactions = new List<ReactionDefinition>();
        public readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        public readonly List<string> Errors = new List<string>();
        public bool Ok => Errors.Count == 0;
    }

    // FScript (T-135): a plain-text, screenplay-style way to write dialogue and dialogue sets, compiled to the story JSON
    // (ADR 0003) and string-table entries. Writers never type text keys: they are generated from the dialogue and node
    // ids, using the convention the existing content already follows. See docs/narrative/FSCRIPT.md.
    //
    //   # a comment (whole lines only)
    //   dialogue tilda.first [start=n0]
    //   n0 tilda: Well, hello there! You must be the new farmer.      <- node: id, speaker ("-" = narration), text
    //     + flag:met.tilda                                            <- an effect run when the node is shown
    //     if hearts:tilda>=3                                          <- node condition (skipped to next when false)
    //     next n1
    //   n1 tilda: Seeds, gossip and a backpack.
    //     ? Nice to meet you. => r0 | friend:tilda,10                 <- choice: text => next | effects
    //     ? What do you sell? => r1 if has:crop.parsnip>=1            <- optional "if condition" after the target
    //   r0 tilda: The pleasure's mine, [player].
    //   r1 tilda: Seeds, mostly.
    //
    //   set npc.tilda.talk
    //     100 !flag:met.tilda => tilda.first
    //     0 season:spring => tilda.sea_spring rarity=rare weight=2 cooldown=10 tag=funny cat=seasonal
    //
    // A line may use \n for a line break and \\ for a backslash. A `key some.key` line under a node or choice overrides
    // the generated text key (kept so existing content round-trips).
    public static class FScript
    {
        static readonly string[] Directives = { "next", "if", "key", "expr", "emote", "sfx", "voice", "camera", "tag", "tone" };

        public static string KeyForNode(string dialogueId, string nodeId) =>
            "dlg." + dialogueId + "." + (IsNumbered(nodeId) ? nodeId.Substring(1) : nodeId);

        public static string KeyForChoice(string dialogueId, int index) => "dlg." + dialogueId + ".c" + index;

        static bool IsNumbered(string nodeId)
        {
            if (nodeId.Length < 2 || nodeId[0] != 'n') return false;
            for (var i = 1; i < nodeId.Length; i++) if (!char.IsDigit(nodeId[i])) return false;
            return true;
        }

        // ---- compiling -------------------------------------------------------------------------------------------

        public static FScriptResult Compile(string source, string name = "fscript") => new Compiler(source, name).Run();

        // One compile: the parser's place in the script (the block it is inside) lives in fields, so each line is handled by a method of its own.
        sealed class Compiler
        {
            readonly string name;
            readonly string[] lines;
            readonly FScriptResult result = new FScriptResult();
            DialogueGraph graph;
            DialogueSet set;
            TopicDefinition topic;
            SocialProfile social;
            SetAddition addTo;
            ReactionDefinition reaction;
            DialogueNode node;
            DialogueChoice lastChoice;
            int choiceIndex;
            readonly Dictionary<DialogueNode, string> nodeKeys = new Dictionary<DialogueNode, string>();
            readonly Dictionary<DialogueChoice, string> choiceKeys = new Dictionary<DialogueChoice, string>();
            readonly Dictionary<DialogueGraph, string> starts = new Dictionary<DialogueGraph, string>();
            readonly Dictionary<DialogueNode, string> nodeText = new Dictionary<DialogueNode, string>();
            readonly Dictionary<DialogueChoice, string> choiceText = new Dictionary<DialogueChoice, string>();

            public Compiler(string source, string name)
            {
                this.name = name;
                lines = (source ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            }

            void Err(int line, string message) => result.Errors.Add($"{name}:{line}: {message}");

            void Close(int line)
            {
                if (graph == null) return;
                Finish(graph, starts, nodeKeys, choiceKeys, result, name, line);
                graph = null; node = null; lastChoice = null;
            }

            public FScriptResult Run()
            {
                for (var i = 0; i < lines.Length; i++) ParseLine(i);
                Close(lines.Length);
                foreach (var kv in nodeKeys)
                {
                    if (!nodeText.TryGetValue(kv.Key, out var text)) continue;
                    kv.Key.Text = kv.Value;
                    AddText(result, kv.Value, text);
                }
                foreach (var kv in choiceKeys)
                {
                    kv.Key.Text = kv.Value;
                    AddText(result, kv.Value, choiceText[kv.Key]);
                }
                return result;
            }

            void ParseLine(int i)
            {
                var raw = lines[i];
                var text = raw.Trim();
                var lineNo = i + 1;
                if (text.Length == 0 || text[0] == '#') return;
                var word = FirstWord(text, out var rest);

                if (word == "dialogue")
                {
                    Close(lineNo); set = null; topic = null; social = null; addTo = null; reaction = null;
                    var parts = rest.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) { Err(lineNo, "dialogue needs an id"); return; }
                    graph = new DialogueGraph { Id = parts[0] };
                    choiceIndex = 0;
                    foreach (var attr in parts.Skip(1))
                    {
                        if (attr.StartsWith("start=", StringComparison.Ordinal)) starts[graph] = attr.Substring(6);
                        else Err(lineNo, $"unknown dialogue option '{attr}'");
                    }
                    if (result.Dialogues.Any(d => d.Id == graph.Id)) Err(lineNo, $"dialogue '{graph.Id}' is defined twice");
                    result.Dialogues.Add(graph);
                    return;
                }
                if (word == "topic")
                {
                    Close(lineNo); set = null; social = null; addTo = null; reaction = null;
                    if (string.IsNullOrWhiteSpace(rest)) { Err(lineNo, "topic needs an id"); topic = null; return; }
                    topic = new TopicDefinition { Id = rest.Trim() };
                    if (result.Topics.Any(t => t.Id == topic.Id)) Err(lineNo, $"topic '{topic.Id}' is defined twice");
                    result.Topics.Add(topic);
                    return;
                }
                if (word == "social")
                {
                    Close(lineNo); set = null; topic = null; addTo = null; reaction = null;
                    if (string.IsNullOrWhiteSpace(rest)) { Err(lineNo, "social needs a villager id"); social = null; return; }
                    social = new SocialProfile { Npc = rest.Trim() };
                    if (result.Socials.Any(p => p.Npc == social.Npc)) Err(lineNo, $"social profile '{social.Npc}' is defined twice");
                    result.Socials.Add(social);
                    return;
                }
                if (word == "say")
                {
                    Close(lineNo); set = null; topic = null; social = null; addTo = null; reaction = null;
                    var colonAt = rest.IndexOf(':');
                    var head = colonAt > 0 ? rest.Substring(0, colonAt).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
                    if (head.Length != 2) { Err(lineNo, "a one-line dialogue looks like 'say <dialogue id> <speaker>: text'"); return; }
                    AddOneLine(result, head[0], head[1], rest.Substring(colonAt + 1).TrimStart(), null, null, null, lineNo, Err);
                    return;
                }
                if (word == "addto")
                {
                    Close(lineNo); set = null; topic = null; social = null; reaction = null;
                    if (string.IsNullOrWhiteSpace(rest)) { Err(lineNo, "addto needs the id of an existing dialogue set"); addTo = null; return; }
                    addTo = new SetAddition { Set = rest.Trim() };
                    result.SetAdditions.Add(addTo);
                    return;
                }
                if (word == "reaction")
                {
                    Close(lineNo); set = null; topic = null; social = null; addTo = null;
                    if (string.IsNullOrWhiteSpace(rest)) { Err(lineNo, "reaction needs an id"); reaction = null; return; }
                    reaction = new ReactionDefinition { Id = rest.Trim() };
                    if (result.Reactions.Any(r => r.Id == reaction.Id)) Err(lineNo, $"reaction '{reaction.Id}' is defined twice");
                    result.Reactions.Add(reaction);
                    return;
                }
                if (word == "set")
                {
                    Close(lineNo); topic = null; social = null; addTo = null; reaction = null;
                    if (string.IsNullOrWhiteSpace(rest)) { Err(lineNo, "set needs an id"); return; }
                    set = new DialogueSet { Id = rest.Trim() };
                    if (result.Sets.Any(s => s.Id == set.Id)) Err(lineNo, $"set '{set.Id}' is defined twice");
                    result.Sets.Add(set);
                    return;
                }

                if (reaction != null) { ParseReactionLine(reaction, word, rest, lineNo, Err); return; }
                if (addTo != null) { ParseAddLine(addTo, text, lineNo, result, Err); return; }
                if (topic != null) { ParseTopicLine(topic, word, rest, lineNo, result, Err); return; }
                if (social != null) { ParseSocialLine(social, word, rest, lineNo, Err); return; }
                if (set != null) { ParseSetEntry(set, text, lineNo, Err); return; }
                if (graph == null) { Err(lineNo, "expected 'dialogue <id>' or 'set <id>'"); return; }

                if (word == "+")
                {
                    if (node == null) { Err(lineNo, "an effect needs a node above it"); return; }
                    node.Effects.Add(rest.Trim());
                }
                else if (word == "if")
                {
                    if (node == null) { Err(lineNo, "'if' needs a node above it"); return; }
                    node.Condition = rest.Trim();
                }
                else if (word == "next")
                {
                    if (node == null) { Err(lineNo, "'next' needs a node above it"); return; }
                    node.Next = rest.Trim();
                }
                else if (word == "expr" || word == "emote" || word == "sfx" || word == "voice" || word == "camera" || word == "tag")
                {
                    if (node == null) { Err(lineNo, $"'{word}' needs a node above it"); return; }
                    var value = rest.Trim();
                    switch (word)
                    {
                        case "expr": node.Expression = value; break;
                        case "emote": node.Emote = value; break;
                        case "sfx": node.Sfx = value; break;
                        case "voice": node.Voice = value; break;
                        case "camera": node.Camera = value; break;
                        default: node.Tag = value; break;
                    }
                }
                else if (word == "tone")
                {
                    if (lastChoice == null) { Err(lineNo, "'tone' needs a choice above it"); return; }
                    lastChoice.Tone = rest.Trim();
                }
                else if (word == "key")
                {
                    if (lastChoice != null) choiceKeys[lastChoice] = rest.Trim();
                    else if (node != null) nodeKeys[node] = rest.Trim();
                    else Err(lineNo, "'key' needs a node or choice above it");
                }
                else if (word == "?")
                {
                    if (node == null) { Err(lineNo, "a choice needs a node above it"); return; }
                    var choice = ParseChoice(rest, lineNo, Err);
                    if (choice == null) return;
                    node.Choices.Add(choice);
                    lastChoice = choice;
                    choiceKeys[choice] = KeyForChoice(graph.Id, choiceIndex++);
                    choiceText[choice] = Unescape(choice.Text);     // held until the key is final: a `key` line may follow
                }
                else
                {
                    var colon = text.IndexOf(':');
                    var space = text.IndexOf(' ');
                    if (space <= 0 || colon < space || Directives.Contains(word))
                    {
                        Err(lineNo, $"cannot understand '{Shorten(text)}'");
                        return;
                    }
                    var id = text.Substring(0, space);
                    var speaker = text.Substring(space + 1, colon - space - 1).Trim();
                    if (speaker.Length == 0 || speaker.Contains(" ")) { Err(lineNo, $"'{Shorten(text)}': write '<id> <speaker>: text' (use - for narration)"); return; }
                    var body = text.Substring(colon + 1).TrimStart();
                    if (graph.Nodes.Any(n => n.Id == id)) { Err(lineNo, $"node '{id}' is defined twice in '{graph.Id}'"); return; }
                    node = new DialogueNode { Id = id, Speaker = speaker == "-" ? null : speaker };
                    lastChoice = null;
                    if (body.Length > 0)
                    {
                        node.Text = KeyForNode(graph.Id, id);
                        nodeKeys[node] = node.Text;
                        nodeText[node] = Unescape(body);
                    }
                    graph.Nodes.Add(node);
                }
            }
        }

        static void AddText(FScriptResult result, string key, string text)
        {
            if (result.Texts.TryGetValue(key, out var existing) && existing != text)
                result.Errors.Add($"text key '{key}' is used for two different texts");
            result.Texts[key] = text;
        }

        static void Finish(DialogueGraph graph, Dictionary<DialogueGraph, string> starts, Dictionary<DialogueNode, string> nodeKeys,
            Dictionary<DialogueChoice, string> choiceKeys, FScriptResult result, string name, int line)
        {
            if (graph.Nodes.Count == 0) { result.Errors.Add($"{name}: dialogue '{graph.Id}' has no nodes"); return; }
            graph.Start = starts.TryGetValue(graph, out var start) ? start : graph.Nodes[0].Id;
            if (graph.Node(graph.Start) == null) result.Errors.Add($"{name}: dialogue '{graph.Id}': start '{graph.Start}' is not a node");
            foreach (var n in graph.Nodes)
            {
                if (!string.IsNullOrEmpty(n.Next) && graph.Node(n.Next) == null) result.Errors.Add($"{name}: dialogue '{graph.Id}': node '{n.Id}' goes to unknown node '{n.Next}'");
                foreach (var c in n.Choices)
                    if (!string.IsNullOrEmpty(c.Next) && graph.Node(c.Next) == null) result.Errors.Add($"{name}: dialogue '{graph.Id}': a choice in '{n.Id}' goes to unknown node '{c.Next}'");
                if (n.Choices.Count > 0 && !string.IsNullOrEmpty(n.Next)) result.Errors.Add($"{name}: dialogue '{graph.Id}': node '{n.Id}' has both choices and next");
            }
        }

        // topic <id> / npc <villager> / label <menu text> / plays <dialogue> / if <condition> / priority <n> / cooldown <days> / once
        static void ParseTopicLine(TopicDefinition topic, string word, string rest, int lineNo, FScriptResult result, Action<int, string> err)
        {
            rest = rest.Trim();
            switch (word)
            {
                case "npc": topic.Npc = rest; break;
                case "label":
                    topic.LabelKey = "topic." + topic.Id;
                    AddText(result, topic.LabelKey, Unescape(rest));
                    break;
                case "plays": topic.Dialogue = rest; break;
                case "if": topic.Condition = rest; break;
                case "once": topic.Once = true; break;
                case "priority":
                    if (int.TryParse(rest, out var p)) topic.Priority = p; else err(lineNo, $"priority '{rest}' is not a whole number");
                    break;
                case "cooldown":
                    if (int.TryParse(rest, out var c)) topic.CooldownDays = c; else err(lineNo, $"cooldown '{rest}' is not a whole number");
                    break;
                default: err(lineNo, $"unknown topic line '{word}'"); break;
            }
        }

        // One entry of an `addto` block: '<priority> <condition or ~> => <dialogue> [options] [:: text]'. With ':: text' the dialogue is made
        // for you: one line spoken by the set's villager (the set is npc.<villager>.talk). expr=, emote= and sfx= may be given.
        static void ParseAddLine(SetAddition addTo, string text, int lineNo, FScriptResult result, Action<int, string> err)
        {
            var textAt = text.IndexOf(" :: ", StringComparison.Ordinal);
            var entryText = textAt >= 0 ? text.Substring(0, textAt) : text;
            string expr = null, emote = null, sfx = null;
            if (textAt >= 0)
            {
                var arrow = entryText.IndexOf(" => ", StringComparison.Ordinal);
                if (arrow >= 0)
                {
                    var kept = new List<string>();
                    var tokens = entryText.Substring(arrow + 4).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var t in tokens)
                    {
                        if (t.StartsWith("expr=", StringComparison.Ordinal)) expr = t.Substring(5);
                        else if (t.StartsWith("emote=", StringComparison.Ordinal)) emote = t.Substring(6);
                        else if (t.StartsWith("sfx=", StringComparison.Ordinal)) sfx = t.Substring(4);
                        else kept.Add(t);
                    }
                    entryText = entryText.Substring(0, arrow + 4) + string.Join(" ", kept);
                }
            }
            var holder = new DialogueSet { Id = addTo.Set };
            ParseSetEntry(holder, entryText, lineNo, err);
            addTo.Entries.AddRange(holder.Entries);
            if (textAt < 0 || holder.Entries.Count == 0) return;

            var parts = addTo.Set.Split('.');
            if (parts.Length == 2 && parts[0] == "gazette")
            {
                // The Gazette's items are narration: no speaker.
                AddOneLine(result, holder.Entries[holder.Entries.Count - 1].Dialogue, "-", text.Substring(textAt + 4).TrimStart(), expr, emote, sfx, lineNo, err);
                return;
            }
            if (parts.Length != 3 || parts[0] != "npc" || (parts[2] != "talk" && parts[2] != "bark")) { err(lineNo, $"':: text' needs a set named npc.<villager>.talk or npc.<villager>.bark, not '{addTo.Set}'"); return; }
            AddOneLine(result, holder.Entries[holder.Entries.Count - 1].Dialogue, parts[1], text.Substring(textAt + 4).TrimStart(), expr, emote, sfx, lineNo, err);
        }

        static void AddOneLine(FScriptResult result, string id, string speaker, string text, string expr, string emote, string sfx, int lineNo, Action<int, string> err)
        {
            if (result.Dialogues.Any(d => d.Id == id)) { err(lineNo, $"dialogue '{id}' is defined twice"); return; }
            var node = new DialogueNode { Id = "n0", Speaker = speaker == "-" ? null : speaker, Text = KeyForNode(id, "n0"), Expression = expr, Emote = emote, Sfx = sfx };
            var graph = new DialogueGraph { Id = id, Start = "n0" };
            graph.Nodes.Add(node);
            result.Dialogues.Add(graph);
            AddText(result, node.Text, Unescape(text));
        }

        // reaction <id> / on <trigger> / npcs a, b / if <condition> / ttl <days> / priority <n> / cooldown <days> / once / plays <dialogue>
        static void ParseReactionLine(ReactionDefinition r, string word, string rest, int lineNo, Action<int, string> err)
        {
            rest = rest.Trim();
            switch (word)
            {
                case "on": r.On = rest; break;
                case "npcs": r.Npcs = string.Join(",", rest.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)); break;
                case "if": r.Condition = rest; break;
                case "plays": r.Dialogue = rest; break;
                case "once": r.Once = true; break;
                case "ttl": if (int.TryParse(rest, out var t)) r.TtlDays = t; else err(lineNo, $"ttl '{rest}' is not a whole number"); break;
                case "priority": if (int.TryParse(rest, out var p)) r.Priority = p; else err(lineNo, $"priority '{rest}' is not a whole number"); break;
                case "cooldown": if (int.TryParse(rest, out var c)) r.CooldownDays = c; else err(lineNo, $"cooldown '{rest}' is not a whole number"); break;
                default: err(lineNo, $"unknown reaction line '{word}'"); break;
            }
        }

        // loves a, b / likes a, b / dislikes a, b
        static void ParseSocialLine(SocialProfile social, string word, string rest, int lineNo, Action<int, string> err)
        {
            var items = rest.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (word)
            {
                case "loves": social.Loves.AddRange(items); break;
                case "likes": social.Likes.AddRange(items); break;
                case "dislikes": social.Dislikes.AddRange(items); break;
                default: err(lineNo, $"unknown social line '{word}' (use loves, likes or dislikes)"); break;
            }
        }

        static void ParseSetEntry(DialogueSet set, string text, int lineNo, Action<int, string> err)
        {
            var arrow = text.IndexOf(" => ", StringComparison.Ordinal);
            if (arrow < 0) { err(lineNo, "a set entry looks like '<priority> [condition] => <dialogue> [options]'"); return; }
            var left = text.Substring(0, arrow).Trim();
            var right = text.Substring(arrow + 4).Trim();
            var firstWord = FirstWord(left, out var condition);
            if (!int.TryParse(firstWord, out var priority)) { err(lineNo, $"'{firstWord}' is not a priority number"); return; }
            condition = condition.Trim();
            var parts = right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) { err(lineNo, "a set entry needs a dialogue id"); return; }
            var entry = new DialogueSetEntry { Priority = priority, Dialogue = parts[0], Condition = condition.Length == 0 || condition == "~" ? null : condition };
            foreach (var option in parts.Skip(1))
            {
                var eq = option.IndexOf('=');
                if (eq <= 0) { err(lineNo, $"unknown option '{option}'"); continue; }
                var key = option.Substring(0, eq);
                var value = option.Substring(eq + 1);
                switch (key)
                {
                    case "rarity": entry.Rarity = value; break;
                    case "tag": entry.Tag = value; break;
                    case "cat": entry.Category = value; break;
                    case "weight":
                        if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var w)) entry.Weight = w;
                        else err(lineNo, $"weight '{value}' is not a number");
                        break;
                    case "cooldown":
                        if (int.TryParse(value, out var c)) entry.Cooldown = c; else err(lineNo, $"cooldown '{value}' is not a whole number");
                        break;
                    default: err(lineNo, $"unknown option '{key}'"); break;
                }
            }
            set.Entries.Add(entry);
        }

        // "text => next if condition | effect | effect"
        static DialogueChoice ParseChoice(string rest, int lineNo, Action<int, string> err)
        {
            var pieces = rest.Split(new[] { " | " }, StringSplitOptions.None);
            var head = pieces[0];
            var arrow = head.LastIndexOf(" => ", StringComparison.Ordinal);
            if (arrow < 0) { err(lineNo, "a choice looks like '? text => next | effect'"); return null; }
            var choice = new DialogueChoice { Text = head.Substring(0, arrow).Trim() };
            var target = head.Substring(arrow + 4).Trim();
            var ifAt = target.IndexOf(" if ", StringComparison.Ordinal);
            if (ifAt >= 0) { choice.Condition = target.Substring(ifAt + 4).Trim(); target = target.Substring(0, ifAt).Trim(); }
            choice.Next = target.Length == 0 || target == "-" ? null : target;
            for (var i = 1; i < pieces.Length; i++) if (pieces[i].Trim().Length > 0) choice.Effects.Add(pieces[i].Trim());
            return choice;
        }

        // ---- exporting (existing JSON content to FScript) --------------------------------------------------------

        // `textOf` resolves a key to its text (normally the English string table).
        public static string Export(IEnumerable<DialogueGraph> dialogues, IEnumerable<DialogueSet> sets, Func<string, string> textOf)
        {
            var sb = new StringBuilder();
            foreach (var d in dialogues)
            {
                sb.Append("dialogue ").Append(d.Id);
                if (d.Nodes.Count > 0 && d.Start != d.Nodes[0].Id) sb.Append(" start=").Append(d.Start);
                sb.Append('\n');
                var choiceIndex = 0;
                foreach (var n in d.Nodes)
                {
                    sb.Append(n.Id).Append(' ').Append(string.IsNullOrEmpty(n.Speaker) ? "-" : n.Speaker).Append(':');
                    if (!string.IsNullOrEmpty(n.Text)) sb.Append(' ').Append(Escape(textOf(n.Text) ?? "{{missing " + n.Text + "}}"));
                    sb.Append('\n');
                    if (!string.IsNullOrEmpty(n.Text) && n.Text != KeyForNode(d.Id, n.Id)) sb.Append("  key ").Append(n.Text).Append('\n');
                    if (!string.IsNullOrEmpty(n.Condition)) sb.Append("  if ").Append(n.Condition).Append('\n');
                    if (!string.IsNullOrEmpty(n.Expression)) sb.Append("  expr ").Append(n.Expression).Append('\n');
                    if (!string.IsNullOrEmpty(n.Emote)) sb.Append("  emote ").Append(n.Emote).Append('\n');
                    if (!string.IsNullOrEmpty(n.Sfx)) sb.Append("  sfx ").Append(n.Sfx).Append('\n');
                    if (!string.IsNullOrEmpty(n.Voice)) sb.Append("  voice ").Append(n.Voice).Append('\n');
                    if (!string.IsNullOrEmpty(n.Camera)) sb.Append("  camera ").Append(n.Camera).Append('\n');
                    if (!string.IsNullOrEmpty(n.Tag)) sb.Append("  tag ").Append(n.Tag).Append('\n');
                    foreach (var e in n.Effects) sb.Append("  + ").Append(e).Append('\n');
                    foreach (var c in n.Choices)
                    {
                        var choiceText = textOf(c.Text) ?? string.Empty;
                        if (choiceText.Contains(" => ") || choiceText.Contains(" | "))
                            throw new FormatException($"choice text in '{d.Id}' contains ' => ' or ' | ', which FScript cannot represent");
                        sb.Append("  ? ").Append(Escape(textOf(c.Text) ?? "{{missing " + c.Text + "}}")).Append(" => ").Append(string.IsNullOrEmpty(c.Next) ? "-" : c.Next);
                        if (!string.IsNullOrEmpty(c.Condition)) sb.Append(" if ").Append(c.Condition);
                        foreach (var e in c.Effects) sb.Append(" | ").Append(e);
                        sb.Append('\n');
                        if (c.Text != KeyForChoice(d.Id, choiceIndex)) sb.Append("  key ").Append(c.Text).Append('\n');
                        if (!string.IsNullOrEmpty(c.Tone)) sb.Append("  tone ").Append(c.Tone).Append('\n');
                        choiceIndex++;
                    }
                    if (!string.IsNullOrEmpty(n.Next)) sb.Append("  next ").Append(n.Next).Append('\n');
                }
                sb.Append('\n');
            }
            foreach (var s in sets)
            {
                sb.Append("set ").Append(s.Id).Append('\n');
                foreach (var e in s.Entries)
                {
                    sb.Append("  ").Append(e.Priority).Append(' ').Append(string.IsNullOrEmpty(e.Condition) ? "~" : e.Condition).Append(" => ").Append(e.Dialogue);
                    if (!string.IsNullOrEmpty(e.Rarity)) sb.Append(" rarity=").Append(e.Rarity);
                    if (e.Weight != 1f) sb.Append(" weight=").Append(e.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    if (e.Cooldown != -1) sb.Append(" cooldown=").Append(e.Cooldown);
                    if (!string.IsNullOrEmpty(e.Tag)) sb.Append(" tag=").Append(e.Tag);
                    if (!string.IsNullOrEmpty(e.Category)) sb.Append(" cat=").Append(e.Category);
                    sb.Append('\n');
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        // ---- JSON output -----------------------------------------------------------------------------------------

        // The story JSON for a compile result, in the same shape as the hand-written files.
        public static string ToStoryJson(FScriptResult result)
        {
            var root = new Dictionary<string, object>();
            if (result.Dialogues.Count > 0) root["dialogues"] = result.Dialogues;
            if (result.Sets.Count > 0) root["sets"] = result.Sets;
            if (result.Topics.Count > 0) root["topics"] = result.Topics;
            if (result.Socials.Count > 0) root["socials"] = result.Socials;
            if (result.Reactions.Count > 0) root["reactions"] = result.Reactions;
            if (result.SetAdditions.Count > 0) root["setEntries"] = result.SetAdditions.Select(a => new Dictionary<string, object> { ["set"] = a.Set, ["entries"] = a.Entries }).ToList();
            return JsonConvert.SerializeObject(root, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new StoryJsonResolver() });
        }

        sealed class StoryJsonResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var p = base.CreateProperty(member, memberSerialization);
                if (p.PropertyType == typeof(string))
                    p.ShouldSerialize = o => !string.IsNullOrEmpty((string)p.ValueProvider.GetValue(o));
                else if (typeof(System.Collections.ICollection).IsAssignableFrom(p.PropertyType))
                    p.ShouldSerialize = o => p.ValueProvider.GetValue(o) is System.Collections.ICollection c && c.Count > 0;
                else if (member.DeclaringType == typeof(DialogueSetEntry) && member.Name == nameof(DialogueSetEntry.Weight))
                    p.ShouldSerialize = o => ((DialogueSetEntry)o).Weight != 1f;
                else if (member.DeclaringType == typeof(DialogueSetEntry) && member.Name == nameof(DialogueSetEntry.Cooldown))
                    p.ShouldSerialize = o => ((DialogueSetEntry)o).Cooldown != -1;
                return p;
            }
        }

        // Adds or updates compiled texts in the English string table, keeping the existing order and layout.
        public static string MergeTexts(string tableJson, IDictionary<string, string> texts, out int added, out int changed)
        {
            var table = Newtonsoft.Json.Linq.JObject.Parse(tableJson);
            added = 0; changed = 0;
            foreach (var kv in texts)
            {
                var current = table[kv.Key];
                if (current == null) { table[kv.Key] = kv.Value; added++; }
                else if ((string)current != kv.Value) { table[kv.Key] = kv.Value; changed++; }
            }
            return table.ToString(Formatting.Indented) + "\n";
        }

        // ---- helpers ---------------------------------------------------------------------------------------------

        static string FirstWord(string text, out string rest)
        {
            var space = text.IndexOf(' ');
            if (space < 0) { rest = string.Empty; return text; }
            rest = text.Substring(space + 1);
            return text.Substring(0, space);
        }

        static string Shorten(string text) => text.Length <= 40 ? text : text.Substring(0, 40) + "...";

        public static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\n", "\\n");

        public static string Unescape(string text)
        {
            if (text.IndexOf('\\') < 0) return text;
            var sb = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    var next = text[++i];
                    sb.Append(next == 'n' ? '\n' : next);
                }
                else sb.Append(text[i]);
            }
            return sb.ToString();
        }
    }
}
