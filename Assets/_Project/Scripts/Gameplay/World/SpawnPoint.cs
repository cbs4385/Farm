using UnityEngine;

namespace Farm.Gameplay
{
    // Named position where the player appears when entering a map ("default", "bed", "fromHouse"...).
    public sealed class SpawnPoint : MonoBehaviour
    {
        [SerializeField] string _id = "default";
        public string Id { get => _id; set => _id = value; }
    }
}
