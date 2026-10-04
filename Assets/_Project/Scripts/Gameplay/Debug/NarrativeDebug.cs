#if UNITY_EDITOR || DEVELOPMENT_BUILD
// Narrative developer tools (T-137). Compiled only in the Editor and in development builds, like DebugCommandProcessor;
// the release guard fails a release build that contains NarrativeDebug.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Farm.Core;
using Newtonsoft.Json;

namespace Farm.Gameplay
{
    // Commands for writers and testers: set hearts, mood, flags and storylines; see which lines a villager has said, why each
    // line is or is not in today's pool and what would be picked; play any dialogue or scene; inspect reactions, topics,
    // social actions and coverage. Registered by DebugCommandProcessor.
    public static class NarrativeDebug
    {
        public static IEnumerable<(string name, string usage, string help, Func<string[], DebugCommandResult> run)> Commands(GameSession s)
        {
            yield return ("hearts", "hearts <npc> <0-10>", "set a villager's friendship in hearts", a => Hearts(s, a));
            yield return ("mood", "mood [npc] [state|clear]", "show or force a villager's mood (content, tired, worried, delighted, lonely, mischievous)", a => MoodCommand(s, a));
            yield return ("storyline", "storyline <id> [on|off]", "turn a village storyline on or off", a => Toggle(s, "storyline.", "Storyline", a));
            yield return ("choice", "choice <flag> [on|off]", "set a remembered choice (flag choice.<flag>)", a => Toggle(s, "choice.", "Choice", a));
            yield return ("heard", "heard <npc>", "the talk lines a villager has said, newest first", a => Heard(s, a));
            yield return ("pool", "pool <npc>", "every line in a villager's talk set: eligible or why not, and what would be said", a => Pool(s, a));
            yield return ("pick", "pick <npc> [seed]", "dry-run what the villager would say now (nothing is recorded)", a => Pick(s, a));
            yield return ("say", "say <dialogueId>", "play any dialogue", a => Say(s, a));
            yield return ("bark", "bark <npc>", "make a villager say an ambient bark as soon as they are on this map", a => Bark(s, a));
            yield return ("scene", "scene <eventId>", "play any scene (event) as soon as the map is idle", a => Scene(s, a));
            yield return ("memory", "memory <eventId>", "unlock and replay a scene as a memory", a => Memory(s, a));
            yield return ("reactions", "reactions", "the villagers' pending reactions", a => ReactionList(s));
            yield return ("fire", "fire <trigger>", "fire a reaction trigger, for example flag:first_harvest or quest.done:id", a => Fire(s, a));
            yield return ("topics", "topics <npc>", "a villager's topics: offered, or why not", a => TopicList(s, a));
            yield return ("social", "social <npc>", "a villager's social profile and what each action would give today", a => Social(s, a));
            yield return ("coverage", "coverage <npc>", "how many of a villager's talk lines have been heard, and which have not", a => Coverage(s, a));
        }

        static DebugCommandResult Fail(string message) => DebugCommandResult.Fail(message);
        static DebugCommandResult Ok(string message) => DebugCommandResult.Success(message);

        static bool TryNpc(GameSession s, string[] a, int index, out string id, out DebugCommandResult error)
        {
            id = null; error = null;
            if (a.Length <= index) { error = Fail("Name a villager, for example wren."); return false; }
            id = a[index].ToLowerInvariant();
            if (s.Npcs.Get(id) == null) { error = Fail($"Unknown villager '{id}'. Known: {string.Join(", ", s.Npcs.All.Select(n => n.Id))}"); return false; }
            return true;
        }

        static string SetId(string npc) => $"npc.{npc}.talk";

        // ---- state ----

        static DebugCommandResult Hearts(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            if (a.Length != 2 || !int.TryParse(a[1], out var hearts) || hearts < 0 || hearts > FriendshipModel.MaxHearts) return Fail("Usage: hearts <npc> <0-10>");
            var state = NpcInteractions.StateOf(s.State, npc);
            state.Points = hearts * FriendshipModel.PointsPerHeart;
            state.Met = state.Met || hearts > 0;
            return Ok($"{npc} now has {FriendshipModel.Hearts(state.Points)} heart(s) ({state.Points} points).");
        }

