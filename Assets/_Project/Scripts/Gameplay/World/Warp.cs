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

        // Optional gate (see Conditions): the warp only works while it holds, e.g. "!weather:rain" or
        // "flag:woods.open && hour<22". When blocked, the message key (if any) is shown as a toast.
        [SerializeField] string _condition;
        [SerializeField] string _blockedMessageKey;

        public string TargetMap { get => _targetMap; set => _targetMap = value; }
        public string TargetSpawn { get => _targetSpawn; set => _targetSpawn = value; }
        public string Condition { get => _condition; set => _condition = value; }
        public string BlockedMessageKey { get => _blockedMessageKey; set => _blockedMessageKey = value; }

        void Reset() => GetComponent<Collider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (!IsOpen(out var session))
            {
                if (session != null && !string.IsNullOrEmpty(_blockedMessageKey)) session.Toast(L.Get(_blockedMessageKey));
                return;
            }
            MapTravel.GoTo(_targetMap, _targetSpawn);
        }

        public bool IsOpen(out GameSession session)
        {
            ServiceLocator.TryGet(out session);
            if (string.IsNullOrWhiteSpace(_condition)) return true;
            if (session == null || !session.InGame) return false;
            return Conditions.Evaluate(_condition, session.World);
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
