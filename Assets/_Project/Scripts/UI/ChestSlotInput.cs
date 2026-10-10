using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // What the mouse can do with one slot of the chest screen besides what its Button does (a left click or the confirm button moves the stack): a right-click
    // moves one item, and the stack can be dragged onto another slot. The screen does the moving.
    public sealed class ChestSlotInput : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        ChestScreen _screen;

        public bool ChestSide { get; private set; }
        public int Slot { get; private set; }

        public void Bind(ChestScreen screen, bool chestSide, int slot)
        {
            _screen = screen;
            ChestSide = chestSide;
            Slot = slot;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.dragging && eventData.button == PointerEventData.InputButton.Right) _screen?.Click(ChestSide, Slot, eventData.button);
        }

        public void OnPointerEnter(PointerEventData eventData) => _screen?.ShowTip(ChestSide, Slot, eventData.position);
        public void OnPointerExit(PointerEventData eventData) => _screen?.HideTip();
        public void OnSelect(BaseEventData eventData) => _screen?.ShowTip(ChestSide, Slot, RectTransformUtility.WorldToScreenPoint(null, transform.position));
        public void OnDeselect(BaseEventData eventData) => _screen?.HideTip();
        public void OnBeginDrag(PointerEventData eventData) => _screen?.BeginDrag(ChestSide, Slot, eventData);
        public void OnDrag(PointerEventData eventData) => _screen?.UpdateDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => _screen?.EndDrag();
        public void OnDrop(PointerEventData eventData) => _screen?.DropOn(ChestSide, Slot);
    }
}
