using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The villagers of the base game (M2 ships three; the rest come with later milestones). The content generator turns
    // these rows into NpcDefinition assets (kept once they exist, so they can be tuned in the inspector), and
    // tests and bare projects use them directly. Cells are map coordinates; stops are departure times (minute of day).
    //
    // Map layouts are in MapBuilder: Village road y 16..18, lane x 24..26; building doors on the village map are
    // the spawn cells "from<MapId>"; each interior has its counter side facing the door.
    public static class NpcIds
    {
        public const string Tilda = "tilda";    // runs the general store
        public const string Bram = "bram";      // the blacksmith
        public const string Ione = "ione";      // the librarian
        public static readonly string[] All = { Tilda, Bram, Ione, NpcRoster.Marcus, NpcRoster.Odalys, NpcRoster.Wren, NpcRoster.Felix,
            NpcRoster.Juno, NpcRoster.Hazel, NpcRoster.Piper, NpcRoster.Dorian, NpcRoster.Elara };
    }

    public static class NpcDefaults
    {
        const int Six = 6 * 60, Eight40 = 8 * 60 + 40, Eight50 = 8 * 60 + 50, Noon = 12 * 60;
        const int Half5 = 17 * 60 + 30, Seven = 19 * 60, Nine = 21 * 60, Ten = 22 * 60;

        public static NpcDefinition[] CreateAll() => new[] { Tilda(), Bram(), Ione() }.Concat(NpcRoster.CreateAll()).ToArray();

        static NpcStop Stop(int minute, string map, int x, int y, string facing = "down") =>
            new NpcStop { Minute = minute, Map = map, X = x, Y = y, Facing = facing };

        static NpcScheduleEntry Day(string id, string condition, int priority, params NpcStop[] stops) =>
            new NpcScheduleEntry { Id = id, Condition = condition, Priority = priority, Stops = new List<NpcStop>(stops) };

        // ---- Tilda: shopkeeper. Open 09:00-17:00, closed Sunday. Warm and chatty; first neighbour the player meets. ----
        static NpcDefinition Tilda()
        {
            var home = NpcHomes.Stop(Six, NpcIds.Tilda);
            return NpcDefinition.Create(NpcIds.Tilda, Season.Spring, 12, MapIds.HomeTilda, NpcHomes.StandX, NpcHomes.StandY, romanceable: false, business: "general")
                .WithTastes(
                    loved: new[] { "crop.strawberry", "forage.elderflower", "food.pumpkin_pie" },
                    liked: new[] { "crop.cauliflower", "crop.potato", "forage.raspberry" },
                    disliked: new[] { "resource.stone", "forage.clam", "resource.slime" },
                    dislikedCategories: new[] { "Fish" })
                .WithSchedule(NpcHomes.WithSleep(NpcIds.Tilda, new[]
                {
                    // Rainy Sundays she stays upstairs with a book.
                    Day("rainy_sunday", "weekday:sun && weather:rain", 10, home, NpcHomes.Stop(Ten, NpcIds.Tilda)),
                    Day("sunday", "weekday:sun", 5, home,
                        Stop(10 * 60, MapIds.Beach, 14, 12, "up"), Stop(15 * 60, MapIds.Village, 25, 20, "down"),
                        Stop(Seven, MapIds.Saloon, 6, 4, "up"), NpcHomes.Stop(Ten, NpcIds.Tilda)),
                    Day("workday", null, 0, home,
                        NpcHomes.Depart(Eight40, NpcIds.Tilda, MapIds.GeneralStore, 7, 5), Stop(Half5, MapIds.Village, 25, 20, "down"),
                        Stop(Seven, MapIds.Saloon, 6, 4, "up"), NpcHomes.Stop(Ten, NpcIds.Tilda)),
                }));
        }

        // ---- Bram: blacksmith. Open 09:00-17:00, closed Monday. Gruff, loves a good mushroom. ----
        static NpcDefinition Bram()
        {
            var home = NpcHomes.Stop(Six, NpcIds.Bram);
            return NpcDefinition.Create(NpcIds.Bram, Season.Fall, 4, MapIds.HomeBram, NpcHomes.StandX, NpcHomes.StandY, romanceable: false, business: "blacksmith")
                .WithTastes(
                    loved: new[] { "forage.truffle", "forage.mushroom", "food.roasted_roots" },
                    liked: new[] { "resource.copperbar", "forage.hazelnut", "crop.potato" },
                    disliked: new[] { "forage.dandelion", "crop.kale", "fish.pufferfish" })
                .WithSchedule(NpcHomes.WithSleep(NpcIds.Bram, new[]
                {
                    Day("rainy_day_off", "weekday:mon && weather:rain", 10, home, NpcHomes.Stop(Nine, NpcIds.Bram)),
                    Day("day_off", "weekday:mon", 5, home,
                        Stop(10 * 60, MapIds.Forest, 19, 10, "up"), Stop(14 * 60, MapIds.Village, 30, 17, "left"),
                        Stop(18 * 60, MapIds.Saloon, 7, 4, "up"), NpcHomes.Stop(Nine, NpcIds.Bram)),
                    Day("workday", null, 0, home,
                        NpcHomes.Depart(Eight50, NpcIds.Bram, MapIds.Blacksmith, 6, 4), Stop(Half5, MapIds.Saloon, 7, 4, "up"),
                        NpcHomes.Stop(Nine, NpcIds.Bram)),
                }));
        }

        // ---- Ione: librarian. Open 09:00-17:00, closed Saturday. Quiet; likes the sea and the first snow. ----
        static NpcDefinition Ione()
        {
            var home = NpcHomes.Stop(Six, NpcIds.Ione);
            return NpcDefinition.Create(NpcIds.Ione, Season.Winter, 20, MapIds.HomeIone, NpcHomes.StandX, NpcHomes.StandY, romanceable: true, business: "library")
                .WithTastes(
                    loved: new[] { "forage.pearl", "forage.snowdrop", "forage.elderflower" },
                    liked: new[] { "forage.seashell", "crop.kale", "forage.blackberry" },
                    disliked: new[] { "resource.stone", "resource.wood", "resource.coal" })
                .WithSchedule(NpcHomes.WithSleep(NpcIds.Ione, new[]
                {
                    Day("rainy_saturday", "weekday:sat && weather:rain", 10, home, NpcHomes.Stop(Ten, NpcIds.Ione)),
                    Day("saturday", "weekday:sat", 5, home,
                        Stop(10 * 60, MapIds.Forest, 19, 15, "up"), Stop(15 * 60, MapIds.Village, 21, 17, "right"),
                        NpcHomes.Stop(Ten, NpcIds.Ione)),
                    Day("summer_evenings", "season:summer", 3, home,
                        NpcHomes.Depart(Eight50, NpcIds.Ione, MapIds.Library, 7, 5), Stop(Half5, MapIds.Beach, 20, 9, "up"),
                        NpcHomes.Stop(Nine, NpcIds.Ione)),
                    Day("workday", null, 0, home,
                        NpcHomes.Depart(Eight50, NpcIds.Ione, MapIds.Library, 7, 5), Stop(Half5, MapIds.Village, 21, 17, "right"),
                        NpcHomes.Stop(Nine, NpcIds.Ione)),
                }));
        }
    }
}
