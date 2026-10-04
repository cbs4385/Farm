using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The memories gallery (T-102): scenes the player has seen (heart events, festivals, the hall ending) can be replayed
    // from the game menu. A replay plays the scene on its own map with every effect switched off, so it changes nothing
    // in the save, then puts the player back where they were.
    public static class Memories
    {
        public const string FestivalGroup = "festival", OtherGroup = "other";
        public const float ReturnFadeSeconds = 0.4f;

        // An event is a memory when it has a title (or is a festival, whose calendar name is its title).
        public static bool IsMemory(EventDefinition ev) => ev != null && !string.IsNullOrEmpty(TitleKey(ev));

        public static string TitleKey(EventDefinition ev) => !string.IsNullOrEmpty(ev.TitleKey) ? ev.TitleKey : ev.Calendar;

        public static bool Unlocked(GameState state, EventDefinition ev) => state.EventsSeen.Contains(ev.Id);

        // "festival", a villager id (heart events are named <villager>_heart<N>), or "other".
        public static string GroupOf(EventDefinition ev)
        {
            if (!string.IsNullOrEmpty(ev.Calendar)) return FestivalGroup;
            var cut = ev.Id.IndexOf("_heart", System.StringComparison.Ordinal);
            return cut > 0 ? ev.Id.Substring(0, cut) : OtherGroup;
        }

        public static List<EventDefinition> All(StoryContent story) =>
            story.Events.Where(IsMemory).OrderBy(e => e.Id, System.StringComparer.Ordinal).ToList();

        public static int UnlockedCount(StoryContent story, GameState state) => All(story).Count(e => Unlocked(state, e));

        // The NPCs a replay must have on stage: everyone who speaks or acts, except the player.
        public static List<string> Cast(EventDefinition ev)
        {
            var ids = new List<string>();
            void Add(string id) { if (!string.IsNullOrEmpty(id) && id != "player" && !ids.Contains(id)) ids.Add(id); }
            void Walk(IEnumerable<EventStep> steps)
            {
                foreach (var s in steps) { Add(s.Speaker); Add(s.Actor); Walk(s.Steps); }
            }
            Walk(ev.Steps);
            return ids;
        }

        // Where a villager who is not on the map is placed for a replay: beside the cell the scene moves the player to.
        public static Vector3Int CastCell(EventDefinition ev, int index)
        {
            var move = ev.Steps.FirstOrDefault(s => s.Type == "move" && s.Actor == "player");
            return move != null ? new Vector3Int(move.X + index, move.Y + 1, 0) : new Vector3Int(8 + index, 8, 0);
        }

        // ---- session wiring ----

        public static bool CanStart(GameSession s, string eventId)
        {
            var ev = s.Story.Event(eventId);
            return ev != null && IsMemory(ev) && Unlocked(s.State, ev) && s.MemoryId == null && !s.IsSleeping && s.InGame;
        }

        public static bool Start(GameSession s, string eventId)
        {
            if (!CanStart(s, eventId) || !ServiceLocator.TryGet<SceneLoader>(out var loader) || loader.IsLoading) return false;
            var ev = s.Story.Event(eventId);
            var player = Object.FindAnyObjectByType<PlayerController>();
            s.MemoryId = eventId;
            s.MemoryReturnMap = s.State.CurrentMap;
            s.MemoryReturnPosition = player != null ? player.transform.position : Vector3.zero;
            s.MemoryHasReturnPosition = player != null;
            var map = string.IsNullOrEmpty(ev.Map) ? s.State.CurrentMap : ev.Map;
            s.State.CurrentMap = map;
            s.State.SpawnPoint = "default";
            loader.Load(map, ReturnFadeSeconds);
            return true;
        }

        // The replay is over: back to the map the player came from, standing where they were.
        public static void Finish(GameSession s)
        {
            if (s.MemoryId == null) return;
            var back = s.MemoryReturnMap;
            s.MemoryId = null;
            s.State.CurrentMap = back;
            s.State.SpawnPoint = "default";
            s.MemoryRestorePosition = s.MemoryHasReturnPosition;
            if (ServiceLocator.TryGet<SceneLoader>(out var loader)) loader.Load(back, ReturnFadeSeconds);
        }
    }
}
