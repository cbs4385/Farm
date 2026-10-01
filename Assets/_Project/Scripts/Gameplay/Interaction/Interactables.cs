using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Ships the stack selected on the hotbar. Placed on the farm next to the house.
    public sealed class ShippingBin : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            var session = player.Session;
            var slot = session.State.SelectedHotbar;
            var stack = session.Backpack.Get(slot);
            if (stack == null) { session.Toast(L.Get("toast.nothing_selected")); return; }

            session.Db.TryGetItem(stack.ItemId, out var item);
            var name = item != null ? L.Get(item.NameKey) : stack.ItemId;
            var count = stack.Count;
            if (session.ShipSlot(slot)) session.Toast(L.Get("toast.shipped", count, name));
            else session.Toast(L.Get("toast.cannot_ship", name));
        }
    }

    // Opens the shop UI. M1 has no town yet, so the general store stall stands on the farm (replaced in M2).
    public sealed class ShopCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] string _shopId = "general";

        public void Interact(PlayerActions player)
        {
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowShop(_shopId);
        }
    }

    public sealed class Bed : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerActions player)
        {
            if (!ServiceLocator.TryGet<IUiService>(out var ui)) { player.Session.StartSleep(false); return; }
            ui.ShowConfirm("confirm.sleep", () => player.Session.StartSleep(false));
        }
    }
}
