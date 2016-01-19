using Metrics.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Metrics.Reporting
{

    public class ConsoleReporter : ScheduledReporter
    {


        public static Builder ForRegistry(MetricRegistry registry)
        {
            return new Builder(registry);
        }
        private static readonly int CONSOLE_WIDTH = 80;

        private readonly TextWriter output;
        private readonly Clock clock;
        //private readonly DateFormat dateFormat;

        private ConsoleReporter(MetricRegistry registry,
                                TextWriter output,
                                Clock clock,
                                TimeZone timeZone,
                                TimeUnit rateUnit,
                                TimeUnit durationUnit,
                                MetricFilter filter) :
            base(registry, "console-reporter", filter, rateUnit, durationUnit)
        {
            this.output = output;
            this.clock = clock;
            /* this.dateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT,
                                                              DateFormat.MEDIUM,
                                                              locale);
             dateFormat.setTimeZone(timeZone);*/
        }

        public override void report(IDictionary<MetricName, Gauge> gauges,
                             IDictionary<MetricName, Counter> counters,
                             IDictionary<MetricName, Histogram> histograms,
                             IDictionary<MetricName, Meter> meters,
                             IDictionary<MetricName, Timer> timers)
        {
            printWithBanner(DateTime.Now.ToString(), '=');
            output.WriteLine();

            if (gauges.Count > 0)
            {
                printWithBanner("-- Gauges", '-');
                foreach (KeyValuePair<MetricName, Gauge> entry in gauges)
                {
                    output.WriteLine(entry.Key);
                    printGauge(entry);
                }
                output.WriteLine();
            }

            if (counters.Count > 0)
            {
                printWithBanner("-- Counters", '-');
                foreach (KeyValuePair<MetricName, Counter> entry in counters)
                {
                    output.WriteLine(entry.Key);
                    printCounter(entry);
                }
                output.WriteLine();
            }

            if (histograms.Count > 0)
            {
                printWithBanner("-- Histograms", '-');
                foreach (KeyValuePair<MetricName, Histogram> entry in histograms)
                {
                    output.WriteLine(entry.Key);
                    printHistogram(entry.Value);
                }
                output.WriteLine();
            }

            if (meters.Count > 0)
            {
                printWithBanner("-- Meters", '-');
                foreach (KeyValuePair<MetricName, Meter> entry in meters)
                {
                    output.WriteLine(entry.Key);
                    printMeter(entry.Value);
                }
                output.WriteLine();
            }

            if (timers.Count > 0)
            {
                printWithBanner("-- Timers", '-');
                foreach (KeyValuePair<MetricName, Timer> entry in timers)
                {
                    output.WriteLine(entry.Key);
                    printTimer(entry.Value);
                }
                output.WriteLine();
            }

            output.WriteLine();
            output.Flush();
        }

        private void printMeter(Meter meter)
        {
            output.WriteLine(string.Format("             count = {0}", meter.Count));
            output.WriteLine(string.Format("         mean rate = {0} events/{1}", convertRate(meter.MeanRate), getRateUnit()));
            output.WriteLine(string.Format("     1-minute rate = {0} events/{1}", convertRate(meter.OneMinuteRate), getRateUnit()));
            output.WriteLine(string.Format("     5-minute rate = {0} events/{1}", convertRate(meter.FiveMinuteRate), getRateUnit()));
            output.WriteLine(string.Format("    15-minute rate = {0} events/{1}", convertRate(meter.FifteenMinuteRate), getRateUnit()));
        }

        private void printCounter(KeyValuePair<MetricName, Counter> entry)
        {
            output.WriteLine(string.Format("             count = {0}", entry.Value.Count));
        }

        private void printGauge(KeyValuePair<MetricName, Gauge> entry)
        {
            output.WriteLine(string.Format("             value = {0}", entry.Value.ValueAsString));
        }

        private void printHistogram(Histogram histogram)
        {
            output.WriteLine(string.Format("             count = {0}", histogram.Count));
            Snapshot snapshot = histogram.Snapshot;
            output.WriteLine(string.Format("               min = {0}", snapshot.Min));
            output.WriteLine(string.Format("               max = {0}", snapshot.Max));
            output.WriteLine(string.Format("              mean = {0}", snapshot.Mean));
            output.WriteLine(string.Format("            stddev = {0}", snapshot.StdDev));
            output.WriteLine(string.Format("            median = {0}", snapshot.Median));
            output.WriteLine(string.Format("              75%% <= {0}", snapshot.Percentile75th));
            output.WriteLine(string.Format("              95%% <= {0}", snapshot.Percentile95th));
            output.WriteLine(string.Format("              98%% <= {0}", snapshot.Percentile98th));
            output.WriteLine(string.Format("              99%% <= {0}", snapshot.Percentile99th));
            output.WriteLine(string.Format("            99.9%% <= {0}", snapshot.Percentile999th));
        }

        private void printTimer(Timer timer)
        {
            Snapshot snapshot = timer.Snapshot;
            output.WriteLine(string.Format("             count = {0}", timer.Count));
            output.WriteLine(string.Format("         mean rate = {0} calls/{1}", convertRate(timer.MeanRate), getRateUnit()));
            output.WriteLine(string.Format("     1-minute rate = {0} calls/{1}", convertRate(timer.OneMinuteRate), getRateUnit()));
            output.WriteLine(string.Format("     5-minute rate = {0} calls/{1}", convertRate(timer.FiveMinuteRate), getRateUnit()));
            output.WriteLine(string.Format("    15-minute rate = {0} calls/{1}", convertRate(timer.FifteenMinuteRate), getRateUnit()));

            output.WriteLine(string.Format("               min = {0} {1}", convertDuration(snapshot.Min), getDurationUnit()));
            output.WriteLine(string.Format("               max = {0} {1}", convertDuration(snapshot.Max), getDurationUnit()));
            output.WriteLine(string.Format("              mean = {0} {1}", convertDuration(snapshot.Mean), getDurationUnit()));
            output.WriteLine(string.Format("            stddev = {0} {1}", convertDuration(snapshot.StdDev), getDurationUnit()));
            output.WriteLine(string.Format("            median = {0} {1}", convertDuration(snapshot.Median), getDurationUnit()));
            output.WriteLine(string.Format("              75%% <= {0} {1}", convertDuration(snapshot.Percentile75th), getDurationUnit()));
            output.WriteLine(string.Format("              95%% <= {0} {1}", convertDuration(snapshot.Percentile95th), getDurationUnit()));
            output.WriteLine(string.Format("              98%% <= {0} {1}", convertDuration(snapshot.Percentile98th), getDurationUnit()));
            output.WriteLine(string.Format("              99%% <= {0} {1}", convertDuration(snapshot.Percentile99th), getDurationUnit()));
            output.WriteLine(string.Format("            99.9%% <= {0} {1}", convertDuration(snapshot.Percentile999th), getDurationUnit()));
        }

        private void printWithBanner(String s, char c)
        {
            output.Write(s);
            output.Write(' ');
            for (int i = 0; i < (CONSOLE_WIDTH - s.Length - 1); i++)
            {
                output.Write(c);
            }
            output.WriteLine();
        }

        public class Builder
        {
            private readonly MetricRegistry registry;
            private TextWriter output;
            private Clock clock;
            private TimeZone timeZone;
            private TimeUnit rateUnit;
            private TimeUnit durationUnit;
            private MetricFilter filter;

            internal Builder(MetricRegistry registry)
            {
                this.registry = registry;
                this.output = Console.Out;
                this.clock = Clock.DEFAULT;
                this.rateUnit = TimeUnit.Seconds;
                this.durationUnit = TimeUnit.Milliseconds;
                this.filter = MetricRegistry.ALL;
            }

            /**
             * Write to the given {@link PrintStream}.
             *
             * @param output a {@link PrintStream} instance.
             * @return {@code this}
             */
            public Builder outputTo(TextWriter output)
            {
                this.output = output;
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
             * Builds a {@link ConsoleReporter} with the given properties.
             *
             * @return a {@link ConsoleReporter}
             */
            public ConsoleReporter build()
            {
                return new ConsoleReporter(registry,
                                           output,
                                           clock,
                                           timeZone,
                                           rateUnit,
                                           durationUnit,
                                           filter);
            }
        }
    }
}
