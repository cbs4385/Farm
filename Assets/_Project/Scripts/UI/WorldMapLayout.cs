using System.Collections.Generic;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.UI
{
    // Where things go on the pictorial map in the game menu (playtest 2026-10-07: the old map did not match the world). The picture follows the real
    // geography: the farm west of the village, joined by the road; the forest north of the village by the lane, with the woods beyond it and the
    // mine on its east side; the beach south of the village. Inside a region a building is drawn where its door really is, scaled to the region.
    // Plain numbers in "design units" (x to the right, y upwards) so the layout can be tested without any UI.
    public static class WorldMapLayout
    {
        public const float Width = 720f, Height = 440f;
        public const float IconSize = 28f, HomeIconSize = 18f;

        public sealed class Region
        {
            public string Map;
            public float X0, Y0, X1, Y1;
            public int TilesW, TilesH;                    // the map's size in cells (0 for a region that is only a picture)

            public float Width => X1 - X0;
            public float Height => Y1 - Y0;
            public Vector2 Centre => new Vector2((X0 + X1) * 0.5f, (Y0 + Y1) * 0.5f);

            // The place on the picture of a cell of the map.
            public Vector2 At(float tileX, float tileY) =>
                new Vector2(X0 + (tileX + 0.5f) / TilesW * Width, Y0 + (tileY + 0.5f) / TilesH * Height);
        }

        public static readonly Region Farm = new Region { Map = MapIds.Farm, X0 = 10f, Y0 = 95f, X1 = 270f, Y1 = 285f, TilesW = MapLayout.FarmW, TilesH = MapLayout.FarmH };
        public static readonly Region Village = new Region { Map = MapIds.Village, X0 = 290f, Y0 = 85f, X1 = 590f, Y1 = 275f, TilesW = MapLayout.VillageW, TilesH = MapLayout.VillageH };
        public static readonly Region Forest = new Region { Map = MapIds.Forest, X0 = 295f, Y0 = 285f, X1 = 495f, Y1 = 375f, TilesW = 40, TilesH = 30 };
        public static readonly Region Woods = new Region { Map = MapIds.Woods, X0 = 320f, Y0 = 380f, X1 = 470f, Y1 = 438f, TilesW = 0, TilesH = 0 };
        public static readonly Region Beach = new Region { Map = MapIds.Beach, X0 = 258f, Y0 = 5f, X1 = 538f, Y1 = 80f, TilesW = 36, TilesH = 24 };
        public static readonly Region MineHill = new Region { Map = MapIds.Mine, X0 = 515f, Y0 = 285f, X1 = 650f, Y1 = 360f, TilesW = 0, TilesH = 0 };

        public static readonly Region[] Regions = { Farm, Village, Forest, Woods, Beach, MineHill };

        // The cells of the doors (the same as MapBuilder puts them; MapLayoutTests compares them with the built scenes).
        public const int FarmHouseDoorX = 7, FarmHouseDoorY = 20;

        // A building or place with a picture and a label on the map.
        public sealed class Spot
        {
            public string Map;              // the map it leads to (the key of its name: map.<Map>)
            public string Icon;             // the sprite in UiArt
            public string Business;         // the shop id for its opening hours, or null
            public string UnlockFlag;       // a farm building opens when this flag is set (null: always)
            public string Condition;        // a home's door is open when this holds (the villagers are asleep otherwise)
            public float Size = IconSize;   // the picture's width and height (the villagers' cottages are drawn small)
            public Vector2 Position;        // design units
        }

        // The village's buildings by the cell of their door.
        static readonly (string map, string icon, string business, int x, int y)[] VillageDoors =
        {
            (MapIds.GeneralStore, "ui_map_store", "general", 8, 24),
            (MapIds.Blacksmith, "ui_map_smith", "blacksmith", 17, 24),
            (MapIds.Carpenter, "ui_map_carpenter", "carpenter", 32, 24),
            (MapIds.Library, "ui_map_library", "library", 42, 24),
            (MapIds.Saloon, "ui_map_saloon", "saloon", 10, 11),
            (MapIds.Clinic, "ui_map_clinic", "clinic", 35, 11),
            (MapIds.CommunityHall, "ui_map_hall", null, 44, 11),
        };

        public static IReadOnlyList<(string map, string icon, string business, int x, int y)> VillageBuildingDoors => VillageDoors;

        // Every spot on the map for a game: the doors in the village, the farmhouse and the farm's buildings where they stand, then the places
        // the pictures of the regions stand for (the forest, the woods when they are open, the beach, the mine).
        public static List<Spot> Spots(GameState state, bool woodsOpen)
        {
            var spots = new List<Spot>();
            foreach (var (map, icon, business, x, y) in VillageDoors)
                spots.Add(new Spot { Map = map, Icon = icon, Business = business, Position = Village.At(x, y) });
            spots.Add(new Spot { Map = MapIds.FarmHouse, Icon = "ui_map_house", Position = Farm.At(FarmHouseDoorX, FarmHouseDoorY) });
            foreach (var home in NpcHomes.All)
                spots.Add(new Spot { Map = home.Map, Icon = "ui_map_house", Condition = NpcHomes.OpenConditionFor(home.Npc), Size = HomeIconSize, Position = Village.At(home.DoorX, home.DoorY) });
            foreach (var type in FarmBuildings.Types)
            {
                var at = state != null ? FarmBuildings.Find(state, type.Id) : null;
                var x = at != null ? at.X : type.DefaultX;
                var y = at != null ? at.Y : type.DefaultY;
                spots.Add(new Spot
                {
                    Map = type.InteriorMap, UnlockFlag = type.UnlockFlag, Position = Farm.At(x + type.DoorX, y),
                    Icon = type.Id == "coop" ? "ui_map_coop" : type.Id == "barn" ? "ui_map_barn" : "ui_map_greenhouse",
                });
            }
            Separate(spots);

            spots.Add(new Spot { Map = MapIds.Forest, Icon = "ui_map_forest", Position = Forest.At(8, 21) });
            if (woodsOpen) spots.Add(new Spot { Map = MapIds.Woods, Icon = "ui_map_woods", Position = Woods.Centre });
            spots.Add(new Spot { Map = MapIds.Beach, Icon = "ui_map_beach", Position = Beach.At(6, 15) });
            spots.Add(new Spot { Map = MapIds.Mine, Icon = "ui_map_mine", Position = new Vector2(MineHill.X0 + 40f, MineHill.Y0 + 36f) });
            return spots;
        }

        // Where the farmer is shown on the map: at the spot of the building they are in, or at their own place in an outdoor region. Null when
        // the map is not on the picture.
        public static Vector2? PlayerPosition(string map, Vector2 tile, IReadOnlyList<Spot> spots)
        {
            switch (map)
            {
                case MapIds.Farm: return Farm.At(tile.x, tile.y);
                case MapIds.Village: return Village.At(tile.x, tile.y);
                case MapIds.Forest: return Forest.At(tile.x, tile.y);
                case MapIds.Beach: return Beach.At(tile.x, tile.y);
            }
            foreach (var spot in spots) if (spot.Map == map) return spot.Position;
            return null;
        }

        // Moves apart pictures that would sit on top of each other (the farm's buildings stand close together), keeping their order from left to right.
        public static void Separate(List<Spot> spots)
        {
            for (var pass = 0; pass < 8; pass++)
            {
                var moved = false;
                for (var i = 0; i < spots.Count; i++)
                    for (var j = i + 1; j < spots.Count; j++)
                    {
                        var a = spots[i]; var b = spots[j];
                        var d = b.Position - a.Position;
                        var minimum = (a.Size + b.Size) * 0.5f + 2f;
                        if (Mathf.Abs(d.x) >= minimum || Mathf.Abs(d.y) >= minimum) continue;
                        // too close in both directions: push the right-hand one to the right
                        var right = d.x >= 0f ? b : a; var left = d.x >= 0f ? a : b;
                        right.Position = new Vector2(left.Position.x + minimum, right.Position.y);
                        moved = true;
                    }
                if (!moved) return;
            }
        }
    }
}
