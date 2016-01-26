using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// A set of named metrics.
    /// <para />
    /// Used with <see cref="MetricRegistry.RegisterAll(MetricName, MetricSet)"/>
    /// </summary>
    public interface MetricSet : IMetric
    {
        /// <summary>
        /// A dictionary of metric names to metrics
        /// </summary>
        IDictionary<MetricName, IMetric> Metrics { get; }
    }
}
