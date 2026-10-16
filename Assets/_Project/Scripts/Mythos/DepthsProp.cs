using UnityEngine;

namespace Farm.Mythos
{
    // A piece of the deep mine that moves: a vein that crawls, an eye that opens and shuts. Plays a sheet of frames side by side on a loop, each piece out of step with the rest.
    public sealed class DepthsProp : MonoBehaviour
    {
        SpriteRenderer _renderer;
        string _sheet;
        int _width, _height, _frames;
        float _fps, _phase;

        public void Init(SpriteRenderer renderer, string sheet, int frameWidth, int frameHeight, float fps, float phase)
        {
            _renderer = renderer;
            _sheet = sheet;
            _width = frameWidth;
            _height = frameHeight;
            _fps = fps;
            _phase = phase;
            _frames = DepthsArt.Frames(sheet, frameWidth);
            Show();
        }

        void Update() => Show();

        void Show()
        {
            if (_renderer == null || _frames <= 0) return;
            _renderer.sprite = DepthsArt.Frame(_sheet, _width, _height, (int)(Time.time * _fps + _phase));
        }
    }
}
