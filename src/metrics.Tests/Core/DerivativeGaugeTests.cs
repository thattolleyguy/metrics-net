using Metrics.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metrics.Tests.Core
{
    class DerivativeGaugeTests
    {
        [Test]
        public void Can_derive()
        {
            int sum = 0;
            Gauge<int> baseGauge = new Gauge<int>(() => 1);
            DerivativeGauge<int, int> derivativeGauge = new DerivativeGauge<int, int>(baseGauge,
                (x) =>
                { sum += x; return sum; }
                );
            Assert.AreEqual(1, derivativeGauge.Value);
            Assert.AreEqual(2, derivativeGauge.Value);
            Assert.AreEqual(3, derivativeGauge.Value);
            Assert.AreEqual(4, derivativeGauge.Value);
            Assert.AreEqual(4, sum);

        }
    }
}
