using System.Collections.Generic;
using Farm.Core;
using Farm.Gameplay;
using UnityEngine;

namespace Farm.Mythos
{
    // The altar in the clearing of Harrow Wood. On a ritual night at full intensity it stands in a ring of candles under a warm glow, and each offering is shown as what it is,
    // lifted from the stone in a drift of golden motes while it dissolves (2026-10-10); at mild intensity the offerings are plain pale lights and there are no candles. The player can
    // take an offering from it (disrupting the ritual) or, outside rituals, use it for the sealing and the initiation, through the altar's dialogue.
    public sealed class AltarObject : MonoBehaviour, IInteractable, Farm.Gameplay.IMapBound
    {
        const int Candles = 8, Motes = 14;

        readonly List<SpriteRenderer> _lights = new List<SpriteRenderer>();          // mild: a plain light per offering
        readonly List<SpriteRenderer> _flames = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> _motes = new List<SpriteRenderer>();
        SpriteRenderer _glow, _sigil, _offering;
        FlickerLight _light;
        Transform _candleRoot;
        GameSession _session;

        public string HoverLabel => L.Get("hover.altar");

        public bool CandlesLit { get; private set; }
        public bool OfferingShown => _offering != null && _offering.enabled;

        public void Bind(MapContext map)
        {
            _session = map.Session;
            if (GetComponent<AltarTentacles>() == null) gameObject.AddComponent<AltarTentacles>().Init(_session);          // Crawling Depths: they rise on a ritual night at full intensity
        }

        // Is the altar's night on (the candles burning)? The hours the Keepers are out in their robes, on a ritual night. (pure, so it can be tested)
        public static bool NightOn(bool full, bool ritualDay, int minuteOfDay, int participants) =>
            full && ritualDay && MythosOutfitSource.RobedAt(minuteOfDay, participants) && minuteOfDay >= RitualDirector.StartMinute - 90;

        void Update()
        {
            if (_session == null || !_session.InGame || !MythosLevel.On(_session)) { Clear(); return; }
            var save = RitualDirector.Load(_session);
            var now = _session.Clock.Now;
            var full = MythosLevel.Full(_session);
            var ritualDay = RitualDirector.IsRitualDay(now);
            var running = ritualDay && save.ResolvedSeason != save.PlannedSeason && !save.Sealed;

            CandlesLit = !save.Sealed && NightOn(full, ritualDay, now.MinuteOfDay, save.Plan.Count);
            UpdateCandles(CandlesLit);

            if (!running) { ClearOfferings(); return; }
            var offset = RitualDirector.Offset(now.MinuteOfDay);
            if (full) ShowOffering(save, now.MinuteOfDay); else UpdatePlainLights(save, offset);
        }

        // ---- candles and glow (full) ----

        void UpdateCandles(bool lit)
        {
            if (lit && _candleRoot == null) BuildCandles();
            if (_candleRoot == null) return;
            _candleRoot.gameObject.SetActive(lit);
            if (!lit) return;
            for (var i = 0; i < _flames.Count; i++) _flames[i].sprite = RitualFx.Flame((int)(Time.time * 7f + i * 2.3f));
            _glow.color = new Color(1f, 0.78f, 0.4f, 0.3f + 0.06f * Mathf.Sin(Time.time * 2.1f));
            _sigil.color = new Color(1f, 0.82f, 0.45f, 0.38f + 0.14f * Mathf.Sin(Time.time * 1.4f));
            _light.Intensity = OfferingShown ? 1.5f : 1f;
        }

