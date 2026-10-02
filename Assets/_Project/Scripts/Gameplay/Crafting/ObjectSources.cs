using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;

namespace Farm.Gameplay
{
    // World-object sources for the systems of T-037/T-038 (ADR 0002): what sits in chests, machines and the shipping bin,
    // and the standing crops. Optional layers use them through GameHooks to find, mark and consume real things.
    //
    //   kind "crafted": artisan goods and dishes (in chests, machine outputs, the shipping bin)
    //   kind "plant":   harvested crops and forage in those places, and fully grown standing crops
    //
    // An item stack is one object: its id is "<mapId>/<objectId>:<slot>" in a chest, "<mapId>/<objectId>:out" for a
    // machine's finished output, "bin:<index>" in the shipping bin; a standing crop is "<mapId>/<x>,<y>".
    public sealed class StoredItemsSource : IWorldObjectSource
    {
        readonly GameSession _session;
        readonly Func<ItemDefinition, bool> _belongs;

        public string Kind { get; }

        public StoredItemsSource(GameSession session, string kind)
        {
            _session = session;
            Kind = kind;
            _belongs = kind == WorldObjectKinds.CraftedItem
                ? (Func<ItemDefinition, bool>)(i => i.Category == ItemCategory.Artisan || i.Category == ItemCategory.Food)
                : i => i.Category == ItemCategory.Crop || i.Category == ItemCategory.Forage;
        }

        public IEnumerable<WorldObjectRef> Enumerate()
        {
            if (!_session.InGame) yield break;
            foreach (var pair in _session.AllObjectGrids)
                foreach (var obj in pair.Value.All)
                {
                    var def = _session.Placeables.Get(obj.TypeId);
                    if (def == null) continue;
                    if (def.Kind == PlaceableKind.Chest)
                    {
                        var chest = pair.Value.ChestOf(obj);
                        for (var slot = 0; slot < chest.Capacity; slot++)
                            if (Matches(chest.Get(slot), out var item)) yield return new WorldObjectRef(Kind, $"{pair.Key}/{obj.Id}:{slot}", pair.Key, item.Id);
                    }
                    else if (def.Kind == PlaceableKind.Machine && !string.IsNullOrEmpty(obj.OutputItemId)
                        && _session.Db.TryGetItem(obj.OutputItemId, out var made) && _belongs(made))
                    {
                        yield return new WorldObjectRef(Kind, $"{pair.Key}/{obj.Id}:out", pair.Key, made.Id);
                    }
                }
            for (var i = 0; i < _session.State.ShippingBin.Count; i++)
                if (Matches(_session.State.ShippingBin[i], out var item)) yield return new WorldObjectRef(Kind, $"bin:{i}", string.Empty, item.Id);
        }

        bool Matches(ItemStack stack, out ItemDefinition item)
        {
            item = null;
            return stack != null && stack.Count > 0 && _session.Db.TryGetItem(stack.ItemId, out item) && _belongs(item);
        }

        public bool Exists(WorldObjectRef reference) => Enumerate().Any(r => r.Equals(reference));

        public bool TryConsume(WorldObjectRef reference)
        {
            if (!Exists(reference)) return false;
            var id = reference.Id;
            if (id.StartsWith("bin:", StringComparison.Ordinal))
            {
                var index = int.Parse(id.Substring(4));
                _session.State.ShippingBin.RemoveAt(index);
                return true;
            }
            var slash = id.IndexOf('/');
            var colon = id.LastIndexOf(':');
            var mapId = id.Substring(0, slash);
            var objectId = id.Substring(slash + 1, colon - slash - 1);
            var what = id.Substring(colon + 1);
            var grid = _session.GetObjects(mapId);
            var obj = grid.ById(objectId);
            if (obj == null) return false;
            if (what == "out")
            {
                obj.RecipeId = null; obj.OutputItemId = null; obj.OutputCount = 0; obj.ReadyAt = 0;
                return true;
            }
            var chest = grid.ChestOf(obj);
            var slot = int.Parse(what);
            var stack = chest.Get(slot);
            if (stack == null) return false;
            chest.RemoveFromSlot(slot, stack.Count);
            return true;
        }
    }

    // Fully grown crops standing in the fields (kind "plant").
    public sealed class StandingCropsSource : IWorldObjectSource
    {
        readonly GameSession _session;
        public StandingCropsSource(GameSession session) { _session = session; }

        public string Kind => WorldObjectKinds.PlantProduct;

        CropDefinition Crop(string id) => id != null && _session.Db.TryGetCrop(id, out var c) ? c : null;

        public IEnumerable<WorldObjectRef> Enumerate()
        {
            if (!_session.InGame) yield break;
            foreach (var pair in _session.Grids)
                foreach (var tile in pair.Value.Tiles)
                    if (tile.Crop != null && pair.Value.IsMature(tile.X, tile.Y, Crop))
                        yield return new WorldObjectRef(Kind, $"{pair.Key}/{tile.X},{tile.Y}", pair.Key, tile.Crop.CropId);
        }

        public bool Exists(WorldObjectRef reference) => Enumerate().Any(r => r.Equals(reference));

        public bool TryConsume(WorldObjectRef reference)
        {
            var id = reference.Id;
            var slash = id.IndexOf('/');
            if (slash < 0) return false;
            var mapId = id.Substring(0, slash);
            var xy = id.Substring(slash + 1).Split(',');
            if (xy.Length != 2 || !int.TryParse(xy[0], out var x) || !int.TryParse(xy[1], out var y)) return false;
            if (!_session.Grids.TryGetValue(mapId, out var grid) || !grid.IsMature(x, y, Crop)) return false;
            return grid.ClearCrop(x, y);
        }
    }
}
