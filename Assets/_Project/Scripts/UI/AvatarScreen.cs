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
        sealed class Swatch { public Button Button; public GameObject Mark; public string Hex; public int Part; }

        static readonly string[] TurnOrder = { AvatarComposer.DownFacing, AvatarComposer.LeftFacing, AvatarComposer.UpFacing, AvatarComposer.RightFacing };

        readonly Image _preview;
        readonly List<Swatch> _skin = new List<Swatch>(), _hair = new List<Swatch>(), _shirt = new List<Swatch>(), _pants = new List<Swatch>(), _accessory = new List<Swatch>();
        readonly List<(string id, Button button)> _presets = new List<(string, Button)>();
        readonly List<(string id, Button button)> _builds = new List<(string, Button)>();
        readonly Dictionary<string, TextMeshProUGUI> _styleNames = new Dictionary<string, TextMeshProUGUI>();
        // The colour sliders edit one part at a time (0 skin, 1 hair, 2 shirt, 3 pants, 4 accessory); the swatches are presets for them.
        static readonly string[] PartKeys = { "avatar.skin", "avatar.hair", "avatar.shirt", "avatar.pants", "avatar.accessory" };
        readonly Slider[] _rgb = new Slider[3];
        readonly TextMeshProUGUI[] _rgbText = new TextMeshProUGUI[3];
        readonly Button[] _targets = new Button[5];
        int _target = 1;
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

            StyleRow(right.transform, "avatar.hair", "hair", AvatarOptions.Hairs, () => _look.Hair, s => _look.Hair = s);
            StyleRow(right.transform, "avatar.shirt", "shirt", AvatarOptions.Shirts, () => _look.Shirt, s => _look.Shirt = s);
            StyleRow(right.transform, "avatar.pants", "pants", AvatarOptions.Pants, () => _look.Pants, s => _look.Pants = s);
            StyleRow(right.transform, "avatar.accessory", "accessory", AvatarOptions.Accessories, () => _look.Accessory, s => _look.Accessory = s);
            ColourSliders(right.transform);
            root.SetActive(false);
        }

        string PartColour(int part) => part == 0 ? _look.Skin : part == 1 ? _look.HairColor : part == 2 ? _look.ShirtColor : part == 3 ? _look.PantsColor : _look.AccessoryColor;

        void SetPartColour(int part, string hex)
        {
            if (part == 0) _look.Skin = hex; else if (part == 1) _look.HairColor = hex; else if (part == 2) _look.ShirtColor = hex;
            else if (part == 3) _look.PantsColor = hex; else _look.AccessoryColor = hex;
        }

        // Red, green and blue sliders for the chosen part, with buttons to pick the part. The swatches above set the sliders to their colour.
        void ColourSliders(Transform parent)
        {
            var pick = Labelled(parent, "avatar.custom");
            for (var i = 0; i < PartKeys.Length; i++)
            {
                var part = i;
                _targets[i] = UiKit.MakeButton(pick, L.Get(PartKeys[i]), () => { _target = part; Refresh(); }, 100f, 28f);
                _targets[i].name = "Custom_" + part;
            }
            // The presets for the chosen part (only that part's are shown); a click sets the sliders to the colour.
            var strip = UiKit.HStack(parent, "Presets_Colour", 4f);
            UiKit.Size(strip.gameObject, -1f, 28f);
            SwatchRow(strip.transform, AvatarOptions.SkinColors, _skin, hex => _look.Skin = hex, "Skin", 0);
            SwatchRow(strip.transform, AvatarOptions.HairColors, _hair, hex => _look.HairColor = hex, "hair", 1);
            SwatchRow(strip.transform, AvatarOptions.ShirtColors, _shirt, hex => _look.ShirtColor = hex, "shirt", 2);
            SwatchRow(strip.transform, AvatarOptions.PantsColors, _pants, hex => _look.PantsColor = hex, "pants", 3);
            SwatchRow(strip.transform, AvatarOptions.AccessoryColors, _accessory, hex => _look.AccessoryColor = hex, "accessory", 4);
            var row = UiKit.HStack(parent, "Rgb", 8f);
            UiKit.Size(row.gameObject, -1f, 32f);
            var keys = new[] { "avatar.color.r", "avatar.color.g", "avatar.color.b" };
            for (var i = 0; i < 3; i++)
            {
                var channel = i;
                UiKit.Size(UiKit.Label(row.transform, L.Get(keys[i]), 17f, TextAlignmentOptions.Left, UiKit.Accent).gameObject, 20f, 30f).minWidth = 20f;
                var slider = UiKit.MakeSlider(row.transform, 0f, null, 150f);
                slider.maxValue = 255f;
                slider.wholeNumbers = true;
                slider.name = "Rgb_" + "RGB"[i];
                slider.onValueChanged.AddListener(_ => { SetPartColour(_target, SlidersHex()); Refresh(); });
                _rgb[i] = slider;
                _rgbText[i] = UiKit.Label(row.transform, string.Empty, 16f, TextAlignmentOptions.Right);
                UiKit.Size(_rgbText[i].gameObject, 36f, 30f).minWidth = 36f;
            }
        }

        string SlidersHex() => $"#{(int)_rgb[0].value:x2}{(int)_rgb[1].value:x2}{(int)_rgb[2].value:x2}";

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

        void StyleRow(Transform parent, string labelKey, string category, string[] styles, Func<string> get, Action<string> set)
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
        }

        void SwatchRow(Transform row, string[] colours, List<Swatch> into, Action<string> set, string prefix, int part)
        {
            foreach (var hex in colours)
            {
                var colour = hex;
                var button = UiKit.MakeSwatch(row, AvatarComposer.ParseColor(hex, default), () => { set(colour); _target = part; Refresh(); }, out var mark);
                button.name = $"{prefix}_{hex.Substring(1)}";
                into.Add(new Swatch { Button = button, Mark = mark, Hex = hex, Part = part });
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
            var worn = _look.Accessory != "none";
            foreach (var s in _accessory) s.Button.interactable = worn;
            // The sliders show the chosen part's colour (without notifying, so they do not write it back).
            var c = AvatarComposer.ParseColor(PartColour(_target), default);
            var values = new[] { c.r, c.g, c.b };
            for (var i = 0; i < 3; i++)
            {
                _rgb[i].SetValueWithoutNotify(values[i]);
                _rgb[i].interactable = _target != 4 || worn;
                _rgbText[i].text = values[i].ToString();
            }
            for (var i = 0; i < _targets.Length; i++) Highlight(_targets[i], i == _target);
            foreach (var list in new[] { _skin, _hair, _shirt, _pants, _accessory })
                foreach (var sw in list) sw.Button.gameObject.SetActive(sw.Part == _target);
        }
    }
}
