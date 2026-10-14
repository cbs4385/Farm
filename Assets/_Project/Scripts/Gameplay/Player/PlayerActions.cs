using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // Turns input into world actions: hotbar selection, tool use on the tile in front, harvesting, interacting.
    public sealed class PlayerActions : MonoBehaviour, IMapBound
    {
        public const int HoeEnergy = 2;
        public const int WateringEnergy = 2;
        public const int AxeEnergy = 3;
        public const int PickaxeEnergy = 3;
        public const int ScytheEnergy = 2;
        public const int ForageEnergy = 1;          // picking wild things by hand (playtest 2026-10-14: it cost nothing)
        public const int FishingEnergy = FishingActions.FishingEnergy;

        [SerializeField] PlayerController _player;
        [SerializeField] FarmMap _map;
        [SerializeField] FarmMapView _view;
        [SerializeField] Transform _cursor;

        InputService _input;

        public GameSession Session { get; private set; }
        public PlayerController Controller => _player;
        PlayerAim _aim;
        FishingActions _fishing;
        PlacementActions _placement;
        ItemActions _items;
        HandInteraction _hand;

        // Sits the player on a seat (SitSpot): they rest there until they move.
        public void Sit(SitSpot spot)
        {
            if (_player.Sit(spot)) Session.Toast(L.Get("toast.sit"));
        }

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

        public void Bind(MapContext map)
        {
            Session = map.Session;
            _input = map.Input;
            var context = new ActionContext { Session = Session, Map = _map, View = _view, Player = _player, Actions = this, Body = gameObject };
            _aim = context.Aim = new PlayerAim(Session, _map, _player, _input);
            _fishing = new FishingActions(context);
            _placement = new PlacementActions(context);
            _items = new ItemActions(context, _fishing, _placement);
            _hand = new HandInteraction(context, _placement);
            HeldItem.For(gameObject).Bind(map);                // the carry poses
        }

        public bool MouseAiming => _aim != null && _aim.MouseAiming;
        public Vector3Int Target => _aim.Target;

        void Update()
        {
            if (Session == null || !Session.InGame) return;

            var ui = Session.Game?.Ui;
            _aim.Update(ui);
            if (_cursor != null) _cursor.position = _map.CellCenter(_aim.Target);

            if (_input.GameplayBlocked) return;

            var capacity = Session.Backpack.Capacity;
            for (var i = 0; i < InputNames.HotbarSlots; i++)
                if (_input.Hotbar(i).WasPressedThisFrame()) SelectHotbar(HotbarRows.Pick(Session.State.SelectedHotbar, i, capacity), "number key " + (i + 1));
            if (_input.HotbarNext.WasPressedThisFrame()) SelectHotbar(HotbarRows.Step(Session.State.SelectedHotbar, 1, capacity), "next (" + PressedBy(_input.HotbarNext) + ")");
            if (_input.HotbarPrev.WasPressedThisFrame()) SelectHotbar(HotbarRows.Step(Session.State.SelectedHotbar, -1, capacity), "previous (" + PressedBy(_input.HotbarPrev) + ")");
            if (_input.RowNext.WasPressedThisFrame()) SelectHotbar(HotbarRows.ChangeRow(Session.State.SelectedHotbar, 1, capacity), "next row (" + PressedBy(_input.RowNext) + ")");
            if (_input.RowPrev.WasPressedThisFrame()) SelectHotbar(HotbarRows.ChangeRow(Session.State.SelectedHotbar, -1, capacity), "previous row (" + PressedBy(_input.RowPrev) + ")");

            if (_player.Seated)
            {
                // Seated, the buttons for using things stand the player up (moving does too) and do nothing else.
                if (_input.UseTool.WasPressedThisFrame() || _input.Interact.WasPressedThisFrame()) _player.StandUp();
            }
            else
            {
                if (_input.UseTool.WasPressedThisFrame() && !_hand.ClickLandedOnTheHud()) { if (HandInteraction.MouseClicked && _hand.ClickUsesTheHand()) _hand.Interact(); else UseSelected(); }
                if (_input.Interact.WasPressedThisFrame()) _hand.Interact();
                if (_input.Rotate.WasPressedThisFrame()) TurnPlacement();
            }

            if (ui == null) return;
            if (_input.Inventory.WasPressedThisFrame()) ui.ToggleInventory();
            if (_input.Pause.WasPressedThisFrame()) ui.ShowPause();
            if (_input.Menu.WasPressedThisFrame()) ui.ShowGameMenu();
            if (_input.Crafting.WasPressedThisFrame()) ui.ShowGameMenu(MenuTabs.Crafting);
            if (_input.Journal.WasPressedThisFrame()) ui.ShowGameMenu(MenuTabs.Journal);
        }

        // The control that was pressed, for the log: "Mouse/scroll/up", "Keyboard/period"...
        static string PressedBy(UnityEngine.InputSystem.InputAction action) =>
            action.activeControl != null ? action.activeControl.path.TrimStart('/') : "?";

        public void SelectHotbar(int slot, string source = "other")
        {
            _placement.ResetTurns();
            var before = Session.State.SelectedHotbar;
            Session.State.SelectedHotbar = Mathf.Clamp(slot, 0, Mathf.Max(InputNames.HotbarSlots, Session.Backpack.Capacity) - 1);
            HotbarRows.Note(before, Session.State.SelectedHotbar, source);
            Session.Bus.Publish(new StatsChanged());
        }

        // Quarter turns the next piece of furniture from the hotbar will be set down with (R turns it; picking another slot resets it).
        public int PlacementTurns => _placement.Turns;

        // R with furniture in hand turns it before it is set down. Does nothing while the mallet is carrying something (it turns that instead).
        public void TurnPlacement() => _placement.TurnPlacement();

        // E, or a click on something used by hand.
        public void Interact() => _hand.Interact();

        // Does a quest wait on this villager for something the player carries? Then a click talks to them instead of giving them what is in the hand.
        public static bool TalksFirst(GameSession s, string npcId) => QuestTargets.Wants(s, npcId);

        public void UseSelected()
        {
            var stack = Session.Backpack.Get(Session.State.SelectedHotbar);
            if (stack == null || !Session.Db.TryGetItem(stack.ItemId, out var item)) return;

            // An item used on a villager is a gift.
            if (item.Category != ItemCategory.Tool && _hand.VillagerToGive() is NpcActor npc)
            {
                // A quest that waits on this villager comes before a gift (playtest 2026-10-13: the rock in hand was given away instead of handing in the quest).
                if (TalksFirst(Session, npc.Definition.Id)) { npc.Interact(this); return; }
                var result = NpcInteractions.Gift(Session, npc.Definition, Session.State.SelectedHotbar);
                NpcInteractions.ToastFor(Session, result);
                if (result == GiftResult.Given) ActionPuff.Hearts(npc.transform.position + Vector3.up * 1.2f, npc.GetComponent<WalkBob>());
                return;
            }

            switch (item.Category)
            {
                case ItemCategory.Tool: _items.UseTool(item.ToolType); break;
                case ItemCategory.Seed: _placement.Plant(item); break;
                case ItemCategory.Resource when item.Id == ItemIds.Acorn: _placement.PlantAcorn(item); break;
                case ItemCategory.Food: _items.Eat(item); break;
                case ItemCategory.Fish: _hand.Interact(); break;          // a fish held out to the village cat is its food
                case ItemCategory.Misc when item.Id == CatBond.FoodItemId: _hand.Interact(); break;
                case ItemCategory.Artisan when item.EnergyRestore > 0: _items.Eat(item); break;          // jam and pickles can be eaten as well as sold
                case ItemCategory.Machine: _placement.PlaceObject(item); break;
                case ItemCategory.Furniture: _placement.PlaceObject(item); break;
                case ItemCategory.Animal: _placement.PlaceAnimal(item); break;
                case ItemCategory.Fertilizer: _placement.Fertilize(item); break;
            }
        }

        // The villager whose picture the mouse is over: the picture is two cells tall (the head is in the cell above their feet), and a counter may stand between (playtest
        // 2026-10-11: "I can only click on Tilda from the side and not the front"). Next to the player, or two cells away across a counter.
        public static NpcActor PointedVillager(Vector3Int playerCell, Vector3Int pointerCell, System.Collections.Generic.IEnumerable<NpcActor> actors, System.Func<Vector3, Vector3Int> toCell,
            System.Func<Vector3Int, bool> isCounter)
        {
            NpcActor best = null;
            var bestDistance = int.MaxValue;
            foreach (var actor in actors)
            {
                if (actor == null) continue;
                var feet = toCell(actor.transform.position);
                if (pointerCell != feet && pointerCell != feet + Vector3Int.up) continue;
                var dx = feet.x - playerCell.x;
                var dy = feet.y - playerCell.y;
                var d = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                if (d > 2 || d >= bestDistance) continue;
                if (d == 2)                                                      // two cells away only across a counter, in a straight or diagonal line
                {
                    if (dx != 0 && dy != 0 && Mathf.Abs(dx) != Mathf.Abs(dy)) continue;
                    var middle = new Vector3Int(playerCell.x + Mathf.Clamp(dx, -1, 1), playerCell.y + Mathf.Clamp(dy, -1, 1), 0);
                    if (isCounter == null || !isCounter(middle)) continue;
                }
                best = actor; bestDistance = d;
            }
            return best;
        }
    }
}
