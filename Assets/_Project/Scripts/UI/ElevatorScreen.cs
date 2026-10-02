using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // The mine's elevator: floor 1 and every fifth floor the player has reached.
    public sealed class ElevatorScreen : UiScreen
    {
        readonly RectTransform _list;

        public ElevatorScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Elevator", new Vector2(420f, 400f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 14);
            UiKit.Stretch((RectTransform)stack.transform);
            UiKit.Label(stack.transform, L.Get("mine.elevator"), 24f, TextAlignmentOptions.Left, UiKit.Accent);
            var list = UiKit.VStack(stack.transform, "List", 4f);
            UiKit.Size(list.gameObject, -1f, -1f, 1f, 1f);
            _list = (RectTransform)list.transform;
            UiKit.MakeButton(stack.transform, L.Get("ui.close"), Close, 160f, 34f).name = "Close";
            root.SetActive(false);
        }

        public void OpenElevator()
        {
            UiKit.ClearChildren(_list);
            foreach (var floor in MineGenerator.ElevatorFloors(Ui.Session.State.Mine.Deepest))
            {
                var f = floor;
                UiKit.MakeButton(_list, L.Get("mine.floor_button", floor), () => { Close(); MineTravel.GoToFloor(Ui.Session, f); }, 300f, 32f).name = "Floor" + floor;
            }
            Open();
        }
    }
}
