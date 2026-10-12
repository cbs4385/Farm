using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // What the player's action classes share: the session, the map and the player's own parts, the aim, and the few things every action needs (spending energy, holding a find up).
    public sealed class ActionContext
    {
        public GameSession Session;
        public FarmMap Map;
        public FarmMapView View;
        public PlayerController Player;
        public PlayerActions Actions;
        public GameObject Body;
        public PlayerAim Aim;

        public IUiService Ui => Session.Game?.Ui;
        public Vector3Int Target => Aim.Target;
        public Vector3Int PlayerCell => Map.WorldToCell(Player.CellSamplePoint);

        public bool SpendEnergy(int cost)
        {
            if (Session.TrySpendEnergy(cost)) return true;
            Session.Toast(L.Get("toast.too_tired"));
            AudioService.PlayIfAvailable(Sfx.Error);
            return false;
        }

        // Holds a fresh find over the head for a moment.
        public void Hoist(string itemId)
        {
            if (Session.Db.TryGetItem(itemId, out var item)) HeldItem.For(Body).Hoist(item.Icon);
        }
    }
}
