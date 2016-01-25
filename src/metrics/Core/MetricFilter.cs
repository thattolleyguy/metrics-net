using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// A filter used to determine whether or not a metric should be reported, among other things.
    /// </summary>
    /// <param name="name">the metric's name</param>
    /// <param name="metric">the metric</param>
    /// <returns><c>true</c> if the metric matches the filter</returns>
    public delegate bool MetricFilter(MetricName name, IMetric metric);
    public class MetricFilters
    {
        /// <summary>
        /// Matches all metrics, regardless of type or name
        /// </summary>
        public readonly static MetricFilter ALL = (name, metric) => true;

    }
}
