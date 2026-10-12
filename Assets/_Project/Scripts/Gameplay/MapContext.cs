using Farm.Core;
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
        public PlacedObjectsView Placed;
        public AnimalManager Animals;       // only the coop and the barn
        public EnemyManager Enemies;        // only the mine
        public MineController Mine;
        public EventDirector Events;

        public GameSession Session => Game?.Session;
        public EventBus Bus => Game?.Bus;
        public InputService Input => Game?.Input;
        public AtmosphereService Atmosphere => Game?.Atmosphere;
        public INotifier Notify => Game?.Notify ?? NullNotifier.Instance;
    }
}
