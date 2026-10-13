using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;

namespace Farm.Mythos
{
    // The library shelves read stranger as the god wakes (playtest 2026-10-13): the more wakefulness, the more of the ordinary titles are swapped for disturbing ones, and
    // the worse the titles are. Mild shows only the first two grades and fewer of them; off changes nothing.
    public sealed class MythosBookshelf : IBookshelfFilter
    {
        public const int Grades = 4;               // mythos.book.g0 (odd) to g3 (dire), TitlesPerGrade of each
        public const int TitlesPerGrade = 8;
        readonly GameSession _session;

        public MythosBookshelf(GameSession session) { _session = session; }

        // How many of `listed` titles are swapped, and the worst grade used (pure). `step` is the wakefulness step 0..20; level 0 off, 1 mild, 2 full.
        public static (int swapped, int grade) Plan(int step, int level, int listed)
        {
            if (level <= 0 || step <= 0) return (0, 0);
            if (level == 1) return (Math.Min(Math.Max(1, listed / 3), step / 6), Math.Min(1, step / 10));
            return (Math.Min(listed, 1 + step / 4), Math.Min(Grades - 1, step / 5));
        }

        public IList<string> Filter(IList<string> titles, int seed)
        {
            var s = _session;
            if (s == null || !s.InGame || titles == null || titles.Count == 0) return titles;
            var step = WakefulnessModel.Step(s.GetVar(MythosIds.Vars.Wakefulness));
            var (swapped, grade) = Plan(step, MythosLevel.Of(s), titles.Count);
            if (swapped <= 0) return titles;

            var result = titles.ToList();
            var slots = Enumerable.Range(0, result.Count).OrderBy(i => WeatherRoller.Unit(unchecked(seed * 13 + i * 7919), 0x2F6E2B1)).Take(swapped).ToList();
            var used = new HashSet<string>();
            for (var n = 0; n < slots.Count; n++)
            {
                var useGrade = Math.Max(0, grade - (n % 2));          // not every strange title is the worst one
                var start = (int)(WeatherRoller.Unit(unchecked(seed * 31 + n * 104729), 0x6A09E667) * TitlesPerGrade) % TitlesPerGrade;
                for (var attempt = 0; attempt < TitlesPerGrade; attempt++)
                {
                    var pick = (start + attempt) % TitlesPerGrade;
                    var key = $"mythos.book.g{useGrade}.{pick}";
                    if (!used.Add(key)) continue;
                    result[slots[n]] = L.Get(key);
                    break;
                }
            }
            return result;
        }
    }
}
