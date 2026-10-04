namespace Farm.Gameplay
{
    // A scene asked for a piece of music ("stop" ends it). Nothing plays it until the audio pass (T-061, T-133): the
    // audio service will subscribe to this.
    public readonly struct MusicCue
    {
        public readonly string Name;
        public MusicCue(string name) { Name = name; }
        public bool IsStop => Name == "stop";
    }
}
