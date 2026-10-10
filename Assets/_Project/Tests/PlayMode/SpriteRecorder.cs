using System.Collections.Generic;
using UnityEngine;

namespace Farm.Tests
{
    // Writes down every picture a renderer shows at the end of each frame (after every late update has chosen it), for tests that look at animation frames.
    [DefaultExecutionOrder(10000)]
    public sealed class SpriteRecorder : MonoBehaviour
    {
        public readonly HashSet<string> Names = new HashSet<string>();
        SpriteRenderer _renderer;

        void Awake() => _renderer = GetComponent<SpriteRenderer>();

        void LateUpdate()
        {
            if (_renderer != null && _renderer.sprite != null) Names.Add(_renderer.sprite.name);
        }
    }
}
