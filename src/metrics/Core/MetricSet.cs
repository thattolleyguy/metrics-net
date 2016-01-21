using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    public interface MetricSet : IMetric
    {
        IDictionary<MetricName, IMetric> Metrics { get; }
    }
}
