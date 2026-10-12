using System.Collections;
using Farm.Core;
using Farm.Gameplay;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    public class BootstrapSmokeTests
    {
        [UnityTest]
        public IEnumerator InitializeServices_RegistersCoreServices()
        {
            var loader = Bootstrapper.InitializeServices();
            yield return null;
            Assert.IsNotNull(loader);
            Assert.IsNotNull(GameContext.Current.Bus);
            Assert.IsNotNull(GameContext.Current.Scenes);
        }
    }
}
