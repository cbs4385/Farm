using System.Collections.Generic;
using UnityEngine;

namespace Farm.Gameplay
{
    // The village's shops and the hall: where each stands, where its door is, how big its inside is. One table, read by the scene builder, the route table and the map
    // in the menu, so that moving a building means editing one line (the villagers' cottages have their own table, NpcHomes).
    public static class VillageShops
    {
        public sealed class Shop
        {
            public string Map;                    // the interior's map id, also the id of the door
            public string Style;                  // picks the building picture (prop_bld_<style>)
            public string Business;               // the shop id for opening hours, or null
            public string MapIcon;                // the picture on the map in the menu (UiArt)
            public int X0, X1, Y0, Y1;            // footprint in the village (cells)
            public int DoorX;                     // the door's cell is (DoorX, Y0): every door faces south
            public int InteriorW, InteriorH, InteriorDoorX;

            public int DoorY => Y0;
            public int OutsideY => Y0 - 1;        // where one stands outside the door
            public RectInt Footprint => new RectInt(X0, Y0, X1 - X0 + 1, Y1 - Y0 + 1);
        }

        // The kit's buildings all show their doors on the south wall, so every building stands north of the road with its door on its south side: the saloon and the
        // clinic in the east of the north row, the hall behind the carpenter and the library (a lane runs up between them).
        public static readonly IReadOnlyList<Shop> All = new[]
        {
            new Shop { Map = MapIds.GeneralStore, Style = "general", Business = "general", MapIcon = "ui_map_store", X0 = 5, X1 = 12, Y0 = 24, Y1 = 29, DoorX = 8, InteriorW = 12, InteriorH = 9, InteriorDoorX = 5 },
            new Shop { Map = MapIds.Blacksmith, Style = "blacksmith", Business = "blacksmith", MapIcon = "ui_map_smith", X0 = 14, X1 = 21, Y0 = 24, Y1 = 29, DoorX = 17, InteriorW = 10, InteriorH = 8, InteriorDoorX = 4 },
            new Shop { Map = MapIds.Carpenter, Style = "carpenter", Business = "carpenter", MapIcon = "ui_map_carpenter", X0 = 29, X1 = 36, Y0 = 24, Y1 = 29, DoorX = 32, InteriorW = 10, InteriorH = 8, InteriorDoorX = 4 },
            new Shop { Map = MapIds.Library, Style = "library", Business = "library", MapIcon = "ui_map_library", X0 = 39, X1 = 46, Y0 = 24, Y1 = 29, DoorX = 42, InteriorW = 12, InteriorH = 9, InteriorDoorX = 5 },
            new Shop { Map = MapIds.Saloon, Style = "saloon", Business = "saloon", MapIcon = "ui_map_saloon", X0 = 49, X1 = 56, Y0 = 24, Y1 = 29, DoorX = 52, InteriorW = 14, InteriorH = 10, InteriorDoorX = 6 },
            new Shop { Map = MapIds.Clinic, Style = "clinic", Business = "clinic", MapIcon = "ui_map_clinic", X0 = 64, X1 = 71, Y0 = 24, Y1 = 29, DoorX = 67, InteriorW = 10, InteriorH = 8, InteriorDoorX = 4 },
            new Shop { Map = MapIds.CommunityHall, Style = "hall", Business = null, MapIcon = "ui_map_hall", X0 = 33, X1 = 42, Y0 = 31, Y1 = 36, DoorX = 37, InteriorW = 14, InteriorH = 10, InteriorDoorX = 7 },
        };

        public static Shop For(string map)
        {
            foreach (var s in All) if (s.Map == map) return s;
            return null;
        }
    }
}
