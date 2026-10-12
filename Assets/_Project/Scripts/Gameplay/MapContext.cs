namespace Farm.Gameplay
{
    // Everything that lives for one loaded map: the run's GameContext and the map's own parts. MapSceneController builds it once the map's objects exist and gives it to the
    // components it creates (their `Init(MapContext)`), in place of the static `Current` singletons (docs/adr/0006-retire-the-service-locator.md).
    public sealed class MapContext
    {
        public GameContext Game;
        public FarmMap Map;
        public FarmMapView View;
        public PlayerController Player;
        public NpcManager Npcs;

        public GameSession Session => Game?.Session;
        public INotifier Notify => Game?.Notify ?? NullNotifier.Instance;
    }
}
