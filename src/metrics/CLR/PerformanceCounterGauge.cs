using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.CLR
{
    public class PerformanceCounterGauge : Gauge<double>
    {

        public static PerformanceCounterGauge create(string category, string counter, string instance)
        {
            var performanceCounter = new PerformanceCounter(category, counter, instance, true);
            return new PerformanceCounterGauge(performanceCounter);

        }

        public static PerformanceCounterGauge create(string category, string counter)
        {
            var performanceCounter = new PerformanceCounter(category, counter, true);
            return new PerformanceCounterGauge(performanceCounter);

        }
        protected PerformanceCounterGauge(PerformanceCounter counter) : base(() => counter.NextValue())
        {
        }
    }
}
