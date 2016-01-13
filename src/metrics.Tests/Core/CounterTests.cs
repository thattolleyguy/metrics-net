﻿using NUnit.Framework;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class CounterTests : MetricTestBase
    {
        MetricRegistry _metrics = new MetricRegistry();

        [Test]
        public void Can_count()
        {
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);
            
            counter.Increment(100);
            Assert.AreEqual(100, counter.Count);
        }

        [TearDown]
        public void TearDown()
        {
            _metrics.Clear();
        }
    }
}
