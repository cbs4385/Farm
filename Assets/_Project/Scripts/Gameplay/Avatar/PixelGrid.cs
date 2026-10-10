using UnityEngine;

namespace Farm.Gameplay
{
    // A small picture as plain pixels, row 0 at the TOP (a texture's rows start at the bottom, so reading and writing flip them). The character rig works on these so
    // that it can be tested without any texture or scene.
    public sealed class PixelGrid
    {
        public readonly int W, H;
        public readonly Color32[] P;

        public PixelGrid(int w, int h)
        {
            W = w; H = h;
            P = new Color32[w * h];
        }

        public bool In(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;
        public Color32 Get(int x, int y) => In(x, y) ? P[y * W + x] : default;
        public void Set(int x, int y, Color32 c) { if (In(x, y)) P[y * W + x] = c; }
        public bool Opaque(int x, int y) => In(x, y) && P[y * W + x].a > 0;

        public PixelGrid Clone()
        {
            var copy = new PixelGrid(W, H);
            System.Array.Copy(P, copy.P, P.Length);
            return copy;
        }

        // From texture order (row 0 at the bottom) to a grid (row 0 at the top).
        public static PixelGrid FromTexture(Color32[] pixels, int w, int h)
        {
            var grid = new PixelGrid(w, h);
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++) grid.P[(h - 1 - y) * w + x] = pixels[y * w + x];
            return grid;
        }

        public Color32[] ToTexture()
        {
            var result = new Color32[W * H];
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++) result[(H - 1 - y) * W + x] = P[y * W + x];
            return result;
        }

        // The first and last row that hold anything, or (-1, -1) when the picture is empty.
        public (int top, int bottom) Rows()
        {
            int top = -1, bottom = -1;
            for (var y = 0; y < H; y++)
                for (var x = 0; x < W; x++)
                    if (P[y * W + x].a > 0) { if (top < 0) top = y; bottom = y; break; }
            return (top, bottom);
        }

        // The first and last column with anything in the rows y0..y1, or (-1, -1).
        public (int left, int right) Columns(int y0, int y1)
        {
            int left = -1, right = -1;
            for (var x = 0; x < W; x++)
                for (var y = y0; y <= y1; y++)
                    if (Opaque(x, y)) { if (left < 0) left = x; right = x; break; }
            return (left, right);
        }

        public int Count()
        {
            var n = 0;
            foreach (var c in P) if (c.a > 0) n++;
            return n;
        }
    }
}
