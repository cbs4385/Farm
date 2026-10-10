using System.Collections.Generic;
using System.IO;
using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Editor
{
    // T-013/T-014/T-031: generates every map scene (Farm, FarmHouse, Village, Forest, Beach and the six village
    // interiors) with placeholder art. Re-running overwrites them. Layout is data-like constants below; every door has
    // a spawn point outside it named "from<MapId>" and every interior has an exit back to the village.
    public static class MapBuilder
    {
        const string SceneDir = "Assets/_Project/Scenes";
        const string ArtDir = "Assets/_Project/Art/Placeholders";
        const string TileDir = "Assets/_Project/Art/Tiles";

        // Farm: 44 x 32 cells. House block at x 4..11, y 20..24, door at (7, 20). The east edge opens onto the village.
        const int FarmW = MapLayout.FarmW, FarmH = MapLayout.FarmH;
        const int HouseX0 = 4, HouseX1 = 11, HouseY0 = 20, HouseY1 = 24, DoorX = 7;
        // The greenhouse, coop and barn are not baked into the scene: FarmBuildingsView draws them from the saved game state (the player can
        // move them), and their doors are locked until the carpenter has built them (FarmBuildings has the types and the default places).
        const int FarmExitY0 = MapLayout.FarmRoadY - 1, FarmExitY1 = MapLayout.FarmRoadY + 1;

        // FarmHouse interior: 12 x 9 cells.
        const int InW = 12, InH = 9;

        // Village: 50 x 36. A main road (y 16..18) runs west-east to the farm; a lane (x 24..26) runs south to the
        // beach and north to the forest. Four buildings face the road from the north, two from the south.
        const int VillageW = MapLayout.VillageW, VillageH = MapLayout.VillageH;
        const int RoadY0 = 16, RoadY1 = 18, LaneX0 = 24, LaneX1 = 26;

        struct Building
        {
            public string MapId, Business, Style;   // Style picks the wall, roof, window, sign and roof ornament art (bld_<style>_*)
            public int X0, X1, Y0, Y1, DoorX;
            public string Condition, BlockedKey;   // a door that is locked at times (the villagers' homes at night)
            public bool FacesSouth;       // door on the south wall (north row of buildings) or the north wall
        }

        // The shops and the hall come from the shared table (VillageShops); the cottages are added by VillageBuildings().
        static readonly Building[] Buildings = VillageShops.All.Select(v => new Building
        {
            MapId = v.Map, Style = v.Style, Business = v.Business, X0 = v.X0, X1 = v.X1, Y0 = v.Y0, Y1 = v.Y1, DoorX = v.DoorX, FacesSouth = true,
        }).ToArray();

        // The footprints of the old shops (for tests that keep the villagers' streets clear of them).
        public static List<RectInt> ShopFootprints() => Buildings.Select(b => new RectInt(b.X0, b.Y0, b.X1 - b.X0 + 1, b.Y1 - b.Y0 + 1)).ToList();

        // The villagers' cottages (NpcHomes) built like the shops: a door, windows, a nameplate, a chimney that smokes.
        static IEnumerable<Building> VillageBuildings()
        {
            foreach (var b in Buildings) yield return b;
            foreach (var h in NpcHomes.All)
                yield return new Building
                {
                    MapId = h.Map, Style = h.Style, Business = null, X0 = h.X0, X1 = h.X1, Y0 = h.Y0, Y1 = h.Y1, DoorX = h.DoorX, FacesSouth = h.FacesSouth,
                    Condition = NpcHomes.OpenConditionFor(h.Npc), BlockedKey = NpcHomes.LockedKey,
                };
        }

        // A piece of furniture in an interior. ShopId makes it a working counter.
        struct Prop
        {
            public string Name, Sprite, ShopId, UpgradesAt;   // UpgradesAt: a counter selling upgrades for that business
            public string SeatNpc, SeatKey, Curio;                   // a seat kept for the player by that villager
            public int X, Y, W, H;                                   // the cells it covers: W x H from (X, Y) upwards and to the right
            public Prop(string name, string sprite, int x, int y, string shopId = null, string upgradesAt = null, string seatNpc = null, string seatKey = null, string curio = null, int w = 1, int h = 1)
            { Name = name; Sprite = sprite; X = x; Y = y; W = w; H = h; ShopId = shopId; UpgradesAt = upgradesAt; SeatNpc = seatNpc; SeatKey = seatKey; Curio = curio; }
        }

        public static void BuildAll()
        {
            Directory.CreateDirectory(TileDir);
            BuildFarm();
            BuildFarmHouse();
            BuildGreenhouse();
            BuildAnimalHouse(MapIds.Coop);
            BuildAnimalHouse(MapIds.Barn);
            BuildVillage();
            BuildForest();
            BuildWoods();
            BuildBeach();
            BuildMine();

            foreach (var home in NpcHomes.All) BuildHome(home);

            BuildShopInterior(MapIds.GeneralStore, new[]
            {
                new Prop("Counter", "obj_counter_wide", 3, 5, "general", w: 2), new Prop("Counter2", "obj_counter_wide", 5, 5, "general", w: 2),
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Crate1", "obj_bin", 1, 2), new Prop("Crate2", "obj_bin", 10, 2),
                new Prop("PackCounter", "obj_counter_wide", 9, 4, upgradesAt: "general", w: 2),
                new Prop("Seat", "obj_chair", 1, 5, seatNpc: "tilda", seatKey: "window"),
                new Prop("CatDoor", "obj_cat_door", 10, 5, curio: "catdoor"),
            });
            BuildShopInterior(MapIds.Blacksmith, new[]
            {
                new Prop("Counter1", "obj_counter_wide", 2, 4, w: 2), new Prop("Counter2", "obj_counter_wide", 4, 4, upgradesAt: "blacksmith", w: 2),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Shelf3", "obj_shelf", 7, 6),
                new Prop("Shelf4", "obj_shelf", 8, 6), new Prop("Anvil", "obj_table", 7, 2),
            });
            BuildShopInterior(MapIds.Carpenter, new[]
            {
                new Prop("Counter1", "obj_counter_wide", 2, 4, "carpenter", w: 2), new Prop("Counter2", "obj_counter_wide", 4, 4, upgradesAt: "carpenter", w: 2),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Bench", "obj_table", 7, 5),
                new Prop("Bench2", "obj_table", 8, 5), new Prop("Planks", "obj_bin", 8, 2),
            });
            BuildShopInterior(MapIds.Saloon, new[]
            {
                new Prop("Bar1", "obj_counter_wide", 3, 7, w: 2), new Prop("Bar2", "obj_counter_wide", 5, 7, w: 2), new Prop("Bar3", "obj_counter_wide", 7, 7, w: 2),
                new Prop("Table1", "obj_table", 2, 3), new Prop("Table2", "obj_table", 11, 3), new Prop("Table3", "obj_table", 11, 5),
                new Prop("Table4", "obj_table", 2, 5), new Prop("Shelf1", "obj_shelf", 4, 8), new Prop("Shelf2", "obj_shelf", 6, 8),
                new Prop("Seat", "obj_chair", 12, 3, seatNpc: "wren", seatKey: "stool"),
                new Prop("WindowLantern", "item_prop_lantern", 10, 8, curio: "lantern"),
            });
            BuildShopInterior(MapIds.Clinic, new[]
            {
                new Prop("Bed1", "obj_bed", 2, 6), new Prop("Bed2", "obj_bed", 4, 6), new Prop("Bed3", "obj_bed", 6, 6),
                new Prop("Couch", "obj_couch", 8, 6, seatNpc: "elara", seatKey: "couch"),
                new Prop("Desk1", "obj_counter_wide", 7, 3, upgradesAt: "clinic", w: 2), new Prop("Shelf", "obj_shelf", 1, 4),
            });
            BuildShopInterior(MapIds.CommunityHall, new[]
            {
                new Prop("Board", "obj_board", 7, 7), new Prop("Table1", "obj_table", 3, 4), new Prop("Table2", "obj_table", 10, 4),
                new Prop("Shelf1", "obj_shelf", 2, 8), new Prop("Shelf2", "obj_shelf", 11, 8),
            });
            BuildShopInterior(MapIds.Library, new[]
            {
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Shelf7", "obj_shelf", 2, 4), new Prop("Shelf8", "obj_shelf", 3, 4), new Prop("Shelf9", "obj_shelf", 8, 4),
                new Prop("Shelf10", "obj_shelf", 9, 4), new Prop("Desk", "obj_counter_wide", 5, 5, w: 2), new Prop("Reading", "obj_table", 10, 2),
                new Prop("Seat", "obj_chair", 1, 2, seatNpc: "ione", seatKey: "chair"),
                new Prop("NookSeat", "obj_armchair", 10, 5, seatNpc: "hazel", seatKey: "nook"),
                new Prop("BackShelf", "obj_shelf", 10, 7, curio: "backshelf"),
                new Prop("HazelShelf", "obj_shelf", 1, 7, curio: "hazelshelf"),
            });
        }

        // ---- the villagers' homes ----------------------------------------------------------------------------------

        // What makes one room its own: two pieces of furniture, put in the two spare corners.
        static readonly Dictionary<string, (string a, string b)> HomeExtras = new Dictionary<string, (string, string)>
        {
            { NpcIds.Tilda, ("obj_shelf", "obj_vase") }, { NpcIds.Bram, ("obj_table", "obj_clock") }, { NpcIds.Ione, ("obj_bookshelf", "obj_armchair") },
            { NpcRoster.Marcus, ("obj_table", "obj_painting") }, { NpcRoster.Odalys, ("obj_vase", "obj_plant") }, { NpcRoster.Wren, ("obj_painting", "obj_chair") },
            { NpcRoster.Felix, ("obj_bin", "obj_plant") }, { NpcRoster.Juno, ("obj_table", "obj_lamp") }, { NpcRoster.Hazel, ("obj_bookshelf", "obj_vase") },
            { NpcRoster.Piper, ("obj_clock", "obj_armchair") }, { NpcRoster.Dorian, ("obj_bin", "obj_bookshelf") }, { NpcRoster.Elara, ("obj_plant", "obj_vase") },
        };

        // A one-room home, 10 x 8: a bed, a hearth, a table with two chairs, a shelf, and two things of the owner's. The owner stands at (3, 3).
        static void BuildHome(NpcHomes.Home home)
        {
            var extras = HomeExtras[home.Npc];
            BuildInterior(home.Map, NpcHomes.InteriorW, NpcHomes.InteriorH, NpcHomes.InteriorDoorX, new[]
            {
                new Prop("Bed", "obj_bed_double", NpcHomes.BedCellsX, NpcHomes.BedCellsY, w: NpcHomes.BedSize, h: NpcHomes.BedSize), new Prop("Hearth", "obj_fireplace", 4, 5, w: 2, h: 2),
                new Prop("Table", "obj_dining_table", 7, 3), new Prop("ChairWest", "obj_chair", 6, 3), new Prop("ChairEast", "obj_chair", 8, 3),
                new Prop("Shelf", "obj_bookshelf", 8, 6), new Prop("Plant", "obj_plant", 8, 1), new Prop("Lamp", "obj_lamp", 1, 2),
                new Prop("OwnerA", extras.a, 1, 4), new Prop("OwnerB", extras.b, 8, 4),
            });
        }

        // ---- Farm ---------------------------------------------------------------------------------------------

        static void BuildFarm()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Farm, indoor: false, allowFarming: true);

            for (var y = 0; y < FarmH; y++)
                for (var x = 0; x < FarmW; x++)
                {
                    var ground = (x * 31 + y * 17) % 23 == 0 ? "tile_dirt" : "tile_grass";
                    // Path from the door south, then east along the road to the village.
                    if (x == DoorX && y < HouseY0 && y >= 14) ground = "tile_path";
                    if (y >= FarmExitY0 + 1 && y <= FarmExitY1 - 1 && x >= DoorX) ground = "tile_path";
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(ground));

                }
            PaintEdgeBand(rig, MapIds.Farm, FarmW, FarmH, (bx, by) => bx == FarmW - 1 && by >= FarmExitY0 && by <= FarmExitY1);
            StandBuilding(rig, "farmhouse", "Farmhouse_Building", DoorX, HouseY0, HouseY0, HouseY1);                      // the kit's farm house over the old footprint
            rig.Ground.SetTile(new Vector3Int(DoorX, HouseY0, 0), GetTile("tile_floor_wood"));
            rig.Map.gameObject.AddComponent<FarmBuildingsView>().Configure(rig.Map, GetTile("tile_wall"), GetTile("tile_roof"), GetTile("tile_door"), GetHiddenWall(), Sprite("prop_bld_coop"), Sprite("prop_bld_barn"));

            AddSpawn("default", Center(DoorX, HouseY0 - 3));
            AddSpawn("fromHouse", Center(DoorX, HouseY0 - 2));
            AddSpawn("fromVillage", Center(MapLayout.FarmArriveX, MapLayout.FarmRoadY));
            AddSpawn("sleepwalk", Center(10, 15));
            AddWarp(Center(DoorX, HouseY0), MapIds.FarmHouse, "default");
            AddWarp(Center(MapLayout.FarmExitX, MapLayout.FarmRoadY), MapIds.Village, "fromFarm", new Vector2(1f, 3f));

            // A tall red box with a yellow flag (16 x 32): the old one was a small blue blob that was easy to miss. It stands on cell (10, 19) and reaches up a cell.
            AddObject("Mailbox", "obj_mailbox_tall", Center(10, 19) + new Vector3(0f, 0.5f, 0f), solid: true, size: new Vector2(1f, 2f)).AddComponent<Mailbox>();

            // The shipping bin stays on the farm; the general store now lives in the village.
            var bin = AddObject("ShippingBin", "obj_shipping_bin", Center(13, 17) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));      // a 2 x 2 bin on cells (13..14, 17..18)
            bin.AddComponent<ShippingBin>();
            var binFixture = bin.AddComponent<MovableFixture>();      // the mallet can carry it next to the house or anywhere else on the farm
            binFixture.Id = "shipping_bin";
            binFixture.Size = new Vector2Int(2, 2);

            // By the farmhouse: hay and sacks against the wall and a wheelbarrow, where the farm's own work happens.
            MakeMovable(AddProp("Hay", "prop_hay", 13, 21), "farm_hay");
            MakeMovable(AddProp("Sacks", "prop_sacks", 14, 21), "farm_sacks");
            MakeMovable(AddProp("Wheelbarrow", "prop_wheelbarrow", 3, 22), "farm_wheelbarrow");
            MakeMovable(AddProp("Barrels", "prop_barrels", 3, 23), "farm_barrels");

            // A pond to the north-east of the fields (fishing, and something to look at): one oval of water with a shoreline, walled so that one stops at the shore.
            PaintPond(rig, MapLayout.FarmPond.Contains, 50, 66, 33, 45);

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Farm}.unity");
        }

        // ---- FarmHouse ----------------------------------------------------------------------------------------

        struct Furnishing
        {
            public string Name, Sprite;
            public int X, Y, W, H;
            public bool Solid;
            public Furnishing(string name, string sprite, int x, int y, int w = 1, int h = 1, bool solid = true) { Name = name; Sprite = sprite; X = x; Y = y; W = w; H = h; Solid = solid; }
        }

        // The farmhouse is 12 x 9 (floor cells 1..10 by 1..7, the door at x 5 on the south wall). The north wall carries the bed, a lamp, a picture, the
        // hearth, a clock and the wardrobe (the kitchen stands at 9, 7); the east wall a bookshelf; a dining corner and a sitting corner fill the rest.
        static readonly Furnishing[] HouseFurnishings =
        {
            new Furnishing("RugLarge", "obj_rug_large", 3, 3, 2, 2, solid: false),
            new Furnishing("Fireplace", "obj_fireplace", 5, 6, 2, 2),
            new Furnishing("Lamp", "obj_lamp", 3, 7), new Furnishing("Painting", "obj_painting", 4, 7), new Furnishing("Clock", "obj_clock", 7, 7),
            new Furnishing("Bookshelf", "obj_bookshelf", 8, 7), new Furnishing("Wardrobe", "obj_wardrobe", 10, 7),
            new Furnishing("BookshelfEast", "obj_bookshelf", 10, 5), new Furnishing("PlantEast", "obj_plant", 10, 2),
            new Furnishing("Couch", "obj_couch", 1, 4, 2, 1), new Furnishing("Armchair", "obj_armchair", 6, 4), new Furnishing("PlantWest", "obj_plant", 1, 1),
            new Furnishing("DiningTable", "obj_dining_table", 8, 3), new Furnishing("ChairWest", "obj_chair", 7, 3), new Furnishing("ChairEast", "obj_chair", 9, 3),
            new Furnishing("Vase", "obj_vase", 10, 1), new Furnishing("Bench", "obj_bench", 8, 1),
        };

        static void BuildFarmHouse()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.FarmHouse, indoor: true, allowFarming: false);

            for (var y = 0; y < InH; y++)
                for (var x = 0; x < InW; x++)
                {
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_floor_wood"));
                    var edge = x == 0 || y == 0 || x == InW - 1 || y == InH - 1;
                    var door = y == 0 && x == 5;
                    if (edge && !door) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(5, 2));
            AddSpawn("bed", Center(3, 6));
            AddWarp(Center(5, 0), MapIds.Farm, "fromHouse");

            // Furnishings the farmhouse starts with (more can be bought at the carpenter's). Nothing here may close off the bed, the kitchen or the door.
            foreach (var p in HouseFurnishings)
            {
                var piece = AddObject(p.Name, p.Sprite, Center(p.X, p.Y) + new Vector3((p.W - 1) * 0.5f, (p.H - 1) * 0.5f, 0f), solid: p.Solid, size: new Vector2(p.W, p.H));
                if (!p.Solid) piece.GetComponent<SpriteRenderer>().sortingOrder = 1;      // a rug lies under everything that stands on it
                var fixture = piece.AddComponent<MovableFixture>();      // everything the player can furnish with can be lifted with the mallet and set down elsewhere
                fixture.Id = "furn_" + p.Name.ToLowerInvariant();
                fixture.Size = new Vector2Int(p.W, p.H);
                fixture.Walkable = !p.Solid;
                MakeSeat(piece, p.Sprite, p.Name);
            }

            // A double bed, two cells square, in the north-west corner (cells 1..2, 6..7). One wakes up on the cell east of it.
            var bed = AddObject("Bed", "obj_bed_double", Center(1, 6) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));
            bed.AddComponent<Bed>();
            var bedFixture = bed.AddComponent<MovableFixture>();
            bedFixture.Id = "bed";
            bedFixture.SpawnId = "bed";
            bedFixture.SpawnOffset = new Vector2Int(2, 0);
            bedFixture.Size = new Vector2Int(2, 2);

            // The kitchen: cook with what is in the backpack.
            var kitchen = AddObject("Kitchen", "obj_kitchen", Center(9, 7), solid: true);
            kitchen.AddComponent<Kitchen>();
            kitchen.AddComponent<MovableFixture>().Id = "kitchen";

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.FarmHouse}.unity");
        }

        // ---- Greenhouse ---------------------------------------------------------------------------------------

        const int GreenW = 16, GreenH = 10;

        static void BuildGreenhouse()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Greenhouse, indoor: true, allowFarming: true);

            for (var y = 0; y < GreenH; y++)
                for (var x = 0; x < GreenW; x++)
                {
                    var door = y == 0 && x == 7;
                    var edge = x == 0 || y == 0 || x == GreenW - 1 || y == GreenH - 1;
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(door ? "tile_floor_wood" : "tile_dirt"));
                    if (edge && !door) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(7, 2));
            AddWarp(Center(7, 0), MapIds.Farm, "fromGreenhouse");

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Greenhouse}.unity");
        }

        // ---- Village ------------------------------------------------------------------------------------------

        static void BuildAnimalHouse(string mapId)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(mapId, indoor: true, allowFarming: false);
            const int w = 12, h = 8;
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var door = y == 0 && x == 6;
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_floor_wood"));
                    if ((x == 0 || y == 0 || x == w - 1 || y == h - 1) && !door) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }
            AddSpawn("default", Center(6, 2));
            AddWarp(Center(6, 0), MapIds.Farm, "from" + mapId);
            AddObject("Trough", "obj_trough", Center(10, 6), solid: true).AddComponent<FeedTrough>();
            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{mapId}.unity");
        }

        static void BuildVillage()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Village, indoor: false, allowFarming: false);

            for (var y = 0; y < VillageH; y++)
                for (var x = 0; x < VillageW; x++)
                {
                    var onRoad = y >= RoadY0 && y <= RoadY1;
                    var onLane = x >= LaneX0 && x <= LaneX1;
                    var ground = onRoad || onLane ? "tile_cobble" : (x * 31 + y * 17) % 29 == 0 ? "tile_dirt" : "tile_grass";
                    foreach (var b in VillageBuildings())
                        if (x == b.DoorX && InConnector(b, y) && !MapIds.IsHome(b.MapId)) ground = "tile_path";       // a shop's path to the road (the cottages have alleys)
                    if (NpcHomes.IsCobbled(x, y)) ground = "tile_cobble";
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(ground));
                }

            // Edges are woods, apart from the three ways out: west to the farm, north to the forest, south to the beach.
            PaintEdgeBand(rig, MapIds.Village, VillageW, VillageH, (bx, by) =>
                bx == 0 && by >= RoadY0 && by <= RoadY1 || (by == 0 || by == VillageH - 1) && bx >= LaneX0 && bx <= LaneX1);

            AddSpawn("default", Center(2, 17));
            AddSpawn("fromFarm", Center(2, 17));
            AddSpawn("fromForest", Center(25, VillageH - 3));
            AddSpawn("fromBeach", Center(25, 2));
            AddSpawn("sleepwalk", Center(24, 17));
            AddWarp(Center(0, 17), MapIds.Farm, "fromVillage", new Vector2(1f, 3f));
            AddWarp(Center(25, VillageH - 1), MapIds.Forest, "fromVillage", new Vector2(3f, 1f));
            AddWarp(Center(25, 0), MapIds.Beach, "fromVillage", new Vector2(3f, 1f));

            foreach (var b in VillageBuildings()) PlaceBuilding(rig, b);
            AddObject("HelpWantedBoard", "obj_board", Center(22, 19), solid: true).AddComponent<HelpWantedBoard>();

            // The traveling merchant's stall appears on some days only (the condition `merchant`).
            var merchant = new GameObject("MerchantStall");
            var visible = merchant.AddComponent<ConditionalObject>();
            visible.Condition = "merchant:today";
            var stall = AddObject("Stall", "prop_stall", Center(30, 19), solid: true);
            stall.AddComponent<ShopCounter>().ShopId = "merchant";
            stall.transform.SetParent(merchant.transform, true);

            // A few trees at the corners so the village is not a bare lawn.
            foreach (var t in new[] { new Vector2Int(3, 31), new Vector2Int(46, 33), new Vector2Int(3, 3), new Vector2Int(46, 3), new Vector2Int(20, 4), new Vector2Int(30, 33) })
                AddObject($"Tree_{t.x}_{t.y}", "obj_tree", Center(t.x, t.y), solid: true);

            // The land added to the east and north (the village grew): scattered trees and a thin wood along the edge, never on the road, the lane or a building.
            for (var y = 2; y < VillageH - 2; y++)
                for (var x = 2; x < VillageW - 2; x++)
                {
                    if (x < 50 && y < 36) continue;                                            // the original village
                    if (NpcHomes.InStreet(x, y)) continue;                                      // the villagers' street
                    if (OnAnyBuilding(x, y)) continue;                                          // nor a shop
                    if (y >= RoadY0 - 1 && y <= RoadY1 + 1 || x >= LaneX0 - 1 && x <= LaneX1 + 1) continue;
                    var edgeBand = x >= VillageW - 6 || y >= VillageH - 6;
                    if ((x * 37 + y * 53) % (edgeBand ? 3 : 17) != 0) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);
                }

            // The village square and the roadside (the Cozy Village kit): a clock tower and a fountain between the road and the shops, benches, flags, lamp posts along
            // both edges of the road, flower boxes and barrels at the shops' doors. All of it stands clear of the doors' lanes (x = a door's x), the cobbled lane and the
            // villagers' streets.
            AddProp("ClockTower", "prop_clock_tower", MapLayout.ClockTower.x, MapLayout.ClockTower.y, 2);
            AddProp("Fountain", "prop_fountain", MapLayout.Fountain.x, MapLayout.Fountain.y, 2);
            AddProp("Flag_West", "prop_flag", 23, 20);
            AddProp("Flag_East", "prop_flag", 27, 19);
            foreach (var lampX in new[] { 3, 11, 20, 34, 40, 46 }) AddProp($"Lamp_N{lampX}", "prop_lamp", lampX, 19);
            foreach (var lampX in new[] { 4, 12, 20, 30, 38, 46 }) AddProp($"Lamp_S{lampX}", "prop_lamp", lampX, 15);
            MakeSeat(AddProp("Bench_1", "prop_bench", 14, 14, 2), "prop_bench", "Bench_1");
            MakeSeat(AddProp("Bench_2", "prop_bench", 40, 14, 2), "prop_bench", "Bench_2");
            foreach (var b in Buildings)
                if (b.FacesSouth && !string.IsNullOrEmpty(b.Business))
                {
                    AddProp($"{b.MapId}_FlowersWest", "prop_flowerbox_1", b.DoorX - 1, 20);
                    AddProp($"{b.MapId}_FlowersEast", "prop_flowerbox_2", b.DoorX + 1, 20);
                }
            AddProp("Barrels_Store", "prop_barrels", 2, 22);
            AddProp("BarrelStack_Smith", "prop_barrel_stack", 23, 22);
            AddProp("Boxes_Library", "prop_boxes", 47, 23);
            AddProp("Wheelbarrow_Library", "prop_wheelbarrow", 48, 22);

            // A pond on the green between the saloon and the lane.
            PaintPond(rig, MapLayout.VillagePond.Contains, 15, 23, 4, 13);

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Village}.unity");
        }

        // The edge of an outdoor map (playtest 2026-10-09): hidden collision two cells thick with trees (or rocks on the beach) standing on it, the ways out left open.
        static void PaintEdgeBand(Rig rig, string mapId, int w, int h, System.Func<int, int, bool> open, bool rocks = false)
        {
            var hidden = GetHiddenWall();
            var group = new GameObject("EdgeBand").transform;
            foreach (var c in WoodLayout.BandCells(w, h, open))
            {
                rig.Walls.SetTile(new Vector3Int(c.x, c.y, 0), hidden);
                if (!WoodLayout.BandShowsTree(c.x, c.y)) continue;
                var at = Center(c.x, c.y) + (Vector3)WoodLayout.Jitter(c.x, c.y);
                var piece = AddObject($"Edge_{c.x}_{c.y}", rocks ? ((c.x + c.y) % 4 == 0 ? "obj_boulder" : "obj_rock") : "obj_tree", at, solid: false);
                piece.transform.SetParent(group, true);
            }
        }

        // Paints a pond: every cell of the shape in the box becomes the water tile for its neighbours (so the shore reads as a shore), and walled.
        static void PaintPond(Rig rig, System.Func<int, int, bool> isWater, int x0, int x1, int y0, int y1)
        {
            for (var y = y0; y <= y1; y++)
                for (var x = x0; x <= x1; x++)
                {
                    if (!isWater(x, y)) continue;
                    var water = GetTile(WaterShore.TileNameAt(isWater, x, y));
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), water);
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), water);
                }
        }

        // Is the cell on (or one cell from) a shop or a cottage?
        static bool OnAnyBuilding(int x, int y)
        {
            foreach (var b in VillageBuildings())
                if (x >= b.X0 - 1 && x <= b.X1 + 1 && y >= b.Y0 - 1 && y <= b.Y1 + 1) return true;
            return false;
        }

        // The cells of the lane from a door to the road.
        static bool InConnector(Building b, int y) =>
            b.FacesSouth ? y > RoadY1 && y < b.Y0 : y < RoadY0 && y > b.Y1;

        // Where the door is in each building's picture (pixels from its left edge), so that the picture can stand with its door on the door cell.
        static readonly Dictionary<string, int> DoorPixels = new Dictionary<string, int>
        {
            { "general", 39 }, { "blacksmith", 33 }, { "carpenter", 48 }, { "library", 57 }, { "saloon", 45 }, { "clinic", 50 }, { "hall", 47 },
            { "cottage1", 26 }, { "cottage2", 32 }, { "cottage3", 24 }, { "cottage4", 33 }, { "farmhouse", 48 },
        };

        // A building from the Cozy Village kit stands on its footprint: its picture (drawn over a player who stands behind it) with the door under the door cell, and
        // collision on the cells the picture covers, with an opening at the door. The old flat wall art is no longer drawn.
        static void PlaceBuilding(Rig rig, Building b)
        {
            var doorY = b.FacesSouth ? b.Y0 : b.Y1;
            var centreX = StandBuilding(rig, b.Style, $"{b.MapId}_Building", b.DoorX, doorY, b.Y0, b.Y1);
            if (b.Style == "blacksmith" || b.Style == "saloon") AddChimneySmoke(b, centreX, doorY);

            // A tag beside the door: green when the business is open, red when it is closed.
            if (!string.IsNullOrEmpty(b.Business))
            {
                var tag = AddDecorObject($"{b.MapId}_OpenTag", "bld_tag_open", Center(b.DoorX + 1, doorY));
                tag.GetComponent<SpriteRenderer>().sortingOrder = TallSortingOrder + 1;
                tag.AddComponent<BoxCollider2D>().isTrigger = true;
                tag.GetComponent<BoxCollider2D>().size = Vector2.one;
                tag.GetComponent<BoxCollider2D>().offset = Vector2.zero;
                tag.AddComponent<BusinessStatusSign>().Configure(b.Business, Sprite("bld_tag_open"), Sprite("bld_tag_closed"));
            }

            var outsideY = b.FacesSouth ? doorY - 1 : doorY + 1;
            var planterY = b.FacesSouth ? outsideY - 1 : outsideY + 1;           // one row further out than the door's own row, which people walk along
            if (!string.IsNullOrEmpty(b.Business))          // the shops: a planter a little way off each side of the door (the cells beside the door stay open for walking; the cottages' alleys stay clear)
            {
                AddObject($"{b.MapId}_PlanterWest", "obj_plant", Center(b.DoorX - 2, planterY), solid: true);
                AddObject($"{b.MapId}_PlanterEast", "obj_plant", Center(b.DoorX + 2, planterY), solid: true);
            }
            AddSpawn("from" + b.MapId, Center(b.DoorX, outsideY));
            AddWarp(Center(b.DoorX, doorY), b.MapId, "default", Vector2.one, business: b.Business, condition: b.Condition, blockedKey: b.BlockedKey);
        }

        // Stands a building's picture (prop_bld_<style>) with its door on the cell (doorX, doorY): hidden collision on the rows y0..y1 under the picture except at the
        // door, the door tile on the ground, the picture itself drawn over a player who stands behind it. Returns the picture's centre x, in cells.
        static float StandBuilding(Rig rig, string style, string name, int doorX, int doorY, int y0, int y1)
        {
            var spriteName = "prop_bld_" + style;
            var sprite = Sprite(spriteName);
            var width = sprite != null ? sprite.rect.width : 96f;
            var doorPx = DoorPixels.TryGetValue(style, out var px) ? px : (int)(width / 2f);
            var left = doorX + 0.5f - doorPx / 16f;                                 // the picture's left edge, in cells
            var right = left + width / 16f;
            var firstCell = Mathf.FloorToInt(left + 0.5f);
            var lastCell = Mathf.CeilToInt(right - 0.5f) - 1;

            var hidden = GetHiddenWall();
            for (var y = y0; y <= y1; y++)
                for (var x = firstCell; x <= lastCell; x++)
                {
                    if (x == doorX && y == doorY) continue;
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), hidden);
                }
            rig.Ground.SetTile(new Vector3Int(doorX, doorY, 0), GetTile("tile_door"));

            // The picture: its foot at the foot of the door row (the sprite's pivot is half a cell above its foot), centred so that the door falls on the door cell.
            var centreX = left + width / 32f;
            var picture = AddObject(name, spriteName, new Vector3(centreX, doorY + 0.5f, 0f), solid: false);
            picture.GetComponent<SpriteRenderer>().sortingOrder = TallSortingOrder;
            if (sprite != null)
            {
                var home = style.StartsWith("cottage");
                HoverNote.Add(picture, home ? "hover.home" : "hover.building." + style, home ? HomeOwnerNameKey(name) : null,
                    new Vector2(sprite.rect.width / 16f, sprite.rect.height / 16f * 0.75f), new Vector2(0f, sprite.rect.height / 32f * 0.75f - 0.5f));
            }
            return centreX;
        }

        // The villager's name for a cottage's label: the map is "Home<Name>", the building object "<Map>_Building".
        static string HomeOwnerNameKey(string objectName)
        {
            var map = objectName.EndsWith("_Building") ? objectName.Substring(0, objectName.Length - "_Building".Length) : objectName;
            var home = NpcHomes.ForMap(map);
            return home != null ? "npc." + home.Npc + ".name" : null;
        }

        // Which way each village building's door faces (true: the south wall, like every interior's door): a test keeps them all the same.
        public static List<(string map, bool facesSouth)> BuildingFacings() => VillageBuildings().Select(b => (b.MapId, b.FacesSouth)).ToList();

        // A chimney that smokes, near the top of the building's picture.
        static void AddChimneySmoke(Building b, float centreX, int doorY)
        {
            var go = new GameObject($"{b.MapId}_Smoke");
            go.transform.position = new Vector3(centreX + 1.2f, doorY + 5.2f, 0f);
            go.AddComponent<RoofSmoke>();
        }

        // An empty tile with a one-cell collider: what stands under a building's picture.
        static Tile GetHiddenWall()
        {
            var path = $"{TileDir}/tile_wall_hidden.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null) return tile;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "tile_wall_hidden";
            tile.sprite = null;
            tile.colliderType = Tile.ColliderType.Grid;
            AssetDatabase.CreateAsset(tile, path);
            return tile;
        }

        // A picture on a building's wall: drawn over the wall tiles, not solid.
        static void AddDecor(string name, string spriteName, Vector3 position) => AddDecorObject(name, spriteName, position);

        static GameObject AddDecorObject(string name, string spriteName, Vector3 position)
        {
            var go = AddObject(name, spriteName, position, solid: false);
            go.GetComponent<SpriteRenderer>().sortingOrder = 4;
            return go;
        }

        // ---- Forest -------------------------------------------------------------------------------------------

        const int ForestW = 40, ForestH = 30, ForestPathX0 = 18, ForestPathX1 = 20;

        static void BuildForest()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Forest, indoor: false, allowFarming: false);

            for (var y = 0; y < ForestH; y++)
                for (var x = 0; x < ForestW; x++)
                {
                    var path = x >= ForestPathX0 && x <= ForestPathX1;
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(path ? "tile_path" : "tile_forest"));
                }
            PaintEdgeBand(rig, MapIds.Forest, ForestW, ForestH, (bx, by) => bx >= ForestPathX0 && bx <= ForestPathX1 && (by == 0 || by == ForestH - 1));

            AddSpawn("default", Center(19, 2));
            AddSpawn("fromVillage", Center(19, 2));
            AddSpawn("fromWoods", Center(19, ForestH - 4));
            AddSpawn("sleepwalk", Center(19, 10));
            AddWarp(Center(19, 0), MapIds.Village, "fromForest", new Vector2(3f, 1f));

            // Trees in thickets and clearings (WoodLayout), leaving the path, the pond and the way to the cave clear.
            for (var y = 3; y < ForestH - WoodLayout.EdgeThickness; y++)
                for (var x = WoodLayout.EdgeThickness; x < ForestW - WoodLayout.EdgeThickness; x++)
                {
                    if (x >= ForestPathX0 - 2 && x <= ForestPathX1 + 2) continue;
                    if (x >= 2 && x <= 16 && y >= 3 && y <= 15) continue;             // the pond, its shore and the way to it from the path
                    if (x >= 21 && x <= 36 && y >= 3 && y <= 7) continue;             // the way to the cave
                    if (!WoodLayout.ForestTreeAt(x, y)) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y) + (Vector3)WoodLayout.Jitter(x, y), solid: true);
                }

            // The cave: a path east from the main path to the mouth of the mine.
            for (var x = ForestPathX1 + 1; x <= 34; x++)
                for (var y = 4; y <= 6; y++) rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_path"));
            AddSpawn("fromMine", Center(33, 5));
            AddWarp(Center(35, 5), MapIds.Mine, "default", new Vector2(1f, 3f));
            AddObject("CaveMouth", "obj_boulder", Center(36, 5), solid: true);

            // A pond to fish in: one oval body of water with a shoreline (the tile for each cell depends on which neighbours are land),
            // walled so the player stops at the shore.
            bool Pond(int px, int py) => MapLayout.ForestPond.Contains(px, py);
            for (var y = 6; y <= 14; y++)
                for (var x = 2; x <= 11; x++)
                {
                    if (!Pond(x, y)) continue;
                    var water = GetTile(WaterShore.TileNameAt(Pond, x, y));
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), water);
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), water);
                }

            // The gate at the top of the path: brambles block it while the flag `woods.open` is off. An optional
            // layer that ships the Woods map turns the flag on (the brambles vanish and the warp works).
            var gate = new GameObject("WoodsGate");
            var gateObject = gate.AddComponent<ConditionalObject>();
            gateObject.Condition = "flag:" + MapIds.WoodsOpenFlag;
            gateObject.Invert = true;
            for (var x = ForestPathX0; x <= ForestPathX1; x++)
            {
                var bramble = AddObject($"Bramble_{x}", "obj_bramble", Center(x, ForestH - 2), solid: true);
                bramble.transform.SetParent(gate.transform, true);
            }
            AddWarp(Center(19, ForestH - 3), MapIds.Woods, "default", new Vector2(3f, 1f),
                condition: "flag:" + MapIds.WoodsOpenFlag, blockedKey: "forest.path_blocked");

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Forest}.unity");
        }

        // ---- Harrow Wood (the horror layer's map; shipped with the game, reachable only through the gate flag) ---------

        const int WoodsW = 40, WoodsH = 30;

        static void BuildWoods()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Woods, indoor: false, allowFarming: false);

            for (var y = 0; y < WoodsH; y++)
                for (var x = 0; x < WoodsW; x++)
                {
                    var path = x >= 18 && x <= 20 && y < 14 || x >= 14 && x <= 24 && y >= 14 && y <= 23;   // the track and the clearing
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(path ? "tile_dirt" : "tile_forest"));
                    var edge = x == 0 || y == 0 || x == WoodsW - 1 || y == WoodsH - 1;
                    var gap = x >= 18 && x <= 20 && y == 0;
                    if (edge && !gap) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(19, 2));
            AddSpawn("sleepwalk", Center(19, 16));
            AddWarp(Center(19, 0), MapIds.Forest, "fromWoods", new Vector2(3f, 1f));

            // The three relics that can seal the god away, hidden in the wood (no tree is grown on top of one).
            var relicCells = new[] { (6, 20), (33, 12), (30, 26) };

            // Dense trees everywhere except the track and the clearing.
            for (var y = 3; y < WoodsH - 1; y++)
                for (var x = 1; x < WoodsW - 1; x++)
                {
                    if (x >= 17 && x <= 21 && y < 14) continue;
                    if (x >= 13 && x <= 25 && y >= 13 && y <= 24) continue;
                    if ((x * 5 + y * 11) % 4 != 0) continue;
                    if (System.Array.IndexOf(relicCells, (x, y)) >= 0) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);
                }

            // The altar and the carved stones around the clearing.
            AddObject("Altar", "obj_altar", Center(19, 18), solid: true).AddComponent<Farm.Mythos.AltarObject>();
            var stones = new[] { (14, 15), (24, 22), (11, 9) };
            for (var i = 0; i < stones.Length; i++)
            {
                var stone = AddObject($"Stone_{i + 1}", "obj_stone", Center(stones[i].Item1, stones[i].Item2), solid: true).AddComponent<Farm.Mythos.LoreStone>();
                stone.Key = $"mythos.stone.{(i % 2) + 1}";
                stone.FlagId = $"mythos.stone.read.{i + 1}";
            }
            for (var i = 0; i < Farm.Mythos.MythosData.Relics.Length; i++)
            {
                var relic = AddObject($"Relic_{i + 1}", "obj_relic", Center(relicCells[i].Item1, relicCells[i].Item2), solid: true).AddComponent<Farm.Mythos.RelicPickup>();
                relic.ItemId = Farm.Mythos.MythosData.Relics[i];
                relic.FlagId = $"mythos.relic.found.{i + 1}";
            }

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Woods}.unity");
        }

        // ---- Mine ---------------------------------------------------------------------------------------------

        static void BuildMine()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Mine, indoor: true, allowFarming: false);
            for (var y = 0; y < MineGenerator.Height; y++)
                for (var x = 0; x < MineGenerator.Width; x++)
                {
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_floor_wood"));
                    var edge = x == 0 || y == 0 || x == MineGenerator.Width - 1 || y == MineGenerator.Height - 1;
                    if (edge) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }
            AddSpawn("default", Center(3, MineGenerator.Height / 2));
            new GameObject("MineController").AddComponent<MineController>();
            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Mine}.unity");
        }

        // ---- Beach --------------------------------------------------------------------------------------------

        const int BeachW = 36, BeachH = 24, BeachWaterRows = 5, BeachExitX0 = 16, BeachExitX1 = 18;

        // The shore is not a ruler line: the water reaches a little further up the sand in places (never past row 6, so the fishing rock, the stall and the spawns stay on land).
        static bool IsBeachWater(int x, int y)
        {
            var wave = Mathf.Clamp(Mathf.RoundToInt(1.2f * Mathf.Sin(x * 0.4f) + 0.8f * Mathf.Sin(x * 1.1f + 1f)), -1, 2);
            return y < BeachWaterRows + wave;
        }

        static void BuildBeach()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Beach, indoor: false, allowFarming: false);

            for (var y = 0; y < BeachH; y++)
                for (var x = 0; x < BeachW; x++)
                {
                    var water = IsBeachWater(x, y);
                    var sand = y < BeachH - 6;
                    var tileName = water ? WaterShore.TileNameAt(IsBeachWater, x, y) : sand ? "tile_sand" : "tile_grass";   // the sea runs on past the map's sides
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(tileName));
                    if (water) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile(tileName));
                }
            // The sides and the top are rocks (the sea is below); the way out is at the top.
            PaintEdgeBand(rig, MapIds.Beach, BeachW, BeachH, (bx, by) => by == BeachH - 1 && bx >= BeachExitX0 && bx <= BeachExitX1 || by == 0 || IsBeachWater(bx, by), rocks: true);

            AddSpawn("default", Center(17, BeachH - 3));
            AddSpawn("fromVillage", Center(17, BeachH - 3));
            AddSpawn("sleepwalk", Center(13, 7));
            AddWarp(Center(17, BeachH - 1), MapIds.Village, "fromBeach", new Vector2(3f, 1f));

            // The fish stall: a working counter whose hours (06:00-14:00, closed Thursday) are enforced.
            var stall = AddObject("FishStall", "prop_stall_blue", Center(10, 11), solid: true);
            stall.AddComponent<ShopCounter>().ShopId = "fish";

            // The good rock Felix keeps warm for the player.
            var rock = AddObject("FishingRock", "obj_fishing_rock", Center(13, 9), solid: true).AddComponent<SeatSpot>();
            rock.NpcId = "felix";
            rock.SeatKey = "rock";

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Beach}.unity");
        }

        // ---- interiors ----------------------------------------------------------------------------------------

        static void BuildShopInterior(string mapId, Prop[] props)
        {
            var shop = VillageShops.For(mapId);
            BuildInterior(mapId, shop.InteriorW, shop.InteriorH, shop.InteriorDoorX, props);
        }

        static void BuildInterior(string mapId, int w, int h, int doorX, Prop[] props)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(mapId, indoor: true, allowFarming: false);

            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_floor_wood"));
                    var edge = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                    if (edge && !(y == 0 && x == doorX)) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(doorX, 2));
            AddWarp(Center(doorX, 0), MapIds.Village, "from" + mapId);

            foreach (var p in props)
            {
                var go = AddObject(p.Name, p.Sprite, Center(p.X, p.Y) + new Vector3((p.W - 1) * 0.5f, (p.H - 1) * 0.5f, 0f), solid: true, size: new Vector2(p.W, p.H));
                if (p.ShopId != null) go.AddComponent<ShopCounter>().ShopId = p.ShopId;
                if (p.UpgradesAt != null) go.AddComponent<UpgradeCounter>().ShopId = p.UpgradesAt;
                if (p.Name == "Board" && mapId == MapIds.CommunityHall) go.AddComponent<HallBoard>();
                if (p.Name == "Desk" && mapId == MapIds.Library) go.AddComponent<LibraryDesk>();
                if (p.Curio != null) go.AddComponent<Curio>().Key = p.Curio;
                if (p.SeatKey != null)
                {
                    var seat = go.AddComponent<SeatSpot>();
                    seat.NpcId = p.SeatNpc;
                    seat.SeatKey = p.SeatKey;
                }
                MakeSeat(go, p.Sprite, p.Name);                                // after the villagers' own seats, so that their words come first
            }

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{mapId}.unity");
        }

        // ---- shared rig ---------------------------------------------------------------------------------------

        struct Rig
        {
            public Tilemap Ground, Walls;
            public FarmMap Map;
        }

        static Rig CreateMapRig(string mapId, bool indoor, bool allowFarming)
        {
            var gridGo = new GameObject("Grid", typeof(Grid));
            var ground = TilemapLayer(gridGo, "Ground", 0, collider: false);
            var soil = TilemapLayer(gridGo, "Soil", 1, collider: false);
            var crops = TilemapLayer(gridGo, "Crops", 2, collider: false);
            var walls = TilemapLayer(gridGo, "Walls", 3, collider: true);
            var nodes = TilemapLayer(gridGo, "Nodes", 2, collider: true);   // trees, rocks, weeds (drawn at runtime)

            var mapGo = new GameObject("Map");
            var map = mapGo.AddComponent<FarmMap>();
            map.Configure(mapId, ground, soil, crops, allowFarming, nodes, walls);
            map.ClutterDensity = mapId == MapIds.Farm ? 0.07f : 0f;

            var view = mapGo.AddComponent<FarmMapView>();
            view.Configure(map, Sprite("tile_tilled"), Sprite("tile_tilled_watered"));

            // Player
            var player = new GameObject("Player") { tag = "Player" };
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite("player_idle_down");
            sr.sortingOrder = 10;
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            var col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 0.3f);
            col.offset = new Vector2(0f, 0.15f);
            var controller = player.AddComponent<PlayerController>();
            controller.Configure(sr, Sprite("player_idle_down"), Sprite("player_idle_up"),
                Sprite("player_idle_left"), Sprite("player_idle_right"));

            var cursor = new GameObject("TargetCursor");
            var csr = cursor.AddComponent<SpriteRenderer>();
            csr.sprite = Sprite("ui_cursor");
            csr.sortingOrder = 5;

            var actions = player.AddComponent<PlayerActions>();
            actions.Configure(controller, map, view, cursor.transform);

            // Camera
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = SceneSetup.RefHeight / 2f / TextureImportPostprocessor.PixelsPerUnit;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            var follow = camGo.AddComponent<CameraFollow>();
            camGo.AddComponent<PixelSnapCamera>();
            var ppc = camGo.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = TextureImportPostprocessor.PixelsPerUnit;
            ppc.refResolutionX = SceneSetup.RefWidth;
            ppc.refResolutionY = SceneSetup.RefHeight;
            ppc.upscaleRT = false;
            ppc.pixelSnapping = true;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;

            // Lighting
            var lightGo = new GameObject("Global Light");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            lightGo.AddComponent<DayNightLighting>().Configure(light, indoor);

            // Scene controller
            var controllerGo = new GameObject("MapSceneController");
            controllerGo.AddComponent<MapSceneController>().Configure(map, view, controller, follow);

            return new Rig { Ground = ground, Walls = walls, Map = map };
        }

        static Tilemap TilemapLayer(GameObject grid, string name, int order, bool collider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = order;
            if (collider)
            {
                // One merged outline instead of a square per tile: a player pushed into a wall while walking along it caught on the seams between
                // the squares (bug report "Stuck?").
                go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                go.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
                go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
            }
            return tm;
        }

        static Vector3 Center(int x, int y) => new Vector3(x + 0.5f, y + 0.5f, 0f);

        static void AddSpawn(string id, Vector3 position)
        {
            var go = new GameObject($"Spawn_{id}");
            go.transform.position = position;
            go.AddComponent<SpawnPoint>().Id = id;
        }

        static void AddWarp(Vector3 position, string targetMap, string targetSpawn, Vector2? size = null,
            string business = null, string condition = null, string blockedKey = null)
        {
            var go = new GameObject($"Warp_{targetMap}");
            go.transform.position = position;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size ?? Vector2.one;
            var warp = go.AddComponent<Warp>();
            warp.TargetMap = targetMap;
            warp.TargetSpawn = targetSpawn;
            warp.BusinessId = business;
            warp.Condition = condition;
            warp.BlockedMessageKey = blockedKey;
        }

        static GameObject AddObject(string name, string spriteName, Vector3 position, bool solid, Vector2? size = null)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite(spriteName);
            sr.sortingOrder = 4;
            if (solid)
            {
                var box = go.AddComponent<BoxCollider2D>();          // adding it fits the collider to the picture; the cell is what blocks, so it is put back on the cell
                box.offset = Vector2.zero;
                box.size = size ?? Vector2.one;
            }
            if (spriteName == "obj_tree") sr.sortingOrder = TallSortingOrder;           // the crown covers a player who stands behind the tree (the player is at 10)
            if (spriteName == "obj_tree" || spriteName == "obj_bramble") go.AddComponent<ObjectSway>();   // leans in the wind about its base; the collider stays put
            return go;
        }

        const int TallSortingOrder = 11;

        // A chair, armchair, couch or bench can be sat on (by the player with Interact, and by a villager who stands still beside it).
        static void MakeSeat(GameObject go, string spriteName, string name)
        {
            if (spriteName != "obj_chair" && spriteName != "obj_armchair" && spriteName != "obj_couch" && spriteName != "obj_bench" && spriteName != "prop_bench") return;
            var sit = go.AddComponent<SitSpot>();
            sit.Facing = name.Contains("West") ? Vector2Int.right : name.Contains("East") ? Vector2Int.left : Vector2Int.down;      // a chair beside a table faces the table
        }

        // A prop from the Cozy Village kit (prop_<name>, its foot at the foot of the cell): stands on cell (x, y); a prop two cells wide stands on (x, y) and (x + 1, y).
        // Tall props are drawn over a player standing behind them.
        static GameObject AddProp(string name, string spriteName, int x, int y, int width = 1, bool solid = true)
        {
            var position = Center(x, y) + (width > 1 ? new Vector3((width - 1) * 0.5f, 0f, 0f) : Vector3.zero);
            var go = AddObject(name, spriteName, position, solid: solid, size: new Vector2(width, 1f));
            go.GetComponent<SpriteRenderer>().sortingOrder = TallSortingOrder;
            if (!solid) go.AddComponent<BoxCollider2D>().isTrigger = true;
            HoverNote.Add(go, "hover.prop." + spriteName.Substring("prop_".Length));
            return go;
        }

        // A prop of the farm that is the player's own: the mallet can lift it and put it down elsewhere, like the shipping bin.
        static void MakeMovable(GameObject prop, string id)
        {
            var fixture = prop.AddComponent<MovableFixture>();
            fixture.Id = id;
            fixture.Size = Vector2Int.one;
        }

        static Sprite Sprite(string name)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");
            if (s == null) Debug.LogWarning($"[MapBuilder] Missing placeholder sprite '{name}'. Run 'Farm/Generate Placeholder Art'.");
            return s;
        }

        // Tile assets must be real assets so scenes can reference them; the tile's name is the sprite name,
        // which FarmMap uses to decide what can be tilled.
        static Tile GetTile(string spriteName)
        {
            var path = $"{TileDir}/{spriteName}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            var sprite = Sprite(spriteName);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = spriteName;
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.Sprite;
                AssetDatabase.CreateAsset(tile, path);
            }
            else if (tile.sprite != sprite)
            {
                tile.sprite = sprite;
                EditorUtility.SetDirty(tile);
            }
            return tile;
        }
    }
}
