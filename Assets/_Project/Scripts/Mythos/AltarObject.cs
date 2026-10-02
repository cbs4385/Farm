using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The altar in the clearing of Harrow Wood. During a ritual it shows the offerings being laid down and dissolving (as
    // plain lights at mild intensity); the player can take an offering from it (disrupting the ritual) or, outside rituals,
    // use it for the sealing and the initiation, through the altar's dialogue.
    public sealed class AltarObject : MonoBehaviour, IInteractable
    {
        readonly List<SpriteRenderer> _lights = new List<SpriteRenderer>();
        GameSession _session;

        void Start() => _session = ServiceLocator.Get<GameSession>();

        void Update()
        {
            if (_session == null || !_session.InGame || !MythosLevel.On(_session)) { Clear(); return; }
            var save = RitualDirector.Load(_session);
            var running = RitualDirector.IsRitualDay(_session.Clock.Now) && save.ResolvedSeason != save.PlannedSeason && !save.Sealed;
            if (!running) { Clear(); return; }
            var offset = RitualDirector.Offset(_session.Clock.Now.MinuteOfDay);
            while (_lights.Count < save.Plan.Count)
            {
                var go = new GameObject("offering", typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(-1.2f + _lights.Count * 0.6f, 1.2f, 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = RuntimeSprites.Square(MythosLevel.Full(_session) ? new Color(0.95f, 0.85f, 0.4f) : new Color(0.8f, 0.9f, 1f));
                sr.sortingOrder = 6;
                _lights.Add(sr);
            }
            for (var i = 0; i < _lights.Count && i < save.Plan.Count; i++)
            {
                var slot = save.Plan[i];
                var on = offset >= RitualModel.SacrificeStart(i) && !slot.Taken && !slot.Missing;
                var left = 1f - Mathf.Clamp01((offset - RitualModel.SacrificeStart(i)) / (float)RitualModel.MinutesPerSacrifice);
                _lights[i].enabled = on && !slot.Consumed;
                _lights[i].color = new Color(1f, 1f, 1f, left);
            }
        }

        void Clear()
        {
            foreach (var l in _lights) if (l != null) l.enabled = false;
        }

        public void Interact(PlayerActions player)
        {
            var s = player.Session;
            if (!MythosLevel.On(s)) return;
            var taken = RitualDirector.TakeFromAltar(s);
            if (taken != null) { s.Toast(L.Get("mythos.altar.taken")); return; }
            s.BeginDialogue("mythos.altar");
        }
    }
}
