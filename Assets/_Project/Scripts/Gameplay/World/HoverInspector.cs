using Farm.Core;
using Farm.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Farm.Gameplay
{
    // Created on every map: when the mouse rests over something in the world, a small label names it (playtest feedback, 2026-10-04: not every
    // icon was obvious, such as the shipping bin). It looks in the same order as the Interact key: a villager, an animal, a placed object,
    // then anything with a collider (bin, bed, doors...). Off when the player turns it off, and never over a menu or during a scene.
    public sealed class HoverInspector : MonoBehaviour
    {
        GameSession _session;
        FarmMap _map;
        string _label;

        public static string Current { get; private set; }
        public static void ResetForTests() { Current = null; }

        public void Init(GameSession session, FarmMap map)
        {
            _session = session; _map = map;
        }

        void Update()
        {
            var mouse = Mouse.current;
            var settings = ServiceLocator.TryGet<SettingsStore>(out var store) ? store.Current : null;
            var ui = ServiceLocator.TryGet<IUiService>(out var service) ? service : null;
            var blocked = ServiceLocator.TryGet<InputService>(out var input) && input.GameplayBlocked;
            if (mouse == null || ui == null || _session == null || !_session.InGame || _map == null || blocked || ui.AnyModalOpen
                || ui.PointerOverUi || (settings != null && !settings.HoverLabels) || Camera.main == null)
            {
                Clear(ui);
                return;
            }
            var position = mouse.position.ReadValue();
            var world = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, -Camera.main.transform.position.z));
            var label = LabelAt(world);
            if (label == null) { Clear(ui); return; }
            _label = label;
            Current = label;
            ui.ShowHover(label, position);
        }

        void Clear(IUiService ui)
        {
            if (_label == null && Current == null) return;
            _label = null; Current = null;
            ui?.HideHover();
        }

        void OnDestroy()
        {
            Current = null;
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.HideHover();
        }

        string LabelAt(Vector3 world)
        {
            var cell = _map.WorldToCell(world);
            // A villager or animal is two cells tall on screen: the head is the cell above the feet.
            foreach (var c in new[] { cell, cell + Vector3Int.down })
            {
                if (NpcManager.Current != null && NpcManager.Current.ActorAt(c) is IInteractable villager && villager.HoverLabel != null) return villager.HoverLabel;
                if (AnimalManager.Current != null && AnimalManager.Current.ActorAt(c) is IInteractable animal && animal.HoverLabel != null) return animal.HoverLabel;
            }
            var placed = PlacedObjectsView.Current != null ? PlacedObjectsView.Current.At(cell) : null;
            if (placed != null && placed.HoverLabel != null) return placed.HoverLabel;
            foreach (var hit in Physics2D.OverlapPointAll(world))
            {
                var interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.HoverLabel != null) return interactable.HoverLabel;
                var warp = hit.GetComponentInParent<Warp>();
                if (warp != null && warp.HoverLabel != null) return warp.HoverLabel;
            }
            return CropLabel(cell);
        }

        // Last, so that anything solid wins: tilled soil and what is planted in it.
        string CropLabel(Vector3Int cell)
        {
            var grid = _session.GetGrid(_map.MapId);
            if (grid != null && grid.TryGetTile(cell.x, cell.y, out var tile)) return CropInfo.Label(tile, _session.Db);
            return ForageLabel(cell);
        }

        // A wild plant, with a reminder to leave the last one of its kind so that more can grow.
        string ForageLabel(Vector3Int cell)
        {
            var nodes = _session.GetNodes(_map.MapId);
            if (!nodes.TryGet(cell.x, cell.y, out var node)) return null;
            var def = _session.Nodes.Get(node.TypeId);
            if (def == null || def.Tool != ToolType.None || !_session.Db.TryGetItem(def.DropItemId, out var item)) return null;
            return L.Get(ForageRules.IsLastOfItsKind(nodes, node.TypeId) ? "hover.forage_last" : "hover.forage", L.Get(item.NameKey));
        }
    }
}
