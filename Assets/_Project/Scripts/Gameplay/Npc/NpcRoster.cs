using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The nine villagers added in Milestone 3 (T-054), making twelve: eight may be romanced, four are not. Allegiance is left
    // empty: who belongs to the Keepers is data the horror layer fills in (X-003). Every one is built from the same pattern
    // (home, a post at work, an evening spot, a day off, a rainy-day variant), tuned per person below.
    public static class NpcRoster
    {
        public const string Marcus = "marcus", Odalys = "odalys", Wren = "wren", Felix = "felix", Juno = "juno",
            Hazel = "hazel", Piper = "piper", Dorian = "dorian", Elara = "elara";

        public static readonly string[] Ids = { Marcus, Odalys, Wren, Felix, Juno, Hazel, Piper, Dorian, Elara };

        // Where each one stands at work, and where the player stands for their heart events (one cell in front).
        public readonly struct Post
        {
            public readonly string Map; public readonly int X, Y;
            public Post(string map, int x, int y) { Map = map; X = x; Y = y; }
        }

        public static Post PostOf(string id)
        {
            switch (id)
            {
                case Marcus: return new Post(MapIds.Carpenter, 6, 4);
                case Odalys: return new Post(MapIds.Clinic, 6, 3);
                case Wren: return new Post(MapIds.Saloon, 9, 7);
                case Felix: return new Post(MapIds.Beach, 11, 11);
                case Juno: return new Post(MapIds.Blacksmith, 2, 3);
                case Hazel: return new Post(MapIds.Library, 4, 5);
                case Piper: return new Post(MapIds.Saloon, 11, 7);
                case Dorian: return new Post(MapIds.Forest, 19, 12);
                default: return new Post(MapIds.Clinic, 3, 3);   // Elara
            }
        }

        static NpcStop Stop(int minute, string map, int x, int y, string facing = "down") =>
            new NpcStop { Minute = minute, Map = map, X = x, Y = y, Facing = facing };

        static NpcScheduleEntry Day(string id, string condition, int priority, params NpcStop[] stops) =>
            new NpcScheduleEntry { Id = id, Condition = condition, Priority = priority, Stops = new List<NpcStop>(stops) };

        // A person with a job: home -> post -> evening spot -> home, a day off somewhere else, and staying in when it rains
        // on the day off.
        static NpcDefinition Person(string id, Season season, int day, string homeMap, int hx, int hy, bool romanceable, string business,
            int workStart, int workEnd, string offDay, Post offSpot, Post evening, string[] loved, string[] liked, string[] disliked,
            string[] dislikedCategories = null, string[] lovedCategories = null)
        {
            var post = PostOf(id);
            var home = Stop(360, homeMap, hx, hy);
            var entries = new List<NpcScheduleEntry>();
            if (offDay != null)
            {
                entries.Add(Day("rainy_day_off", $"weekday:{offDay} && weather:rain", 10, home, Stop(1260, homeMap, hx, hy)));
                entries.Add(Day("day_off", $"weekday:{offDay}", 5, home, Stop(600, offSpot.Map, offSpot.X, offSpot.Y, "up"),
                    Stop(1080, evening.Map, evening.X, evening.Y), Stop(1260, homeMap, hx, hy)));
            }
            // Late workers (the saloon) go straight home when it closes; the others spend the evening out first.
            entries.Add(workEnd >= 1260
                ? Day("workday", null, 0, home, Stop(workStart, post.Map, post.X, post.Y), Stop(workEnd, homeMap, hx, hy))
                : Day("workday", null, 0, home, Stop(workStart, post.Map, post.X, post.Y), Stop(workEnd, evening.Map, evening.X, evening.Y),
                    Stop(workEnd + 240 > 1380 ? 1380 : workEnd + 240, homeMap, hx, hy)));
            return NpcDefinition.Create(id, season, day, homeMap, hx, hy, romanceable, business)
                .WithTastes(loved, liked, disliked, lovedCategories, dislikedCategories)
                .WithSchedule(entries);
        }

        public static NpcDefinition[] CreateAll() => new[]
        {
            // Carpenter. Closed Wednesdays.
            Person(Marcus, Season.Summer, 9, MapIds.Carpenter, 6, 3, false, "carpenter", 520, 1050, "wed", new Post(MapIds.Forest, 19, 15), new Post(MapIds.Saloon, 8, 4),
                new[] { "resource.wood", "crop.pumpkin" }, new[] { "resource.stone", "forage.hazelnut" }, new[] { "forage.dandelion", "crop.kale" }),
            // The clinic's doctor. Closed Saturdays.
            Person(Odalys, Season.Winter, 3, MapIds.Clinic, 8, 5, false, "clinic", 520, 1050, "sat", new Post(MapIds.Library, 5, 3), new Post(MapIds.Library, 8, 5),
                new[] { "forage.elderflower", "crop.spinach" }, new[] { "crop.kale", "forage.snowdrop" }, new[] { "resource.slime", "resource.bone" }),
            // Keeps the saloon (12:00-02:00, closed Tuesdays).
            Person(Wren, Season.Fall, 17, MapIds.Saloon, 11, 8, true, "saloon", 700, 1560, "tue", new Post(MapIds.Beach, 20, 9), new Post(MapIds.Village, 30, 17),
                new[] { "artisan.wine", "crop.hops" }, new[] { "artisan.juice", "crop.tomato" }, new[] { "forage.clam" }),
            // Runs the fish stall (06:00-14:00, closed Thursdays).
            Person(Felix, Season.Summer, 21, MapIds.Saloon, 12, 8, true, "fish", 420, 840, "thu", new Post(MapIds.Forest, 7, 7), new Post(MapIds.Saloon, 5, 4),
                new[] { "fish.tuna", "fish.sturgeon" }, new[] { "fish.carp", "forage.seashell" }, new[] { "forage.dandelion" }, lovedCategories: new[] { "Fish" }),
            // The blacksmith's apprentice.
            Person(Juno, Season.Spring, 25, MapIds.Blacksmith, 8, 3, true, "blacksmith", 540, 1080, "mon", new Post(MapIds.Beach, 14, 12), new Post(MapIds.Saloon, 9, 4),
                new[] { "resource.goldbar", "forage.truffle" }, new[] { "resource.copperbar", "crop.potato" }, new[] { "crop.kale", "forage.dandelion" }),
            // Looks after the library with Ione.
            Person(Hazel, Season.Fall, 8, MapIds.Library, 9, 3, true, "library", 530, 1050, "sat", new Post(MapIds.Forest, 19, 10), new Post(MapIds.Village, 20, 17),
                new[] { "forage.blackberry", "crop.strawberry" }, new[] { "forage.elderflower", "crop.cauliflower" }, new[] { "resource.stone" }),
            // Plays in the saloon in the evenings.
            Person(Piper, Season.Spring, 5, MapIds.Saloon, 12, 7, true, "saloon", 1060, 1440, "tue", new Post(MapIds.Beach, 25, 12), new Post(MapIds.Village, 25, 22),
                new[] { "crop.sunflower", "artisan.jam" }, new[] { "crop.strawberry", "forage.raspberry" }, new[] { "resource.coal", "resource.slime" }),
            // Gathers things in the forest all day; lodges at the carpenter's.
            Person(Dorian, Season.Winter, 11, MapIds.Carpenter, 8, 3, true, null, 480, 1020, null, new Post(MapIds.Forest, 19, 10), new Post(MapIds.Saloon, 4, 4),
                new[] { "forage.mushroom", "forage.truffle" }, new[] { "forage.hazelnut", "forage.wildgarlic" }, new[] { "crop.tomato" }),
            // The clinic's nurse.
            Person(Elara, Season.Spring, 18, MapIds.Clinic, 2, 3, true, "clinic", 520, 1050, "sat", new Post(MapIds.Beach, 20, 9), new Post(MapIds.Village, 28, 17),
                new[] { "forage.snowdrop", "crop.cranberry" }, new[] { "forage.seashell", "crop.spinach" }, new[] { "resource.bone", "resource.bat_wing" }),
        };
    }
}
