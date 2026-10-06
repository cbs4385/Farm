using UnityEngine;

namespace Farm.Gameplay
{
    // The solid body of a villager, the cat or an animal: the player cannot walk through whoever stands on a tile. It is a child with its own
    // collider so that the actor itself (which may use a trigger for clicking) is unchanged, and a kinematic body so that it can move cheaply.
    public static class ActorBody
    {
        public const string BodyName = "Body";
        public const float Size = 0.7f;

        public static void Add(GameObject host)
        {
            if (host.transform.Find(BodyName) != null) return;
            var body = new GameObject(BodyName);
            body.transform.SetParent(host.transform, false);
            var box = body.AddComponent<BoxCollider2D>();
            box.size = new Vector2(Size, Size);
            if (!host.TryGetComponent<Rigidbody2D>(out var rb)) rb = host.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }
}
