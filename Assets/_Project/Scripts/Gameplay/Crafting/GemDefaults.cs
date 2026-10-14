using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Gems (playtest 2026-10-13): clusters in the deeper mine floors, each a stone that drops its gem; the gems go into the Community Hall's later rooms and into jewelry
    // that can be sold or given. Ids are saved and never renamed. The pictures are the Caves and Mines Props crystal, recoloured (tools/art/build_gems.py).
    public readonly struct GemRow
    {
        public readonly string Id, NodeId, ItemId;
        public readonly int MinFloor, Sell, ToolTier, Hits;
        public readonly float Weight;          // the share of ore spots that hold this gem on a floor where it can appear
        public readonly Color Color;

        public GemRow(string id, int minFloor, int sell, int toolTier, int hits, float weight, Color color)
        {
            Id = id; NodeId = id + "_node"; ItemId = "gem." + id; MinFloor = minFloor; Sell = sell; ToolTier = toolTier; Hits = hits; Weight = weight; Color = color;
        }
    }

    public static class GemDefaults
    {
        public const string Skill = "mining";
        public const int FirstFloor = 16;

        public static readonly GemRow[] Gems =
        {
            new GemRow("amethyst", 16, 100, 1, 4, 1.0f, new Color(0.62f, 0.30f, 0.80f)),
            new GemRow("topaz", 20, 120, 1, 4, 0.9f, new Color(0.95f, 0.70f, 0.20f)),
            new GemRow("aquamarine", 24, 150, 1, 5, 0.8f, new Color(0.35f, 0.80f, 0.90f)),
            new GemRow("emerald", 28, 200, 2, 5, 0.7f, new Color(0.20f, 0.70f, 0.35f)),
            new GemRow("ruby", 32, 260, 2, 6, 0.55f, new Color(0.80f, 0.15f, 0.20f)),
            new GemRow("diamond", 36, 400, 3, 7, 0.35f, new Color(0.88f, 0.94f, 1f)),
        };

        public static bool IsGem(string itemId) => itemId != null && itemId.StartsWith("gem.", System.StringComparison.Ordinal);
        public static bool IsJewelry(string itemId) => itemId != null && itemId.StartsWith("jewelry.", System.StringComparison.Ordinal);

        public static string RingId(GemRow g) => "jewelry." + g.Id + "_ring";
        public static string PendantId(GemRow g) => "jewelry." + g.Id + "_pendant";

        // A ring is worth the gem twice over and a gold bar; a pendant the gem and an iron bar's worth, more than the gem sold by itself.
        public static int RingSell(GemRow g) => g.Sell * 2 + 250;
        public static int PendantSell(GemRow g) => g.Sell + 160;

        public static ResourceNodeDefinition[] CreateNodes() => Gems.Select(g =>
            ResourceNodeDefinition.Create(g.NodeId, ToolType.Pickaxe, g.Hits, g.ToolTier, g.ItemId, 1, g.MinFloor >= 28 ? 3 : 2, Skill, 12 + g.MinFloor / 2, null, true, 0f)).ToArray();

        public static ExtraItemRow[] CreateItems()
        {
            var rows = new List<ExtraItemRow>();
            foreach (var g in Gems)
            {
                rows.Add(new ExtraItemRow(g.ItemId, ItemCategory.Resource, g.Sell, g.Color));
                rows.Add(new ExtraItemRow(RingId(g), ItemCategory.Artisan, RingSell(g), g.Color));
                rows.Add(new ExtraItemRow(PendantId(g), ItemCategory.Artisan, PendantSell(g), g.Color));
            }
            return rows.ToArray();
        }

        // The gem node a floor holds for this roll (0..1 over the gems that can appear there), or null when none can.
        public static string NodeFor(int floor, float roll)
        {
            var open = Gems.Where(g => floor >= g.MinFloor).ToList();
            if (open.Count == 0) return null;
            var at = roll * open.Sum(g => g.Weight);
            foreach (var g in open)
            {
                if (at < g.Weight) return g.NodeId;
                at -= g.Weight;
            }
            return open[open.Count - 1].NodeId;
        }

        // Share of a deeper floor's ore spots that are a gem cluster instead.
        public static float ChanceOnFloor(int floor) => floor < FirstFloor ? 0f : System.Math.Min(0.22f, 0.07f + 0.004f * (floor - FirstFloor));
    }
}
