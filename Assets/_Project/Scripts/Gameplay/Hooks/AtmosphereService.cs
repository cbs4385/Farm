using UnityEngine;

namespace Farm.Gameplay
{
    // Scene-persistent holder of the atmosphere layers (registered in the ServiceLocator by GameServices).
    public sealed class AtmosphereService : MonoBehaviour
    {
        public AtmosphereStack Stack { get; } = new AtmosphereStack();
    }
}