        static DebugCommandResult MoodCommand(GameSession s, string[] a)
        {
            if (a.Length == 0) return Ok(string.Join("\n", s.Npcs.All.Select(n => $"{n.Id}: {MoodModel.Of(s, n.Id).ToString().ToLowerInvariant()}")));
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var key = $"mood.{npc}.force";
            if (a.Length == 2)
            {
                if (a[1].Equals("clear", StringComparison.OrdinalIgnoreCase)) s.SetVar(key, 0);
                else if (MoodModel.TryParse(a[1], out var mood)) s.SetVar(key, (int)mood + 1);
                else return Fail($"Unknown mood '{a[1]}'. Use {string.Join(", ", Enum.GetNames(typeof(Mood)).Select(n => n.ToLowerInvariant()))} or clear.");
            }
            var forced = s.GetVar(key) > 0 ? " (forced)" : string.Empty;
            return Ok($"{npc} is {MoodModel.Of(s, npc).ToString().ToLowerInvariant()}{forced}.");
        }

        static DebugCommandResult Toggle(GameSession s, string prefix, string noun, string[] a)
        {
            if (a.Length < 1 || a.Length > 2) return Fail($"Usage: {noun.ToLowerInvariant()} <id> [on|off]");
            var on = true;
            if (a.Length == 2)
            {
                if (a[1].Equals("off", StringComparison.OrdinalIgnoreCase)) on = false;
                else if (!a[1].Equals("on", StringComparison.OrdinalIgnoreCase)) return Fail("Use on or off.");
            }
            s.SetFlag(prefix + a[0], on);
            return Ok($"{noun} {a[0]} is {(s.HasFlag(prefix + a[0]) ? "on" : "off")}.");
        }

        // ---- what has been said, what is in the pool ----

        static DebugCommandResult Heard(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var memory = LineMemory.Load(s);
            var today = s.Clock.Now.TotalDays;
            if (!memory.Heard.TryGetValue(SetId(npc), out var lines) || lines.Count == 0) return Ok($"{npc} has not said anything from the talk set yet.");
            var sb = new StringBuilder();
            foreach (var kv in lines.OrderByDescending(l => l.Value).Take(40))
                sb.AppendLine($"{kv.Key}  x{memory.TimesHeard(SetId(npc), kv.Key)}  last day {kv.Value} ({today - kv.Value} day(s) ago)");
            return Ok(sb.ToString().TrimEnd());
        }

        sealed class Row
        {
            public DialogueSetEntry Entry; public bool Eligible; public bool Fresh; public int CooldownLeft; public string Why; public bool Reaction;
        }

        static List<Row> Rows(GameSession s, string npc, LineMemory memory, int today)
        {
            var set = s.Story.Set(SetId(npc));
            var rows = new List<Row>();
            if (set == null) return rows;
            var reactions = Reactions.EntriesFor(ReactionState.Load(s), npc, today);
            foreach (var e in set.Entries.Concat(reactions))
            {
                var row = new Row { Entry = e, Reaction = reactions.Contains(e) };
                if (!Conditions.TryEvaluate(e.Condition, s.World, out var ok)) row.Why = $"the condition '{e.Condition}' is not valid";
                else if (!ok) row.Why = $"condition is false: {e.Condition}";
                row.Eligible = row.Why == null;
                var last = memory.LastHeardDay(set.Id, e.Dialogue);
                var cooldown = DialogueSet.CooldownOf(e);
                row.CooldownLeft = last >= 0 && cooldown > 0 && today - last < cooldown ? cooldown - (today - last) : 0;
                row.Fresh = row.CooldownLeft == 0;
                rows.Add(row);
            }
            return rows;
        }

