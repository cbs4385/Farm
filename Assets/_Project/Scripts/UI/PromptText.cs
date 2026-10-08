using Farm.Core;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // A label holding a hint with [[Action]] controls in it (ControlPrompts): it says the right thing for the pad or the keyboard whenever it is shown
    // and again if the player changes from one to the other while it is on screen.
    [RequireComponent(typeof(TMP_Text))]
    public sealed class PromptText : MonoBehaviour
    {
        public string Key;
        TMP_Text _text;

        public static TMP_Text Attach(TMP_Text label, string key)
        {
            var prompt = label.gameObject.AddComponent<PromptText>();
            prompt.Key = key;
            prompt.Refresh();
            return label;
        }

        void Awake() => _text = GetComponent<TMP_Text>();

        void OnEnable()
        {
            ControlPrompts.Changed += Refresh;
            Refresh();
        }

        void OnDisable() => ControlPrompts.Changed -= Refresh;

        public void Refresh()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_text != null && !string.IsNullOrEmpty(Key)) _text.text = L.Get(Key);
        }
    }
}
