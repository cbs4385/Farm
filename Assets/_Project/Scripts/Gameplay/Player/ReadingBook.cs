using UnityEngine;

namespace Farm.Gameplay
{
    // The book in the farmer's hands while reading: held up in front of the chest, it rises and settles with the farmer's breath and turns a page now and then (the picture
    // narrows to the spine and opens again). A child of the player, made by PlayerController.StartReading and destroyed when the reading ends.
    public sealed class ReadingBook : MonoBehaviour
    {
        public const float PageSeconds = 1.6f;
        public const float BreathSeconds = 1.8f;

        SpriteRenderer _renderer;
        float _t;

        public static ReadingBook Create(Transform player, Sprite icon, int sortingOrder)
        {
            var go = new GameObject("ReadingBook");
            go.transform.SetParent(player, false);
            go.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var book = go.AddComponent<ReadingBook>();
            book._renderer = go.AddComponent<SpriteRenderer>();
            book._renderer.sprite = icon;
            book._renderer.sortingOrder = sortingOrder;
            go.transform.localScale = Vector3.one * 0.9f;
            return book;
        }

        // The page turn: 1 = open, down to 0.15 at the middle of the turn (pure, for the tests).
        public static float PageWidth(float seconds)
        {
            var phase = Mathf.Repeat(seconds, PageSeconds) / PageSeconds;
            return phase < 0.85f ? 1f : Mathf.Lerp(0.15f, 1f, Mathf.Abs(phase - 0.925f) / 0.075f);
        }

        void Update()
        {
            _t += Time.deltaTime;
            var rise = Mathf.Sin(_t / BreathSeconds * Mathf.PI * 2f) * 0.03f;
            transform.localPosition = new Vector3(0f, 0.5f + rise, 0f);
            transform.localScale = new Vector3(0.9f * PageWidth(_t), 0.9f, 1f);
        }
    }
}
