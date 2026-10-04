using System;

namespace Farm.Gameplay
{
    // The fixed words dialogue data may use for the optional presentation fields (T-095). The validator checks them, the
    // dialogue UI maps them to portraits, bubbles and icons, and FScript uses the same names.
    public static class DialogueVocabulary
    {
        public static readonly string[] Expressions = Farm.Data.NpcDefinition.ExpressionNames;
        public static readonly string[] Emotes = { "heart", "note", "sweat", "exclaim", "question", "ellipsis", "sparkle", "zzz" };
        public static readonly string[] Cameras = { "speaker", "player", "wide" };
        public static readonly string[] Tones = { "kind", "honest", "playful", "shy", "curt" };
        public static readonly string[] MomentTags = { "funny", "wholesome", "surprise", "mystery" };

        public static bool Has(string[] list, string value) => string.IsNullOrEmpty(value) || Array.IndexOf(list, value) >= 0;

        // Placeholder glyphs until the emote sprites exist (T-130).
        public static string EmoteGlyph(string emote)
        {
            switch (emote)
            {
                case "heart": return "<3";
                case "note": return "~";
                case "sweat": return ";;";
                case "exclaim": return "!";
                case "question": return "?";
                case "ellipsis": return "...";
                case "sparkle": return "*";
                case "zzz": return "Zz";
                default: return null;
            }
        }
    }
}
