using System;
using NUnit.Framework;
using metrics.Core;

namespace metrics.Tests.Core
{
    [TestFixture]
    public class SampleExtensionsTests
    {
        [Test]
        public void NewSample_ForEachSampleType_DoesNotThrow()
        {
            foreach(var sampleType in (Histogram.SampleType[])Enum.GetValues(typeof(Histogram.SampleType)))
                sampleType.NewSample();
        }
    }
}
