using UnityEngine;

namespace Farm.Data
{
    // Something on the ground a tool can clear: a tree, a stump, a rock, a weed. Pure data: the interaction code only
    // looks at these fields. A destroyed node may leave another node behind (a tree leaves a stump). Ids are saved
    // and must never change once shipped. The name is the string key "node.<id>".
    [CreateAssetMenu(menuName = "Farm/Resource Node")]
    public sealed class ResourceNodeDefinition : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] ToolType _tool;
        [SerializeField] int _hitPoints = 1;        // damage needed; a tool deals 1 plus its tier per swing
        [SerializeField] int _minToolTier;          // 0 basic ... 3 gold: weaker tools cannot damage it
        [SerializeField] string _dropItemId;
        [SerializeField] int _dropMin = 1;
        [SerializeField] int _dropMax = 1;
        [SerializeField] string _skill;             // skill that gains the XP
        [SerializeField] int _xp = 1;
        [SerializeField] string _leavesNodeId;      // what is left standing when it is cleared (a stump), or empty
        [SerializeField] bool _solid;               // blocks walking
        [SerializeField] float _spawnWeight = 1f;   // relative share when clutter is generated (0 = never spawns)
        [SerializeField] Sprite _sprite;

        public string Id => _id;
        public string NameKey => "node." + _id;
        public ToolType Tool => _tool;
        public int HitPoints => _hitPoints;
        public int MinToolTier => _minToolTier;
        public string DropItemId => _dropItemId;
        public int DropMin => _dropMin;
        public int DropMax => _dropMax;
        public string Skill => _skill;
        public int Xp => _xp;
        public string LeavesNodeId => _leavesNodeId;
        public bool Solid => _solid;
        public float SpawnWeight => _spawnWeight;
        public Sprite Sprite => _sprite;

        public void SetSprite(Sprite sprite) => _sprite = sprite;

        public static ResourceNodeDefinition Create(string id, ToolType tool, int hitPoints, int minToolTier, string dropItemId,
            int dropMin, int dropMax, string skill, int xp, string leavesNodeId, bool solid, float spawnWeight)
        {
            var d = CreateInstance<ResourceNodeDefinition>();
            d._id = id;
            d.name = id;
            d._tool = tool;
            d._hitPoints = hitPoints;
            d._minToolTier = minToolTier;
            d._dropItemId = dropItemId;
            d._dropMin = dropMin;
            d._dropMax = dropMax;
            d._skill = skill;
            d._xp = xp;
            d._leavesNodeId = leavesNodeId;
            d._solid = solid;
            d._spawnWeight = spawnWeight;
            return d;
        }
    }

    // The base game's clutter. The editor content tool writes these out as assets (and assigns sprites by the
    // convention obj_<id>); tests and projects without assets use them directly.
    public static class NodeDefaults
    {
        public const string Weed = "weed";
        public const string Rock = "rock";
        public const string Boulder = "boulder";
        public const string Tree = "tree";
        public const string Stump = "stump";

        public static ResourceNodeDefinition[] CreateAll() => new[]
        {
            ResourceNodeDefinition.Create(Weed, ToolType.Scythe, 1, 0, ItemIds.Fiber, 1, 2, "foraging", 1, null, false, 55f),
            ResourceNodeDefinition.Create(Rock, ToolType.Pickaxe, 2, 0, ItemIds.Stone, 1, 2, "mining", 3, null, true, 25f),
            ResourceNodeDefinition.Create(Boulder, ToolType.Pickaxe, 6, 1, ItemIds.Stone, 4, 6, "mining", 12, null, true, 3f),
            ResourceNodeDefinition.Create(Tree, ToolType.Axe, 6, 0, ItemIds.Wood, 8, 12, "foraging", 12, Stump, true, 12f),
            ResourceNodeDefinition.Create(Stump, ToolType.Axe, 3, 0, ItemIds.Wood, 3, 5, "foraging", 4, null, true, 5f),
        };
    }
}
