using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Somewhere crafting can take ingredients from: the backpack, a chest, a machine's finished output.
    public interface IIngredientSource
    {
        int Count(string itemId);
        int Remove(string itemId, int count);
    }

    public sealed class InventorySource : IIngredientSource
    {
        readonly Inventory _inventory;
        public InventorySource(Inventory inventory) { _inventory = inventory; }
        public int Count(string itemId) => _inventory.Count(itemId);
        public int Remove(string itemId, int count) => _inventory.Remove(itemId, count);
    }

    // The goods a machine has finished and that sit in it waiting to be collected.
    public sealed class MachineOutputSource : IIngredientSource
    {
        readonly PlacedObject _machine;
        public MachineOutputSource(PlacedObject machine) { _machine = machine; }
        public int Count(string itemId) => _machine.OutputItemId == itemId ? _machine.OutputCount : 0;

        public int Remove(string itemId, int count)
        {
            var take = Math.Min(count, Count(itemId));
            if (take <= 0) return 0;
            _machine.OutputCount -= take;
            if (_machine.OutputCount <= 0)
            {
                _machine.RecipeId = null; _machine.OutputItemId = null; _machine.OutputCount = 0; _machine.ReadyAt = 0;
            }
            return take;
        }
    }

    // What lies within reach of the player while crafting (playtest 2026-10-14): chests first (the nearest first), then finished machine output. The backpack always
    // comes before these. Everything placed is the player's own; nothing belonging to a villager is on the list.
    public static class NearbyStock
    {
        public const int Radius = 5;

        public static List<IIngredientSource> Gather(GameSession session, int cellX, int cellY)
        {
            var found = new List<IIngredientSource>();
            if (session == null || !session.InGame) return found;
            var grid = session.GetObjects(session.State.CurrentMap);
            var now = ObjectGrid.Minute(session.Clock.Now);
            var chests = new List<(int d, IIngredientSource source)>();
            var machines = new List<(int d, IIngredientSource source)>();
            foreach (var obj in grid.All)
            {
                var d = (obj.X - cellX) * (obj.X - cellX) + (obj.Y - cellY) * (obj.Y - cellY);
                if (d > Radius * Radius) continue;
                var def = session.Placeables.Get(obj.TypeId);
                if (def == null) continue;
                if (def.Kind == Farm.Data.PlaceableKind.Chest) chests.Add((d, new InventorySource(grid.ChestOf(obj))));
                else if (def.Kind == Farm.Data.PlaceableKind.Machine && CraftingRules.IsReady(obj, now) && !string.IsNullOrEmpty(obj.OutputItemId))
                    machines.Add((d, new MachineOutputSource(obj)));
            }
            chests.Sort((a, b) => a.d.CompareTo(b.d));
            machines.Sort((a, b) => a.d.CompareTo(b.d));
            foreach (var c in chests) found.Add(c.source);
            foreach (var m in machines) found.Add(m.source);
            return found;
        }
    }
}
