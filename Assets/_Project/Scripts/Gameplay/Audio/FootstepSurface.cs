namespace Farm.Gameplay
{
    // What a step sounds like on each kind of ground (by the ground tile's name): firm for paths, cobbles and floors, scuffing for sand,
    // soft for everything else (grass, dirt, forest floor, tilled soil).
    public static class FootstepSurface
    {
        public static Sfx For(string groundTileName)
        {
            switch (groundTileName)
            {
                case "tile_path":
                case "tile_cobble":
                case "tile_floor_wood":
                case "tile_wall":
                case "tile_door":
                    return Sfx.StepHard;
                case "tile_sand":
                    return Sfx.StepSand;
                default:
                    return Sfx.Step;
            }
        }
    }
}
