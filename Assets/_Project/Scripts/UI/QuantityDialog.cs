using System;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // What the "how many?" window is asking about.
    public sealed class QuantityRequest
    {
        public string Title;            // "Sell how many?"
        public string ItemName;
        public Sprite Icon;
        public int Min = 1;             // 0 lets a player take a stack out of a lot
        public int Max = 1;
        public int Initial = 1;
        public int UnitValue;           // gold for one (0: no total is shown)
        public string TotalKey = "quantity.total_worth";          // "{0}" is the total
    }

    // A window with a number box and buttons around it (-All, -5, -1, the number, +1, +5, +All) that opens when a stack is chosen to sell, ship or buy, so that the amount
    // is set in one go instead of many clicks (playtest 2026-10-16). The number can also be typed. It can never go above `Max` (a stack given away never exceeds what the
    // backpack holds) or below `Min`. Every control is a Button or a field, so mouse, keyboard and gamepad all work.
    public sealed class QuantityDialog : UiScreen
    {
        readonly TextMeshProUGUI _title, _name, _total, _status;
        readonly Image _icon;
        readonly TMP_InputField _input;
        readonly Button _ok;
        QuantityRequest _request;
        Action<int> _onConfirm;
        int _value;
        bool _writing;

        public int Value => _value;
        public int Max => _request?.Max ?? 0;
        public override GameObject InitialFocus => _ok != null ? _ok.gameObject : null;

        public QuantityDialog(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Quantity", new Vector2(600f, 300f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 8f, 16, TextAnchor.UpperCenter);
            UiKit.Stretch((RectTransform)stack.transform);

            _title = UiKit.Label(stack.transform, string.Empty, 22f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Size(_title.gameObject, -1f, 30f);

            var who = UiKit.HStack(stack.transform, "Item", 8f, TextAnchor.MiddleCenter);
            UiKit.Size(who.gameObject, -1f, 40f);
            _icon = UiKit.Panel(who.transform, "Icon", Color.white);
            _icon.preserveAspect = true;
            UiKit.Size(_icon.gameObject, 36f, 36f);
            UiTip.HasOwn(_icon.gameObject);          // the item's name is written beside it
            _name = UiKit.Label(who.transform, string.Empty, 19f, TextAlignmentOptions.Left);
            UiKit.Size(_name.gameObject, 320f, 36f);

            var row = UiKit.HStack(stack.transform, "Amount", 6f, TextAnchor.MiddleCenter);
            UiKit.Size(row.gameObject, -1f, 38f);
            Step(row.transform, L.Get("quantity.minus_all"), () => Set(Min));
            Step(row.transform, "-5", () => Set(_value - 5));
            Step(row.transform, "-1", () => Set(_value - 1));
            _input = UiKit.MakeInput(row.transform, "0", "1", 7, 96f);
            _input.name = "Number";
            _input.contentType = TMP_InputField.ContentType.IntegerNumber;          // digits only
            _input.onValueChanged.AddListener(OnTyped);
            _input.onEndEdit.AddListener(_ => Write());
            _input.onSubmit.AddListener(_ => Confirm());
            Step(row.transform, "+1", () => Set(_value + 1));
            Step(row.transform, "+5", () => Set(_value + 5));
            Step(row.transform, L.Get("quantity.plus_all"), () => Set(Max));

            _total = UiKit.Label(stack.transform, string.Empty, 22f, TextAlignmentOptions.Center, UiKit.Accent);
            _total.fontStyle = FontStyles.Bold;
            UiKit.Size(_total.gameObject, -1f, 32f);
            _status = UiKit.Label(stack.transform, string.Empty, 15f, TextAlignmentOptions.Center, UiKit.DimText);
            UiKit.Size(_status.gameObject, -1f, 22f);

            var buttons = UiKit.PinnedRow(frame, 12f, 360f);
            _ok = UiKit.MakeButton(buttons, L.Get("ui.ok"), Confirm, 160f, 38f);
            _ok.name = "Ok";
            UiKit.MakeButton(buttons, L.Get("ui.cancel"), Close, 160f, 38f).name = "Cancel";
            root.SetActive(false);
        }

        int Min => _request?.Min ?? 0;

        static Button Step(Transform parent, string text, UnityEngine.Events.UnityAction action)
        {
            var button = UiKit.MakeButton(parent, text, action, 72f, 36f);
            button.name = "Quantity_" + text;
            return button;
        }

        public void Ask(QuantityRequest request, Action<int> onConfirm)
        {
            if (IsOpen) Close();
            _request = request;
            _onConfirm = onConfirm;
            if (_request.Max < _request.Min) _request.Max = _request.Min;
            _title.text = request.Title;
            _name.text = request.ItemName;
            _icon.sprite = request.Icon;
            _icon.enabled = request.Icon != null;
            _status.text = string.Empty;
            Set(request.Initial);
            Open();
        }

        // Sets the number (kept between the smallest and the biggest allowed) and redraws.
        public void Set(int value)
        {
            if (_request == null) return;
            var clamped = QuantityPicker.Clamp(value, _request.Min, _request.Max);
            _status.text = value > _request.Max ? L.Get("quantity.limit", _request.Max) : string.Empty;
            _value = clamped;
            Write();
            Refresh();
        }

        // What is typed is read as a number and kept in range as it is typed.
        void OnTyped(string text)
        {
            if (_writing || _request == null) return;
            var typed = QuantityPicker.Parse(text, _request.Min, int.MaxValue);
            var clamped = QuantityPicker.Clamp(typed, _request.Min, _request.Max);
            _status.text = typed > _request.Max ? L.Get("quantity.limit", _request.Max) : string.Empty;
            _value = clamped;
            if (clamped != typed) Write();          // too many: the box shows what is allowed at once
            Refresh();
        }

        void Write()
        {
            _writing = true;
            _input.text = _value.ToString();
            _writing = false;
        }

        void Refresh()
        {
            var unit = _request?.UnitValue ?? 0;
            _total.text = unit > 0 ? L.Get(_request.TotalKey, unit * _value) : string.Empty;
            _ok.interactable = _request != null && _value >= Math.Max(0, _request.Min) && (_value > 0 || _request.Min == 0);
        }

        // Fills in and accepts without clicking (tests and tools).
        public void Submit(int value) { Set(value); Confirm(); }

        public void Confirm()
        {
            if (_request == null || !_ok.interactable) return;
            var callback = _onConfirm;
            var amount = _value;
            _onConfirm = null;
            Close();
            callback?.Invoke(amount);
        }

        public override void OnCancel() => Close();

        public override void Close()
        {
            _onConfirm = null;
            base.Close();
        }
    }
}
