using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // How the player's farmer looks (playtest request, 2026-10-05): a body build, a style for hair, shirt, pants and accessory, and a colour for
    // skin, hair, shirt, pants and accessory. Styles are stable ids and colours are "#RRGGBB" strings, so the save does not depend on list order.
    [Serializable]
    public sealed class AvatarData
    {
        public string Build = "feminine";
        public string Hair = "long", Shirt = "tshirt", Pants = "trousers", Accessory = "sunhat";
        public string Skin = "#e8b48a", HairColor = "#6b4226", ShirtColor = "#3f8f9b", PantsColor = "#5a4a8c", AccessoryColor = "#e0a83a";

        public AvatarData Clone() => (AvatarData)MemberwiseClone();

        // One string that names this exact look (the sprite cache key).
        public string Key => string.Join("|", Build, Hair, Shirt, Pants, Accessory, Skin, HairColor, ShirtColor, PantsColor, AccessoryColor);

        public bool SameAs(AvatarData other) => other != null && Key == other.Key;
    }

    // What the player may choose from, and the starting looks.
    public static class AvatarOptions
    {
        public static readonly string[] Builds = { "feminine", "masculine" };
        public static readonly string[] Hairs = { "short", "parted", "long", "ponytail", "bun", "curly" };
        public static readonly string[] Shirts = { "tshirt", "longsleeve", "overalls", "vest", "dress" };
        public static readonly string[] Pants = { "trousers", "shorts", "skirt", "cuffed" };
        public static readonly string[] Accessories = { "none", "sunhat", "cap", "beanie", "glasses", "scarf", "flower" };

        public static readonly string[] SkinColors = { "#f6d5bd", "#e8b48a", "#d99a6c", "#c68642", "#a9693a", "#8d5524", "#6b3d1f", "#4a2a14" };
        public static readonly string[] HairColors = { "#1f1a17", "#3b2a20", "#6b4226", "#8a5a32", "#a8522a", "#c8602d", "#d9b45a", "#efd9a0", "#9a9a9a", "#e8e8e8", "#3f6fb5", "#c85a9a" };
        public static readonly string[] ShirtColors = { "#3f8f9b", "#c0453f", "#e0a83a", "#5f9e4f", "#4a6fb5", "#8a5aa8", "#e8e0d0", "#3a3a44", "#e08a5a", "#d98aa8", "#7a4a2e", "#9ad0c8" };
        public static readonly string[] PantsColors = { "#5a4a8c", "#3b4a6b", "#6b4a2e", "#3a3a44", "#7a8a5a", "#b09060", "#8a3a3a", "#4f7a6a", "#c8c8d0", "#d9b45a", "#1f2a44", "#5a3a5a" };
        public static readonly string[] AccessoryColors = { "#e0a83a", "#c0453f", "#3f8f9b", "#5f9e4f", "#4a6fb5", "#8a5aa8", "#e8e0d0", "#2b2b33", "#e08a5a", "#d98aa8", "#7a4a2e", "#f2d24a" };

        // Eight ready-made looks to start from.
        public static readonly (string id, AvatarData look)[] Presets =
        {
            ("meadow", Look("feminine", "long", "tshirt", "trousers", "sunhat", "#e8b48a", "#6b4226", "#3f8f9b", "#5a4a8c", "#e0a83a")),
            ("orchard", Look("masculine", "short", "overalls", "trousers", "cap", "#d99a6c", "#3b2a20", "#e8e0d0", "#3b4a6b", "#c0453f")),
            ("harbor", Look("masculine", "parted", "longsleeve", "cuffed", "beanie", "#f6d5bd", "#c8602d", "#4a6fb5", "#b09060", "#3f8f9b")),
            ("lantern", Look("feminine", "bun", "dress", "skirt", "flower", "#a9693a", "#1f1a17", "#c0453f", "#3a3a44", "#d98aa8")),
            ("hedgerow", Look("feminine", "ponytail", "vest", "shorts", "scarf", "#e8b48a", "#d9b45a", "#5f9e4f", "#3b4a6b", "#e8e0d0")),
            ("hearth", Look("masculine", "curly", "vest", "trousers", "glasses", "#8d5524", "#1f1a17", "#7a4a2e", "#7a8a5a", "#2b2b33")),
            ("willow", Look("feminine", "parted", "longsleeve", "skirt", "beanie", "#c68642", "#a8522a", "#9ad0c8", "#8a3a3a", "#e8e0d0")),
            ("ember", Look("masculine", "short", "tshirt", "cuffed", "none", "#6b3d1f", "#9a9a9a", "#e08a5a", "#3a3a44", "#e0a83a")),
        };

        static AvatarData Look(string build, string hair, string shirt, string pants, string accessory, string skin, string hairColor, string shirtColor, string pantsColor, string accessoryColor) =>
            new AvatarData { Build = build, Hair = hair, Shirt = shirt, Pants = pants, Accessory = accessory, Skin = skin, HairColor = hairColor, ShirtColor = shirtColor, PantsColor = pantsColor, AccessoryColor = accessoryColor };

        public static AvatarData Default() => Presets[0].look.Clone();

        static string Pick(string value, string[] allowed, string fallback) => Array.IndexOf(allowed, value) >= 0 ? value : fallback;
        static string PickColor(string value, string fallback) => IsHex(value) ? value.ToLowerInvariant() : fallback;

        public static bool IsHex(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 7 || s[0] != '#') return false;
            for (var i = 1; i < 7; i++) if (!Uri.IsHexDigit(s[i])) return false;
            return true;
        }

        // A look with anything unknown (a style that no longer exists, a bad colour, nothing at all) replaced by the starting look's choice.
        public static AvatarData Sanitize(AvatarData a)
        {
            var d = Default();
            if (a == null) return d;
            return new AvatarData
            {
                Build = Pick(a.Build, Builds, d.Build), Hair = Pick(a.Hair, Hairs, d.Hair), Shirt = Pick(a.Shirt, Shirts, d.Shirt),
                Pants = Pick(a.Pants, Pants, d.Pants), Accessory = Pick(a.Accessory, Accessories, d.Accessory),
                Skin = PickColor(a.Skin, d.Skin), HairColor = PickColor(a.HairColor, d.HairColor), ShirtColor = PickColor(a.ShirtColor, d.ShirtColor),
                PantsColor = PickColor(a.PantsColor, d.PantsColor), AccessoryColor = PickColor(a.AccessoryColor, d.AccessoryColor),
            };
        }

        // A random look for the same seed (the Randomize button).
        public static AvatarData Random(int seed)
        {
            var r = new System.Random(seed);
            string Of(string[] list) => list[r.Next(list.Length)];
            return new AvatarData
            {
                Build = Of(Builds), Hair = Of(Hairs), Shirt = Of(Shirts), Pants = Of(Pants), Accessory = Of(Accessories),
                Skin = Of(SkinColors), HairColor = Of(HairColors), ShirtColor = Of(ShirtColors), PantsColor = Of(PantsColors), AccessoryColor = Of(AccessoryColors),
            };
        }

        public static IEnumerable<string> StyleIds(string category)
        {
            switch (category)
            {
                case "hair": return Hairs;
                case "shirt": return Shirts;
                case "pants": return Pants;
                case "accessory": return Accessories;
                default: return Builds;
            }
        }
    }
}
