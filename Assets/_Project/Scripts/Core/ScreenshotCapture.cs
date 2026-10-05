using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Farm.Core
{
    // QA helper: when the player is started with `-farmCapture <dir>`, saves a few screenshots and quits.
    // Used for automated visual checks (e.g. pixel-perfect scaling) on Windows and Linux builds.
    public sealed class ScreenshotCapture : MonoBehaviour
    {
        const int WarmupFrames = 90;
        const int FrameSpacing = 17;
        const int ShotCount = 6;

        static bool _started;

        // The capture lives on its own object that survives scene loads, so a run that jumps to another map (a startup command that reloads
        // the scene, such as `floor 28`) is still photographed, and only one capture ever runs.
        public static void StartIfRequested(MonoBehaviour host)
        {
            var dir = CommandLine.GetArg("-farmCapture");
            if (string.IsNullOrEmpty(dir) || _started) return;
            _started = true;
            var go = new GameObject("ScreenshotCapture");
            DontDestroyOnLoad(go);
            go.AddComponent<ScreenshotCapture>().StartCoroutine(Run(dir));
        }

        static IEnumerator Run(string dir)
        {
            Directory.CreateDirectory(dir);
            for (int i = 0; i < WarmupFrames; i++) yield return null;

            // Rough performance sample: frame times and managed allocation over the capture run.
            var frames = 0;
            var totalDt = 0f;
            var maxDt = 0f;
            var memStart = GC.GetTotalMemory(false);
            var gcStart = GC.CollectionCount(0);

            for (int shot = 0; shot < ShotCount; shot++)
            {
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                var path = Path.Combine(dir, $"shot_{Screen.width}x{Screen.height}_{shot}.png");
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Destroy(tex);
                Log.Info($"[Capture] {path}");
                for (int i = 0; i < FrameSpacing; i++)
                {
                    yield return null;
                    frames++;
                    totalDt += Time.unscaledDeltaTime;
                    maxDt = Mathf.Max(maxDt, Time.unscaledDeltaTime);
                }
            }

            if (frames > 0)
                Log.Info($"[Perf] frames={frames} avg={totalDt / frames * 1000f:F2}ms ({frames / totalDt:F0} fps) max={maxDt * 1000f:F2}ms " +
                         $"gc0={GC.CollectionCount(0) - gcStart} managedDelta={(GC.GetTotalMemory(false) - memStart) / 1024}KB " +
                         $"vsync={QualitySettings.vSyncCount}");

            Application.Quit(0);
        }
    }
}
