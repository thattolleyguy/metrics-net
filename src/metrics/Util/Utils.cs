using System.Collections.Generic;
using metrics.Core;
using System.Linq;

namespace metrics.Util
{
    internal static class Utils
    {
        internal static IDictionary<string, IMetric> SortMetrics(IDictionary<MetricName, IMetric> metrics)
        {
            var sortedMetrics = new SortedDictionary<string, IMetric>(metrics.ToDictionary(x=> x.Key.Name,x=>x.Value));
            return sortedMetrics;
        }
    }
}
