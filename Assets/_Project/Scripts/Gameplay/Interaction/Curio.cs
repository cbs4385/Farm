using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A small thing in a room that is worth a look (a cat door, a keepsake shelf): interacting shows its line, nothing else.
    public sealed class Curio : MonoBehaviour, IInteractable
    {
        [SerializeField] string _key;                // names the string: curio.<key>

        public string Key { get => _key; set => _key = value; }

        public string HoverLabel => L.Get("hover.curio");

        public void Interact(PlayerActions player) => player.Session.Toast(L.Get("curio." + _key));
    }
}
