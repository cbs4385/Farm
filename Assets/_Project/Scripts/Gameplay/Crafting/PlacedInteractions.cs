using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // What using a placed object does: open a chest, load or empty a machine, explain a sprinkler. Also placing and picking
    // them up. Pure with respect to the scene (the actors call it), so it is unit tested.
    public static class PlacedInteractions
    {
        public static void Use(GameSession session, PlacedObject obj, PlaceableDefinition def, string heldItemId)
        {
            var now = ObjectGrid.Minute(session.Clock.Now);
            switch (def.Kind)
            {
                case PlaceableKind.Chest:
                    UiAccess.Run(ui => ui.ShowChest(obj.Id));
                    break;

                case PlaceableKind.Machine:
                    UseMachine(session, obj, def, heldItemId, now);
                    break;

                case PlaceableKind.Sprinkler:
                    session.Toast(L.Get("placeable.sprinkler_info", def.Range == 1 ? 4 : def.Range == 2 ? 8 : 24));
                    break;

                case PlaceableKind.Scarecrow:
                    session.Toast(L.Get("placeable.scarecrow_info"));
                    break;

                case PlaceableKind.Decor:       // furniture goes back into the pack with the interact key
                    switch (PickUp(session, session.State.CurrentMap, obj.X, obj.Y))
                    {
                        case PickUpResult.Ok: PlacedObjectsView.Current?.Despawn(obj.Id); AudioService.PlayIfAvailable(Sfx.Harvest); break;
                        case PickUpResult.NoRoom: session.Toast(L.Get("toast.inventory_full")); break;
                    }
                    break;
            }
        }

        static void UseMachine(GameSession session, PlacedObject obj, PlaceableDefinition def, string heldItemId, int now)
        {
            if (CraftingRules.IsReady(obj, now))
            {
                var name = session.Db.TryGetItem(obj.OutputItemId, out var made) ? L.Get(made.NameKey) : obj.OutputItemId;
                if (CraftingRules.TryCollect(obj, session.Backpack, now)) session.Toast(L.Get("machine.collected", name));
                else session.Toast(L.Get("toast.inventory_full"));
                return;
            }
            if (CraftingRules.IsBusy(obj))
            {
                var left = CraftingRules.MinutesLeft(obj, now);
                session.Toast(L.Get("machine.busy", left >= 1440 ? L.Get("machine.days", (left + 1439) / 1440) : L.Get("machine.hours", (left + 59) / 60)));
                return;
            }
            if (string.IsNullOrEmpty(heldItemId)) { session.Toast(L.Get("machine.hint")); return; }

            switch (CraftingRules.TryLoad(obj, def, heldItemId, session.Recipes, session.Backpack, now, out var started))
            {
                case LoadResult.Loaded:
                    session.Toast(L.Get("machine.started", session.Db.TryGetItem(started.OutputItemId, out var item) ? L.Get(item.NameKey) : started.OutputItemId));
                    break;
                case LoadResult.MissingIngredients: session.Toast(L.Get("machine.missing")); break;
                default: session.Toast(L.Get("machine.hint")); break;
            }
        }

        public enum PickUpResult { Ok, NotEmpty, NoRoom, NothingThere }

        // Takes a placed object back into the backpack (chests and machines must be empty).
        public static PickUpResult PickUp(GameSession session, string mapId, int x, int y)
        {
            var grid = session.GetObjects(mapId);
            var obj = grid.At(x, y);
            if (obj == null) return PickUpResult.NothingThere;
            var def = session.Placeables.Get(obj.TypeId);
            if (def == null) return PickUpResult.NothingThere;
            if (def.Kind == PlaceableKind.Chest && !grid.ChestIsEmpty(obj)) return PickUpResult.NotEmpty;
            if (def.Kind == PlaceableKind.Machine && CraftingRules.IsBusy(obj)) return PickUpResult.NotEmpty;
            if (!session.Backpack.CanAdd(def.ItemId, 1)) return PickUpResult.NoRoom;
            grid.Remove(obj.Id);
            session.Backpack.Add(def.ItemId, 1);
            return PickUpResult.Ok;
        }
    }
}
