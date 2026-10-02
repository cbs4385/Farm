using UnityEngine;

namespace Farm.Data
{
    public enum CropKind { Vegetable, Fruit, Flower, Grain }

    // A plantable crop. GrowthDays[i] = watered days spent in stage i. Stage GrowthDays.Length is "mature".
    // StageSprites therefore has GrowthDays.Length + 1 entries.
    [CreateAssetMenu(menuName = "Farm/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] string _seedItemId;
        [SerializeField] string _harvestItemId;
        [SerializeField] int[] _growthDays = { 1, 1, 1, 1 };
        [SerializeField] SeasonMask _seasons = SeasonMask.Spring;
        [SerializeField] int _regrowDays;        // 0 = harvest removes the plant
        [SerializeField] int _harvestXp = 8;
        [SerializeField] Sprite[] _stageSprites;

        // Optional condition (see Conditions) for the crop to grow overnight, e.g. "var:dread>=20". While it does
        // not hold the plant stays dormant: it neither grows nor dies (unless the season is wrong).
        [SerializeField] string _growCondition;

        // Fruit trees: planted any time, grow for GrowthDays in total, never die with the season, and bear one fruit a day
        // in FruitSeasons once mature (harvesting leaves the tree standing).
        [SerializeField] bool _isTree;
        [SerializeField] SeasonMask _fruitSeasons;
        [SerializeField] CropKind _kind;

        public string Id => _id;
        public string SeedItemId => _seedItemId;
        public string HarvestItemId => _harvestItemId;
        public int[] GrowthDays => _growthDays;
        public SeasonMask Seasons => _seasons;
        public int RegrowDays => _regrowDays;
        public int HarvestXp => _harvestXp;
        public Sprite[] StageSprites => _stageSprites;
        public string GrowCondition => _growCondition;
        public int MatureStage => _growthDays.Length;
        public bool IsTree => _isTree;
        public SeasonMask FruitSeasons => _fruitSeasons;
        public CropKind Kind => _kind;

        public Sprite SpriteForStage(int stage)
        {
            if (_stageSprites == null || _stageSprites.Length == 0) return null;
            return _stageSprites[Mathf.Clamp(stage, 0, _stageSprites.Length - 1)];
        }

        public static CropDefinition Create(string id, int[] growthDays, SeasonMask seasons, int regrowDays = 0,
            Sprite[] stageSprites = null)
        {
            var crop = CreateInstance<CropDefinition>();
            crop._id = id;
            crop.name = id;
            crop._seedItemId = $"seed.{id}";
            crop._harvestItemId = $"crop.{id}";
            crop._growthDays = growthDays;
            crop._seasons = seasons;
            crop._regrowDays = regrowDays;
            crop._stageSprites = stageSprites;
            return crop;
        }

        public CropDefinition WithKind(CropKind kind) { _kind = kind; return this; }

        public CropDefinition AsTree(SeasonMask fruitSeasons)
        {
            _isTree = true;
            _fruitSeasons = fruitSeasons;
            _seasons = SeasonMask.All;   // a sapling can be planted in any season and survives all of them
            _kind = CropKind.Fruit;
            return this;
        }

        public void SetHarvestXp(int xp) => _harvestXp = xp;

        public void SetStageSprites(Sprite[] sprites) => _stageSprites = sprites;
        public void SetGrowCondition(string condition) => _growCondition = condition;
    }
}
