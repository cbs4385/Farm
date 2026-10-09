using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Data classes for story JSON (ADR 0003). All text fields are string-table keys; every Condition is a
    // condition expression and every effect an effect string (see Effects).

    [Serializable]
    public sealed class QuestObjective
    {
        public string Text;                 // key shown in the journal
        public string Condition;            // done while it holds, e.g. "has:crop.parsnip>=5" or "flag:x"
        public string TakeItem;             // handed over (removed) when the quest is turned in
        public int TakeCount;
    }

    [Serializable]
    public sealed class QuestDefinition
    {
        public string Id;
        public string TitleKey;
        public string DescriptionKey;
        public string Category = "story";   // story, tutorial, board
        public string Giver;                // npc id (display only)
        public string Available;            // condition: the quest can be started / is offered
        public bool AutoStart;              // starts by itself as soon as Available holds (tutorial chain)
        public bool AutoComplete;           // completes by itself when every objective holds
        public bool Repeatable;
        public List<QuestObjective> Objectives = new List<QuestObjective>();
        public List<string> OnStart = new List<string>();
        public List<string> Rewards = new List<string>();   // effects run on completion
    }

    [Serializable]
    public sealed class LetterDefinition
    {
        public string Id;
        public string Condition;            // the letter arrives in the mailbox once this holds (at dawn)
        public string Sender;               // npc id, or empty
        public string SubjectKey;
        public string BodyKey;
        public List<string> Effects = new List<string>();   // run when the letter is taken from the mailbox (gifts, quests)
    }

    [Serializable]
    public sealed class EventStep
    {
        public string Type;                 // see EventSteps.Known: say, dialogue, move, face, wait, advance, fadeout, fadein, effects, place,
                                            // emote, expression, anim, camera, sfx, music, lighting, branch, label, parallel, waitFor, spawn, despawn, letterbox
        public string Actor;                // "player" or an npc id
        public string Speaker;              // for say
        public string Text;                 // key, for say
        public string Dialogue;             // dialogue id, for dialogue
        public int X, Y;
        public string Facing;               // up, down, left, right
        public float Seconds;               // for wait, and the length of a fade
        public int Minutes;                 // for advance: game minutes the clock moves on
        public List<string> Effects = new List<string>();
        // Added by T-100 (all optional):
        public string Name;                 // emote, expression, animation, camera mode, sound, music cue, lighting preset, prop item, letterbox on/off
        public string Id;                   // a prop's id (spawn, despawn)
        public string Label;                // for `label`: a place a branch can jump to
        public string Target;               // for `branch`: the label to jump to
        public string Condition;            // a step runs only while this holds; a branch jumps when it holds (empty = always)
        public float Value;                 // camera shake strength
        public bool Async;                  // start and carry on; `waitFor` waits for it (move, emote, anim, camera, lighting)
        public string Expression, Emote;    // for `say`: the portrait expression and emote bubble of that line
        public List<EventStep> Steps = new List<EventStep>();   // for `parallel`: steps that run together
    }

    [Serializable]
    public sealed class EventDefinition
    {
        public string Id;
        public string Trigger = "map";      // map (on entering Map), dawn (picked in the morning), manual (effect "event:<id>")
        public string Map;
        public string Condition;
        public bool Once = true;
        public string TitleKey;             // a replayable memory's title (festivals use their calendar name instead)
        public string Tag;                  // moment tag (funny, wholesome, surprise, mystery): this scene is meant to be remembered and clipped
        public string Calendar;             // festivals: string key of the name shown on the calendar ...
        public int CalendarSeason = -1;     // ... on this season (0 spring .. 3 winter)
        public int CalendarDay;             // ... and day
        public bool RunClock;               // keep the clock running during the scene (timed scenes use "advance" steps)
        public bool Recheck;                // a "map" event that is also looked for again as the clock moves while its map is on screen (a visitor who comes at 8 in the morning)
        public int Priority;
        public List<EventStep> Steps = new List<EventStep>();
        public List<string> SkipEffects = new List<string>();   // run if the player skips the scene and these were not run
    }

    // A seasonal random event drawn at dawn from a weighted table (weights can be shifted by a hook and by luck).
    [Serializable]
    public sealed class RandomEventDefinition
    {
        public string Id;
        public float Weight = 10f;
        public string Seasons;              // "spring,summer"; empty = all
        public string Condition;
        public string TextKey;              // shown in the day summary
        public string Mood = "neutral";     // good, bad or neutral: luck shifts weight toward good (or bad) events
        public List<string> Effects = new List<string>();
    }

    // A template for a help-wanted board job: bring N of one of these items for a reward.
    [Serializable]
    public sealed class BoardJobTemplate
    {
        public string Id;
        public List<string> Items = new List<string>();
        public int MinCount = 1;
        public int MaxCount = 3;
        public int RewardPercent = 250;     // of the items' total sell price
        public string Condition;
    }
}
