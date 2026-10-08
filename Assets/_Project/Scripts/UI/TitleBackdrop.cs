using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    // The picture behind the main menu, drawn as a grid of points that the breeze (TitleSway) moves a little each frame. A mesh instead of a shader, so it
    // needs nothing but the standard UI material and works the same on Windows, Linux and macOS.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TitleBackdrop : MaskableGraphic
    {
        public const int Columns = 64, Rows = 36;

        Texture _texture;
        float[] _weights;
        Vector3[] _points;                              // where each point of the grid was put in the last frame (for tests)
        public bool Moving { get; set; } = true;

        public override Texture mainTexture => _texture != null ? _texture : base.mainTexture;

        // Sets the picture. A picture that cannot be read still shows, it just does not sway.
        public void Show(Texture2D picture)
        {
            _texture = picture;
            _weights = null;
            if (picture != null && picture.isReadable)
            {
                picture.filterMode = FilterMode.Bilinear;            // the picture is moved by less than a pixel: crisp point sampling would shimmer
                _weights = TitleSway.Weights(picture.GetPixels32(), picture.width, picture.height, Columns, Rows);
            }
            SetAllDirty();
        }

        public bool Sways => _weights != null;
        public int PointCount => (Columns + 1) * (Rows + 1);
        public Vector3 PointAt(int index) => _points != null ? _points[index] : Vector3.zero;

        // The share of points that sway (for tests and for a glance at whether the picture was read).
        public float SwayingShare()
        {
            if (_weights == null) return 0f;
            var n = 0;
            foreach (var w in _weights) if (w > 0.2f) n++;
            return n / (float)_weights.Length;
        }

        void Update()
        {
            if (_texture != null && _weights != null && Moving) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_texture == null) return;
            var r = GetPixelAdjustedRect();
            var t = Time.unscaledTime;
            var stride = Columns + 1;
            _points ??= new Vector3[PointCount];
            for (var j = 0; j <= Rows; j++)
                for (var i = 0; i <= Columns; i++)
                {
                    var u = i / (float)Columns; var v = j / (float)Rows;
                    var move = _weights != null && Moving ? TitleSway.Offset(u, v, t, _weights[j * stride + i]) : Vector2.zero;
                    var position = new Vector3(r.xMin + (u + move.x) * r.width, r.yMin + (v + move.y) * r.height);
                    _points[j * stride + i] = position;
                    vh.AddVert(position, color, new Vector2(u, v));
                }
            for (var j = 0; j < Rows; j++)
                for (var i = 0; i < Columns; i++)
                {
                    var a = j * stride + i;
                    vh.AddTriangle(a, a + stride, a + stride + 1);
                    vh.AddTriangle(a, a + stride + 1, a + 1);
                }
        }
    }
}
