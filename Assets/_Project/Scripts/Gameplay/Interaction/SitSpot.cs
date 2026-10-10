using System.Collections.Generic;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A chair, armchair, couch or bench that anyone may sit on (playtest 2026-10-09): the player with the Interact key, and a villager who is standing still beside it.
    // The seated player rests (SeatRest) until they move. One sitter at a time.
    public sealed class SitSpot : MonoBehaviour, IInteractable
    {
        public const float Reach = 1.55f;                                // a villager standing this close (cell centre to seat centre) takes the seat

        static readonly List<SitSpot> AllSpots = new List<SitSpot>();
        public static IReadOnlyList<SitSpot> All => AllSpots;

        [SerializeField] Vector2Int _facing = Vector2Int.down;           // the way a sitter faces

        public Vector2Int Facing { get => _facing; set => _facing = value; }
        public Component Occupant { get; set; }

        // Where a sitter's feet are: a little below the middle of the seat, so that the picture sits on it.
        public Vector3 SeatPosition => transform.position + new Vector3(0f, -0.3f, 0f);

        public string HoverLabel => L.Get(Occupant == null ? "hover.sit" : "hover.sit_taken");

        void OnEnable() => AllSpots.Add(this);
        void OnDisable() { AllSpots.Remove(this); Occupant = null; }

        static SitSpot() => TestResets.Add(ResetForTests);
        public static void ResetForTests() => AllSpots.Clear();

        // Is this seat within reach of a villager standing on a cell?
        public bool Reaches(FarmMap map, Vector3Int cell) => Vector3.Distance(map.CellCenter(cell), transform.position) <= Reach;

        public void Interact(PlayerActions player)
        {
            if (Occupant != null && Occupant != player.Controller)
            {
                player.Session.Toast(L.Get("toast.sit_taken"));
                return;
            }
            player.Sit(this);
        }
    }

    // Resting in a seat: how much energy comes back for each game minute spent sitting (pure, so it can be tested). Slow on purpose: about 24 an hour.
    public static class SeatRest
    {
        public const float PerMinute = 0.4f;

        // The whole points of energy earned by `minutes` of sitting; `carry` keeps the fraction left over for next time.
        public static int Gain(float minutes, ref float carry)
        {
            carry += Mathf.Max(0f, minutes) * PerMinute;
            var whole = (int)carry;
            carry -= whole;
            return whole;
        }
    }
}
