using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    public enum FestivalStationKind { Maypole, Scoreboard, JudgingTable, FeastTable, LanternRack }

    // A thing on the festival ground the player can use with Interact: the maypole (dance), the Tidewatch scoreboard, the judging table, the feast table and the lantern rack.
    // They stand in the village scene all day of the festival (a ConditionalObject holder); what they do is in FestivalActivities.
    public sealed class FestivalStation : MonoBehaviour, IInteractable, IMapBound
    {
        [SerializeField] FestivalStationKind _kind;
        [SerializeField] GameObject _lit;            // the lantern rack's lit picture, shown once the player has hung a lantern

        GameSession _session;

        public FestivalStationKind Kind { get => _kind; set => _kind = value; }
        public GameObject Lit { get => _lit; set => _lit = value; }

        public int Reach => 1;
        public string HoverLabel => L.Get("hover.festival." + _kind.ToString().ToLowerInvariant());

        public void Bind(MapContext map)
        {
            _session = map.Session;
            Refresh();
        }

        public void Interact(PlayerActions player)
        {
            FestivalActivities.Use(player, _kind);
            Refresh();
        }

        void Refresh()
        {
            if (_lit == null || _session == null || !_session.InGame) return;
            var row = Festivals.Get("winter");
            _lit.SetActive(row != null && _session.HasFlag(Festivals.Key(row, _session.Clock.Now.Year, FestivalActivities.LanternDone)));
        }
    }
}
