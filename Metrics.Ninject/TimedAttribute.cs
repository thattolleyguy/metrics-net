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
    public class TimedAttribute : InterceptAttribute
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public bool Absolute { get; set; }

        private Timer timer = null;

        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            if (timer == null)
            {
                MetricName metricName = null;
                if (Absolute)
                    metricName = new MetricName(Name);
                else
                    metricName = new MetricName(request.Target.GetType().FullName + "." + Name);

                MetricRegistry registry = request.Context.Kernel.Get<MetricRegistry>();
                timer = registry.Timer(metricName);
            }
            return new TimingInterceptor(timer);
        }
    }

    public class TimingInterceptor : IInterceptor
    {
        private readonly Timer timer;

        public TimingInterceptor(Timer timer)
        {
            this.timer = timer;
        }

        public void Intercept(IInvocation invocation)
        {
            timer.Time(() => invocation.Proceed());
        }
    }
}
