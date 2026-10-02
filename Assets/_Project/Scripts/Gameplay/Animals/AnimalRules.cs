using System;
using System.Collections.Generic;
using System.Linq;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // One farm animal. Saved in GameState.Animals (additive).
    [Serializable]
    public sealed class AnimalState
    {
        public string Id;
        public string Type;          // AnimalDefaults row id
        public string Name;
        public string Building;      // MapIds.Coop or MapIds.Barn
        public int Happiness = 50;   // 0..100
        public bool FedToday;
        public bool PettedToday;
        public int DaysSinceProduct;
        public bool ProductReady;
    }

    public readonly struct AnimalRow
    {
        public readonly string Id, Name, Building, ProductItemId;
        public readonly int Price, IntervalDays, ProductSell;
        public readonly Color Color;
        public AnimalRow(string id, string name, string building, int price, string product, int interval, int productSell, Color color)
        { Id = id; Name = name; Building = building; Price = price; ProductItemId = product; IntervalDays = interval; ProductSell = productSell; Color = color; }

        public string ItemId => "animal." + Id;
    }

    public static class AnimalDefaults
    {
        static Color C(float r, float g, float b) => new Color(r, g, b);

        public static readonly AnimalRow[] Rows =
        {
            new AnimalRow("chicken", "Chicken", MapIds.Coop, 400, "product.egg", 1, 50, C(0.95f, 0.95f, 0.9f)),
            new AnimalRow("duck", "Duck", MapIds.Coop, 600, "product.duck_egg", 2, 95, C(0.85f, 0.8f, 0.55f)),
            new AnimalRow("rabbit", "Rabbit", MapIds.Coop, 800, "product.rabbit_wool", 3, 190, C(0.8f, 0.75f, 0.7f)),
            new AnimalRow("cow", "Cow", MapIds.Barn, 1000, "product.milk", 1, 125, C(0.9f, 0.85f, 0.8f)),
            new AnimalRow("goat", "Goat", MapIds.Barn, 1200, "product.goat_milk", 2, 225, C(0.75f, 0.7f, 0.6f)),
            new AnimalRow("sheep", "Sheep", MapIds.Barn, 1500, "product.wool", 3, 340, C(0.95f, 0.95f, 0.95f)),
        };

        public static AnimalRow Row(string id) => Rows.First(r => r.Id == id);
        public static AnimalRow? Find(string id) => Rows.Any(r => r.Id == id) ? Row(id) : (AnimalRow?)null;

        public static ExtraItemRow[] CreateItems() =>
            Rows.Select(r => new ExtraItemRow(r.ItemId, ItemCategory.Animal, 0, r.Color, buy: r.Price, placeableId: r.Id, soldIn: new[] { "carpenter" }))
                .Concat(Rows.Select(r => new ExtraItemRow(r.ProductItemId, ItemCategory.Artisan, r.ProductSell, Color.Lerp(r.Color, Color.white, 0.4f)))).ToArray();
    }

    // The rules of looking after animals (T-053): feeding, petting, the overnight update, collecting products. Pure.
    public static class AnimalRules
    {
        public const int Capacity = 4;
        public const string Feed = "resource.fiber";

        public static string BuildingFlag(string building) => building == MapIds.Coop ? "farm.coop" : "farm.barn";

        public static List<AnimalState> In(GameState state, string building) => state.Animals.Where(a => a.Building == building).ToList();

        public static bool HasRoom(GameState state, string building) => In(state, building).Count < Capacity;

        // Adds an animal of a type to its building. Null when the building is full or the type is unknown.
        public static AnimalState Add(GameState state, string type, string id, string name)
        {
            var row = AnimalDefaults.Find(type);
            if (row == null || !HasRoom(state, row.Value.Building)) return null;
            var animal = new AnimalState { Id = id, Type = type, Name = name, Building = row.Value.Building };
            state.Animals.Add(animal);
            return animal;
        }

        // Fills the feed trough: each hungry animal in the building takes one feed. Returns how many were fed.
        public static int FeedAll(GameState state, string building, Inventory backpack)
        {
            var fed = 0;
            foreach (var a in In(state, building).Where(x => !x.FedToday))
            {
                if (backpack.Remove(Feed, 1) == 0) break;
                a.FedToday = true;
                fed++;
            }
            return fed;
        }

        // Petting is worth a little happiness, once a day.
        public static bool Pet(AnimalState a)
        {
            if (a.PettedToday) return false;
            a.PettedToday = true;
            a.Happiness = Math.Min(100, a.Happiness + 8);
            return true;
        }

        // Quality of a product follows how happy the animal is.
        public static int Quality(AnimalState a) => a.Happiness >= 90 ? 2 : a.Happiness >= 60 ? 1 : 0;

        public static bool Collect(AnimalState a, Inventory backpack, out string itemId)
        {
            itemId = null;
            var row = AnimalDefaults.Find(a.Type);
            if (row == null || !a.ProductReady) return false;
            if (!backpack.CanAdd(row.Value.ProductItemId, 1, Quality(a))) return false;
            backpack.Add(row.Value.ProductItemId, 1, Quality(a));
            a.ProductReady = false;
            a.DaysSinceProduct = 0;
            itemId = row.Value.ProductItemId;
            return true;
        }

        // Overnight: fed animals grow happier and make their product on schedule; hungry ones sulk and make nothing.
        public static void NewDay(GameState state)
        {
            foreach (var a in state.Animals)
            {
                var row = AnimalDefaults.Find(a.Type);
                if (a.FedToday)
                {
                    a.Happiness = Math.Min(100, a.Happiness + 10);
                    if (row != null && !a.ProductReady && ++a.DaysSinceProduct >= row.Value.IntervalDays) a.ProductReady = true;
                }
                else a.Happiness = Math.Max(0, a.Happiness - 15);
                a.FedToday = false;
                a.PettedToday = false;
            }
        }
    }

    // `animal` objects for optional layers (ADR 0002): enumerate, check, and consume (the animal dissolves into light; nothing
    // is ever shown harming it).
    public sealed class AnimalSource : IWorldObjectSource
    {
        readonly GameSession _session;
        public AnimalSource(GameSession session) { _session = session; }
        public string Kind => WorldObjectKinds.Animal;

        public IEnumerable<WorldObjectRef> Enumerate()
        {
            if (!_session.InGame) yield break;
            foreach (var a in _session.State.Animals) yield return new WorldObjectRef(Kind, a.Id, a.Building, a.Type);
        }

        public bool Exists(WorldObjectRef reference) => _session.InGame && _session.State.Animals.Any(a => a.Id == reference.Id);

        public bool TryConsume(WorldObjectRef reference)
        {
            if (!_session.InGame) return false;
            var removed = _session.State.Animals.RemoveAll(a => a.Id == reference.Id) > 0;
            if (removed) _session.NotifyChanged();
            return removed;
        }
    }
}
