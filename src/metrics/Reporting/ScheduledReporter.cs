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
    public abstract class ScheduledReporter : IReporter, IDisposable
    {
        private static ILog LOG = LogManager.GetLogger(typeof(ScheduledReporter));



        private static readonly AtomicLong FACTORY_ID = new AtomicLong();

        private readonly MetricRegistry registry;
        //private readonly ScheduledExecutorService executor;
        private readonly MetricFilter filter;
        private readonly double durationFactor;
        private readonly string durationUnit;
        private readonly double rateFactor;
        private readonly string rateUnit;
        private System.Timers.Timer threadTimer;

        /**
         * Creates a new {@link ScheduledReporter} instance.
         *
         * @param registry the {@link io.dropwizard.metrics.MetricRegistry} containing the metrics this
         *                 reporter will report
         * @param name     the reporter's name
         * @param filter   the filter for which metrics to report
         * @param rateUnit a unit of time
         * @param durationUnit a unit of time
         */
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

        /**
         * Starts the reporter polling at the given period.
         *
         * @param period the amount of time between polls
         * @param unit   the unit for {@code period}
         */
        public void start(long period, TimeUnit unit)
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

        /**
         * Stops the reporter and shuts down its thread of execution.
         *
         */
        public void stop()
        {
            threadTimer.Stop();
        }

        /**
        * Report the current values of all metrics in the registry.
        */
        public void report()
        {
            report(registry.getGauges(filter),
                    registry.getCounters(filter),
                    registry.getHistograms(filter),
                    registry.getMeters(filter),
                    registry.getTimers(filter));
        }

        /**
        * Called periodically by the polling thread. Subclasses should report all the given metrics.
        *
        * @param gauges     all of the gauges in the registry
        * @param counters   all of the counters in the registry
        * @param histograms all of the histograms in the registry
        * @param meters     all of the meters in the registry
        * @param timers     all of the timers in the registry
        */
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

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScheduledReporter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
