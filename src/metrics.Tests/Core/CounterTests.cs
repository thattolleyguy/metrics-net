using Metrics.Core;
using NUnit.Framework;

namespace Metrics.Tests.Core
{
    [TestFixture]
    public class CounterTests : MetricTestBase
    {


        [Test]
        public void Can_inc_multiple()
        {
            MetricRegistry _metrics = new MetricRegistry();
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);

            counter.Increment(100);
            Assert.AreEqual(100, counter.Count);
        }

        [Test]
        public void Can_inc()
        {
            MetricRegistry _metrics = new MetricRegistry();
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);

            counter.Increment();
            Assert.AreEqual(1, counter.Count);
        }

        [Test]
        public void Can_dec()
        {
            MetricRegistry _metrics = new MetricRegistry();
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);

            counter.Decrement();
            Assert.AreEqual(-1, counter.Count);
        }

        [Test]
        public void Can_dec_multiple()
        {
            MetricRegistry _metrics = new MetricRegistry();
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);

            counter.Decrement(8);
            Assert.AreEqual(-8, counter.Count);
        }
    }
}
