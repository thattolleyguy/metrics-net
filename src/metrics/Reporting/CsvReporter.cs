using log4net;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Reporting
{
    public class CsvReporter : ScheduledReporter
    {
        /**
    * Returns a new {@link Builder} for {@link CsvReporter}.
    *
    * @param registry the registry to report
    * @return a {@link Builder} instance for a {@link CsvReporter}
    */
        public static Builder forRegistry(MetricRegistry registry)
        {
            return new Builder(registry);
        }



        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(CsvReporter));

        private readonly string directory;
        private readonly Clock clock;

        public CsvReporter(MetricRegistry registry,
                            string directory,
                            TimeUnit rateUnit,
                            TimeUnit durationUnit,
                            Clock clock,
                            MetricFilter filter) : base(registry, "csv-reporter", filter, rateUnit, durationUnit)
        {

            this.directory = directory;
            this.clock = clock;
        }

        public override void report(IDictionary<MetricName, Gauge> gauges,
                              IDictionary<MetricName, Counter> counters,
                              IDictionary<MetricName, Histogram> histograms,
                              IDictionary<MetricName, Meter> meters,
                              IDictionary<MetricName, Timer> timers)
        {
            long timestamp = TimeUnit.Milliseconds.ToSeconds(clock.CurrentTime);

            foreach (KeyValuePair<MetricName, Gauge> entry in gauges)
            {
                reportGauge(timestamp, entry.Key, entry.Value);
            }
            foreach (KeyValuePair<MetricName, Counter> entry in counters)
            {
                reportCounter(timestamp, entry.Key, entry.Value);
            }

            foreach (KeyValuePair<MetricName, Histogram> entry in histograms)
            {
                reportHistogram(timestamp, entry.Key, entry.Value);
            }

            foreach (KeyValuePair<MetricName, Meter> entry in meters)
            {
                reportMeter(timestamp, entry.Key, entry.Value);
            }

            foreach (KeyValuePair<MetricName, Timer> entry in timers)
            {
                reportTimer(timestamp, entry.Key, entry.Value);
            }
        }

        private void reportTimer(long timestamp, MetricName name, Timer timer)
        {
            Snapshot snapshot = timer.Snapshot;

            report(timestamp,
                   name,
                   "count,max,mean,min,stddev,p50,p75,p95,p98,p99,p999,mean_rate,m1_rate,m5_rate,m15_rate,rate_unit,duration_unit",
                   timer.Count,
                   convertDuration(snapshot.Max),
                   convertDuration(snapshot.Mean),
                   convertDuration(snapshot.Min),
                   convertDuration(snapshot.StdDev),
                   convertDuration(snapshot.Median),
                   convertDuration(snapshot.Percentile75th),
                   convertDuration(snapshot.Percentile95th),
                   convertDuration(snapshot.Percentile98th),
                   convertDuration(snapshot.Percentile99th),
                   convertDuration(snapshot.Percentile999th),
                   convertRate(timer.MeanRate),
                   convertRate(timer.OneMinuteRate),
                   convertRate(timer.FiveMinuteRate),
                   convertRate(timer.FifteenMinuteRate),
                   "calls/" + getRateUnit(),
                   getDurationUnit());
        }

        private void reportMeter(long timestamp, MetricName name, Meter meter)
        {
            report(timestamp,
                   name,
                   "count,mean_rate,m1_rate,m5_rate,m15_rate,rate_unit",
                   meter.Count,
                   convertRate(meter.MeanRate),
                   convertRate(meter.OneMinuteRate),
                   convertRate(meter.FiveMinuteRate),
                   convertRate(meter.FifteenMinuteRate),
                   "events/" + getRateUnit());
        }

        private void reportHistogram(long timestamp, MetricName name, Histogram histogram)
        {
            Snapshot snapshot = histogram.Snapshot;

            report(timestamp,
                   name,
                   "count,max,mean,min,stddev,p50,p75,p95,p98,p99,p999",
                   histogram.Count,
                   snapshot.Max,
                   snapshot.Mean,
                   snapshot.Min,
                   snapshot.StdDev,
                   snapshot.Median,
                   snapshot.Percentile75th,
                   snapshot.Percentile95th,
                   snapshot.Percentile98th,
                   snapshot.Percentile99th,
                   snapshot.Percentile999th);
        }

        private void reportCounter(long timestamp, MetricName name, Counter counter)
        {
            report(timestamp, name, "count", counter.Count);
        }

        private void reportGauge(long timestamp, MetricName name, Gauge gauge)
        {
            report(timestamp, name, "value", gauge.ValueAsString);
        }
        private void report(long timestamp, MetricName name, string header, params Object[] valuesArr)
        {
            header = "t," + header;
            List<object> values = new List<object>();
            values.Add(timestamp);
            foreach (object obj in valuesArr)
                values.Add(obj);
            report(name, header, values);

        }

        private void report(MetricName name, String header, IEnumerable<object> values)
        {
            try
            {
                string filePath = System.IO.Path.Combine(directory, sanitize(name.Key));
                bool fileAlreadyExists = File.Exists(filePath);
                using (StreamWriter stream = File.AppendText(filePath))
                {

                    if (!fileAlreadyExists)
                    {
                        stream.WriteLine(header);
                    }

                    stream.WriteLine(string.Join(",", values));
                }
            }
            catch (IOException e)
            {
                LOGGER.Warn("Error writing to " + name, e);
            }
        }

        protected String sanitize(String name)
        {
            return name;
        }

        /**
    * A builder for {@link CsvReporter} instances. Defaults to using the default locale, converting
    * rates to events/second, converting durations to milliseconds, and not filtering metrics.
    */
        public class Builder
        {
            private readonly MetricRegistry registry;
            private TimeUnit rateUnit;
            private TimeUnit durationUnit;
            private Clock clock;
            private MetricFilter filter;
            

            public Builder(MetricRegistry registry)
            {
                this.registry = registry;
                this.rateUnit = TimeUnit.Seconds;
                this.durationUnit = TimeUnit.Milliseconds;
                this.clock = Clock.DefaultClock;
                this.filter = MetricFilters.ALL;
            }

            /**
             * Convert rates to the given time unit.
             *
             * @param rateUnit a unit of time
             * @return {@code this}
             */
            public Builder convertRatesTo(TimeUnit rateUnit)
            {
                this.rateUnit = rateUnit;
                return this;
            }

            /**
             * Convert durations to the given time unit.
             *
             * @param durationUnit a unit of time
             * @return {@code this}
             */
            public Builder convertDurationsTo(TimeUnit durationUnit)
            {
                this.durationUnit = durationUnit;
                return this;
            }

            /**
             * Use the given {@link Clock} instance for the time.
             *
             * @param clock a {@link Clock} instance
             * @return {@code this}
             */
            public Builder withClock(Clock clock)
            {
                this.clock = clock;
                return this;
            }

            /**
             * Only report metrics which match the given filter.
             *
             * @param filter a {@link MetricFilter}
             * @return {@code this}
             */
            public Builder withFilter(MetricFilter filter)
            {
                this.filter = filter;
                return this;
            }

            /**
             * Builds a {@link CsvReporter} with the given properties, writing {@code .csv} files to the
             * given directory.
             *
             * @param directory the directory in which the {@code .csv} files will be created
             * @return a {@link CsvReporter}
             */
            public CsvReporter build(string directory)
            {
                return new CsvReporter(registry,
                                       directory,
                                       rateUnit,
                                       durationUnit,
                                       clock,
                                       filter);
            }
        }
    }
}
