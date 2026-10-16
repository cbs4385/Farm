using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Doing things by hand: picking forage, harvesting, talking to a villager or animal, using a chest, bed or counter, and the click that does the same as E.
    public sealed class HandInteraction
    {
        readonly ActionContext _c;
        readonly PlacementActions _placement;

        public HandInteraction(ActionContext context, PlacementActions placement)
        {
            _c = context; _placement = placement;
        }

        NpcActor NpcUnderPointer() =>
            _c.Aim.MouseAiming && _c.Here?.Npcs != null
                ? PlayerActions.PointedVillager(_c.Map.WorldToCell(_c.Player.CellSamplePoint), _c.Aim.PointerCell, _c.Here?.Npcs.Actors.Values, _c.Map.WorldToCell, c => InteractableAt(c, 0) is ShopCounter)
                : null;

        // Who an item in hand is for: the villager under the mouse (across a counter too), the one in the cell in front, or the one behind the counter in front (playtest
        // 2026-10-12: a gift for Tilda could not be given while she worked behind her counter).
        public NpcActor VillagerToGive()
        {
            var pointed = NpcUnderPointer();
            if (pointed != null) return pointed;
            var cell = _c.Target;
            var near = NpcAt(cell);
            if (near != null) return near;
            if (_c.Aim.MouseAiming || !(InteractableAt(cell, 0) is ShopCounter)) return null;
            return NpcAt(cell + new Vector3Int(_c.Player.Facing.x, _c.Player.Facing.y, 0));
        }

        public NpcActor NpcAt(Vector3Int cell) => _c.Here?.Npcs != null ? _c.Here?.Npcs.ActorNear(cell) : null;

        // Picks forage on the cell, if there is any. Returns true if the interaction was used up (picked or refused).
        public bool TryForage(Vector3Int cell)
        {
            var nodes = _c.Session.GetNodes(_c.Map.MapId);
            if (!nodes.TryGet(cell.x, cell.y, out var node)) return false;
            var def = _c.Session.Nodes.Get(node.TypeId);
            if (def == null || def.Tool != ToolType.None) return false;
            if (!string.IsNullOrEmpty(def.DropItemId) && !_c.Session.Backpack.CanAdd(def.DropItemId, def.DropMax))
            {
                _c.Session.Toast(L.Get("toast.inventory_full"));
                return true;
            }
            if (!_c.SpendEnergy(PlayerActions.ForageEnergy)) return true;

            var day = _c.Session.Clock.Now.TotalDays;
            var wasLast = ForageRules.IsLastOfItsKind(nodes, node.TypeId);
            var result = nodes.Gather(cell.x, cell.y, _c.Session.Nodes.Get, WeatherRoller.Unit(cell.x * 53 + cell.y * 19 + day, _c.Session.State.WorldSeed));
            var quality = ForageModel.Quality(_c.Session.GetSkillLevel(SkillIds.Foraging), _c.Session.Luck + Professions.LuckBonus(_c.Session.State, ProfessionEffect.LuckForaging),
                WeatherRoller.Unit(cell.x * 97 + cell.y * 41 + day * 3, _c.Session.State.WorldSeed ^ 0x7F4A7C15));
            if (result.DropCount > 0) _c.Session.Backpack.Add(result.DropItemId, result.DropCount, quality);
            _c.Session.AddVar(QuestLog.Stats.Foraged, 1);
            _c.Session.AddSkillXp(result.Skill, result.Xp);
            AudioService.PlayIfAvailable(Sfx.Harvest);
            _c.View.RefreshNode(cell);
            if (wasLast && _c.Session.Db.TryGetItem(def.DropItemId, out var picked)) _c.Session.Toast(L.Get(ForageRules.ReminderKey("toast.forage_last", picked.Id), L.Get(picked.NameKey)));
            return true;
        }

        public static bool MouseClicked => UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;

        public bool ClickLandedOnTheHud() =>
            UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame
            && _c.Ui != null && _c.Ui.PointerOverUi;

        public bool ClickUsesTheHand()
        {
            var stack = _c.Session.Backpack.Get(_c.Session.State.SelectedHotbar);
            ItemDefinition item = null;
            if (stack != null) _c.Session.Db.TryGetItem(stack.ItemId, out item);
            var tool = item != null && item.IsTool ? item.ToolType : ToolType.None;
            if (tool == ToolType.Hammer) return false;
            if (_c.Body.GetComponent<HammerMode>() is HammerMode hammer && hammer.Carrying) return false;

            var cell = _c.Target;
            var villager = NpcAt(cell) != null || NpcUnderPointer() != null;
            if (tool == ToolType.Rod && !villager) return false;          // the rod casts, but a click on a villager talks to them (playtest 2026-10-13)
            if (villager) return item == null || item.IsTool;
            if (_c.Here?.Animals != null && _c.Here?.Animals.ActorAt(cell) != null) return true;
            if (_c.Session.GetGrid(_c.Map.MapId).IsMature(cell.x, cell.y, CropLookup)) return true;
            if (_c.Session.GetNodes(_c.Map.MapId).TryGet(cell.x, cell.y, out var node) && _c.Session.Nodes.Get(node.TypeId) is ResourceNodeDefinition def && def.Tool == ToolType.None) return true;
            if (_c.Here?.Placed != null && _c.Here?.Placed.At(cell) != null) return true;          // the axe and pickaxe no longer take placed things back (the mallet does), so the click opens or uses it
            return InteractableAt(cell, 0) != null;
        }

        public void Interact()
        {
            if (_c.Body.GetComponent<HammerMode>() is HammerMode hammer && hammer.Cancel()) return;          // the Interact button puts a lifted thing back
            var cell = _c.Target;
            var grid = _c.Session.GetGrid(_c.Map.MapId);

            if (NpcUnderPointer() is NpcActor pointed) { pointed.Interact(_c.Actions); return; }          // the mouse is on a villager

            if (grid.IsWithered(cell.x, cell.y)) { _c.Session.Toast(L.Get("toast.crop_withered")); return; }

            // 1. Harvest a mature crop in front of the player.
            if (grid.IsMature(cell.x, cell.y, CropLookup))
            {
                var crop = CropLookup(grid.TryGetTile(cell.x, cell.y, out var tile) ? tile.Crop.CropId : null);
                if (crop != null && !_c.Session.Backpack.CanAdd(crop.HarvestItemId, 1))
                {
                    _c.Session.Toast(L.Get("toast.inventory_full"));
                    return;
                }
                var fertilizer = grid.FertilizerAt(cell.x, cell.y);
                if (grid.TryHarvest(cell.x, cell.y, CropLookup, out var result))
                {
                    var quality = CropQuality.Roll(_c.Session.GetSkillLevel(SkillIds.Farming), fertilizer, _c.Session.Luck + Professions.LuckBonus(_c.Session.State, ProfessionEffect.LuckFarming),
                        WeatherRoller.Unit(cell.x * 61 + cell.y * 29 + _c.Session.Clock.Now.TotalDays * 5, _c.Session.State.WorldSeed ^ 0x2545F491));
                    _c.Session.Backpack.Add(result.ItemId, result.Count, quality);
                    _c.Hoist(result.ItemId);
                    _c.Session.AddVar(QuestLog.Stats.Harvested, 1);
                    _c.Session.AddSkillXp(SkillIds.Farming, result.Xp);
                    AudioService.PlayIfAvailable(Sfx.Harvest);
                    ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.45f, 0.78f, 0.30f), 3, Fx.HarvestPop);
                    _c.View.RefreshCell(cell);
                }
                return;
            }

            // 1b. Something wild to pick up by hand (forage).
            if (TryForage(cell)) return;

            // 1c. A villager standing there.
            if (NpcAt(cell) is NpcActor villager)
            {
                villager.Interact(_c.Actions);
                return;
            }

            // 1c2. A farm animal.
            var animalHere = _c.Here?.Animals != null ? _c.Here?.Animals.ActorAt(cell) : null;
            if (animalHere != null)
            {
                animalHere.Interact(_c.Actions);
                return;
            }

            // 1d. A chest, machine, sprinkler or scarecrow the player placed.
            var placedHere = _c.Here?.Placed != null ? _c.Here?.Placed.At(cell) : null;
            if (placedHere != null)
            {
                placedHere.Interact(_c.Actions);
                return;
            }

            // 2. Something interactable on that tile (bin, bed, shop counter...), or a large one (counter, bin) one tile further on.
            if (InteractableAt(cell, 0) is IInteractable here) { here.Interact(_c.Actions); return; }
            if (!_c.Aim.MouseAiming && InteractableAt(cell + new Vector3Int(_c.Player.Facing.x, _c.Player.Facing.y, 0), 1) is IInteractable far) { far.Interact(_c.Actions); return; }

            // 3. Convenience: Interact also plants the selected seeds (the main way is Use Tool).
            var stack = _c.Session.Backpack.Get(_c.Session.State.SelectedHotbar);
            if (stack != null && _c.Session.Db.TryGetItem(stack.ItemId, out var item) && item.Category == ItemCategory.Seed) _placement.Plant(item);
        }

        // The thing to use on a cell: the first interactable there whose reach is at least `minReach`.
        IInteractable InteractableAt(Vector3Int cell, int minReach)
        {
            foreach (var hit in Physics2D.OverlapPointAll(_c.Map.CellCenter(cell)))
            {
                var interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.Reach >= minReach) return interactable;
            }
            return null;
        }

        CropDefinition CropLookup(string id) => id != null && _c.Session.Db.TryGetCrop(id, out var c) ? c : null;
    }
}