        void BuildCandles()
        {
            _candleRoot = new GameObject("Candles").transform;
            _candleRoot.SetParent(transform, false);
            _glow = Piece("Glow", RitualFx.Glow(), new Vector3(0f, 0.2f, 0f), 3);
            _glow.transform.localScale = Vector3.one * 7f;
            _sigil = Piece("Sigil", RitualFx.Sigil(), new Vector3(0f, -0.15f, 0f), 2);
            _sigil.transform.localScale = Vector3.one * 1.9f;
            _light = new GameObject("Light").AddComponent<FlickerLight>();
            _light.transform.SetParent(_candleRoot, false);
            _light.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            _light.Radius = 7.5f;
            _light.Intensity = 1f;
            _light.Flicker = 0.1f;
            for (var i = 0; i < Candles; i++)
            {
                var angle = i * Mathf.PI * 2f / Candles + 0.2f;
                var at = new Vector3(Mathf.Cos(angle) * 2.4f, Mathf.Sin(angle) * 1.7f - 0.5f, 0f);
                Piece("Candle" + i, RitualFx.Candle(), at, 5);
                _flames.Add(Piece("Flame" + i, RitualFx.Flame(i), at + new Vector3(0f, 0.25f, 0f), 6));
            }
        }

        SpriteRenderer Piece(string name, Sprite sprite, Vector3 localPosition, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(_candleRoot, false);
            go.transform.localPosition = localPosition;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        // ---- offerings ----

        // Full: the offering on the stone, as its own picture, lifting and fading while it dissolves, with motes of light drifting up from it.
        void ShowOffering(MythosSave save, int minuteOfDay)
        {
            var slot = RitualDirector.OnAltar(save, minuteOfDay);
            if (slot == null) { ClearOfferings(); return; }
            var index = save.Plan.IndexOf(slot);
            var progress = Mathf.Clamp01((RitualDirector.Offset(minuteOfDay) - RitualModel.SacrificeStart(index)) / (float)RitualModel.MinutesPerSacrifice);
            if (_offering == null)
            {
                _offering = new GameObject("offering", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                _offering.transform.SetParent(transform, false);
                _offering.sortingOrder = 7;
                for (var j = 0; j < Motes; j++)
                {
                    var m = new GameObject("mote" + j, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                    m.transform.SetParent(transform, false);
                    m.sprite = RitualFx.Mote();
                    m.sortingOrder = 8;
                    m.transform.localScale = Vector3.one * (j % 3 == 0 ? 2f : 1f);
                    _motes.Add(m);
                }
            }
            if (_session.Db.TryGetItem(slot.ItemId, out var item) && item.Icon != null) _offering.sprite = item.Icon;
            _offering.enabled = true;
            _offering.transform.localPosition = new Vector3(0f, 1.0f + 0.8f * progress + Mathf.Sin(Time.time * 2f) * 0.04f, 0f);
            _offering.color = new Color(1f, 0.97f, 0.85f, 1f - 0.85f * progress);
            for (var j = 0; j < _motes.Count; j++)
            {
                var t = Mathf.Repeat(Time.time * 0.3f + j / (float)_motes.Count, 1f);
                _motes[j].enabled = true;
                _motes[j].transform.localPosition = new Vector3(Mathf.Sin(j * 2.1f + t * 3f) * 0.55f, 0.9f + t * 1.7f, 0f);
                _motes[j].color = new Color(1f, 0.88f, 0.5f, (1f - t) * (0.35f + 0.6f * progress));
            }
        }

        // Mild: a plain pale light for each offering that has been laid, fading as it is taken.
        void UpdatePlainLights(MythosSave save, int offset)
        {
            while (_lights.Count < save.Plan.Count)
            {
                var go = new GameObject("offering", typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(-1.2f + _lights.Count * 0.6f, 1.2f, 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = RuntimeSprites.Square(new Color(0.8f, 0.9f, 1f));
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

        void ClearOfferings()
        {
            foreach (var l in _lights) if (l != null) l.enabled = false;
            if (_offering != null) _offering.enabled = false;
            foreach (var m in _motes) if (m != null) m.enabled = false;
        }

        void Clear()
        {
            ClearOfferings();
            CandlesLit = false;
            if (_candleRoot != null) _candleRoot.gameObject.SetActive(false);
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
