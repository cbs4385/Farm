using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // Mouse drag and drop for backpack slots. Click-to-pick and click-to-place still work for keyboard and gamepad;
    // this adds dragging a stack onto another slot (swap or merge). The screen owns the drag icon and the move.
    public sealed class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        InventoryScreen _screen;
        int _index;

        public void Bind(InventoryScreen screen, int index)
        {
            _screen = screen;
            _index = index;
        }

        public void OnBeginDrag(PointerEventData eventData) => _screen?.BeginDrag(_index, eventData);
        public void OnDrag(PointerEventData eventData) => _screen?.UpdateDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => _screen?.EndDrag();
        public void OnDrop(PointerEventData eventData) => _screen?.DropOn(_index);
    }
}
