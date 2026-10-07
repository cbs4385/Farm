using System;
using System.Collections.Generic;
using Farm.Data;

namespace Farm.Gameplay
{
    // One thing the player has set down: a chest, a machine, a sprinkler or a scarecrow. Saved per map (MapState.Objects).
    [Serializable]
    public sealed class PlacedObject
    {
        public string Id;                 // unique within the save
        public string TypeId;             // PlaceableDefinition.Id
        public int X, Y;
        public int Turns;                 // quarter turns, 0 to 3, for furniture (the older saves have none: 0)
        public InventoryData Items;       // chests
        public string RecipeId;           // machines: what is being made (null when idle)
        public int ReadyAt;               // machines: game minute (ObjectGrid.Minute) the output is ready
        public string OutputItemId;
        public int OutputCount;
    }

    // The placed objects of one map: pure state with the rules for placing, removing and looking them up. Machine
    // timing uses absolute game minutes, so machines keep working across the night without any nightly processing.
    public sealed class ObjectGrid
    {
        readonly List<PlacedObject> _objects = new List<PlacedObject>();
        readonly Dictionary<(int, int), PlacedObject> _byCell = new Dictionary<(int, int), PlacedObject>();
        readonly Dictionary<string, Inventory> _chests = new Dictionary<string, Inventory>();
        Func<string, int> _maxStack = _ => 999;

        public int Count => _objects.Count;
        public IReadOnlyList<PlacedObject> All => _objects;

        // Minutes since the start of Year 1 Spring 1, 00:00. The day runs 06:00 to 30:00, so the next morning's 06:00 is
        // exactly 24 hours after the previous one and the number never goes backwards.
        public static int Minute(Farm.Core.GameDateTime now) => now.TotalDays * 1440 + now.MinuteOfDay;

        public static ObjectGrid FromList(IEnumerable<PlacedObject> objects, Func<string, int> maxStack = null)
        {
            var grid = new ObjectGrid();
            if (maxStack != null) grid._maxStack = maxStack;
            if (objects != null)
                foreach (var o in objects)
                {
                    grid._objects.Add(o);
                    grid._byCell[(o.X, o.Y)] = o;
                }
            return grid;
        }

        // Chest inventories are kept live while the game runs and written back by ToList.
        public List<PlacedObject> ToList()
        {
            foreach (var pair in _chests)
            {
                var obj = _objects.Find(o => o.Id == pair.Key);
                if (obj != null) obj.Items = pair.Value.ToData();
            }
            return new List<PlacedObject>(_objects);
        }

        public PlacedObject At(int x, int y) => _byCell.TryGetValue((x, y), out var o) ? o : null;
        public PlacedObject ById(string id) => _objects.Find(o => o.Id == id);
        public bool Has(int x, int y) => _byCell.ContainsKey((x, y));

        // Places an object. The caller has checked that the ground allows it. Returns null when the cell is taken.
        public PlacedObject Place(PlaceableDefinition def, int x, int y, string id)
        {
            if (_byCell.ContainsKey((x, y))) return null;
            var obj = new PlacedObject { Id = id, TypeId = def.Id, X = x, Y = y };
            if (def.Kind == PlaceableKind.Chest) obj.Items = new Inventory(Math.Max(1, def.Capacity), _maxStack).ToData();
            _objects.Add(obj);
            _byCell[(x, y)] = obj;
            return obj;
        }

        // Moves an object to another cell. False when the cell is taken (by another object) or the object is unknown.
        public bool Move(string id, int x, int y)
        {
            var obj = ById(id);
            if (obj == null) return false;
            if (_byCell.TryGetValue((x, y), out var there) && there != obj) return false;
            _byCell.Remove((obj.X, obj.Y));
            obj.X = x; obj.Y = y;
            _byCell[(x, y)] = obj;
            return true;
        }

        public bool Remove(string id)
        {
            var obj = ById(id);
            if (obj == null) return false;
            _objects.Remove(obj);
            _byCell.Remove((obj.X, obj.Y));
            _chests.Remove(id);
            return true;
        }

        public Inventory ChestOf(PlacedObject obj)
        {
            if (obj == null) return null;
            if (!_chests.TryGetValue(obj.Id, out var inv))
            {
                inv = obj.Items != null ? Inventory.FromData(obj.Items, _maxStack) : new Inventory(36, _maxStack);
                _chests[obj.Id] = inv;
            }
            return inv;
        }

        public bool ChestIsEmpty(PlacedObject obj)
        {
            var inv = ChestOf(obj);
            for (var i = 0; i < inv.Capacity; i++) if (inv.Get(i) != null) return false;
            return true;
        }

        // A short unique id for a new object.
        public static string NewId(int seed, int counter) => $"o{seed & 0xFFFFFF:x}{counter:x}";
    }
}
