using Ninject.Extensions.Interception.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Request;
using Metrics.Core;
using Ninject;

namespace Metrics.Ninject
{
    public class MeteredAttribute : InterceptAttribute
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public bool Absolute { get; set; }

        private Meter meter = null;

        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            if (meter == null)
            {
                MetricName metricName = null;
                if (Absolute)
                    metricName = new MetricName(Name);
                else
                    metricName = new MetricName(request.Target.GetType().FullName + "." + Name);

                MetricRegistry registry = request.Context.Kernel.Get<MetricRegistry>();
                meter = registry.Meter(metricName);
            }
            return new MeteringInterceptor(meter);
        }
    }

    public class MeteringInterceptor : IInterceptor
    {
        private readonly Meter meter;

        public MeteringInterceptor(Meter meter)
        {
            this.meter = meter;
        }

        public void Intercept(IInvocation invocation)
        {
            this.meter.Mark();
            invocation.Proceed();
        }
    }
}
