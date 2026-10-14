using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The old chest on the mine's 30th floor that holds the three sealing relics: opening it takes the ones not yet found. It is gone once they all are.
    public sealed class RelicChest : MonoBehaviour, IInteractable
    {
        GameSession _session;

        public string HoverLabel => L.Get("hover.relic_chest");

        public void Init(GameSession session, FarmMap map, Vector3Int cell)
        {
            _session = session;
            transform.position = map.CellCenter(cell);
            var sr = gameObject.AddComponent<SpriteRenderer>();
            LitSprites.Apply(sr);
            sr.sortingOrder = 5;
            if (session.Db.TryGetItem(ItemIds.Machine("chest"), out var chest)) sr.sprite = chest.Icon;
            gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        void Update()
        {
            if (_session == null || !_session.InGame || MythosRelics.AnyLeft(_session)) return;
            Destroy(gameObject);
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!MythosLevel.On(s)) return;
            var taken = new List<string>();
            for (var i = 0; i < MythosData.Relics.Length; i++)
            {
                if (s.HasFlag(MythosRelics.FlagOf(i))) continue;
                var id = MythosData.Relics[i];
                if (!s.Backpack.CanAdd(id, 1)) { s.Toast(L.Get("toast.inventory_full")); break; }
                s.Backpack.Add(id, 1);
                s.SetFlag(MythosRelics.FlagOf(i));
                s.AddVar(MythosIds.Vars.Lore, 1);
                s.AddVar(MythosIds.Vars.Dread, Mathf.RoundToInt(3 * MythosLevel.Scale(s)), 0, DreadModel.Max);
                taken.Add(s.Db.TryGetItem(id, out var item) ? L.Get(item.NameKey) : id);
            }
            if (taken.Count > 0) s.Toast(L.Get("mythos.found", string.Join(", ", taken)));
        }
    }
}