        static DebugCommandResult Pool(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var set = s.Story.Set(SetId(npc));
            if (set == null) return Fail($"{npc} has no talk set.");
            var memory = LineMemory.Load(s);
            var today = s.Clock.Now.TotalDays;
            var rows = Rows(s, npc, memory, today);
            var sb = new StringBuilder();
            sb.AppendLine($"{npc}: {rows.Count(r => r.Eligible)} of {rows.Count} entries eligible today (day {today}).");
            foreach (var r in rows.OrderByDescending(r => r.Entry.Priority).ThenBy(r => r.Entry.Dialogue, StringComparer.Ordinal))
            {
                var tag = string.Join(" ", new[] { r.Entry.Rarity, r.Entry.Tag }.Where(x => !string.IsNullOrEmpty(x)));
                var status = !r.Eligible ? "no   " : !r.Fresh ? "wait " : "READY";
                var detail = !r.Eligible ? r.Why : !r.Fresh ? $"said recently, {r.CooldownLeft} day(s) until it can repeat" : memory.WasHeard(set.Id, r.Entry.Dialogue) ? "heard before" : "never heard";
                sb.AppendLine($"[{status}] p{r.Entry.Priority,-3} {r.Entry.Dialogue}{(r.Reaction ? " (reaction)" : "")}{(tag.Length > 0 ? " [" + tag + "]" : "")}  {detail}");
            }
            var pick = DryRun(s, npc, null, out var seed);
            sb.AppendLine(pick != null ? $"Would say: {pick} (seed {seed})" : "Would say: nothing (no eligible line).");
            return Ok(sb.ToString().TrimEnd());
        }

        // The same choice the villager would make now, on a copy of the memory so nothing is recorded.
        static string DryRun(GameSession s, string npc, int? seedOverride, out int seed)
        {
            var set = s.Story.Set(SetId(npc));
            var today = s.Clock.Now.TotalDays;
            seed = seedOverride ?? today * 7919 + NpcInteractions.StableHash(npc);
            if (set == null) return null;
            var copy = JsonConvert.DeserializeObject<LineMemory>(JsonConvert.SerializeObject(LineMemory.Load(s))) ?? new LineMemory();
            return set.PickVaried(s.World, seed, copy, set.Id, today, Reactions.EntriesFor(ReactionState.Load(s), npc, today));
        }

        static DebugCommandResult Pick(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            int? seed = null;
            if (a.Length > 1) { if (!int.TryParse(a[1], out var n)) return Fail("The seed must be a whole number."); seed = n; }
            var id = DryRun(s, npc, seed, out var used);
            if (id == null) return Ok($"{npc} would say nothing.");
            var graph = s.Story.Dialogue(id);
            var line = graph != null ? new DialogueRunner(graph, s.World, s.StoryText, _ => { }).Current : null;
            return Ok($"{npc} would say {id} (seed {used}):\n{(line != null ? line.Text : "(no text)")}");
        }

        // ---- playing things ----

        static DebugCommandResult Say(GameSession s, string[] a)
        {
            if (a.Length != 1) return Fail("Usage: say <dialogueId>");
            if (s.Story.Dialogue(a[0]) == null) return Fail($"Unknown dialogue '{a[0]}'.");
            return s.BeginDialogue(a[0]) ? Ok($"Playing {a[0]}.") : Fail("The dialogue could not be shown (no UI here).");
        }

