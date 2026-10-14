using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Setting things down and putting seeds, acorns and fertilizer in the ground: placed objects, animals, planting.
    public sealed class PlacementActions
    {
        readonly ActionContext _c;

        public PlacementActions(ActionContext context) { _c = context; }

        // Quarter turns the next piece of furniture from the hotbar will be set down with (R turns it; picking another slot resets it).
        int _turns;

        public int Turns => _turns;
        public void ResetTurns() => _turns = 0;

        // R with furniture in hand turns it before it is set down. Does nothing while the mallet is carrying something (it turns that instead).
        public void TurnPlacement()
        {
            if (_c.Body.GetComponent<HammerMode>() is HammerMode hammer && hammer.Carrying) return;
            var stack = _c.Session.Backpack.Get(_c.Session.State.SelectedHotbar);
            if (stack == null || !_c.Session.Db.TryGetItem(stack.ItemId, out var item) || item.Category != ItemCategory.Furniture) return;
            _turns = (_turns + 1) & 3;
            _c.Session.Toast(L.Get("placeable.turned", _turns * 90));
        }

        public void PlaceObject(ItemDefinition item)
        {
            var def = _c.Session.Placeables.Get(item.PlaceableId);
            if (def == null) return;
            var cell = _c.Target;
            if (def.FarmingOnly && !_c.Map.AllowFarming) { _c.Session.Toast(L.Get("placeable.farm_only")); return; }
            if (def.Kind == PlaceableKind.Decor)
            {
                if (!DecorRules.AllowedOn(_c.Map.MapId)) { _c.Session.Toast(L.Get("placeable.decor_where")); return; }
                if (DecorRules.BlocksDoor(cell, def.Walkable, DoorCells())) { _c.Session.Toast(L.Get("placeable.decor_door")); return; }
            }
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            var objects = _c.Session.GetObjects(_c.Map.MapId);
            if (cell == _c.Map.WorldToCell(_c.Player.CellSamplePoint) && !def.Walkable && def.Kind != PlaceableKind.Sprinkler) { _c.Session.Toast(L.Get("placeable.blocked")); return; }
            if (!_c.Map.CanPlaceAt(cell) || objects.Has(cell.x, cell.y) || grid.IsTilled(cell.x, cell.y) || _c.Session.GetNodes(_c.Map.MapId).Has(cell.x, cell.y))
            {
                _c.Session.Toast(L.Get("placeable.blocked"));
                return;
            }
            var placed = objects.Place(def, cell.x, cell.y, System.Guid.NewGuid().ToString("N").Substring(0, 10));
            if (placed == null) return;
            if (def.Kind == PlaceableKind.Decor) placed.Turns = _turns;
            _c.Session.Backpack.Remove(item.Id, 1);
            _c.Here?.Placed?.Spawn(placed);
            AudioService.PlayIfAvailable(Sfx.Plant);
        }

        public bool TryPickUp(Vector3Int cell)
        {
            if (!_c.Session.GetObjects(_c.Map.MapId).Has(cell.x, cell.y)) return false;
            var id = _c.Session.GetObjects(_c.Map.MapId).At(cell.x, cell.y).Id;
            switch (PlacedInteractions.PickUp(_c.Session, _c.Map.MapId, cell.x, cell.y))
            {
                case PlacedInteractions.PickUpResult.Ok: _c.Here?.Placed?.Despawn(id); AudioService.PlayIfAvailable(Sfx.Harvest); break;
                case PlacedInteractions.PickUpResult.NotEmpty: _c.Session.Toast(L.Get("placeable.not_empty")); break;
                case PlacedInteractions.PickUpResult.NoRoom: _c.Session.Toast(L.Get("toast.inventory_full")); break;
            }
            return true;
        }

        public void Fertilize(ItemDefinition item)
        {
            var cell = _c.Target;
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            if (!grid.IsTilled(cell.x, cell.y)) { _c.Session.Toast(L.Get("toast.fertilizer_needs_soil")); return; }
            if (grid.FertilizerAt(cell.x, cell.y) == item.Id) return;
            grid.Fertilize(cell.x, cell.y, item.Id);
            _c.Session.Backpack.Remove(item.Id, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _c.View.RefreshCell(cell);
        }

        // Returns true if a seed was planted. Explains with a toast when it was not.
        public bool Plant(ItemDefinition seed)
        {
            var cell = _c.Target;
            if (!_c.Session.Db.TryGetCrop(seed.CropId, out var crop)) return false;
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            var season = _c.Session.Clock.Now.Season;

            if (!grid.IsTilled(cell.x, cell.y)) { _c.Session.Toast(L.Get("toast.plant_needs_soil")); return false; }
            if (grid.TryGetTile(cell.x, cell.y, out var existing) && existing.Crop != null)
            {
                _c.Session.Toast(L.Get("toast.already_planted"));
                return false;
            }
            if (!grid.Plantable(crop, season))          // playtest 2026-10-13: out of season is refused and says which seasons the crop grows in
            {
                _c.Session.Toast(L.Get("toast.wrong_season", L.Get(_c.Session.Db.TryGetItem(crop.HarvestItemId, out var harvest) ? harvest.NameKey : seed.NameKey), CropInfo.SeasonsText(crop.Seasons)));
                return false;
            }
            if (!grid.Plant(cell.x, cell.y, crop, season)) return false;

            _c.Session.Backpack.Remove(seed.Id, 1);
            _c.Session.AddVar(QuestLog.Stats.Planted, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _c.View.RefreshCell(cell);
            return true;
        }

        // An acorn on open ground becomes a sapling that grows into a tree in a few days. Explains with a toast when it cannot go there.
        public bool PlantAcorn(ItemDefinition acorn)
        {
            var cell = _c.Target;
            var nodes = _c.Session.GetNodes(_c.Map.MapId);
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            var sapling = _c.Session.Nodes.Get(NodeDefaults.Sapling);
            if (sapling == null) return false;
            if (!_c.Map.AllowFarming && _c.Map.MapId != MapIds.Forest) { _c.Session.Toast(L.Get("toast.acorn_where")); return false; }
            if (grid.IsTilled(cell.x, cell.y) || nodes.Has(cell.x, cell.y) || _c.Session.GetObjects(_c.Map.MapId).Has(cell.x, cell.y)
                || cell == _c.Map.WorldToCell(_c.Player.CellSamplePoint) || !_c.Map.IsTreeGround(cell))
            {
                _c.Session.Toast(L.Get("toast.acorn_blocked"));
                return false;
            }
            if (!nodes.Add(cell.x, cell.y, sapling)) return false;
            _c.Session.Backpack.Remove(acorn.Id, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _c.View.RefreshNode(cell);
            _c.Session.Toast(L.Get("toast.acorn_planted", NodeDefaults.SaplingDays));
            return true;
        }

        public void PlaceAnimal(ItemDefinition item)
        {
            var row = AnimalDefaults.Find(item.PlaceableId);
            if (row == null || _c.Map.MapId != row.Value.Building) { _c.Session.Toast(L.Get("animal.wrong_place", L.Get("map." + (row?.Building ?? MapIds.Coop)))); return; }
            if (!AnimalRules.HasRoom(_c.Session.State, _c.Map.MapId)) { _c.Session.Toast(L.Get("animal.full")); return; }
            var name = L.Get("animal.default_name", L.Get("item." + item.Id + ".name"), AnimalRules.In(_c.Session.State, _c.Map.MapId).Count + 1);
            var animal = AnimalRules.Add(_c.Session.State, row.Value.Id, System.Guid.NewGuid().ToString("N").Substring(0, 8), name);
            if (animal == null) return;
            _c.Session.Backpack.Remove(item.Id, 1);
            var cell = _c.Target;
            _c.Here?.Animals?.Spawn(animal, cell.x, cell.y);
            _c.Session.Toast(L.Get("animal.welcome", name));
            if (_c.Session.Ui is IUiService ui)             // the player may give it a name of their own
                ui.ShowNamePrompt(L.Get("animal.name_title", name), name, newName => { animal.Name = newName; _c.Session.NotifyChanged(); });
        }

        // The cells every door (warp) of this map covers.
        public IEnumerable<Vector3Int> DoorCells()
        {
            foreach (var warp in Object.FindObjectsByType<Warp>(FindObjectsSortMode.None))
            {
                var collider = warp.GetComponent<Collider2D>();
                if (collider == null) continue;
                var min = _c.Map.WorldToCell(collider.bounds.min + new Vector3(0.05f, 0.05f, 0f));
                var max = _c.Map.WorldToCell(collider.bounds.max - new Vector3(0.05f, 0.05f, 0f));
                for (var x = min.x; x <= max.x; x++)
                    for (var y = min.y; y <= max.y; y++)
                        yield return new Vector3Int(x, y, 0);
            }
            foreach (var cell in _c.Map.DoorTiles()) yield return cell;
        }
    }
}
