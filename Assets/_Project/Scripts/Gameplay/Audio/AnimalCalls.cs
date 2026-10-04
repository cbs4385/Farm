namespace Farm.Gameplay
{
    // Which call each farm animal makes. A rabbit says nothing; every other kind has a synthetic call (see SfxSynth).
    public static class AnimalCalls
    {
        public static Sfx? For(string animalType)
        {
            switch (animalType)
            {
                case "chicken": return Sfx.Cluck;
                case "cow": return Sfx.Moo;
                case "sheep": return Sfx.Baa;
                case "goat": return Sfx.Baa;
                case "duck": return Sfx.Quack;
                default: return null;
            }
        }
    }
}
