using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Opens the shop UI. M1 has no town yet, so the general store stall stands on the farm (replaced in M2).
    public sealed class ShopCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] string _shopId = "general";

        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowShop(_shopId);
        }
    }
}
