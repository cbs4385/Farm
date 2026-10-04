using Farm.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // A very small tick when the pointer moves onto a button (added to every button by UiKit.MakeButton).
    public sealed class UiHoverSound : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData) => AudioService.PlayIfAvailable(Sfx.Hover, 0.35f);
    }
}
