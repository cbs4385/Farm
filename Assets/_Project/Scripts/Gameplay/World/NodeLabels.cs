using Farm.Core;
using Farm.Data;

namespace Farm.Gameplay
{
    // The hover name of something a tool clears (a tree, a bush, a log, a stump, a rock, a weed, an ore vein). Wild plants that are picked by hand are named after what they give
    // instead (HoverInspector.ForageLabel); the "picked" stage of one is not a thing worth a name.
    public static class NodeLabels
    {
        public static bool NamesItself(ResourceNodeDefinition def) => def != null && (def.Tool == ToolType.Axe || def.Tool == ToolType.Pickaxe || def.Tool == ToolType.Scythe);

        public static string For(ResourceNodeDefinition def) => NamesItself(def) ? L.Get(def.NameKey) : null;
    }
}
