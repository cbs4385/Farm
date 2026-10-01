using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Farm.Core
{
    // Loads scenes with a black fade. Lives on the persistent Services object.
    public sealed class SceneLoader : MonoBehaviour
    {
        const float DefaultFadeSeconds = 0.25f;

        CanvasGroup _fade;

        public bool IsLoading { get; private set; }

        void Awake() => EnsureFade();

        public void Load(string sceneName, float fadeSeconds = DefaultFadeSeconds, Action onLoaded = null)
        {
            if (IsLoading)
            {
                Log.Warn($"SceneLoader: ignoring load of '{sceneName}' while another load is in progress.");
                return;
            }
            StartCoroutine(LoadRoutine(sceneName, fadeSeconds, onLoaded));
        }

        IEnumerator LoadRoutine(string sceneName, float fadeSeconds, Action onLoaded)
        {
            IsLoading = true;
            yield return Fade(1f, fadeSeconds);

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Log.Error($"SceneLoader: scene '{sceneName}' could not be loaded (is it in Build Settings?).");
            }
            else
            {
                while (!op.isDone) yield return null;
            }

            onLoaded?.Invoke();
            yield return Fade(0f, fadeSeconds);
            IsLoading = false;
        }

        // Public so flows like sleeping can fade the screen without loading a scene.
        public IEnumerator FadeTo(float target, float seconds) => Fade(target, seconds);

        IEnumerator Fade(float target, float seconds)
        {
            EnsureFade();
            if (seconds <= 0f)
            {
                _fade.alpha = target;
                _fade.blocksRaycasts = target > 0f;
                yield break;
            }

            _fade.blocksRaycasts = true;
            var start = _fade.alpha;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                _fade.alpha = Mathf.Lerp(start, target, t / seconds);
                yield return null;
            }
            _fade.alpha = target;
            _fade.blocksRaycasts = target > 0f;
        }

        void EnsureFade()
        {
            if (_fade != null) return;

            var canvasGo = new GameObject("FadeCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            _fade = canvasGo.AddComponent<CanvasGroup>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var imgGo = new GameObject("Black");
            imgGo.transform.SetParent(canvasGo.transform, false);
            var img = imgGo.AddComponent<RawImage>();
            img.color = Color.black;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _fade.alpha = 0f;
            _fade.blocksRaycasts = false;
        }
    }
}
