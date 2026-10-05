using UnityEngine;

namespace Farm.Gameplay
{
    // A door swinging open as the farmer walks through it: for a moment an open doorway (dark inside, a slice of door at its edge) is drawn
    // over the door, and fades. Made on the spot at the door's position and destroyed when it has faded; the scene fade covers the rest.
    public sealed class DoorFlash : MonoBehaviour
    {
        public const float Life = 0.45f;

        static Sprite _open;
        SpriteRenderer _renderer;
        float _age;

        public static void Show(Vector3 position)
        {
            var go = new GameObject("DoorFlash");
            go.transform.position = position;
            go.AddComponent<DoorFlash>().Begin();
        }

        // How opaque the open doorway is `age` seconds in: fully there for the first part, then fading out. (pure)
        public static float AlphaAt(float age)
        {
            if (age <= 0f) return 1f;
            if (age >= Life) return 0f;
            var fade = Life * 0.5f;
            return age < fade ? 1f : 1f - (age - fade) / (Life - fade);
        }

        public static Sprite OpenDoorway()
        {
            if (_open != null) return _open;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "door_open" };
            var clear = new Color32(0, 0, 0, 0);
            var frame = new Color32(78, 52, 34, 255);
            var dark = new Color32(26, 20, 24, 255);
            var glow = new Color32(70, 56, 46, 255);
            var wood = new Color32(150, 104, 62, 255);
            var woodLight = new Color32(186, 138, 84, 255);
            for (var y = 0; y < 16; y++)
                for (var x = 0; x < 16; x++)
                {
                    Color32 c = clear;
                    if (x >= 2 && x <= 13 && y <= 14)
                    {
                        c = dark;
                        if (x == 2 || x == 13 || y == 14) c = frame;                          // the doorframe
                        else if (y < 3) c = glow;                                             // light on the floor inside
                        else if (x >= 3 && x <= 5) c = x == 3 ? woodLight : wood;             // the door swung back against the wall
                    }
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            _open = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            _open.name = "door_open";
            return _open;
        }

        void Begin()
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = OpenDoorway();
            _renderer.sortingOrder = 11;
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            if (_age >= Life) { Destroy(gameObject); return; }
            _renderer.color = new Color(1f, 1f, 1f, AlphaAt(_age));
        }
    }
}
