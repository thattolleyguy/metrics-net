using Metrics.Core;
using Metrics.Tests.Core;
using NUnit.Framework;

namespace Metrics.Tests
{
    [TestFixture]
    public class MetricsTests
    {
        MetricRegistry registry;
        [SetUp]
        public void SetUp()
        {
            registry = new MetricRegistry();
        }

        [Test]
        public void Can_get_same_metric_when_metric_exists()
        {
           // var metrics = new Metrics();
            var counter = registry.Counter("CounterTests.Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = registry.Counter( "CounterTests.Can_get_same_metric_when_metric_exists");
            Assert.AreSame(counter, same);
        }

        [Test]
        public void Can_get_all_registered_metrics()
        {
            var counter = registry.Counter( "CounterTests.Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(counter);

            var same = registry.Counter( "CounterTests.Can_get_same_metric_when_metric_exists");
            Assert.IsNotNull(same);

            Assert.AreEqual(1, registry.Metrics.Count);
        }

        [Test]
        public void Can_get_all_registered_metrics_as_readonly()
        {
            var all = registry.Metrics.Count;

            Assert.AreEqual(0, all);
            Assert.Throws(typeof(System.NotSupportedException), () => registry.Metrics.Add(new MetricName("CounterTests.No way this is going to get added"),
                             new Counter()));
           

            Assert.AreEqual(0, all);
        }

        [Test]
        public void Can_get_metrics_from_collection_with_registered_changes()
        {
            // Counter
            var name = new MetricName("MeterTests.counter");
            var counter = registry.Counter( "MeterTests.counter");
            Assert.IsNotNull(registry.Metrics[name], "Metric not found in central registry");
            counter.Increment(10);
            var actual = ((Counter)registry.Metrics[name]).Count;
            Assert.AreEqual(10, actual, "Immutable copy did not contain correct values for this metric");
            
            // Meter
            name = new MetricName("MeterTests.meter");
            var meter = registry.Meter( "MeterTests.meter");
            Assert.IsNotNull(registry.Metrics[name], "Metric not found in central registry");
            meter.Mark(3);
            actual = ((Meter)registry.Metrics[name]).Count;
            Assert.AreEqual(3, actual, "Immutable copy did not contain correct values for this metric");
        }

        [Test]
        public void CanRemoveMetricFromRegistry()
        {
            var name = new MetricName( "MeterTests.Can_safely_remove_metrics_from_outer_collection_without_affecting_registry");
            var meter = registry.Meter("MeterTests.Can_safely_remove_metrics_from_outer_collection_without_affecting_registry");
            meter.Mark(3);

            registry.Remove(name);
            Assert.True(!registry.Metrics.ContainsKey(name), "Metric was removed from central registry");
        }
    }
}
