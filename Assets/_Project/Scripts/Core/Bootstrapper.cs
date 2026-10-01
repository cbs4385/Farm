using UnityEngine;

namespace Farm.Core
{
    // Lives in the Bootstrap scene (build index 0). Creates persistent services, then loads the main menu.
    public sealed class Bootstrapper : MonoBehaviour
    {
        static bool _initialized;

        void Start()
        {
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

            _initialized = true;
            Log.Info("Services initialized.");
            return loader;
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
