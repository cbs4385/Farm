using System.Linq;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The tentacles of Crawling Depths rising round the altar on a ritual night (timing in RitualTentacles). Added to the altar at run time by AltarObject; at any intensity below
    // full, or off the ritual night, they are not there at all.
    public sealed class AltarTentacles : MonoBehaviour
    {
        const float RiseSpeed = 0.35f, SinkSpeed = 0.5f, Fps = 5f;

        readonly SpriteRenderer[] _renderers = new SpriteRenderer[RitualTentacles.Slots];
        readonly float[] _height = new float[RitualTentacles.Slots];
        GameSession _session;

        public int ShownCount => _renderers.Count(r => r != null && r.enabled);
        public float HeightOf(int slot) => _height[slot];

        public void Init(GameSession session) => _session = session;

        void Update()
        {
            var targets = Targets();
            for (var i = 0; i < RitualTentacles.Slots; i++)
            {
                var t = targets == null ? 0f : targets[i];
                _height[i] = Mathf.MoveTowards(_height[i], t, (t > _height[i] ? RiseSpeed : SinkSpeed) * Time.deltaTime);
                Show(i);
            }
        }

        // The height each tentacle aims at now; null when this is not a ritual night at full intensity.
        float[] Targets()
        {
            if (_session == null || !_session.InGame || !MythosLevel.Full(_session)) return null;
            var now = _session.Clock.Now;
            if (!RitualDirector.IsRitualDay(now)) return null;
            var save = RitualDirector.Load(_session);
            if (save.Sealed || save.Plan.Count == 0) return null;
            var offset = RitualDirector.Offset(now.MinuteOfDay);
            var disrupted = save.Plan.Any(p => p.Taken) || (save.ResolvedSeason == save.PlannedSeason && !save.LastSucceeded);
            var targets = new float[RitualTentacles.Slots];
            for (var i = 0; i < targets.Length; i++) targets[i] = RitualTentacles.Target(i, offset, save.Plan.Count, disrupted);
            return targets;
        }

        void Show(int slot)
        {
            var rows = RitualTentacles.Rows(_height[slot], DepthsArt.TentacleH);
            if (rows <= 0)
            {
                if (_renderers[slot] != null) _renderers[slot].enabled = false;
                return;
            }
            var sr = _renderers[slot];
            if (sr == null)
            {
                var go = new GameObject("Tentacle" + slot, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                var (x, y) = RitualTentacles.Place(slot);
                go.transform.localPosition = new Vector3(x, y, 0f);
                sr = _renderers[slot] = go.GetComponent<SpriteRenderer>();
                LitSprites.Apply(sr);
                sr.flipX = slot % 2 == 1;
                sr.sortingOrder = y > 0f ? 1 : 9;               // the ones behind the altar are behind it
            }
            var sprite = DepthsArt.Frame("tentacle_" + (slot % 2 + 1), DepthsArt.TentacleW, DepthsArt.TentacleH, (int)(Time.time * Fps + slot * 1.3f), rows);
            sr.sprite = sprite;
            sr.enabled = sprite != null;
        }
    }
}
