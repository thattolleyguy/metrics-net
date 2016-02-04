using Metrics.Core;
using Ninject.Extensions.Interception.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Ninject
{
    public static class Utils
    {
        public static MetricName BuildName(IProxyRequest request, string Name, bool Absolute)
        {
            MetricName metricName = null;
            if (Absolute)
                metricName = new MetricName(Name);
            else
                metricName = new MetricName(request.Target.GetType().FullName + "." + Name);
            return metricName;
        }
    }
}
