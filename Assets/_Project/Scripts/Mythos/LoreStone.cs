using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // A carved stone in Harrow Wood: reading it gives a piece of lore (once) and a short, unsettling text.
    public sealed class LoreStone : MonoBehaviour, IInteractable
    {
        public string Key;          // string key of the text (Key + ".mild" is the gentler version)
        public string FlagId;
        GameSession _session;

        void Start() => _session = ServiceLocator.Get<GameSession>();

        void Update()
        {
            if (_session == null || !_session.InGame) return;
            var on = MythosLevel.On(_session);
            foreach (var r in GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = on;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = on;
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!MythosLevel.On(s)) return;
            if (!s.HasFlag(FlagId))
            {
                s.SetFlag(FlagId);
                s.AddVar(MythosIds.Vars.Lore, 1);
                s.SetFlag(MythosIds.Flags.CultKnown);
            }
            if (ServiceLocator.TryGet<IUiService>(out var ui)) ui.ShowMessage(MythosLevel.Full(s) ? Key : Key + ".mild");
        }
    }
}
