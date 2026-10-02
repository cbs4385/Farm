using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // Something left in the woods that can be picked up once: a relic for the true fix, or seeds. `FlagId` records it.
    public sealed class RelicPickup : MonoBehaviour, IInteractable
    {
        public string ItemId;
        public int Count = 1;
        public string FlagId;
        GameSession _session;

        void Start() => _session = ServiceLocator.Get<GameSession>();

        void Update()
        {
            if (_session == null || !_session.InGame) return;
            var gone = _session.HasFlag(FlagId) || !MythosLevel.On(_session);
            foreach (var r in GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = !gone;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = !gone;
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!MythosLevel.On(s) || s.HasFlag(FlagId)) return;
            if (!s.Backpack.CanAdd(ItemId, Count)) { s.Toast(L.Get("toast.inventory_full")); return; }
            s.Backpack.Add(ItemId, Count);
            s.SetFlag(FlagId);
            s.AddVar(MythosIds.Vars.Lore, 1);
            s.AddVar(MythosIds.Vars.Dread, Mathf.RoundToInt(3 * MythosLevel.Scale(s)), 0, DreadModel.Max);
            s.Toast(L.Get("mythos.found", s.Db.TryGetItem(ItemId, out var item) ? L.Get(item.NameKey) : ItemId));
        }
    }
}
