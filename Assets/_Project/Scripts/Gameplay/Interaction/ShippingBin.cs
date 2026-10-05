using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Ships the stack selected on the hotbar. Placed on the farm next to the house.
    public sealed class ShippingBin : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.shipping_bin");

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
}
