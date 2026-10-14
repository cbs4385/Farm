using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using TMPro;
using UnityEngine;

namespace Farm.UI
{
    // The accessibility choices for text (owner, 2026-10-14): the font (any font installed on the computer) and the colour of text. The choice lives in the settings
    // (SettingsData.FontName, TextColorIndex). The font is set on each label as it is built (UiKit.Font) and on every label already built when the option changes; the colour is
    // applied to every label in the HUD and the windows (not dark text or warnings) and kept up as the game recolours them. A font that cannot be made
    // leaves the game's own.
    public static class UiTextStyle
    {
        // "RRGGBB" to a colour, or null when empty or not a colour (the game's own).
        public static Color? Parse(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length != 6 || !int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var v)) return null;
            return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f);
        }

        public static string ToHex(Color c) => ((int)Mathf.Round(c.r * 255) << 16 | (int)Mathf.Round(c.g * 255) << 8 | (int)Mathf.Round(c.b * 255)).ToString("X6");

        static string[] _installed;
        static readonly Dictionary<string, TMP_FontAsset> Made = new Dictionary<string, TMP_FontAsset>();

        // "" (the game's font) followed by the installed fonts, in order.
        public static string[] FontChoices()
        {
            if (_installed == null)
            {
                string[] names;
                try { names = Font.GetOSInstalledFontNames() ?? new string[0]; }
                catch (Exception) { names = new string[0]; }
                _installed = new[] { string.Empty }.Concat(names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase)).ToArray();
            }
            return _installed;
        }

        static TMP_FontAsset FontFor(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Made.TryGetValue(name, out var cached)) return cached;
            TMP_FontAsset asset = null;
            try
            {
                asset = TMP_FontAsset.CreateFontAsset(name, "Regular", 90);
                if (asset != null && TMP_Settings.defaultFontAsset != null)
                    asset.fallbackFontAssetTable = new List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };          // glyphs the chosen font lacks come from the game's font
            }
            catch (Exception e) { Debug.LogWarning("[UiTextStyle] font '" + name + "' could not be made: " + e.Message); }
            Made[name] = asset;
            return asset;
        }

        // Sets the font and the text colour, and restyles every label already built.
        public static void Apply(SettingsData settings, params Transform[] roots)
        {
            var oldFont = UiKit.Font;
            UiKit.Font = FontFor(settings.FontName);
            _title = Parse(settings.TitleColor);
            _body = Parse(settings.BodyColor);
            foreach (var root in roots)
            {
                if (root == null) continue;
                RestyleFont(root, oldFont);
                Recolor(root);
            }
            if (_title == null && _body == null) RestoreColors();
        }

        // Labels built outside UiKit.Label (or before a change) catch up when a screen opens.
        public static void Refresh(Transform root)
        {
            if (root == null) return;
            if (UiKit.Font != null) RestyleFont(root, null);
            if (_title != null || _body != null) Recolor(root);
        }

        // The game changes label colours as it runs (the clock, the bars, warnings), so with a colour chosen every label is recoloured again a few times a second: `Tick` from the UI's Update.
        public static void Tick(params Transform[] roots)
        {
            if ((_title == null && _body == null) || Time.unscaledTime < _nextTick) return;
            _nextTick = Time.unscaledTime + 0.2f;
            if (Recoloured.Count > 400) foreach (var gone in Recoloured.Keys.Where(k => k == null).ToList()) Recoloured.Remove(gone);
            foreach (var root in roots) if (root != null) Recolor(root);
        }

        static Color? _title, _body;
        static float _nextTick;
        static readonly List<TMP_Text> Scratch = new List<TMP_Text>();
        // What each label was before we coloured it, and what we made it (so a colour the game sets in the meantime is noticed and kept as the new original).
        static readonly Dictionary<TMP_Text, (Color original, Color applied)> Recoloured = new Dictionary<TMP_Text, (Color, Color)>();

        // Dark text (the ink on pale panels) and warnings (red, or the colour-blind palette's orange for the same job) keep their colour: the chosen colour is for ordinary text.
        static bool IsDark(Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b)) < 0.3f;
        static bool IsWarning(Color c) => SameRgb(c, UiKit.Danger) || SameRgb(c, UiPalette.Low) || (c.r > 0.7f && c.g < 0.45f && c.b < 0.45f);
        static bool Exempt(Color c) => IsDark(c) || IsWarning(c);

        // Titles and headings are the game's gold; everything else is ordinary text.
        static bool IsTitle(Color c) => SameRgb(c, UiKit.Accent);

        static void Recolor(Transform root)
        {
            if (_title == null && _body == null) return;
            root.GetComponentsInChildren(true, Scratch);
            foreach (var text in Scratch)
            {
                var now = text.color;
                var tracked = Recoloured.TryGetValue(text, out var seen);
                var original = tracked && SameRgb(now, seen.applied) ? seen.original : now;          // a colour the game gave it since is the new original
                if (Exempt(original)) { Recoloured.Remove(text); continue; }
                var target = IsTitle(original) ? _title : _body;
                if (target == null)                                                                   // this kind keeps the game's colour
                {
                    if (tracked) { if (SameRgb(now, seen.applied)) text.color = new Color(original.r, original.g, original.b, now.a); Recoloured.Remove(text); }
                    continue;
                }
                var chosen = new Color(target.Value.r, target.Value.g, target.Value.b, now.a);
                if (tracked && SameRgb(now, chosen) && SameRgb(seen.original, original)) { Recoloured[text] = (original, chosen); continue; }
                Recoloured[text] = (original, chosen);
                text.color = chosen;
            }
            Scratch.Clear();
        }

        static void RestoreColors()
        {
            foreach (var pair in Recoloured)
                if (pair.Key != null && SameRgb(pair.Key.color, pair.Value.applied)) pair.Key.color = new Color(pair.Value.original.r, pair.Value.original.g, pair.Value.original.b, pair.Key.color.a);
            Recoloured.Clear();
        }

        static void RestyleFont(Transform root, TMP_FontAsset oldFont)
        {
            var wanted = UiKit.Font != null ? UiKit.Font : TMP_Settings.defaultFontAsset;
            if (wanted == null) return;
            root.GetComponentsInChildren(true, Scratch);
            foreach (var text in Scratch)
                if (text.font != wanted && (UiKit.Font != null || text.font == oldFont)) text.font = wanted;
            Scratch.Clear();
        }

        static bool SameRgb(Color a, Color b) => Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
    }
}
