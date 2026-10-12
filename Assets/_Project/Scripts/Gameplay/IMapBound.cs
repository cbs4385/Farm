namespace Farm.Gameplay
{
    // A component that lives in a map scene and needs the run's services. MapSceneController gives it the MapContext once, when the map has been set up
    // (it replaces the component asking a static for what it uses; docs/adr/0006-retire-the-service-locator.md). Code that creates one later calls Bind itself.
    public interface IMapBound
    {
        void Bind(MapContext map);
    }
}
