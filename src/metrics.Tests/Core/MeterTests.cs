using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Metrics.Core;
using Metrics.Reporting;

namespace Metrics.Tests.Core
{
    [TestFixture]
    public class MeterTests
    {
        MetricRegistry _metrics = new MetricRegistry();

        [Test]
        public void Can_count()
        {
            var meter = _metrics.Meter("MeterTests.Can_count");
            meter.Mark(3);
            Assert.AreEqual(3, meter.Count);
        }

        [Test]
        public void Can_meter()
        {
            const int count = 100000;
            var block = new ManualResetEvent(false);
            var meter = _metrics.Meter("MeterTests.Can_meter");
            Assert.IsNotNull(meter);

            var i = 0;
            // ThreadPool.QueueUserWorkItem(s => 
            //{
            do
            {
                meter.Mark();
                i++;

            } while (i < count);
            Thread.Sleep(5000);
            //  block.Set();
            //});
            //block.WaitOne();

            Assert.AreEqual(count, meter.Count);

            var oneMinuteRate = meter.OneMinuteRate;
            var fiveMinuteRate = meter.FiveMinuteRate;
            var fifteenMinuteRate = meter.FifteenMinuteRate;
            var meanRate = meter.MeanRate;

            Assert.IsTrue(oneMinuteRate > 0);
            Trace.WriteLine("One minute rate:" + meter.OneMinuteRate);

            Assert.IsTrue(fiveMinuteRate > 0);
            Trace.WriteLine("Five minute rate:" + meter.FiveMinuteRate);

            Assert.IsTrue(fifteenMinuteRate > 0);
            Trace.WriteLine("Fifteen minute rate:" + meter.FifteenMinuteRate);

            Assert.IsTrue(meanRate > 0);
            Trace.WriteLine("Mean rate:" + meter.MeanRate);
        }
    }
}