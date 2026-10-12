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
        PlayerActions _actions;
        SettingsStore _store;
        IUiService _ui;
        InputService _input;
        Camera _camera;                                       // looked up once, not four times a frame
        string _label;

        public static string Current { get; private set; }
        public static string Why { get; private set; }          // why no label is up (for the bug report)
        static HoverInspector() => TestResets.Add(ResetForTests);
        public static void ResetForTests() { Current = null; }

        GameContext _game;

        public void Init(GameSession session, FarmMap map)
        {
            _session = session; _map = map;
        }

        public void Init(MapContext map)
        {
            _game = map.Game;
            Init(map.Session, map.Map);
        }

        void Update()
        {
            var mouse = Mouse.current;
            // With a pad the label follows the cell the player faces, as a mouse's does the pointer (playtest 2026-10-09: a controller player never saw what a thing was).
            var pad = ControlPrompts.Kind == InputKind.Gamepad;          // any pad, a generic joystick too: the label follows the target cell, not a mouse that nobody is using
            _store = _game?.Settings;
            _ui = _game?.Ui;
            _input = _game?.Input;
            if (_camera == null) _camera = Camera.main;
            var settings = _store != null ? _store.Current : null;
            var ui = _ui;
            var blocked = _input != null && _input.GameplayBlocked;
            if (_actions == null) _actions = FindAnyObjectByType<PlayerActions>();
            if ((mouse == null && !pad) || ui == null || _session == null || !_session.InGame || _map == null || blocked || ui.AnyModalOpen
                || (!pad && ui.PointerOverUi) || (pad && _actions == null) || (settings != null && !settings.HoverLabels) || _camera == null)
            {
                Why = ui == null || _session == null || !_session.InGame ? "not in a game" : blocked || ui.AnyModalOpen ? "a window is open" : !pad && ui.PointerOverUi ? "the mouse is over a window"
                    : settings != null && !settings.HoverLabels ? "hover labels are off" : "no pointer or target";
                Clear(ui);
                return;
            }
            Vector2 position;
            Vector3 world;
            if (pad)
            {
                world = _map.CellCenter(_actions.Target);
                position = _camera.WorldToScreenPoint(world) + new Vector3(0f, 28f, 0f);
            }
            else
            {
                position = mouse.position.ReadValue();
                world = _camera.ScreenToWorldPoint(new Vector3(position.x, position.y, -_camera.transform.position.z));
            }
            var label = LabelAt(world);
            if (label == null) { Why = pad ? "nothing at the target cell" : "nothing under the mouse"; Clear(ui); return; }
            Why = "shown";
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
            _game?.Ui?.HideHover();
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
                var note = hit.GetComponentInParent<HoverNote>();
                if (note != null && note.HoverLabel != null) return note.HoverLabel;
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
            return L.Get(ForageRules.ReminderKey(ForageRules.IsLastOfItsKind(nodes, node.TypeId) ? "hover.forage_last" : "hover.forage", item.Id), L.Get(item.NameKey));
        }
    }
}
