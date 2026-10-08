using System;
using System.IO;
using UnityEngine;

namespace Farm.Core
{
    [Serializable]
    public sealed class SettingsData
    {
        public int Version = 1;
        public float MasterVolume = 1f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 0.8f;
        public float AmbienceVolume = 0.7f;
        public int ResolutionWidth = 1280;
        public int ResolutionHeight = 720;
        public bool Fullscreen = true;
        public bool VSync = true;
        public string Language = "en";
        public float TextScale = 1f;
        public string BindingOverridesJson = "";

        // Content intensity for the planned horror layer: 0 = off (a plain farming game), 1 = mild, 2 = full.
        // Every horror hook must respect this; see docs/adr/0002-mythos-extension-points.md.
        public int HorrorLevel = 2;

        // Accessibility and quality of life (T-063, T-066).
        public bool ColorblindPalette;      // bars and warnings use colours that stay apart for red-green colour blindness
        public bool ReduceFlashes;          // no lightning flashes
        public bool RelaxedEnergy;          // tools and combat cost half the energy
        public int DayLength = 1;           // 0 = long days, 1 = normal, 2 = short, 3 = very long (about 45 minutes, for a player on an exercise bike)
        public int DialogueSpeed = 1;       // text speed in conversations: 0 slow, 1 normal, 2 fast, 3 instant
        public bool AutoAdvance;            // conversations move on by themselves after a read time
        public float HudTransparency = 0.2f;   // how see-through the status bar along the top of the screen is (0 solid, 0.6 mostly clear)
        public bool HoverLabels = true;     // a small label names what the mouse is over in the world
        public bool MouseAim = true;        // the tool square follows the mouse around the player (playtest feedback); off keeps facing-only aiming
        public bool Barks = true;           // villagers near you say short ambient lines in speech bubbles (T-125)
        public bool ChatMenu = true;        // after a chat, offer topics and social actions (jokes, compliments ...)
        public bool VoiceBlips = true;      // villagers' talking blips and signature sounds (T-132)

        // Streaming (T-145): larger, brisker dialogue and a visible content-level badge; and an optional timer for choices, for
        // a chat that votes (0 = off). The game never shows file paths or account names, with or without stream mode.
        public bool StreamMode;
        public int ChoiceTimer;             // seconds a choice waits before the default is taken (0 = no timer)
        public static readonly int[] ChoiceTimerSteps = { 0, 15, 30, 60 };
        public const float StreamDialogueTextFactor = 1.2f;     // the dialogue text only; menus keep their size
        public const int StreamMinDialogueSpeed = 2;            // fast

        public float DialogueTextFactor => StreamMode ? StreamDialogueTextFactor : 1f;
        public int EffectiveDialogueSpeed => StreamMode ? Mathf.Max(DialogueSpeed, StreamMinDialogueSpeed) : DialogueSpeed;

        public static readonly float[] SecondsPerStepByDayLength = { 10f, 7f, 5f, 19f };
        public static int DayLengthCount => SecondsPerStepByDayLength.Length;
        public float SecondsPerStep => SecondsPerStepByDayLength[Mathf.Clamp(DayLength, 0, DayLengthCount - 1)];

        // How long a whole day (6 in the morning to 6 the next morning, 144 steps of ten minutes) lasts in real minutes at a day length setting. (pure)
        public static float RealMinutesPerDay(int dayLength) =>
            (GameDateTime.DayEndMinute - GameDateTime.DayStartMinute) / GameClock.MinutesPerStep * SecondsPerStepByDayLength[Mathf.Clamp(dayLength, 0, DayLengthCount - 1)] / 60f;

        public void Clamp()
        {
            HorrorLevel = Mathf.Clamp(HorrorLevel, 0, 2);
            DayLength = Mathf.Clamp(DayLength, 0, DayLengthCount - 1);
            DialogueSpeed = Mathf.Clamp(DialogueSpeed, 0, 3);
            ChoiceTimer = Mathf.Clamp(ChoiceTimer, 0, 120);
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            AmbienceVolume = Mathf.Clamp01(AmbienceVolume);
            TextScale = Mathf.Clamp(TextScale, 0.75f, 1.5f);
            HudTransparency = Mathf.Clamp(HudTransparency, 0f, 0.6f);
            ResolutionWidth = Mathf.Max(640, ResolutionWidth);
            ResolutionHeight = Mathf.Max(360, ResolutionHeight);
            if (string.IsNullOrEmpty(Language)) Language = "en";
        }
    }

    // Loads/saves settings.json under a root directory (Application.persistentDataPath in the game).
    public sealed class SettingsStore
    {
        readonly string _path;

        public SettingsStore(string rootDirectory)
        {
            _path = Path.Combine(rootDirectory, "settings.json");
        }

        public SettingsData Current { get; private set; } = new SettingsData();

        public event Action Changed;

        public SettingsData Load()
        {
            var text = AtomicFile.Read(_path, IsValid, out _);
            if (text != null)
            {
                Current = JsonUtility.FromJson<SettingsData>(text);
                Current.Clamp();
            }
            else
            {
                Current = new SettingsData();
            }
            return Current;
        }

        public void Save()
        {
            Current.Clamp();
            AtomicFile.Write(_path, JsonUtility.ToJson(Current, true));
            Changed?.Invoke();
        }

        static bool IsValid(string text)
        {
            try { return JsonUtility.FromJson<SettingsData>(text) != null; }
            catch (Exception) { return false; }
        }
    }
}
