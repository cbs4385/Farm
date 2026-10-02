using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A counter where the player pays for upgrades (tools at the blacksmith, the backpack at the general store, the
    // energy reserve at the clinic). Like a shop counter it keeps the business's hours.
    public sealed class UpgradeCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] string _shopId = "blacksmith";

        public string ShopId { get => _shopId; set => _shopId = value; }

        public void Interact(PlayerActions player)
        {
            if (!BusinessHoursRegistry.IsOpen(_shopId, player.Session.Clock.Now))
            {
                player.Session.Toast(BusinessHoursRegistry.ClosedMessage(_shopId));
                return;
            }
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowUpgrades(_shopId);
        }
    }
}
