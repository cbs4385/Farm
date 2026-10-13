using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A shelf of the library (playtest 2026-10-13): looking at it lists some of the books on it. Which ones changes from day to day. An optional layer may change the
    // titles through GameHooks.FilterBookshelf.
    public sealed class Bookshelf : MonoBehaviour, IInteractable
    {
        public const int Listed = 6;               // titles on show
        public const int Pool = 18;                // the ordinary titles: bookshelf.title.0 to bookshelf.title.17

        [SerializeField] string _shelfId;
        public string ShelfId { get => _shelfId; set => _shelfId = value; }

        public int Reach => 1;
        public string HoverLabel => L.Get("hover.bookshelf");

        static int HashOf(string shelfId)
        {
            var hash = 17;
            foreach (var c in shelfId ?? string.Empty) hash = unchecked(hash * 31 + c);
            return hash;
        }

        // Which ordinary titles are on this shelf today (pure): `Listed` different numbers from the pool, the same for the same shelf and day.
        public static int[] PickIndices(string shelfId, int day)
        {
            var hash = HashOf(shelfId);
            return Enumerable.Range(0, Pool)
                .OrderBy(i => WeatherRoller.Unit(unchecked(day * 7919 + i * 104729 + hash), 0x51ED270B))
                .Take(Listed).ToArray();
        }

        public static IList<string> TitlesFor(GameSession s, string shelfId)
        {
            var day = s.Clock.Now.TotalDays;
            IList<string> titles = PickIndices(shelfId, day).Select(i => L.Get("bookshelf.title." + i)).ToList();
            return s.Hooks.FilterBookshelf(titles, unchecked(day * 31 + HashOf(shelfId)));
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var titles = TitlesFor(s, _shelfId);
            var text = L.Get("bookshelf.heading") + "\n\n" + string.Join("\n", titles.Select(t => "- " + t));
            if (s.Ui != null) s.Ui.ShowText(text);
            else s.Toast(text);
        }
    }
}
