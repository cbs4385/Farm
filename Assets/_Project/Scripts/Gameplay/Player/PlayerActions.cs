using System.Collections.Generic;
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
            HeldItem.For(gameObject);                // the carry poses
        }

        // The cell the tool square is on: the one in front of the player, or, while the mouse is steering it, the one around the player
        // that the pointer is over (AimMath).
        bool _mouseAim;
        Vector2 _lastMouse;
        Vector3Int _pointerCell;

        public bool MouseAiming => _mouseAim;
        public Vector3Int Target => TargetCell;

        Vector3Int TargetCell
        {
            get
            {
                var cell = _map.WorldToCell(_player.CellSamplePoint);
                if (_mouseAim) return AimMath.Target(cell, _pointerCell);
                return cell + new Vector3Int(_player.Facing.x, _player.Facing.y, 0);
            }
        }

        // The mouse takes over the square when it moves over the world and gives it back as soon as the player walks.
        void UpdateAim()
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            if (mouse == null || settings == null || !settings.MouseAim || _input.GameplayBlocked) { _mouseAim = false; return; }
            var position = mouse.position.ReadValue();
            var moved = (position - _lastMouse).sqrMagnitude > 1f;
            _lastMouse = position;
            var walking = _input.Move.ReadValue<Vector2>().sqrMagnitude > 0.04f;
            var overUi = ServiceLocator.TryGet<IUiService>(out var ui) && ui.PointerOverUi;
            if (walking) _mouseAim = false;
            else if (moved && !overUi) _mouseAim = true;
            if (!_mouseAim) return;
            var cam = Camera.main;
            if (cam == null) { _mouseAim = false; return; }
            var world = cam.ScreenToWorldPoint(new Vector3(position.x, position.y, -cam.transform.position.z));
            _pointerCell = _map.WorldToCell(world);
            var playerCell = _map.WorldToCell(_player.CellSamplePoint);
            var facing = AimMath.FacingToward(playerCell, AimMath.Target(playerCell, _pointerCell), _player.Facing);
            if (facing != _player.Facing) _player.Face(facing);
        }

        void Update()
        {
            if (Session == null || !Session.InGame) return;

            UpdateAim();
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
            if (_input.Menu.WasPressedThisFrame() && ServiceLocator.TryGet<IUiService>(out var ui3)) ui3.ShowGameMenu();
            if (_input.Journal.WasPressedThisFrame() && ServiceLocator.TryGet<IUiService>(out var ui4)) ui4.ShowGameMenu(MenuTabs.Journal);
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

            // An item used on a villager is a gift.
            if (item.Category != ItemCategory.Tool && NpcAt(TargetCell) is NpcActor npc)
            {
                var result = NpcInteractions.Gift(Session, npc.Definition, Session.State.SelectedHotbar);
                NpcInteractions.ToastFor(Session, result);
                if (result == GiftResult.Given) ActionPuff.Hearts(npc.transform.position + Vector3.up * 1.2f, npc.GetComponent<WalkBob>());
                return;
            }

            switch (item.Category)
            {
                case ItemCategory.Tool: UseTool(item.ToolType); break;
                case ItemCategory.Seed: Plant(item); break;
                case ItemCategory.Food: Eat(item); break;
                case ItemCategory.Machine: PlaceObject(item); break;
                case ItemCategory.Furniture: PlaceObject(item); break;
                case ItemCategory.Animal: PlaceAnimal(item); break;
                case ItemCategory.Fertilizer: Fertilize(item); break;
            }
        }

        // The cells every door (warp) of this map covers.
        IEnumerable<Vector3Int> DoorCells()
        {
            foreach (var warp in Object.FindObjectsByType<Warp>(FindObjectsSortMode.None))
            {
                var collider = warp.GetComponent<Collider2D>();
                if (collider == null) continue;
                var min = _map.WorldToCell(collider.bounds.min + new Vector3(0.05f, 0.05f, 0f));
                var max = _map.WorldToCell(collider.bounds.max - new Vector3(0.05f, 0.05f, 0f));
                for (var x = min.x; x <= max.x; x++)
                    for (var y = min.y; y <= max.y; y++)
                        yield return new Vector3Int(x, y, 0);
            }
        }

        NpcActor NpcAt(Vector3Int cell) => NpcManager.Current != null ? NpcManager.Current.ActorAt(cell) : null;

        void UseTool(ToolType tool)
        {
            if (TryGetComponent<WalkBob>(out var bob) && TryGetComponent<PlayerController>(out var pc)) bob.Lunge(pc.Facing);
            var cell = TargetCell;
            var grid = Session.GetGrid(_map.MapId);
            var tier = Session.ToolTier(ToolModel.ItemId(tool));
            var cost = ToolModel.EnergyCost(EnergyCost(tool), tier);

            switch (tool)
            {
                case ToolType.Hoe:
                    if (grid.IsTilled(cell.x, cell.y)) return;
                    if (Session.GetNodes(_map.MapId).Has(cell.x, cell.y)) { Session.Toast(L.Get("toast.clear_first")); return; }
                    if (!_map.IsTillable(cell)) { Session.Toast(L.Get("toast.cannot_till")); return; }
                    if (!SpendEnergy(cost)) return;
                    grid.Till(cell.x, cell.y);
                    Session.AddVar(QuestLog.Stats.Tilled, 1);
                    AudioService.PlayIfAvailable(Sfx.Hoe);
                    ActionPuff.Burst(_map.CellCenter(cell), new Color(0.45f, 0.30f, 0.16f), 3, Fx.DigDirt);
                    break;

                case ToolType.WateringCan:
                    if (!grid.IsTilled(cell.x, cell.y)) return;
                    if (grid.TryGetTile(cell.x, cell.y, out var t) && t.Watered) return;
                    if (!SpendEnergy(cost)) return;
                    grid.Water(cell.x, cell.y);
                    Session.AddVar(QuestLog.Stats.Watered, 1);
                    AudioService.PlayIfAvailable(Sfx.Water);
                    ActionPuff.Burst(_map.CellCenter(cell), new Color(0.35f, 0.65f, 0.95f), 3, Fx.WaterDrop);
                    break;

                case ToolType.Rod:
                    Cast(cell);
                    return;

                case ToolType.Sword:
                    var held = Session.Backpack.Get(Session.State.SelectedHotbar);
                    var combat = GetComponent<PlayerCombat>();
                    if (combat != null && held != null) combat.Swing(held.ItemId);
                    return;

                case ToolType.Axe:
                case ToolType.Pickaxe:
                case ToolType.Scythe:
                    // The axe and pickaxe take a placed chest, machine or sprinkler back (no energy needed).
                    if (tool != ToolType.Scythe && TryPickUp(cell)) return;
                    cost = Mathf.Max(1, Mathf.RoundToInt(ToolModel.EnergyCost(EnergyCost(tool), tier) * Professions.EnergyMultiplier(Session.State)));
                    if (!SwingAtNode(tool, tier, cost, cell)) return;
                    _view.RefreshNode(cell);
                    break;

                default:
                    return;
            }
            _view.RefreshCell(cell);

            // Lets other systems react to the player finishing an action that cost energy (for example the late-night
            // stay-awake check, T-046).
            if (cost > 0) ServiceLocator.Get<EventBus>().Publish(new EnergyActionCompleted(tool.ToString(), cost));
        }

        // Picks forage on the cell, if there is any. Returns true if the interaction was used up (picked or refused).
        bool TryForage(Vector3Int cell)
        {
            var nodes = Session.GetNodes(_map.MapId);
            if (!nodes.TryGet(cell.x, cell.y, out var node)) return false;
            var def = Session.Nodes.Get(node.TypeId);
            if (def == null || def.Tool != ToolType.None) return false;
            if (!string.IsNullOrEmpty(def.DropItemId) && !Session.Backpack.CanAdd(def.DropItemId, def.DropMax))
            {
                Session.Toast(L.Get("toast.inventory_full"));
                return true;
            }

            var day = Session.Clock.Now.TotalDays;
            var result = nodes.Gather(cell.x, cell.y, Session.Nodes.Get, WeatherRoller.Unit(cell.x * 53 + cell.y * 19 + day, Session.State.WorldSeed));
            var quality = ForageModel.Quality(Session.GetSkillLevel(SkillIds.Foraging), Session.Luck + Professions.LuckBonus(Session.State, ProfessionEffect.LuckForaging),
                WeatherRoller.Unit(cell.x * 97 + cell.y * 41 + day * 3, Session.State.WorldSeed ^ 0x7F4A7C15));
            if (result.DropCount > 0) Session.Backpack.Add(result.DropItemId, result.DropCount, quality);
            Session.AddVar(QuestLog.Stats.Foraged, 1);
            Session.AddSkillXp(result.Skill, result.Xp);
            AudioService.PlayIfAvailable(Sfx.Harvest);
            _view.RefreshNode(cell);
            return true;
        }

        // A swing of the axe, pickaxe or scythe at the node on a cell. Returns true if the swing happened (and cost energy).
        bool SwingAtNode(ToolType tool, int tier, int cost, Vector3Int cell)
        {
            var nodes = Session.GetNodes(_map.MapId);
            if (!nodes.TryGet(cell.x, cell.y, out var node)) return false;
            var def = Session.Nodes.Get(node.TypeId);
            if (def == null || def.Tool != tool) return false;          // the wrong tool for this: nothing happens
            if (tier < def.MinToolTier) { Session.Toast(L.Get("toast.tool_too_weak")); return false; }

            // Clearing it must not lose the loot: check the backpack before spending energy.
            var clears = node.Hp - ToolModel.Damage(tier) <= 0;
            if (clears && !string.IsNullOrEmpty(def.DropItemId) && !Session.Backpack.CanAdd(def.DropItemId, def.DropMax))
            {
                Session.Toast(L.Get("toast.inventory_full"));
                return false;
            }
            if (!SpendEnergy(cost)) return false;

            var roll = WeatherRoller.Unit(cell.x * 131 + cell.y * 17 + Session.Clock.Now.TotalDays * 7, Session.State.WorldSeed);
            var result = nodes.Hit(cell.x, cell.y, tool, tier, Session.Nodes.Get, roll);
            ActionPuff.Burst(_map.CellCenter(cell), result.Outcome == NodeHit.Cleared ? new Color(0.7f, 0.62f, 0.45f) : new Color(0.75f, 0.75f, 0.75f), result.Outcome == NodeHit.Cleared ? 7 : 3,
                tool == ToolType.Axe ? Fx.ChopChip : tool == ToolType.Pickaxe ? Fx.OreSpark : Fx.Dust);
            if (result.Outcome == NodeHit.Cleared)
            {
                if (!string.IsNullOrEmpty(result.DropItemId) && result.DropCount > 0)
                    Session.Backpack.Add(result.DropItemId, result.DropCount);
                Session.AddSkillXp(result.Skill, result.Xp);
                AudioService.PlayIfAvailable(Sfx.Harvest);
            }
            else
            {
                AudioService.PlayIfAvailable(tool == ToolType.Scythe ? Sfx.Harvest : Sfx.Hoe);
            }
            return true;
        }

        // ---- animals ---------------------------------------------------------------------------------------------------------

        void PlaceAnimal(ItemDefinition item)
        {
            var row = AnimalDefaults.Find(item.PlaceableId);
            if (row == null || _map.MapId != row.Value.Building) { Session.Toast(L.Get("animal.wrong_place", L.Get("map." + (row?.Building ?? MapIds.Coop)))); return; }
            if (!AnimalRules.HasRoom(Session.State, _map.MapId)) { Session.Toast(L.Get("animal.full")); return; }
            var name = L.Get("animal.default_name", L.Get("item." + item.Id + ".name"), AnimalRules.In(Session.State, _map.MapId).Count + 1);
            var animal = AnimalRules.Add(Session.State, row.Value.Id, System.Guid.NewGuid().ToString("N").Substring(0, 8), name);
            if (animal == null) return;
            Session.Backpack.Remove(item.Id, 1);
            var cell = TargetCell;
            AnimalManager.Current?.Spawn(animal, cell.x, cell.y);
            Session.Toast(L.Get("animal.welcome", name));
        }

        // ---- fishing ---------------------------------------------------------------------------------------------------------

        public const int FishingEnergy = 5;

        Vector3Int _castCell;

        void Cast(Vector3Int cell)
        {
            var spot = FishSpots.ForMap(_map.MapId);
            if (spot == null || !_map.IsWater(cell)) { Session.Toast(L.Get("fishing.no_water")); return; }
            if (!ServiceLocator.TryGet<IUiService>(out var ui) || !SpendEnergy(FishingEnergy)) return;
            var bait = Session.Backpack.Has(FishDefaults.Bait);
            if (bait) Session.Backpack.Remove(FishDefaults.Bait, 1);
            _castCell = cell;
            ActionPuff.Burst(_map.CellCenter(cell), new Color(0.75f, 0.9f, 1f), 1, Fx.Splash);          // the line hits the water
            var n = Session.AddVar("stat.casts", 1);
            float R(int k) => WeatherRoller.Unit(n * 97 + k * 13, Session.State.WorldSeed ^ 0x3C6EF372);
            var eligible = FishingModel.Eligible(FishDefaults.Rows, spot, Session.World);
            var fishing = new FishingSession(eligible, Session.GetSkillLevel(SkillIds.Fishing),
                Session.Luck + Professions.LuckBonus(Session.State, ProfessionEffect.LuckFishing), bait, R(1), R(2), R(3), R(4), Professions.BiteSpeed(Session.State));
            FishingPose.For(gameObject).Show(_map.CellCenter(cell));
            ui.ShowFishing(fishing, OnFished);
        }

        void OnFished(FishingSession done)
        {
            FishingPose.For(gameObject).Hide();
            if (done == null) return;
            if (!done.Caught || !done.Fish.HasValue) { Session.Toast(L.Get("fishing.escaped")); return; }
            var fish = done.Fish.Value;
            ActionPuff.Burst(_map.CellCenter(_castCell), new Color(0.75f, 0.9f, 1f), 4, Fx.WaterDrop);          // a fish breaks the surface
            if (Session.Backpack.Add(fish.ItemId, 1, done.Quality) > 0) { Session.Toast(L.Get("toast.inventory_full")); return; }
            Hoist(fish.ItemId);
            Session.AddVar("stat.fished", 1);
            Session.AddSkillXp(SkillIds.Fishing, FishingModel.Xp(fish));
            Session.Toast(L.Get(done.Perfect ? "fishing.perfect" : "fishing.caught", L.Get("item." + fish.ItemId + ".name")));
        }

        // Holds a fresh find over the head for a moment.
        void Hoist(string itemId)
        {
            if (Session.Db.TryGetItem(itemId, out var item)) HeldItem.For(gameObject).Hoist(item.Icon);
        }

        // ---- placing objects, fertilizer ------------------------------------------------------------------------------------

        void PlaceObject(ItemDefinition item)
        {
            var def = Session.Placeables.Get(item.PlaceableId);
            if (def == null) return;
            var cell = TargetCell;
            if (def.FarmingOnly && !_map.AllowFarming) { Session.Toast(L.Get("placeable.farm_only")); return; }
            if (def.Kind == PlaceableKind.Decor)
            {
                if (!DecorRules.AllowedOn(_map.MapId)) { Session.Toast(L.Get("placeable.decor_where")); return; }
                if (DecorRules.BlocksDoor(cell, def.Walkable, DoorCells())) { Session.Toast(L.Get("placeable.decor_door")); return; }
            }
            var grid = Session.GetGrid(_map.MapId);
            var objects = Session.GetObjects(_map.MapId);
            if (cell == _map.WorldToCell(_player.CellSamplePoint) && !def.Walkable && def.Kind != PlaceableKind.Sprinkler) { Session.Toast(L.Get("placeable.blocked")); return; }
            if (!_map.CanPlaceAt(cell) || objects.Has(cell.x, cell.y) || grid.IsTilled(cell.x, cell.y) || Session.GetNodes(_map.MapId).Has(cell.x, cell.y))
            {
                Session.Toast(L.Get("placeable.blocked"));
                return;
            }
            var placed = objects.Place(def, cell.x, cell.y, System.Guid.NewGuid().ToString("N").Substring(0, 10));
            if (placed == null) return;
            Session.Backpack.Remove(item.Id, 1);
            PlacedObjectsView.Current?.Spawn(placed);
            AudioService.PlayIfAvailable(Sfx.Plant);
        }

        // Takes a placed object at the cell back into the backpack. Returns true if something was there (picked up or refused).
        bool TryPickUp(Vector3Int cell)
        {
            if (!Session.GetObjects(_map.MapId).Has(cell.x, cell.y)) return false;
            var id = Session.GetObjects(_map.MapId).At(cell.x, cell.y).Id;
            switch (PlacedInteractions.PickUp(Session, _map.MapId, cell.x, cell.y))
            {
                case PlacedInteractions.PickUpResult.Ok: PlacedObjectsView.Current?.Despawn(id); AudioService.PlayIfAvailable(Sfx.Harvest); break;
                case PlacedInteractions.PickUpResult.NotEmpty: Session.Toast(L.Get("placeable.not_empty")); break;
                case PlacedInteractions.PickUpResult.NoRoom: Session.Toast(L.Get("toast.inventory_full")); break;
            }
            return true;
        }

        void Fertilize(ItemDefinition item)
        {
            var cell = TargetCell;
            var grid = Session.GetGrid(_map.MapId);
            if (!grid.IsTilled(cell.x, cell.y)) { Session.Toast(L.Get("toast.fertilizer_needs_soil")); return; }
            if (grid.FertilizerAt(cell.x, cell.y) == item.Id) return;
            grid.Fertilize(cell.x, cell.y, item.Id);
            Session.Backpack.Remove(item.Id, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _view.RefreshCell(cell);
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
            if (!grid.Plantable(crop, season)) { Session.Toast(L.Get("toast.wrong_season")); return false; }
            if (!grid.Plant(cell.x, cell.y, crop, season)) return false;

            Session.Backpack.Remove(seed.Id, 1);
            Session.AddVar(QuestLog.Stats.Planted, 1);
            AudioService.PlayIfAvailable(Sfx.Plant);
            _view.RefreshCell(cell);
            return true;
        }

        void Eat(ItemDefinition food)
        {
            if (Session.State.Energy >= Session.State.MaxEnergy && Session.State.Health >= Session.State.MaxHealth) return;
            Session.Backpack.Remove(food.Id, 1);
            Session.RestoreEnergy(food.EnergyRestore);
            Combat.Heal(Session, food.EnergyRestore / 2);
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
                var fertilizer = grid.FertilizerAt(cell.x, cell.y);
                if (grid.TryHarvest(cell.x, cell.y, CropLookup, out var result))
                {
                    var quality = CropQuality.Roll(Session.GetSkillLevel(SkillIds.Farming), fertilizer, Session.Luck + Professions.LuckBonus(Session.State, ProfessionEffect.LuckFarming),
                        WeatherRoller.Unit(cell.x * 61 + cell.y * 29 + Session.Clock.Now.TotalDays * 5, Session.State.WorldSeed ^ 0x2545F491));
                    Session.Backpack.Add(result.ItemId, result.Count, quality);
                    Hoist(result.ItemId);
                    Session.AddVar(QuestLog.Stats.Harvested, 1);
                    Session.AddSkillXp(SkillIds.Farming, result.Xp);
                    AudioService.PlayIfAvailable(Sfx.Harvest);
                    ActionPuff.Burst(_map.CellCenter(cell), new Color(0.45f, 0.78f, 0.30f), 3, Fx.HarvestPop);
                    _view.RefreshCell(cell);
                }
                return;
            }

            // 1b. Something wild to pick up by hand (forage).
            if (TryForage(cell)) return;

            // 1c. A villager standing there.
            if (NpcAt(cell) is NpcActor villager)
            {
                villager.Interact(this);
                return;
            }

            // 1c2. A farm animal.
            var animalHere = AnimalManager.Current != null ? AnimalManager.Current.ActorAt(cell) : null;
            if (animalHere != null)
            {
                animalHere.Interact(this);
                return;
            }

            // 1d. A chest, machine, sprinkler or scarecrow the player placed.
            var placedHere = PlacedObjectsView.Current != null ? PlacedObjectsView.Current.At(cell) : null;
            if (placedHere != null)
            {
                placedHere.Interact(this);
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
