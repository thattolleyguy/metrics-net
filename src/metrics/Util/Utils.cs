using System.Collections.Generic;
using Metrics.Core;
using System.Linq;
using System;

namespace Metrics.Util
{
    public static class Utils
    {
        internal static IDictionary<string, IMetric> SortMetrics(IDictionary<MetricName, IMetric> metrics)
        {
            var sortedMetrics = new SortedDictionary<string, IMetric>(metrics.ToDictionary(x=> x.Key.Key,x=>x.Value));
            return sortedMetrics;
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
    }
}
