using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The pictures of a villager in something other than their ordinary clothes (robes on a ritual night): Resources/Outfits/npc_<outfit>_<villager>_idle_<facing>.png.
    // Which outfit someone wears when is decided by outfit sources (GameHooks.AddOutfitSource); a villager with no picture for an outfit keeps their clothes.
    public static class NpcOutfits
    {
        static Dictionary<string, Sprite> _sprites;

        static NpcOutfits() => TestResets.Add(() => _sprites = null);

        static Dictionary<string, Sprite> Sprites =>
            _sprites ?? (_sprites = Resources.LoadAll<Sprite>("Outfits").GroupBy(s => s.name).ToDictionary(g => g.Key, g => g.First()));

        public static string FacingName(Vector2Int facing) => facing == Vector2Int.up ? "up" : facing == Vector2Int.left ? "left" : facing == Vector2Int.right ? "right" : "down";

        public static string SpriteName(string outfit, string npcId, Vector2Int facing) => $"npc_{outfit}_{npcId}_idle_{FacingName(facing)}";

        // The picture of `npcId` in `outfit` facing a way, or null when there is none.
        public static Sprite SpriteFor(string outfit, string npcId, Vector2Int facing) =>
            string.IsNullOrEmpty(outfit) ? null : Sprites.TryGetValue(SpriteName(outfit, npcId, facing), out var sprite) ? sprite : null;

        public static bool Has(string outfit, string npcId) => SpriteFor(outfit, npcId, Vector2Int.down) != null;

        // What a villager wears now, from the game's outfit sources (null: their own clothes).
        public static string Current(GameSession session, string npcId) => session != null && session.InGame ? session.Hooks.OutfitFor(npcId) : null;
    }
}
