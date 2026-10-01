using System.Collections.Generic;
using UnityEngine;

namespace Farm.Data
{
    // Static data for one kind of item. The Id is a stable string used in saves: never rename a shipped Id.
    [CreateAssetMenu(menuName = "Farm/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string _nameKey;
        [SerializeField] string _descriptionKey;
        [SerializeField] Sprite _icon;
        [SerializeField] ItemCategory _category;
        [SerializeField] int _maxStack = 999;
        [SerializeField] int _sellPrice;
        [SerializeField] int _buyPrice;          // 0 = not sold in shops
        [SerializeField] int _energyRestore;
        [SerializeField] ToolType _toolType;
        [SerializeField] string _cropId;         // for seeds: the crop this plants

        // Shops that stock this item (shop ids such as "general"). Empty = sold nowhere: content must opt in to a
        // shop, so items from optional content packs never appear in the main store by accident.
        [SerializeField] string[] _soldIn = new string[0];
        // Optional extra condition (see Conditions) that must hold for the shop to offer it today.
        [SerializeField] string _saleCondition;

        public string Id => _id;
        public string NameKey => _nameKey;
        public string DescriptionKey => _descriptionKey;
        public Sprite Icon => _icon;
        public ItemCategory Category => _category;
        public int MaxStack => Mathf.Max(1, _maxStack);
        public int SellPrice => _sellPrice;
        public int BuyPrice => _buyPrice;
        public int EnergyRestore => _energyRestore;
        public ToolType ToolType => _toolType;
        public string CropId => _cropId;
        public IReadOnlyList<string> SoldIn => _soldIn;
        public string SaleCondition => _saleCondition;
        public bool IsTool => _toolType != ToolType.None;

        // Used by editor content generation and tests.
        public static ItemDefinition Create(string id, ItemCategory category, int maxStack = 999, int sellPrice = 0,
            int buyPrice = 0, int energyRestore = 0, ToolType toolType = ToolType.None, string cropId = null, Sprite icon = null,
            string[] soldIn = null, string saleCondition = null)
        {
            var item = CreateInstance<ItemDefinition>();
            item._id = id;
            item.name = id;
            item._nameKey = $"item.{id}.name";
            item._descriptionKey = $"item.{id}.desc";
            item._category = category;
            item._maxStack = maxStack;
            item._sellPrice = sellPrice;
            item._buyPrice = buyPrice;
            item._energyRestore = energyRestore;
            item._toolType = toolType;
            item._cropId = cropId;
            item._icon = icon;
            item._soldIn = soldIn ?? new string[0];
            item._saleCondition = saleCondition;
            return item;
        }

        public void SetIcon(Sprite icon) => _icon = icon;
    }
}
