using UnityEngine;

namespace Farm.Gameplay
{
    // Scene-persistent holder of the atmosphere layers (held by the GameContext).
    public sealed class AtmosphereService : MonoBehaviour
    {
        public AtmosphereStack Stack { get; } = new AtmosphereStack();
    }
}
