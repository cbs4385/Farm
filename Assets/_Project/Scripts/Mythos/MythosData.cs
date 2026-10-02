using System.Collections.Generic;
using System.Linq;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The layer's items, crops and relics as data tables: the editor tool writes them into a ContentPack
    // (Resources/Packs/mythos) and generates placeholder art from the same rows. None of it is sold by any shop (items
    // opt in to shops and these do not): seeds come from the woods, the Keepers and rituals.
    public readonly struct HorrorCropRow
    {
        public readonly string Id, GrowCondition;
        public readonly int[] Days;
        public readonly int SellPrice;
        public readonly Color Color;
        public readonly bool Mutant;      // a mutated ordinary crop: no seed, never planted by hand

        public HorrorCropRow(string id, int[] days, string growCondition, int sell, Color color, bool mutant = false)
        { Id = id; Days = days; GrowCondition = growCondition; SellPrice = sell; Color = color; Mutant = mutant; }
    }

    public static class MythosData
    {
        static Color C(float r, float g, float b) => new Color(r, g, b);

        public const string RelicSeal = "mythos.relic_seal", RelicBell = "mythos.relic_bell", RelicThread = "mythos.relic_thread";
        public static readonly string[] Relics = { RelicSeal, RelicBell, RelicThread };

        public static readonly HorrorCropRow[] Crops =
        {
            // Grow only while the player's dread is high enough; otherwise they lie dormant.
            new HorrorCropRow("nightbloom", new[] { 2, 2, 2, 2 }, "var:dread>=20", 400, C(0.45f, 0.35f, 0.75f)),
            new HorrorCropRow("hollowroot", new[] { 2, 3, 3, 3, 2 }, "var:dread>=40", 700, C(0.25f, 0.30f, 0.20f)),
            new HorrorCropRow("ashfruit", new[] { 3, 3, 3, 3, 3 }, "var:dread>=60", 1200, C(0.85f, 0.35f, 0.15f)),
            // What ordinary crops become as dread rises (see MythosMutation).
            new HorrorCropRow("mutant_root", new[] { 1, 1, 1, 1 }, null, 90, C(0.70f, 0.55f, 0.60f), mutant: true),
            new HorrorCropRow("mutant_gourd", new[] { 1, 1, 1, 1 }, null, 110, C(0.65f, 0.70f, 0.45f), mutant: true),
            new HorrorCropRow("mutant_leaf", new[] { 1, 1, 1, 1 }, null, 70, C(0.45f, 0.65f, 0.60f), mutant: true),
        };

        public static IEnumerable<string> MutantIds => Crops.Where(c => c.Mutant).Select(c => c.Id);

        // The mutant an ordinary crop of this kind turns into.
        public static string MutantFor(CropKind kind)
        {
            switch (kind)
            {
                case CropKind.Fruit: return "mutant_gourd";
                case CropKind.Vegetable: return "mutant_root";
                default: return "mutant_leaf";
            }
        }
    }

    // Crops change in place overnight as dread rises (X-007): "mild and optional, never destroying progress": a mature crop
    // becomes a strange but valuable cousin, at a few percent a night, never in the greenhouse and never at intensity 0.
    public static class MythosMutation
    {
        public static float Chance(int dread, float scale) => dread < 20 ? 0f : (dread - 20) / 80f * 0.05f * scale;

        public static int Mutate(GameSession s, int today)
        {
            var scale = MythosLevel.Scale(s);
            if (scale <= 0f) return 0;
            var chance = Chance(s.GetVar(MythosIds.Vars.Dread), scale);
            if (chance <= 0f) return 0;
            var changed = 0;
            foreach (var pair in s.Grids)
            {
                if (pair.Key == MapIds.Greenhouse) continue;
                foreach (var tile in pair.Value.Tiles.OrderBy(t => t.X).ThenBy(t => t.Y))
                {
                    var crop = tile.Crop;
                    if (crop == null || crop.CropId.StartsWith("mutant_") || !s.Db.TryGetCrop(crop.CropId, out var def) || def.IsTree) continue;
                    if (crop.Stage < def.MatureStage || def.GrowCondition != null && def.GrowCondition.Contains("dread")) continue;   // only ordinary mature crops
                    if (WeatherRoller.Unit(today * 101 + tile.X * 7 + tile.Y * 13, s.State.WorldSeed ^ 0x3AD) >= chance) continue;
                    var mutantId = MythosData.MutantFor(def.Kind);
                    if (!s.Db.TryGetCrop(mutantId, out var mutant)) continue;
                    crop.CropId = mutantId;
                    crop.Stage = mutant.MatureStage;
                    crop.Regrowing = false;
                    changed++;
                }
            }
            return changed;
        }
    }

    // The mythos page in the journal (a generic journal hook): how far the player's understanding goes, never a meter. It lists
    // the lore the wakefulness steps have revealed and, with enough lore, the offerings that have been marked.
    public sealed class MythosJournalPage : IJournalPage
    {
        readonly GameSession _s;
        public MythosJournalPage(GameSession s) { _s = s; }

        public string TitleKey => "journal.mythos";
        public bool Visible => MythosLevel.On(_s) && _s.GetVar(MythosIds.Vars.Lore) > 0;

        public string Body()
        {
            var lore = _s.GetVar(MythosIds.Vars.Lore);
            var lines = new List<string> { L.Get("journal.mythos.intro") };
            var step = _s.GetVar(MythosIds2.Step);
            for (var i = 1; i <= step && i <= WakefulnessModel.StepCount; i++)
                if (lore >= i / 2 + 1) lines.Add("- " + L.Get("mythos.step." + i));
            if (lore >= 4)
            {
                var save = RitualDirector.Load(_s);
                var marked = RitualDirector.Marked(save);
                lines.Add(L.Get("journal.mythos.marked", marked.Count));
                foreach (var m in marked)
                    lines.Add("  - " + (_s.Db.TryGetItem(m.ItemId, out var item) ? L.Get(item.NameKey) : m.ItemId));
            }
            if (MythosLevel.Full(_s) && lore >= 6) lines.Add(L.Get("journal.mythos.ritual"));
            return string.Join("\n", lines);
        }
    }
}
