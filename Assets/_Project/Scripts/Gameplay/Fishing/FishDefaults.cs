using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // One catchable fish: where, when and how hard. Weight is how common it is among the fish that can bite right now.
    // Condition is an optional condition expression (night, weather, moon...). The content tools write the item assets from
    // this table, so it is the one place to tune fishing.
    public readonly struct FishRow
    {
        public readonly string Id, Name, Spot, Condition;
        public readonly SeasonMask Seasons;
        public readonly float Weight, Difficulty;     // difficulty 0 (easy) .. 1 (very hard)
        public readonly int Sell;
        public readonly Color Color;

        public FishRow(string id, string name, string spot, SeasonMask seasons, float weight, float difficulty, int sell, Color color, string condition = null)
        {
            Id = id; Name = name; Spot = spot; Seasons = seasons; Weight = weight; Difficulty = difficulty; Sell = sell; Color = color; Condition = condition;
        }

        public string ItemId => "fish." + Id;
    }

    public static class FishSpots
    {
        public const string Ocean = "ocean";    // the beach
        public const string Pond = "pond";      // the ponds of the forest, the village and the farm

        public static string ForMap(string mapId) => mapId == MapIds.Beach ? Ocean : mapId == MapIds.Forest || mapId == MapIds.Village || mapId == MapIds.Farm ? Pond : null;
    }

    public static class FishDefaults
    {
        const SeasonMask Sp = SeasonMask.Spring, Su = SeasonMask.Summer, Fa = SeasonMask.Fall, Wi = SeasonMask.Winter, All = SeasonMask.All;
        static Color C(float r, float g, float b) => new Color(r, g, b);

        public static readonly FishRow[] Rows =
        {
            // The ocean
            new FishRow("sardine", "Sardine", FishSpots.Ocean, All, 10, 0.1f, 40, C(0.7f, 0.75f, 0.8f)),
            new FishRow("anchovy", "Anchovy", FishSpots.Ocean, Sp | Su, 8, 0.15f, 45, C(0.55f, 0.65f, 0.75f)),
            new FishRow("herring", "Herring", FishSpots.Ocean, Fa | Wi, 8, 0.2f, 50, C(0.6f, 0.7f, 0.8f)),
            new FishRow("mackerel", "Mackerel", FishSpots.Ocean, Su, 6, 0.3f, 80, C(0.3f, 0.5f, 0.6f)),
            new FishRow("flounder", "Flounder", FishSpots.Ocean, Sp | Su, 5, 0.35f, 100, C(0.65f, 0.55f, 0.4f)),
            new FishRow("sea_bass", "Sea Bass", FishSpots.Ocean, Sp | Fa, 5, 0.4f, 120, C(0.5f, 0.55f, 0.55f)),
            new FishRow("pufferfish", "Pufferfish", FishSpots.Ocean, Su, 3, 0.6f, 200, C(0.9f, 0.8f, 0.4f), "weather:sunny"),
            new FishRow("eel", "Eel", FishSpots.Ocean, All, 3, 0.55f, 180, C(0.3f, 0.35f, 0.3f), "weather:rain"),
            new FishRow("octopus", "Octopus", FishSpots.Ocean, Su | Fa, 2, 0.6f, 250, C(0.7f, 0.3f, 0.5f), "hour>=20"),
            new FishRow("squid", "Squid", FishSpots.Ocean, Wi, 3, 0.5f, 150, C(0.8f, 0.6f, 0.7f), "hour>=20"),
            new FishRow("tuna", "Tuna", FishSpots.Ocean, Su, 2, 0.8f, 300, C(0.2f, 0.3f, 0.6f)),
            // The pond
            new FishRow("carp", "Carp", FishSpots.Pond, All, 10, 0.1f, 50, C(0.7f, 0.55f, 0.3f)),
            new FishRow("perch", "Perch", FishSpots.Pond, Sp | Su, 8, 0.2f, 60, C(0.4f, 0.6f, 0.3f)),
            new FishRow("bream", "Bream", FishSpots.Pond, Su, 6, 0.25f, 70, C(0.65f, 0.65f, 0.55f)),
            new FishRow("trout", "Trout", FishSpots.Pond, Sp | Fa, 6, 0.35f, 90, C(0.55f, 0.5f, 0.6f)),
            new FishRow("catfish", "Catfish", FishSpots.Pond, All, 4, 0.5f, 130, C(0.4f, 0.35f, 0.3f), "weather:rain"),
            new FishRow("pike", "Pike", FishSpots.Pond, Fa | Wi, 4, 0.55f, 150, C(0.3f, 0.5f, 0.35f)),
            new FishRow("glass_minnow", "Glass Minnow", FishSpots.Pond, Wi, 6, 0.15f, 40, C(0.8f, 0.9f, 0.95f)),
            new FishRow("sturgeon", "Sturgeon", FishSpots.Pond, Wi, 2, 0.8f, 300, C(0.35f, 0.4f, 0.45f)),
            new FishRow("moonfish", "Moonfish", FishSpots.Pond, All, 1, 0.7f, 400, C(0.9f, 0.9f, 1f), "moon:full && hour>=21"),
        };

        public const string Rod = "tool.rod";
        public const string Bait = "resource.bait";

        public static ExtraItemRow[] CreateItems()
        {
            var rows = Rows.Select(r => new ExtraItemRow(r.ItemId, ItemCategory.Fish, r.Sell, r.Color)).ToList();
            rows.Add(new ExtraItemRow(Bait, ItemCategory.Resource, 1, C(0.8f, 0.5f, 0.5f), buy: 5, soldIn: new[] { "fish" }));
            return rows.ToArray();
        }
    }
}
