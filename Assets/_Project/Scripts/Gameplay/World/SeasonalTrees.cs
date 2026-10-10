using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The trees of an outdoor map change with the season: round green ones and willows in spring and summer, orange and yellow ones in fall, bare ones and conifers in
    // winter, each tree its own kind (the same kind every time for the same map, seed and place). The trees are set into the map's scene; this swaps their pictures
    // when the map starts, from the pictures in Resources/Trees. A share of the trees keeps the original pine in every season. Playtest 2026-10-09: "the world looks flat".
    public sealed class SeasonalTrees : MonoBehaviour
    {
        public const string TreeSprite = "obj_tree";
        public const int PineSlots = 2;                      // of every (kinds + this many), this many keep the original pine

        static Dictionary<string, Sprite[]> _sprites;

        public static void ResetForTests() => _sprites = null;

        public static string SetFor(Season season) => season == Season.Fall ? "fall" : season == Season.Winter ? "winter" : "spring";

        static Sprite[] Kinds(string set)
        {
            _sprites ??= Resources.LoadAll<Sprite>("Trees")
                .GroupBy(s => Regex.Replace(s.name, @"_\d+$", string.Empty))
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.name, System.StringComparer.Ordinal).ToArray());
            return _sprites.TryGetValue("tree_" + set, out var list) ? list : new Sprite[0];
        }

        // Which kind of tree stands in a place: an index into the season's kinds, or -1 for the original pine.
        public static int KindAt(int seed, string mapId, int x, int y, int kinds)
        {
            if (kinds <= 0) return -1;
            var pick = DecorPlanner.Slot(seed ^ 0x51ED270B, mapId, x, y, kinds + PineSlots);
            return pick < kinds ? pick : -1;
        }

        // Swaps the picture of every tree standing in the scene; returns how many were changed.
        public int Apply(string mapId, int seed, Season season)
        {
            var kinds = Kinds(SetFor(season));
            if (kinds.Length == 0) return 0;
            var changed = 0;
            foreach (var renderer in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer.sprite == null || renderer.sprite.name != TreeSprite) continue;
                var sway = renderer.GetComponentInParent<ObjectSway>();
                var at = (sway != null ? sway.transform : renderer.transform).position;       // the swaying copy shares its tree's place
                var kind = KindAt(seed, mapId, Mathf.FloorToInt(at.x), Mathf.FloorToInt(at.y), kinds.Length);
                if (kind < 0) continue;
                renderer.sprite = kinds[kind];
                changed++;
            }
            return changed;
        }
    }
}
