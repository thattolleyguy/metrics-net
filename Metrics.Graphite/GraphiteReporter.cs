using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics.Core;
using log4net;
using Metrics.Util;

namespace Metrics.Reporting.Graphite
{
    public class GraphiteReporter : ScheduledReporter
    {
        /// <summary>
        /// Creates a new <see cref="Builder"/> for <see cref="ConsoleReporter"/>
        /// </summary>
        /// <param name="registry">the registry to report</param>
        /// <returns>a new <see cref="Builder"/> for <see cref="ConsoleReporter"/></returns>
        public static Builder ForRegistry(MetricRegistry registry)
        {
            return new Builder(registry);
        }

        /// <summary>
        /// A builder for <see cref="ConsoleReporter"/> instances. Defaults writing to <c>Console.out</c>, 
        /// converting rates to events/second, converting durations to milliseconds, and not filtering metrics
        /// </summary>
        public class Builder
        {
            private readonly MetricRegistry registry;
            private Clock clock;
            private string prefix;
            private TimeUnit rateUnit;
            private TimeUnit durationUnit;
            private MetricFilter filter;

            internal Builder(MetricRegistry registry)
            {
                this.registry = registry;
                this.clock = Clock.DefaultClock;
                this.prefix = null;
                this.rateUnit = TimeUnit.Seconds;
                this.durationUnit = TimeUnit.Milliseconds;
                this.filter = MetricFilters.ALL;
            }

            /// <summary>
            /// Use the given <see cref="Clock"/> instance for the time.
            /// </summary>
            /// <param name="clock">A <see cref="Clock"/> instance</param>
            /// <returns><c>this</c></returns>
            public Builder withClock(Clock clock)
            {
                this.clock = clock;
                return this;
            }

            /// <summary>
            /// Prefix all metric names with the given string.
            /// </summary>
            /// <param name="prefix">the prefix for all metric names</param>
            /// <returns><c>this</c></returns>

            public Builder prefixedWith(string prefix)
            {
                this.prefix = prefix;
                return this;
            }

            /// <summary>
            /// Convert rates to the given time unit
            /// </summary>
            /// <param name="rateUnit">a unit of time</param>
            /// <returns><c>this</c></returns>
            public Builder convertRatesTo(TimeUnit rateUnit)
            {
                this.rateUnit = rateUnit;
                return this;
            }

            /// <summary>
            /// Conver durations to the given time unit
            /// </summary>
            /// <param name="durationUnit">a unit of time</param>
            /// <returns><c>this</c></returns>
            public Builder convertDurationsTo(TimeUnit durationUnit)
            {
                this.durationUnit = durationUnit;
                return this;
            }
            /// <summary>
            /// Only report metrics which match the given <see cref="MetricFilter"/>
            /// </summary>
            /// <param name="filter">a <see cref="MetricFilter"/></param>
            /// <returns><c>this</c></returns>
            public Builder withFilter(MetricFilter filter)
            {
                this.filter = filter;
                return this;
            }

            /// <summary>
            /// Builds a <see cref="ConsoleReporter"/> with the given properties
            /// </summary>
            /// <returns>a <see cref="ConsoleReporter"/></returns>
            public GraphiteReporter Build(GraphiteSender sender)
            {
                return new GraphiteReporter(registry,
                                           sender,
                                           clock,
                                           prefix,
                                           rateUnit,
                                           durationUnit,
                                           filter);
            }
        }

        private readonly GraphiteSender graphite;
        private readonly Clock clock;
        private readonly MetricName prefix;
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(GraphiteReporter));

        private GraphiteReporter(MetricRegistry registry,
                         GraphiteSender graphite,
                         Clock clock,
                         String prefix,
                         TimeUnit rateUnit,
                         TimeUnit durationUnit,
                         MetricFilter filter) : base(registry, "graphite-reporter", filter, rateUnit, durationUnit)
        {

            this.graphite = graphite;
            this.clock = clock;
            this.prefix = MetricName.build(prefix);
        }



        public override void report(
            IDictionary<MetricName, Gauge> gauges,
            IDictionary<MetricName, Counter> counters,
            IDictionary<MetricName, Histogram> histograms,
            IDictionary<MetricName, Meter> meters,
            IDictionary<MetricName, Timer> timers)
        {
            
            long timestamp = DateTime.UtcNow.ToUnixTime();

            try
            {
                if (!graphite.IsConnected)
                {
                    graphite.Connect();
                }

                foreach (KeyValuePair<MetricName, Gauge> entry in gauges)
                {
                    reportGauge(entry.Key, entry.Value, timestamp);
                }

                foreach (KeyValuePair<MetricName, Counter> entry in counters)
                {
                    reportCounter(entry.Key, entry.Value, timestamp);
                }

                foreach (KeyValuePair<MetricName, Histogram> entry in histograms)
                {
                    reportHistogram(entry.Key, entry.Value, timestamp);
                }

                foreach (KeyValuePair<MetricName, Meter> entry in meters)
                {
                    reportMetered(entry.Key, entry.Value, timestamp);
                }

                foreach (KeyValuePair<MetricName, Timer> entry in timers)
                {
                    reportTimer(entry.Key, entry.Value, timestamp);
                }

                graphite.Flush();
            }
            catch (Exception t)
            {
                LOGGER.Warn("Unable to report to Graphite", t);
                Stop();
            }
        }

