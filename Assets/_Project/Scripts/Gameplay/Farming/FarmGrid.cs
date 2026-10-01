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
    }

    [Serializable]
    public sealed class FarmTile
    {
        public int X;
        public int Y;
        public bool Watered;
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
            _tiles.TryGetValue((x, y), out var tile) && tile.Crop == null && crop.Seasons.Includes(season);

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
            return def != null && tile.Crop.Stage >= def.MatureStage;
        }

        // Returns false if there is nothing mature to harvest.
        public bool TryHarvest(int x, int y, Func<string, CropDefinition> cropLookup, out HarvestResult result)
        {
            result = default;
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null) return false;
            var def = cropLookup(tile.Crop.CropId);
            if (def == null || tile.Crop.Stage < def.MatureStage) return false;

            result = new HarvestResult(def.HarvestItemId, 1, def.HarvestXp);

            if (def.RegrowDays > 0)
            {
                // Drop back to the last growth stage so it matures again after RegrowDays watered days.
                tile.Crop.Stage = def.MatureStage - 1;
                tile.Crop.DaysInStage = 0;
                tile.Crop.Regrowing = true;
            }
            else
            {
                tile.Crop = null;
            }
            return true;
        }

        // Removes a crop (scythe on a dead/unwanted plant). Returns true if something was removed.
        public bool ClearCrop(int x, int y)
        {
            if (!_tiles.TryGetValue((x, y), out var tile) || tile.Crop == null) return false;
            tile.Crop = null;
            return true;
        }

        // Called once per night. Crops watered today (or any crop if it rained today) advance a day; crops that are
        // out of season in `newSeason` die. Watering resets. Returns the number of crops that died.
        public int AdvanceDay(Season newSeason, bool rainedToday, Func<string, CropDefinition> cropLookup)
        {
            var died = 0;
            foreach (var tile in _tiles.Values)
            {
                var crop = tile.Crop;
                if (crop != null)
                {
                    var def = cropLookup(crop.CropId);
                    if (def == null || !def.Seasons.Includes(newSeason))
                    {
                        tile.Crop = null;
                        died++;
                    }
                    else if ((tile.Watered || rainedToday) && crop.Stage < def.MatureStage)
                    {
                        crop.DaysInStage++;
                        var needed = crop.Regrowing && crop.Stage == def.MatureStage - 1
                            ? def.RegrowDays
                            : def.GrowthDays[crop.Stage];
                        if (crop.DaysInStage >= needed)
                        {
                            crop.Stage++;
                            crop.DaysInStage = 0;
                            crop.Regrowing = false;
                        }
                    }
                }
                tile.Watered = false;
            }
            return died;
        }

        // Rain at dawn: every tilled tile starts the day watered.
        public void WaterAll()
        {
            foreach (var tile in _tiles.Values) tile.Watered = true;
        }
    }
}
