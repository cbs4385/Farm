using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Farm.Gameplay
{
    // Which cells of the ground a building's picture really covers, so that a building blocks what it shows and nothing else (playtest 2026-10-10: the coop and the barn
    // blocked empty space around their pictures). The data is Resources/BuildingMasks.json, made from the pictures by tools/art/build_building_masks.py, which also
    // holds the pixel of each picture's door. A mask is relative to the door cell, so one serves every place a building can stand.
    public sealed class BuildingMask
    {
        [JsonProperty("doorPx")] public int DoorPx;
        [JsonProperty("dx0")] public int FirstColumn;          // the cell (relative to the door's) of the first character of each row
        [JsonProperty("rows")] public List<string> Rows = new List<string>();      // from the door's row upwards; '#' blocks

        // The cells that block, relative to the door cell (x to the right, y upwards). The door cell itself is never one of them.
        public IEnumerable<Vector2Int> SolidCells()
        {
            for (var dy = 0; dy < Rows.Count; dy++)
                for (var i = 0; i < Rows[dy].Length; i++)
                {
                    var dx = FirstColumn + i;
                    if (Rows[dy][i] == '#' && !(dx == 0 && dy == 0)) yield return new Vector2Int(dx, dy);
                }
        }

        public bool Blocks(int dx, int dy)
        {
            var i = dx - FirstColumn;
            return dy >= 0 && dy < Rows.Count && i >= 0 && i < Rows[dy].Length && Rows[dy][i] == '#' && !(dx == 0 && dy == 0);
        }
    }

    public static class BuildingMasks
    {
        public const string ResourcePath = "BuildingMasks";

        static Dictionary<string, BuildingMask> _all;

        static BuildingMasks() => Farm.Core.TestResets.Add(() => _all = null);

        public static IReadOnlyDictionary<string, BuildingMask> All
        {
            get
            {
                if (_all != null) return _all;
                var text = Resources.Load<TextAsset>(ResourcePath);
                _all = text != null ? JsonConvert.DeserializeObject<Dictionary<string, BuildingMask>>(text.text) : new Dictionary<string, BuildingMask>();
                return _all;
            }
        }

        // The mask of a style of building ("coop", "saloon", "cottage2"...), or null when there is none.
        public static BuildingMask Get(string style) => style != null && All.TryGetValue(style, out var mask) ? mask : null;
    }
}
