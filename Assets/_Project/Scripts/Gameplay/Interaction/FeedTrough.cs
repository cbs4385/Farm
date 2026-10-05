using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // The trough in a coop or barn: Interact fills it for every hungry animal, using feed (fibre) from the backpack.
    public sealed class FeedTrough : MonoBehaviour, IInteractable
    {
        public string HoverLabel => Farm.Core.L.Get("hover.trough");

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            var building = s.State.CurrentMap;
            var hungry = AnimalRules.In(s.State, building).FindAll(a => !a.FedToday).Count;
            if (AnimalRules.In(s.State, building).Count == 0) { s.Toast(L.Get("animal.none")); return; }
            if (hungry == 0) { s.Toast(L.Get("animal.all_fed")); return; }
            var fed = AnimalRules.FeedAll(s.State, building, s.Backpack);
            s.Toast(fed == 0 ? L.Get("animal.no_feed") : L.Get("animal.fed", fed));
            if (fed > 0) AnimalManager.Current?.Munch();
            s.NotifyChanged();
        }
    }
}
