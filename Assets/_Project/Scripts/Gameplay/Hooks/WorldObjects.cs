using System;
using System.Collections.Generic;

namespace Farm.Gameplay
{
    // Kinds of things that exist in the world and that other layers may want to find, mark or remove.
    public static class WorldObjectKinds
    {
        public const string Animal = "animal";
        public const string PlantProduct = "plant";      // harvested produce (and standing crops, if a source offers them)
        public const string CraftedItem = "crafted";     // artisan goods and crafted items in chests, machines, bins
    }

    // A stable reference to one thing in the world. Id is unique within its kind (an animal id, a "chestId:slot" for
    // an item stack, a "map:x,y" for a crop tile...). Sources define the format; others treat it as opaque.
    public readonly struct WorldObjectRef : IEquatable<WorldObjectRef>
    {
        public readonly string Kind;
        public readonly string Id;
        public readonly string MapId;     // where it is (may be empty for things carried by the player)
        public readonly string ItemId;    // item or animal type, for display and filtering

        public WorldObjectRef(string kind, string id, string mapId = "", string itemId = "")
        {
            Kind = kind;
            Id = id;
            MapId = mapId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
        }

        public bool Equals(WorldObjectRef other) => Kind == other.Kind && Id == other.Id;
        public override bool Equals(object obj) => obj is WorldObjectRef o && Equals(o);
        public override int GetHashCode() => ((Kind ?? string.Empty).GetHashCode() * 397) ^ (Id ?? string.Empty).GetHashCode();
        public override string ToString() => $"{Kind}:{Id}";
    }

    // Implemented by the core system that owns a kind of world object (animals, chests, machines, crop fields...).
    // Optional layers use GameHooks to enumerate candidates, check they still exist, and consume them, without
    // knowing how the owning system stores them. For example, the mythos layer picks each season's ritual offerings
    // from these sources and later checks whether the player took or used them.
    public interface IWorldObjectSource
    {
        string Kind { get; }
        IEnumerable<WorldObjectRef> Enumerate();
        bool Exists(WorldObjectRef reference);
        // Removes the object from the world (it is sacrificed, eaten...). Returns false if it no longer exists.
        bool TryConsume(WorldObjectRef reference);
    }
}
