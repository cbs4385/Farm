using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The help-wanted board in the village (T-039): up to three jobs are posted at a time ("bring N of an item"), each
    // lasts a few days and pays more than the shipping bin would. Jobs come from the templates in the story data, drawn
    // deterministically from the world seed and the day, and gated by the template's condition (so a layer can add its own).
    public static class HelpWanted
    {
        public const int MaxJobs = 3;
        public const int DurationDays = 4;

        // Drops expired jobs and posts new ones. Deterministic for a given seed and day.
        public static void Refresh(GameState state, StoryContent story, GameDatabase db, IWorldQuery world, int today)
        {
            state.Board.RemoveAll(j => j.ExpiresDay < today);
            if (story == null) return;
            var templates = story.BoardJobs.OrderBy(t => t.Id, StringComparer.Ordinal)
                .Where(t => Conditions.TryEvaluate(t.Condition, world, out var ok) && ok).ToList();
            if (templates.Count == 0) return;

            // At most one new job a morning, and not every morning.
            if (state.Board.Count >= MaxJobs) return;
            if (WeatherRoller.Unit(today * 17 + 3, state.WorldSeed) > 0.7f) return;

            var template = templates[(int)(WeatherRoller.Unit(today * 31 + 5, state.WorldSeed) * templates.Count) % templates.Count];
            var items = template.Items.Where(id => db.TryGetItem(id, out _)).ToList();
            if (items.Count == 0) return;
            var itemId = items[(int)(WeatherRoller.Unit(today * 13 + 7, state.WorldSeed) * items.Count) % items.Count];
            if (state.Board.Any(j => j.ItemId == itemId)) return;                       // not two of the same

            db.TryGetItem(itemId, out var item);
            var count = template.MinCount + (int)(WeatherRoller.Unit(today * 7 + 11, state.WorldSeed) * (template.MaxCount - template.MinCount + 1));
            count = Math.Min(count, template.MaxCount);
            var reward = (int)Math.Round(item.SellPrice * count * template.RewardPercent / 100f / 10f) * 10;
            state.Board.Add(new BoardJob
            {
                Id = $"job.{today}.{itemId}", ItemId = itemId, Count = count, Reward = Math.Max(10, reward),
                PostedDay = today, ExpiresDay = today + DurationDays,
            });
        }

        public static bool CanDeliver(GameSession s, BoardJob job) => s.Backpack.Has(job.ItemId, job.Count);

        public static void Accept(GameSession s, BoardJob job)
        {
            job.Accepted = true;
            s.Toast(L.Get("board.accepted"));
        }

        // Hands the items over and takes the reward.
        public static bool Deliver(GameSession s, BoardJob job)
        {
            if (!s.State.Board.Contains(job) || !CanDeliver(s, job)) return false;
            s.Backpack.Remove(job.ItemId, job.Count);
            s.State.Board.Remove(job);
            s.AddGold(job.Reward);
            s.AddVar("stat.jobs_done", 1);
            s.Toast(L.Get("board.delivered", job.Reward));
            return true;
        }
    }
}
