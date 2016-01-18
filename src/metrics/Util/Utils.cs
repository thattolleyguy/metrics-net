using System.Collections.Generic;
using Metrics.Core;
using System.Linq;

namespace Metrics.Util
{
    internal static class Utils
    {
        internal static IDictionary<string, IMetric> SortMetrics(IDictionary<MetricName, IMetric> metrics)
        {
            var sortedMetrics = new SortedDictionary<string, IMetric>(metrics.ToDictionary(x=> x.Key.Key,x=>x.Value));
            return sortedMetrics;
        }
    }
}
