using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Turns input into world actions: hotbar selection, tool use on the tile in front, harvesting, interacting.
    public sealed class PlayerActions : MonoBehaviour
    {
        public const int HoeEnergy = 2;
        public const int WateringEnergy = 2;
        public const int AxeEnergy = 3;
        public const int PickaxeEnergy = 3;
        public const int ScytheEnergy = 2;

        [SerializeField] PlayerController _player;
        [SerializeField] FarmMap _map;
        [SerializeField] FarmMapView _view;
        [SerializeField] Transform _cursor;

        InputService _input;

        public GameSession Session { get; private set; }

        public void Configure(PlayerController player, FarmMap map, FarmMapView view, Transform cursor)
        {
            _player = player; _map = map; _view = view; _cursor = cursor;
        }

        public static int EnergyCost(ToolType tool)
        {
            switch (tool)
            {
                case ToolType.Hoe: return HoeEnergy;
                case ToolType.WateringCan: return WateringEnergy;
                case ToolType.Axe: return AxeEnergy;
                case ToolType.Pickaxe: return PickaxeEnergy;
                case ToolType.Scythe: return ScytheEnergy;
                default: return 0;
            }
        }

        void Start()
        {
            Session = ServiceLocator.Get<GameSession>();
            _input = ServiceLocator.Get<InputService>();
        }

        Vector3Int TargetCell
        {
            get
            {
                var cell = _map.WorldToCell(_player.CellSamplePoint);
                return cell + new Vector3Int(_player.Facing.x, _player.Facing.y, 0);
            }
        }

        void Update()
        {
            if (Session == null || !Session.InGame) return;

            if (_cursor != null) _cursor.position = _map.CellCenter(TargetCell);

            if (_input.GameplayBlocked) return;

            for (var i = 0; i < InputNames.HotbarSlots; i++)
                if (_input.Hotbar(i).WasPressedThisFrame()) SelectHotbar(i);
            if (_input.HotbarNext.WasPressedThisFrame()) SelectHotbar((Session.State.SelectedHotbar + 1) % InputNames.HotbarSlots);
            if (_input.HotbarPrev.WasPressedThisFrame())
                SelectHotbar((Session.State.SelectedHotbar + InputNames.HotbarSlots - 1) % InputNames.HotbarSlots);

            if (_input.UseTool.WasPressedThisFrame()) UseSelected();
            if (_input.Interact.WasPressedThisFrame()) Interact();

            if (_input.Inventory.WasPressedThisFrame() && ServiceLocator.TryGet<IUiService>(out var ui)) ui.ToggleInventory();
            if (_input.Pause.WasPressedThisFrame() && ServiceLocator.TryGet<IUiService>(out var ui2)) ui2.ShowPause();
        }

        void SelectHotbar(int slot)
        {
            Session.State.SelectedHotbar = Mathf.Clamp(slot, 0, InputNames.HotbarSlots - 1);
            ServiceLocator.Get<EventBus>().Publish(new StatsChanged());
        }

        // ---- tools ---------------------------------------------------------------------------------------------

        public void UseSelected()
        {
            var stack = Session.Backpack.Get(Session.State.SelectedHotbar);
            if (stack == null || !Session.Db.TryGetItem(stack.ItemId, out var item)) return;

            switch (item.Category)
            {
                case ItemCategory.Tool: UseTool(item.ToolType); break;
                case ItemCategory.Seed: Plant(item); break;
                case ItemCategory.Food: Eat(item); break;
            }
        }

        void UseTool(ToolType tool)
        {
            var cell = TargetCell;
            var grid = Session.GetGrid(_map.MapId);
            var cost = EnergyCost(tool);

            switch (tool)
            {
                case ToolType.Hoe:
                    if (grid.IsTilled(cell.x, cell.y)) return;
                    if (!_map.IsTillable(cell)) { Session.Toast(L.Get("toast.cannot_till")); return; }
                    if (!SpendEnergy(cost)) return;
                    grid.Till(cell.x, cell.y);
                    AudioService.PlayIfAvailable(Sfx.Hoe);
                    break;

                case ToolType.WateringCan:
                    if (!grid.IsTilled(cell.x, cell.y)) return;
                    if (grid.TryGetTile(cell.x, cell.y, out var t) && t.Watered) return;
                    if (!SpendEnergy(cost)) return;
                    grid.Water(cell.x, cell.y);
                    AudioService.PlayIfAvailable(Sfx.Water);
                    break;

                default:
                    // Axe, pickaxe and scythe have no targets until trees, rocks and weeds arrive (T-032/T-033).
                    return;
            }
            _view.RefreshCell(cell);

            // Lets other systems react to the player finishing an action that cost energy (for example the late-night
            // stay-awake check, T-046).
            if (cost > 0) ServiceLocator.Get<EventBus>().Publish(new EnergyActionCompleted(tool.ToString(), cost));
        }

        // Returns true if a seed was planted. Explains with a toast when it was not.
        bool Plant(ItemDefinition seed)
        {
            var cell = TargetCell;
            if (!Session.Db.TryGetCrop(seed.CropId, out var crop)) return false;
            var grid = Session.GetGrid(_map.MapId);
            var season = Session.Clock.Now.Season;

            if (!grid.IsTilled(cell.x, cell.y)) { Session.Toast(L.Get("toast.plant_needs_soil")); return false; }
            if (grid.TryGetTile(cell.x, cell.y, out var existing) && existing.Crop != null)
            {
                Session.Toast(L.Get("toast.already_planted"));
                return false;
            }
            if (!crop.Seasons.Includes(season)) { Session.Toast(L.Get("toast.wrong_season")); return false; }
            if (!grid.Plant(cell.x, cell.y, crop, season)) return false;

            Session.Backpack.Remove(seed.Id, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _view.RefreshCell(cell);
            return true;
        }

        void Eat(ItemDefinition food)
        {
            if (Session.State.Energy >= Session.State.MaxEnergy) return;
            Session.Backpack.Remove(food.Id, 1);
            Session.RestoreEnergy(food.EnergyRestore);
        }

        bool SpendEnergy(int cost)
        {
            if (Session.TrySpendEnergy(cost)) return true;
            Session.Toast(L.Get("toast.too_tired"));
            AudioService.PlayIfAvailable(Sfx.Error);
            return false;
        }

        // ---- interaction ---------------------------------------------------------------------------------------

        void Interact()
        {
            var cell = TargetCell;
            var grid = Session.GetGrid(_map.MapId);

            // 1. Harvest a mature crop in front of the player.
            if (grid.IsMature(cell.x, cell.y, CropLookup))
            {
                var crop = CropLookup(grid.TryGetTile(cell.x, cell.y, out var tile) ? tile.Crop.CropId : null);
                if (crop != null && !Session.Backpack.CanAdd(crop.HarvestItemId, 1))
                {
                    Session.Toast(L.Get("toast.inventory_full"));
                    return;
                }
                if (grid.TryHarvest(cell.x, cell.y, CropLookup, out var result))
                {
                    Session.Backpack.Add(result.ItemId, result.Count);
                    Session.AddSkillXp("farming", result.Xp);
                    AudioService.PlayIfAvailable(Sfx.Harvest);
                    _view.RefreshCell(cell);
                }
                return;
            }

            // 2. Something interactable on that tile (bin, bed, shop counter...).
            foreach (var hit in Physics2D.OverlapPointAll(_map.CellCenter(cell)))
            {
                var interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                    return;
                }
            }

            // 3. Convenience: Interact also plants the selected seeds (the main way is Use Tool).
            var stack = Session.Backpack.Get(Session.State.SelectedHotbar);
            if (stack != null && Session.Db.TryGetItem(stack.ItemId, out var item) && item.Category == ItemCategory.Seed) Plant(item);
        }

        CropDefinition CropLookup(string id) => id != null && Session.Db.TryGetCrop(id, out var c) ? c : null;
    }
}
