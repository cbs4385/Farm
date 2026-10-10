using System.Collections.Generic;
using Farm.Core;
using UnityEngine;

namespace Farm.Gameplay
{
    // A soft shadow on the ground under anything that walks (the farmer, villagers, animals, the cat): the cheapest way to make a flat picture stand in the world.
    public static class ActorShadow
    {
        const string ChildName = "Shadow";
        static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();

        static ActorShadow() => TestResets.Add(() => Cache.Clear());

        // The width of the shadow of a picture that is `spriteWidth` pixels wide: a little narrower than the picture, never wider than a cell.
        public static int WidthFor(float spriteWidth) => Mathf.Clamp(Mathf.RoundToInt(spriteWidth * 0.75f), 6, 14);

        static Sprite SpriteOf(int width)
        {
            if (Cache.TryGetValue(width, out var cached) && cached != null) return cached;
            var grid = CharacterLook.Shadow(width);
            var texture = new Texture2D(grid.W, grid.H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "shadow_" + width };
            texture.SetPixels32(grid.ToTexture());
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, grid.W, grid.H), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = texture.name;
            Cache[width] = sprite;
            return sprite;
        }

        // Puts a shadow under `owner` (once). Returns its renderer, or null when the owner has no picture to follow.
        public static SpriteRenderer Add(GameObject owner, SpriteRenderer follow)
        {
            if (owner == null || follow == null || follow.sprite == null) return null;
            var existing = owner.transform.Find(ChildName);
            if (existing != null) return existing.GetComponent<SpriteRenderer>();
            var child = new GameObject(ChildName);
            child.transform.SetParent(owner.transform, false);
            child.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteOf(WidthFor(follow.sprite.rect.width));
            renderer.sortingLayerID = follow.sortingLayerID;
            renderer.sortingOrder = follow.sortingOrder - 1;
            return renderer;
        }
    }
}
