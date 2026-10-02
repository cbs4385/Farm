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
        public int DayLength = 1;           // 0 = long days, 1 = normal, 2 = short

        public static readonly float[] SecondsPerStepByDayLength = { 10f, 7f, 5f };
        public float SecondsPerStep => SecondsPerStepByDayLength[Mathf.Clamp(DayLength, 0, 2)];

        public void Clamp()
        {
            HorrorLevel = Mathf.Clamp(HorrorLevel, 0, 2);
            DayLength = Mathf.Clamp(DayLength, 0, 2);
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            AmbienceVolume = Mathf.Clamp01(AmbienceVolume);
            TextScale = Mathf.Clamp(TextScale, 0.75f, 1.5f);
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