        static DebugCommandResult Bark(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var id, out var error)) return error;
            if (s.Story.Set(Barks.SetId(id)) == null) return Fail($"{id} has no barks.");
            BarkDirector.Request(id);
            return Ok($"{id} will say a bark once on this map.");
        }

        static DebugCommandResult Scene(GameSession s, string[] a)
        {
            if (a.Length != 1) return Fail("Usage: scene <eventId>");
            return EventRunner.Trigger(s, a[0]) ? Ok($"Queued scene {a[0]}; it plays when the map is idle.") : Fail($"Unknown scene '{a[0]}'.");
        }

        static DebugCommandResult Memory(GameSession s, string[] a)
        {
            if (a.Length != 1) return Fail("Usage: memory <eventId>");
            var ev = s.Story.Event(a[0]);
            if (ev == null || !Memories.IsMemory(ev)) return Fail($"'{a[0]}' is not a replayable scene.");
            s.State.EventsSeen.Add(ev.Id);
            return Memories.Start(s, ev.Id) ? Ok($"Replaying {ev.Id}.") : Fail("Could not start the replay right now.");
        }

        // ---- reactions, topics, social ----

        static DebugCommandResult ReactionList(GameSession s)
        {
            var state = ReactionState.Load(s);
            var today = s.Clock.Now.TotalDays;
            if (state.Pending.Count == 0) return Ok("No reactions are waiting.");
            return Ok(string.Join("\n", state.Pending.Select(p =>
                $"{p.Npc}: {p.Dialogue} (reaction {p.Id}, p{p.Priority}, expires day {p.ExpiresDay}, {(p.ConsumedDay >= 0 ? "said day " + p.ConsumedDay : p.ExpiresDay < today ? "expired" : "waiting")})")));
        }

        static DebugCommandResult Fire(GameSession s, string[] a)
        {
            if (a.Length != 1) return Fail("Usage: fire <trigger>, for example fire flag:first_harvest");
            if (!Reactions.IsKnownTrigger(a[0])) return Fail($"'{a[0]}' is not a trigger (flag:, quest.start:, quest.done:, skill.up:, event:, season:, gift:, random:, day).");
            var before = ReactionState.Load(s).Pending.Count;
            Reactions.Fire(s, a[0]);
            var after = ReactionState.Load(s).Pending.Count;
            return Ok($"Fired {a[0]}: {after - before} reaction(s) queued.");
        }

        static DebugCommandResult TopicList(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var state = InteractionState.Load(s);
            var today = s.Clock.Now.TotalDays;
            var offered = Topics.Available(s.Story.Topics, s.World, state, npc, today).Select(t => t.Id).ToList();
            var all = s.Story.Topics.Where(t => t.Npc == npc).OrderByDescending(t => t.Priority).ToList();
            if (all.Count == 0) return Ok($"{npc} has no topics.");
            var sb = new StringBuilder();
            foreach (var t in all)
            {
                string why;
                if (offered.Contains(t.Id)) why = "OFFERED";
                else if (!Conditions.TryEvaluate(t.Condition, s.World, out var ok) || !ok) why = $"condition is false: {t.Condition}";
                else if (t.Once && state.TopicTimes.TryGetValue(t.Id, out var n) && n > 0) why = "asked already (once only)";
                else if (state.TopicLastAsked.TryGetValue(t.Id, out var last) && today - last < Math.Max(1, t.CooldownDays)) why = $"asked recently, {Math.Max(1, t.CooldownDays) - (today - last)} day(s) to wait";
                else why = "eligible, but only two are offered per visit";
                sb.AppendLine($"{t.Id} (p{t.Priority}): {why}");
            }
            return Ok(sb.ToString().TrimEnd());
        }

        static DebugCommandResult Social(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var profile = s.Story.Socials.FirstOrDefault(p => p.Npc == npc);
            var state = InteractionState.Load(s);
            var today = s.Clock.Now.TotalDays;
            var done = state.SocialToday(npc, today);
            var sb = new StringBuilder();
            sb.AppendLine(profile == null
                ? $"{npc} has no profile: every action is neutral."
                : $"{npc}: loves [{string.Join(", ", profile.Loves)}] likes [{string.Join(", ", profile.Likes)}] dislikes [{string.Join(", ", profile.Dislikes)}]");
            sb.AppendLine($"Actions today: {done} of {SocialActions.MaxPerDay}.");
            foreach (var action in SocialActions.All)
            {
                var outcome = SocialActions.Roll(profile, action, SocialActions.Seed(npc, action, today, done));
                var reaction = SocialActions.ReactionDialogue(s.Story, npc, action, outcome) ?? "(no reaction line)";
                sb.AppendLine($"{action}: {outcome} -> +{SocialActions.PointsFor(outcome, done)} points, plays {reaction}");
            }
            return Ok(sb.ToString().TrimEnd());
        }

        static DebugCommandResult Coverage(GameSession s, string[] a)
        {
            if (!TryNpc(s, a, 0, out var npc, out var error)) return error;
            var set = s.Story.Set(SetId(npc));
            if (set == null) return Fail($"{npc} has no talk set.");
            var memory = LineMemory.Load(s);
            var unheard = set.Entries.Select(e => e.Dialogue).Distinct().Where(d => !memory.WasHeard(set.Id, d)).ToList();
            var total = set.Entries.Select(e => e.Dialogue).Distinct().Count();
            var heard = total - unheard.Count;
            return Ok($"{npc}: {heard} of {total} talk lines heard ({(total == 0 ? 0 : heard * 100 / total)}%).\nNot yet: {(unheard.Count == 0 ? "none" : string.Join(", ", unheard))}");
        }
    }
}
#endif
