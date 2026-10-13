using System.Collections.Generic;
using Farm.Core;
using Farm.Data;
using UnityEngine;

namespace Farm.Gameplay
{
    // The rack at the side of the farmhouse where a new farmer's tools hang (playtest 2026-10-13: "start the player with all tools on a tool rack"). Interacting takes the
    // tools that are still on it into the backpack; a tool that does not fit stays. Games begun before the rack have nothing on it.
    public sealed class ToolRack : MonoBehaviour, IInteractable
    {
        public const string StockedFlag = "rack.stocked";          // set by a new game that starts with its tools on the rack
        public static readonly string[] Tools = { ItemIds.Hoe, ItemIds.WateringCan, ItemIds.Axe, ItemIds.Pickaxe, ItemIds.Scythe, ItemIds.Sword, ItemIds.Hammer };

        public int Reach => 1;

        static string TakenFlag(string toolId) => "rack.taken." + toolId;

        // The tools still hanging on the rack (pure).
        public static List<string> Remaining(GameSession s)
        {
            var left = new List<string>();
            if (!s.HasFlag(StockedFlag)) return left;
            foreach (var id in Tools) if (!s.HasFlag(TakenFlag(id))) left.Add(id);
            return left;
        }

        public string HoverLabel
        {
            get
            {
                var s = GameContext.Current?.Session;
                var left = s != null && s.InGame ? Remaining(s).Count : 0;
                return left > 0 ? L.Get("hover.tool_rack", left) : L.Get("hover.tool_rack.empty");
            }
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var left = Remaining(s);
            if (left.Count == 0) { s.Toast(L.Get("toast.rack_empty")); return; }
            var taken = 0;
            foreach (var id in left)
            {
                if (!s.Backpack.CanAdd(id, 1)) continue;
                s.Backpack.Add(id, 1);
                s.SetFlag(TakenFlag(id));
                taken++;
            }
            s.Toast(taken == 0 ? L.Get("toast.inventory_full") : taken < left.Count ? L.Get("toast.rack_some", taken) : L.Get("toast.rack_all", taken));
            if (taken > 0) AudioService.PlayIfAvailable(Sfx.Pickup);
            s.NotifyChanged();
        }
    }
}
