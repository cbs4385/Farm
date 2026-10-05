using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The farmer creator (playtest request, 2026-10-05): a preview that turns around, eight ready-made looks, the body build, and for skin, hair,
    // shirt, pants and accessory a colour swatch row (and, for all but skin, a style to step through). Every control is a Button, so mouse,
    // keyboard and gamepad all work.
    public sealed class AvatarScreen : UiScreen
    {
        sealed class Swatch { public Button Button; public GameObject Mark; public string Hex; }

        static readonly string[] TurnOrder = { AvatarComposer.DownFacing, AvatarComposer.LeftFacing, AvatarComposer.UpFacing, AvatarComposer.RightFacing };

        readonly Image _preview;
        readonly List<Swatch> _skin = new List<Swatch>(), _hair = new List<Swatch>(), _shirt = new List<Swatch>(), _pants = new List<Swatch>(), _accessory = new List<Swatch>();
        readonly List<(string id, Button button)> _presets = new List<(string, Button)>();
        readonly List<(string id, Button button)> _builds = new List<(string, Button)>();
        readonly Dictionary<string, TextMeshProUGUI> _styleNames = new Dictionary<string, TextMeshProUGUI>();
        AvatarData _look = AvatarOptions.Default();
        Action<AvatarData> _onDone;
        int _facing;
        int _randoms;

        public AvatarData Look => _look;
        public string Facing => TurnOrder[_facing];

        public AvatarScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Avatar", new Vector2(900f, 500f), out var root);
            Root = root;
            var row = UiKit.HStack(frame, "Columns", 14f, TextAnchor.UpperLeft);
            UiKit.Stretch((RectTransform)row.transform, 14f);
            row.childForceExpandHeight = true;

            // ---- left: the preview and the buttons under it
            var left = UiKit.VStack(row.transform, "Left", 8f, 0, TextAnchor.UpperCenter);
            UiKit.Size(left.gameObject, 220f, -1f);
            UiKit.Label(left.transform, L.Get("avatar.title"), 26f, TextAlignmentOptions.Center, UiKit.Accent);
            var stage = UiKit.Panel(left.transform, "Stage", UiKit.PanelLight);
            UiKit.Size(stage.gameObject, 200f, 250f);
            _preview = UiKit.Panel(stage.transform, "Preview", Color.white);
            _preview.preserveAspect = true;
            _preview.raycastTarget = false;
            UiKit.Place(_preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(112f, 224f), Vector2.zero);
            UiKit.MakeButton(left.transform, L.Get("avatar.turn"), Turn, 200f, 32f).name = "Turn";
            UiKit.MakeButton(left.transform, L.Get("avatar.randomize"), Randomize, 200f, 32f).name = "Randomize";
            UiKit.MakeButton(left.transform, L.Get("avatar.done"), Done, 200f, 38f).name = "Done";

            // ---- right: the choices
            var right = UiKit.VStack(row.transform, "Right", 8f, 0);
            UiKit.Size(right.gameObject, -1f, -1f, 1f, 1f);

            UiKit.Label(right.transform, L.Get("avatar.presets"), 16f, TextAlignmentOptions.Left, UiKit.DimText);
            var presetRow = UiKit.HStack(right.transform, "Presets", 4f);
            foreach (var (id, look) in AvatarOptions.Presets)
            {
                var preset = id; var value = look;
                var b = UiKit.MakeButton(presetRow.transform, L.Get("avatar.preset." + id), () => { _look = value.Clone(); Refresh(); }, 78f, 30f);
                b.name = "Preset_" + id;
                _presets.Add((id, b));
            }

            var bodyRow = Labelled(right.transform, "avatar.body");
            foreach (var build in AvatarOptions.Builds)
            {
                var id = build;
                var b = UiKit.MakeButton(bodyRow, L.Get("avatar.build." + id), () => { _look.Build = id; Refresh(); }, 120f, 30f);
                b.name = "Build_" + id;
                _builds.Add((id, b));
            }

            SwatchRow(Labelled(right.transform, "avatar.skin"), AvatarOptions.SkinColors, _skin, hex => _look.Skin = hex, "Skin");
            StyleRow(right.transform, "avatar.hair", "hair", AvatarOptions.Hairs, () => _look.Hair, s => _look.Hair = s, AvatarOptions.HairColors, _hair, hex => _look.HairColor = hex);
            StyleRow(right.transform, "avatar.shirt", "shirt", AvatarOptions.Shirts, () => _look.Shirt, s => _look.Shirt = s, AvatarOptions.ShirtColors, _shirt, hex => _look.ShirtColor = hex);
            StyleRow(right.transform, "avatar.pants", "pants", AvatarOptions.Pants, () => _look.Pants, s => _look.Pants = s, AvatarOptions.PantsColors, _pants, hex => _look.PantsColor = hex);
            StyleRow(right.transform, "avatar.accessory", "accessory", AvatarOptions.Accessories, () => _look.Accessory, s => _look.Accessory = s, AvatarOptions.AccessoryColors, _accessory, hex => _look.AccessoryColor = hex);
            root.SetActive(false);
        }

        // A row with a name on the left; the controls go into the returned parent.
        static Transform Labelled(Transform parent, string labelKey)
        {
            var row = UiKit.HStack(parent, "Row_" + labelKey, 8f);
            UiKit.Size(row.gameObject, -1f, 32f);
            var label = UiKit.Label(row.transform, L.Get(labelKey), 17f, TextAlignmentOptions.Left, UiKit.Accent);
            var size = UiKit.Size(label.gameObject, 92f, 30f);
            size.minWidth = 92f;
            return row.transform;
        }

        void StyleRow(Transform parent, string labelKey, string category, string[] styles, Func<string> get, Action<string> set, string[] colours, List<Swatch> swatches, Action<string> setColour)
        {
            var row = Labelled(parent, labelKey);
            void Step(int delta)
            {
                var i = Array.IndexOf(styles, get());
                set(styles[((i + delta) % styles.Length + styles.Length) % styles.Length]);
                Refresh();
            }
            UiKit.MakeButton(row, L.Get("avatar.prev"), () => Step(-1), 30f, 30f).name = category + "_prev";
            var name = UiKit.Label(row, "", 16f, TextAlignmentOptions.Center);
            UiKit.Size(name.gameObject, 112f, 30f).minWidth = 112f;
            _styleNames[category] = name;
            UiKit.MakeButton(row, L.Get("avatar.next"), () => Step(1), 30f, 30f).name = category + "_next";
            SwatchRow(row, colours, swatches, setColour, category);
        }

        void SwatchRow(Transform row, string[] colours, List<Swatch> into, Action<string> set, string prefix)
        {
            foreach (var hex in colours)
            {
                var colour = hex;
                var button = UiKit.MakeButton(row, "", () => { set(colour); Refresh(); }, 24f, 24f);
                button.name = $"{prefix}_{hex.Substring(1)}";
                var mark = UiKit.Panel(button.transform, "Mark", UiKit.Accent);
                UiKit.Stretch(mark.rectTransform);
                mark.raycastTarget = false;
                var fill = UiKit.Panel(button.transform, "Fill", new Color32(AvatarComposer.ParseColor(hex, default).r, AvatarComposer.ParseColor(hex, default).g, AvatarComposer.ParseColor(hex, default).b, 255));
                UiKit.Stretch(fill.rectTransform, 3f);
                fill.raycastTarget = false;
                into.Add(new Swatch { Button = button, Mark = mark.gameObject, Hex = hex });
            }
        }

        public void OpenWith(AvatarData look, Action<AvatarData> onDone)
        {
            _look = AvatarOptions.Sanitize(look);
            _onDone = onDone;
            _facing = 0;
            Refresh();
            Open();
        }

        void Turn() { _facing = (_facing + 1) % TurnOrder.Length; Refresh(); }
        void Randomize() { _look = AvatarOptions.Random(Environment.TickCount + _randoms++ * 7919); Refresh(); }

        void Done()
        {
            var result = _look.Clone();
            Close();
            _onDone?.Invoke(result);
        }

        // Cancelling (Escape) leaves the earlier look as it was.
        public override void OnCancel() => Close();

        void Mark(List<Swatch> swatches, string current)
        {
            foreach (var s in swatches) s.Mark.SetActive(string.Equals(s.Hex, current, StringComparison.OrdinalIgnoreCase));
        }

        static void Highlight(Button button, bool on)
        {
            var image = button.targetGraphic as Image;
            if (image != null) image.color = on ? UiKit.Accent : Color.white;
        }

        void Refresh()
        {
            _preview.sprite = AvatarSprites.For(_look).For(Facing);
            Mark(_skin, _look.Skin); Mark(_hair, _look.HairColor); Mark(_shirt, _look.ShirtColor); Mark(_pants, _look.PantsColor); Mark(_accessory, _look.AccessoryColor);
            _styleNames["hair"].text = L.Get("avatar.hair." + _look.Hair);
            _styleNames["shirt"].text = L.Get("avatar.shirt." + _look.Shirt);
            _styleNames["pants"].text = L.Get("avatar.pants." + _look.Pants);
            _styleNames["accessory"].text = L.Get("avatar.accessory." + _look.Accessory);
            foreach (var (id, button) in _builds) Highlight(button, id == _look.Build);
            for (var i = 0; i < _presets.Count; i++) Highlight(_presets[i].button, AvatarOptions.Presets[i].look.SameAs(_look));
            // Colours of an accessory that is not worn would change nothing.
            foreach (var s in _accessory) s.Button.interactable = _look.Accessory != "none";
        }
    }
}
