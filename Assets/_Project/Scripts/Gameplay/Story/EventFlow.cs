using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    // The kinds of step an event may contain and the words they accept (T-100). The director, the validator and the
    // authoring docs share these lists.
    public static class EventSteps
    {
        public static readonly string[] Anims = { "hop", "jiggle", "nod", "sway", "look", "dance" };
        public static readonly string[] CameraModes = { "focus", "pan", "shake", "reset" };
        public static readonly string[] LightingPresets = { "day", "dawn", "dusk", "night", "warm", "dim", "reset" };
        public static readonly string[] Letterbox = { "on", "off" };

        public static readonly HashSet<string> Known = new HashSet<string>
        {
            "say", "dialogue", "move", "face", "wait", "advance", "fadeout", "fadein", "effects", "place",
            "emote", "expression", "anim", "camera", "sfx", "music", "lighting", "branch", "label", "parallel", "waitFor",
            "spawn", "despawn", "letterbox",
        };

        // Steps that may run in the background (`async`) or as a child of `parallel`.
        public static readonly HashSet<string> Background = new HashSet<string> { "move", "emote", "anim", "camera", "lighting" };
        public static readonly HashSet<string> ParallelChildren = new HashSet<string>
        {
            "move", "face", "place", "wait", "emote", "anim", "camera", "sfx", "lighting", "spawn", "despawn", "expression",
        };

        // Everything wrong with an event's steps (each problem prefixed with its step path). `npcExists`, `itemExists` and
        // `dialogueExists` let the validator pass in the game's data.
        public static List<string> Problems(EventDefinition ev, Func<string, bool> npcExists, Func<string, bool> itemExists, Func<string, bool> dialogueExists)
        {
            var problems = new List<string>();
            var labels = new Dictionary<string, int>();
            for (var i = 0; i < ev.Steps.Count; i++)
                if (ev.Steps[i].Type == "label")
                {
                    var name = ev.Steps[i].Label;
                    if (string.IsNullOrEmpty(name)) problems.Add($"step{i}: a label needs a name");
                    else if (labels.ContainsKey(name)) problems.Add($"step{i}: label '{name}' is defined twice");
                    else labels[name] = i;
                }

            var spawned = new HashSet<string>();
            for (var i = 0; i < ev.Steps.Count; i++)
            {
                var step = ev.Steps[i];
                CheckStep(step, $"step{i}", false, problems, npcExists, itemExists, dialogueExists, spawned);
                if (step.Type == "branch")
                {
                    if (string.IsNullOrEmpty(step.Target)) problems.Add($"step{i}: a branch needs a target label");
                    else if (!labels.TryGetValue(step.Target, out var at)) problems.Add($"step{i}: unknown label '{step.Target}'");
                    else if (at <= i && string.IsNullOrEmpty(step.Condition)) problems.Add($"step{i}: a branch back to '{step.Target}' needs a condition, or the scene would never end");
                }
            }
            return problems;
        }

        static void CheckStep(EventStep step, string at, bool child, List<string> problems, Func<string, bool> npcExists,
            Func<string, bool> itemExists, Func<string, bool> dialogueExists, HashSet<string> spawned)
        {
            if (!Known.Contains(step.Type ?? string.Empty)) { problems.Add($"{at}: unknown step type '{step.Type}'"); return; }
            if (child && !ParallelChildren.Contains(step.Type)) problems.Add($"{at}: '{step.Type}' cannot run inside a parallel group");
            if (step.Async && !Background.Contains(step.Type)) problems.Add($"{at}: '{step.Type}' cannot be async");
            if (step.Async && child) problems.Add($"{at}: a step inside a parallel group is already in the background");
            if (!string.IsNullOrEmpty(step.Actor) && step.Actor != "player" && !npcExists(step.Actor)) problems.Add($"{at}: unknown actor '{step.Actor}'");

            bool NeedActor() { if (string.IsNullOrEmpty(step.Actor)) { problems.Add($"{at}: '{step.Type}' needs an actor"); return false; } return true; }
            void NeedWord(string[] list, string what)
            {
                if (string.IsNullOrEmpty(step.Name) || Array.IndexOf(list, step.Name) < 0)
                    problems.Add($"{at}: unknown {what} '{step.Name}' (use {string.Join(", ", list)})");
            }

            switch (step.Type)
            {
                case "say":
                    if (!DialogueVocabulary.Has(DialogueVocabulary.Expressions, step.Expression)) problems.Add($"{at}: unknown expression '{step.Expression}'");
                    if (!DialogueVocabulary.Has(DialogueVocabulary.Emotes, step.Emote)) problems.Add($"{at}: unknown emote '{step.Emote}'");
                    break;
                case "emote": NeedActor(); NeedWord(DialogueVocabulary.Emotes, "emote"); break;
                case "expression": NeedActor(); NeedWord(DialogueVocabulary.Expressions, "expression"); break;
                case "anim": NeedActor(); NeedWord(Anims, "animation"); break;
                case "camera":
                    NeedWord(CameraModes, "camera mode");
                    break;
                case "sfx":
                    if (string.IsNullOrEmpty(step.Name) || !Enum.TryParse<Sfx>(step.Name, true, out _)) problems.Add($"{at}: unknown sound '{step.Name}'");
                    break;
                case "music":
                    if (string.IsNullOrEmpty(step.Name)) problems.Add($"{at}: music needs a cue name (or 'stop')");
                    break;
                case "lighting": NeedWord(LightingPresets, "lighting preset"); break;
                case "letterbox": NeedWord(Letterbox, "letterbox setting"); break;
                case "spawn":
                    if (string.IsNullOrEmpty(step.Id)) problems.Add($"{at}: a prop needs an id");
                    else spawned.Add(step.Id);
                    if (string.IsNullOrEmpty(step.Name) || !itemExists(step.Name)) problems.Add($"{at}: the prop '{step.Name}' is not an item");
                    break;
                case "despawn":
                    if (string.IsNullOrEmpty(step.Id)) problems.Add($"{at}: needs the id of the prop");
                    else if (!spawned.Contains(step.Id)) problems.Add($"{at}: no prop '{step.Id}' was spawned before this");
                    break;
                case "dialogue":
                    if (!dialogueExists(step.Dialogue ?? string.Empty)) problems.Add($"{at}: unknown dialogue '{step.Dialogue}'");
                    break;
                case "parallel":
                    if (child) problems.Add($"{at}: parallel groups cannot nest");
                    if (step.Steps.Count == 0) problems.Add($"{at}: a parallel group needs steps");
                    for (var c = 0; c < step.Steps.Count; c++)
                        CheckStep(step.Steps[c], $"{at}.{c}", true, problems, npcExists, itemExists, dialogueExists, spawned);
                    break;
                case "move": case "face": case "place": NeedActor(); break;
            }
        }
    }

    // The control flow of a scene, as pure functions (T-100): optional step conditions, labels and branches. The director
    // asks these what to do next; they are tested without a scene.
    public static class EventFlow
    {
        public const int MaxSteps = 2000;       // a safety stop for a scene that loops

        // A step with a condition only runs while it holds (a branch uses its condition to decide whether to jump instead).
        public static bool ShouldRun(EventStep step, IWorldQuery world)
        {
            if (step.Type == "branch" || string.IsNullOrWhiteSpace(step.Condition)) return true;
            return Conditions.TryEvaluate(step.Condition, world, out var ok) && ok;
        }

        public static int LabelIndex(IList<EventStep> steps, string label)
        {
            for (var i = 0; i < steps.Count; i++)
                if (steps[i].Type == "label" && steps[i].Label == label) return i;
            return -1;
        }

        // The index to run after `index`: the next step, or the label a branch jumps to when its condition holds.
        public static int Next(IList<EventStep> steps, int index, IWorldQuery world)
        {
            var step = steps[index];
            if (step.Type == "branch")
            {
                var jump = string.IsNullOrWhiteSpace(step.Condition) || (Conditions.TryEvaluate(step.Condition, world, out var ok) && ok);
                if (jump)
                {
                    var target = LabelIndex(steps, step.Target);
                    if (target >= 0) return target;
                }
            }
            return index + 1;
        }

        // The steps that will run from `fromIndex` to the end, following branches and conditions (used to find what a
        // skipped scene still has to do).
        public static List<int> PathFrom(IList<EventStep> steps, int fromIndex, IWorldQuery world)
        {
            var path = new List<int>();
            var i = fromIndex;
            for (var guard = 0; i >= 0 && i < steps.Count && guard < MaxSteps; guard++)
            {
                if (ShouldRun(steps[i], world)) path.Add(i);
                i = ShouldRun(steps[i], world) ? Next(steps, i, world) : i + 1;
            }
            return path;
        }
    }
}
