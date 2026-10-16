using System;
using Farm.Core;
using Farm.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.UI
{
    // The fishing mini-game: wait for the bite, press the action button (Enter, Space, A or a click) when it comes, then
    // press again while the marker is inside the green zone. Escape pulls the line in.
    public sealed class FishingScreen : UiScreen
    {
        public const string HintsHiddenFlag = "fishing.hints.hidden";          // the instructions are put away (saved with the game)

        readonly TextMeshProUGUI _status, _hint, _toggle, _warning;
        readonly RectTransform _barRoot, _zone, _sweet, _marker;
        FishingSession _session;
        Action<FishingSession> _onDone;

        public bool HintShown => _hint.gameObject.activeSelf;
        public bool WarningShown => _warning.gameObject.activeSelf;
        public string StatusText => _status.text;

        public FishingScreen(UiService ui) : base(ui)
        {
            var layer = UiKit.Panel(ui.ScreenCanvas.transform, "Fishing", new Color(0f, 0f, 0f, 0.15f));
            UiKit.Stretch(layer.rectTransform);
            Root = layer.gameObject;
            var click = Root.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => Press());
            click.triggers.Add(entry);

            // Above the item bar (it hid what the bar held, playtest 2026-10-14). What the player needs while fishing (the state, the bar, a full backpack) is
            // always shown; the sentences that explain it can be put away.
            var frame = UiKit.Panel(Root.transform, "Frame", UiKit.PanelColor);
            UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(560f, 128f), new Vector2(0f, 76f));
            _status = UiKit.Label(frame.transform, "", 24f, TextAlignmentOptions.Center, UiKit.Accent);
            UiKit.Place(_status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 32f), new Vector2(0f, -6f));
            _hint = UiKit.Label(frame.transform, "", 17f, TextAlignmentOptions.Center);
            UiKit.Place(_hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(540f, 24f), new Vector2(0f, -40f));
            _hint.textWrappingMode = TextWrappingModes.NoWrap;
            _hint.enableAutoSizing = true;
            _hint.fontSizeMin = 12f;
            _hint.fontSizeMax = 17f;

            var bar = UiKit.Panel(frame.transform, "Bar", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Place(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(500f, 26f), new Vector2(0f, 30f));
            _barRoot = bar.rectTransform;
            var zone = UiKit.Panel(bar.transform, "Zone", new Color(0.3f, 0.8f, 0.3f, 1f));
            _zone = zone.rectTransform;
            var sweet = UiKit.Panel(zone.transform, "PerfectSpot", new Color(0.75f, 1f, 0.55f, 1f));          // the middle of the zone: a stop here is a perfect catch
            sweet.raycastTarget = false;
            _sweet = sweet.rectTransform;
            _sweet.anchorMin = new Vector2(0.5f - FishingModel.PerfectShare, 0f);
            _sweet.anchorMax = new Vector2(0.5f + FishingModel.PerfectShare, 1f);
            _sweet.offsetMin = _sweet.offsetMax = Vector2.zero;
            var marker = UiKit.Panel(bar.transform, "Marker", UiKit.Accent);
            _marker = marker.rectTransform;

            _toggle = UiKit.Label(frame.transform, "", 13f, TextAlignmentOptions.Center, UiKit.DimText);
            UiKit.Place(_toggle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(540f, 20f), new Vector2(0f, 5f));

            _warning = UiKit.Label(Root.transform, L.Get("fishing.pack_full"), 18f, TextAlignmentOptions.Center, new Color(0.95f, 0.45f, 0.35f));
            UiKit.Place(_warning.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(640f, 28f), new Vector2(0f, 210f));
            _warning.fontStyle = FontStyles.Bold;
            _warning.raycastTarget = false;
            Root.SetActive(false);
        }

        public void OpenFishing(FishingSession session, Action<FishingSession> onDone)
        {
            _session = session;
            _onDone = onDone;
            AudioService.PlayIfAvailable(Sfx.Cast);
            Open();
            Draw();
        }

        public FishingSession Session => _session;

        void Press()
        {
            if (_session == null || Time.frameCount == OpenedFrame) return;
            _session.Press();
        }

        public override void Tick()
        {
            if (_session == null) return;
            if (Time.frameCount != OpenedFrame && Ui.Input.Ui[InputNames.TabPrev].WasPressedThisFrame()) ToggleHint();
            if (Time.frameCount != OpenedFrame && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame()) _session.Press();
            var before = _session.State;
            _session.Tick(Time.unscaledDeltaTime);
            if (before != _session.State)
            {
                if (_session.State == FishingState.Bite) AudioService.PlayIfAvailable(Sfx.Bite);
                else if (_session.State == FishingState.Done && _session.Caught) AudioService.PlayIfAvailable(Sfx.Splash);
            }
            Draw();
            if (_session.State == FishingState.Done) Finish();
        }

        // Puts the explanations away or brings them back; the state, the bar and the warning stay.
        public void ToggleHint()
        {
            var session = Ui.Session;
            if (session == null) return;
            session.SetFlag(HintsHiddenFlag, !session.HasFlag(HintsHiddenFlag));
            Draw();
        }

        void Draw()
        {
            switch (_session.State)
            {
                case FishingState.Waiting: _status.text = L.Get("fishing.state.waiting"); _hint.text = L.Get(_session.Baited ? "fishing.waiting_bait" : "fishing.waiting_nobait"); break;
                case FishingState.Bite: _status.text = L.Get("fishing.state.bite"); _hint.text = L.Get("fishing.bite"); break;
                case FishingState.Bar: _status.text = L.Get("fishing.state.reel"); _hint.text = L.Get("fishing.reel"); break;
                default: _status.text = string.Empty; _hint.text = string.Empty; break;
            }
            var hidden = Ui.Session != null && Ui.Session.HasFlag(HintsHiddenFlag);
            _hint.gameObject.SetActive(!hidden);
            _toggle.text = L.Get(hidden ? "fishing.show_hint" : "fishing.hide_hint");
            _warning.gameObject.SetActive(Ui.Session != null && Ui.Session.InGame && !Ui.Session.Backpack.HasEmptySlot());
            var show = _session.State == FishingState.Bar;
            _barRoot.gameObject.SetActive(show);
            if (!show) return;
            _zone.anchorMin = new Vector2(_session.ZoneCenter - _session.ZoneWidth / 2f, 0f);
            _zone.anchorMax = new Vector2(_session.ZoneCenter + _session.ZoneWidth / 2f, 1f);
            _zone.offsetMin = _zone.offsetMax = Vector2.zero;
            _marker.anchorMin = new Vector2(Mathf.Clamp01(_session.Marker) - 0.006f, 0f);
            _marker.anchorMax = new Vector2(Mathf.Clamp01(_session.Marker) + 0.006f, 1f);
            _marker.offsetMin = _marker.offsetMax = Vector2.zero;
        }

        public override void OnCancel()
        {
            _session?.Cancel();
            Finish();
        }

        void Finish()
        {
            var s = _session;
            var cb = _onDone;
            _session = null;
            _onDone = null;
            Close();
            cb?.Invoke(s);
        }
    }
}
