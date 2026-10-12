using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // Everything that lives for the whole run, built once by the Bootstrapper's service step (GameServices.Create) and handed to whoever needs it (docs/adr/0006-retire-the-service-locator.md). `Current` is the one static anchor: the scene entry points (MapSceneController, the main menu) take the
    // context from it and pass it on; nothing else asks for it.
    public sealed class GameContext
    {
        public EventBus Bus;
        public SettingsStore Settings;
        public GameDatabase Db;
        public SaveService Saves;
        public GameSession Session;
        public InputService Input;
        public AudioService Audio;
        public AtmosphereService Atmosphere;
        public SceneLoader Scenes;
        public INotifier Notify = NullNotifier.Instance;
        public MapContext Map;         // the loaded map: set by MapSceneController when a map starts, null in the menu
        public MusicDirector Music;
        public IUiService Ui;          // set when the UI service is created (it is made in its own assembly, a step later)

        public static GameContext Current { get; private set; }

        static GameContext() => TestResets.Add(() => Current = null);

        public static void SetCurrent(GameContext context) => Current = context;

        // The UI service exists in its own assembly and may be created before or after the context: whichever comes second calls this.
        public void AttachUi(IUiService ui)
        {
            Ui = ui;
            ui?.Attach(this);
        }

        // The members that are still unset. The UI is not counted: it arrives with its own assembly (and a test or a bare scene may have none).
        public IReadOnlyList<string> Missing()
        {
            var missing = new List<string>();
            if (Bus == null) missing.Add(nameof(Bus));
            if (Settings == null) missing.Add(nameof(Settings));
            if (Db == null) missing.Add(nameof(Db));
            if (Saves == null) missing.Add(nameof(Saves));
            if (Session == null) missing.Add(nameof(Session));
            if (Input == null) missing.Add(nameof(Input));
            if (Audio == null) missing.Add(nameof(Audio));
            if (Atmosphere == null) missing.Add(nameof(Atmosphere));
            if (Scenes == null) missing.Add(nameof(Scenes));
            if (Notify == null) missing.Add(nameof(Notify));
            return missing;
        }
    }
}
