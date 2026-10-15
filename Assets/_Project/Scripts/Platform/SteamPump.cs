#if FARM_STEAM
using Steamworks;
using UnityEngine;

namespace Farm.Platform
{
    // Runs Steam's callbacks every frame and shuts Steam down when the game quits.
    sealed class SteamPump : MonoBehaviour
    {
        public static void Create()
        {
            var go = new GameObject("SteamPump") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<SteamPump>();
        }

        void Update() => SteamAPI.RunCallbacks();

        void OnApplicationQuit() => SteamAPI.Shutdown();
    }
}
#endif
