using UnityEngine;

namespace Farm.Data
{
    public enum PlaceableKind { Chest, Machine, Sprinkler, Scarecrow }

    // Something the player can set down in the world: a chest, a machine, a sprinkler, a scarecrow. The item that places
    // it (`ItemId`) names it by `PlaceableId`. Ids are stable: placed objects are saved by TypeId.
    [CreateAssetMenu(menuName = "Farm/Placeable")]
    public sealed class PlaceableDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] PlaceableKind _kind;
        [SerializeField] string _station;          // machines: the recipe station (furnace, keg, jar)
        [SerializeField] Sprite _sprite;
        [SerializeField] bool _farmingOnly;        // only on maps where farming is allowed (sprinklers, scarecrows)
        [SerializeField] int _capacity;            // chests: slots
        [SerializeField] int _range;               // sprinklers: 1 = the four neighbours, 2 = the eight around, 3 = the 24 around; scarecrows: cells

        public string Id => _id;
        public string ItemId => $"machine.{_id}";
        public PlaceableKind Kind => _kind;
        public string Station => _station;
        public Sprite Sprite => _sprite;
        public bool FarmingOnly => _farmingOnly;
        public int Capacity => _capacity;
        public int Range => _range;
        public string NameKey => $"item.{ItemId}.name";

        public static PlaceableDefinition Create(string id, PlaceableKind kind, string station = null, bool farmingOnly = false,
            int capacity = 0, int range = 0)
        {
            var p = CreateInstance<PlaceableDefinition>();
            p._id = id;
            p.name = id;
            p._kind = kind;
            p._station = station;
            p._farmingOnly = farmingOnly;
            p._capacity = capacity;
            p._range = range;
            return p;
        }

        public void SetSprite(Sprite sprite) => _sprite = sprite;
    }
}
