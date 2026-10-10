using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // A right-click on a slot of a window (backpack, shipping bin, shop sale) means "one item"; the screen decides what that does.
    public sealed class SlotRightClick : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnRightClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.dragging && eventData.button == PointerEventData.InputButton.Right) OnRightClick?.Invoke();
        }
    }
}
