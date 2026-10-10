using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The story side of the session: flags, variables, module data, story text and dialogues.
    public sealed partial class GameSession
    {
        // ---- story flags and variables -------------------------------------------------------------------------

        public bool HasFlag(string flag) => InGame && State.Flags.Contains(flag);

        public void SetFlag(string flag, bool on = true)
        {
            var changed = on ? State.Flags.Add(flag) : State.Flags.Remove(flag);
            if (changed) _bus.Publish(new FlagChanged(flag, on));
        }

        public int GetVar(string name) => InGame && State.Vars.TryGetValue(name, out var v) ? v : 0;

        public void SetVar(string name, int value)
        {
            var old = GetVar(name);
            if (old == value) return;
            State.Vars[name] = value;
            _bus.Publish(new VarChanged(name, old, value));
        }

        // Adds `delta` and clamps to [min, max]. Returns the new value.
        public int AddVar(string name, int delta, int min = int.MinValue, int max = int.MaxValue)
        {
            var value = Mathf.Clamp(GetVar(name) + delta, min, max);
            SetVar(name, value);
            return value;
        }

        // ---- module-owned persistent data ----------------------------------------------------------------------

        // Each module keeps its own serializable object in the save under its id. Returns a fresh T when none exists.
        public T GetModuleData<T>(string moduleId) where T : class, new()
        {
            if (InGame && State.ModuleData.TryGetValue(moduleId, out var json))
            {
                try { return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json) ?? new T(); }
                catch (System.Exception e) { Log.Error($"Module data '{moduleId}' unreadable: {e.Message}"); }
            }
            return new T();
        }

        public void SetModuleData<T>(string moduleId, T data) where T : class
        {
            if (!InGame) return;
            State.ModuleData[moduleId] = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        // Text from the string table with the player's and farm's names filled in ("[player]", "[farm]").
        public string StoryText(string key, object[] args)
        {
            var text = L.Get(key, args);
            if (!InGame) return text;
            return StoryTokens.Apply(text, token =>
            {
                switch (token)
                {
                    case "player": return State.PlayerName;
                    case "farm": return State.FarmName;
                    case "season": return Clock.Now.Season.ToString().ToLowerInvariant();
                    case "weekday": return StoryTokens.Weekdays[Clock.Now.DayOfWeek];
                }
                if (token.StartsWith("npc:", StringComparison.Ordinal) && Npcs?.Get(token.Substring(4)) is NpcDefinition npc)
                    return StoryTokens.ShortName(L.Get(npc.NameKey));
                return null;
            });
        }

        // Starts a conversation (modal; the clock is paused while it is open). Returns false when there is no such
        // dialogue or no UI to show it.
        public bool BeginDialogue(string dialogueId, Action onFinished = null)
        {
            var graph = Story.Dialogue(dialogueId);
            if (graph == null) { Log.Warn($"Unknown dialogue '{dialogueId}'."); return false; }
            if (!ServiceLocator.TryGet<IUiService>(out var ui)) return false;
            var runner = new DialogueRunner(graph, World, StoryText, effect => Effects.Run(this, effect));
            ui.ShowDialogue(runner, onFinished);
            return true;
        }

        // Starts a conversation from a graph built on the fly (the chat menu).
        public bool BeginDialogueGraph(DialogueGraph graph, Action onFinished = null)
        {
            if (graph == null || !ServiceLocator.TryGet<IUiService>(out var ui)) return false;
            ui.ShowDialogue(new DialogueRunner(graph, World, StoryText, effect => Effects.Run(this, effect)), onFinished);
            return true;
        }
    }
}
