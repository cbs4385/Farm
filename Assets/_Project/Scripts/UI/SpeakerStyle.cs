using System.Collections.Generic;
using UnityEngine;

namespace Farm.UI
{
    // T-134: each villager's name plate and portrait frame carry their own accent, so a viewer can tell who is speaking at a glance.
    // Every colour is light enough to read on the dark dialogue panel (contrast checked in a test).
    public static class SpeakerStyle
    {
        static readonly Dictionary<string, Color> Accents = new Dictionary<string, Color>
        {
            { "wren", Hex(0xF2C14E) }, { "hazel", Hex(0x8FD18A) }, { "bram", Hex(0xE08A5C) },
            { "tilda", Hex(0xF29BB8) }, { "juno", Hex(0x6FD0D6) }, { "piper", Hex(0xC59BF2) },
            { "marcus", Hex(0x9DB8F2) }, { "odalys", Hex(0xF2E08A) }, { "felix", Hex(0x7FE0B0) },
            { "dorian", Hex(0xD6A3A3) }, { "elara", Hex(0xB7E36F) }, { "ione", Hex(0xA8C8E8) },
        };

        public static Color AccentFor(string npcId) =>
            !string.IsNullOrEmpty(npcId) && Accents.TryGetValue(npcId, out var c) ? c : UiKit.Accent;

        public static IEnumerable<string> Styled => Accents.Keys;

        static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
    }
}
