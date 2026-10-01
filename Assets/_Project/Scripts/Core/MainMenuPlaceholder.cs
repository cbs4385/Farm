using UnityEngine;

namespace Farm.Core
{
    // M0 placeholder. Replaced by the real main menu in T-021.
    public sealed class MainMenuPlaceholder : MonoBehaviour
    {
        GUIStyle _style;

        void OnGUI()
        {
            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"Farm\nv{Application.version} (M0 placeholder)", _style);
        }
    }
}
