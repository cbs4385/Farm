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
        const int FarmW = 44, FarmH = 32;
        const int HouseX0 = 4, HouseX1 = 11, HouseY0 = 20, HouseY1 = 24, DoorX = 7;
        const int FarmExitY0 = 14, FarmExitY1 = 16;

        // FarmHouse interior: 12 x 9 cells.
        const int InW = 12, InH = 9;

        // Village: 50 x 36. A main road (y 16..18) runs west-east to the farm; a lane (x 24..26) runs south to the
        // beach and north to the forest. Four buildings face the road from the north, two from the south.
        const int VillageW = 50, VillageH = 36;
        const int RoadY0 = 16, RoadY1 = 18, LaneX0 = 24, LaneX1 = 26;

        struct Building
        {
            public string MapId, Business;
            public int X0, X1, Y0, Y1, DoorX;
            public bool FacesSouth;       // door on the south wall (north row of buildings) or the north wall
        }

        static readonly Building[] Buildings =
        {
            new Building { MapId = MapIds.GeneralStore, Business = "general",    X0 = 5,  X1 = 12, Y0 = 24, Y1 = 29, DoorX = 8,  FacesSouth = true },
            new Building { MapId = MapIds.Blacksmith,   Business = "blacksmith", X0 = 14, X1 = 21, Y0 = 24, Y1 = 29, DoorX = 17, FacesSouth = true },
            new Building { MapId = MapIds.Carpenter,    Business = "carpenter",  X0 = 29, X1 = 36, Y0 = 24, Y1 = 29, DoorX = 32, FacesSouth = true },
            new Building { MapId = MapIds.Library,      Business = "library",    X0 = 39, X1 = 46, Y0 = 24, Y1 = 29, DoorX = 42, FacesSouth = true },
            new Building { MapId = MapIds.Saloon,       Business = "saloon",     X0 = 6,  X1 = 15, Y0 = 6,  Y1 = 11, DoorX = 10, FacesSouth = false },
            new Building { MapId = MapIds.Clinic,       Business = "clinic",     X0 = 31, X1 = 39, Y0 = 6,  Y1 = 11, DoorX = 35, FacesSouth = false },
        };

        // A piece of furniture in an interior. ShopId makes it a working counter.
        struct Prop
        {
            public string Name, Sprite, ShopId;
            public int X, Y;
            public Prop(string name, string sprite, int x, int y, string shopId = null)
            { Name = name; Sprite = sprite; X = x; Y = y; ShopId = shopId; }
        }

        public static void BuildAll()
        {
            Directory.CreateDirectory(TileDir);
            BuildFarm();
            BuildFarmHouse();
            BuildVillage();
            BuildForest();
            BuildBeach();

            BuildInterior(MapIds.GeneralStore, 12, 9, 5, new[]
            {
                new Prop("Counter", "obj_counter", 4, 5, "general"), new Prop("Counter2", "obj_counter", 5, 5, "general"),
                new Prop("Counter3", "obj_counter", 6, 5, "general"),
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Crate1", "obj_bin", 1, 2), new Prop("Crate2", "obj_bin", 10, 2),
            });
            BuildInterior(MapIds.Blacksmith, 10, 8, 4, new[]
            {
                new Prop("Counter1", "obj_counter", 3, 4), new Prop("Counter2", "obj_counter", 4, 4), new Prop("Counter3", "obj_counter", 5, 4),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Shelf3", "obj_shelf", 7, 6),
                new Prop("Shelf4", "obj_shelf", 8, 6), new Prop("Anvil", "obj_table", 7, 2),
            });
            BuildInterior(MapIds.Carpenter, 10, 8, 4, new[]
            {
                new Prop("Counter1", "obj_counter", 3, 4), new Prop("Counter2", "obj_counter", 4, 4), new Prop("Counter3", "obj_counter", 5, 4),
                new Prop("Shelf1", "obj_shelf", 1, 6), new Prop("Shelf2", "obj_shelf", 2, 6), new Prop("Bench", "obj_table", 7, 5),
                new Prop("Bench2", "obj_table", 8, 5), new Prop("Planks", "obj_bin", 8, 2),
            });
            BuildInterior(MapIds.Saloon, 14, 10, 6, new[]
            {
                new Prop("Bar1", "obj_counter", 3, 7), new Prop("Bar2", "obj_counter", 4, 7), new Prop("Bar3", "obj_counter", 5, 7),
                new Prop("Bar4", "obj_counter", 6, 7), new Prop("Bar5", "obj_counter", 7, 7), new Prop("Bar6", "obj_counter", 8, 7),
                new Prop("Table1", "obj_table", 2, 3), new Prop("Table2", "obj_table", 11, 3), new Prop("Table3", "obj_table", 11, 5),
                new Prop("Table4", "obj_table", 2, 5), new Prop("Shelf1", "obj_shelf", 4, 8), new Prop("Shelf2", "obj_shelf", 6, 8),
            });
            BuildInterior(MapIds.Clinic, 10, 8, 4, new[]
            {
                new Prop("Bed1", "obj_bed", 2, 6), new Prop("Bed2", "obj_bed", 4, 6), new Prop("Bed3", "obj_bed", 6, 6),
                new Prop("Desk1", "obj_counter", 7, 3), new Prop("Desk2", "obj_counter", 8, 3), new Prop("Shelf", "obj_shelf", 1, 4),
            });
            BuildInterior(MapIds.Library, 12, 9, 5, new[]
            {
                new Prop("Shelf1", "obj_shelf", 2, 7), new Prop("Shelf2", "obj_shelf", 3, 7), new Prop("Shelf3", "obj_shelf", 4, 7),
                new Prop("Shelf4", "obj_shelf", 7, 7), new Prop("Shelf5", "obj_shelf", 8, 7), new Prop("Shelf6", "obj_shelf", 9, 7),
                new Prop("Shelf7", "obj_shelf", 2, 4), new Prop("Shelf8", "obj_shelf", 3, 4), new Prop("Shelf9", "obj_shelf", 8, 4),
                new Prop("Shelf10", "obj_shelf", 9, 4), new Prop("Desk", "obj_counter", 5, 5), new Prop("Reading", "obj_table", 10, 2),
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
                    if (edge || house) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }
            rig.Ground.SetTile(new Vector3Int(DoorX, HouseY0, 0), GetTile("tile_floor_wood"));

            AddSpawn("default", Center(DoorX, HouseY0 - 3));
            AddSpawn("fromHouse", Center(DoorX, HouseY0 - 2));
            AddSpawn("fromVillage", Center(FarmW - 3, 15));
            AddWarp(Center(DoorX, HouseY0), MapIds.FarmHouse, "default");
            AddWarp(Center(FarmW - 1, 15), MapIds.Village, "fromFarm", new Vector2(1f, 3f));

            // The shipping bin stays on the farm; the general store now lives in the village.
            var bin = AddObject("ShippingBin", "obj_bin", Center(13, 17), solid: true);
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

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{MapIds.FarmHouse}.unity");
        }

        // ---- Village ------------------------------------------------------------------------------------------

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
            AddWarp(Center(0, 17), MapIds.Farm, "fromVillage", new Vector2(1f, 3f));
            AddWarp(Center(25, VillageH - 1), MapIds.Forest, "fromVillage", new Vector2(3f, 1f));
            AddWarp(Center(25, 0), MapIds.Beach, "fromVillage", new Vector2(3f, 1f));

            foreach (var b in Buildings) PlaceBuilding(rig, b);

            // A few trees at the corners so the village is not a bare lawn.
            foreach (var t in new[] { new Vector2Int(3, 31), new Vector2Int(46, 33), new Vector2Int(3, 3), new Vector2Int(46, 3), new Vector2Int(20, 4), new Vector2Int(30, 33) })
                AddObject($"Tree_{t.x}_{t.y}", "obj_tree", Center(t.x, t.y), solid: true);

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
                    rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile(y == roofY ? "tile_roof" : "tile_wall"));
                }
            rig.Ground.SetTile(new Vector3Int(b.DoorX, doorY, 0), GetTile("tile_door"));

            var outsideY = b.FacesSouth ? doorY - 1 : doorY + 1;
            AddSpawn("from" + b.MapId, Center(b.DoorX, outsideY));
            AddWarp(Center(b.DoorX, doorY), b.MapId, "default", Vector2.one, business: b.Business);
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
            AddWarp(Center(19, 0), MapIds.Village, "fromForest", new Vector2(3f, 1f));

            // Trees on a loose lattice, leaving the path and the arrival area clear.
            for (var y = 3; y < ForestH - 1; y++)
                for (var x = 1; x < ForestW - 1; x++)
                {
                    if (x >= ForestPathX0 - 2 && x <= ForestPathX1 + 2) continue;
                    if ((x * 7 + y * 13) % 9 != 0) continue;
                    AddObject($"Tree_{x}_{y}", "obj_tree", Center(x, y), solid: true);
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
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(water ? "tile_water" : sand ? "tile_sand" : "tile_grass"));
                    if (water) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_water"));
                    var edge = x == 0 || y == BeachH - 1 || x == BeachW - 1;
                    var gap = y == BeachH - 1 && x >= BeachExitX0 && x <= BeachExitX1;
                    if (edge && !gap) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }

            AddSpawn("default", Center(17, BeachH - 3));
            AddSpawn("fromVillage", Center(17, BeachH - 3));
            AddWarp(Center(17, BeachH - 1), MapIds.Village, "fromBeach", new Vector2(3f, 1f));

            // The fish stall: a working counter whose hours (06:00-14:00, closed Thursday) are enforced.
            var stall = AddObject("FishStall", "obj_stall", Center(10, 11), solid: true);
            stall.AddComponent<ShopCounter>().ShopId = "fish";

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
                var go = AddObject(p.Name, p.Sprite, Center(p.X, p.Y), solid: true);
                if (p.ShopId != null) go.AddComponent<ShopCounter>().ShopId = p.ShopId;
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

            var mapGo = new GameObject("Map");
            var map = mapGo.AddComponent<FarmMap>();
            map.Configure(mapId, ground, soil, crops, allowFarming);

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

        static GameObject AddObject(string name, string spriteName, Vector3 position, bool solid)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite(spriteName);
            sr.sortingOrder = 4;
            if (solid) go.AddComponent<BoxCollider2D>().size = Vector2.one;
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
