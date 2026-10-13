using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Using what is in hand on the world: the tools, and eating.
    public sealed class ItemActions
    {
        readonly ActionContext _c;
        readonly FishingActions _fishing;
        readonly PlacementActions _placement;

        public ItemActions(ActionContext context, FishingActions fishing, PlacementActions placement)
        {
            _c = context; _fishing = fishing; _placement = placement;
        }

        public void UseTool(ToolType tool)
        {
            if (_c.Body.TryGetComponent<WalkBob>(out var bob) && _c.Body.TryGetComponent<PlayerController>(out var pc)) { bob.Lunge(pc.Facing); var at = _c.Target - _c.Map.WorldToCell(_c.Player.CellSamplePoint); bob.Strike(tool, new Vector2Int(at.x, at.y)); }
            var cell = _c.Target;
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            var tier = _c.Session.ToolTier(ToolModel.ItemId(tool));
            var cost = ToolModel.EnergyCost(PlayerActions.EnergyCost(tool), tier);

            switch (tool)
            {
                case ToolType.Hoe:
                    if (grid.IsTilled(cell.x, cell.y)) { _c.Session.Toast(L.Get("toast.untill_hint")); return; }
                    if (_c.Session.GetNodes(_c.Map.MapId).Has(cell.x, cell.y)) { _c.Session.Toast(L.Get("toast.clear_first")); return; }
                    if (!_c.Map.IsTillable(cell)) { _c.Session.Toast(L.Get("toast.cannot_till")); return; }
                    if (!_c.SpendEnergy(cost)) return;
                    grid.Till(cell.x, cell.y);
                    if (_c.Map.MapId == MapIds.Village) _c.Session.Publish(new VillageDamaged(cell.x, cell.y, true, VillageDamage.Soil));
                    _c.Session.AddVar(QuestLog.Stats.Tilled, 1);
                    AudioService.PlayIfAvailable(Sfx.Hoe);
                    ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.45f, 0.30f, 0.16f), 3, Fx.DigDirt);
                    break;

                case ToolType.WateringCan:
                    if (!grid.IsTilled(cell.x, cell.y)) return;
                    if (grid.TryGetTile(cell.x, cell.y, out var t) && t.Watered) return;
                    if (!_c.SpendEnergy(cost)) return;
                    grid.Water(cell.x, cell.y);
                    _c.Session.AddVar(QuestLog.Stats.Watered, 1);
                    AudioService.PlayIfAvailable(Sfx.Water);
                    ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.35f, 0.65f, 0.95f), 3, Fx.WaterDrop);
                    break;

                case ToolType.Rod:
                    _fishing.Cast(cell);
                    return;

                case ToolType.Hammer:
                    _c.Body.GetComponent<HammerMode>()?.Use(cell);
                    return;

                case ToolType.Sword:
                    var held = _c.Session.Backpack.Get(_c.Session.State.SelectedHotbar);
                    var combat = _c.Body.GetComponent<PlayerCombat>();
                    if (combat != null && held != null) combat.Swing(held.ItemId, _c.Map.CellCenter(cell));
                    return;

                case ToolType.Axe:
                case ToolType.Pickaxe:
                case ToolType.Scythe:
                    // The axe and pickaxe take a placed chest, machine or sprinkler back (no energy needed).
                    if (tool != ToolType.Scythe && _placement.TryPickUp(cell)) return;
                    cost = Mathf.Max(1, Mathf.RoundToInt(ToolModel.EnergyCost(PlayerActions.EnergyCost(tool), tier) * Professions.EnergyMultiplier(_c.Session.State)));
                    if (tool == ToolType.Scythe && TryUproot(cell)) break;
                    if (tool == ToolType.Scythe && TryClearWithered(cell, cost)) break;
                    if (tool == ToolType.Scythe && TryUntill(cell, cost)) break;
                    if (!SwingAtNode(tool, tier, cost, cell)) return;
                    _c.View.RefreshNode(cell);
                    break;

                default:
                    return;
            }
            _c.View.RefreshCell(cell);

            // Lets other systems react to the player finishing an action that cost energy (for example the late-night
            // stay-awake check, T-046).
            if (cost > 0) _c.Session.Bus.Publish(new EnergyActionCompleted(tool.ToString(), cost));
        }

        // The scythe takes a crop that has not grown yet back out of the ground, and the seed returns to the backpack.
        bool TryUproot(Vector3Int cell)
        {
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            if (!grid.TryGetTile(cell.x, cell.y, out var tile) || tile.Crop == null || tile.Crop.Withered || tile.Crop.Stage != 0 || tile.Crop.DaysInStage != 0) return false;
            if (!_c.Session.Db.TryGetCrop(tile.Crop.CropId, out var crop) || !_c.Session.Backpack.CanAdd(crop.SeedItemId, 1)) { _c.Session.Toast(L.Get("toast.inventory_full")); return true; }
            grid.TryUproot(cell.x, cell.y, out _);
            _c.Session.Backpack.Add(crop.SeedItemId, 1);
            _c.Session.Toast(L.Get("toast.uprooted"));
            AudioService.PlayIfAvailable(Sfx.Harvest);
            ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.45f, 0.38f, 0.30f), 3, Fx.DigDirt);
            return true;
        }

        // The scythe smooths bare tilled soil back to grass (playtest 2026-10-13: there was no way to take tilled soil away).
        bool TryUntill(Vector3Int cell, int cost)
        {
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            if (!grid.IsTilled(cell.x, cell.y) || grid.TryGetTile(cell.x, cell.y, out var t) && t.Crop != null) return false;
            if (!_c.SpendEnergy(cost)) return true;
            grid.Untill(cell.x, cell.y);
            AudioService.PlayIfAvailable(Sfx.Hoe);
            ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.30f, 0.50f, 0.22f), 3, Fx.DigDirt);
            return true;
        }

        // The scythe clears a withered crop off the soil (blight, frost). Returns true when the swing was used on one.
        bool TryClearWithered(Vector3Int cell, int cost)
        {
            var grid = _c.Session.GetGrid(_c.Map.MapId);
            if (!grid.IsWithered(cell.x, cell.y)) return false;
            if (!_c.SpendEnergy(cost)) return true;
            grid.ClearCrop(cell.x, cell.y);
            AudioService.PlayIfAvailable(Sfx.Harvest);
            ActionPuff.Burst(_c.Map.CellCenter(cell), new Color(0.45f, 0.38f, 0.30f), 3, Fx.DigDirt);
            return true;
        }

        // A swing of the axe, pickaxe or scythe at the node on a cell. Returns true if the swing happened (and cost energy).
        bool SwingAtNode(ToolType tool, int tier, int cost, Vector3Int cell)
        {
            var nodes = _c.Session.GetNodes(_c.Map.MapId);
            if (!nodes.TryGet(cell.x, cell.y, out var node)) return false;
            var def = _c.Session.Nodes.Get(node.TypeId);
            if (def == null || def.Tool != tool) return false;          // the wrong tool for this: nothing happens
            if (tier < def.MinToolTier) { _c.Session.Toast(L.Get("toast.tool_too_weak")); return false; }

            // Clearing it must not lose the loot: check the backpack before spending energy.
            var clears = node.Hp - ToolModel.Damage(tier) <= 0;
            if (clears && !string.IsNullOrEmpty(def.DropItemId) && !_c.Session.Backpack.CanAdd(def.DropItemId, def.DropMax))
            {
                _c.Session.Toast(L.Get("toast.inventory_full"));
                return false;
            }
            if (!_c.SpendEnergy(cost)) return false;

            var roll = WeatherRoller.Unit(cell.x * 131 + cell.y * 17 + _c.Session.Clock.Now.TotalDays * 7, _c.Session.State.WorldSeed);
            var result = nodes.Hit(cell.x, cell.y, tool, tier, _c.Session.Nodes.Get, roll);
            ActionPuff.Burst(_c.Map.CellCenter(cell), result.Outcome == NodeHit.Cleared ? new Color(0.7f, 0.62f, 0.45f) : new Color(0.75f, 0.75f, 0.75f), result.Outcome == NodeHit.Cleared ? 7 : 3,
                tool == ToolType.Axe ? Fx.ChopChip : tool == ToolType.Pickaxe ? Fx.OreSpark : Fx.Dust);
            if (_c.Map.MapId == MapIds.Village && (result.Outcome == NodeHit.Cleared || result.Outcome == NodeHit.Damaged) && VillageDamaged.TryKindOf(node.TypeId, out var damage))
                _c.Session.Publish(new VillageDamaged(cell.x, cell.y, result.Outcome == NodeHit.Cleared, damage));
            if (result.Outcome == NodeHit.Cleared)
            {
                if (!string.IsNullOrEmpty(result.DropItemId) && result.DropCount > 0)
                    _c.Session.Backpack.Add(result.DropItemId, result.DropCount);
                _c.Session.AddSkillXp(result.Skill, result.Xp);
                if (result.LeftBehind == NodeDefaults.Stump)                          // a tree came down: it may have dropped acorns
                {
                    var acorns = TreeRules.AcornsFor(WeatherRoller.Unit(cell.x * 977 + cell.y * 31 + _c.Session.Clock.Now.TotalDays * 13, _c.Session.State.WorldSeed ^ 0x3C6EF372));
                    if (acorns > 0 && _c.Session.Backpack.Add(ItemIds.Acorn, acorns) == 0) _c.Session.Toast(L.Get("toast.acorn_found", acorns));
                }
                AudioService.PlayIfAvailable(Sfx.Harvest);
            }
            else
            {
                AudioService.PlayIfAvailable(tool == ToolType.Scythe ? Sfx.Harvest : Sfx.Hoe);
            }
            return true;
        }

        public void Eat(ItemDefinition food)
        {
            if (_c.Session.State.Energy >= _c.Session.State.MaxEnergy && _c.Session.State.Health >= _c.Session.State.MaxHealth)
            {
                _c.Session.Toast(L.Get("toast.not_hungry"));
                return;
            }
            _c.Session.Backpack.Remove(food.Id, 1);
            _c.Session.RestoreEnergy(food.EnergyRestore);
            Combat.Heal(_c.Session, food.EnergyRestore / 2);
            _c.Session.Toast(L.Get("toast.ate", L.Get(food.NameKey), food.EnergyRestore));
        }
    }
}
