using Metrics;
using Metrics.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Tests.Core
{


    [TestFixture]
    public class TimerTests
    {

        private Reservoir reservoir;
        private Clock clock;
        private Timer timer;

        [SetUp]
        public void SetupTests()
        {
            reservoir = new ExponentiallyDecayingReservoir();
            clock = new TimerTestClock();
            this.timer = new Timer(reservoir, clock);

        }

        [Test]
        public void hasRates()
        {
            Assert.AreEqual(0, timer.Count);

            Assert.AreEqual(0.0, timer.MeanRate);

            Assert.AreEqual(0.0, timer.OneMinuteRate);

            Assert.AreEqual(0.0, timer.FiveMinuteRate);

            Assert.AreEqual(0.0, timer.FifteenMinuteRate);

        }

        [Test]
        public void updatesTheCountOnUpdates()
        {
            Assert.AreEqual(0, timer.Count);

            timer.Update(1, TimeUnit.Seconds);

            Assert.AreEqual(1, timer.Count);
        }

        [Test]
        public void timesCallableInstances()
        {
            string value = timer.Time(() => "one");

            Assert.AreEqual(1, timer.Count);

            Assert.AreEqual("one", value);

            Assert.AreEqual(1, reservoir.Snapshot.Values.Length);
            Assert.AreEqual(50000000, reservoir.Snapshot.Values[0]);
        }

        [Test]
        public void timesRunnableInstances()
        {

            bool[] called = { false };
            timer.Time(() => called[0] = true);

            Assert.AreEqual(1, timer.Count);

            Assert.True(called[0]);
            Assert.AreEqual(1, reservoir.Snapshot.Values.Length);
            Assert.AreEqual(50000000, reservoir.Snapshot.Values[0]);
        }

        [Test]
        public void timesContexts()
        {

            timer.Time().stop();

            Assert.AreEqual(1, timer.Count);

            Assert.AreEqual(1, reservoir.Snapshot.Values.Length);
            Assert.AreEqual(50000000, reservoir.Snapshot.Values[0]);
        }



        [Test]
        public void ignoresNegativeValues()
        {

            timer.Update(-1, TimeUnit.Seconds);

            Assert.AreEqual(0, timer.Count);

            Assert.AreEqual(0, reservoir.Snapshot.Values.Length);

        }
        [Test]
        public void UsingContextWorks()
        {
            Assert.AreEqual(0, timer.Count);

            int dummy = 0;

            using (Timer.Context context = timer.Time())
            {
                dummy += 1;
            }

            Assert.AreEqual(1, timer.Count);
            Assert.AreEqual(1, reservoir.Snapshot.Values.Length);
            Assert.AreEqual(50000000, reservoir.Snapshot.Values[0]);
        }
    }

    internal class TimerTestClock : Clock
    {
        private long val = 0;

        public override long getTick()
        {
            return val += 50000000;
        }
    }
}
