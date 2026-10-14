using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The on-screen body of a placed object (chest, machine, sprinkler, scarecrow).
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlacedObjectActor : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Definition != null ? Farm.Core.L.Get("item.machine." + Definition.Id + ".name") : null;

        // Machines and scarecrows are drawn half as big again (playtest 2026-10-11: "the crafted items need to be bigger"): they stand on their cell and rise above it,
        // as the chest does. Chests, sprinklers and furniture keep their size.
        public const float MachineScale = 1.5f;
        public static float ScaleFor(PlaceableKind kind) => kind == PlaceableKind.Machine || kind == PlaceableKind.Scarecrow ? MachineScale : 1f;

        // The farm's own props, when they have been put in the backpack and set down again, keep their bigger size.
        // Big furniture (the wardrobe, the bookshelf, the dining table, the armchair, the bench) is drawn as large as the machines, so that it is not smaller than the person beside it.
        public static float ScaleFor(PlaceableDefinition def) => def != null && (System.Array.IndexOf(CraftingDefaults.FarmProps, def.Id) >= 0 || System.Array.IndexOf(CraftingDefaults.LargeDecor, def.Id) >= 0) ? MachineScale : ScaleFor(def.Kind);

        SpriteRenderer _renderer;

        public PlacedObject Object { get; private set; }
        public PlaceableDefinition Definition { get; private set; }

        public void Setup(PlacedObject obj, PlaceableDefinition def, Vector3 position)
        {
            Object = obj;
            Definition = def;
            name = $"{def.Id}_{obj.Id}";
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f * (obj.Turns & 3));
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = def.Sprite;
            _renderer.sortingOrder = 4;

            var scale = ScaleFor(def);
            if (scale != 1f)
            {
                transform.localScale = new Vector3(scale, scale, 1f);
                transform.position = position + new Vector3(0f, (scale - 1f) * 0.5f, 0f);          // the foot of the picture stays on the cell
            }
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = Vector2.one / scale;                                                          // one cell, however big the picture is
            box.offset = new Vector2(0f, -(scale - 1f) * 0.5f / scale);
            // Sprinklers can be walked over; everything else blocks the way. Both can be found by a point query.
            box.isTrigger = def.Kind == PlaceableKind.Sprinkler || def.Walkable;
            if (def.Walkable) _renderer.sortingOrder = 1;       // a rug lies under everything that walks over it
        }

        // Machines show whether they are working or done.
        public void RefreshLook(int nowMinute)
        {
            if (Definition.Kind != PlaceableKind.Machine) return;
            _renderer.color = CraftingRules.IsReady(Object, nowMinute) ? new Color(1f, 1f, 0.6f)
                : CraftingRules.IsBusy(Object) ? new Color(0.72f, 0.72f, 0.78f) : Color.white;
        }

        public void Interact(PlayerActions player)
        {
            var session = player.Session;
            var held = session.Backpack.Get(session.State.SelectedHotbar);
            PlacedInteractions.Use(session, Object, Definition, held?.ItemId);
            RefreshLook(ObjectGrid.Minute(session.Clock.Now));
        }
    }
}
