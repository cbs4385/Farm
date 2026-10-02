using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Opens a shop's buy screen while the business is open; a closed counter says when it opens again.
    public sealed class ShopCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] string _shopId = "general";

        public string ShopId { get => _shopId; set => _shopId = value; }

        public void Interact(PlayerActions player)
        {
            if (!BusinessHoursRegistry.IsOpen(_shopId, player.Session.Clock.Now))
            {
                player.Session.Toast(BusinessHoursRegistry.ClosedMessage(_shopId));
                return;
            }
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowShop(_shopId);
        }
    }
}
