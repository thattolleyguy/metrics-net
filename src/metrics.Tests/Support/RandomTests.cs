using System.Diagnostics;
using NUnit.Framework;

namespace Metrics.Tests.Support
{
    [TestFixture]
    public class RandomTests
    {
        [Test]
        public void Can_generate_random_longs()
        {
            for(var i = 0; i < 1000; i++)
            {
                long random = Metrics.Support.Random.NextLong();
                Trace.WriteLine(random);
            }
        }
    }
}
