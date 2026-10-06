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

        // Test runs: every volume of the options is forced to this value (the tests set 1% so that the constant sound effects do not become a noise).
        public static float? AudioVolumeOverride;

        static string DataRoot => DataRootOverride ?? Application.persistentDataPath;
        public static string DataRootPath => DataRoot;

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
            if (AudioVolumeOverride.HasValue)
            {
                var quiet = Mathf.Clamp01(AudioVolumeOverride.Value);
                settings.Current.MasterVolume = settings.Current.MusicVolume = settings.Current.SfxVolume = settings.Current.AmbienceVolume = quiet;
            }
            ServiceLocator.Register(settings);

            BusinessHoursRegistry.RegisterConditionAtom();   // `open:<shopId>` in conditions
            StoryConditions.Register();                       // hearts, has, quest, knows in conditions
            Merchant.RegisterConditions();                    // merchant, rotate in conditions
            BusinessHoursRegistry.RegisterDefaults();

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
            session.Story = StoryContent.LoadFromResources();
            ServiceLocator.Register(session);

            // Optional content packs (extra items/crops) merge into the core database.
            foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder))
                db.Merge(pack);

            ServiceLocator.Register(root.AddComponent<AtmosphereService>());

            var audio = root.AddComponent<AudioService>();
            ServiceLocator.Register(audio);
            audio.ApplySettings(settings.Current);
            root.AddComponent<MusicDirector>();

            DisplaySettings.ApplyAtStartup(settings.Current);

            // Modules (optional layers in their own assemblies) hook in last, once every service exists.
            GameModules.InitializeAll(new ModuleContext
            {
                Bus = bus, Session = session, Hooks = session.Hooks, Db = db, Settings = settings.Current,
            });
        }
    }
}
