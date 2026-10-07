using System.IO;
using Farm.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The macOS build and its Steam depot (5408393): the architecture choice, the depot and upload files, and the release guard inside an .app bundle.
    public class MacBuildTests
    {
        [Test]
        public void TheArchitecture_FollowsTheRequest_AndDefaultsToIntelOffAMac()
        {
            Assert.AreEqual("x64", BuildScript.MacArchitecture("x64"));
            Assert.AreEqual("x64", BuildScript.MacArchitecture("Intel"));
            Assert.AreEqual("ARM64", BuildScript.MacArchitecture("arm64"));
            Assert.AreEqual("x64ARM64", BuildScript.MacArchitecture("universal"));
            var expected = Application.platform == RuntimePlatform.OSXEditor ? "x64ARM64" : "x64";
            Assert.AreEqual(expected, BuildScript.MacArchitecture(null));
        }

        [Test]
        public void TheSteamFiles_IncludeTheMacDepot()
        {
            StringAssert.Contains("\"5408393\" \"depot_macos.vdf\"", File.ReadAllText("Steam/app_build.vdf"));
            var depot = File.ReadAllText("Steam/depot_macos.vdf");
            StringAssert.Contains("\"DepotID\" \"5408393\"", depot);
            StringAssert.Contains("\"ContentRoot\" \"Mac/VERSION/\"", depot);
            var script = File.ReadAllText("Steam/upload.sh");
            StringAssert.Contains("Builds/$os/$version", script);
            StringAssert.Contains("SKIP_MAC", script);
            StringAssert.Contains("depot_macos.vdf", script);
        }

        [Test]
        public void TheReleaseGuard_ScansInsideAnAppBundle()
        {
            var root = Path.Combine(Path.GetTempPath(), "mac-guard-" + System.Guid.NewGuid().ToString("N"));
            try
            {
                var dir = Path.Combine(root, "Farm.app", "Contents", "Resources", "Data", "Managed");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "x.dll"), "xx DebugConsoleScreen xx");
                var found = ReleaseGuard.Scan(root);
                Assert.AreEqual(1, found.Count);
                StringAssert.Contains("DebugConsoleScreen", found[0]);
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
