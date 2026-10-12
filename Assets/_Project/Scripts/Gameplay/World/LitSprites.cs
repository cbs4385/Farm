using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A picture made while the game runs gets the engine's plain unlit material in a built player (in the Editor it gets the lit one, which hid this), so it ignored the light
    // and glowed at night: slimes, stairs, animals, villagers, the grass decoration (playtest 2026-10-12: the outside world must follow the light). Pictures that belong to the
    // world are given the lit material here; pictures that must stay readable in the dark (icons, bubbles, health bars, weather, glints) are left as they are.
    public static class LitSprites
    {
        public const string ShaderName = "Universal Render Pipeline/2D/Sprite-Lit-Default";

        static Material _material;

        static LitSprites() => TestResets.Add(() => _material = null);

        // One shared lit material for everything made at run time.
        public static Material Material
        {
            get
            {
                if (_material != null) return _material;
                var shader = Shader.Find(ShaderName);
                if (shader == null) { Log.Warn("The lit sprite shader is missing: pictures made while the game runs will ignore the light."); return null; }
                _material = new Material(shader) { name = "RuntimeSpriteLit" };
                return _material;
            }
        }

        public static void Apply(Renderer renderer)
        {
            if (renderer == null) return;
            var lit = Material;
            if (lit != null) renderer.sharedMaterial = lit;
        }

        public static void Apply(GameObject go)
        {
            if (go == null) return;
            foreach (var renderer in go.GetComponents<Renderer>()) Apply(renderer);
        }
    }
}
