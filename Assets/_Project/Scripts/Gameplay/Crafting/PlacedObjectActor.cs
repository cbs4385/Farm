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

        SpriteRenderer _renderer;

        public PlacedObject Object { get; private set; }
        public PlaceableDefinition Definition { get; private set; }

        public void Setup(PlacedObject obj, PlaceableDefinition def, Vector3 position)
        {
            Object = obj;
            Definition = def;
            name = $"{def.Id}_{obj.Id}";
            transform.position = position;
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = def.Sprite;
            _renderer.sortingOrder = 4;

            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
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
