using System;
using System.Collections;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    // The test runs are quiet: the options the game starts with in a test are all at 1%.
    public class QuietTestRunsFlowTests
    {
        [UnityTest]
        public IEnumerator InATestRun_EveryAudioOptionStartsAtOnePercent()
        {
            var root = Path.Combine(Path.GetTempPath(), "farm-quiet-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            GameServices.DataRootOverride = root;
            Bootstrapper.ResetForTests();
            try
            {
                Bootstrapper.InitializeServices();
                yield return null;
                var s = ServiceLocator.Get<SettingsStore>().Current;
                Assert.AreEqual(0.01f, s.MasterVolume, 1e-6f);
                Assert.AreEqual(0.01f, s.MusicVolume, 1e-6f);
                Assert.AreEqual(0.01f, s.SfxVolume, 1e-6f);
                Assert.AreEqual(0.01f, s.AmbienceVolume, 1e-6f);
                Assert.AreEqual(0.01f, AudioListener.volume, 1e-6f);
            }
            finally
            {
                Bootstrapper.ResetForTests();
                GameServices.DataRootOverride = null;
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
