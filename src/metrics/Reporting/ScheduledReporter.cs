using log4net;
using Metrics.Core;
using Metrics.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Reporting
{
    /// <summary>
    /// The abstract base class for all scheduled reporters (i.e., reporters which process a registry's
    /// metrics periodically).
    /// <para />
    /// <see cref="ConsoleReporter"/>
    /// <see cref="CsvReporter"/>
    /// </summary>
    public abstract class ScheduledReporter : IReporter, IDisposable
    {
        private static ILog LOG = LogManager.GetLogger(typeof(ScheduledReporter));

        private static readonly AtomicLong FACTORY_ID = new AtomicLong();

        private readonly MetricRegistry registry;
        private readonly MetricFilter filter;
        private readonly double durationFactor;
        private readonly string durationUnit;
        private readonly double rateFactor;
        private readonly string rateUnit;
        private System.Timers.Timer threadTimer;

        /// <summary>
        /// Creates a new <see cref="ScheduledReporter"/> instance
        /// </summary>
        /// <param name="registry">the <see cref="MetricRegistry"/> containing the metrics this reporter will report</param>
        /// <param name="name">the reporter's name</param>
        /// <param name="filter">the filter for which metrics to report</param>
        /// <param name="rateUnit">a unit of time</param>
        /// <param name="durationUnit">a unit of time</param>
        protected ScheduledReporter(MetricRegistry registry,
                                    string name,
                                    MetricFilter filter,
                                    TimeUnit rateUnit,
                                    TimeUnit durationUnit)
        {
            this.registry = registry;
            this.filter = filter;
            this.rateFactor = rateUnit.ToSeconds(1);
            this.rateUnit = calculateRateUnit(rateUnit);
            this.durationFactor = 1.0 / durationUnit.ToNanos(1);
            this.durationUnit = durationUnit.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Creates a new <see cref="ScheduledReporter"/> instance
        /// </summary>
        /// <param name="period">the amount of time between reports</param>
        /// <param name="unit">the unit of <c>period</c></param>
        public void Start(long period, TimeUnit unit)
        {

            this.threadTimer = new System.Timers.Timer { AutoReset = false, Interval = unit.ToMillis(period) };
            this.threadTimer.Elapsed += delegate
            {
                try
                {
                    report();
                }
                catch (Exception ex)
                {
                    LOG.ErrorFormat("Exception was thrown from {0}. Exception was suppressed. Exception: {1}", typeof(ScheduledReporter), ex);
                }
                threadTimer.Start();
            };
            threadTimer.Start();


        }

        /// <summary>
        /// Stops the reporter and shuts down its thread of execution
        /// </summary>
        public void stop()
        {
            threadTimer.Stop();
        }
        /// <summary>
        /// Report the current values of all metrics in the registry.
        /// </summary>
        public void report()
        {
            report(registry.getGauges(filter),
                    registry.getCounters(filter),
                    registry.getHistograms(filter),
                    registry.getMeters(filter),
                    registry.getTimers(filter));
        }

        /// <summary>
        /// Called periodically by the polling thread. Subclasses should report all the given metrics
        /// </summary>
        /// <param name="gauges">all of the gauges in the registry</param>
        /// <param name="counters">all of the counters in the registry</param>
        /// <param name="histograms">all of the histograms in the registry</param>
        /// <param name="meters">all of the meters in the registry</param>
        /// <param name="timers">all of the timers in the registry</param>
        public abstract void report(IDictionary<MetricName, Gauge> gauges,
                             IDictionary<MetricName, Counter> counters,
                             IDictionary<MetricName, Histogram> histograms,
                             IDictionary<MetricName, Meter> meters,
                             IDictionary<MetricName, Timer> timers);


        protected string getRateUnit()
        {
            return rateUnit;
        }

        protected string getDurationUnit()
        {
            return durationUnit;
        }

        protected double convertDuration(double duration)
        {
            return duration * durationFactor;
        }

        protected double convertRate(double rate)
        {
            return rate * rateFactor;
        }

        private string calculateRateUnit(TimeUnit unit)
        {
            string s = unit.ToString().ToLowerInvariant();
            return s.Substring(0, s.Length - 1);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stop();
                }


                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
