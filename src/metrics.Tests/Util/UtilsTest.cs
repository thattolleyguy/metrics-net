using Metrics.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Tests.Util
{
    class UtilsTest
    {
        [Test]
        public void UnixTimeCorrect()
        {
            Assert.AreEqual(1, new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc).ToUnixTime());
            Assert.AreEqual(60, new DateTime(1970, 1, 1, 0, 1, 0, DateTimeKind.Utc).ToUnixTime());
            Assert.AreEqual(3600, new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc).ToUnixTime());
            Assert.AreEqual(24 * 3600, new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc).ToUnixTime());
            Assert.AreEqual(31536000, new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUnixTime());
            Assert.AreEqual(1453919339, new DateTime(2016, 1, 27, 18, 28, 59, DateTimeKind.Utc).ToUnixTime());
        }

    }
}
