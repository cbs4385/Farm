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
        const int HouseX0 = FarmBuildings.HouseX0, HouseX1 = FarmBuildings.HouseX1, HouseY0 = FarmBuildings.HouseY0, HouseY1 = FarmBuildings.HouseY1, DoorX = MapLayout.FarmHouseDoorX;
        // The greenhouse, coop and barn are not baked into the scene: FarmBuildingsView draws them from the saved game state (the player can
        // move them), and their doors are locked until the carpenter has built them (FarmBuildings has the types and the default places).
        const int FarmExitY0 = MapLayout.FarmRoadY - 1, FarmExitY1 = MapLayout.FarmRoadY + 1;

        // FarmHouse interior: 22 x 9 cells, three rooms (playtest 2026-10-14: "the house could use more rooms for the decorations"): the main room (x 1..10), the kitchen
        // (x 12..20, y 1..3) east of it through the doorway at x 11, y 2, and the bedroom (x 12..20, y 5..7) north of the kitchen through the doorway at x 16, y 4.
        const int InW = 22, InH = 9, KitchenWallX = 11, KitchenDoorY = 2, BedroomWallY = 4, BedroomDoorX = 16;

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
                new Prop("Bar1", "obj_counter_wide", 3, 7, "saloon", w: 2), new Prop("Bar2", "obj_counter_wide", 5, 7, "saloon", w: 2), new Prop("Bar3", "obj_counter_wide", 7, 7, w: 2),
                new Prop("Table1", "obj_table", 2, 3), new Prop("Table2", "obj_table", 11, 3), new Prop("Table3", "obj_table", 11, 5),
                new Prop("Table4", "obj_table", 2, 5), new Prop("Shelf1", "obj_shelf", 4, 8), new Prop("Shelf2", "obj_shelf", 6, 8),
                new Prop("Seat", "obj_chair", 12, 3, seatNpc: "wren", seatKey: "stool"),
                new Prop("WindowLantern", "item_prop_lantern", 10, 8, curio: "lantern"),
            });
            BuildShopInterior(MapIds.Clinic, new[]
            {
                new Prop("Bed1", "obj_bed", 2, 5), new Prop("Bed2", "obj_bed", 4, 5), new Prop("Bed3", "obj_bed", 6, 5),
                new Prop("Couch", "obj_couch", 8, 6, seatNpc: "elara", seatKey: "couch"),
                new Prop("Desk1", "obj_counter_wide", 7, 3, upgradesAt: "clinic", w: 2), new Prop("Shelf", "obj_shelf", 1, 4),
            });
            BuildShopInterior(MapIds.CommunityHall, new[]
            {
                new Prop("Board", "obj_board", 7, 7), new Prop("Table1", "obj_table", 3, 4), new Prop("Table2", "obj_table", 10, 4),
                new Prop("Shelf1", "obj_bookshelf_tall", 2, 8), new Prop("Shelf2", "obj_bookshelf_tall", 11, 8),
            });
            BuildShopInterior(MapIds.Library, new[]
            {
                new Prop("Shelf1", "obj_bookshelf_tall", 2, 7), new Prop("Shelf2", "obj_bookshelf_tall", 3, 7), new Prop("Shelf3", "obj_bookshelf_tall", 4, 7),
                new Prop("Shelf4", "obj_bookshelf_tall", 7, 7), new Prop("Shelf5", "obj_bookshelf_tall", 8, 7), new Prop("Shelf6", "obj_bookshelf_tall", 9, 7),
                new Prop("Shelf7", "obj_bookshelf_tall", 2, 3), new Prop("Shelf8", "obj_bookshelf_tall", 3, 3), new Prop("Shelf9", "obj_bookshelf_tall", 8, 3),
                new Prop("Shelf10", "obj_bookshelf_tall", 9, 3), new Prop("Desk", "obj_counter_wide", 5, 5, w: 2), new Prop("Reading", "obj_table", 10, 2),
                new Prop("Seat", "obj_chair", 1, 2, seatNpc: "ione", seatKey: "chair"),
                new Prop("NookSeat", "obj_armchair", 10, 5, seatNpc: "hazel", seatKey: "nook"),
                new Prop("BackShelf", "obj_bookshelf_tall", 10, 7, curio: "backshelf"),
                new Prop("HazelShelf", "obj_bookshelf_tall", 1, 7, curio: "hazelshelf"),
            });
        }

        // ---- the Community Hall's progress ---------------------------------------------------------------------------
        // Restoring a room of the hall (its quest done: the flag `hall.<room>`) used to change nothing that could be seen (playtest 2026-10-13: "no change to the village or the
        // community center after completing a quest"). Now each of the six rooms shows in the hall itself and in the village. Everything here is hidden until its flag is set.

        static readonly string[] HallRoomIds = { "pantry", "crafts", "fishtank", "boiler", "bulletin", "vault" };

        static GameObject WhenRestored(string prefix, string room)
        {
            var holder = new GameObject(prefix + "_" + room);
            holder.AddComponent<ConditionalObject>().Condition = "flag:hall." + room;
            return holder;
        }

        static GameObject RestoredHolder(string prefix, string room) => GameObject.Find(prefix + "_" + room) ?? WhenRestored(prefix, room);

        static void AddHallRestorations()
        {
            GameObject Put(string room, string name, string sprite, int x, int y, bool solid = true, int w = 1, int h = 1)
            {
                var go = AddObject(name, sprite, Center(x, y) + new Vector3((w - 1) * 0.5f, (h - 1) * 0.5f, 0f), solid, size: new Vector2(w, h));
                go.transform.SetParent(RestoredHolder("HallRestored", room).transform, true);
                return go;
            }
            Put("pantry", "Barrels", "obj_barrels", 1, 6);
            Put("pantry", "Sacks", "obj_sacks", 1, 5);
            Put("crafts", "RugWest", "obj_rug", 6, 3, solid: false);
            Put("crafts", "RugEast", "obj_rug", 8, 3, solid: false);
            Put("fishtank", "Tank", "obj_jar", 12, 5);
            Put("fishtank", "Plant", "obj_plant", 12, 6);
            var hearth = Put("boiler", "Hearth", "obj_fireplace", 4, 7, w: 2, h: 2);
            var glow = new GameObject("HearthGlow");
            glow.transform.SetParent(hearth.transform.parent, true);
            glow.transform.position = hearth.transform.position + new Vector3(0f, -0.3f, 0f);
            var flame = glow.AddComponent<FlickerLight>();
            flame.Radius = 5f;
            flame.Intensity = 1f;
            flame.Color = new Color(1f, 0.62f, 0.3f);
            Put("bulletin", "Painting", "obj_painting", 9, 8);
            Put("bulletin", "Lamp", "obj_lamp", 12, 2);
            Put("vault", "Clock", "obj_clock", 10, 8);
            Put("vault", "Vase", "obj_vase", 1, 2);
        }

        // ---- the festival grounds ----------------------------------------------------------------------------------

        // What stands in the village square, on the road and by the pond on each festival day (playtest 2026-10-13): a holder per festival whose condition is the day, so the
        // props are there from dawn and gone the morning after. The villagers' cells are in Festivals.Gathering; every cell used here was checked against them and the
        // doors' lanes. Each festival has its own station (FestivalStation) to use.
        static void AddFestivalGrounds()
        {
            GameObject Holder(string id)
            {
                var holder = new GameObject("Festival_" + id);
                holder.AddComponent<ConditionalObject>().Condition = Festivals.Get(id).Condition;
                return holder;
            }
            GameObject Put(GameObject holder, string name, string sprite, int x, int y, int width = 1, bool solid = true, int order = TallSortingOrder)
            {
                var go = AddObject(name, sprite, Center(x, y) + (width > 1 ? new Vector3((width - 1) * 0.5f, 0f, 0f) : Vector3.zero), solid, new Vector2(width, 1f));
                go.GetComponent<SpriteRenderer>().sortingOrder = order;
                go.transform.SetParent(holder.transform, true);
                return go;
            }
            FestivalStation Station(GameObject go, FestivalStationKind kind) { var s = go.AddComponent<FestivalStation>(); s.Kind = kind; return s; }
            void Bunting(GameObject holder, string season, int y, params int[] xs) { foreach (var x in xs) Put(holder, $"Bunting_{season}_{x}_{y}", "prop_fest_bunting_" + season, x, y, 2, solid: false, order: 14); }

            // Blossom Day: a dance floor on the road where it crosses the lane, the maypole in the middle, a band stand beside it and bunting along both edges of the road.
            var spring = Holder("spring");
            for (var x = 23; x <= 31; x++)
                for (var y = 16; y <= 18; y++)
                    Put(spring, $"DanceFloor_{x}_{y}", "obj_fest_floor", x, y, solid: false, order: 1);
            Station(Put(spring, "Maypole", "prop_fest_maypole", FestivalGrounds.Maypole.x, FestivalGrounds.Maypole.y), FestivalStationKind.Maypole);
            Put(spring, "BandStand", "prop_fest_bandstand", 33, 17, 2);
            Bunting(spring, "spring", 19, 23, 25, 27, 29, 31);
            Bunting(spring, "spring", 15, 23, 25, 27, 29);

            // The Tidewatch: the scoreboard booth on the shore of the village pond, two boats on the water, a crate of the day's catch and blue bunting.
            var summer = Holder("summer");
            Station(Put(summer, "Scoreboard", "prop_fest_scoreboard", 22, 14, 2), FestivalStationKind.Scoreboard);
            Put(summer, "Boat1", "prop_fest_boat", 17, 10, 2);
            Put(summer, "Boat2", "prop_fest_boat", 21, 7, 2);
            Put(summer, "Crate", "prop_fest_fishcrate", 20, 14);
            Bunting(summer, "summer", 15, 15, 17, 19, 21);

            // The Harvest Fair: stalls on both sides of the square, the judging table by the clock tower, a heap of pumpkins and orange bunting under the shops.
            var fall = Holder("fall");
            Put(fall, "StallWest1", "prop_fest_stall", 12, 22, 2);
            Put(fall, "StallWest2", "prop_fest_stall", 14, 22, 2);
            Put(fall, "StallEast", "prop_fest_stall", 34, 22, 2);
            Station(Put(fall, "JudgingTable", "prop_fest_judging", 29, 22, 2), FestivalStationKind.JudgingTable);
            Put(fall, "Pumpkins", "prop_fest_pumpkins", 36, 21);
            Bunting(fall, "fall", 23, 12, 14, 29, 34);

            // The Lantern Feast: the long table along the north edge of the road, lantern poles, and the rack where the player hangs a lantern.
            var winter = Holder("winter");
            foreach (var x in new[] { 21, 23, 25, 27 })
                Station(Put(winter, $"FeastTable_{x}", "prop_fest_feasttable", x, 18, 2), FestivalStationKind.FeastTable);
            foreach (var (x, y) in new[] { (22, 15), (25, 15), (28, 15), (33, 19) })
            {
                var pole = Put(winter, $"Lantern_{x}_{y}", "prop_fest_lantern_lit", x, y);
                var glow = new GameObject("Glow");
                glow.transform.SetParent(pole.transform, false);
                glow.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                var flame = glow.AddComponent<FlickerLight>();
                flame.Radius = 4f; flame.Intensity = 0.9f; flame.Color = new Color(1f, 0.78f, 0.4f);
            }
            var rack = Put(winter, "LanternRack", "prop_fest_lanternrack", 36, 19);
            var lit = new GameObject("Lit", typeof(SpriteRenderer));
            lit.transform.SetParent(rack.transform, false);
            lit.GetComponent<SpriteRenderer>().sprite = Sprite("prop_fest_lanternrack_lit");
            lit.GetComponent<SpriteRenderer>().sortingOrder = TallSortingOrder + 1;
            lit.SetActive(false);
            var rackStation = Station(rack, FestivalStationKind.LanternRack);
            rackStation.Lit = lit;
            Bunting(winter, "winter", 15, 21, 23, 25, 27);
        }

        // In the village, along the open row in front of the hall (between it and the shops), clear of the lane to its door.
        static void AddVillageRestorations()
        {
            var hall = Buildings.First(b => b.MapId == MapIds.CommunityHall);
            var row = hall.Y0 - 1;
            var dx = hall.DoorX;
            GameObject Put(string room, string name, string sprite, int x, int width = 1, bool lamp = false)
            {
                var go = AddProp(name, sprite, x, row, width);
                go.transform.SetParent(RestoredHolder("VillageRestored", room).transform, true);
                if (lamp) go.AddComponent<StreetLamp>();
                return go;
            }
            Put("pantry", "HallFlowersWest", "prop_flowerbox_1", dx - 3);
            Put("pantry", "HallFlowersFar", "prop_flowerbox_2", dx - 4);
            Put("crafts", "HallFlagWest", "prop_flag", dx - 6);
            Put("crafts", "HallFlagEast", "prop_flag", dx + 6);
            Put("fishtank", "HallRainBarrel", "prop_barrels", dx + 4);
            Put("bulletin", "HallLampWest", "prop_lamp", dx - 5, lamp: true);
            Put("bulletin", "HallLampEast", "prop_lamp", dx + 5, lamp: true);
            Put("vault", "HallBench", "prop_bench", dx + 2, 2);
            AddChimneySmoke(hall, (hall.X0 + hall.X1 + 1) / 2f, hall.Y0);            // the boiler is lit again: smoke from the hall's chimney
            var smoke = GameObject.Find($"{hall.MapId}_Smoke");
            if (smoke != null) smoke.transform.SetParent(RestoredHolder("VillageRestored", "boiler").transform, true);
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
                    var ground = "tile_grass";           // the old lattice of dirt squares read as a pattern, not as a farm gone wild
                    if (FarmRoad.Contains(x, y)) ground = "tile_path";           // from the door south, then east along the road to the village: one cell wide, and it bends (FarmRoad)
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(ground));

                }
            PaintEdgeBand(rig, MapIds.Farm, FarmW, FarmH, (bx, by) => bx == FarmW - 1 && by >= FarmExitY0 && by <= FarmExitY1);
            StandBuilding(rig, "farmhouse", "Farmhouse_Building", DoorX, HouseY0, HouseY0, HouseY1);                      // the kit's farm house over the old footprint
            rig.Ground.SetTile(new Vector3Int(DoorX, HouseY0, 0), GetTile("tile_floor_wood"));
            rig.Map.gameObject.AddComponent<FarmBuildingsView>().Configure(rig.Map, GetTile("tile_wall"), GetTile("tile_roof"), GetTile("tile_door"), GetHiddenWall(), Sprite("prop_bld_coop"), Sprite("prop_bld_barn"), Sprite("prop_bld_greenhouse"));

            AddSpawn("default", Center(DoorX, HouseY0 - 3));
            AddSpawn("fromHouse", Center(DoorX, HouseY0 - 2));
            AddSpawn("fromVillage", Center(MapLayout.FarmArriveX, MapLayout.FarmRoadY));
            AddSpawn("sleepwalk", Center(77, 15));
            AddWarp(Center(DoorX, HouseY0), MapIds.FarmHouse, "default");
            AddWarp(Center(MapLayout.FarmExitX, MapLayout.FarmRoadY), MapIds.Village, "fromFarm", new Vector2(1f, 3f));

            // A tall red box with a yellow flag (16 x 32): the old one was a small blue blob that was easy to miss. It stands on cell (77, 19) and reaches up a cell.
            AddObject("Mailbox", "obj_mailbox_tall", Center(77, 19) + new Vector3(0f, 0.5f, 0f), solid: true, size: new Vector2(1f, 2f)).AddComponent<Mailbox>().gameObject.AddComponent<MailIndicator>().Configure(Sprite("fx_mail"));

            // The shipping bin stays on the farm; the general store now lives in the village.
            var bin = AddObject("ShippingBin", "obj_shipping_bin", Center(73, 17) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));      // a 2 x 2 bin on cells (73..74, 17..18)
            bin.AddComponent<ShippingBin>();
            var binFixture = bin.AddComponent<MovableFixture>();      // the mallet can carry it next to the house or anywhere else on the farm
            binFixture.Id = "shipping_bin";
            binFixture.Size = new Vector2Int(2, 2);

            // By the farmhouse: hay and sacks against the wall and a wheelbarrow, where the farm's own work happens.
            MakeMovable(AddProp("Hay", "prop_hay", 74, 21), "farm_hay", "hay");
            MakeMovable(AddProp("Sacks", "prop_sacks", 73, 21), "farm_sacks", "sacks");
            MakeMovable(AddProp("Wheelbarrow", "prop_wheelbarrow", 84, 22), "farm_wheelbarrow", "wheelbarrow");
            MakeMovable(AddProp("Barrels", "prop_barrels", 84, 23), "farm_barrels", "barrels");

            // A pond to the north-west of the fields (fishing, and something to look at): one oval of water with a shoreline, walled so that one stops at the shore.
            PaintPond(rig, MapLayout.FarmPond.Contains, 21, 37, 33, 45);

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

        // The main room is the living and dining room: the hearth, the sofa and armchair, the dining table, bookshelves, a picture, a clock and plants. The kitchen has the range,
        // a small table, sacks, barrels and a jar shelf; the bedroom the double bed, a wardrobe, a lamp, a picture and a rug. Nothing may close off a doorway, the bed or the range.
        static readonly Furnishing[] HouseFurnishings =
        {
            // main room
            new Furnishing("RugLarge", "obj_rug_large", 3, 3, 2, 2, solid: false),
            new Furnishing("Fireplace", "obj_fireplace", 5, 6, 2, 2),
            new Furnishing("Lamp", "obj_lamp", 3, 7), new Furnishing("Painting", "obj_painting", 4, 7), new Furnishing("Clock", "obj_clock", 7, 7),
            new Furnishing("Bookshelf", "obj_bookshelf", 8, 7), new Furnishing("BookshelfEast", "obj_bookshelf", 10, 5),
            new Furnishing("Couch", "obj_couch", 1, 4, 2, 1), new Furnishing("Armchair", "obj_armchair", 6, 4), new Furnishing("PlantWest", "obj_plant", 1, 1),
            new Furnishing("DiningTable", "obj_dining_table", 8, 3), new Furnishing("ChairWest", "obj_chair", 7, 3), new Furnishing("ChairEast", "obj_chair", 9, 3),
            new Furnishing("Bench", "obj_bench", 8, 1), new Furnishing("PlantEast", "obj_plant", 10, 7), new Furnishing("PlantSouth", "obj_plant", 1, 7),
            // kitchen
            new Furnishing("KitchenTable", "obj_dining_table", 15, 1), new Furnishing("KitchenChairWest", "obj_chair", 14, 1), new Furnishing("KitchenChairEast", "obj_chair", 16, 1),
            new Furnishing("Sacks", "obj_sacks", 12, 3), new Furnishing("Barrels", "obj_barrels", 13, 3), new Furnishing("JarShelf", "obj_jar", 20, 1), new Furnishing("PlantKitchen", "obj_plant", 18, 1),
            // bedroom
            new Furnishing("Wardrobe", "obj_wardrobe", 20, 5), new Furnishing("BedLamp", "obj_lamp", 14, 7), new Furnishing("BedPainting", "obj_painting", 16, 7),
            new Furnishing("Vase", "obj_vase", 18, 7), new Furnishing("RugAnnex", "obj_rug_large", 17, 5, 2, 2, solid: false), new Furnishing("PlantBedroom", "obj_plant", 20, 7),
        };

        static void BuildFarmHouse()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.FarmHouse, indoor: true, allowFarming: false);

            for (var y = 0; y < InH; y++)
                for (var x = 0; x < InW; x++)
                {
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_floor_wood"));
                    var edge = x == 0 || y == 0 || x == InW - 1 || y == InH - 1 || (x == KitchenWallX && y != KitchenDoorY) || (y == BedroomWallY && x > KitchenWallX && x != BedroomDoorX);
                    var door = y == 0 && x == 5;
                    if (edge && !door) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(5, 2));
            AddSpawn("bed", Center(14, 6));
            rig.Ground.SetTile(new Vector3Int(KitchenWallX, KitchenDoorY, 0), GetTile("tile_door"));          // the doors between the rooms, drawn on the floor like the doors of the shops
            rig.Ground.SetTile(new Vector3Int(BedroomDoorX, BedroomWallY, 0), GetTile("tile_door"));
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

            // A double bed, two cells square, in the bedroom's north-west corner (cells 12..13, 6..7). One wakes up on the cell east of it.
            var bed = AddObject("Bed", "obj_bed_double", Center(12, 6) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));
            bed.AddComponent<Bed>();
            var bedFixture = bed.AddComponent<MovableFixture>();
            bedFixture.Id = "bed";
            bedFixture.SpawnId = "bed";
            bedFixture.SpawnOffset = new Vector2Int(2, 0);
            bedFixture.Size = new Vector2Int(2, 2);

            // The kitchen: cook with what is in the backpack.
            var kitchen = AddObject("Kitchen", "obj_kitchen", Center(19, 2) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));          // a range two cells wide and two tall: it reads as a cooker
            kitchen.AddComponent<Kitchen>();
            var kitchenFixture = kitchen.AddComponent<MovableFixture>();
            kitchenFixture.Id = "kitchen";
            kitchenFixture.Size = new Vector2Int(2, 2);

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
            foreach (var lampX in new[] { 3, 11, 20, 34, 40, 46 }) AddProp($"Lamp_N{lampX}", "prop_lamp", lampX, 19).AddComponent<StreetLamp>();
            foreach (var lampX in new[] { 4, 12, 20, 30, 38, 46 }) AddProp($"Lamp_S{lampX}", "prop_lamp", lampX, 15).AddComponent<StreetLamp>();
            MakeSeat(AddProp("Bench_1", "prop_bench", 14, 14, 2), "prop_bench", "Bench_1");
            MakeSeat(AddProp("Bench_2", "prop_bench", 40, 14, 2), "prop_bench", "Bench_2");
            foreach (var b in Buildings)
                if (b.FacesSouth && !string.IsNullOrEmpty(b.Business))
                {
                    AddProp($"{b.MapId}_FlowersWest", "prop_flowerbox_1", b.DoorX - 1, 20, solid: false);          // walked through: people kept catching on them (playtest 2026-10-13)
                    AddProp($"{b.MapId}_FlowersEast", "prop_flowerbox_2", b.DoorX + 1, 20, solid: false);
                }
            AddProp("Barrels_Store", "prop_barrels", 2, 22);
            AddProp("BarrelStack_Smith", "prop_barrel_stack", 23, 22);
            AddProp("Boxes_Library", "prop_boxes", 47, 23);
            AddProp("Wheelbarrow_Library", "prop_wheelbarrow", 48, 22);
            AddVillageRestorations();
            AddFestivalGrounds();

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
            var blocker = GetHiddenWall();          // the water is drawn on the ground; a water tile on the wall layer was drawn over the crown of a tree standing south of the pond
            for (var y = y0; y <= y1; y++)
                for (var x = x0; x <= x1; x++)
                {
                    if (!isWater(x, y)) continue;
                    var water = GetTile(WaterShore.TileNameAt(isWater, x, y));
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), water);
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), blocker);
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
                tag.AddComponent<BuildingDepth>().Configure(doorY + 1f, TallSortingOrder + 1, BuildingDepth.DefaultFront + 1);
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
            var mask = BuildingMasks.Get(style);                                     // where the picture's door is, and which cells it covers
            var doorPx = mask != null ? mask.DoorPx : (int)(width / 2f);
            var left = doorX + 0.5f - doorPx / 16f;                                 // the picture's left edge, in cells
            var right = left + width / 16f;
            var firstCell = Mathf.FloorToInt(left + 0.5f);
            var lastCell = Mathf.CeilToInt(right - 0.5f) - 1;

            var hidden = GetHiddenWall();
            if (mask != null)
                foreach (var c in mask.SolidCells()) rig.Walls.SetTile(new Vector3Int(doorX + c.x, doorY + c.y, 0), hidden);          // only what the picture covers
            else
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
            picture.AddComponent<BuildingDepth>().Configure(doorY + 1f, TallSortingOrder);          // over a player behind it, under one in front or in the doorway
            picture.AddComponent<BuildingGlow>();          // its windows glow from dusk to dawn
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

        const int ForestW = ForestTrees.Width, ForestH = ForestTrees.Height, ForestPathX0 = ForestTrees.PathX0, ForestPathX1 = ForestTrees.PathX1;

        static void BuildForest()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Forest, indoor: false, allowFarming: false);

            for (var y = 0; y < ForestH; y++)
                for (var x = 0; x < ForestW; x++)
                {
                    var path = x >= ForestTrees.Trail[y].x0 && x <= ForestTrees.Trail[y].x1;
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(path ? "tile_path" : "tile_forest"));
                }
            PaintEdgeBand(rig, MapIds.Forest, ForestW, ForestH, (bx, by) => bx >= ForestPathX0 && bx <= ForestPathX1 && (by == 0 || by == ForestH - 1));

            AddSpawn("default", Center(19, 2));
            AddSpawn("fromVillage", Center(19, 2));
            AddSpawn("fromWoods", Center(19, ForestH - 4));
            AddSpawn("sleepwalk", Center(19, 10));
            AddWarp(Center(19, 0), MapIds.Village, "fromForest", new Vector2(3f, 1f));

            // The trees of the thickets are resource nodes, put in when the forest is first visited (ForestTrees): they can be chopped.

            // The cave: a path east from the main path to the mouth of the mine.
            // its middle row is whole; the rows beside it start and stop raggedly
            for (var x = ForestTrees.Trail[5].x0 + 1; x <= 34; x++)
                for (var y = 4; y <= 6; y++)
                    if (y == 5 || x < ForestPathX1 + 2 || x > 32 || WoodLayout.Random01(x, y, 61) < 0.72f) rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_path"));
            AddSpawn("fromMine", Center(33, 5));
            AddWarp(Center(35, 5), MapIds.Mine, "default", new Vector2(1f, 3f));
            // the mine mouth: a timber-framed opening in the rock, two cells wide and two tall (cells 36-37, rows 5-6)
            AddObject("CaveMouth", "prop_mine_entrance", new Vector3(37f, 6f, 0f), solid: true, size: new Vector2(2f, 2f));

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

        static bool WoodsOpenGround(int x, int y) => x >= 17 && x <= 21 && y < 14 || x >= 13 && x <= 25 && y >= 13 && y <= 24;

        public static readonly (int x, int y)[] WoodsStones = { (14, 15), (24, 22), (11, 9) };

        // The cells that hold a tree, placed like the forest's (WoodLayout: thickets and clearings, nothing in rows; playtest 2026-10-14: "the forest is too regular").
        // Slanting lines of trees used to wall things in ("stuck behind a row of trees I cannot walk around or cut down"), so each carved stone stands in a small
        // clearing with a lane cleared from it to the track or the clearing.
        public static List<(int x, int y)> WoodsTrees()
        {
            var kept = new HashSet<(int, int)>();
            foreach (var (sx, sy) in WoodsStones)
            {
                for (var dx = -1; dx <= 1; dx++)
                    for (var dy = -1; dy <= 1; dy++) kept.Add((sx + dx, sy + dy));
                var (x, y) = (sx, sy);
                var step = x < 19 ? 1 : -1;
                while (!WoodsOpenGround(x, y) && x > 0 && x < WoodsW - 1) kept.Add((x += step, y));
            }
            var trees = new List<(int x, int y)>();
            for (var y = 3; y < WoodsH - WoodLayout.EdgeThickness; y++)
                for (var x = WoodLayout.EdgeThickness; x < WoodsW - WoodLayout.EdgeThickness; x++)
                    if (!WoodsOpenGround(x, y) && !kept.Contains((x, y)) && WoodLayout.ForestTreeAt(x, y, WoodsSeed)) trees.Add((x, y));
            return trees;
        }

        const int WoodsSeed = 23;

        // The track to the clearing winds, like the forest's trail (TrailShape); the trees keep off the cells 17-21 beside it.
        static readonly (int x0, int x1)[] WoodsTrack = TrailShape.Build(14, 18, 20, 17, 21, 29, endRows: 3);

        static void BuildWoods()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Woods, indoor: false, allowFarming: false);

            for (var y = 0; y < WoodsH; y++)
                for (var x = 0; x < WoodsW; x++)
                {
                    var path = y < 14 && x >= WoodsTrack[y].x0 && x <= WoodsTrack[y].x1 || x >= 14 && x <= 24 && y >= 14 && y <= 23;   // the track and the clearing
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(path ? "tile_dirt" : "tile_forest"));
                }
            PaintEdgeBand(rig, MapIds.Woods, WoodsW, WoodsH, (bx, by) => bx >= 18 && bx <= 20 && by == 0);

            AddSpawn("default", Center(19, 2));
            AddSpawn("sleepwalk", Center(19, 16));
            AddWarp(Center(19, 0), MapIds.Forest, "fromWoods", new Vector2(3f, 1f));

            // Dense trees everywhere except the track, the clearing and a lane to each stone.
            foreach (var (x, y) in WoodsTrees())
                AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);

            // The altar and the carved stones around the clearing.
            AddObject("Altar", "obj_altar", Center(19, 18), solid: true).AddComponent<Farm.Mythos.AltarObject>();
            var stones = WoodsStones;
            for (var i = 0; i < stones.Length; i++)
            {
                var stone = AddObject($"Stone_{i + 1}", "obj_stone", Center(stones[i].Item1, stones[i].Item2), solid: true).AddComponent<Farm.Mythos.LoreStone>();
                stone.Key = $"mythos.stone.{(i % 2) + 1}";
                stone.FlagId = $"mythos.stone.read.{i + 1}";
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

        // Pictures that are one cell wide and two tall (16 x 32): the single bed and the shelves.
        static readonly HashSet<string> TallPictures = new HashSet<string> { "obj_bed", "obj_shelf", "obj_bookshelf_tall" };

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

            var business = props.Select(q => q.ShopId ?? q.UpgradesAt).FirstOrDefault(id => id != null);
            foreach (var p in props)
            {
                var rows = Mathf.Max(p.H, TallPictures.Contains(p.Sprite) ? 2 : 1);          // a tall picture covers its cell and the one above (the back wall or the next floor cell)
                var go = AddObject(p.Name, p.Sprite, Center(p.X, p.Y) + new Vector3((p.W - 1) * 0.5f, (rows - 1) * 0.5f, 0f), solid: true, size: new Vector2(p.W, rows));
                // Every counter of a business gives all that it offers (its shop and its upgrades): a counter without a business of its own takes the room's.
                if (p.UpgradesAt != null) go.AddComponent<UpgradeCounter>().ShopId = p.UpgradesAt;
                else
                {
                    var counterBusiness = p.ShopId ?? (p.Sprite == "obj_counter_wide" ? business : null);
                    if (counterBusiness != null) go.AddComponent<ShopCounter>().ShopId = counterBusiness;
                }
                if (p.Name == "Board" && mapId == MapIds.CommunityHall) go.AddComponent<HallBoard>();
                if (p.Name == "Desk" && mapId == MapIds.Library) go.AddComponent<LibraryDesk>();
                if (mapId == MapIds.Library && p.Sprite == "obj_bookshelf_tall") go.AddComponent<Bookshelf>().ShelfId = p.Name;
                if (p.Curio != null) go.AddComponent<Curio>().Key = p.Curio;
                if (p.SeatKey != null)
                {
                    var seat = go.AddComponent<SeatSpot>();
                    seat.NpcId = p.SeatNpc;
                    seat.SeatKey = p.SeatKey;
                }
                MakeSeat(go, p.Sprite, p.Name);                                // after the villagers' own seats, so that their words come first
            }

            if (mapId == MapIds.CommunityHall) AddHallRestorations();

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
            map.ClutterDensity = mapId == MapIds.Farm ? 0.12f : 0f;

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
            if (name == "Nodes")
            {
                tr.mode = TilemapRenderer.Mode.Individual;                    // chunk mode drew a tree behind one in front of it (playtest 2026-10-13)
                tr.sortingOrder = WorldSorting.ActorOrder;                    // with the player and the villagers, put in order by height (2026-10-15)
                tr.sortOrder = TilemapRenderer.SortOrder.TopLeft;             // the rows higher up are drawn first, so the lower ones overlap them
            }
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
            var label = LabelKeyFor(spriteName);
            if (label != null) HoverNote.Add(go, label, spriteName == "prop_mine_entrance" ? "hover.mine.caution" : null, solid ? (Vector2?)null : size ?? Vector2.one);          // what a thing is, for a mouse resting on it or a pad facing it
            if (spriteName == "obj_tree") sr.sortingOrder = WorldSorting.ActorOrder;           // sorted by height with the player (same order, WorldSorting.Axis): the crown covers a player who stands behind the tree
            if (spriteName == "obj_tree" || spriteName == "obj_bramble") go.AddComponent<ObjectSway>();   // leans in the wind about its base; the collider stays put
            return go;
        }

        const int TallSortingOrder = 11;

        // The hover name of a picture put on a map (null: a flat floor covering or a decoration nobody points at). Counters, beds, shelves and boards that are used by hand get their own
        // label from their component, which wins over this one (HoverInspector asks the interactable first). HoverLabelCoverageTests walks every map scene for anything left without a name.
        static readonly Dictionary<string, string> ObjectLabels = new Dictionary<string, string>
        {
            { "obj_tree", "node.tree" }, { "obj_rock", "node.rock" }, { "obj_boulder", "node.boulder" }, { "obj_bramble", "hover.obj.bramble" },
            { "obj_bed", "hover.obj.bed" }, { "obj_bed_double", "hover.obj.bed" }, { "obj_counter_wide", "hover.obj.counter" }, { "obj_bookshelf_tall", "hover.furn.bookshelf" },
            { "obj_board", "hover.obj.board" }, { "obj_cat_door", "hover.obj.cat_door" }, { "obj_fishing_rock", "hover.obj.fishing_rock" },
        };

        public static string LabelKeyFor(string spriteName)
        {
            if (spriteName.StartsWith("obj_"))
            {
                var plain = spriteName.Substring(4);
                return PlainFurniture.Contains(plain) ? "hover.furn." + plain : ObjectLabels.TryGetValue(spriteName, out var key) ? key : null;
            }
            if (!spriteName.StartsWith("prop_")) return null;
            var prop = spriteName.Substring("prop_".Length);
            if (prop.StartsWith("bld_")) return null;          // a building's picture is named by StandBuilding ("hover.building.<style>"); a second note here shadowed it with a key that has no text
            return prop.StartsWith("fest_bunting_") ? "hover.prop.fest_bunting" : "hover.prop." + prop;
        }

        // Furniture that is only to look at or sit on: it gets a name for the hover label (the bed, the kitchen, the counters and the board have their own).
        static readonly HashSet<string> PlainFurniture = new HashSet<string>
        {
            "armchair", "barrels", "bin", "bookshelf", "chair", "clock", "couch", "dining_table", "fireplace", "jar", "lamp", "painting", "plant", "rug_large", "sacks", "shelf", "table", "vase", "wardrobe", "bench",
        };

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
            return go;          // AddObject has named it (and given a walk-through prop its trigger area)
        }

        // A prop of the farm that is the player's own: the mallet can lift it and put it down elsewhere, like the shipping bin.
        static void MakeMovable(GameObject prop, string id, string itemId = null)
        {
            var fixture = prop.AddComponent<MovableFixture>();
            fixture.Id = id;
            fixture.ItemId = itemId;          // set: the mallet takes it into the backpack as that decoration
            fixture.Size = Vector2Int.one;
            if (itemId != null)
            {
                fixture.Scale = PlacedObjectActor.MachineScale;          // drawn half as big again, like the other things the player carries (playtest 2026-10-13)
                prop.transform.localScale = new Vector3(fixture.Scale, fixture.Scale, 1f);
                prop.transform.position += new Vector3(0f, (fixture.Scale - 1f) * 0.5f, 0f);
                var box = prop.GetComponent<BoxCollider2D>();
                if (box != null) { box.size = Vector2.one / fixture.Scale; box.offset = new Vector2(0f, -(fixture.Scale - 1f) * 0.5f / fixture.Scale); }
            }
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
