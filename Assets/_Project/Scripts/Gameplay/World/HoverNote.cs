using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A name for something in the world that is not used by hand (a fountain, a lamp post, a building's wall): the hover label shows it when the mouse rests there, or
    // when a pad player faces it (HoverInspector). The key is a string key; an optional second key names a part of the label ("{0}'s home").
    public sealed class HoverNote : MonoBehaviour
    {
        [SerializeField] string _key;
        [SerializeField] string _argKey;

        public string Key { get => _key; set => _key = value; }
        public string ArgKey { get => _argKey; set => _argKey = value; }

        public string HoverLabel => string.IsNullOrEmpty(_key) ? null : string.IsNullOrEmpty(_argKey) ? L.Get(_key) : L.Get(_key, L.Get(_argKey));

        // Gives a thing a label; with `area` it also gets a trigger over that area so that the mouse can find it (a building's picture has no collider of its own).
        public static HoverNote Add(GameObject go, string key, string argKey = null, Vector2? area = null, Vector2? offset = null)
        {
            var note = go.AddComponent<HoverNote>();
            note.Key = key;
            note.ArgKey = argKey;
            if (area.HasValue)
            {
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = area.Value;
                box.offset = offset ?? Vector2.zero;
            }
            return note;
        }
    }
}
