using System;
using System.Text.RegularExpressions;
using Farm.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Farm.Tests
{
    public class EventBusTests
    {
        struct Ping { public int Value; }

        [Test]
        public void Publish_InvokesSubscribers()
        {
            var bus = new EventBus();
            var received = 0;
            bus.Subscribe<Ping>(p => received += p.Value);
            bus.Publish(new Ping { Value = 3 });
            Assert.AreEqual(3, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var count = 0;
            Action<Ping> h = _ => count++;
            bus.Subscribe(h);
            bus.Unsubscribe(h);
            bus.Publish(new Ping());
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Handler_CanUnsubscribeDuringPublish()
        {
            var bus = new EventBus();
            var count = 0;
            Action<Ping> h = null;
            h = _ => { count++; bus.Unsubscribe(h); };
            bus.Subscribe(h);
            bus.Publish(new Ping());
            bus.Publish(new Ping());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ThrowingHandler_DoesNotBlockOthers()
        {
            var bus = new EventBus();
            var ran = false;
            bus.Subscribe<Ping>(_ => throw new Exception("boom"));
            bus.Subscribe<Ping>(_ => ran = true);
            LogAssert.Expect(LogType.Error, new Regex("threw"));
            bus.Publish(new Ping());
            Assert.IsTrue(ran);
        }
    }
}
