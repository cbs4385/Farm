using System.Collections;
using Farm.Core;
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
            Assert.IsTrue(ServiceLocator.TryGet<EventBus>(out _));
            Assert.IsTrue(ServiceLocator.TryGet<SceneLoader>(out _));
        }
    }
}
