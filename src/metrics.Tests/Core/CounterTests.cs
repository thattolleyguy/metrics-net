using NUnit.Framework;

namespace Metrics.Tests.Core
{
    [TestFixture]
    public class CounterTests : MetricTestBase
    {


        [Test]
        public void Can_count()
        {
            MetricRegistry _metrics = new MetricRegistry();
            var counter = _metrics.Counter("CounterTests.Can_count");
            Assert.IsNotNull(counter);
            
            counter.Increment(100);
            Assert.AreEqual(100, counter.Count);
        }
    }
}
