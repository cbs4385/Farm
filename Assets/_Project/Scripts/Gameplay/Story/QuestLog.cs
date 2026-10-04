using System.Collections.Generic;
using System.Linq;
using Farm.Core;

namespace Farm.Gameplay
{
    public readonly struct QuestStarted
    {
        public readonly string QuestId;
        public QuestStarted(string questId) { QuestId = questId; }
    }

    public readonly struct QuestCompleted
    {
        public readonly string QuestId;
        public QuestCompleted(string questId) { QuestId = questId; }
    }

    // Quests (T-039): data in Resources/Story, progress in GameState.Quests, objectives are conditions over the same flags,
    // variables and inventory everything else uses. The tutorial chain is ordinary quests that start themselves.
    public static class QuestLog
    {
        // Counters the tutorial (and any later quest) can ask about: `var:stat.planted>=3`.
        public static class Stats
        {
            public const string Tilled = "stat.tilled";
            public const string Planted = "stat.planted";
            public const string Watered = "stat.watered";
            public const string Harvested = "stat.harvested";
            public const string Shipped = "stat.shipped";
            public const string Foraged = "stat.foraged";
            public const string Crafted = "stat.crafted";
            public const string Gifts = "stat.gifts";
        }

        public static string StatusOf(GameState state, string questId) =>
            state.Quests.TryGetValue(questId, out var q) ? q.Status : "new";

        public static bool IsActive(GameState state, string questId) => StatusOf(state, questId) == QuestStatus.Active;

        public static IEnumerable<QuestDefinition> Active(GameSession s) =>
            s.Story.Quests.Where(q => IsActive(s.State, q.Id)).OrderBy(q => q.Category == "tutorial" ? 1 : 0).ThenBy(q => q.Id, System.StringComparer.Ordinal);

        public static IEnumerable<QuestDefinition> Done(GameSession s) =>
            s.Story.Quests.Where(q => StatusOf(s.State, q.Id) == QuestStatus.Done).OrderBy(q => q.Id, System.StringComparer.Ordinal);

        // Is this one objective satisfied right now? (Handing over an item also needs the items to be in the backpack.)
        public static bool ObjectiveMet(GameSession s, QuestObjective o) =>
            Conditions.TryEvaluate(o.Condition, s.World, out var ok) && ok
            && (string.IsNullOrEmpty(o.TakeItem) || s.Backpack.Has(o.TakeItem, o.TakeCount));

        public static bool ObjectivesMet(GameSession s, QuestDefinition def) => def.Objectives.All(o => ObjectiveMet(s, o));

        // Begins a quest. Returns false when it is already active or done (unless it can be repeated).
        public static bool Start(GameSession s, QuestDefinition def)
        {
            var status = StatusOf(s.State, def.Id);
            if (status == QuestStatus.Active) return false;
            if (status != "new" && !def.Repeatable) return false;
            s.State.Quests[def.Id] = new QuestProgress { Status = QuestStatus.Active, StartedDay = s.Clock.Now.TotalDays };
            s.Publish(new QuestStarted(def.Id));
            s.Toast(L.Get("quest.started", L.Get(def.TitleKey)));
            Effects.RunAll(s, def.OnStart);
            return true;
        }

        // Turns the quest in: hands over the items, pays the rewards. Returns false when an objective is not met yet.
        public static bool TryComplete(GameSession s, QuestDefinition def)
        {
            if (!IsActive(s.State, def.Id)) return false;
            if (!ObjectivesMet(s, def)) return false;
            foreach (var o in def.Objectives)
                if (!string.IsNullOrEmpty(o.TakeItem)) s.Backpack.Remove(o.TakeItem, o.TakeCount);

            var progress = s.State.Quests[def.Id];
            progress.Status = QuestStatus.Done;
            progress.EndedDay = s.Clock.Now.TotalDays;
            if (def.Repeatable) s.State.Quests.Remove(def.Id);
            s.Publish(new QuestCompleted(def.Id));
            s.Toast(L.Get("quest.completed", L.Get(def.TitleKey)));
            AudioService.PlayIfAvailable(Sfx.QuestDone);
            Effects.RunAll(s, def.Rewards);
            return true;
        }

        static bool _ticking;

        // Starts quests that start themselves, completes the ones that complete themselves, and lets expired jobs lapse.
        // Cheap enough to run whenever something changes; guarded against re-entry (rewards change state).
        public static void Tick(GameSession s)
        {
            if (_ticking || !s.InGame || s.Story == null) return;
            _ticking = true;
            try
            {
                var again = true;
                for (var pass = 0; again && pass < 6; pass++)    // a completed quest may start the next one
                {
                    again = false;
                    foreach (var def in s.Story.Quests.OrderBy(q => q.Id, System.StringComparer.Ordinal).ToList())
                    {
                        var status = StatusOf(s.State, def.Id);
                        if (status == "new" || (def.Repeatable && status == QuestStatus.Done))
                        {
                            if (def.AutoStart && Conditions.TryEvaluate(def.Available, s.World, out var open) && open && Start(s, def)) again = true;
                        }
                        else if (status == QuestStatus.Active && def.AutoComplete && ObjectivesMet(s, def) && TryComplete(s, def)) again = true;
                    }
                }
            }
            finally { _ticking = false; }
        }

        // Quests with an expiry (offered jobs) fail once it has passed.
        public static void Expire(GameState state, int today)
        {
            foreach (var q in state.Quests.Values)
                if (q.Status == QuestStatus.Active && q.ExpiresDay >= 0 && q.ExpiresDay < today)
                {
                    q.Status = QuestStatus.Failed;
                    q.EndedDay = today;
                }
        }
    }
}
