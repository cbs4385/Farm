using System.Collections.Generic;
using UnityEngine;

namespace Farm.Data
{
    // A bundle of extra items and crops merged into the GameDatabase at startup. Put assets of this type under
    // any Resources/Packs folder. This is how optional layers (e.g. the mythos content) add data without touching
    // the core database or core code.
    [CreateAssetMenu(menuName = "Farm/Content Pack")]
    public sealed class ContentPack : ScriptableObject
    {
        public const string ResourceFolder = "Packs";

        [SerializeField] string _packId;
        [SerializeField] List<ItemDefinition> _items = new List<ItemDefinition>();
        [SerializeField] List<CropDefinition> _crops = new List<CropDefinition>();

        public string PackId => _packId;
        public IReadOnlyList<ItemDefinition> Items => _items;
        public IReadOnlyList<CropDefinition> Crops => _crops;

        public static ContentPack Create(string packId, IEnumerable<ItemDefinition> items, IEnumerable<CropDefinition> crops)
        {
            var pack = CreateInstance<ContentPack>();
            pack._packId = packId;
            pack.name = packId;
            if (items != null) pack._items.AddRange(items);
            if (crops != null) pack._crops.AddRange(crops);
            return pack;
        }
    }
}
