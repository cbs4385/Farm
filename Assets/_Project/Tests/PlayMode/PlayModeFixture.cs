using System;
using System.IO;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Farm.Tests
{
    // The set-up every flow test needs: a throwaway data folder (so saves and settings never touch the real ones) and a clean set of services, both undone afterwards.
    // Tests that need more reset set-up in their own [SetUp] / [TearDown] (the base ones run first and last respectively).
    public abstract class PlayModeFixture
    {
        protected string DataRoot { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            DataRoot = Path.Combine(Path.GetTempPath(), "farm-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DataRoot);
            GameServices.DataRootOverride = DataRoot;
            Bootstrapper.ResetForTests();
            Uncapped();
        }

        // Tests drive the game frame by frame, so a frame rate cap (vertical sync, the editor's default) only makes them slower.
        public static void Uncapped()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        [TearDown]
        public void BaseTearDown()
        {
            Bootstrapper.ResetForTests();
            GameServices.DataRootOverride = null;
            if (DataRoot != null && Directory.Exists(DataRoot)) Directory.Delete(DataRoot, true);
        }
    }
}
