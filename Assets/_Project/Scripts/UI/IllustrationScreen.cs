using System;
using Farm.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // A picture shown full screen with a caption (the endings): the picture is a plain texture drawn with a point filter so the pixels stay crisp.
    public sealed class IllustrationScreen : UiScreen
    {
        public const float PictureWidth = 640f, PictureHeight = 360f;      // twice the 320 x 180 art

        readonly RawImage _picture;
        readonly TextMeshProUGUI _caption;
        Action _onClose;

        public string ShownPath { get; private set; }
        public bool ShowsPicture => _picture != null && _picture.texture != null;

        public IllustrationScreen(UiService ui) : base(ui)
        {
            var frame = UiKit.ModalFrame(ui.ScreenCanvas.transform, "Illustration", new Vector2(700f, 520f), out var root);
            Root = root;
            var stack = UiKit.VStack(frame, "Stack", 10f, 14, TextAnchor.MiddleCenter);
            UiKit.Stretch((RectTransform)stack.transform);

            var rt = UiKit.Rect("Picture", stack.transform);
            _picture = rt.gameObject.AddComponent<RawImage>();
            _picture.raycastTarget = false;
            UiKit.Size(rt.gameObject, PictureWidth, PictureHeight);

            _caption = UiKit.Label(stack.transform, "", 19f, TextAlignmentOptions.Center);
            UiKit.Size(_caption.gameObject, -1f, 56f);
            UiKit.MakeButton(stack.transform, L.Get("ui.continue"), Close, 200f, 36f).name = "Continue";
            root.SetActive(false);
        }

        public void OpenIllustration(string resourcePath, string captionKey, Action onClose)
        {
            ShownPath = resourcePath;
            var texture = string.IsNullOrEmpty(resourcePath) ? null : Resources.Load<Texture2D>(resourcePath);
            if (texture != null) texture.filterMode = FilterMode.Point;
            _picture.texture = texture;
            _picture.gameObject.SetActive(texture != null);
            _caption.text = L.Get(captionKey);
            _onClose = onClose;
            Open();
        }

        public override void Close()
        {
            var callback = _onClose;
            _onClose = null;
            base.Close();
            callback?.Invoke();
        }
    }
}
