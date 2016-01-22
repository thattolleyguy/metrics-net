using System.Linq;
using NUnit.Framework;
using Metrics.Core;
using Metrics.Stats;

namespace Metrics.Tests.Core
{
    [TestFixture]
    public class HistogramTests
    {
        [Test]
        public void Max_WhenPassed8And9_Returns9()
        {
            var underTest = new Histogram(new ExponentiallyDecayingReservoir());
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(9, underTest.Snapshot.Max);
        }

        [Test]
        public void Min_WhenPassed8And9_Returns9()
        {
            var underTest = new Histogram(new ExponentiallyDecayingReservoir());
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8, underTest.Snapshot.Min);
        }

        [Test]
        public void Count_WhenPassedTwoValues_Returns2()
        {
            var underTest = new Histogram(new ExponentiallyDecayingReservoir());
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(2, underTest.Count);
        }

        [Test]
        public void Mean_WhenPassed8And9_Returns8Point5()
        {
            var underTest = new Histogram(new ExponentiallyDecayingReservoir());
            underTest.Update(9);
            underTest.Update(8);
            Assert.AreEqual(8.5, underTest.Snapshot.Mean);
        }


        [Test]
        public void ValidateMeanAndMedianDifferent()
        {
            var underTest = new Histogram(new ExponentiallyDecayingReservoir());
            underTest.Update(7);
            underTest.Update(8);
            underTest.Update(12);

            Assert.AreEqual(8, underTest.Snapshot.Median);
            Assert.AreNotEqual(8, underTest.Snapshot.Mean);
            
        }

    }
}
