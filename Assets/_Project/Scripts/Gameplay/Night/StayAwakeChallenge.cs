using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    public enum QtePrompt { Up, Down, Left, Right, Interact, UseTool }

    // The quick-time event itself (owner design, GDD section 9, V): the player follows a series of prompts. With each
    // repetition in the same day the difficulty rises by shortening the time allowed to respond.
    // Every number below is an initial tuning value, not a decision; tune it in T-046 and the balance pass (T-066).
    public static class StayAwakeChallenge
    {
        public const int PromptsPerChallenge = 3;
        public const float StartWindowSeconds = 2.5f;      // allowed time per prompt on the first check of a day
        public const float WindowDecayPerCheck = 0.06f;    // each later check in the day shortens it by 6%
        public const float MinWindowSeconds = 0.4f;        // never shorter than this

        // checkIndex is 0 for the first check of the day, 1 for the second, and so on.
        public static float WindowSeconds(int checkIndex)
        {
            var index = Math.Max(0, checkIndex);
            var window = StartWindowSeconds * (float)Math.Pow(1.0 - WindowDecayPerCheck, index);
            return Math.Max(MinWindowSeconds, window);
        }

        // The prompts are random but fully determined by the seed (so tests and replays are repeatable).
        public static List<QtePrompt> GeneratePrompts(int seed, int checkIndex, int count = PromptsPerChallenge)
        {
            var rng = new Random(unchecked(seed * 31 + checkIndex * 7919 + 13));
            var kinds = (QtePrompt[])Enum.GetValues(typeof(QtePrompt));
            var prompts = new List<QtePrompt>(count);
            for (var i = 0; i < count; i++) prompts.Add(kinds[rng.Next(kinds.Length)]);
            return prompts;
        }

        public readonly struct Response
        {
            public readonly QtePrompt Pressed;
            public readonly float Seconds;       // time from the prompt appearing to the press
            public Response(QtePrompt pressed, float seconds) { Pressed = pressed; Seconds = seconds; }
        }

        public enum Outcome { Passed, WrongPrompt, TooSlow, Incomplete }

        // The player passes if every prompt was answered, correctly and within the window. A missing response is a
        // timeout. The first mistake decides the outcome.
        public static Outcome Evaluate(IReadOnlyList<QtePrompt> prompts, IReadOnlyList<Response> responses, float windowSeconds)
        {
            for (var i = 0; i < prompts.Count; i++)
            {
                if (i >= responses.Count) return Outcome.Incomplete;
                if (responses[i].Pressed != prompts[i]) return Outcome.WrongPrompt;
                if (responses[i].Seconds > windowSeconds) return Outcome.TooSlow;
            }
            return Outcome.Passed;
        }
    }
}
