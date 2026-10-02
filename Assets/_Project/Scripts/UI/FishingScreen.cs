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
        readonly TextMeshProUGUI _text;
        readonly RectTransform _barRoot, _zone, _marker;
        FishingSession _session;
        Action<FishingSession> _onDone;

        public FishingScreen(UiService ui) : base(ui)
        {
            var layer = UiKit.Panel(ui.ScreenCanvas.transform, "Fishing", new Color(0f, 0f, 0f, 0.15f));
            UiKit.Stretch(layer.rectTransform);
            Root = layer.gameObject;
            var click = Root.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => Press());
            click.triggers.Add(entry);

            var frame = UiKit.Panel(Root.transform, "Frame", UiKit.PanelColor);
            UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(560f, 110f), new Vector2(0f, 20f));
            _text = UiKit.Label(frame.transform, "", 22f, TextAlignmentOptions.Center);
            UiKit.Place(_text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 40f), new Vector2(0f, -8f));

            var bar = UiKit.Panel(frame.transform, "Bar", new Color(0.08f, 0.06f, 0.04f, 1f));
            UiKit.Place(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(500f, 26f), new Vector2(0f, 16f));
            _barRoot = bar.rectTransform;
            var zone = UiKit.Panel(bar.transform, "Zone", new Color(0.3f, 0.8f, 0.3f, 1f));
            _zone = zone.rectTransform;
            var marker = UiKit.Panel(bar.transform, "Marker", UiKit.Accent);
            _marker = marker.rectTransform;
            Root.SetActive(false);
        }

        public void OpenFishing(FishingSession session, Action<FishingSession> onDone)
        {
            _session = session;
            _onDone = onDone;
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
            if (Time.frameCount != OpenedFrame && Ui.Input.Ui[InputNames.Submit].WasPressedThisFrame()) _session.Press();
            _session.Tick(Time.unscaledDeltaTime);
            Draw();
            if (_session.State == FishingState.Done) Finish();
        }

        void Draw()
        {
            switch (_session.State)
            {
                case FishingState.Waiting: _text.text = L.Get("fishing.waiting"); break;
                case FishingState.Bite: _text.text = L.Get("fishing.bite"); break;
                case FishingState.Bar: _text.text = L.Get("fishing.reel"); break;
                default: _text.text = string.Empty; break;
            }
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
