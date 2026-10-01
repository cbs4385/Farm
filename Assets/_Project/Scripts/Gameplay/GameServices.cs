using Farm.Core;
using Farm.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Gameplay
{
    // Creates the gameplay-level persistent services when the Bootstrapper builds the service root.
    public static class GameServices
    {
        static bool _hooked;

        // Tests point this at a temp folder so they never touch the player's real saves and settings.
        public static string DataRootOverride;

        static string DataRoot => DataRootOverride ?? Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetHook() => _hooked = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            if (_hooked) return;
            _hooked = true;
            Bootstrapper.ServicesCreated += Create;
        }

        static void Create(GameObject root)
        {
            var bus = ServiceLocator.Get<EventBus>();

            var settings = new SettingsStore(DataRoot);
            settings.Load();
            ServiceLocator.Register(settings);

            var db = Resources.Load<GameDatabase>(GameDatabase.ResourcePath);
            if (db == null)
            {
                Log.Error("GameDatabase asset missing at Resources/GameDatabase. Run 'Farm/Setup/Generate Content'.");
                db = GameDatabase.Create(new ItemDefinition[0], new CropDefinition[0]);
            }
            ServiceLocator.Register(db);

            var inputAsset = Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            if (inputAsset == null)
                Log.Error("FarmInput asset missing at Resources/FarmInput. Run 'Farm/Setup/Generate Input Actions'.");
            else
            {
                var input = new InputService(inputAsset);
                input.ApplyOverrides(settings.Current.BindingOverridesJson);
                ServiceLocator.Register(input);
            }

            var saves = new SaveService(DataRoot);
            ServiceLocator.Register(saves);

            var session = root.AddComponent<GameSession>();
            session.Init(bus, db, saves);
            ServiceLocator.Register(session);

            // Optional content packs (extra items/crops) merge into the core database.
            foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder))
                db.Merge(pack);

            ServiceLocator.Register(root.AddComponent<AtmosphereService>());

            var audio = root.AddComponent<AudioService>();
            ServiceLocator.Register(audio);
            audio.ApplySettings(settings.Current);

            DisplaySettings.ApplyAtStartup(settings.Current);

            // Modules (optional layers in their own assemblies) hook in last, once every service exists.
            GameModules.InitializeAll(new ModuleContext
            {
                Bus = bus, Session = session, Hooks = session.Hooks, Db = db, Settings = settings.Current,
            });
        }
    }
}
