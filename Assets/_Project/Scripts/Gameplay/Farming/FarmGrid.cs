using System;
using System.Collections.Generic;
using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    [Serializable]
    public sealed class CropInstance
    {
        public string CropId;
        public int Stage;
        public int DaysInStage;
        public bool Regrowing;   // true after a harvest of a regrowing crop: the last stage then takes RegrowDays
        public bool Fruit;       // fruit trees: there is fruit to pick
        public bool Withered;    // blighted or otherwise dead: it stops growing and cannot be harvested; the scythe clears it
    }

    [Serializable]
    public sealed class FarmTile
    {
        public int X;
        public int Y;
        public bool Watered;
        public string Fertilizer;   // item id of the fertilizer worked into the soil (quality or speed), or null
        public CropInstance Crop;   // null when nothing is planted
    }

    public readonly struct HarvestResult
    {
        public readonly string ItemId;
        public readonly int Count;
        public readonly int Xp;
        public HarvestResult(string itemId, int count, int xp) { ItemId = itemId; Count = count; Xp = xp; }
    }

    // Tilled soil + crops for one map. Every entry in the dictionary is a tilled tile.
    // Pure C# so growth rules are testable; rendering lives in the scene layer.
    public sealed class FarmGrid
    {
        readonly Dictionary<(int, int), FarmTile> _tiles = new Dictionary<(int, int), FarmTile>();

        // A greenhouse: every crop may be planted in every season and none dies with the season.
        public bool AllSeasons { get; set; }

        public int Count => _tiles.Count;
        public IEnumerable<FarmTile> Tiles => _tiles.Values;

        public static FarmGrid FromTiles(IEnumerable<FarmTile> tiles)
        {
            var grid = new FarmGrid();
            if (tiles != null)
                foreach (var t in tiles) grid._tiles[(t.X, t.Y)] = t;
            return grid;
        }

        public List<FarmTile> ToList() => new List<FarmTile>(_tiles.Values);

        public bool TryGetTile(int x, int y, out FarmTile tile) => _tiles.TryGetValue((x, y), out tile);

        public bool IsTilled(int x, int y) => _tiles.ContainsKey((x, y));

        // Caller checks that the ground is tillable. Returns false if already tilled.
        public bool Till(int x, int y)
        {
            if (_tiles.ContainsKey((x, y))) return false;
            _tiles[(x, y)] = new FarmTile { X = x, Y = y };
            return true;
        }

        // Returns true if the tile changed (tilled, not already watered).
        public bool Water(int x, int y)
        {
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Watered) return false;
            tile.Watered = true;
            return true;
        }

        public bool CanPlant(int x, int y, CropDefinition crop, Season season) =>
            _tiles.TryGetValue((x, y), out var tile) && tile.Crop == null && Plantable(crop, season);

        // Trees can be planted any time; ordinary crops need their season (anything goes in a greenhouse).
        public bool Plantable(CropDefinition crop, Season season) => AllSeasons || crop.IsTree || crop.Seasons.Includes(season);

        // Works fertilizer into a tilled tile (replacing any earlier one). Returns false when the tile is not tilled.
        public bool Fertilize(int x, int y, string fertilizerItemId)
        {
            if (!_tiles.TryGetValue((x, y), out var tile)) return false;
            tile.Fertilizer = fertilizerItemId;
            return true;
        }

        public string FertilizerAt(int x, int y) => _tiles.TryGetValue((x, y), out var tile) ? tile.Fertilizer : null;

        public bool Plant(int x, int y, CropDefinition crop, Season season)
        {
            if (!CanPlant(x, y, crop, season)) return false;
            _tiles[(x, y)].Crop = new CropInstance { CropId = crop.Id };
            return true;
        }

        public bool IsMature(int x, int y, Func<string, CropDefinition> cropLookup)
        {
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null) return false;
            var def = cropLookup(tile.Crop.CropId);
            return def != null && !tile.Crop.Withered && tile.Crop.Stage >= def.MatureStage && (!def.IsTree || tile.Crop.Fruit);
        }

        // Returns false if there is nothing mature to harvest.
        public bool TryHarvest(int x, int y, Func<string, CropDefinition> cropLookup, out HarvestResult result)
        {
            result = default;
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null) return false;
            var def = cropLookup(tile.Crop.CropId);
            if (def == null || tile.Crop.Withered || tile.Crop.Stage < def.MatureStage) return false;
            if (def.IsTree && !tile.Crop.Fruit) return false;

            result = new HarvestResult(def.HarvestItemId, 1, def.HarvestXp);

            if (def.IsTree)
            {
                tile.Crop.Fruit = false;    // the tree stays; there is more fruit tomorrow while it is in season
            }
            else if (def.RegrowDays > 0)
            {
                // Drop back to the last growth stage so it matures again after RegrowDays watered days.
                tile.Crop.Stage = def.MatureStage - 1;
                tile.Crop.DaysInStage = 0;
                tile.Crop.Regrowing = true;
            }
            else
            {
                tile.Crop = null;
                tile.Fertilizer = null;
            }
            return true;
        }

        // Kills the crop on a tile where it stands (a blight, a frost): it stays, withered, until it is cleared. Returns true if a living crop withered.
        public bool Wither(int x, int y)
        {
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null || tile.Crop.Withered) return false;
            tile.Crop.Withered = true;
            return true;
        }

        public bool IsWithered(int x, int y) => _tiles.TryGetValue((x, y), out var tile) && tile.Crop != null && tile.Crop.Withered;

        // Removes a crop (scythe on a dead/unwanted plant). Returns true if something was removed.
        public bool ClearCrop(int x, int y)
        {
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null) return false;
            tile.Crop = null;
            tile.Fertilizer = null;
            return true;
        }

        // Called once per night. Crops watered today (or any crop if it rained today) advance a day; crops that are
        // out of season in `newSeason` die. Watering resets. Returns the number of crops that died.
        public int AdvanceDay(Season newSeason, bool rainedToday, Func<string, CropDefinition> cropLookup, IWorldQuery world = null)
        {
            var died = 0;
            foreach (var tile in _tiles.Values)
            {
                var crop = tile.Crop;
                if (crop != null)
                {
                    var def = cropLookup(crop.CropId);
                    if (def == null || !(AllSeasons || def.IsTree || def.Seasons.Includes(newSeason)))
                    {
                        tile.Crop = null;
                        tile.Fertilizer = null;
                        died++;
                    }
                    else if (!crop.Withered && (def.IsTree || tile.Watered || rainedToday) && crop.Stage < def.MatureStage && CanGrow(def, world))
                    {
                        crop.DaysInStage++;
                        var needed = crop.Regrowing && crop.Stage == def.MatureStage - 1
                            ? def.RegrowDays
                            : def.GrowthDays[crop.Stage];
                        // Speed-gro takes a day off every stage that lasts two days or more.
                        if (tile.Fertilizer == ItemIds.FertilizerSpeed && needed >= 2) needed--;
                        if (crop.DaysInStage >= needed)
                        {
                            crop.Stage++;
                            crop.DaysInStage = 0;
                            crop.Regrowing = false;
                        }
                    }

                    // A mature tree bears fruit overnight while it is in season.
                    if (tile.Crop != null && !crop.Withered && def != null && def.IsTree && crop.Stage >= def.MatureStage)
                        crop.Fruit = def.FruitSeasons.Includes(newSeason);
                }
                tile.Watered = false;
            }
            return died;
        }

        // A crop with a GrowCondition stays dormant while it does not hold. Without a world to ask, it grows normally.
        static bool CanGrow(CropDefinition def, IWorldQuery world) =>
            string.IsNullOrEmpty(def.GrowCondition) || world == null || Conditions.Evaluate(def.GrowCondition, world);

        // Rain at dawn: every tilled tile starts the day watered.
        public void WaterAll()
        {
            foreach (var tile in _tiles.Values) tile.Watered = true;
        }
    }
}
