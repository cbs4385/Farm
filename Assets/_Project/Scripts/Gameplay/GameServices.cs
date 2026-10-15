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

        static void Create(ServicesRoot services)
        {
            var root = services.Root;
            var bus = services.Bus;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var dataRoot = CommandLine.GetArg("-farmDataRoot");
            if (!string.IsNullOrEmpty(dataRoot) && DataRootOverride == null) DataRootOverride = dataRoot;
#endif
            var settings = new SettingsStore(DataRoot);
            settings.Load();
            if (AudioVolumeOverride.HasValue)
            {
                var quiet = Mathf.Clamp01(AudioVolumeOverride.Value);
                settings.Current.MasterVolume = settings.Current.MusicVolume = settings.Current.SfxVolume = settings.Current.AmbienceVolume = quiet;
            }

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

            InputService inputService = null;
            var inputAsset = Resources.Load<InputActionAsset>(InputNames.ResourcePath);
            if (inputAsset == null)
                Log.Error("FarmInput asset missing at Resources/FarmInput. Run 'Farm/Setup/Generate Input Actions'.");
            else
            {
                inputService = new InputService(inputAsset);
                inputService.ApplyOverrides(settings.Current.BindingOverridesJson);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // QA: `-farmDataRoot <folder>` keeps saves and settings away from the real ones; `-farmFakeCloud <folder>` makes a folder the cloud, so the cloud-save screens can be seen and tried without Steam.
            var fakeCloud = CommandLine.GetArg("-farmFakeCloud");
            if (!string.IsNullOrEmpty(fakeCloud)) Farm.Platform.PlatformServices.Use(new Farm.Platform.FolderCloudPlatform(fakeCloud));
#endif
            var saves = new SaveService(DataRoot, new ISaveMigration[] { new FarmMirrorMigration() }, () => Farm.Platform.PlatformServices.Current.Cloud);

            var session = root.AddComponent<GameSession>();
            session.Init(bus, db, saves);
            session.Story = StoryContent.LoadFromResources();

            // Optional content packs (extra items/crops) merge into the core database.
            foreach (var pack in Resources.LoadAll<ContentPack>(ContentPack.ResourceFolder))
                db.Merge(pack);

            var atmosphere = root.AddComponent<AtmosphereService>();

            var audio = root.AddComponent<AudioService>();
            audio.ApplySettings(settings.Current);
            var music = root.AddComponent<MusicDirector>();
            var watcher = root.AddComponent<InputDeviceWatcher>();

            DisplaySettings.ApplyAtStartup(settings.Current);

            // The run's context (docs/adr/0006): everything above, in one object.
            var context = new GameContext
            {
                Bus = bus, Settings = settings, Db = db, Saves = saves, Session = session, Audio = audio,
                Atmosphere = atmosphere, Scenes = services.Loader, Input = inputService,
            };
            context.Notify = new Notifier(session, () => context.Ui);
            saves.CloudNotice = key => context.Notify.Toast(L.Get(key));
            GameContext.SetCurrent(context);
            session.Game = context;
            context.Music = music;
            music.Init(context);
            watcher.Init(context);
            var uiAlready = root.GetComponent<IUiService>();          // the UI service may have been made first
            if (uiAlready != null) context.AttachUi(uiAlready);

            // Modules (optional layers in their own assemblies) hook in last, once every service exists.
            GameModules.InitializeAll(new ModuleContext
            {
                Bus = bus, Session = session, Hooks = session.Hooks, Db = db, Settings = settings.Current,
            });
        }
    }
}
