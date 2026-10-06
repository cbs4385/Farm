using System.IO;
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
        // The greenhouse stands beside the house; its door is locked until the carpenter has built it.
        const int GhX0 = 15, GhX1 = 23, GhY0 = 20, GhY1 = 24, GhDoorX = 19;
        // The coop and the barn stand east of the greenhouse; their doors are locked until the carpenter has built them.
        const int CoopX0 = 26, CoopX1 = 30, CoopDoorX = 28, BarnX0 = 32, BarnX1 = 38, BarnDoorX = 35, OutY0 = 20, OutY1 = 24;
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
            public bool FacesSouth;       // door on the south wall (north row of buildings) or the north wall
        }

        static readonly Building[] Buildings =
        {
            new Building { MapId = MapIds.GeneralStore, Style = "general", Business = "general",    X0 = 5,  X1 = 12, Y0 = 24, Y1 = 29, DoorX = 8,  FacesSouth = true },
            new Building { MapId = MapIds.Blacksmith, Style = "blacksmith",   Business = "blacksmith", X0 = 14, X1 = 21, Y0 = 24, Y1 = 29, DoorX = 17, FacesSouth = true },
            new Building { MapId = MapIds.Carpenter, Style = "carpenter",    Business = "carpenter",  X0 = 29, X1 = 36, Y0 = 24, Y1 = 29, DoorX = 32, FacesSouth = true },
            new Building { MapId = MapIds.Library, Style = "library",      Business = "library",    X0 = 39, X1 = 46, Y0 = 24, Y1 = 29, DoorX = 42, FacesSouth = true },
            new Building { MapId = MapIds.Saloon, Style = "saloon",       Business = "saloon",     X0 = 6,  X1 = 15, Y0 = 6,  Y1 = 11, DoorX = 10, FacesSouth = false },
            new Building { MapId = MapIds.Clinic, Style = "clinic",       Business = "clinic",     X0 = 31, X1 = 39, Y0 = 6,  Y1 = 11, DoorX = 35, FacesSouth = false },
            new Building { MapId = MapIds.CommunityHall, Style = "hall", Business = null,        X0 = 41, X1 = 48, Y0 = 6,  Y1 = 11, DoorX = 44, FacesSouth = false },
        };

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

            BuildInterior(MapIds.GeneralStore, 12, 9, 5, new[]
            {
                new Prop("Counter", "obj_counter_wide", 3, 5, "general", w: 2), new Prop("Counter2", "obj_counter_wide", 5, 5, "general", w: 2),
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Crate1", "obj_bin", 1, 2), new Prop("Crate2", "obj_bin", 10, 2),
                new Prop("PackCounter", "obj_counter_wide", 9, 4, upgradesAt: "general", w: 2),
                new Prop("Seat", "obj_chair", 1, 5, seatNpc: "tilda", seatKey: "window"),
                new Prop("CatDoor", "obj_cat_door", 10, 5, curio: "catdoor"),
            });
            BuildInterior(MapIds.Blacksmith, 10, 8, 4, new[]
            {
                new Prop("Counter1", "obj_counter_wide", 2, 4, w: 2), new Prop("Counter2", "obj_counter_wide", 4, 4, upgradesAt: "blacksmith", w: 2),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Shelf3", "obj_shelf", 7, 6),
                new Prop("Shelf4", "obj_shelf", 8, 6), new Prop("Anvil", "obj_table", 7, 2),
            });
            BuildInterior(MapIds.Carpenter, 10, 8, 4, new[]
            {
                new Prop("Counter1", "obj_counter_wide", 2, 4, "carpenter", w: 2), new Prop("Counter2", "obj_counter_wide", 4, 4, upgradesAt: "carpenter", w: 2),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Bench", "obj_table", 7, 5),
                new Prop("Bench2", "obj_table", 8, 5), new Prop("Planks", "obj_bin", 8, 2),
            });
            BuildInterior(MapIds.Saloon, 14, 10, 6, new[]
            {
                new Prop("Bar1", "obj_counter_wide", 3, 7, w: 2), new Prop("Bar2", "obj_counter_wide", 5, 7, w: 2), new Prop("Bar3", "obj_counter_wide", 7, 7, w: 2),
                new Prop("Table1", "obj_table", 2, 3), new Prop("Table2", "obj_table", 11, 3), new Prop("Table3", "obj_table", 11, 5),
                new Prop("Table4", "obj_table", 2, 5), new Prop("Shelf1", "obj_shelf", 4, 8), new Prop("Shelf2", "obj_shelf", 6, 8),
                new Prop("Seat", "obj_chair", 12, 3, seatNpc: "wren", seatKey: "stool"),
            });
            BuildInterior(MapIds.Clinic, 10, 8, 4, new[]
            {
                new Prop("Bed1", "obj_bed", 2, 6), new Prop("Bed2", "obj_bed", 4, 6), new Prop("Bed3", "obj_bed", 6, 6),
                new Prop("Couch", "obj_couch", 8, 6, seatNpc: "elara", seatKey: "couch"),
                new Prop("Desk1", "obj_counter_wide", 7, 3, upgradesAt: "clinic", w: 2), new Prop("Shelf", "obj_shelf", 1, 4),
            });
            BuildInterior(MapIds.CommunityHall, 14, 10, 7, new[]
            {
                new Prop("Board", "obj_board", 7, 7), new Prop("Table1", "obj_table", 3, 4), new Prop("Table2", "obj_table", 10, 4),
                new Prop("Shelf1", "obj_shelf", 2, 8), new Prop("Shelf2", "obj_shelf", 11, 8),
            });
            BuildInterior(MapIds.Library, 12, 9, 5, new[]
            {
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Shelf7", "obj_shelf", 2, 4), new Prop("Shelf8", "obj_shelf", 3, 4), new Prop("Shelf9", "obj_shelf", 8, 4),
                new Prop("Shelf10", "obj_shelf", 9, 4), new Prop("Desk", "obj_counter_wide", 5, 5, w: 2), new Prop("Reading", "obj_table", 10, 2),
                new Prop("Seat", "obj_chair", 1, 2, seatNpc: "ione", seatKey: "chair"),
                new Prop("NookSeat", "obj_armchair", 10, 5, seatNpc: "hazel", seatKey: "nook"),
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

                    var exit = x == FarmW - 1 && y >= FarmExitY0 && y <= FarmExitY1;
                    var edge = (x == 0 || y == 0 || x == FarmW - 1 || y == FarmH - 1) && !exit;
                    var house = x >= HouseX0 && x <= HouseX1 && y >= HouseY0 && y <= HouseY1 && !(x == DoorX && y == HouseY0);
                    var greenhouse = x >= GhX0 && x <= GhX1 && y >= GhY0 && y <= GhY1 && !(x == GhDoorX && y == GhY0);
                    var coop = x >= CoopX0 && x <= CoopX1 && y >= OutY0 && y <= OutY1 && !(x == CoopDoorX && y == OutY0);
                    var barn = x >= BarnX0 && x <= BarnX1 && y >= OutY0 && y <= OutY1 && !(x == BarnDoorX && y == OutY0);
                    var roof = (greenhouse || coop || barn) && y == OutY1;
                    if (edge || house || greenhouse || coop || barn) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile(roof ? "tile_roof" : "tile_wall"));
                }
            rig.Ground.SetTile(new Vector3Int(DoorX, HouseY0, 0), GetTile("tile_floor_wood"));
            rig.Ground.SetTile(new Vector3Int(GhDoorX, GhY0, 0), GetTile("tile_door"));
            rig.Ground.SetTile(new Vector3Int(CoopDoorX, OutY0, 0), GetTile("tile_door"));
            rig.Ground.SetTile(new Vector3Int(BarnDoorX, OutY0, 0), GetTile("tile_door"));

            AddSpawn("default", Center(DoorX, HouseY0 - 3));
            AddSpawn("fromHouse", Center(DoorX, HouseY0 - 2));
            AddSpawn("fromVillage", Center(MapLayout.FarmArriveX, MapLayout.FarmRoadY));
            AddSpawn("fromCoop", Center(CoopDoorX, OutY0 - 2));
            AddSpawn("fromBarn", Center(BarnDoorX, OutY0 - 2));
            AddWarp(Center(CoopDoorX, OutY0), MapIds.Coop, "default", condition: "flag:" + AnimalRules.BuildingFlag(MapIds.Coop), blockedKey: "coop.locked");
            AddWarp(Center(BarnDoorX, OutY0), MapIds.Barn, "default", condition: "flag:" + AnimalRules.BuildingFlag(MapIds.Barn), blockedKey: "barn.locked");
            AddSpawn("fromGreenhouse", Center(GhDoorX, GhY0 - 2));
            AddSpawn("sleepwalk", Center(10, 15));
            AddWarp(Center(DoorX, HouseY0), MapIds.FarmHouse, "default");
            AddWarp(Center(GhDoorX, GhY0), MapIds.Greenhouse, "default", condition: "flag:" + MapIds.GreenhouseFlag, blockedKey: "greenhouse.locked");
            AddWarp(Center(MapLayout.FarmExitX, MapLayout.FarmRoadY), MapIds.Village, "fromFarm", new Vector2(1f, 3f));

            AddObject("Mailbox", "obj_mailbox", Center(10, 19), solid: true).AddComponent<Mailbox>();

            // The shipping bin stays on the farm; the general store now lives in the village.
            var bin = AddObject("ShippingBin", "obj_shipping_bin", Center(13, 17) + new Vector3(0.5f, 0.5f, 0f), solid: true, size: new Vector2(2f, 2f));      // a 2 x 2 bin on cells (13..14, 17..18)
            bin.AddComponent<ShippingBin>();

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Farm}.unity");
        }

        // ---- FarmHouse ----------------------------------------------------------------------------------------

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

            var bed = AddObject("Bed", "obj_bed", Center(2, 6), solid: true);
            bed.AddComponent<Bed>();

            // The kitchen: cook with what is in the backpack.
            var kitchen = AddObject("Kitchen", "obj_kitchen", Center(9, 7), solid: true);
            kitchen.AddComponent<Kitchen>();

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
                    foreach (var b in Buildings)
                        if (x == b.DoorX && InConnector(b, y)) ground = "tile_path";
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(ground));
                }

            // Edges are walls, apart from the three ways out: west to the farm, north to the forest, south to the beach.
            for (var y = 0; y < VillageH; y++)
                for (var x = 0; x < VillageW; x++)
                {
                    var edge = x == 0 || y == 0 || x == VillageW - 1 || y == VillageH - 1;
                    var west = x == 0 && y >= RoadY0 && y <= RoadY1;
                    var lane = (y == 0 || y == VillageH - 1) && x >= LaneX0 && x <= LaneX1;
                    if (edge && !west && !lane) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(2, 17));
            AddSpawn("fromFarm", Center(2, 17));
            AddSpawn("fromForest", Center(25, VillageH - 3));
            AddSpawn("fromBeach", Center(25, 2));
            AddSpawn("sleepwalk", Center(24, 17));
            AddWarp(Center(0, 17), MapIds.Farm, "fromVillage", new Vector2(1f, 3f));
            AddWarp(Center(25, VillageH - 1), MapIds.Forest, "fromVillage", new Vector2(3f, 1f));
            AddWarp(Center(25, 0), MapIds.Beach, "fromVillage", new Vector2(3f, 1f));

            foreach (var b in Buildings) PlaceBuilding(rig, b);
            AddObject("HelpWantedBoard", "obj_board", Center(22, 19), solid: true).AddComponent<HelpWantedBoard>();

            // The traveling merchant's stall appears on some days only (the condition `merchant`).
            var merchant = new GameObject("MerchantStall");
            var visible = merchant.AddComponent<ConditionalObject>();
            visible.Condition = "merchant:today";
            var stall = AddObject("Stall", "obj_stall", Center(30, 19), solid: true);
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
                    if (y >= RoadY0 - 1 && y <= RoadY1 + 1 || x >= LaneX0 - 1 && x <= LaneX1 + 1) continue;
                    var edgeBand = x >= VillageW - 6 || y >= VillageH - 6;
                    if ((x * 37 + y * 53) % (edgeBand ? 3 : 17) != 0) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);
                }

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Village}.unity");
        }

        // The cells of the lane from a door to the road.
        static bool InConnector(Building b, int y) =>
            b.FacesSouth ? y > RoadY1 && y < b.Y0 : y < RoadY0 && y > b.Y1;

        static void PlaceBuilding(Rig rig, Building b)
        {
            var doorY = b.FacesSouth ? b.Y0 : b.Y1;
            var roofY = b.FacesSouth ? b.Y1 : b.Y0;
            for (var y = b.Y0; y <= b.Y1; y++)
                for (var x = b.X0; x <= b.X1; x++)
                {
                    if (x == b.DoorX && y == doorY) continue;
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile($"bld_{b.Style}_{(y == roofY ? "roof" : "wall")}"));
                }
            rig.Ground.SetTile(new Vector3Int(b.DoorX, doorY, 0), GetTile("tile_door"));

            // What makes each building its own: windows, a sign with a picture of the trade over the door, and an ornament on the roof.
            var inward = b.FacesSouth ? 1 : -1;
            foreach (var wx in new[] { b.DoorX - 2, b.DoorX + 2 })
                AddDecor($"{b.MapId}_Window_{wx}", $"bld_{b.Style}_window", Center(wx, doorY + 2 * inward));
            AddDecor($"{b.MapId}_Sign", $"bld_{b.Style}_sign", Center(b.DoorX, doorY + inward));
            var top = AddDecorObject($"{b.MapId}_RoofTop", $"bld_{b.Style}_roof_top", Center(b.X1 - 1, roofY));
            var frame2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/bld_{b.Style}_roof_top2.png");
            if (frame2 != null) top.AddComponent<FrameAnimator>().Configure(new[] { Sprite($"bld_{b.Style}_roof_top"), frame2 }, 0.7f, b.X0 * 0.23f);   // a flag or vane that moves
            else if (b.Style == "blacksmith" || b.Style == "saloon") top.AddComponent<RoofSmoke>();                                                   // a chimney that smokes

            // A tag beside the door: green when the business is open, red when it is closed.
            if (!string.IsNullOrEmpty(b.Business))
            {
                var tag = AddDecorObject($"{b.MapId}_OpenTag", "bld_tag_open", Center(b.DoorX + 1, doorY));
                tag.AddComponent<BoxCollider2D>().isTrigger = true;
                tag.GetComponent<BoxCollider2D>().size = Vector2.one;
                tag.AddComponent<BusinessStatusSign>().Configure(b.Business, Sprite("bld_tag_open"), Sprite("bld_tag_closed"));
            }

            var outsideY = b.FacesSouth ? doorY - 1 : doorY + 1;
            AddSpawn("from" + b.MapId, Center(b.DoorX, outsideY));
            AddWarp(Center(b.DoorX, doorY), b.MapId, "default", Vector2.one, business: b.Business);
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
                    var edge = x == 0 || y == 0 || x == ForestW - 1 || y == ForestH - 1;
                    var gap = path && (y == 0 || y == ForestH - 1);
                    if (edge && !gap) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(19, 2));
            AddSpawn("fromVillage", Center(19, 2));
            AddSpawn("fromWoods", Center(19, ForestH - 4));
            AddSpawn("sleepwalk", Center(19, 10));
            AddWarp(Center(19, 0), MapIds.Village, "fromForest", new Vector2(3f, 1f));

            // Trees on a loose lattice, leaving the path and the arrival area clear.
            for (var y = 3; y < ForestH - 1; y++)
                for (var x = 1; x < ForestW - 1; x++)
                {
                    if (x >= ForestPathX0 - 2 && x <= ForestPathX1 + 2) continue;
                    if (x >= 3 && x <= 11 && y >= 6 && y <= 14) continue;            // the pond and its shore
                    if (x >= 21 && x <= 36 && y >= 3 && y <= 7) continue;             // the way to the cave
                    if ((x * 7 + y * 13) % 9 != 0) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);
                }

            // The cave: a path east from the main path to the mouth of the mine.
            for (var x = ForestPathX1 + 1; x <= 34; x++)
                for (var y = 4; y <= 6; y++) rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile("tile_path"));
            AddSpawn("fromMine", Center(33, 5));
            AddWarp(Center(35, 5), MapIds.Mine, "default", new Vector2(1f, 3f));
            AddObject("CaveMouth", "obj_boulder", Center(36, 5), solid: true);

            // A pond to fish in: one oval body of water with a shoreline (the tile for each cell depends on which neighbours are land),
            // walled so the player stops at the shore.
            bool Pond(int px, int py) { var dx = (px - 6.5f) / 3.8f; var dy = (py - 10f) / 2.9f; return dx * dx + dy * dy <= 1f; }
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

        static void BuildBeach()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = CreateMapRig(MapIds.Beach, indoor: false, allowFarming: false);

            for (var y = 0; y < BeachH; y++)
                for (var x = 0; x < BeachW; x++)
                {
                    var water = y < BeachWaterRows;
                    var sand = y < BeachH - 6;
                    var tileName = water ? WaterShore.TileNameAt((wx, wy) => wy < BeachWaterRows, x, y) : sand ? "tile_sand" : "tile_grass";   // the sea runs on past the map's sides
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(tileName));
                    if (water) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile(tileName));
                    var edge = x == 0 || y == BeachH - 1 || x == BeachW - 1;
                    var gap = y == BeachH - 1 && x >= BeachExitX0 && x <= BeachExitX1;
                    if (edge && !gap) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(17, BeachH - 3));
            AddSpawn("fromVillage", Center(17, BeachH - 3));
            AddSpawn("sleepwalk", Center(13, 7));
            AddWarp(Center(17, BeachH - 1), MapIds.Village, "fromBeach", new Vector2(3f, 1f));

            // The fish stall: a working counter whose hours (06:00-14:00, closed Thursday) are enforced.
            var stall = AddObject("FishStall", "obj_stall", Center(10, 11), solid: true);
            stall.AddComponent<ShopCounter>().ShopId = "fish";

            // The good rock Felix keeps warm for the player.
            var rock = AddObject("FishingRock", "obj_fishing_rock", Center(13, 9), solid: true).AddComponent<SeatSpot>();
            rock.NpcId = "felix";
            rock.SeatKey = "rock";

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.Beach}.unity");
        }

        // ---- interiors ----------------------------------------------------------------------------------------

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
            }

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{mapId}.unity");
        }

        // ---- shared rig ---------------------------------------------------------------------------------------

        struct Rig
        {
            public Tilemap Ground, Walls;
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

            return new Rig { Ground = ground, Walls = walls };
        }

        static Tilemap TilemapLayer(GameObject grid, string name, int order, bool collider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = order;
            if (collider) go.AddComponent<TilemapCollider2D>();
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
            if (solid) go.AddComponent<BoxCollider2D>().size = size ?? Vector2.one;
            if (spriteName == "obj_tree" || spriteName == "obj_bramble") go.AddComponent<ObjectSway>();   // leans in the wind about its base; the collider stays put
            return go;
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
