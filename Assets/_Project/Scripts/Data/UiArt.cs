using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Data
{
    // The HUD icons, effect sprites and UI pieces that code needs by name (hud_weather_rain, fx_dig_dirt ...). Built from the sprites in
    // Art/Placeholders by the editor (Farm > Setup > Build UI Art) and loaded from Resources; every lookup answers null when a sprite is
    // missing, so a missing picture is never an error in the game.
    public sealed class UiArt : ScriptableObject
    {
        public const string ResourcePath = "UiArt";

        [Serializable]
        public struct Entry
        {
            public string Name;
            public Sprite Sprite;
        }

        [SerializeField] List<Entry> _sprites = new List<Entry>();

        Dictionary<string, Sprite> _byName;
        static UiArt _instance;
        static bool _tried;

        public IReadOnlyList<Entry> Sprites => _sprites;

        public void Set(IEnumerable<Entry> entries)
        {
            _sprites = new List<Entry>(entries);
            _byName = null;
        }

        public Sprite Find(string name)
        {
            if (_byName == null)
            {
                _byName = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                foreach (var e in _sprites) if (!string.IsNullOrEmpty(e.Name) && e.Sprite != null) _byName[e.Name] = e.Sprite;
            }
            return name != null && _byName.TryGetValue(name, out var sprite) ? sprite : null;
        }

        // The sprite called `name`, or null.
        public static Sprite Get(string name)
        {
            if (!_tried)
            {
                _tried = true;
                _instance = Resources.Load<UiArt>(ResourcePath);
            }
            return _instance != null ? _instance.Find(name) : null;
        }
    }
}
