using System;
using Farm.Core;
using NUnit.Framework;

namespace Farm.Tests
{
    public class ServiceLocatorTests
    {
        class Dummy { }

        [SetUp] public void SetUp() => ServiceLocator.Clear();
        [TearDown] public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void Get_ReturnsRegisteredService()
        {
            var d = new Dummy();
            ServiceLocator.Register(d);
            Assert.AreSame(d, ServiceLocator.Get<Dummy>());
        }

        [Test]
        public void Get_Throws_WhenMissing()
        {
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<Dummy>());
        }

        [Test]
        public void TryGet_ReturnsFalse_WhenMissing()
        {
            Assert.IsFalse(ServiceLocator.TryGet<Dummy>(out _));
        }
    }
}
