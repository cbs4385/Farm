using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using Farm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Farm.Editor
{
    // T-006/T-007: creates the M0 scenes (Bootstrap, MainMenu, PixelPerfectTest) and registers them in Build Settings.
    // Idempotent: re-running overwrites the generated scenes.
    public static class SceneSetup
    {
        const string SceneDir = "Assets/_Project/Scenes";
        const string PlaceholderDir = "Assets/_Project/Art/Placeholders";

        // Reference resolution from the GDD / Tech Design s3.14
        public const int RefWidth = 480;
        public const int RefHeight = 270;

        [MenuItem("Farm/Setup/Create M0 Scenes")]
        public static void CreateScenes()
        {
            System.IO.Directory.CreateDirectory(SceneDir);
            CreateBootstrap();
            CreateMainMenu();
            CreatePixelPerfectTest();
            MapBuilder.BuildAll();
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] M0 scenes created and registered.");
        }

        public static void CreateScenesAndExit()
        {
            CreateScenes();
            EditorApplication.Exit(0);
        }

        static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        static Camera AddPixelPerfectCamera(string name)
        {
            var go = new GameObject(name) { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = RefHeight / 2f / TextureImportPostprocessor.PixelsPerUnit;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f);
            cam.transform.position = new Vector3(0, 0, -10);
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>();
            go.AddComponent<PixelSnapCamera>();

            var ppc = go.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = TextureImportPostprocessor.PixelsPerUnit;
            ppc.refResolutionX = RefWidth;
            ppc.refResolutionY = RefHeight;
            ppc.upscaleRT = false;
            ppc.pixelSnapping = true;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.None;
            return cam;
        }

        static void CreateBootstrap()
        {
            var scene = NewScene();
            new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{SceneNames.Bootstrap}.unity");
        }

        static void CreateMainMenu()
        {
            var scene = NewScene();
            AddPixelPerfectCamera("Main Camera");
            new GameObject("MainMenu").AddComponent<MainMenuController>();
            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{SceneNames.MainMenu}.unity");
        }

        static void CreatePixelPerfectTest()
        {
            var scene = NewScene();
            var cam = AddPixelPerfectCamera("Main Camera");
            cam.gameObject.AddComponent<CameraDrift>();

            var grass = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/tile_grass.png");
            var dirt = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/tile_dirt.png");
            var wall = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/tile_wall.png");
            var player = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/player_idle_down.png");
            if (grass == null || dirt == null || wall == null || player == null)
                Debug.LogWarning("[SceneSetup] Placeholder sprites missing; run 'Farm/Generate Placeholder Art' first.");

            var root = new GameObject("Grid").transform;
            const int half = 20;
            for (int y = -half; y <= half; y++)
                for (int x = -half; x <= half; x++)
                {
                    var sprite = ((x + y) & 1) == 0 ? grass : dirt;
                    if (Mathf.Abs(x) == half || Mathf.Abs(y) == half) sprite = wall;
                    var tile = new GameObject($"t_{x}_{y}");
                    tile.transform.SetParent(root, false);
                    tile.transform.position = new Vector3(x, y, 0);
                    tile.AddComponent<SpriteRenderer>().sprite = sprite;
                }

            var p = new GameObject("PlayerMarker");
            p.AddComponent<SpriteRenderer>().sprite = player;
            p.GetComponent<SpriteRenderer>().sortingOrder = 1;
            p.transform.position = new Vector3(0, 0.5f, 0);

            EditorSceneManager.SaveScene(scene, $"{SceneDir}/{SceneNames.PixelPerfectTest}.unity");
        }

        static void RegisterBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{SceneDir}/{SceneNames.Bootstrap}.unity", true),
                new EditorBuildSettingsScene($"{SceneDir}/{SceneNames.MainMenu}.unity", true),
                new EditorBuildSettingsScene($"{SceneDir}/{SceneNames.PixelPerfectTest}.unity", true),
            };
            foreach (var map in MapIds.All) scenes.Add(new EditorBuildSettingsScene($"{SceneDir}/{map}.unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