        private void closeGraphiteConnection()
        {
            try
            {
                graphite.Dispose();
            }
            catch (Exception e)
            {
                LOGGER.Debug("Error disconnecting from Graphite", e);
            }
        }

        public override void Stop()
        {
            try
            {
                base.Stop();
            }
            finally
            {
                closeGraphiteConnection();
            }
        }

        private void reportTimer(MetricName name, Timer timer, long timestamp)
        {
            Snapshot snapshot = timer.Snapshot;

            graphite.Send(Prefix(name, "max"), format(convertDuration(snapshot.Max)), timestamp);
            graphite.Send(Prefix(name, "mean"), format(convertDuration(snapshot.Mean)), timestamp);
            graphite.Send(Prefix(name, "min"), format(convertDuration(snapshot.Min)), timestamp);
            graphite.Send(Prefix(name, "stddev"),
                      format(convertDuration(snapshot.StdDev)),
                      timestamp);
            graphite.Send(Prefix(name, "p50"),
                      format(convertDuration(snapshot.Median)),
                      timestamp);
            graphite.Send(Prefix(name, "p75"),
                      format(convertDuration(snapshot.Percentile75th)),
                      timestamp);
            graphite.Send(Prefix(name, "p95"),
                      format(convertDuration(snapshot.Percentile95th)),
                      timestamp);
            graphite.Send(Prefix(name, "p98"),
                      format(convertDuration(snapshot.Percentile98th)),
                      timestamp);
            graphite.Send(Prefix(name, "p99"),
                      format(convertDuration(snapshot.Percentile99th)),
                      timestamp);
            graphite.Send(Prefix(name, "p999"),
                      format(convertDuration(snapshot.Percentile999th)),
                      timestamp);

            reportMetered(name, timer, timestamp);
        }

        private void reportMetered(MetricName name, IMetered meter, long timestamp)
        {
            graphite.Send(Prefix(name, "count"), format(meter.Count), timestamp);
            graphite.Send(Prefix(name, "m1_rate"),
                      format(convertRate(meter.OneMinuteRate)),
                      timestamp);
            graphite.Send(Prefix(name, "m5_rate"),
                      format(convertRate(meter.FiveMinuteRate)),
                      timestamp);
            graphite.Send(Prefix(name, "m15_rate"),
                      format(convertRate(meter.FifteenMinuteRate)),
                      timestamp);
            graphite.Send(Prefix(name, "mean_rate"),
                      format(convertRate(meter.MeanRate)),
                      timestamp);
        }

        private void reportHistogram(MetricName name, Histogram histogram, long timestamp)
        {
            Snapshot snapshot = histogram.Snapshot;
            graphite.Send(Prefix(name, "count"), format(histogram.Count), timestamp);
            graphite.Send(Prefix(name, "max"), format(snapshot.Max), timestamp);
            graphite.Send(Prefix(name, "mean"), format(snapshot.Mean), timestamp);
            graphite.Send(Prefix(name, "min"), format(snapshot.Min), timestamp);
            graphite.Send(Prefix(name, "stddev"), format(snapshot.StdDev), timestamp);
            graphite.Send(Prefix(name, "p50"), format(snapshot.Median), timestamp);
            graphite.Send(Prefix(name, "p75"), format(snapshot.Percentile75th), timestamp);
            graphite.Send(Prefix(name, "p95"), format(snapshot.Percentile95th), timestamp);
            graphite.Send(Prefix(name, "p98"), format(snapshot.Percentile98th), timestamp);
            graphite.Send(Prefix(name, "p99"), format(snapshot.Percentile99th), timestamp);
            graphite.Send(Prefix(name, "p999"), format(snapshot.Percentile999th), timestamp);
        }

        private void reportCounter(MetricName name, Counter counter, long timestamp)
        {
            graphite.Send(Prefix(name, "count"), format(counter.Count), timestamp);
        }

        private void reportGauge(MetricName name, Gauge gauge, long timestamp)
        {
            string value = format(gauge.ValueAsString);
            if (value != null)
            {
                graphite.Send(Prefix(name), value, timestamp);
            }
        }

        private string format(Object o)
        {

            if(o is string)
            {
                return (string)o;
            }
            else if (o is float || o is double)
            {
                return formatDouble((double)o);
            }
            else if (o is decimal)
            {
                return formatDecimal((decimal)o);
            }
            else if (o is Byte || o is short || o is int || o is long)
            {
                return formatLong((long)o);
            }
            return o.ToString();
        }

        private string Prefix(MetricName name, params String[] components)
        {
            return MetricName.join(MetricName.join(this.prefix, name), MetricName.build(components)).Key;
        }


        private string formatLong(long l)
        {
            return l.ToString("D");
        }

        private string formatDouble(double v)
        {
            return v.ToString("F2");
        }
        private string formatDecimal(decimal v)
        {
            return v.ToString("F2");
        }


    }


}
