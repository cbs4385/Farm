using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // Trigger volume that moves the player to another map scene.
    [RequireComponent(typeof(Collider2D))]
    public sealed class Warp : MonoBehaviour
    {
        [SerializeField] string _targetMap;
        [SerializeField] string _targetSpawn = "default";

        public string TargetMap { get => _targetMap; set => _targetMap = value; }
        public string TargetSpawn { get => _targetSpawn; set => _targetSpawn = value; }

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            MapTravel.GoTo(_targetMap, _targetSpawn);
        }
    }

    public static class MapTravel
    {
        public static void GoTo(string mapId, string spawnId)
        {
            if (!ServiceLocator.TryGet<GameSession>(out var session) || !session.InGame) return;
            if (!ServiceLocator.TryGet<SceneLoader>(out var loader) || loader.IsLoading) return;

            session.State.CurrentMap = mapId;
            session.State.SpawnPoint = spawnId;
            loader.Load(mapId);
        }
    }
}
