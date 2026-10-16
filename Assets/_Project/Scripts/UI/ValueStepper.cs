using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI
{
    // Left and right on the focused control change its value instead of moving the focus: a choice button cycles back and forward, a toggle goes off or on. (A slider already
    // does this itself.) The control's own navigation must have nothing to the left or right, so only this acts on those keys.
    public sealed class ValueStepper : MonoBehaviour, IMoveHandler
    {
        public Action<int> Step;

        public void OnMove(AxisEventData eventData)
        {
            if (Step == null) return;
            if (eventData.moveDir == MoveDirection.Left) { Step(-1); eventData.Use(); }
            else if (eventData.moveDir == MoveDirection.Right) { Step(1); eventData.Use(); }
        }
    }
}
