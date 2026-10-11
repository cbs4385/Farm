namespace Farm.Gameplay
{
    public enum VillageDamage { Tree, Rock, Soil }

    // The player hit, felled, broke or dug something on the village green (published by PlayerActions; VillageWatch listens). Cleared: it is done (the tree is down, the rock is
    // broken, the soil is dug), not only hit.
    public readonly struct VillageDamaged
    {
        public readonly int X, Y;
        public readonly bool Cleared;
        public readonly VillageDamage Kind;
        public VillageDamaged(int x, int y, bool cleared, VillageDamage kind = VillageDamage.Tree) { X = x; Y = y; Cleared = cleared; Kind = kind; }

        public static bool TryKindOf(string nodeTypeId, out VillageDamage kind)
        {
            switch (nodeTypeId)
            {
                case Farm.Data.NodeDefaults.Tree:
                case Farm.Data.NodeDefaults.Stump: kind = VillageDamage.Tree; return true;
                case Farm.Data.NodeDefaults.Rock:
                case Farm.Data.NodeDefaults.Boulder: kind = VillageDamage.Rock; return true;
                default: kind = VillageDamage.Tree; return false;
            }
        }
    }
}
