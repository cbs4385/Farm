using UnityEngine;

namespace Farm.Core
{
    // What the Bootstrapper hands to each layer that builds its own services: the persistent object to put components on, and the two services of the core.
    public sealed class ServicesRoot
    {
        public GameObject Root;
        public EventBus Bus;
        public SceneLoader Loader;
    }
}
