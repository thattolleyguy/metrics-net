using System;
using System.Text;
using Metrics.Core;
using Metrics.Util;

namespace Metrics.Reporting
{
    public class HumanReadableReportFormatter : IReportFormatter
    {
        private readonly MetricRegistry _metrics;

        public HumanReadableReportFormatter(MetricRegistry metrics)
        {
            _metrics = metrics;
        }

        public string GetSample()
        {
            var sb = new StringBuilder();
            var now = DateTime.UtcNow;
            var dateTime = string.Format("{0} {1}", now.ToShortDateString(), now.ToShortTimeString());
            sb.Append(dateTime);
            sb.Append(' ');
            for (var i = 0; i < (80 - dateTime.Length - 1); i++)
            {
                sb.Append('=');
            }
            sb.AppendLine();

            // TODO: Allow user to pass in ordered list of string tags to build metric heirarchy
            foreach (var entry in Utils.SortMetrics(_metrics.Metrics))
            {
                sb.Append(entry.Key);
                sb.AppendLine(":");


                var metric = entry.Value;
                if (metric is Gauge)
                {
                    WriteGauge(sb, (Gauge)metric);
                }
                else if (metric is Counter)
                {
                    WriteCounter(sb, (Counter)metric);
                }
                else if (metric is Histogram)
                {
                    WriteHistogram(sb, (Histogram)metric);
                }
                else if (metric is Meter)
                {
                    WriteMetered(sb, (Meter)metric);
                }
                else if (metric is Timer)
                {
                    WriteTimer(sb, (Timer)metric);
                }
                sb.AppendLine();

            }

            return sb.ToString();
        }

        protected void WriteGauge(StringBuilder sb, Gauge gauge)
        {
            sb.Append("    value = ");
            sb.AppendLine(gauge.ValueAsString);
        }

        protected void WriteCounter(StringBuilder sb, Counter counter)
        {
            sb.Append("    count = ");
            sb.AppendLine(counter.Count.ToString());
        }

        protected void WriteMetered(StringBuilder sb, IMetered meter)
        {
            var unit = "ms";
            sb.AppendFormat("             count = {0}\n", meter.Count);
            sb.AppendFormat("         mean rate = {0} requests/{1}\n", meter.MeanRate, unit);
            sb.AppendFormat("     1-minute rate = {0} requests/{1}\n", meter.OneMinuteRate, unit);
            sb.AppendFormat("     5-minute rate = {0} requests/{1}\n", meter.FiveMinuteRate, unit);
            sb.AppendFormat("    15-minute rate = {0} requests/{1}\n", meter.FifteenMinuteRate, unit);
        }

        protected void WriteHistogram(StringBuilder sb, Histogram histogram)
        {
            var snapshot = histogram.Snapshot;

            sb.AppendFormat("               min = {0:F2}\n", snapshot.Min);
            sb.AppendFormat("               max = {0:F2}\n", snapshot.Max);
            sb.AppendFormat("              mean = {0:F2}\n", snapshot.Mean);
            sb.AppendFormat("            stddev = {0:F2}\n", snapshot.StdDev);
            sb.AppendFormat("            median = {0:F2}\n", snapshot.Median);
            sb.AppendFormat("              75%% <= {0:F2}\n", snapshot.Percentile75th);
            sb.AppendFormat("              95%% <= {0:F2}\n", snapshot.Percentile95th);
            sb.AppendFormat("              98%% <= {0:F2}\n", snapshot.Percentile98th);
            sb.AppendFormat("              99%% <= {0:F2}\n", snapshot.Percentile99th);
            sb.AppendFormat("            99.9%% <= {0:F2}\n", snapshot.Percentile999th);
        }

        protected void WriteTimer(StringBuilder sb, Timer timer)
        {
            WriteMetered(sb, timer);

            var durationUnit = "ms";// Abbreviate(timer.DurationUnit);

            var snapshot = timer.Snapshot;

            sb.AppendFormat("               min = {0:F2}{1}\n", snapshot.Min, durationUnit);
            sb.AppendFormat("               max = {0:F2}{1}\n", snapshot.Max, durationUnit);
            sb.AppendFormat("              mean = {0:F2}{1}\n", snapshot.Mean, durationUnit);
            sb.AppendFormat("            stddev = {0:F2}{1}\n", snapshot.StdDev, durationUnit);
            sb.AppendFormat("            median = {0:F2}\n", snapshot.Median);
            sb.AppendFormat("              75%% <= {0:F2}\n", snapshot.Percentile75th);
            sb.AppendFormat("              95%% <= {0:F2}\n", snapshot.Percentile95th);
            sb.AppendFormat("              98%% <= {0:F2}\n", snapshot.Percentile98th);
            sb.AppendFormat("              99%% <= {0:F2}\n", snapshot.Percentile99th);
            sb.AppendFormat("            99.9%% <= {0:F2}\n", snapshot.Percentile999th);
        }

        protected static string Abbreviate(TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Nanoseconds:
                    return "ns";
                case TimeUnit.Microseconds:
                    return "us";
                case TimeUnit.Milliseconds:
                    return "ms";
                case TimeUnit.Seconds:
                    return "s";
                case TimeUnit.Minutes:
                    return "m";
                case TimeUnit.Hours:
                    return "h";
                case TimeUnit.Days:
                    return "d";
                default:
                    throw new ArgumentOutOfRangeException("unit");
            }
        }
    }
}
