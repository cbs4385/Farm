using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The shipping bin next to the house: opens the shipping window. Goods put in are paid for overnight.
    public sealed class ShippingBin : MonoBehaviour, IInteractable
    {
        public int Reach => 1;
        public string HoverLabel => Farm.Core.L.Get("hover.shipping_bin");

        public void Interact(PlayerActions player)
        {
            // Normally the bin opens as a window (pick items and amounts, ship them as one lot); without a UI the selected stack goes in.
            if (ServiceLocator.TryGet<IUiService>(out var ui) && ui != null) { ui.ShowShipping(); return; }
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
