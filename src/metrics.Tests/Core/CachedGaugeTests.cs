using Metrics;
using Metrics.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Tests.Core
{
    class CachedGaugeTests
    {
        [Test]
        public void Can_cache()
        {
            CachedGagugeTestClock clock = new CachedGagugeTestClock();
            Gauge<long> gauge = new CachedGauge<long>(
                clock,
                1,
                TimeUnit.Nanoseconds,
                () =>
                DateTime.Now.Ticks
                );
            long initialValue = gauge.Value;
            Assert.AreEqual(initialValue, gauge.Value);
            Thread.Sleep(1000);
            Assert.AreEqual(initialValue, gauge.Value);
            clock.tick();
            Assert.AreNotEqual(initialValue, gauge.Value);
        }

        internal class CachedGagugeTestClock : Clock
        {
            int tickValue = 0;
            public override long getTick()
            {
                return tickValue;
            }

            public void tick()
            {
                tickValue++;
            }
        }


    }
}
