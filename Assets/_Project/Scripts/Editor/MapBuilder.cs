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
    // T-013/T-014: generates the Farm and FarmHouse map scenes with placeholder art. Re-running overwrites them.
    public static class MapBuilder
    {
        const string SceneDir = "Assets/_Project/Scenes";
        const string ArtDir = "Assets/_Project/Art/Placeholders";
        const string TileDir = "Assets/_Project/Art/Tiles";

        // Farm: 44 x 32 cells. House block at x 4..11, y 20..24, door at (7, 20).
        const int FarmW = 44, FarmH = 32;
        const int HouseX0 = 4, HouseX1 = 11, HouseY0 = 20, HouseY1 = 24, DoorX = 7;

        // FarmHouse interior: 12 x 9 cells.
        const int InW = 12, InH = 9;

        public static void BuildAll()
        {
            Directory.CreateDirectory(TileDir);
            BuildFarm();
            BuildFarmHouse();
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
                    // Path from the door south, then east to the bin and stall area.
                    if (x == DoorX && y < HouseY0 && y >= 14) ground = "tile_path";
                    if (y == 14 && x >= DoorX && x <= 14) ground = "tile_path";
                    rig.Ground.SetTile(new Vector3Int(x, y, 0), GetTile(ground));

                    var edge = x == 0 || y == 0 || x == FarmW - 1 || y == FarmH - 1;
                    var house = x >= HouseX0 && x <= HouseX1 && y >= HouseY0 && y <= HouseY1 && !(x == DoorX && y == HouseY0);
                    if (edge || house) rig.Walls.SetTile(new Vector3Int(x, y, 0), GetTile("tile_wall"));
                }
            rig.Ground.SetTile(new Vector3Int(DoorX, HouseY0, 0), GetTile("tile_floor_wood"));

            AddSpawn("default", Center(DoorX, HouseY0 - 3));
            AddSpawn("fromHouse", Center(DoorX, HouseY0 - 2));
            AddWarp(Center(DoorX, HouseY0), MapIds.FarmHouse, "default");

            var bin = AddObject("ShippingBin", "obj_bin", Center(13, 17), solid: true);
            bin.AddComponent<ShippingBin>();
            var shop = AddObject("ShopCounter", "obj_shop", Center(15, 17), solid: true);
            shop.AddComponent<ShopCounter>();

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

        static void AddWarp(Vector3 position, string targetMap, string targetSpawn)
        {
            var go = new GameObject($"Warp_{targetMap}");
            go.transform.position = position;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            var warp = go.AddComponent<Warp>();
            warp.TargetMap = targetMap;
            warp.TargetSpawn = targetSpawn;
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
