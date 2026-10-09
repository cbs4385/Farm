using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // What the hover label says about a tilled tile and what grows on it (playtest chat 2026-10-09: "show what a planted crop is and when it is ready").
    public static class CropInfo
    {
        // Watered days still needed before the crop is ripe (0 when it is). A tree counts every day, watered or not.
        public static int DaysToHarvest(CropInstance crop, CropDefinition def, bool speedGro = false)
        {
            if (crop == null || def == null || crop.Stage >= def.MatureStage) return 0;
            var days = 0;
            for (var stage = crop.Stage; stage < def.MatureStage; stage++)
            {
                var needed = crop.Regrowing && stage == def.MatureStage - 1 ? def.RegrowDays : def.GrowthDays[stage];
                if (speedGro && needed >= 2) needed--;
                days += stage == crop.Stage ? System.Math.Max(0, needed - crop.DaysInStage) : needed;
            }
            return days;
        }

        public static string Label(FarmTile tile, GameDatabase db)
        {
            if (tile == null) return null;
            var water = L.Get(tile.Watered ? "hover.watered" : "hover.dry");
            var crop = tile.Crop;
            if (crop == null) return L.Get("hover.soil", water);
            if (!db.TryGetCrop(crop.CropId, out var def)) return L.Get("hover.soil", water);
            var name = db.TryGetItem(def.HarvestItemId, out var item) ? L.Get(item.NameKey) : crop.CropId;
            if (crop.Withered) return L.Get("hover.withered", name);
            if (crop.Stage >= def.MatureStage && (!def.IsTree || crop.Fruit)) return L.Get("hover.ready", name);
            if (crop.Stage >= def.MatureStage) return L.Get("hover.crop_idle", name);
            var days = DaysToHarvest(crop, def, tile.Fertilizer == ItemIds.FertilizerSpeed);
            return L.Get("hover.crop", name, L.Get(days == 1 ? "hover.day" : "hover.days", days), water);
        }
    }
}
