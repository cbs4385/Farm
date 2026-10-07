using System;
using UnityEngine;

namespace Farm.Core
{
    // Lives in the Bootstrap scene (build index 0). Creates persistent services, then loads the main menu.
    public sealed class Bootstrapper : MonoBehaviour
    {
        static bool _initialized;

        // Higher layers (Gameplay, Platform) register here to create their services on the persistent root.
        public static event Action<GameObject> ServicesCreated;

        void Start()
        {
            // The game was renamed, which moved its data folder: carry the old saves and settings across the first time (failure-isolated).
            try { DataMigration.Run(Application.persistentDataPath); }
            catch (Exception e) { Debug.LogWarning("[Bootstrapper] Could not carry the old saves across: " + e.Message); }
            CrashLog.Install(System.IO.Path.Combine(Application.persistentDataPath, "logs"));
            var sceneLoader = InitializeServices();
            ScreenshotCapture.StartIfRequested(sceneLoader);
            // `-farmScene <name>` lets QA/automation start in another scene.
            sceneLoader.Load(CommandLine.GetArg("-farmScene") ?? SceneNames.MainMenu, 0f);
        }

        // Safe to call repeatedly (e.g. from tests).
        public static SceneLoader InitializeServices()
        {
            if (_initialized && ServiceLocator.TryGet<SceneLoader>(out var existing) && existing != null)
                return existing;

            var root = new GameObject("Services");
            DontDestroyOnLoad(root);

            ServiceLocator.Clear();
            ServiceLocator.Register(new EventBus());
            var loader = root.AddComponent<SceneLoader>();
            ServiceLocator.Register(loader);

            ServicesCreated?.Invoke(root);

            _initialized = true;
            Log.Info("Services initialized.");
            return loader;
        }

        // Tests only: tear down the persistent services so the next InitializeServices starts clean.
        public static void ResetForTests()
        {
            var root = GameObject.Find("Services");
            if (root != null) DestroyImmediate(root);
            ServiceLocator.Clear();
            _initialized = false;
        }

        // Domain reload may be disabled in Enter Play Mode settings; reset statics defensively.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _initialized = false;
            ServiceLocator.Clear();
        }
    }
}
