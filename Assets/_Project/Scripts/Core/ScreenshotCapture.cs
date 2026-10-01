using System.Collections;
using System.IO;
using UnityEngine;

namespace Farm.Core
{
    // QA helper: when the player is started with `-farmCapture <dir>`, saves a few screenshots and quits.
    // Used for automated visual checks (e.g. pixel-perfect scaling) on Windows and Linux builds.
    public sealed class ScreenshotCapture : MonoBehaviour
    {
        const int WarmupFrames = 30;
        const int FrameSpacing = 17;
        const int ShotCount = 6;

        public static void StartIfRequested(MonoBehaviour host)
        {
            var dir = CommandLine.GetArg("-farmCapture");
            if (string.IsNullOrEmpty(dir)) return;
            host.gameObject.AddComponent<ScreenshotCapture>().StartCoroutine(Run(dir));
        }

        static IEnumerator Run(string dir)
        {
            Directory.CreateDirectory(dir);
            for (int i = 0; i < WarmupFrames; i++) yield return null;

            for (int shot = 0; shot < ShotCount; shot++)
            {
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                var path = Path.Combine(dir, $"shot_{Screen.width}x{Screen.height}_{shot}.png");
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Destroy(tex);
                Log.Info($"[Capture] {path}");
                for (int i = 0; i < FrameSpacing; i++) yield return null;
            }

            Application.Quit(0);
        }
    }
}
