using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A seat that a villager keeps for the player ("the good stool is yours", "a chair by the window"). Sitting a while restores a little
    // energy, once a day per seat. The words name who kept it once the two are friends.
    public sealed class SeatSpot : MonoBehaviour, IInteractable
    {
        public const int Rest = 10;
        public const int FriendHearts = 3;

        [SerializeField] string _npcId;
        [SerializeField] string _seatKey;            // names the seat's strings: seat.<key>.friend / seat.<key>.plain

        public string NpcId { get => _npcId; set => _npcId = value; }
        public string SeatKey { get => _seatKey; set => _seatKey = value; }

        public string HoverLabel => L.Get("hover.seat");

        public static string DayVar(string seatKey) => "seat." + seatKey + ".day";

        // May this seat be used for rest today? (pure)
        public static bool CanRest(int lastDay, int today) => lastDay != today + 1;

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var today = s.Clock.Now.TotalDays;
            var friend = !string.IsNullOrEmpty(_npcId) && Conditions.Evaluate($"hearts:{_npcId}>={FriendHearts}", s.World);
            var line = L.Get("seat." + _seatKey + (friend ? ".friend" : ".plain"));
            if (!CanRest(s.GetVar(DayVar(_seatKey)), today)) { s.Toast(line); if (TryGetComponent<SitSpot>(out var again)) again.Interact(player); return; }
            s.SetVar(DayVar(_seatKey), today + 1);                  // stored +1 so that day 0 is not "never"
            s.RestoreEnergy(Rest);
            s.Toast(line + " " + L.Get("seat.rested", Rest));
            if (TryGetComponent<SitSpot>(out var sit)) sit.Interact(player);              // and the player sits down for a while
        }
    }
}
