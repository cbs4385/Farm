using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    public readonly struct EventFinished
    {
        public readonly string EventId; public readonly bool Skipped;
        public EventFinished(string eventId, bool skipped) { EventId = eventId; Skipped = skipped; }
    }

    // Which events play, and when (T-041). The scripted playback lives in EventDirector (a scene object); this is the pure
    // part: queueing events (`event:<id>` effect) and finding the ones whose trigger and condition hold on a map.
    public static class EventRunner
    {
        // `event:<id>` from a dialogue, quest or hook: plays as soon as the player is on a map with nothing else open.
        public static bool Trigger(GameSession s, string eventId)
        {
            if (s.Story.Event(eventId) == null) { Log.Warn($"Unknown event '{eventId}'."); return false; }
            if (!s.PendingEvents.Contains(eventId)) s.PendingEvents.Add(eventId);
            return true;
        }

        public static bool Seen(GameState state, string eventId) => state.EventsSeen.Contains(eventId);

        // Events that start on their own when the player is on `mapId`: highest priority first, then by id. `firstOfDay`
        // also allows the "dawn" events (the first scene loaded after waking up).
        public static List<EventDefinition> FindTriggered(GameSession s, string mapId, bool firstOfDay)
        {
            return s.Story.Events
                .Where(e => (e.Trigger == "map" || (e.Trigger == "dawn" && firstOfDay))
                            && (string.IsNullOrEmpty(e.Map) || e.Map == mapId)
                            && (!e.Once || !Seen(s.State, e.Id))
                            && Conditions.TryEvaluate(e.Condition, s.World, out var ok) && ok)
                .OrderByDescending(e => e.Priority).ThenBy(e => e.Id, System.StringComparer.Ordinal)
                .ToList();
        }

        // Marks an event played and runs what a skipped scene still has to do (the effects steps that remain).
        public static void RunSkipped(GameSession s, EventDefinition ev, int fromStep)
        {
            // Follow the scene's own path (conditions and branches) so a skipped scene runs the effects of the branch it was on.
            foreach (var i in EventFlow.PathFrom(ev.Steps, fromStep, s.World))
                if (ev.Steps[i].Type == "effects") Effects.RunAll(s, ev.Steps[i].Effects);
            Effects.RunAll(s, ev.SkipEffects);
        }
    }
}
